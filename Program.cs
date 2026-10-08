using ApexCharts;
using ChessAdmin.Components;
using ChessAdmin.Services;
//using ChessAdmin.Hubs;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddApexCharts();

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, AdminAuthStateProvider>();
builder.Services.AddSignalR();
builder.Services.AddScoped(sp => new HttpClient { 
    BaseAddress = new Uri("http://localhost:8080/") });

// ลงทะเบียน Notification Service
builder.Services.AddScoped<INotificationService, NotificationService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAntiforgery();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
//app.MapHub<GameHub>("/gamehub");
app.Run();
