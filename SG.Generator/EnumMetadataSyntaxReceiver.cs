namespace SG.Generator
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System.Collections.Generic;

    internal class EnumMetadataSyntaxReceiver : ISyntaxReceiver
    {
        internal readonly List<ExpressionSyntax> enumerations = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            // Collect all types passed to 'Metadata.IsValid(<>)'
            {
                if ((syntaxNode is InvocationExpressionSyntax invocationExpression) &&
                    (invocationExpression.Expression is MemberAccessExpressionSyntax memberAccess) &&
                    (memberAccess.Expression is IdentifierNameSyntax classIdentifier))
                {
                    if (memberAccess.Name.Identifier.Text == "IsValid")
                    {
                        this.enumerations.Add(invocationExpression.ArgumentList.Arguments[0].Expression);
                    }
                }
            }

            // Collect all types passed to '<>.IsValid()'
            {
                if ((syntaxNode is InvocationExpressionSyntax invocationExpression) &&
                    (invocationExpression.Expression is MemberAccessExpressionSyntax memberAccess) &&
                    (memberAccess.Expression is MemberAccessExpressionSyntax expressionMemberAccess))
                {
                    if (memberAccess.Name.Identifier.Text == "IsValid")
                    {
                        this.enumerations.Add(expressionMemberAccess);
                    }
                }
            }
        }
    }
}
