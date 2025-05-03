namespace SourceGenerator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;


[Generator]
public class LiteMediatorGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
//#if DEBUG
//        if (!System.Diagnostics.Debugger.IsAttached)
//        {
//            System.Diagnostics.Debugger.Launch();
//        }
//#endif
        // Registrar para encontrar todos los tipos que implementan las interfaces de handlers
        var handlerTypes = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is ClassDeclarationSyntax,
                transform: static (ctx, ct) =>
                {
                    var classDeclaration = (ClassDeclarationSyntax)ctx.Node;
                    var symbol = ctx.SemanticModel.GetDeclaredSymbol(classDeclaration, ct) as INamedTypeSymbol;

                    if (symbol == null) return null;

                    var handlerInterfaces = new List<INamedTypeSymbol>();

                    foreach (var iface in symbol.AllInterfaces)
                    {
                        if (!iface.IsGenericType) continue;

                        var genericName = iface.ConstructedFrom.ToDisplayString();

                        if (genericName == "LiteMediator.Abstractions.IRequestHandler<TRequest, TResponse>" ||
                            genericName == "LiteMediator.Abstractions.INotificationHandler<TNotification>" ||
                            genericName == "LiteMediator.Abstractions.IStreamRequestHandler<TRequest, TResponse>")
                        {
                            handlerInterfaces.Add(iface);
                        }
                    }

                    if (handlerInterfaces.Count == 0)
                        return null;

                    return new HandlerInfo
                    {
                        ClassName = symbol.ToString(),
                        Interfaces = handlerInterfaces.Select(i => i.ToString()).ToList()
                    };
                })
            .Where(static m => m != null);

        // Combinar todos los handlers en una colección
        var allHandlers = handlerTypes.Collect();

        // Generar el código de registro
        context.RegisterSourceOutput(allHandlers, (spc, handlers) =>
        {
            var sourceBuilder = new StringBuilder();

            sourceBuilder.AppendLine("using Microsoft.Extensions.DependencyInjection;");
            sourceBuilder.AppendLine("using System;");
            sourceBuilder.AppendLine();
            sourceBuilder.AppendLine("namespace LiteMediator.Extensions");
            sourceBuilder.AppendLine("{");
            sourceBuilder.AppendLine("    /// <summary>");
            sourceBuilder.AppendLine("    /// Clase generada automáticamente para registrar handlers en LiteMediator");
            sourceBuilder.AppendLine("    /// </summary>");
            sourceBuilder.AppendLine("    public static partial class LiteMediatorGeneratedRegistrations");
            sourceBuilder.AppendLine("    {");
            sourceBuilder.AppendLine("        public static void RegisterHandlers(IServiceCollection services, ServiceLifetime lifetime)");
            sourceBuilder.AppendLine("        {");

            foreach (var handler in handlers)
            {
                foreach (var iface in handler?.Interfaces ?? [])
                {
                    sourceBuilder.AppendLine($"            services.Add(new ServiceDescriptor(typeof({iface}), typeof({handler?.ClassName}), lifetime));");
                }
            }

            sourceBuilder.AppendLine("        }");
            sourceBuilder.AppendLine("    }");
            sourceBuilder.AppendLine("}");

            spc.AddSource("LiteMediatorGeneratedRegistrations.g.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        });
    }

    private class HandlerInfo
    {
        public string? ClassName { get; set; }
        public List<string>? Interfaces { get; set; }
    }
}