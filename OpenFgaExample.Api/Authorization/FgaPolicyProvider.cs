using Microsoft.Extensions.Options;

namespace OpenFgaExample.Api.Authorization;

using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

public class FgaPolicyProvider(IOptions<AuthorizationOptions> options) : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider = new(options);

    public Task<AuthorizationPolicy?> GetDefaultPolicyAsync() =>
        _fallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() =>
        _fallbackPolicyProvider.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // PolicyName is something like "reader:document"
        var parts = policyName.Split(':');
        if (parts.Length == 2)
        {
            var relation = parts[0];
            var objectType = parts[1];

            var policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new FgaRequirement(relation, objectType))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        // If it’s not an FGA policy, fallback
        return _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }
}