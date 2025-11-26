namespace Zeus.Models;

public class UserTokenClaims
{
    public string Login { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public Role Role { get; set; }
    public string Email { get; set; } = string.Empty;
}