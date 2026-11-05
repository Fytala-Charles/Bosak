# Production promotion: internal explicit XSLT context

1 November 2026. The qualified context bridge is now implemented in production source at **1.37.0**. It adds an internal entry point with independent document-match and node-global inputs; existing public invocation keeps legacy source fallback. No public facade, package publication, consumer acceptance or API freeze is introduced.

The [adoption basis](SPEC-BASIS.md) was recorded before source changes. This promotes the semantic changes from the [corrected isolated candidate](../foundation-xslt-context-qualification/README.md), retains the original project metadata/friend declarations, and adds 17 focused production regression cases. Source histories record the change; the shared minor version increases for the internal feature.

| Fresh production qualification | Result |
|---|---:|
| Actual solution build | 0 errors; same 5 existing warnings |
| All ten rebuilt unit projects | 24,284 passed, zero failed/skipped |
| XSLT project within that total | 8,213 passed, including 17 new cases |
| Database project within that total | 47 passed with loopback access |
| QT3 frozen gate | 31,397 / 0 / 424 |
| qt4 gated sets | 8,941 / 0 / 1,918; 11 experimental passes |
| Full public declaration audit | 11 assemblies / 270 types / 5,770 rows, unchanged |

Both frozen skip maps remain exact; corpus revisions/status remain clean and unchanged. Actual load-time observers verify the rebuilt ten-engine cohort and every test/gate entry assembly, including XSLT execution in its suite and both gates. Eight named fn:transform/nested-transform controls pass within the full XSLT run. The standard-library unit suite does not load XSLT and is not attributed actual XSLT host execution.

## Implementation boundary

TransformEngine.Transform retains its signature and delegates to an internal core with legacy fallback enabled. The internal XsltExecutable.TransformWithExplicitContext disables fallback: absent global input stays absent, required checks use explicit input, inherited focus is cleared, and initial named-template focus defaults to effective global input. Named-template entry does not inherit a current matching rule from its incidental match source. Stylesheet-declared absent context ignores supplied global input. Only document-root initial matching is admitted; other match kinds raise NotSupportedException. Element global-context input is covered.

The bridge returns the raw principal result and captures secondary results without delivering them externally. It uses the existing stack runner. The borrowed EvaluationContext remains subject to transform mutation; restoration, owner lifetime/admission, resource policies and private session integration are separate work. The existing implementation diagnostic XTDE3086 is retained with the specification caveat in the adoption basis. Broader item selection, atomic globals, explicit named-focus overrides, modes/streaming profiles and public host producers are not supplied by this bounded internal entry.

## Exact cohort and metadata

[Source archive/pins](evidence/SOURCE.json), [actual execution layouts](evidence/INPUTS.json), [test counters](evidence/UNITS.json), [TRX archive](evidence/unit-trx.zip), [gate exits](evidence/GATES.json) and [final summary](evidence/RESULTS.json) identify this production run. [The API comparison](evidence/API-COMPARISON.json) and [repeated full inventory](evidence/public-api-0.json) retain every prior declaration row. Assembly identities/versions are recorded separately: all eleven product assemblies are 1.37.0.0. Actual assembly-level XSLT friend grants equal the corrected 1.36.0 baseline; no grant was added or lost.

The language server is outside Bosak.sln and receives a separate no-dependencies build against the qualified outputs. The first audit exposed its stale 1.36.0 binary; that mixed-version observation remains separate. The final inventory is entirely 1.37.0. The source archive and licensed eleven-DLL archive preserve exact tested bytes. Informational repository revisions can name the pre-commit baseline; source hashes and this packet's delivery commit identify the applied change, rather than inferring source from that revision alone.

Tests execute from isolated copies of actual newly built outputs, using the temporary-tree Application Control workaround and a workspace copy for database loopback fixtures. No assertion/corpus skip policy changed. Earlier candidate receipts remain historical and are not relabeled as production receipts. Earlier tools that assert current files equal the 1.36.0 cohort do not describe current 1.37.0 outputs.

Next connect and qualify private all-host session execution using this bridge, including caller-context restoration and borrowed-operation lifetime checks. Exact numeric execution, public C45 contracts and actual Schema/XmlDb/Braid acceptance remain pending. This source promotion does not freeze the API or publish packages.

The [phase runner](tools/run.py) records the production build/test/gate commands. Use a fresh evidence destination for replay. Read-only verification: `python -B docs/foundation-xslt-context-promotion/tools/verify.py`. Negative cases are caught under the scoped runner error mode; no deliberate unhandled crash or machine-wide error-reporting change occurred.
