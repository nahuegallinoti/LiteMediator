using LiteMediator.Abstractions;
using LiteMediator.Extensions;
using LiteMediator.Samples.Behaviors;
using LiteMediator.Samples.Extensions;
using LiteMediator.Samples.Handlers;
using LiteMediator.Samples.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

Console.WriteLine($"{ConsoleColors.Bold}{ConsoleColors.Cyan}=== LiteMediator Sample Started ==={ConsoleColors.Reset}\n");

ServiceCollection services = new();

services.AddLogging(config =>
{
    config.AddConsole();
});

// Registro automático de handlers generados
LiteMediatorGeneratedRegistrations.RegisterHandlers(services, ServiceLifetime.Scoped);

services.AddLiteMediator(options =>
{
    //options.Assemblies = [typeof(TestRequestHandler).Assembly];
    options.AddOpenBehavior(typeof(RequestLogginBehavior<,>));
    options.AddOpenBehavior(typeof(RequestLogginBehaviorDos<,>));
});


// Construcción del provider y obtención del mediador
var provider = services.BuildServiceProvider();
var mediator = provider.GetRequiredService<IMediator>();

#region Send
Console.WriteLine($"{ConsoleColors.Bold}{ConsoleColors.Yellow}------ SEND EXAMPLE ------{ConsoleColors.Reset}");

var response = await mediator.Send(new TestRequest("Hello World"));
Console.WriteLine($"{ConsoleColors.Green}[Send Response]{ConsoleColors.Reset}: {response}\n");
#endregion

#region Publish
Console.WriteLine($"{ConsoleColors.Bold}{ConsoleColors.Yellow}------ PUBLISH EXAMPLE ------{ConsoleColors.Reset}");

await mediator.Publish(new TestNotification("Notificación de usuario registrado desde publish concreto"));

INotification notification = new TestNotification("Notificación de usuario registrado desde publish genérico");
await mediator.Publish(notification);

Console.WriteLine();
#endregion

#region Stream
Console.WriteLine($"{ConsoleColors.Bold}{ConsoleColors.Yellow}------ STREAM EXAMPLE ------{ConsoleColors.Reset}");

await foreach (var item in mediator.CreateStream(new TestStreamRequest(5)))
{
    Console.WriteLine($"{ConsoleColors.Magenta}[Stream Item]{ConsoleColors.Reset}: {item}");
}

Console.WriteLine();
#endregion

Console.WriteLine($"{ConsoleColors.Bold}{ConsoleColors.Cyan}=== LiteMediator Sample Finished ==={ConsoleColors.Reset}");
Console.ReadLine();