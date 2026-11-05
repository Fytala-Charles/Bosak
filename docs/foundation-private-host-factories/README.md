# Private compilation factories and provider binding

1 November 2026. Production source **1.39.0** adds internal XPath, XQuery and XSLT compilation factories that capture effective context before binding a borrowed provider. [Specification basis](SPEC-BASIS.md) records the adoption sources. Public declarations and original friend grants remain unchanged; publication, C45 acceptance and API freeze remain pending.

| Qualification | Result |
|---|---:|
| Full solution rebuild | 0 errors; same 5 existing warnings |
| Ten unit projects | 24,341 passed; zero failed/skipped |
| New regressions | 26: Runtime 11, XSLT 15 |
| Runtime / XSLT totals | 317 / 8,234 |
| QT3 | 31,397 / 0 / 424 |
| qt4 gated sets | 8,941 / 0 / 1,918; 11 experimental passes |
| Actual factory integration | 103 checks passed twice |
| Public inventory | 11 assemblies / 270 types / 5,770 declarations, unchanged |

The [results](evidence/RESULTS.json), [test archive](evidence/unit-trx.zip), [execution inputs](evidence/INPUTS.json), [API comparison](evidence/API-COMPARISON.json) and [probe](evidence/probe-0.json) retain exact cohort evidence. Frozen skip maps and corpus revisions are unchanged. Actual load reports identify the rebuilt engines, including XSLT in its suite and both gates. All eleven product assembly versions are 1.39.0.0. Source and product archives include license notices; archive hashes identify the tested bytes independently of informational pre-commit revisions.

## Supported private profile

XPath captures its compiled context under XPath 3.1 compatibility. XQuery freezes registered module source strings and external namespaces, compiles that frozen registry, and captures effective prolog context; schema imports refuse. XSLT captures effective primitive static values and compiler settings from a principal stylesheet/transform. Custom resolvers, resource policies, listeners, schema services, imports/packages, use-when and shadow attributes refuse before provider binding. Static resource attempts refuse even when the stylesheet catches their errors. Captured descriptors retain exact numeric bit patterns, including negative zero.

These factories implement a bounded closed-resource profile. Each typed evaluation receives a fresh denying resource policy. The provider binds only after successful compilation and effective capture, must return the identical environment object, and supplies its issuer once. Availability and environment identity are rechecked at operation boundaries and guarded access. Bound sessions require typed snapshot-stamped inputs. Revocation blocks access while deferred lease cleanup remains possible. Providers are borrowed and are not disposed by the session.

The probe uses actual private factories and direct host delegates, with reflection only during setup. It retains a test provider and composite snapshot; native authenticated assessment and independent multi-source ownership are not established. Closed resource policy is not an operating-system sandbox or total execution budget.

## Performance and next work

Repeated 1,000-iteration component measurements retain 448 bytes for typed operation input capture, 2,920 for a fresh populated context, zero for guarded single-atomic visitation, and add 800 bytes for a fresh closed resource policy per evaluation. The earlier reflective fixture costs 33,461 bytes and the removed XQuery wrapper copy 3,744. These are component allocations, excluding full evaluation, compilation, worker allocations and consumer workload throughput.

Next qualify independent multi-source ownership and acquisition/cleanup failures through these factories before broadening resource or native-schema capture. Exact numeric execution, actual Schema/XmlDb/Braid qualification, owner acceptance, public facade and API freeze remain pending.

Read-only verification: `python -B docs/foundation-private-host-factories/tools/verify.py`. Tooling and raw receipts retain the complete qualification. No intentional unhandled crash tests or machine-wide error settings were used.
