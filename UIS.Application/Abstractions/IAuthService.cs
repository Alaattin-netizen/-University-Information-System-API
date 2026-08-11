using UIS.Application.DTOs.Auth;

namespace UIS.Application.Abstractions;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);

    //Task<AuthResponse> RegisterAsync(RegisterRequest request);
}