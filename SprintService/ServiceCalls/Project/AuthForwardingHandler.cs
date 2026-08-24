using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace SprintService.ServiceCalls.Project
{
    public class AuthForwardingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthForwardingHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var incomingAuthHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();

            if (!string.IsNullOrWhiteSpace(incomingAuthHeader)
                && AuthenticationHeaderValue.TryParse(incomingAuthHeader, out var parsed))
            {
                request.Headers.Authorization = parsed;
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
