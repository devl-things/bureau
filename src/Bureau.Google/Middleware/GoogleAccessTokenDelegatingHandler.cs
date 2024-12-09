using Bureau.Core;
using Bureau.Google.Managers;
using Bureau.Identity.Extensions;
using Bureau.Identity.Factories;
using Bureau.Identity.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

namespace Bureau.Google.Middleware
{
    internal class GoogleAccessTokenDelegatingHandler : DelegatingHandler
    {   
        private readonly IServiceProvider _serviceProvider;
        public GoogleAccessTokenDelegatingHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.GetIsAuthenticationNeeded())
            {
                if (!request.TryGetBureauUserLogin(out BureauUserLoginModel? userLogin) || userLogin == null)
                {
                    return HttpResponseMessageFactory.CreateBureauUserLoginUnauthorizedResponse();
                }
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    IGoogleAccessTokenManager tokenManager = _serviceProvider.GetRequiredService<IGoogleAccessTokenManager>();
                    Result<AuthenticationHeaderValue> authHeaderResult = await tokenManager.GetAuthenticationHeaderAsync(userLogin);
                    if (authHeaderResult.IsError) 
                    {
                        return HttpResponseMessageFactory.CreateBureauUserLoginUnauthorizedResponse(authHeaderResult.Error);
                    }
                    request.Headers.Authorization = authHeaderResult.Value;
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
