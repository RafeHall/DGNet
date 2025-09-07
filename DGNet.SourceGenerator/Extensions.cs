using Microsoft.CodeAnalysis;

namespace DGNet.SourceGenerator;

internal static class Extensions
{
    public static string FullPathAndNamespace(this INamedTypeSymbol s) =>
        s.ToDisplayString(NullableFlowState.NotNull, SymbolDisplayFormat.FullyQualifiedFormat
            .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));

    public static bool IsEventGroupAttribute(this INamedTypeSymbol s) => s.FullPathAndNamespace() == "DGNet.Event.GenerateEventsAttribute";
}