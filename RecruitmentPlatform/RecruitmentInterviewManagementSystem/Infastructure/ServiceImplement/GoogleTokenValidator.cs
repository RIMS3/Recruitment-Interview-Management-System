using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using RecruitmentInterviewManagementSystem.Applications.Features.Interface;

namespace RecruitmentInterviewManagementSystem.Infastructure.ServiceImplement;

public class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly IConfiguration _configuration;

    public GoogleTokenValidator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<GoogleJsonWebSignature.Payload> ValidateAsync(string idToken)
    {
        var clientId = _configuration["Google:ClientId"]
                       ?? throw new Exception("Google ClientId not configured");

        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = new[] { clientId }
        };

        return await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
    }
}