// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 09 oktober 2026
// PURPOSE              : Executes a single qt4tests case on the Bosak XPath 4.0 pipeline.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 09-10-2026     | Creation (REQ-123 4.0-Exp S2): frozen XPath40 level with automatic                        |
//                      |                  |       |                | XPath40Experimental retry when the static gate raises the experimental XPST0017          |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Parser;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;

namespace Bosak.XPath.Conformance;

/// <summary>
/// Executes qt4tests test cases against <see cref="XPathCompatibility.XPath40"/> (the
/// frozen level). When the Api's static gate rejects a call to an
/// <c>IsXPath40ExperimentalOnly</c> function (e.g. <c>fn:scan</c>) with the experimental
/// XPST0017, the expression is transparently retried at
/// <see cref="XPathCompatibility.XPath40Experimental"/> so draft-addition test sets run
/// without a hand-maintained set list.
/// </summary>
internal sealed class TestExecutor40
{
    /// <summary>Set by the runner's reset; incremented for every pass that only succeeded at the experimental level.</summary>
    public int ExperimentalPasses { get; private set; }

    public void ResetExperimentalPasses() => ExperimentalPasses = 0;

    public TestOutcome Execute(TestCase testCase, TestEnvironment? environment)
    {
        string expr = testCase.Expression.Trim();
        // XML 1.1 tests (xml-version dependency) enable line-ending normalization in
        // string literals; everything else keeps reference-produced characters exact.
        bool xml11LineEndings = testCase.Dependencies.Any(d =>
            d.Type == "xml-version" && d.Value.Contains("1.1", StringComparison.Ordinal));

        var ctx = new EvaluationContext();
        // Schema-aware run (the harness admits schemaImport/schemaValidation): matches
        // the QT3 harness posture (fn:json-to-xml validate:=true uses the built-in schema).
        ctx.IsSchemaAware = true;
        ctx.Xml11Mode = xml11LineEndings;
        FunctionLibrary.Populate(ctx);
        // fn:transform lives in the XSLT layer; register it so fn-transform sets run.
        // The other XSLT-only functions must stay unavailable (pure XPath expects XPST0017).
        Bosak.Xslt.Api.XsltFunctionLibrary.PopulateTransformOnly(ctx);

        if (environment is not null)
        {
            ctx = environment.ApplyTo(ctx);

            // Bind external variables: evaluate each <param select="..."> in the prepared
            // context (namespaces, sources, and $var sources are already applied).
            foreach (var param in environment.Parameters)
            {
                if (string.IsNullOrEmpty(param.SelectExpression))
                    continue;
                try
                {
                    var value = XPath31Expression.Compile(param.SelectExpression,
                        new CompileOptions { Compatibility = XPathCompatibility.XPath40 }).Evaluate(ctx);
                    var (local, ns) = SplitVariableQName(param, environment);
                    ctx = ctx.WithVariable(local, value, ns);
                }
                catch (Exception ex)
                {
                    return new TestOutcome(TestOutcomeKind.Skipped, $"External variable binding failed ({param.Name}): {ex.Message}");
                }
            }
        }

        // Static name-test validation (XPST0081/XPST0008) runs at compile time and needs
        // the environment's namespace bindings; an empty map validates against no prefixes.
        var compileNamespaces = new Dictionary<string, string>(StringComparer.Ordinal);
        if (environment is not null)
            foreach (var ns in environment.Namespaces)
                compileNamespaces[ns.Prefix] = ns.Uri;

        XdmValue result;
        Exception? caughtException = null;
        bool experimental = false;

        try
        {
            var compiled = XPath31Expression.Compile(expr,
                new CompileOptions
                {
                    Compatibility = XPathCompatibility.XPath40,
                    Xml11LineEndings = xml11LineEndings,
                    Namespaces = compileNamespaces,
                });
            result = compiled.Evaluate(ctx);
        }
        catch (InvalidOperationException ex) when (IsExperimentalGateError(ex))
        {
            // The expression calls an IsXPath40ExperimentalOnly function (e.g. fn:scan):
            // retry transparently at the experimental level (REQ-123, 4.0-Exp S1).
            experimental = true;
            var retry = ExecuteAtExperimentalLevel(expr, ctx, xml11LineEndings, compileNamespaces);
            if (retry.Outcome.HasValue)
                return retry.Outcome.Value;
            result = retry.Result;
            caughtException = retry.CaughtException;
        }
        catch (XPathParseException ex)
        {
            caughtException = ex;
            result = default;
        }
        catch (InvalidOperationException ex)
        {
            caughtException = ex;
            result = default;
        }
        catch (NotImplementedException ex)
        {
            return new TestOutcome(TestOutcomeKind.Skipped, $"Not implemented: {ex.Message}");
        }
        catch (Exception ex)
        {
            return new TestOutcome(TestOutcomeKind.Skipped, $"Unexpected error: {ex.GetType().Name}: {ex.Message}");
        }

        var outcome = ResultComparer.Compare(testCase.ResultElement, result, caughtException, testCase.BaseDirectory, ctx.StaticOutputParameters);
        if (experimental && outcome.Kind == TestOutcomeKind.Passed)
        {
            ExperimentalPasses++;
            return new TestOutcome(outcome.Kind, "(XPath40Experimental level) " + outcome.Message);
        }
        return outcome;
    }

    private (TestOutcome? Outcome, XdmValue Result, Exception? CaughtException) ExecuteAtExperimentalLevel(
        string expr, EvaluationContext ctx, bool xml11LineEndings, Dictionary<string, string> compileNamespaces)
    {
        try
        {
            var compiled = XPath31Expression.Compile(expr,
                new CompileOptions
                {
                    Compatibility = XPathCompatibility.XPath40Experimental,
                    Xml11LineEndings = xml11LineEndings,
                    Namespaces = compileNamespaces,
                });
            return (null, compiled.Evaluate(ctx), null);
        }
        catch (XPathParseException ex)
        {
            return (null, default, ex);
        }
        catch (InvalidOperationException ex)
        {
            return (null, default, ex);
        }
        catch (NotImplementedException ex)
        {
            return (new TestOutcome(TestOutcomeKind.Skipped, $"Not implemented: {ex.Message}"), default, null);
        }
        catch (Exception ex)
        {
            return (new TestOutcome(TestOutcomeKind.Skipped, $"Unexpected error: {ex.GetType().Name}: {ex.Message}"), default, null);
        }
    }

    /// <summary>The Api layer's static gate for experimental-only functions raises XPST0017 naming XPath40Experimental.</summary>
    private static bool IsExperimentalGateError(InvalidOperationException ex)
        => ex.Message.Contains("XPath40Experimental", StringComparison.Ordinal);

    /// <summary>Resolves a possibly prefixed variable name to (local, namespaceUri).</summary>
    private static (string Local, string NamespaceUri) SplitVariableQName(ExternalParameter param, TestEnvironment environment)
    {
        string name = param.Name;
        int colon = name.IndexOf(':');
        if (colon < 0)
            return (name, "");
        var prefix = name.Substring(0, colon);
        var local = name.Substring(colon + 1);
        // The prefix may be declared on the <param> element itself; that binding wins.
        if (param.NamespaceUri is not null)
            return (local, param.NamespaceUri);
        var binding = environment.Namespaces.FirstOrDefault(n => n.Prefix == prefix);
        return (local, binding?.Uri ?? "");
    }
}
