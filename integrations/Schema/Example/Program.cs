// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 29 October 2026
// PURPOSE              : Runs the activation-free schema-aware transformation example.
// SPECIAL NOTES        : Development integration example; no production core API change.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 29-10-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 17-11-2026     | Add explicit native decimal example mode; retain default price output
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.Schema.Example;

if (args is ["--native"])
{
    var data = Path.Combine(AppContext.BaseDirectory, "TestData", "Schemas");
    Console.WriteLine(NativeDecimalExample.Run(File.ReadAllText(Path.Combine(data, "native-decimal.xsd")),
        File.ReadAllText(Path.Combine(data, "native-decimal.xml"))));
    return 0;
}
if (args.Length != 0)
{
    Console.Error.WriteLine("Use no arguments for the typed-price example, or --native for the finite native decimal example.");
    return 2;
}
Console.WriteLine(PriceExample.Transform(XDocument.Parse(
    "<price xmlns='urn:bosak:example:prices' currency='EUR'>12.50</price>")));
return 0;
