// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 01 November 2026
// PURPOSE              : Binds private compilation environments to borrowed live provider capabilities.
// SPECIAL NOTES        : Part of the register-based virtual machine execution engine.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 01-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Resources;
namespace Bosak.XPath.Runtime.Vm;

/// <summary>Owner-created borrowed binding; acquiring or disposing provider resources is not transferred.</summary>
internal interface ISchemaSessionProvider
{
    /// <summary>Binds only after the compiler establishes this exact immutable environment.</summary>
    ISchemaSessionBinding Bind(SchemaQueryEnvironment environment);
}
/// <summary>Live owner capability whose environment and issuer must remain immutable.</summary>
internal interface ISchemaSessionBinding
{
    SchemaQueryEnvironment Environment { get; }
    ISchemaEvidenceIssuer Issuer { get; }
    void ThrowIfUnavailable();
}
/// <summary>Composes compiler-issued context, owner availability and a closed acquisition profile.</summary>
internal static class SchemaProviderSessionBinding
{
    internal const string Profile = "bounded-data-closed-resources-v1";
    private static readonly ControlledResourceRoute[] Routes = Enum.GetValues<ControlledResourceRoute>();
    private sealed class ClosedAuthority : IControlledResourceAuthority
    {
        internal static readonly ClosedAuthority Instance = new();
        ControlledResourceResponse? IControlledResourceAuthority.Authorize(ControlledResourceRequest request)
            => ControlledResourceResponse.Deny("The private profile has no captured external resources.");
    }
    internal static ControlledResourcePolicy ClosedResources() => new(ClosedAuthority.Instance, Routes);
    internal static void Validate(ISchemaSessionProvider provider, int snapshots, int values, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(provider);
        if (snapshots <= 0) throw new ArgumentOutOfRangeException(nameof(snapshots));
        if (values <= 0) throw new ArgumentOutOfRangeException(nameof(values));
        cancellationToken.ThrowIfCancellationRequested();
    }
    internal static SchemaQuerySession Create(SchemaQueryEnvironment environment, Func<EvaluationContext, XdmValue> evaluate,
        ISchemaSessionProvider provider, int snapshots, int values, CancellationToken cancellationToken)
    {
        Validate(provider, snapshots, values, cancellationToken);
        var binding = provider.Bind(environment) ?? throw new ArgumentException("Provider returned no binding.", nameof(provider));
        cancellationToken.ThrowIfCancellationRequested();
        void Check()
        {
            if (!ReferenceEquals(binding.Environment, environment))
                throw new ArgumentException("Provider binding belongs to another compilation environment.", nameof(provider));
            binding.ThrowIfUnavailable();
        }
        Check();
        var issuer = binding.Issuer ?? throw new ArgumentException("Provider returned no issuer.", nameof(provider));
        Check();
        cancellationToken.ThrowIfCancellationRequested();
        return new SchemaQuerySession(environment, context =>
        {
            Check();
            context.ResourcePolicy = ClosedResources();
            return evaluate(context);
        }, issuer, snapshots, values, Check, true);
    }
}
