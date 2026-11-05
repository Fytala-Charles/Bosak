// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 03 November 2026
// PURPOSE              : Defines host-owned operation admission and availability contracts.
// SPECIAL NOTES        : Foundation types for the XQuery Data Model; used by all higher layers.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 03-11-2026     | Creation; bounded public ordinary-source facade (1.45.0).                               |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System;
using System.Threading;
namespace Bosak.XPath.Core.Schema;
/// <summary>Optional admission over resources already acquired and owned by the host/provider.</summary>
public interface IQueryOperationLifetime
{
    /// <summary>Synchronously admits work without transferring snapshot/stream ownership.</summary>
    /// <param name="cancellationToken">Cancellation before admission.</param>
    /// <returns>An idempotent admission lease retained until raw work completes.</returns>
    IQueryOperationLease Enter(CancellationToken cancellationToken);
}
/// <summary>Borrowed operation admission; disposal releases only this admission.</summary>
public interface IQueryOperationLease : IDisposable
{
    /// <summary>Checks revocation/cancellation when provider work resumes.</summary>
    /// <param name="cancellationToken">Cancellation of this work, not owner disposal.</param>
    void ThrowIfUnavailable(CancellationToken cancellationToken);
}
