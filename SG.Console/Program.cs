namespace SourceGenerators;

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

internal static partial class Metadata
{
    internal static partial bool IsContiguous<TEnum>(this TEnum value) where TEnum : Enum;
}

internal static partial class Program
{
    private static void Main(string[] arguments)
    {
        try
        {
            Console.WriteLine($"SimpleEnum: {default(SimpleEnum).IsContiguous()}");
            Console.WriteLine($"ComplexEnum: {default(ComplexEnum).IsContiguous()}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex}");
        }
    }
}

