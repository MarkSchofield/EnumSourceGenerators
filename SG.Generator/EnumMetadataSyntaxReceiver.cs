namespace SG.Generator
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System.Collections.Generic;

    internal class EnumMetadataSyntaxReceiver : ISyntaxReceiver
    {
        internal readonly List<ArgumentSyntax> enumerations = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            // Collect all types passed to 'Metadata.IsValid(<>)'
            if ((syntaxNode is InvocationExpressionSyntax invocationExpression) &&
                (invocationExpression.Expression is MemberAccessExpressionSyntax memberAccess) &&
                (memberAccess.Expression is IdentifierNameSyntax classIdentifier) &&
                (classIdentifier.Identifier.Text == "Metadata"))
            {
                if (memberAccess.Name.Identifier.Text == "IsValid")
                {
                    this.enumerations.Add(invocationExpression.ArgumentList.Arguments[0]);
                }
            }
        }
    }
}
