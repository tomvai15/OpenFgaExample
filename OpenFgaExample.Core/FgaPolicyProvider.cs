using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace OpenFgaExample.Core;

public class FgaPolicyProvider(IOptions<AuthorizationOptions> options) : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider = new(options);

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
        _fallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() =>
        _fallbackPolicyProvider.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var parts = policyName.Split(':');
        
        if (parts.Length < 2) 
            return _fallbackPolicyProvider.GetPolicyAsync(policyName);
        
        var relation = parts[0];
        var objectType = parts[1];
        var objectIdParamName = parts.Length > 2 ? parts[2]  : null;

        var policy = new AuthorizationPolicyBuilder()
        .AddRequirements(new FgaRequirement(relation, objectType, objectIdParamName))
        .Build();

        return Task.FromResult<AuthorizationPolicy?>(policy);

        // If it’s not an FGA policy, fallback
    }
}