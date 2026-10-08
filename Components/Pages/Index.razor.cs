using ApexCharts;
using ChessAdmin.Components.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ChessAdmin.Components.Pages
{
    public partial class Index
    {
        [Inject]
        protected HttpClient HttpClient { get; set; } = default!;

        [Inject]
        protected NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        protected bool isLoading = false;
        //  แก้ไขจุดนี้: เปลี่ยนจาก DashboardMetricsDto เป็น DashboardOverviewDto
        protected DashboardOverviewDto dashboardData = new();
        protected AiPerformanceResponseDto aiPerformanceData = new();

        protected ApexChartOptions<AiBenchmarkDepthDto> NodeChartOptions { get; set; } = new()
        {
            Chart = new ApexCharts.Chart
            {
                Stacked = false,
                Background = "transparent",
                Toolbar = new ApexCharts.Toolbar { Show = false }
            },
            Colors = new List<string> { "#dc3545", "#198754" }
        };

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await LoadDashboardDataAsync();
                StateHasChanged();
            }
        }

        protected async Task LoadDashboardDataAsync()
        {
            isLoading = true;
            try
            {
                var token = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "token");
                if (string.IsNullOrEmpty(token))
                {
                    NavigationManager.NavigateTo("/login");
                    return;
                }
                HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // ดึงข้อมูลภาพรวมจาก API
                var overview = await HttpClient.GetFromJsonAsync<DashboardOverviewDto>("/api/Leaderboard/admin/dashboard");
                if (overview != null)
                {
                     dashboardData = overview;
                }

                // ดึงข้อมูลกราฟ AI Benchmark Analytics
                var aiResult = await HttpClient.GetFromJsonAsync<AiPerformanceResponseDto>("api/AiPerformance/admin/Analytic?depth=4");
                if (aiResult != null)
                {
                    aiPerformanceData = aiResult;
                }
              
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading dashboard data: {ex.Message}");
            }
            finally
            {
                isLoading = false;
            }
        }
    }
}