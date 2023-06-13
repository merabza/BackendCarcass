using System.Reflection;

namespace ServerCarcassMini;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}