using Microsoft.CodeAnalysis;

namespace DGNet.SourceGenerator;


public class Common
{
    public static readonly DiagnosticDescriptor EventClassNotRecord = new(
        "DGN0201",
        "The [GenerateEvents] attribute must be used on a record",
        "The class `{0}` must be a record to use the [GenerateEvents] attribute",
        "Usage",
        DiagnosticSeverity.Error,
        true,
        "The [GenerateEvents] attribute must be used on a record. Change the class to a record or remove the [GenerateEvents] attribute."
    );

    public static readonly DiagnosticDescriptor EventIncorrectBase = new(
        "DGN0202",
        "The [GenerateEvents] event must inherit from the [GenerateEvents]",
        "The record `{0}` must inherit from `{1}` to be a valid event",
        "Usage",
        DiagnosticSeverity.Error,
        true,
        "The [GenerateEvents] event must inherit from it's [GenerateEvents]. Change it's base class or it will be ignored."
    );

    public static readonly DiagnosticDescriptor EventNotUsingPrimaryConstructor = new(
        "DGN0203",
        "The [GenerateEvents] event must use a primary constructor",
        "The record `{0}` must use a primary constructor to be a valid event",
        "Usage",
        DiagnosticSeverity.Error,
        true,
        "The [GenerateEvents] must use a primary constructor. Add one or this event will be ignored."
    );

    public static readonly DiagnosticDescriptor EventUnsupportedType = new(
        "DGN0204",
        "Unsupported type used in event as it cannot be serialized or deserialized",
        "The type `{0}` cannot be used in an event as it cannot be serialized or deserialized",
        "Usage",
        DiagnosticSeverity.Error,
        true,
        "Events do not support this type, remove from event or make is serializable and deserializable."
    );
}