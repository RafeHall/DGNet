using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DGNet.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public class EventGroupGenerator : IIncrementalGenerator
{
    private static readonly (ulong, string)[] SizeTypes = [
        (byte.MaxValue, "byte"),
        (ushort.MaxValue, "ushort"),
        (uint.MaxValue, "uint"),
        (ulong.MaxValue, "ulong"),
    ];

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.ForAttributeWithMetadataName(
            "DGNet.Event.EventGroupAttribute",
            static (node, _) => node is TypeDeclarationSyntax,
            static (context, _) =>
            {
                var symbol = (context.TargetSymbol as INamedTypeSymbol)!;
                var eventGroupNode = context.TargetNode;
                var eventNodes = eventGroupNode
                    .ChildNodes()
                    .Where(n => n.IsKind(SyntaxKind.RecordDeclaration))
                    .ToArray();

                return (symbol, eventGroupNode, eventNodes);
            }
        );

        context.RegisterSourceOutput(provider, (c, v) => EventGroupOutput(c, v.symbol, v.eventGroupNode, v.eventNodes));
    }

    public static void EventGroupOutput(SourceProductionContext context, INamedTypeSymbol symbol, SyntaxNode eventGroupNode, SyntaxNode[] eventNodes)
    {
        var name = symbol!.Name;
        var containedNamespace = symbol.ContainingNamespace;

        var members = symbol.GetTypeMembers();

        // NOTE: Attributes can limit what they can be put on, but records fall under the category of classes
        // so a manual check here is necessary as classes shouldn't be allowed.
        if (!symbol.IsRecord)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Common.EventGroupOnClassNotRecord,
                symbol.Locations.FirstOrDefault(),
                symbol.ToDisplayString()
            ));

            return;
        }

        var events = members
            .Where(s => s.Kind == SymbolKind.NamedType)
            .Cast<INamedTypeSymbol>()
            .Where(s => s.IsRecord)
            .ToArray();

        var b = new StringBuilder();
        // b.AppendLine("/*");
        // b.AppendLine();
        // b.AppendLine("*/");

        b.AppendLine($"namespace {containedNamespace};\n");

        b.AppendLine($"public partial record {name} {{");
        b.AppendLine($"\tpublic delegate void {name}Delegate({name} ev);");
        b.AppendLine($"\tpublic static event {name}Delegate? Event;\n");

        var sizeType = SizeTypes.First((t) => (ulong)events.Length < t.Item1).Item2;

        // EventGroup.EventKind
        b.AppendLine($"\tpublic enum EventKind : {sizeType} {{");
        for (int i = 0; i < events.Length; i++)
        {
            var e = events[i];
            b.AppendLine($"\t\t{e.Name} = {i},");
        }
        b.AppendLine("\t}\n");

        // EventGroup.Serialize
        b.AppendLine("\tpublic static void Serialize(byte @event, object self, DGNet.Serde.Serializer se) {");
        b.AppendLine("\t\tEventKind kind = (EventKind)@event;");
        b.AppendLine("\t\tswitch(kind) {");
        foreach (var e in events)
        {
            b.AppendLine($"\t\t\tcase EventKind.{e.Name}:");
            b.AppendLine($"\t\t\t\t{e.Name}.Serialize(({e.Name})self, se);");
            b.AppendLine("\t\t\t\tbreak;");
        }
        b.AppendLine("\t\t\tdefault:");
        b.AppendLine("\t\t\t\tthrow new IndexOutOfRangeException();");
        b.AppendLine("\t\t}");
        b.AppendLine("\t}\n");

        // EventGroup.Deserialize
        b.AppendLine($"\tpublic static {name} Deserialize(byte @event, DGNet.Serde.Deserializer de) {{");
        b.AppendLine("\t\tEventKind kind = (EventKind)@event;");
        b.AppendLine("\t\treturn kind switch {");
        foreach (var e in events)
        {
            b.AppendLine($"\t\t\tEventKind.{e.Name} => {e.Name}.Deserialize(de),");
        }
        b.AppendLine("\t\t\t_ => throw new IndexOutOfRangeException(),");
        b.AppendLine("\t\t};");
        b.AppendLine("\t}\n");

        // EventGroup.Receive
        b.AppendLine($"\tpublic static void Receive({name} ev) {{");
        b.AppendLine("\t\tEvent!.Invoke(ev);");
        b.AppendLine("\t}");

        // EventGroup.Event
        foreach (var (e, en) in events.Zip(eventNodes, (a, b) => (a, b)))
        {
            var eventBase = e.BaseType;
            // NOTE: Ensure the event extends the event group type
            if (!SymbolEqualityComparer.Default.Equals(eventBase, symbol))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Common.EventGroupEventIncorrectBase,
                    e.Locations.FirstOrDefault(),
                    e.ToDisplayString()
                ));
                continue;
            }

            // NOTE: Check the SyntaxNode that there is a parameters list as part of the record definition (aka. the primary initializer)
            var parameterList = en.ChildNodes()
                .Where(n => n.IsKind(SyntaxKind.ParameterList))
                .FirstOrDefault();

            if (parameterList == null)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Common.EventGroupEventNotPrimaryConstructor,
                    e.Locations.FirstOrDefault(),
                    e.ToDisplayString()
                ));
                continue;
            }

            var primary = e.GetMembers()
                .Where(s => s.Kind == SymbolKind.Method)
                .Cast<IMethodSymbol>()
                .Where(m => m.MethodKind == MethodKind.Constructor)
                .First();

            var eventMembers = e.GetMembers();
            var properties = eventMembers
                .Where(s => s.Kind == SymbolKind.Property)
                .Cast<IPropertySymbol>()
                .Where(p => !(p.IsOverride || p.IsVirtual || p.IsAbstract))
                .Where(p => p.CanBeReferencedByName)
                .ToArray();

            // NOTE: Sanity check, the previous check for a parameter list should cover all bases but just to be sure...
            // NOTE: Luckily properties defined in order with the primary constructor generated properties first
            // which means any user defined properties will be afterwards, zip will only zip Min(a.Length, b.Length) elements
            // will not interfere with checking for a primary constructor and the check will be trivial...
            var filtered = primary.Parameters
                .Zip(properties, (a, b) => a.Name == b.Name)
                .All(b => b);

            if (!filtered)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Common.EventGroupEventNotPrimaryConstructor,
                    e.Locations.FirstOrDefault(),
                    e.ToDisplayString()
                ));
                continue;
            }

            // Validate types are serializable / deserializable
            for (int i = 0; i < primary.Parameters.Length; i++)
            {
                
                var param = primary.Parameters[i];
                var p = properties[i];
                var type = p.Type;

                b.AppendLine($"// {p.Name} TypeKind = {type.TypeKind};");

                if (type.TypeKind == TypeKind.Enum)
                {
                    var namedType = (INamedTypeSymbol)type;
                    type = namedType.EnumUnderlyingType!;
                    b.AppendLine($"// {p.Name} EnumType = {type};");
                }
                else if (type.TypeKind == TypeKind.Array)
                {
                    var arrayType = (IArrayTypeSymbol)type;
                    type = arrayType.ElementType;
                    b.AppendLine($"// {p.Name} ArrayType = {type};");
                }

                var specialType = type.SpecialType;
                b.AppendLine($"// {p.Name} SpecialType = {specialType};");

                if (!ValidType(type))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        Common.EventGroupUnsupportedType,
                        param.Locations.FirstOrDefault(),
                        type.ToDisplayString()
                    ));
                    continue;
                }
            }

            b.AppendLine($"\tpublic partial record {e.Name} {{");
            b.AppendLine($"\t\tpublic delegate void {e.Name}Delegate({e.Name} ev);");
            b.AppendLine($"\t\tpublic static new event {e.Name}Delegate? Event;\n");
            b.AppendLine($"\t\tpublic static void Receive({e.Name} ev) {{");
            b.AppendLine("\t\t\tEvent!.Invoke(ev);");
            b.AppendLine("\t\t}");
            b.AppendLine("\t}\n");
        }
        b.AppendLine("}");

        context.AddSource($"{name}.EventGroup.cs", b.ToString());
    }

    private static bool ValidType(ITypeSymbol type)
    {
        switch (type.TypeKind)
        {
            case TypeKind.Enum:
                {
                    var namedType = (INamedTypeSymbol)type;
                    return ValidType(namedType.EnumUnderlyingType!);
                }
            case TypeKind.Array:
                {
                    var arrayType = (IArrayTypeSymbol)type;
                    return ValidType(arrayType.ElementType);
                }
            case TypeKind.Struct:
            case TypeKind.Class:
                {
                    return ValidSpecialType(type.SpecialType);
                }
            default:
                return false;
        }
    }

    private static bool ValidSpecialType(SpecialType type)
    {
        return type switch
        {
            SpecialType.System_String => true,
            SpecialType.System_Int64 => true,
            SpecialType.System_UInt64 => true,
            SpecialType.System_Int32 => true,
            SpecialType.System_UInt32 => true,
            SpecialType.System_Int16 => true,
            SpecialType.System_UInt16 => true,
            SpecialType.System_Byte => true,
            SpecialType.System_SByte => true,
            SpecialType.System_Boolean => true,
            SpecialType.System_Single => true,
            SpecialType.System_Double => true,
            _ => false,
        };
    }
}