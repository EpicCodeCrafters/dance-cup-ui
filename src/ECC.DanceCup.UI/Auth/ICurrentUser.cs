using System.Security.Claims;

namespace ECC.DanceCup.UI.Auth;

public interface ICurrentUser
{
    bool Authenticated { get; }
    
    long Id { get; }
    
    string Username { get; }

    Task SignInAsync(ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken);
}