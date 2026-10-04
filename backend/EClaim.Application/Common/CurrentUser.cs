namespace EClaim.Application.Common;

public class CurrentUser
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
