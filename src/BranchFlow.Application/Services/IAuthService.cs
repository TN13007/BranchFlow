using BranchFlow.Application.DTOs.Sprint4;

namespace BranchFlow.Application.Services;

public interface IAuthService
{
    Task<TokenPair?> LoginAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default);

    Task<TokenPair?> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);
}
