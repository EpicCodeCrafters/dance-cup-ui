using System.Security.Claims;

namespace ECC.DanceCup.UI.Auth;

public class CurrentUser : ICurrentUser
{
    public bool Authenticated => Id != default;

    public long Id { get; private set; }

    public string Username { get; private set; } = string.Empty;

    public Task SignInAsync(ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken)
    {
        var idClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
        if (long.TryParse(idClaim?.Value, out long id))
        {
            Id = id;
        }
        
        var usernameClaim = claimsPrincipal.FindFirst(ClaimTypes.Name);
        if (string.IsNullOrWhiteSpace(usernameClaim?.Value) is false)
        {
            Username = usernameClaim.Value;
        }
        
        return Task.CompletedTask;
    }
}