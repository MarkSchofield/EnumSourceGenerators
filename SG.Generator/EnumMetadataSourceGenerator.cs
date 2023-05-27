namespace SG.Generator
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;
    using System.Text;

    [Generator]
    public class EnumMetadataSourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new EnumMetadataSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            EnumMetadataSyntaxReceiver syntaxReceiver = (EnumMetadataSyntaxReceiver)context.SyntaxReceiver;
            var entryPoint = context.Compilation.GetEntryPoint(context.CancellationToken);

            context.AddSource("Metadata.cs", SourceText.From(@"
namespace SourceGenerators;

internal static partial class Metadata
{
    internal static partial bool IsContiguous<TEnum>(this TEnum value) where TEnum : Enum
    {
        if (value is ComplexEnum complexEnum)
        {
            return false;
        }

        if (value is SimpleEnum simpleEnum)
        {
            return true;
        }

        return false;
    }
}
", Encoding.UTF8));
        }
    }
}
