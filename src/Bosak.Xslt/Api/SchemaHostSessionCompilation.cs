// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 01 November 2026
// PURPOSE              : Compiles bounded private host sessions before binding borrowed provider capabilities.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 01-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2 | 01-11-2026     | Refuse composite XSLT until source-role membership is implemented. |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3 | 01-11-2026     | Enable composite transforms only through exact match/global source admission. |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Immutable;
using System.Globalization;
using System.Text.Json;
using System.Xml.Linq;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Providers.Xml;
using Bosak.XQuery.Api;
namespace Bosak.Xslt.Api;

/// <summary>Private source factories for a closed, explicitly bounded compilation/binding profile.</summary>
internal static class SchemaHostSessionCompilation
{
    internal static bool TryCompileXPath(string expression, CompileOptions options, ISchemaSessionProvider provider,
        int snapshots, int values, out SchemaQuerySession? session, out string? unsupported, CancellationToken cancellationToken = default)
    {
        SchemaProviderSessionBinding.Validate(provider, snapshots, values, cancellationToken);
        ArgumentNullException.ThrowIfNull(options); session = null; unsupported = null;
        if (options.Compatibility != XPathCompatibility.XPath31)
        { unsupported = "This factory captures XPath 3.1 only."; return false; }
        if (!SchemaSessionCompilation.TryCaptureCompilation(expression, options, out var captured, out var evaluate, out unsupported)) return false;
        var environment = new SchemaQueryEnvironment(captured!.Namespaces, JsonSerializer.Serialize(new
        { format = SchemaQueryEnvironment.DescriptorFormat, host = "XPath", profile = SchemaProviderSessionBinding.Profile, captured = captured.Fingerprint, engine = typeof(XPath31Expression).Assembly.GetName().Version!.ToString() }));
        session = SchemaProviderSessionBinding.Create(environment, evaluate!, provider, snapshots, values, cancellationToken); return true;
    }

    internal static bool TryCompileXQuery(string query, XQueryCompiler compiler, bool xml11LineEndings, ISchemaSessionProvider provider,
        int snapshots, int values, out SchemaQuerySession? session, out string? unsupported, CancellationToken cancellationToken = default)
    {
        SchemaProviderSessionBinding.Validate(provider, snapshots, values, cancellationToken);
        ArgumentNullException.ThrowIfNull(compiler); session = null;
        if (!compiler.TryCaptureSchemaCompilation(query, xml11LineEndings, out var executable, out var environment, out unsupported)) return false;
        session = SchemaProviderSessionBinding.Create(environment!, SchemaHostExecution.BindXQuery(executable!), provider, snapshots, values, cancellationToken); return true;
    }

    internal static bool TryCompileXslt(string source, XsltCompiler compiler, string? baseUri,
        IXdmNode? match, IXdmNode? global, string? initialTemplate, Func<IXdmNode, bool> ownsNode,
        ISchemaSessionProvider provider, int snapshots, int values, out SchemaQuerySession? session, out string? unsupported,
        CancellationToken cancellationToken = default)
    {
        SchemaProviderSessionBinding.Validate(provider, snapshots, values, cancellationToken);
        ArgumentNullException.ThrowIfNull(compiler); ArgumentNullException.ThrowIfNull(ownsNode); session = null; unsupported = null;
        if (compiler.UriResolver is not null || compiler.ResourcePolicy is not null || compiler.MessageListener is not null
            || compiler.SchemaAware || compiler.SchemaResolver is not null || compiler.SchemaSet is not null || compiler.EnvironmentSchemaSet is not null
            || compiler.ProcessorProfile != XsltProcessorProfile.Legacy)
        { unsupported = "Mutable resolver/schema/listener and nonlegacy processor profiles need a separate capture contract."; return false; }
        var parameters = compiler.StaticParameters;
        if (parameters is not null && (parameters.GetType() != typeof(Dictionary<(string, string), XdmValue>)
            || !ReferenceEquals(parameters.Comparer, EqualityComparer<(string, string)>.Default)))
        { unsupported = "Custom static-parameter collection semantics are not captured."; return false; }
        var copied = parameters is null ? null : new Dictionary<(string, string), XdmValue>(parameters);
        if (copied is not null && copied.Values.Any(v => !PlainStatic(v)))
        { unsupported = "Only plain primitive static parameter values are captured."; return false; }
        var document = Xml11Loader.Parse(source, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo | LoadOptions.SetBaseUri, baseUri ?? "");
        XNamespace xsl = "http://www.w3.org/1999/XSL/Transform";
        if (document.Root is null || (document.Root.Name != xsl + "stylesheet" && document.Root.Name != xsl + "transform"))
        { unsupported = "This captured profile requires a principal stylesheet or transform element."; return false; }
        if (document.Descendants().Any(e => e.Name.Namespace == xsl && e.Name.LocalName is "include" or "import" or "use-package" or "import-schema")
            || document.Descendants().Attributes().Any(a => !a.IsNamespaceDeclaration && (a.Name.LocalName.StartsWith("_", StringComparison.Ordinal)
                || a.Name.LocalName == "use-when" && (a.Name.Namespace == xsl || a.Parent!.Name.Namespace == xsl))))
        { unsupported = "External modules/schema and conditional/shadow preprocessing need a separate captured profile."; return false; }
        var policy = SchemaProviderSessionBinding.ClosedResources();
        var frozen = new XsltCompiler
        {
            ProcessorProfile = compiler.ProcessorProfile, StaticParameters = copied, ResourcePolicy = policy,
            UriResolver = new ClosedResolver(), TreatRecoverableAmbiguousMatchAsError = compiler.TreatRecoverableAmbiguousMatchAsError,
            PackageVersionResolutionStrategy = compiler.PackageVersionResolutionStrategy
        };
        XsltExecutable executable;
        try { executable = frozen.Compile(document, baseUri); }
        catch (ClosedCaptureException error) { unsupported = error.Message; return false; }
        catch (Exception) when (policy.Receipts.Count != 0) { unsupported = "Static resource acquisition is outside the captured profile."; return false; }
        if (policy.Receipts.Count != 0) { unsupported = "Static resource acquisition is outside the captured profile."; return false; }
        var statics = executable.CaptureSchemaStaticValues();
        if (statics.Values.Any(v => !PlainStatic(v))) { unsupported = "Effective static values require an unsupported captured representation."; return false; }
        var namespaces = document.Root!.Attributes().Where(a => a.IsNamespaceDeclaration)
            .ToImmutableDictionary(a => a.Name.LocalName == "xmlns" ? "" : a.Name.LocalName, a => a.Value, StringComparer.Ordinal);
        var environment = new SchemaQueryEnvironment(namespaces, JsonSerializer.Serialize(new
        {
            format = SchemaQueryEnvironment.DescriptorFormat, host = "XSLT", profile = SchemaProviderSessionBinding.Profile,
            engine = typeof(XsltExecutable).Assembly.GetName().Version!.ToString(), source, baseUri,
            processor = (int)frozen.ProcessorProfile, frozen.TreatRecoverableAmbiguousMatchAsError, strategy = (int)frozen.PackageVersionResolutionStrategy,
            supplied = Describe(copied), effective = Describe(statics)
        }));
        session = SchemaProviderSessionBinding.Create(environment, SchemaHostExecution.BindXslt(executable, match, global, initialTemplate, ownsNode),
            provider, snapshots, values, cancellationToken, compositeEvaluation: false, compositeAdmission: SchemaHostExecution.BindXsltAdmission(match, global)); return true;
    }

    private static bool PlainStatic(XdmValue value) => XdmValue.SchemaTransportMarker(value) is null
        && value.Kind is XdmValueKind.String or XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Double or XdmValueKind.Float or XdmValueKind.Boolean;
    private static object[]? Describe(IReadOnlyDictionary<(string LocalName, string NamespaceUri), XdmValue>? values)
        => values?.OrderBy(p => p.Key.NamespaceUri, StringComparer.Ordinal).ThenBy(p => p.Key.LocalName, StringComparer.Ordinal).Select(p => (object)new
        {
            p.Key.LocalName, p.Key.NamespaceUri, kind = (int)p.Value.Kind, p.Value.SchemaTypeName, p.Value.UserSchemaTypeName,
            payload = p.Value.Kind switch
            {
                XdmValueKind.Integer => p.Value.IntegerValue.ToString(CultureInfo.InvariantCulture),
                XdmValueKind.Decimal => string.Join(",", decimal.GetBits(p.Value.DecimalValue).Select(v => v.ToString(CultureInfo.InvariantCulture))),
                XdmValueKind.Double or XdmValueKind.Float => BitConverter.DoubleToInt64Bits(p.Value.DoubleValue).ToString(CultureInfo.InvariantCulture),
                XdmValueKind.Boolean => p.Value.BooleanValue ? "true" : "false", _ => p.Value.StringValue
            }
        }).ToArray();
    private sealed class ClosedCaptureException(string message) : NotSupportedException(message);
    private sealed class ClosedResolver : IXsltUriResolver
    {
        XDocument IXsltUriResolver.Resolve(string href, string? baseUri) => throw new ClosedCaptureException("Uncaptured stylesheet resource refused.");
    }
}
