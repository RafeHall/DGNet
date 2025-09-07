using Microsoft.CodeAnalysis;

namespace DGNet.SourceGenerator;


public class Common
{
    public static readonly DiagnosticDescriptor EventGroupOnClassNotRecord = new(
        "DGN0201",
        "The [EventGroup] attribute must be used on a record",
        "The class `{0}` must be a record to use the [EventGroup] attribute",
        "Usage",
        DiagnosticSeverity.Error,
        true,
        "The [EventGroup] attribute must be used on a record. Change the class to a record or remove the [EventGroup] attribute."
    );

    public static readonly DiagnosticDescriptor EventGroupEventIncorrectBase = new(
        "DGN0202",
        "The [EventGroup] event must inherit from the [EventGroup]",
        "The record `{0}` must inherit from `{1}` to be a valid event",
        "Usage",
        DiagnosticSeverity.Error,
        true,
        "The [EventGroup] event must inherit from it's [EventGroup]. Change it's base class or it will be ignored."
    );

    public static readonly DiagnosticDescriptor EventGroupEventNotPrimaryConstructor = new(
        "DGN0203",
        "The [EventGroup] event must use a primary constructor",
        "The record `{0}` must use a primary constructor to be a valid event",
        "Usage",
        DiagnosticSeverity.Error,
        true,
        "The [EventGroup] must use a primary constructor. Add one or this event will be ignored."
    );

    public static readonly DiagnosticDescriptor EventGroupUnsupportedType = new(
        "DGN0204",
        "Unsupported type used in event as it cannot be serialized or deserialized",
        "The type `{0}` cannot be used in an event as it cannot be serialized or deserialized",
        "Usage",
        DiagnosticSeverity.Error,
        true,
        "Events do not support this type, remove from event or make is serializable and deserializable."
    );
}