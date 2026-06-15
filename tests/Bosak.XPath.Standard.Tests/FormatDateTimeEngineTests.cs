// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 June 2026
// PURPOSE              : Unit tests for English-language date/time formatting extensions.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 13-06-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Xunit;

namespace Bosak.XPath.Standard.Tests;

public class FormatDateTimeEngineTests
{
    [Fact]
    public void FormatDate_CardinalDay_Uppercase()
    {
        var expr = XPath31Expression.Compile("format-date(xs:date('2003-12-01'), '[DW]')");
        var result = expr.Evaluate((IXdmNode?)null!);
        Assert.Equal("ONE", result.ToString());
    }

    [Fact]
    public void FormatDate_OrdinalDay_Lowercase()
    {
        var expr = XPath31Expression.Compile("format-date(xs:date('2003-12-21'), '[Dwo]')");
        var result = expr.Evaluate((IXdmNode?)null!);
        Assert.Equal("twenty first", result.ToString());
    }

    [Fact]
    public void FormatDate_OrdinalDay_TitleCase()
    {
        var expr = XPath31Expression.Compile("format-date(xs:date('2003-12-31'), '[DWwo]')");
        var result = expr.Evaluate((IXdmNode?)null!);
        Assert.Equal("Thirty First", result.ToString());
    }

    [Fact]
    public void FormatDate_CardinalYear_TitleCase()
    {
        var expr = XPath31Expression.Compile("format-date(xs:date('2021-01-01'), '[YWw]')");
        var result = expr.Evaluate((IXdmNode?)null!);
        Assert.Equal("Two Thousand And Twenty One", result.ToString());
    }

    [Fact]
    public void FormatDate_OrdinalYear_Numeric()
    {
        var expr = XPath31Expression.Compile("format-date(xs:date('1990-01-01'), '[Y1o]')");
        var result = expr.Evaluate((IXdmNode?)null!);
        Assert.Equal("1990th", result.ToString());
    }

    [Fact]
    public void FormatDate_NegativeYear_WithEra()
    {
        var expr = XPath31Expression.Compile("format-date(xs:date('-0055-01-01'), '[Y][EN]')");
        var result = expr.Evaluate((IXdmNode?)null!);
        Assert.Equal("55BC", result.ToString());
    }
}
