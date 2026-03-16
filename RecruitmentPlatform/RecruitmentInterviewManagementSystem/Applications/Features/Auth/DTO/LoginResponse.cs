public class LoginResponse
{
    public Guid UserId { get; set; }

    public Guid? IdCompany { get; set; }

    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public int Role { get; set; }
    
}