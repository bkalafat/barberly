using System.Reflection;

namespace Barberly.Domain;

/// <summary>
/// Assembly reference marker for the Domain layer
/// Used by other layers to reference this assembly
/// </summary>
public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
