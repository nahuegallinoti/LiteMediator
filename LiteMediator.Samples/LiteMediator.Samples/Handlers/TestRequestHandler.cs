using LiteMediator.Abstractions;
using LiteMediator.Samples.Extensions;

namespace LiteMediator.Samples.Handlers;

public record TestRequest(string Message) : IRequest<string>;

public class TestRequestHandler : IRequestHandler<TestRequest, string>
{
    public ValueTask<string> Handle(TestRequest request, CancellationToken cancellationToken)
    {
        StyledConsole.Success($"[{nameof(TestRequestHandler)}] Handling request: {request.Message}");
        return new ValueTask<string>($"Echo: {request.Message}");
    }
}