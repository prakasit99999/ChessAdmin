using ChessAdmin.Components.Models;
using ChessAdmin.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using System.Security.Claims;

namespace ChessAdmin.Components.Pages
{
    public partial class Games : IAsyncDisposable
    {
        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;
        [Inject]
        public HttpClient Http { get; set; } = default!;
        [Inject]
        public IJSRuntime JSRuntime { get; set; } = default!;
        [Inject]
        public AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

        private List<GameListDto> games = new List<GameListDto>();

        // 🔹 ตัวแปรสำหรับการแบ่งหน้าตาราง (Pagination)
        private int PageNumber = 1;
        private int PageSize = 10;
        private int TotalCount = 0;
        private int TotalPages => TotalCount > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 1;
        private int ShowingFrom => TotalCount == 0 ? 0 : ((PageNumber - 1) * PageSize) + 1;
        private int ShowingTo => Math.Min(PageNumber * PageSize, TotalCount);

        // 🔹 ตัวแปรสำหรับแสดงการ์ดสถิติรวมของระบบ (Dashboard Stats)
        private int TotalGames = 0;
        private int ActiveMatches = 0;
        private int AIMatches = 0;
        private int RankedGames = 0;

        private HubConnection? gameHubConnection;
        private string searchTerm = string.Empty;
        private string GameType = string.Empty;
        private string GameStatus = string.Empty;
        private string? errorMessage;
        private bool isLoading = true;
        private CancellationTokenSource? searchCancellation;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await ReceiveGameHubCountsAsync();
                await LoadGamesAsync(PageNumber, PageSize, CancellationToken.None);
                await InvokeAsync(StateHasChanged);
            }
        }

        private void NavigateToGameDetails(int gameId)
        {
            NavigationManager.NavigateTo($"/games/{gameId}");
        }

        private async Task SearchChanged()
        {
            searchCancellation?.Cancel();
            searchCancellation?.Dispose();
            searchCancellation = new CancellationTokenSource();

            try
            {
                // Debounce 300ms
                await Task.Delay(300, searchCancellation.Token);
                await LoadGamesAsync(1, PageSize, searchCancellation.Token);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task ReceiveGameHubCountsAsync(CancellationToken cancellationToken = default)
        {
            errorMessage = null;
            try
            {
                var token = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "token");
                if (string.IsNullOrEmpty(token))
                {
                    errorMessage = "ผู้ใช้ยังไม่ได้เข้าสู่ระบบ";
                    return;
                }

                var hubUrl = new Uri(Http.BaseAddress!, "gamehub");
                gameHubConnection = new HubConnectionBuilder()
                    .WithUrl(hubUrl, options =>
                    {
                        options.AccessTokenProvider = async () => token;
                    })
                    .WithAutomaticReconnect()
                    .Build();

                gameHubConnection.On<GameCountsDto>("ReceiveGameCounts", (GameCountsDto gameCounts) =>
                {
                    // 🟢 แก้ไข: อัปเดตลง TotalGames เพื่อไม่ให้กระทบ TotalCount ของระบบ Pagination
                    TotalGames = gameCounts.TotalCount;
                    ActiveMatches = gameCounts.ActiveMatches;
                    AIMatches = gameCounts.AiMatches;
                    RankedGames = gameCounts.RankedGames;

                    InvokeAsync(StateHasChanged);
                });

                await gameHubConnection.StartAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                errorMessage = $"ไม่สามารถเชื่อมต่อกับ Game Hub ได้: {ex.Message}";
            }
        }

        private async Task LoadGamesAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            isLoading = true;
            try
            {
                var token = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "token");
                if (string.IsNullOrEmpty(token))
                {
                    errorMessage = "ผู้ใช้ยังไม่ได้เข้าสู่ระบบ";
                    return;
                }

                Http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var querParams = new Dictionary<string, string>
                {
                    { "pageNumber", PageNumber.ToString() },
                    { "pageSize", PageSize.ToString() },
                    { "searchTerm", searchTerm },
                    { "gameType", GameType },
                    { "gameStatus", GameStatus }
                };

                var queryString = string.Join("&", querParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
                var response = await Http.GetAsync($"/api/Game/list?{queryString}", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<PagedResult<GameListDto>>(cancellationToken: cancellationToken);
                    if (result != null)
                    {
                        games = result.Items ?? new List<GameListDto>();
                        TotalCount = result.TotalCount; // ใช้คำนวณ Pagination ในตาราง
                    }
                }
                else
                {
                    errorMessage = $"ไม่สามารถโหลดข้อมูลเกม: {response.ReasonPhrase}";
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"เกิดข้อผิดพลาด: {ex.Message}";
            }
            finally
            {
                isLoading = false;
            }
        }

        private Task PreviousPage() => PageNumber > 1
            ? LoadGamesAsync(PageNumber - 1, PageSize, CancellationToken.None)
            : Task.CompletedTask;

        private Task NextPage() => PageNumber < TotalPages
            ? LoadGamesAsync(PageNumber + 1, PageSize, CancellationToken.None)
            : Task.CompletedTask;

        public async ValueTask DisposeAsync()
        {
            searchCancellation?.Cancel();
            searchCancellation?.Dispose();

            if (gameHubConnection is not null)
            {
                await gameHubConnection.DisposeAsync();
            }
        }
    }
}