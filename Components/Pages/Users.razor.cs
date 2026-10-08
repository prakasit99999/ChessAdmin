using ChessAdmin.Components.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ChessAdmin.Components.Pages
{
    public partial class Users : ComponentBase, IAsyncDisposable
    {
        [Inject] public HttpClient Http { get; set; } = default!;
        [Inject] public IJSRuntime JSRuntime { get; set; } = default!;
        private readonly List<UserItem> users = new();
        private CancellationTokenSource? searchCancellation;
        private bool showAddUserModal;
        private string search = string.Empty;
        private string? errorMessage;
        private bool isLoading;
        private int pageNumber = 1;
        private int pageSize = 30;
        private int totalCount;
        private int totalPages;
        private int Online;
        private int Offline;
        private int Pending;
        private int TotalPages => totalPages > 0
            ? totalPages
            : Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));
        private int ShowingFrom => totalCount == 0 ? 0 : ((pageNumber - 1) * pageSize) + 1;
        private int ShowingTo => Math.Min(pageNumber * pageSize, totalCount);
        private HubConnection? userHubConnection;
        private PeriodicTimer? usersPollingTimer;
        private CancellationTokenSource? usersPollingCancellation;
        private Task? usersPollingTask;


        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await ConnectUserHubAsync();
                await LoadUsersAsync(pageNumber);

                usersPollingCancellation = new CancellationTokenSource();
                usersPollingTask = PollUsersAsync(usersPollingCancellation.Token);

                await InvokeAsync(StateHasChanged);
            }
        }
        private async Task PollUsersAsync(
         CancellationToken cancellationToken)
        {
            usersPollingTimer = new PeriodicTimer(
                TimeSpan.FromSeconds(15));

            try
            {
                while (await usersPollingTimer.WaitForNextTickAsync(
                    cancellationToken))
                {
                    await LoadUsersAsync(
                        pageNumber,
                        cancellationToken);

                    await InvokeAsync(StateHasChanged);
                }
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
            }
        }

        private async Task SearchChanged()
        {
            searchCancellation?.Cancel();
            searchCancellation?.Dispose();
            searchCancellation = new CancellationTokenSource();

            try
            {
                await Task.Delay(300, searchCancellation.Token);
                await LoadUsersAsync(1, searchCancellation.Token);
            }
            catch (OperationCanceledException)
            {
            }
        }
    
        private async Task ConnectUserHubAsync(CancellationToken cancellationToken = default)
        {
            errorMessage = null;
            try
            {
                if (Http.BaseAddress is null)
                {
                    return;
                }
                var hubUrl = new Uri(
                    Http.BaseAddress,
                    "userhub");

                userHubConnection = new HubConnectionBuilder()
               .WithUrl(hubUrl, options =>
               {
                   options.AccessTokenProvider = async () =>
                       await JSRuntime.InvokeAsync<string?>(
                           "localStorage.getItem",
                           "token");
               })
               .WithAutomaticReconnect()
               .Build();

                userHubConnection.On<int>(
                         "Online",
                         async count =>
                         {
                             Online = count;
                             await InvokeAsync(StateHasChanged);
                         });

                userHubConnection.On<int>(
                    "Offline",
                    async count =>
                    {
                        Offline = count;
                        await InvokeAsync(StateHasChanged);
                    });

                userHubConnection.On<int>(
                    "Pending",
                    async count =>
                    {
                        Pending = count;
                        await InvokeAsync(StateHasChanged);
                    });

                await userHubConnection.StartAsync(cancellationToken);

                await userHubConnection.InvokeAsync(
                    "Connect",
                    cancellationToken);

            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                errorMessage = $"ไม่สามารถเชื่อมต่อกับ User Hub ได้: {ex.Message}";
            }
        }

        private async Task LoadUsersAsync(int requestedPage, CancellationToken cancellationToken = default)
        {
            pageNumber = requestedPage;
            isLoading = true;
            errorMessage = null;

            try
            {
                var query = $"api/User/paged?PageNumber={pageNumber}&PageSize={pageSize}&Search={Uri.EscapeDataString(search)}";
                using var request = new HttpRequestMessage(HttpMethod.Get, query);
                var token = await JSRuntime.InvokeAsync<string?>("localStorage.getItem", "token");

                if (!string.IsNullOrWhiteSpace(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                using var response = await Http.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<PagedUserResponse>(cancellationToken);
                users.Clear();
                users.AddRange(result?.Items ?? Enumerable.Empty<UserItem>());
                totalCount = result?.TotalCount ?? 0;
                totalPages = result?.TotalPages ?? 0;
                pageNumber = result?.PageNumber ?? pageNumber;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                users.Clear();
                totalCount = 0;
                errorMessage = $"ไม่สามารถโหลดข้อมูลผู้ใช้ได้: {ex.Message}";
            }
            finally
            {
                isLoading = false;
            }
        }

        private void OpenAddUserModal()
        {
            showAddUserModal = true;
        }

        private Task OnAddUserModalChanged(bool isVisible)
        {
            showAddUserModal = isVisible;
            return Task.CompletedTask;
        }

        private async Task HandleUserAdded()
        {
            showAddUserModal = false;

            await LoadUsersAsync(
                pageNumber,
                CancellationToken.None);

            await InvokeAsync(StateHasChanged);
        }

        private Task PreviousPage() => pageNumber > 1
            ? LoadUsersAsync(pageNumber - 1)
            : Task.CompletedTask;

        private Task NextPage() => pageNumber < TotalPages
            ? LoadUsersAsync(pageNumber + 1)
            : Task.CompletedTask;


        public async ValueTask DisposeAsync()
        {
            searchCancellation?.Cancel();
            searchCancellation?.Dispose();

            usersPollingCancellation?.Cancel();

            if (usersPollingTask is not null)
            {
                try
                {
                    await usersPollingTask;
                }
                catch (OperationCanceledException)
                {
                }
            }

            usersPollingTimer?.Dispose();
            usersPollingCancellation?.Dispose();

            if (userHubConnection is not null)
            {
                await userHubConnection.DisposeAsync();
            }
        }

    
    }
}
