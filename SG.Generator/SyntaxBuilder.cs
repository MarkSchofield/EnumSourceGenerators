namespace SG.Generator
{
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System.Collections.Generic;

    using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    internal static class SyntaxBuilder
    {
        internal static ExpressionSyntax LogicalOr(IList<ExpressionSyntax> expressions)
        {
            if (expressions.Count == 0)
            {
                // No enum values: 'false'
                return LiteralExpression(SyntaxKind.FalseLiteralExpression);
            }

            if (expressions.Count == 1)
            {
                // Only a single value: <<expressions[0]>>
                return expressions[0];
            }

            // Multiple enum values: <<expressions[0]>> || <<expressions[1]>> || ...
            BinaryExpressionSyntax resultExpression = BinaryExpression(SyntaxKind.LogicalOrExpression, expressions[0], expressions[1]);

            for (int index = 2; index < expressions.Count; index++)
            {
                resultExpression = BinaryExpression(SyntaxKind.LogicalOrExpression, resultExpression, expressions[index]);
            }

            return resultExpression;
        }
    }
}
