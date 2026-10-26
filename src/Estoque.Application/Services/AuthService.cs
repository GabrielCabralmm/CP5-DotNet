using Estoque.Application.DTOs;
using Estoque.Application.Interfaces;
using Estoque.Domain.Common;
using Estoque.Domain.Errors;
using Estoque.Domain.Interfaces;

namespace Estoque.Application.Services;

public sealed class AuthService(IUsuarioRepository usuarios, IPasswordHasher hasher, ITokenService tokenService) : IAuthService
{
    public async Task<OperationResult<TokenResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return AuthErrors.CredenciaisInvalidas();

        var usuario = await usuarios.GetByUsernameAsync(request.Username.Trim().ToLowerInvariant(), ct);
        if (usuario is null || !hasher.Verify(request.Password, usuario.PasswordHash))
            return AuthErrors.CredenciaisInvalidas();

        return OperationResult<TokenResponse>.Success(tokenService.GerarToken(usuario));
    }
}
