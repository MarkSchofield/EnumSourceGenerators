namespace SG.Generator;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

internal class SyntaxBuilder
{
    private readonly IDictionary<Type, TypeSyntax> predefinedTypes = new Dictionary<Type, TypeSyntax>()
    {
        { typeof(bool), PredefinedType(Token(SyntaxKind.BoolKeyword)) },
    };

    public TypeSyntax GetTypeSyntax(Type type) => this.predefinedTypes[type];
}
