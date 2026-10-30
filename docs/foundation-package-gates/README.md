# F140 — Package-backed solution and frozen gates

30 October 2026. **24,225 unit tests and both frozen gates pass with the exact seven candidate-package DLLs.** Actual runtime binding reports verify package hashes. This is a local mixed assembly cohort: remaining product dependencies are explicitly retained source-built bytes. No production source/version, test assertion, package payload or delegated owner project changed.

## Qualified assembly mix

The seven DLLs come byte-for-byte from [the immutable-source candidate feed](../foundation-package-qualification/README.md), version 1.34.2/source c88eea43. Complete existing test/gate output directories were copied into fresh ignored runners; only matching package DLLs were replaced. Test/gate entry binaries remain unchanged.

Bosak.Xslt, Bosak.XQuery and Bosak.XPath.Providers.Database remain on their previously qualified source-built hashes when required. They are **not claimed as package-derived**. LanguageServer is outside the solution and this run. Actual Schema/XmlDb/Braid qualification remains delegated; these are engine solution tests, not external consumer acceptance.

## Selected execution results

| Solution unit-test project | Passed / zero failures |
|---|---:|
| `Bosak.XPath.Core.Tests` | 132 |
| `Bosak.XPath.Parser.Tests` | 202 |
| `Bosak.XPath.Runtime.Tests` | 269 |
| `Bosak.XPath.Api.Tests` | 489 |
| `Bosak.XPath.Standard.Tests` | 13797 |
| `Bosak.XPath.Compiler.Tests` | 66 |
| `Bosak.Xslt.Tests` | 8196 |
| `Bosak.XQuery.Tests` | 804 |
| `Bosak.XPath.Providers.Tests` | 223 |
| `Bosak.XPath.Providers.Database.Tests` | 47 |
| **Total** | **24,225** |

| Frozen gate | Passed / failed / skipped | Additional checks |
|---|---|---|
| QT3 | 31,397 / 0 / 424 | Exact retained skip names/reasons unchanged |
| Gated qt4 | 8,941 / 0 / 1,918 | 230 sets, 11 experimental passes; exact skip names/reasons unchanged |

[RESULTS.json](RESULTS.json) links compressed original TRX files, commands, logs, observer identities and process binding reports. Eight verifier/refusal controls pass. Repeated/corrected attempts are preserved separately and are not added to the selected aggregate.

## Runtime binding proof

The qualification-only [startup observer](tools/StartupHook.cs) captures assembly identity, location, context and content hash. It stores strings, not Assembly, load-context, node or provider objects. It executes no provider operations. Verification requires a report containing each actual test/gate entry assembly and at least one exact package binding, validates every observed default-context package hash, and requires all seven in both gate processes. Empty VSTest parent reports alone are insufficient.

Both diagnostic observer versions are retained with source/binary hashes. V1 writes at process exit; V2 also persists at load time and captures its output path during initialization. Collectible/non-default contexts are retained as observations but are not conflated with the default-context package cohort. This instrumentation is not a general unload, lifetime or concurrency certificate.

## Preserved setup corrections

1. **Runtime fixture layout:** the initial isolated runner passed 218 and failed 51 because unchanged tests resolve `../../../../qt3tests` from the assembly directory. All failures are DirectoryNotFoundException for the corpus. A nested Runtime runner plus a task-owned junction to the original unmodified QT3 fixtures restores that layout. The same 269 cases then pass; no package/test/assertion change occurred.
2. **XSLT binding capture:** the initial XSLT run passed all 8,196 cases but lacked a usable testhost exit-time binding report. Its successful receipt is retained without claiming complete binding evidence. The same binary/package cohort was rerun with V2 load-time capture: all 8,196 pass and actual package bindings are recorded. No test behavior or assertion was edited to produce the pass.

Original failed/incomplete-evidence receipts, their bindings and correction notes remain in evidence. [The verifier](tools/verify.py) checks their original counts and distinguishes them from selected qualification.

## Reproduce without overwriting evidence

From the repository root, with compiled solution outputs and .NET 10/Python available:

```powershell
python docs/foundation-package-gates/tools/verify.py
python -m unittest discover -s docs/foundation-package-gates/tools -p test_receipts.py -v
python docs/foundation-package-gates/tools/prepare.py --workspace .guard-tmp/package-gates-new
python docs/foundation-package-gates/tools/run.py --workspace .guard-tmp/package-gates-new --all-units
python docs/foundation-package-gates/tools/run.py --workspace .guard-tmp/package-gates-new --gate qt3
python docs/foundation-package-gates/tools/run.py --workspace .guard-tmp/package-gates-new --gate qt4
```

Prepare requires a fresh directory, builds the diagnostic observer and preserves Runtime's fixture-relative layout. Original bin directories and corpora are untouched. Unit execution needs permission for the existing controlled local HTTP fixtures. On this run Application Control did not require the XSLT workaround. New runs write only into their own ignored workspace; retain this sealed cohort unchanged. Collection into an already sealed dossier is refused.

## Disposition and next task

The exact seven-package cohort now has selected legacy caller source/binary checks plus full solution/frozen-gate qualification in the documented mixed assembly environment. It remains unsigned, unpublished and not accepted as the permanent foundation API. No new skip admission or API change was made; full XSD 1.1 and neutral C45 contracts remain pending.

Next core task: consolidate a concise owner handoff/readiness packet with the exact feed/API/qualification identities, the remaining neutral-contract decisions and explicit Schema/XmlDb acceptance checklist. Do not turn these core tests into invented external sign-offs or general API freeze acceptance.
