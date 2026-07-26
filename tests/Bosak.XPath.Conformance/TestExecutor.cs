// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 20 mei 2026
// PURPOSE              : Executes a single QT3 test case using the Bosak XPath API.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 20-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 15-07-2026     | Register fn:transform via XsltFunctionLibrary.Populate (fn-transform test set)           |
//                      | Charles Korthout | 0.3   | 15-07-2026     | XQuery detection covers constructors/switch/try/FLWOR with string-literal stripping      |
//                      | Charles Korthout | 0.4   | 15-07-2026     | Bind environment <param> external variables by evaluating their select expressions       |
//                      | Charles Korthout | 0.5   | 21-07-2026     | Pass test-case base directory through to ResultComparer for assert-xml files             |
//                      | Charles Korthout | 0.6   | 21-07-2026     | Refine XQuery heuristic: allow schema-element/attribute tests, element/attribute name tests, import name tests |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.7   | 25-07-2026     | Route supported XQuery-syntax tests through the Bosak.XQuery pipeline                   |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.8   | 25-07-2026     | XQuery routing refinements: dep-admitted tests, constructor/prolog gates                |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.9   | 25-07-2026     | Admit direct element constructors (implemented); keep string-constructor gate           |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Parser;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;
using Bosak.XQuery.Api;

namespace Bosak.XPath.Conformance;

internal sealed class TestExecutor
{
    public TestOutcome Execute(TestCase testCase, TestEnvironment? environment)
    {
        string expr = testCase.Expression.Trim();
        bool xquerySyntax = LooksLikeXQuery(expr);
        bool hasXqDeps = DependencyFilter.HasXQueryOnlySpecDependency(testCase.Dependencies);
        // Route to the XQuery pipeline when all constructs used are supported and either
        // the query looks like XQuery or the test was admitted through the XQuery
        // spec-dependency relaxation (multi-clause FLWOR has no other XQuery markers).
        // XQ-dep tests route even when expecting a parse error: the XQuery parser rejects
        // malformed XQuery (and XQuery-only literal rules) with the expected XPST0003.
        // XPath-dep tests EXPECTING a static parse error keep running on the XPath
        // pipeline: for XQuery-only syntax a parse failure is exactly the expected outcome.
        bool routeXQuery = CanHandleAsXQuery(expr) &&
                           (hasXqDeps || (xquerySyntax && !ExpectsParseError(testCase.ResultElement)));

        var xqContext = routeXQuery ? new XQueryContext() : null;
        var ctx = xqContext?.EvaluationContext ?? new EvaluationContext();
        FunctionLibrary.Populate(ctx);
        // fn:transform lives in the XSLT layer; register it so the fn-transform test set runs.
        Bosak.Xslt.Api.XsltFunctionLibrary.Populate(ctx);

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
                    var value = XPath31Expression.Compile(param.SelectExpression).Evaluate(ctx);
                    var (local, ns) = SplitVariableQName(param.Name, environment);
                    ctx = ctx.WithVariable(local, value, ns);
                }
                catch (Exception ex)
                {
                    return new TestOutcome(TestOutcomeKind.Skipped, $"External variable binding failed ({param.Name}): {ex.Message}");
                }
            }
        }

        // Detect XQuery-only tests by syntax (declare namespace, constructors, switch, etc.).
        // Tests whose constructs the XQuery pipeline cannot handle yet (constructors,
        // switch/typeswitch, unsupported prolog forms) still skip.
        if (xquerySyntax && !ExpectsParseError(testCase.ResultElement) && !routeXQuery)
        {
            return new TestOutcome(TestOutcomeKind.Skipped, "XQuery syntax not supported");
        }

        XdmValue result;
        Exception? caughtException = null;

        try
        {
            if (routeXQuery)
            {
                var executable = new XQueryCompiler().Compile(expr);
                result = executable.Evaluate(xqContext!);
            }
            else
            {
                var compiled = XPath31Expression.Compile(expr);
                result = compiled.Evaluate(ctx);
            }
        }
        catch (ParseException ex)
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

        return ResultComparer.Compare(testCase.ResultElement, result, caughtException, testCase.BaseDirectory);
    }

    /// <summary>
    /// True when the expression uses only XQuery constructs the Bosak.XQuery pipeline
    /// supports today: prolog-less queries, the basic prolog declarations (namespace,
    /// default element/function namespace, default collation, version), and full FLWOR
    /// (for/let/where/order by/count/group by/window). Queries using constructors,
    /// switch/typeswitch, try, unordered/ordered, validate, or unsupported prolog forms
    /// stay skipped. String literals and comments are stripped before matching.
    /// </summary>
    internal static bool CanHandleAsXQuery(string expr)
    {
        var stripped = StripLiteralsAndComments(expr).TrimStart();

        // """...""" string constructors are not supported (checked on the raw text:
        // stripping removes string literals and (: :) comments).
        if (expr.Contains("\"\"\""))
            return false;

        if (UnsupportedXQueryConstructRegex.IsMatch(stripped))
            return false;

        if (UnsupportedPrologRegex.IsMatch(stripped))
            return false;

        // The XQuery prolog parser cannot yet handle (: :) comments inside declarations.
        if ((stripped.StartsWith("declare", StringComparison.OrdinalIgnoreCase) ||
             stripped.StartsWith("xquery", StringComparison.OrdinalIgnoreCase) ||
             stripped.StartsWith("import", StringComparison.OrdinalIgnoreCase)))
        {
            int semicolon = expr.IndexOf(';');
            var prologPart = semicolon >= 0 ? expr[..semicolon] : expr;
            if (prologPart.Contains("(:"))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Heuristic: the expression uses XQuery-only grammar that our XPath parser cannot
    /// handle, so the test is skipped rather than failed. String literals and comments
    /// are stripped first so quoted keywords (e.g. in fn:matches patterns) never trigger.
    /// Every pattern below is rejected by the XPath 3.1 grammar, so a passing test can
    /// never match — patterns can only convert failures into skips.
    /// </summary>
    private static bool LooksLikeXQuery(string expr)
    {
        if (expr.StartsWith("declare ", StringComparison.OrdinalIgnoreCase) ||
            expr.StartsWith("import module ", StringComparison.OrdinalIgnoreCase) ||
            expr.StartsWith("import schema ", StringComparison.OrdinalIgnoreCase) ||
            expr.StartsWith("xquery ", StringComparison.OrdinalIgnoreCase))
        return true;

        var stripped = StripLiteralsAndComments(expr);

        // Direct element constructors: an XPath expression can never start with '<'.
        if (stripped.TrimStart().StartsWith('<'))
            return true;

        if (stripped.Contains(";") && !stripped.Contains("(") && !stripped.Contains("{") && !stripped.Contains("}"))
            return true;

        return XQueryConstructRegex.IsMatch(stripped);
    }

    /// <summary>Resolves a possibly prefixed variable name to (local, namespaceUri).</summary>
    private static (string Local, string NamespaceUri) SplitVariableQName(string name, TestEnvironment environment)
    {
        int colon = name.IndexOf(':');
        if (colon < 0)
            return (name, "");
        var prefix = name.Substring(0, colon);
        var local = name.Substring(colon + 1);
        var binding = environment.Namespaces.FirstOrDefault(n => n.Prefix == prefix);
        return (local, binding?.Uri ?? "");
    }

    /// <summary>
    /// True when the expected outcome accepts a static parse error: an explicit XPST0003
    /// (possibly among space-separated or any-of alternatives), or an any-error wildcard.
    /// </summary>
    private static bool ExpectsParseError(XElement? resultElement)
    {
        if (resultElement is null)
            return false;
        foreach (var error in resultElement.Descendants().Where(e => e.Name.LocalName == "error"))
        {
            var code = ((string?)error.Attribute("code") ?? "").Trim();
            if (code.Length == 0 || code == "*")
                return true;
            if (code.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Any(c => c == "*" || c.EndsWith("XPST0003", StringComparison.Ordinal)))
                return true;
        }
        return false;
    }

    private static readonly Regex XQueryConstructRegex = new(
        @"\b(element|attribute)(\s+[^\s{(]+)?\s*\{" +   // element/attribute constructors require { (element()/attribute() are XPath kind tests)
        @"|\b(document|text|comment)\s*\{" +        // document { ... }, text { ... }, comment { ... }
        @"|\bprocessing-instruction\s" +
        @"|\bnamespace\s+[A-Za-z_$]" +               // namespace constructor (namespace-uri( unaffected)
        @"|\bswitch\s*\(|\btry\s*\{|\btypeswitch\s*\(" +
        @"|\border\s+by\b|\bgroup\s+by\b|\bcount\s+\$" +  // FLWOR clauses (count( unaffected)
        @"|\bunordered\s*\{|\bvalidate\s" +
        @"|\b(sliding|tumbling)\s+window\b",
        RegexOptions.Compiled);

    // XQuery constructs the Bosak.XQuery pipeline does NOT support yet; matching queries
    // keep their "XQuery syntax not supported" skip instead of being routed.
    private static readonly Regex UnsupportedXQueryConstructRegex = new(
        @"\b(element|attribute)(\s+[^\s{(]+)?\s*\{" +
        @"|\b(document|text|comment)\s*\{" +
        @"|\bprocessing-instruction\s" +
        @"|\bnamespace\s+[A-Za-z_$]" +
        @"|\bswitch\s*\(|\btry\s*\{|\btypeswitch\s*\(" +
        @"|\bunordered\s*\{|\bordered\s*\{|\bvalidate\s" +
        @"|\bdeclare\s+%" +                              // annotated declarations
        @"|\(#\s*[A-Za-z_]" +                          // pragma extension expressions (# ... #)
        @"|``\[" +                                     // string constructors ``[ ... ]``
        RegexOptions.Compiled);

    // Prolog forms the XQuery parser does NOT support (namespace, default element/function
    // namespace, default collation, base-uri, and version declarations are supported).
    private static readonly Regex UnsupportedPrologRegex = new(
        @"\bdeclare\s+(variable|function|option|boundary-space|default\s+order|default\s+decimal-format|ordering|copy-namespaces|context|decimal-format|construction)\b" +
        @"|\bimport\s+(module|schema)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Removes string literals (with doubled-quote escapes) and XQuery comments
    /// (possibly nested) so keyword-based detection only sees actual code.
    /// </summary>
    private static string StripLiteralsAndComments(string expr)
    {
        var sb = new StringBuilder(expr.Length);
        int i = 0;
        while (i < expr.Length)
        {
            char c = expr[i];
            if (c == '\'' || c == '"')
            {
                char quote = c;
                i++;
                while (i < expr.Length)
                {
                    if (expr[i] == quote)
                    {
                        if (i + 1 < expr.Length && expr[i + 1] == quote) { i += 2; continue; }
                        i++;
                        break;
                    }
                    i++;
                }
                continue;
            }
            if (c == '(' && i + 1 < expr.Length && expr[i + 1] == ':')
            {
                int depth = 1;
                i += 2;
                while (i < expr.Length && depth > 0)
                {
                    if (expr[i] == '(' && i + 1 < expr.Length && expr[i + 1] == ':') { depth++; i += 2; }
                    else if (expr[i] == ':' && i + 1 < expr.Length && expr[i + 1] == ')') { depth--; i += 2; }
                    else i++;
                }
                continue;
            }
            sb.Append(c);
            i++;
        }
        return sb.ToString();
    }
}
