namespace SG.Generator
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    [Generator]
    public class EnumMetadataSourceGenerator : ISourceGenerator
    {
        private readonly SyntaxBuilder syntaxBuilder = new();

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new EnumMetadataSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            EnumMetadataSyntaxReceiver syntaxReceiver = context.SyntaxReceiver as EnumMetadataSyntaxReceiver ?? throw new InvalidOperationException();

            // Build a HashSet of all of the TypeInfo's the need metadata emitting.
            HashSet<TypeInfo> enumerationTypeInfos = new();
            foreach ((SyntaxTree syntaxTree, IEnumerable<ArgumentSyntax> enumerations) in syntaxReceiver.enumerations.GroupBy(argumentSyntax => argumentSyntax.SyntaxTree))
            {
                SemanticModel semanticModel = context.Compilation.GetSemanticModel(syntaxTree);
                foreach (ArgumentSyntax enumeration in enumerations)
                {
                    enumerationTypeInfos.Add(semanticModel.GetTypeInfo(enumeration.Expression, context.CancellationToken));
                }
            }

            List<MethodDeclarationSyntax> typedIsValidMethods = new();
            // Build the method:
            //
            //     internal static bool IsValid(<<enum>> value)
            //     {
            //         return value == <<enum>>.<<enum member 1>> || value == <<enum>>.<<enum member 2>> || value == <<enum>>.<<enum member 3>>;
            //     }
            foreach (TypeInfo enumerationTypeInfo in enumerationTypeInfos)
            {
                ITypeSymbol? typeSymbol = enumerationTypeInfo.Type;
                if ((typeSymbol == null) || (typeSymbol.TypeKind != TypeKind.Enum) || (typeSymbol is not INamedTypeSymbol namedTypeSymbol))
                {
                    throw new InvalidOperationException();
                }

                BlockSyntax methodBody;
                // This is wrong...
                QualifiedNameSyntax enumExpression = QualifiedName(IdentifierName(namedTypeSymbol.ContainingNamespace.Name), IdentifierName(namedTypeSymbol.Name));
                List<BinaryExpressionSyntax> equalsExpressions = new();

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

                if (equalsExpressions.Count == 0)
                {
                    // No enum values:
                    //  return false;
                    methodBody = Block(ReturnStatement(LiteralExpression(SyntaxKind.FalseLiteralExpression)));
                }
                else if (equalsExpressions.Count == 1)
                {
                    // Only a single enum value:
                    //  return value == <<enum>>.<<enum member 1>>;
                    methodBody = Block(ReturnStatement(equalsExpressions[0]));
                }
                else
                {
                    // Multiple enum values:
                    //  return value == <<enum>>.<<enum member 1>> || value == <<enum>>.<<enum member 2>> || value == <<enum>>.<<enum member 3>>;
                    BinaryExpressionSyntax equalityExpression = BinaryExpression(SyntaxKind.LogicalOrExpression, equalsExpressions[0], equalsExpressions[1]);

                    for (int index = 2; index < equalsExpressions.Count; index++)
                    {
                        equalityExpression = BinaryExpression(SyntaxKind.LogicalOrExpression, equalityExpression, equalsExpressions[index]);
                    }

                    methodBody = Block(ReturnStatement(equalityExpression));
                }

                MethodDeclarationSyntax isValidMethod = MethodDeclaration(
                    attributeLists: default,
                    modifiers: TokenList(
                        Token(SyntaxKind.InternalKeyword),
                        Token(SyntaxKind.StaticKeyword)
                        ),
                    returnType: this.syntaxBuilder.GetTypeSyntax(typeof(bool)),
                    explicitInterfaceSpecifier: default,
                    identifier: Identifier("IsValid"),
                    typeParameterList: default,
                    parameterList: ParameterList(
                            SeparatedList(new[] { Parameter(Identifier("value")).WithType(enumExpression) })
                            ),
                    constraintClauses: default,
                    body: methodBody,
                    semicolonToken: default);

                typedIsValidMethods.Add(isValidMethod);
            }

            ClassDeclarationSyntax classDeclaration = ClassDeclaration("Metadata")
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.InternalKeyword),
                        Token(SyntaxKind.StaticKeyword),
                        Token(SyntaxKind.PartialKeyword)
                        )
                    )
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
            context.AddSource("Metadata.cs", SourceText.From(@"
namespace Enumeration
{
    internal static partial class Metadata
    {
        internal static partial bool IsValid<TEnum>(this TEnum value) where TEnum : Enum
        {
            if (value is SourceGenerators.SimpleEnum simpleEnum)
            {
                return IsValid(simpleEnum);
            }

            return false;
        }
    }
}
", Encoding.UTF8));
        }
    }
}
