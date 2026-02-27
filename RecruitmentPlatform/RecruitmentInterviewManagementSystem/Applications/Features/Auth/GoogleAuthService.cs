using Google.Apis.Auth;
using RecruitmentInterviewManagementSystem.Applications.Features.Auth.DTO;
using RecruitmentInterviewManagementSystem.Applications.Features.Interface;
using RecruitmentInterviewManagementSystem.Applications.Interface;
namespace RecruitmentInterviewManagementSystem.Applications.Features.Auth;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IGoogleTokenValidator _googleValidator;
    private readonly IJwtService _jwtService;

    public GoogleAuthService(
        IUserRepository userRepository,
        IGoogleTokenValidator googleValidator,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _googleValidator = googleValidator;
        _jwtService = jwtService;
    }

    public async Task<LoginResponse> LoginAsync(string idToken)
    {
        var googleUser = await _googleValidator.ValidateAsync(idToken);

        var user = await _userRepository.GetByEmailAsync(googleUser.Email);

        if (user == null)
        {
            user = await _userRepository
                .CreateGoogleUserAsync(googleUser.Email, googleUser.Name);
        }

        var token = _jwtService.GenerateToken(user);

        return new LoginResponse
        {
            AccessToken = token,
            Email = user.Email,
            FullName = user.FullName
        };
    }
}