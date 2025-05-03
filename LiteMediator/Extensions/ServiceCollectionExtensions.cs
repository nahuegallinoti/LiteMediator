using System.Reflection;
using LiteMediator.Abstractions;
using LiteMediator.Behaviors;
using LiteMediator.Core;
using Microsoft.Extensions.DependencyInjection;

namespace LiteMediator.Extensions;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLiteMediator(this IServiceCollection services, Action<LiteMediatorOptions>? configure = null)
    {
        var options = new LiteMediatorOptions();
        configure?.Invoke(options);

        var serviceFactoryDescriptor = new ServiceDescriptor(typeof(ServiceFactory), provider => new ServiceFactory(provider.GetService), options.Lifetime);
        services.Add(serviceFactoryDescriptor);

        var mediatorServiceDescriptor = new ServiceDescriptor(typeof(IMediator), typeof(Mediator), options.Lifetime);
        services.Add(mediatorServiceDescriptor);

        // Registramos los handlers usando el código generado
        //LiteMediatorGeneratedRegistrations.RegisterHandlers(services, options.Lifetime);

        // Registramos los behaviors
        foreach (var openBehavior in options.OpenBehaviors)
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), openBehavior);
        }

        return services;
    }
}



public class LiteMediatorOptions
{
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Scoped;
    //public Assembly[] Assemblies { get; set; } = [];

    internal List<Type> OpenBehaviors { get; } = [];

    public void AddOpenBehavior(Type openBehavior)
    {
        if (!openBehavior.IsGenericTypeDefinition)
            throw new ArgumentException("Only open generic types are supported", nameof(openBehavior));

        var genericArgs = openBehavior.GetGenericArguments();
        if (genericArgs.Length != 2)
            throw new ArgumentException("Behavior must have two generic arguments", nameof(openBehavior));

        OpenBehaviors.Add(openBehavior);
    }
}
