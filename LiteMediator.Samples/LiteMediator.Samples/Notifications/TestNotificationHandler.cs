using LiteMediator.Abstractions;
using LiteMediator.Samples.Extensions;

namespace LiteMediator.Samples.Notifications;

public record TestNotification(string NotificationText) : INotification;

public class TestNotificationHandler : INotificationHandler<TestNotification>
{
    public Task Handle(TestNotification notification, CancellationToken cancellationToken)
    {
        StyledConsole.Info($"[{nameof(TestNotificationHandler)}]: {notification.NotificationText}\n");
        return Task.CompletedTask;
    }
}

public class TestNotificationAnotherHandler : INotificationHandler<TestNotification>
{
    public Task Handle(TestNotification notification, CancellationToken cancelToken)
    {
        StyledConsole.Success($"[{nameof(TestNotificationAnotherHandler)}]: {notification.NotificationText}\n");
        return Task.CompletedTask;
    }
}