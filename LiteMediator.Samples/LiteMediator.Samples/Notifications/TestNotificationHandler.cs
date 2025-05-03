using LiteMediator.Abstractions;
using LiteMediator.Samples.Extensions;

namespace LiteMediator.Samples.Notifications;

public record TestNotification(string NotificationText) : INotification;

public class TestNotificationHandler : INotificationHandler<TestNotification>
{
    public Task Handle(TestNotification notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"{ConsoleColors.Cyan}[{nameof(TestNotificationHandler)}]{ConsoleColors.Reset}: {notification.NotificationText}\n");
        return Task.CompletedTask;
    }
}

public class TestNotificationAnotherHandler : INotificationHandler<TestNotification>
{
    public Task Handle(TestNotification notification, CancellationToken cancelToken)
    {
        Console.WriteLine($"{ConsoleColors.Green}[{nameof(TestNotificationAnotherHandler)}]{ConsoleColors.Reset}: {notification.NotificationText}\n");
        return Task.CompletedTask;
    }
}
