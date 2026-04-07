using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;
using EducationAssignmentPortal.Models;
using System.Text.Json;

namespace EducationAssignmentPortal.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ProtectedSessionStorage _sessionStorage;
        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        private User? _currentUser;

        public CustomAuthenticationStateProvider(ProtectedSessionStorage sessionStorage)
        {
            _sessionStorage = sessionStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // If we already have the user in memory (within the same circuit), return it immediately.
                if (_currentUser != null)
                {
                    return new AuthenticationState(CreateClaimsPrincipal(_currentUser));
                }

                // Attempt to retrieve from session storage.  
                // Note: This may throw an exception during prerendering or if JS interop is not ready.
                var result = await _sessionStorage.GetAsync<User>("UserSession");
                
                if (result.Success && result.Value != null)
                {
                    _currentUser = result.Value;
                    return new AuthenticationState(CreateClaimsPrincipal(_currentUser));
                }
            }
            catch (Exception ex)
            {
                // JS Interop not available yet or other failure
                Console.WriteLine("Authentication error: " + ex.Message);
                return new AuthenticationState(_anonymous);
            }

            return new AuthenticationState(_anonymous);
        }

        public async Task UpdateAuthenticationState(User? user)
        {
            ClaimsPrincipal claimsPrincipal;

            if (user != null)
            {
                _currentUser = user;
                // Store the object directly; ProtectedSessionStorage handles serialization.
                await _sessionStorage.SetAsync("UserSession", user);
                claimsPrincipal = CreateClaimsPrincipal(user);
            }
            else
            {
                _currentUser = null;
                await _sessionStorage.DeleteAsync("UserSession");
                claimsPrincipal = _anonymous;
            }

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        }

        private ClaimsPrincipal CreateClaimsPrincipal(User user)
        {
            // Create identity with "CustomAuth" authentication type to ensure it's IsAuthenticated = true
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("UserId", user.Id.ToString())
            };

            var identity = new ClaimsIdentity(claims, "CustomAuth");
            return new ClaimsPrincipal(identity);
        }
    }
}
