namespace SG.Generator
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    [Generator]
    public class EnumMetadataSourceGenerator : ISourceGenerator
    {
        private static readonly SyntaxTokenList InternalStaticTokenList = TokenList(
            Token(SyntaxKind.InternalKeyword),
            Token(SyntaxKind.StaticKeyword)
        );

        private static readonly SyntaxTokenList InternalStaticPartialTokenList = TokenList(
            Token(SyntaxKind.InternalKeyword),
            Token(SyntaxKind.StaticKeyword),
            Token(SyntaxKind.PartialKeyword)
        );

        private static readonly SyntaxTokenList ThisTokenList = TokenList(
            Token(SyntaxKind.ThisKeyword)
        );

        private static MethodDeclarationSyntax BuildIsValid(ITypeSymbol typeSymbol)
        {
            if ((typeSymbol.TypeKind != TypeKind.Enum) || (typeSymbol is not INamedTypeSymbol namedTypeSymbol))
            {
                throw new InvalidOperationException();
            }

            BlockSyntax methodBody;
            // This is wrong...
            QualifiedNameSyntax enumExpression = QualifiedName(IdentifierName(namedTypeSymbol.ContainingNamespace.Name), IdentifierName(namedTypeSymbol.Name));
            List<ExpressionSyntax> equalsExpressions = new();

            foreach (ISymbol symbol in namedTypeSymbol.GetMembers())
            {
                if (symbol.Kind != SymbolKind.Field)
                {
                    continue;
                }

                MemberAccessExpressionSyntax enumValue = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, enumExpression, IdentifierName(symbol.Name));
                BinaryExpressionSyntax equalsExpression = BinaryExpression(SyntaxKind.EqualsExpression, IdentifierName("value"), enumValue);
                equalsExpressions.Add(equalsExpression);
            }

            methodBody = Block(ReturnStatement(SyntaxBuilder.LogicalOr(equalsExpressions)));

            var parameter = Parameter(Identifier("value"))
                .WithType(enumExpression)
                .WithModifiers(ThisTokenList);

            return MethodDeclaration(
                attributeLists: default,
                modifiers: InternalStaticTokenList,
                returnType: PredefinedType(Token(SyntaxKind.BoolKeyword)),
                explicitInterfaceSpecifier: default,
                identifier: Identifier("IsValid"),
                typeParameterList: default,
                parameterList: ParameterList(
                        SeparatedList(new[] { parameter })
                        ),
                constraintClauses: default,
                body: methodBody,
                expressionBody: default,
                semicolonToken: default);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new EnumMetadataSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            EnumMetadataSyntaxReceiver syntaxReceiver = context.SyntaxReceiver as EnumMetadataSyntaxReceiver ?? throw new InvalidOperationException();

            // Build a HashSet of all of the TypeInfo's the need metadata emitting.
            HashSet<TypeInfo> enumerationTypeInfos = new();
            foreach ((SyntaxTree syntaxTree, IEnumerable<ExpressionSyntax> enumerations) in syntaxReceiver.enumerations.GroupBy(argumentSyntax => argumentSyntax.SyntaxTree))
            {
                SemanticModel semanticModel = context.Compilation.GetSemanticModel(syntaxTree);
                foreach (ExpressionSyntax enumeration in enumerations)
                {
                    enumerationTypeInfos.Add(semanticModel.GetTypeInfo(enumeration, context.CancellationToken));
                }
            }

            List<MethodDeclarationSyntax> typedIsValidMethods = new();
            foreach (TypeInfo enumerationTypeInfo in enumerationTypeInfos)
            {
                ITypeSymbol? typeSymbol = enumerationTypeInfo.Type;
                MethodDeclarationSyntax isValidMethod = BuildIsValid(typeSymbol!);

                typedIsValidMethods.Add(isValidMethod);
            }

            ClassDeclarationSyntax classDeclaration = ClassDeclaration("MetadataGenerated")
                .WithModifiers(InternalStaticPartialTokenList)
                .WithMembers(List<MemberDeclarationSyntax>(typedIsValidMethods))
                ;

            NamespaceDeclarationSyntax namespaceDeclaration = NamespaceDeclaration(IdentifierName("Enumeration"))
                .AddMembers(classDeclaration)
                ;

            CompilationUnitSyntax compilationUnit = CompilationUnit(
                externs: default,
                usings: default,
                attributeLists: default,
                members: SingletonList<MemberDeclarationSyntax>(namespaceDeclaration)
                );

            // TODO: Formatter? Do nothing?
            compilationUnit = compilationUnit.NormalizeWhitespace();

            context.AddSource("MetadataGenerated.cs", compilationUnit.GetText(Encoding.UTF8));
        }
    }
}
