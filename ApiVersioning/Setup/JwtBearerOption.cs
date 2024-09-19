using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ApiVersioning.Setup;

public class JwtBearerOptionsSetup : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly JwtOptions _jwtOption;
    public JwtBearerOptionsSetup(IOptions<JwtOptions> jwtOption)
    {
        _jwtOption = jwtOption.Value;
    }

    public void Configure(JwtBearerOptions options)
    {
        byte[] key = Encoding
           .UTF8
           .GetBytes(_jwtOption.SecretKey);

        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtOption.Issuer,
            ValidAudience = _jwtOption.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(key),
        };
    }

    public void Configure(string? name, JwtBearerOptions options) => Configure(options);
 }

