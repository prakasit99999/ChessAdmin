using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.JSInterop;

namespace ChessAdmin.Services
{
    public class AdminAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;

        public AdminAuthStateProvider(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try {
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "token");
                if (string.IsNullOrEmpty(token))
                {
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                if (jwtToken.ValidTo < DateTime.UtcNow)
                {
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }
                var identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
                var user = new ClaimsPrincipal(identity);

                return new AuthenticationState(user);
            }
            catch
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        public void NotifyUserAuthentication(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public void NotifyLogout()
        {
            var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(
                Task.FromResult(new AuthenticationState(anonymous)));
        }
    }
}
