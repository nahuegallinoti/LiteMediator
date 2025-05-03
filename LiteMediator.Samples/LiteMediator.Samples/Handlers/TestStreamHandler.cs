using LiteMediator.Abstractions;
using LiteMediator.Samples.Extensions;
using System.Runtime.CompilerServices;

namespace LiteMediator.Samples.Handlers;

public record TestStreamRequest(int Count) : IStreamRequest<int>;

public class TestStreamHandler : IStreamRequestHandler<TestStreamRequest, int>
{
    public async IAsyncEnumerable<int> Handle(TestStreamRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        Console.WriteLine($"{ConsoleColors.Yellow}[{nameof(TestStreamHandler)}] Streaming {request.Count} items...{ConsoleColors.Reset}");

        for (int i = 0; i < request.Count; i++)
        {
            yield return i;
            await Task.Delay(10, cancellationToken);
        }

        Console.WriteLine($"{ConsoleColors.Yellow}[{nameof(TestStreamHandler)}] Stream completed.{ConsoleColors.Reset}");
    }
}
