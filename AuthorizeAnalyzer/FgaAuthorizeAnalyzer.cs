using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AuthorizeAnalyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class FgaAuthorizeAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "FG001";
    private static readonly LocalizableString Title = "Invalid route parameter in FgaAuthorize";
    private static readonly LocalizableString MessageFormat = "The parameter '{0}' is not defined in the route template.";
    private const string Category = "Usage";

    private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId, Title, MessageFormat, Category,
        DiagnosticSeverity.Error, isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics 
        => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
    }

    private void AnalyzeMethod(SymbolAnalysisContext context)
    {
        var methodSymbol = (IMethodSymbol)context.Symbol;

        // Find FgaAuthorize attribute
        var fgaAttr = methodSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name == "FgaAuthorizeAttribute");

        if (fgaAttr == null) return;

        // Get second argument = route param
        if (fgaAttr.ConstructorArguments.Length < 2) return;
        var routeParamArg = fgaAttr.ConstructorArguments[1].Value as string;
        if (string.IsNullOrEmpty(routeParamArg)) return;

        // Find HttpGet/Post/etc. attributes with route template
        var routeAttr = methodSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name == "HttpGetAttribute"
                              || a.AttributeClass?.Name == "HttpPostAttribute"
                              || a.AttributeClass?.Name == "HttpPutAttribute"
                              || a.AttributeClass?.Name == "HttpDeleteAttribute");

        if (routeAttr == null || routeAttr.ConstructorArguments.Length == 0) return;

        var template = routeAttr.ConstructorArguments[0].Value as string;
        if (string.IsNullOrEmpty(template)) return;

        // Extract params from route (e.g. "{id:guid}" -> "id")
        var routeParams = Regex.Matches(template, @"\{(\w+)(:[^}]*)?\}")
            .Cast<Match>()
            .Select(m => m.Groups[1].Value)
            .ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

        if (!routeParams.Contains(routeParamArg))
        {
            var diagnostic = Diagnostic.Create(Rule,
                fgaAttr.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation(),
                routeParamArg);
            context.ReportDiagnostic(diagnostic);
        }
    }
}