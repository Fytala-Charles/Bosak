// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 November 2026
// PURPOSE              : Proves the calendar package consumer has no friend access.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 25-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Reflection;
using System.Runtime.CompilerServices;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Api.Schema;
using Bosak.XPath.Runtime.Vm;
using Xunit;

/// <summary>Verifies the independent package-only assembly boundary.</summary>
public sealed class NoFriendTests
{
    /// <summary>Requires all consumed engine assemblies to omit a friend grant to this consumer.</summary>
    [Fact] public void PublicConsumerHasNoFriendGrant()
    {
        string name = Assembly.GetExecutingAssembly().GetName().Name!;
        foreach(var a in new[] {typeof(XdmValue).Assembly,typeof(Bosak.XPath.Api.Schema.SchemaQuerySession).Assembly,typeof(EvaluationContext).Assembly})
            Assert.DoesNotContain(a.GetCustomAttributes<InternalsVisibleToAttribute>(),x=>x.AssemblyName.Split(',')[0]==name);
    }
}
