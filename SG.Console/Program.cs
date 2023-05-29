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
            Console.WriteLine($"IsValid(SimpleEnum.One) : {SimpleEnum.One.IsValid()}");
            Console.WriteLine($"IsValid((SimpleEnum)12) : {((SimpleEnum)12).IsValid()}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex}");
        }
    }
}

