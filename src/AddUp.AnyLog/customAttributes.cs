#if NET20 || NET35 || NETCOREAPP1_0 || NETCOREAPP1_1 || NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD1_2 || NETSTANDARD1_3 || NETSTANDARD1_4 || NETSTANDARD1_5 || NETSTANDARD1_6
namespace System.Diagnostics.CodeAnalysis
{
    // ExcludeFromCodeCoverageAttribute does not exist in all frameworks...
    [global::System.AttributeUsage(
        global::System.AttributeTargets.Assembly |
        global::System.AttributeTargets.Class |
        global::System.AttributeTargets.Struct |
        global::System.AttributeTargets.Constructor |
        global::System.AttributeTargets.Method |
        global::System.AttributeTargets.Property |
        global::System.AttributeTargets.Event,
        Inherited = false, AllowMultiple = false)]
    internal sealed class ExcludeFromCodeCoverageAttribute : global::System.Attribute { }
}
#endif