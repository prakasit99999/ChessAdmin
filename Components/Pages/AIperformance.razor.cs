using ApexCharts;
using Azure.Core;
using ChessAdmin.Components.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ChessAdmin.Components.Pages
{
    public partial class AIperformance
    {
        [Inject]
        protected HttpClient HttpClient { get; set; } = default!;

        [Inject]
        protected IJSRuntime JSRuntime { get; set; } = default!;

        private int _selectedDepth = 4;
        protected int selectedDepth
        {
            get => _selectedDepth;
            set
            {
                if (_selectedDepth != value)
                {
                    _selectedDepth = value;
                    _ = FetchAnalyticsData();
                }
            }
        }

        protected bool isLoading = false;
        protected AiPerformanceResponseDto responseDto = new();

        protected ApexChartOptions<AiBenchmarkDepthDto> NodeChartOptions { get; set; } = new()
        {
            Chart = new ApexCharts.Chart { Stacked = false, Background = "transparent" },
            Colors = new List<string> { "#dc3545", "#198754" }
        };

        protected ApexChartOptions<AiBenchmarkDepthDto> TimeChartOptions { get; set; } = new()
        {
            Chart = new ApexCharts.Chart { Background = "transparent" },
            Colors = new List<string> { "#fd7e14", "#0d6efd" }
        };

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await FetchAnalyticsData();
            }
        }

        protected async Task FetchAnalyticsData()
        {
            isLoading = true;

            try
            {
                var token = await JSRuntime.InvokeAsync<string?>("localStorage.getItem", "token");
                var request = new HttpRequestMessage(HttpMethod.Get, $"api/AiPerformance/admin/Analytic?depth={selectedDepth}");
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await HttpClient.SendAsync(request);
                if(response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AiPerformanceResponseDto>();
                    if (result != null)
                    {
                        responseDto = result;
                    }
                }
                else
                {
                    Console.WriteLine($"Failed to fetch analytics: {response.ReasonPhrase}");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching analytics: {ex.Message}");
            }
            finally
            {
                isLoading = false;
            }
        }
    }
}