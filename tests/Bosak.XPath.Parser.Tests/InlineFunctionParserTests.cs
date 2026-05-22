// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 21 mei 2026
// PURPOSE              : Unit tests verifying parser support for typed inline function parameters.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 21-05-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.XPath.Parser.Ast;
using Xunit;

namespace Bosak.XPath.Parser.Tests;

public class InlineFunctionParserTests
{
    [Fact]
    public void Parse_InlineFunction_WithTypedParams()
    {
        var ast = XPathParser.Parse("function($n as xs:string){upper-case($n)}");
        Assert.IsType<InlineFunctionNode>(ast);
    }

    [Fact]
    public void Parse_InlineFunction_WithReturnType()
    {
        var ast = XPathParser.Parse("function($e as xs:string) as xs:string { lower-case($e) }");
        Assert.IsType<InlineFunctionNode>(ast);
    }

    [Fact]
    public void Parse_NestedInlineFunction()
    {
        var ast = XPathParser.Parse("function($this as xs:integer) as xs:boolean {$seqParam[$this] is $srchParam}");
        Assert.IsType<InlineFunctionNode>(ast);
    }

    [Fact]
    public void Parse_InstanceOfFunctionStar()
    {
        var ast = XPathParser.Parse("$arg instance of function(*)");
        Assert.IsType<InstanceOfNode>(ast);
    }

    [Fact]
    public void Parse_TreatAsEmptySequence()
    {
        var ast = XPathParser.Parse("fn:error() treat as empty-sequence()");
        Assert.IsType<TreatNode>(ast);
    }
}
