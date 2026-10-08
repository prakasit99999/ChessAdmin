using ChessAdmin.Components.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ChessAdmin.Services
{
    public interface INotificationService
    {
        Task InitializeAsync(string token);
        Task<NotificationResponseDto?> GetNotificationsAsync(int limit = 50);
        Task<UnreadCountResponseDto?> GetUnreadCountAsync();
        Task MarkAsReadAsync(string notificationId);
        Task MarkAllAsReadAsync();
        event Action<NotificationDto>? OnNotificationReceived;
        event Action? OnUnreadCountChanged;
        IAsyncEnumerable<NotificationDto> GetNotificationsStream();
    }

    public class NotificationService : INotificationService, IAsyncDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly NavigationManager _navigationManager;
        private HubConnection? _hubConnection;
        private string? _token;
        private List<NotificationDto> _notifications = new();

        public event Action<NotificationDto>? OnNotificationReceived;
        public event Action? OnUnreadCountChanged;

        public NotificationService(HttpClient httpClient, NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _navigationManager = navigationManager;
        }

        public async Task InitializeAsync(string token)
        {
            _token = token;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // ตั้งค่า SignalR Connection
            try
            {
                var baseUri = _httpClient.BaseAddress?.ToString().TrimEnd('/') ?? "http://localhost:8080";
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl($"{baseUri}/notificationhub", options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult(_token);
                    })
                    .WithAutomaticReconnect(new[] { 
                        TimeSpan.Zero, 
                        TimeSpan.FromSeconds(2), 
                        TimeSpan.FromSeconds(5), 
                        TimeSpan.FromSeconds(10) 
                    })
                    .Build();

                // ดักฟัง Event เมื่อได้รับแจ้งเตือนแบบเรียลไทม์
                _hubConnection.On<NotificationDto>("ReceiveNotification", (notification) =>
                {
                    _notifications.Insert(0, notification);
                    OnNotificationReceived?.Invoke(notification);
                    OnUnreadCountChanged?.Invoke();
                });

                await _hubConnection.StartAsync();
                Console.WriteLine("SignalR Connection เชื่อมต่อสำเร็จ");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalR Connection Error: {ex.Message}");
            }
        }

        public async Task<NotificationResponseDto?> GetNotificationsAsync(int limit = 50)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<NotificationResponseDto>(
                    $"/api/notification?limit={limit}");

                if (response?.Data != null)
                {
                    _notifications = response.Data;
                }

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting notifications: {ex.Message}");
                return null;
            }
        }

        public async Task<UnreadCountResponseDto?> GetUnreadCountAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<UnreadCountResponseDto>(
                    "/api/notification/unread-count");
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting unread count: {ex.Message}");
                return null;
            }
        }

        public async Task MarkAsReadAsync(string notificationId)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync(
                    $"/api/notification/mark-read/{notificationId}", 
                    new { });

                if (response.IsSuccessStatusCode)
                {
                    var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
                    if (notification != null)
                    {
                        notification.IsRead = true;
                        OnUnreadCountChanged?.Invoke();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking notification as read: {ex.Message}");
            }
        }

        public async Task MarkAllAsReadAsync()
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync(
                    "/api/notification/mark-all-read",
                    new { });

                if (response.IsSuccessStatusCode)
                {
                    foreach (var notification in _notifications)
                    {
                        notification.IsRead = true;
                    }
                    OnUnreadCountChanged?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking all notifications as read: {ex.Message}");
            }
        }

        public async IAsyncEnumerable<NotificationDto> GetNotificationsStream()
        {
            var response = await GetNotificationsAsync();
            if (response?.Data != null)
            {
                foreach (var notification in response.Data)
                {
                    yield return notification;
                }
            }
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.DisposeAsync();
            }
        }
    }
}
