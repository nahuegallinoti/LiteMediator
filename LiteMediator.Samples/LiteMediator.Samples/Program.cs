using LiteMediator.Abstractions;
using LiteMediator.Extensions;
using LiteMediator.Samples.Behaviors;
using LiteMediator.Samples.Extensions;
using LiteMediator.Samples.Handlers;
using LiteMediator.Samples.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

StyledConsole.Info("=== LiteMediator Sample Started ===\n");

ServiceCollection services = new();

services.AddLogging(config =>
{
    config.AddConsole();
});

LiteMediatorGeneratedRegistrations.RegisterHandlers(services, ServiceLifetime.Scoped);

services.AddLiteMediator(options =>
{
    options.AddOpenBehavior(typeof(RequestLogginBehavior<,>));
    options.AddOpenBehavior(typeof(RequestLogginBehaviorDos<,>));
});

var provider = services.BuildServiceProvider();
var mediator = provider.GetRequiredService<IMediator>();

#region Send
StyledConsole.Section("------ SEND EXAMPLE ------");

var response = await mediator.Send(new TestRequest("Hello World"));
StyledConsole.Success($"[Send Response]: {response}\n");
#endregion

#region Publish
StyledConsole.Section("------ PUBLISH EXAMPLE ------");

await mediator.Publish(new TestNotification("Notificación de usuario registrado desde publish concreto"));

INotification notification = new TestNotification("Notificación de usuario registrado desde publish genérico");
await mediator.Publish(notification);

Console.WriteLine();
#endregion

#region Stream
StyledConsole.Section("------ STREAM EXAMPLE ------");

await foreach (var item in mediator.CreateStream(new TestStreamRequest(5)))
{
    StyledConsole.Item("Stream Item", item.ToString());
}

Console.WriteLine();
#endregion

StyledConsole.Info("=== LiteMediator Sample Finished ===");
Console.ReadLine();