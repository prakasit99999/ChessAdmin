using ChessAdmin.Components.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ChessAdmin.Components.Pages
{
    public partial class Profile
    {
        [Parameter] public int UserId { get; set; }

        [Inject] public HttpClient Http { get; set; } = default!;
        [Inject] public IJSRuntime JSRuntime { get; set; } = default!;
        [Inject] public NavigationManager Navigation { get; set; } = default!;

        protected bool isLoading = true;
        protected bool isSubmitting = false;
        protected string? errorMessage;
        protected string? successMessage;

        protected UserProfileDto profileModel = new();

        protected override async Task OnInitializedAsync()
        {
            await LoadProfileAsync();
        }

        protected async Task LoadProfileAsync()
        {
            isLoading = true;
            errorMessage = null;
            successMessage = null;
            try
            {
                var token = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "token");
                if (string.IsNullOrEmpty(token))
                {
                    errorMessage = "ผู้ใช้ยังไม่ได้เข้าสู่ระบบ";
                    return;
                }

                Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await Http.GetAsync($"/api/Admin/users/{UserId}");

                if (response.IsSuccessStatusCode)
                {
                    profileModel = await response.Content.ReadFromJsonAsync<UserProfileDto>() ?? new UserProfileDto();
                    // บังคับให้ช่องรหัสผ่านเป็นค่าว่างเสมอหลังจากดึงข้อมูล
                    profileModel.Password = null;
                }
                else
                {
                    errorMessage = $"ไม่สามารถโหลดข้อมูล: {response.ReasonPhrase}";
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

        protected async Task SaveProfileAsync()
        {
            isSubmitting = true;
            errorMessage = null;
            successMessage = null;

            try
            {
                var token = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "token");
                if (string.IsNullOrEmpty(token))
                {
                    errorMessage = "ผู้ใช้ยังไม่ได้เข้าสู่ระบบ";
                    return;
                }

                Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                if (UserId <= 0)
                {
                    errorMessage = "UserId ไม่ถูกต้อง";
                    return;
                }

                // ✅ ตัด Rating และ Status ออกเรียบร้อยครับ
                var updatePayload = new
                {
                    Username = profileModel.Username,
                    Email = profileModel.Email,
                    NewPassword = string.IsNullOrWhiteSpace(profileModel.Password) ? null : profileModel.Password
                };

                var response = await Http.PutAsJsonAsync($"/api/Admin/users/{UserId}", updatePayload);
                var raw = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    profileModel.Password = null; // เคลียร์รหัสผ่านในหน่วยความจำ
                    await LoadProfileAsync();     // โหลดข้อมูลล่าสุดมาแสดง

                    successMessage = "บันทึกข้อมูลสำเร็จ ✓";
                    await Task.Delay(1500);
                    successMessage = null;
                }
                else
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(raw);
                        var root = doc.RootElement;
                        if (root.TryGetProperty("message", out var msgElement) ||
                            root.TryGetProperty("Message", out msgElement))
                        {
                            errorMessage = $"บันทึกข้อมูลไม่สำเร็จ: {msgElement.GetString()}";
                        }
                        else
                        {
                            errorMessage = $"บันทึกข้อมูลไม่สำเร็จ: {response.ReasonPhrase}";
                        }
                    }
                    catch
                    {
                        errorMessage = $"บันทึกข้อมูลไม่สำเร็จ: {response.ReasonPhrase}";
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"เกิดข้อผิดพลาด: {ex.Message}";
            }
            finally
            {
                isSubmitting = false;
            }
        }
    }
}