using FlowBoard.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace FlowBoard.Api.Authentication;

/// <summary>
/// Registers JWT bearer authentication for the API. Binds <see cref="JwtOptions"/>, registers the
/// shared <see cref="IJwtKeyProvider"/> singleton (used both here for validation and by Identity
/// for issuance), and configures the bearer scheme to validate the issuer, audience, lifetime, and
/// RS256 signature of incoming access tokens.
/// </summary>
public static class JwtAuthenticationExtensions
{
    /// <summary>Adds JWT bearer authentication and the supporting JWT services.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetSection(JwtOptions.SectionName);
        services.Configure<JwtOptions>(section);

        var jwtOptions = section.Get<JwtOptions>() ?? new JwtOptions();

        // One key instance signs (in Identity) and validates (here). Registered as a singleton so
        // the same key is shared across the process.
        var keyProvider = new RsaJwtKeyProvider(jwtOptions);
        services.AddSingleton<IJwtKeyProvider>(keyProvider);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Keep claim names as issued (sub, email, jti) rather than remapping to the long
                // legacy claim-type URIs.
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = keyProvider.SigningKey,
                    ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
            });

        services.AddAuthorization();

        return services;
    }
}
