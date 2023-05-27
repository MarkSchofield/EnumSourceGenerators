namespace SG.Generator
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System.Collections.Generic;

    internal class EnumMetadataSyntaxReceiver : ISyntaxReceiver
    {
#pragma warning disable CA1823 // Avoid unused private fields
        private List<SimpleNameSyntax> enumerations = new List<SimpleNameSyntax>();
#pragma warning restore CA1823 // Avoid unused private fields

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            // Business logic to decide what we're interested in goes here
        }
    }
}
