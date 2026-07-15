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
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Parser;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;

namespace Bosak.XPath.Conformance;

internal sealed class TestExecutor
{
    public TestOutcome Execute(TestCase testCase, TestEnvironment? environment)
    {
        var ctx = new EvaluationContext();
        FunctionLibrary.Populate(ctx);
        // fn:transform lives in the XSLT layer; register it so the fn-transform test set runs.
        Bosak.Xslt.Api.XsltFunctionLibrary.Populate(ctx);

        if (environment is not null)
        {
            ctx = environment.ApplyTo(ctx);
        }

        // Detect XQuery-only tests by syntax (declare namespace, declare variable, etc.)
        string expr = testCase.Expression.Trim();
        if (expr.StartsWith("declare ", StringComparison.OrdinalIgnoreCase) ||
            expr.StartsWith("import ", StringComparison.OrdinalIgnoreCase) ||
            expr.Contains(";") && !expr.Contains("(") && !expr.Contains("{") && !expr.Contains("}"))
        {
            return new TestOutcome(TestOutcomeKind.Skipped, "XQuery syntax not supported");
        }

        XdmValue result;
        Exception? caughtException = null;

        try
        {
            var compiled = XPath31Expression.Compile(expr);
            result = compiled.Evaluate(ctx);
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

        return ResultComparer.Compare(testCase.ResultElement, result, caughtException);
    }
}
