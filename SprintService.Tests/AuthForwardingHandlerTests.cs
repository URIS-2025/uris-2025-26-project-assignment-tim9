using System.Net;
using Microsoft.AspNetCore.Http;
using Moq;
using SprintService.ServiceCalls.Project;

namespace SprintService.Tests
{
    public class AuthForwardingHandlerTests
    {
        private sealed class RecordingHandler : DelegatingHandler
        {
            public HttpRequestMessage? LastRequest { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                LastRequest = request;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }
        }

        private static (AuthForwardingHandler Handler, RecordingHandler Inner) CreateHandler(HttpContext? httpContext)
        {
            var accessor = new Mock<IHttpContextAccessor>();
            accessor.Setup(a => a.HttpContext).Returns(httpContext);

            var inner = new RecordingHandler();
            var handler = new AuthForwardingHandler(accessor.Object) { InnerHandler = inner };
            return (handler, inner);
        }

        [Fact]
        public async Task SendAsync_WithIncomingBearerToken_ForwardsItToOutgoingRequest()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Authorization = "Bearer abc123";
            var (handler, inner) = CreateHandler(context);
            using var invoker = new HttpMessageInvoker(handler);

            await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://project-service.test/api/milestone/project/x"), default);

            Assert.NotNull(inner.LastRequest!.Headers.Authorization);
            Assert.Equal("Bearer", inner.LastRequest.Headers.Authorization!.Scheme);
            Assert.Equal("abc123", inner.LastRequest.Headers.Authorization!.Parameter);
        }

        [Fact]
        public async Task SendAsync_WithNoIncomingHttpContext_LeavesOutgoingRequestUnauthenticated()
        {
            var (handler, inner) = CreateHandler(httpContext: null);
            using var invoker = new HttpMessageInvoker(handler);

            await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://project-service.test/api/milestone/project/x"), default);

            Assert.Null(inner.LastRequest!.Headers.Authorization);
        }

        [Fact]
        public async Task SendAsync_WithNoIncomingAuthorizationHeader_LeavesOutgoingRequestUnauthenticated()
        {
            var context = new DefaultHttpContext();
            var (handler, inner) = CreateHandler(context);
            using var invoker = new HttpMessageInvoker(handler);

            await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://project-service.test/api/milestone/project/x"), default);

            Assert.Null(inner.LastRequest!.Headers.Authorization);
        }
    }
}
