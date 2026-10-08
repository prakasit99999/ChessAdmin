using ChessAdmin.Components.Models;
using ChessAdmin.Services;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace ChessAdmin.Components.Pages.Base
{
    public partial class Navbar
    {
        private string displayUsername = "กำลังโหลด...";
        private bool isLoggingOut;
        private List<NotificationDto> notifications = new();
        private int unreadCount = 0;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;

                if (user.Identity?.IsAuthenticated == true)
                {
                    displayUsername = user.Identity.Name
                                      ?? user.FindFirst(ClaimTypes.Name)?.Value
                                      ?? user.FindFirst("username")?.Value
                                      ?? "Admin";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading user profile: " + ex.Message);
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    // ดึง JWT Token จาก LocalStorage
                    var token = await JSRuntime.InvokeAsync<string?>("localStorage.getItem", "token");
                    if (!string.IsNullOrEmpty(token))
                    {
                        // เริ่มการเชื่อมต่อ SignalR และลงทะเบียน Event Handlers
                        NotificationService.OnNotificationReceived += OnNotificationReceived;

                        //  แก้ไขจุดที่ 1: เปลี่ยนมาใช้ HandleUnreadCountChanged
                        NotificationService.OnUnreadCountChanged += HandleUnreadCountChanged;

                        await NotificationService.InitializeAsync(token);
                        await LoadNotificationsAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error initializing NotificationService: " + ex.Message);
                }
            }
        }

        private async Task LoadNotificationsAsync()
        {
            var response = await NotificationService.GetNotificationsAsync(10);
            if (response?.Data != null)
            {
                notifications = response.Data;
                unreadCount = response.UnreadCount;
                await InvokeAsync(StateHasChanged);
            }
        }

        private async void OnNotificationReceived(NotificationDto notification)
        {
            await InvokeAsync(async () =>
            {
                notifications.Insert(0, notification);
                if (notifications.Count > 10)
                {
                    notifications.RemoveAt(notifications.Count - 1);
                }
                await RefreshUnreadCountAsync();
            });
        }

        // Method Task สำหรับใช้คำสั่ง await ภายใน
        private async Task RefreshUnreadCountAsync()
        {
            var result = await NotificationService.GetUnreadCountAsync();
            if (result != null)
            {
                await InvokeAsync(() =>
                {
                    unreadCount = result.UnreadCount;
                    StateHasChanged();
                });
            }
        }

        //  เพิ่มจุดนี้: Handler ตัวกลางชนิด async void สำหรับผูกกับ Event Action เพื่อแก้ Error CS0407
        private async void HandleUnreadCountChanged()
        {
            await RefreshUnreadCountAsync();
        }

        private async Task HandleNotificationClick(NotificationDto notification)
        {
            if (!notification.IsRead)
            {
                await NotificationService.MarkAsReadAsync(notification.Id);
                notification.IsRead = true;
                if (unreadCount > 0) unreadCount--;
            }

            if (notification.Metadata != null && notification.Metadata.TryGetValue("link", out var linkObj))
            {
                var link = linkObj?.ToString();
                if (!string.IsNullOrEmpty(link))
                {
                    Navigation.NavigateTo(link);
                }
            }
        }

        private async Task HandleMarkAllAsRead()
        {
            await NotificationService.MarkAllAsReadAsync();
            foreach (var item in notifications)
            {
                item.IsRead = true;
            }
            unreadCount = 0;
            StateHasChanged();
        }

        private string GetNotificationIcon(string severity)
        {
            return (severity ?? "").ToLower() switch
            {
                "success" => "bi bi-check-circle-fill text-success",
                "warning" => "bi bi-exclamation-triangle-fill text-warning",
                "error" => "bi bi-x-circle-fill text-danger",
                _ => "bi bi-info-circle-fill text-info"
            };
        }

        private string GetTimeAgo(DateTime createdAt)
        {
            var timeSpan = DateTime.UtcNow - createdAt;
            return timeSpan.TotalSeconds < 60 ? "เมื่อสักครู่" :
                   timeSpan.TotalMinutes < 60 ? $"{(int)timeSpan.TotalMinutes} น." :
                   timeSpan.TotalHours < 24 ? $"{(int)timeSpan.TotalHours} ชม." :
                   createdAt.ToString("dd/MM");
        }

        private async Task HandleLogout()
        {
            if (isLoggingOut) return;
            isLoggingOut = true;

            try
            {
                var token = await JSRuntime.InvokeAsync<string?>("localStorage.getItem", "token");
                if (!string.IsNullOrWhiteSpace(token))
                {
                    using var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/logout");
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    await Http.SendAsync(request);
                }
            }
            catch (HttpRequestException) { }
            finally
            {
                await JSRuntime.InvokeVoidAsync("localStorage.removeItem", "token");
                await JSRuntime.InvokeVoidAsync("localStorage.removeItem", "username");
                await JSRuntime.InvokeVoidAsync("localStorage.removeItem", "role");
                await JSRuntime.InvokeVoidAsync("localStorage.removeItem", "email");

                if (AuthenticationStateProvider is AdminAuthStateProvider authProvider)
                {
                    authProvider.NotifyLogout();
                }

                Navigation.NavigateTo("/", forceLoad: true);
            }
        }

        public void Dispose()
        {
            NotificationService.OnNotificationReceived -= OnNotificationReceived;

            NotificationService.OnUnreadCountChanged -= HandleUnreadCountChanged;
        }
    }
}