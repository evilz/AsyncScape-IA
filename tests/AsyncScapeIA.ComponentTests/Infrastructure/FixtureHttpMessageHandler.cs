namespace AsyncScapeIA.ComponentTests.Infrastructure;

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public sealed class FixtureHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

    public FixtureHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        _responder = responder ?? throw new ArgumentNullException(nameof(responder));
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var response = _responder(request);
            response.RequestMessage ??= request;
            return Task.FromResult(response);
        }
        catch (Exception ex)
        {
            var failure = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                RequestMessage = request,
                ReasonPhrase = ex.Message,
                Content = new StringContent(ex.ToString())
            };
            return Task.FromResult(failure);
        }
    }
}
