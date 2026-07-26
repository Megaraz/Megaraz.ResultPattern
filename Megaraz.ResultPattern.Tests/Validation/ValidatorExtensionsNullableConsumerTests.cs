using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Megaraz.ResultPattern.Tests.Validation;

public class ValidatorExtensionsNullableConsumerTests
{
    [Fact]
    public void BooleanValidatorsPreserveThePublishedNonNullableOutContracts()
    {
        const string source = """
            #nullable enable
            using Megaraz.ResultPattern;

            public static class Consumer
            {
                public static void Validate(string? value, ErrorContext context)
                {
                    if (value.IsNullOrWhiteSpace(context, out var requiredError))
                    {
                        _ = requiredError.FieldName;
                    }

                    _ = requiredError.FieldName;

                    if (value.IsNullOrWhiteSpace("Value", context, out var labeledRequiredError))
                    {
                        _ = labeledRequiredError.FieldName;
                    }

                    _ = labeledRequiredError.FieldName;

                    if (value.DoesNotMatch(value, "Value", "Confirmation", context, out var mismatchError))
                    {
                        _ = mismatchError.FieldName;
                    }

                    _ = mismatchError.FieldName;
                }
            }
            """;

        var compilation = CSharpCompilation.Create(
            assemblyName: "NullableConsumer",
            syntaxTrees: [CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest))],
            references: GetMetadataReferences(),
            options: new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable));

        var diagnostics = compilation.GetDiagnostics();

        Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Id == "CS8602");
    }

    private static IEnumerable<MetadataReference> GetMetadataReferences()
    {
        var trustedPlatformAssemblies = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;
        var references = trustedPlatformAssemblies
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(ValidatorExtensions).Assembly.Location));

        return references;
    }
}
