# Full 1.36 API inventory and C45 public delta

31 October 2026. The complete observed inventory now covers **11 assemblies,270 exported types and5,770 entries**. It is refreshed metadata with proposed review classifications, not an accepted stable API manifest or consumer qualification.

All5,766 exact previous IDs remain; none is removed. The four added entries are the two implemented XdmValue equality method declarations and their two attribute-evidence rows. The [legacy crosswalk](evidence/LEGACY-CROSSWALK.json) preserves all176 XmlDb mappings despite shifted row positions. Existing enum/default/interface-slot/value-field rows and carried-forward review classifications remain unchanged.

- `A Bosak.XPath.Core.Xdm.XdmValue.Equals(System.Object other:None:none:required=:optional=:attrs=):[System.Runtime.CompilerServices.NullableContextAttribute((Byte)2)]`
- `A Bosak.XPath.Core.Xdm.XdmValue.GetHashCode():`
- `M Bosak.XPath.Core.Xdm.XdmValue.Equals:Public, Virtual, HideBySig:Standard, HasThis:System.Boolean :None:none:required=:optional=:attrs=(System.Object other:None:none:required=:optional=:attrs=):`
- `M Bosak.XPath.Core.Xdm.XdmValue.GetHashCode:Public, Virtual, HideBySig:Standard, HasThis:System.Int32 :None:none:required=:optional=:attrs=():`

## Actual inventory and provenance

LanguageServer is the eleventh inventory assembly and is outside Bosak.sln. It was rebuilt against current 1.36 outputs using --no-dependencies; all ten qualified engine DLL hashes remain identical before/after the audit and proposal builds. The guarded original inventory encoding was captured twice with identical results. No production source or owner checkout changed. No language-server runtime test or consumer/engine suite is claimed by this metadata-only step.

[public-api.json](evidence/public-api.json) contains exact rows; [member-manifest.json](evidence/member-manifest.json) retains content-derived IDs, conservative review groups and historical compatibility-case links. [signature-closure.json](evidence/signature-closure.json) refreshes conservative public type reachability. A/L rows are attribute/layout evidence, not callable additions. The old stable/legacy/experimental review policy is carried forward for review; all four role dispositions remain Pending. The historical baseline and owner matrices are untouched.

## Proposed public delta

The [exact 40-type proposal](PROPOSAL.md) separates provider/lifetime contracts in Bosak.XPath.Core.Schema from host facades/limits in Bosak.XPath.Api.Schema. It compiles warning-free in two isolated assemblies. The [member delta](evidence/proposed-member-delta.json) records every proposed declaration and intended target assembly, separately from the implemented 5,770 entries. No proposed facade executes and no product export is added by compiling it.

This proposal adds explicit producer entry points, finite limits and multi-snapshot callback ownership to V5's provider shape. It remains incomplete for full numeric precision/domain, XQuery/XSLT host entry points, closure granularity and actual provider/worker semantics. Those required decisions are prominently recorded before freeze. The public shape is reviewable; it is not a complete accepted C45 SDK.

## Verification and next work

Run `python -B docs/foundation-c45-delta136/tools/verify.py`. [Capture](tools/capture.py), [proposal compilation](tools/propose.py) and [manifest generation](tools/manifest.py) retain bounded/scoped execution and guarded metadata loading. Existing 1.36 runtime/gate results remain attached to their tested source and hashes; no new behavioral/consumer acceptance is inferred.

Next: resolve the named numeric-domain and all-host signature gaps with actual Schema/XmlDb/Braid use cases, and dispose closure/lifetime performance choices before advancing this public delta to C45 acceptance. Product pins, publication and final API freeze remain pending.
