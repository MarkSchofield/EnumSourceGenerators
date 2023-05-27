namespace SourceGenerators;

using System;
using Enumeration;

internal enum SimpleEnum
{
    One,
    Two,
    Three,
}

internal enum ComplexEnum
{
    One = 1,
    Two = 2,
    Four = 4,
}

internal static partial class Program
{
    private static void Main(string[] arguments)
    {
        try
        {
            Console.WriteLine($"IsValid(SimpleEnum.One) : {Metadata.IsValid(SimpleEnum.One)}");
            Console.WriteLine($"IsValid((SimpleEnum)12) : {Metadata.IsValid((SimpleEnum)12)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex}");
        }
    }
}

