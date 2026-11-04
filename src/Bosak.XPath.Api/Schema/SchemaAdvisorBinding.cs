// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 04 November 2026
// PURPOSE              : Binds public assessor endpoints to exact engine environments and admitted inputs.
// SPECIAL NOTES        : Public surface API for compiling and evaluating XPath 3.1 expressions.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 04-11-2026     | Creation; internal public-contract adapter (1.56.0).                                      |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Runtime.Vm;

namespace Bosak.XPath.Api.Schema;

/// <summary>Binds public assessor endpoints to exact engine environments and admitted inputs.</summary>
internal sealed class SchemaAdvisorBinding
{
    private readonly QuerySourceSession _runtime;
    private readonly IQuerySemanticEnvironment _environment;
    private readonly ISchemaAssessor _assessor;
    private readonly ISchemaAssessmentAdvisor _advisor;
    private readonly ISchemaAuthority _authority;
    private readonly Action<CancellationToken> _checkSession;
    private SchemaAdvisorBinding(QuerySourceSession runtime,IQuerySemanticEnvironment environment,ISchemaAssessor assessor,
        ISchemaAssessmentAdvisor advisor,ISchemaAuthority authority,Action<CancellationToken> checkSession)
    {_runtime=runtime;_environment=environment;_assessor=assessor;_advisor=advisor;_authority=authority;_checkSession=checkSession;}
    internal static SchemaAdvisorBinding? Bind(QuerySourceSession runtime,IQuerySemanticEnvironment environment,ISchemaAssessor assessor,
        Action<CancellationToken> checkSession,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(assessor);checkSession(token);
        var authority=assessor.Authority;checkSession(token);
        if(authority is null)throw new ArgumentException("Assessor has no authority.",nameof(assessor));
        var outcome=assessor.Bind(environment,token);checkSession(token);
        if(outcome is null)throw new ArgumentException("Assessor returned no binding outcome.",nameof(assessor));
        var kind=outcome.Kind;checkSession(token);
        if(kind==SchemaResultKind.Unsupported)return null;
        if(kind!=SchemaResultKind.Supported)throw new ArgumentException("Invalid assessor binding outcome.",nameof(assessor));
        var advisor=outcome.Value;checkSession(token);
        if(advisor is null)throw new ArgumentException("Supported binding has no advisor.",nameof(assessor));
        var result=new SchemaAdvisorBinding(runtime,environment,assessor,advisor,authority,checkSession);result.Check(token);return result;
    }
    private void Check(CancellationToken token)
    {
        _checkSession(token);_advisor.ThrowIfUnavailable(token);_checkSession(token);
        var assessor=_advisor.Assessor;_checkSession(token);_advisor.ThrowIfUnavailable(token);_checkSession(token);
        if(!ReferenceEquals(assessor,_assessor))throw new ArgumentException("Advisor endpoint changed.");
        var environment=_advisor.QueryEnvironment;_checkSession(token);_advisor.ThrowIfUnavailable(token);_checkSession(token);
        if(!ReferenceEquals(environment,_environment))throw new ArgumentException("Advisor environment changed.");
        var authority=_assessor.Authority;_checkSession(token);_advisor.ThrowIfUnavailable(token);_checkSession(token);
        if(!ReferenceEquals(authority,_authority))throw new ArgumentException("Assessor authority changed.");
    }
    internal IQueryTypedContainerIssuer ForInput(QuerySourceSession runtime,ISchemaAssessmentInput original,IQuerySourceInput current,CancellationToken token)
    {
        if(!ReferenceEquals(runtime,_runtime))throw new ArgumentException("Foreign binding session.");
        Check(token);return new Issuer(this,original,current);
    }
    private sealed class Issuer(SchemaAdvisorBinding owner,ISchemaAssessmentInput original,IQuerySourceInput current) : IQueryTypedContainerIssuer
    {
        SchemaQueryEnvironment IQueryTypedContainerIssuer.Environment=>owner._runtime.Environment;
        ISchemaAuthority IQueryTypedContainerIssuer.Authority=>owner._authority;
        void IQueryTypedContainerIssuer.ThrowIfUnavailable(CancellationToken token)=>owner.Check(token);
        bool IQueryTypedContainerIssuer.OwnsInput(ISchemaAssessmentInput input,IQuerySourceInput projection)
            =>ReferenceEquals(input,original)&&ReferenceEquals(projection,current);
        bool IQueryTypedContainerIssuer.OwnsOrigin(ISchemaAssessmentOrigin origin,ISchemaAssessmentInput input,IQuerySourceInput projection)
            =>ReferenceEquals(input,original)&&ReferenceEquals(projection,current)&&owner._advisor.OwnsAssessment(origin);
        bool IQueryTypedContainerIssuer.OwnsContainer(ISchemaTypedValue value,ISchemaAssessmentInput input,IQuerySourceInput projection,ISchemaAssessmentOrigin? origin)
            =>ReferenceEquals(input,original)&&ReferenceEquals(projection,current)&&owner._advisor.OwnsTypedValue(value,original,origin);
    }
}
