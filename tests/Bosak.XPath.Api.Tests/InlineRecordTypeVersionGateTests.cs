// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 22 October 2026
// PURPOSE              : Verifies version gates for inline-function record parameter and return types.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 22-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.XPath.Api;
using Bosak.XPath.Parser;
using Xunit;

namespace Bosak.XPath.Api.Tests;

/// <summary>Verifies version gates for inline-function record parameter and return types.</summary>
public class InlineRecordTypeVersionGateTests
{
    /// <summary>Requires 4.0 for record types in direct and nested inline signatures.</summary>
    /// <param name="expression">The inline function expression.</param>
    /// <param name="compatibility">The expression compatibility level.</param>
    /// <param name="allowed">Whether the signature is legal in this version.</param>
    [Theory]
    [InlineData("function($r as record(a)) { $r }", XPathCompatibility.XPath30, false)]
    [InlineData("function($r as record(*)) { $r }", XPathCompatibility.XPath30, false)]
    [InlineData("function() as record(a) { map{'a':1} }", XPathCompatibility.XPath30, false)]
    [InlineData("function($r as map(xs:string, record(a))) { $r }", XPathCompatibility.XPath30, false)]
    [InlineData("function($r as array(record(a))) { $r }", XPathCompatibility.XPath30, false)]
    [InlineData("function($f as function(record(a)) as record(a)) { $f }", XPathCompatibility.XPath30, false)]
    [InlineData("function($f as function() as record(a)) { $f }", XPathCompatibility.XPath30, false)]
    [InlineData("function() as map(xs:string, record(*)) { map{} }", XPathCompatibility.XPath30, false)]
    [InlineData("function($r as record(a)) { $r }", XPathCompatibility.XPath31, false)]
    [InlineData("function($r as record(*)) { $r }", XPathCompatibility.XPath31, false)]
    [InlineData("function() as record(a) { map{'a':1} }", XPathCompatibility.XPath31, false)]
    [InlineData("function($r as map(xs:string, record(a))) { $r }", XPathCompatibility.XPath31, false)]
    [InlineData("function($r as array(record(a))) { $r }", XPathCompatibility.XPath31, false)]
    [InlineData("function($f as function(record(a)) as record(a)) { $f }", XPathCompatibility.XPath31, false)]
    [InlineData("function($f as function() as record(a)) { $f }", XPathCompatibility.XPath31, false)]
    [InlineData("function() as map(xs:string, record(*)) { map{} }", XPathCompatibility.XPath31, false)]
    [InlineData("function($r as record(a)) { $r }", XPathCompatibility.XPath40, true)]
    [InlineData("function($r as record(*)) { $r }", XPathCompatibility.XPath40, true)]
    [InlineData("function() as record(a) { map{'a':1} }", XPathCompatibility.XPath40, true)]
    [InlineData("function($r as map(xs:string, record(a))) { $r }", XPathCompatibility.XPath40, true)]
    [InlineData("function($r as array(record(a))) { $r }", XPathCompatibility.XPath40, true)]
    [InlineData("function($f as function(record(a)) as record(a)) { $f }", XPathCompatibility.XPath40, true)]
    [InlineData("function($f as function() as record(a)) { $f }", XPathCompatibility.XPath40, true)]
    [InlineData("function() as map(xs:string, record(*)) { map{} }", XPathCompatibility.XPath40, true)]
    [InlineData("function($r as record(a)) { $r }", XPathCompatibility.XPath40Experimental, true)]
    [InlineData("function($r as record(*)) { $r }", XPathCompatibility.XPath40Experimental, true)]
    [InlineData("function() as record(a) { map{'a':1} }", XPathCompatibility.XPath40Experimental, true)]
    [InlineData("function($r as map(xs:string, record(a))) { $r }", XPathCompatibility.XPath40Experimental, true)]
    [InlineData("function($r as array(record(a))) { $r }", XPathCompatibility.XPath40Experimental, true)]
    [InlineData("function($f as function(record(a)) as record(a)) { $f }", XPathCompatibility.XPath40Experimental, true)]
    [InlineData("function($f as function() as record(a)) { $f }", XPathCompatibility.XPath40Experimental, true)]
    [InlineData("function() as map(xs:string, record(*)) { map{} }", XPathCompatibility.XPath40Experimental, true)]
    public void RecordSignatures(string expression, XPathCompatibility compatibility, bool allowed)
    {
        var options = new CompileOptions { Compatibility = compatibility };
        if (allowed)
            Assert.NotNull(XPath31Expression.Compile(expression, options));
        else
        {
            var error = Assert.Throws<XPathParseException>(() => XPath31Expression.Compile(expression, options));
            Assert.Contains("XPST0003", error.Message);
        }
    }

    /// <summary>Retains record as a legal node name or processing-instruction target.</summary>
    /// <param name="expression">The ordinary node type signature.</param>
    /// <param name="compatibility">The expression compatibility level.</param>
    [Theory]
    [InlineData("function($n as element(record)) { $n }", XPathCompatibility.XPath30)]
    [InlineData("function($n as attribute(record)) { $n }", XPathCompatibility.XPath30)]
    [InlineData("function($n as processing-instruction('record')) { $n }", XPathCompatibility.XPath30)]
    [InlineData("function($n as element(record)) { $n }", XPathCompatibility.XPath31)]
    [InlineData("function($n as attribute(record)) { $n }", XPathCompatibility.XPath31)]
    [InlineData("function($n as processing-instruction('record')) { $n }", XPathCompatibility.XPath31)]
    [InlineData("function($n as element(record)) { $n }", XPathCompatibility.XPath40)]
    [InlineData("function($n as attribute(record)) { $n }", XPathCompatibility.XPath40)]
    [InlineData("function($n as processing-instruction('record')) { $n }", XPathCompatibility.XPath40)]
    [InlineData("function($n as element(record)) { $n }", XPathCompatibility.XPath40Experimental)]
    [InlineData("function($n as attribute(record)) { $n }", XPathCompatibility.XPath40Experimental)]
    [InlineData("function($n as processing-instruction('record')) { $n }", XPathCompatibility.XPath40Experimental)]
    public void OrdinaryNodeNames(string expression, XPathCompatibility compatibility)
    {
        Assert.NotNull(XPath31Expression.Compile(expression,
            new CompileOptions { Compatibility = compatibility }));
    }
}
