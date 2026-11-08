// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 05 November 2026
// PURPOSE              : Compiles captured ordinary XQuery into a shared engine-owned query session.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 05-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Api.Schema;
using Bosak.XPath.Core.Schema;
using Session=Bosak.XPath.Api.Schema.SchemaQuerySession;
namespace Bosak.XQuery.Api.Schema;

/// <summary>Captures effective XQuery prolog and registered modules for ordinary query sessions.</summary>
public static class SchemaXQueryCompiler
{
    /// <summary>Compiles a frozen ordinary XQuery session with explicit input/resource ownership.</summary>
    /// <param name="query">Query source, including its prolog.</param>
    /// <param name="limits">Explicit positive session limits and nonnegative decimal scale.</param>
    /// <param name="compiler">Settings/module catalog copied before evaluation; null uses a new compiler.</param>
    /// <param name="assessor">Reserved borrowed assessor; non-null returns Unsupported in this ordinary profile.</param>
    /// <param name="xml11LineEndings">The existing XQuery XML 1.1 line-ending profile.</param>
    /// <param name="cancellationToken">Cooperative compilation cancellation, checked before and after compilation.</param>
    /// <returns>A shared ordinary XQuery session, or Unsupported for schema-aware/assessor routes.</returns>
    /// <exception cref="ArgumentException">Required query or explicit limits are invalid.</exception>
    /// <exception cref="OperationCanceledException">Compilation was canceled.</exception>
    /// <remarks>Compile errors propagate. Context permits direct scalar and admitted node data; results are empty, atomic, node or flat sequences. Maps, arrays and functions are unavailable. Deferred sequence work belongs inside Run/RunAsync; returned nodes and cached values are borrowed and not recursively detached. Existing legacy numeric behavior remains; no general exact arithmetic or hard time/byte budget is added.</remarks>
    public static SchemaResult<Session> Compile(string query,SchemaQueryLimits limits,XQueryCompiler? compiler=null,
        ISchemaAssessor? assessor=null,bool xml11LineEndings=false,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(query);limits.Validate();cancellationToken.ThrowIfCancellationRequested();
        if(assessor is not null)return Session.Unsupported<Session>();
        var session=CapturedQueryHost.CompilePublic(query,compiler??new XQueryCompiler(),limits,xml11LineEndings,cancellationToken);
        return session is null?Session.Unsupported<Session>():Session.Supported(session);
    }
}
