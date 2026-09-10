// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 September 2026
// PURPOSE              : Generates the synthetic catalog XML document shared by all benchmarks.
// SPECIAL NOTES        : Benchmark harness for the Bosak XPath 3.1 implementation; not shipped.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-09-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Globalization;
using System.Text;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;

namespace Bosak.Benchmarks;

/// <summary>
/// Builds a synthetic catalog document used as benchmark input. No external files are
/// involved: the XML is generated deterministically in memory and parsed once per
/// benchmark class in GlobalSetup, so document construction is never part of the
/// measured workload.
/// </summary>
internal static class CatalogData
{
    /// <summary>The number of <c>item</c> elements in the generated catalog.</summary>
    public const int ItemCount = 2000;

    private static string? _xml;

    /// <summary>The catalog as XML text (generated once, then cached).</summary>
    public static string Xml => _xml ??= GenerateXml();

    /// <summary>Parses the catalog XML into a fresh XDM document node.</summary>
    /// <returns>The document node of the parsed catalog.</returns>
    public static IXdmNode CreateDocument() => XDocumentProvider.ParseXml(Xml);

    /// <summary>
    /// Generates the catalog XML: <c>ItemCount</c> <c>item</c> elements, each carrying
    /// id/name/price/quantity/category attributes and description/rating child elements.
    /// Values are pure functions of the item index so runs are fully deterministic.
    /// </summary>
    private static string GenerateXml()
    {
        var sb = new StringBuilder(ItemCount * 260);
        sb.Append("<catalog>");
        for (int i = 1; i <= ItemCount; i++)
        {
            var price = (i * 37) % 500 + 1;          // 1..500, well distributed
            var quantity = i % 97;                    // 0..96
            var category = i % 25;                    // cat0..cat24
            var rating = i % 5 + 1;                   // 1..5
            sb.Append("<item id=\"item-").Append(i.ToString("D4", CultureInfo.InvariantCulture))
              .Append("\" name=\"Product ").Append(i)
              .Append("\" price=\"").Append(price)
              .Append("\" quantity=\"").Append(quantity)
              .Append("\" category=\"cat").Append(category)
              .Append("\"><description>Description for product ").Append(i)
              .Append(" in category cat").Append(category)
              .Append(".</description><rating>").Append(rating)
              .Append("</rating></item>");
        }
        sb.Append("</catalog>");
        return sb.ToString();
    }
}
