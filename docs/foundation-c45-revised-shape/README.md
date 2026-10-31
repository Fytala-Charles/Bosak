# Revised numeric and all-host C45 declarations

31 October 2026. Review proposal after the [exact numeric experiment](../foundation-exact-numeric-prototype/README.md). No production API, version, baseline, qualified binary or owner pin changed. [Specification basis](SPEC-BASIS.md) preceded compilation. All declared operation bodies throw Proposal only; none was executed.

Four isolated assemblies compile with zero warnings/errors and export 45 proposed public types: Core 37, API 4, XQuery 1, XSLT 3. [Metadata capture](evidence/api-0.json) and [repeat](evidence/api-1.json) are identical, with 615 declaration/evidence rows. This includes metadata attributes and private value-layout evidence, not 615 callable APIs. All 40 preceding proposed type identities remain. Provider/lifetime source files are unchanged copies. The five added types are XdmExactDecimal and four host types. The earlier [proposal](../foundation-c45-delta136/PROPOSAL.md) remains sealed historical evidence.

## Selected declaration changes

| Boundary | Revised review shape |
|---|---|
| Exact decimal | [XdmExactDecimal](proposal/ExactNumeric.cs.txt): bounded supplied coefficient/scale constructor, normalized properties, numeric equality/hash and bounded ToInvariantString. This formatter does not promise XSD canonical lexical text. |
| Existing XdmValue | Six intended factory/accessor additions in the [member ledger](NUMERIC-MEMBERS.json), checked by an internal compiled interface witness. They are not members of the real product type and are not counted as public exports in this inventory. Cross-assembly partial types cannot establish that integration. |
| Finite limits | [SchemaQueryLimits](proposal/QueryFacade.cs.txt) now takes five required arguments, adding positive MaxNumericDigits and nonnegative MaxDecimalScale. This deliberately replaces the preceding unaccepted three-argument proposal constructor. No workload defaults are selected. |
| XQuery | [SchemaXQueryCompiler.Compile](proposal/XQuery.cs.txt) returns the common SchemaQuerySession, capturing effective prolog/module context. Operation.Evaluate uses the captured XPath or XQuery executable. |
| XSLT | [SchemaXsltCompiler.Compile](proposal/Xslt.cs.txt) returns SchemaXsltExecutable with its common Session. Transform requires that session's actual borrowed operation and an explicit SchemaXsltInvocation. |
| Transform inputs | InitialMatchInput and GlobalContextInput are separately identified snapshot admissions; template/mode, raw-result and base-output settings are explicit. Dynamic context is subject to the same complete input-admission/resource policy. Empty admission sets permit source-free named templates. |

Core's proposal references only actual Core; API references Core/Runtime/API and the Core proposal. XQuery/XSLT producers reference the common proposal abstractions above their existing host layers. No concrete host dependency points down from API, Runtime or Core. See the explicit dependency graph in [input pins](evidence/INPUTS.json). Production implementation will need internal engine-owned session/executable creation bridges and host dispatch; no public arbitrary producer, authority attachment factory or friend grant is implemented here.

The exact decimal constructor checks supplied bounds before normalization and distinguishes invalid arguments from unsupported capacity. Its default value represents zero. The prototype demonstrates the selected representation rules, not these throwing declarations. Legacy narrowing must preserve wrong-kind checks and refuse overflow/inexact results. New BigInteger access recognizes existing decimal-backed integer values; an ordinary decimal is not made an integer merely because it has no fractional part. Existing annotation-string factory overloads remain descriptive and cannot mint authenticated assessment evidence. Already allocated BigInteger inputs do not enforce parsing memory limits.

## Finite profile and unresolved acceptance

The XSLT invocation here admits document roots as initial-match/global-context inputs. Arbitrary initial item/sequence selection, atomic global context and streaming are explicitly outside this proposed profile. They require Unsupported without fallback or implicit materialization. This is a concrete bounded all-host entry proposal, not a claim of complete XSLT invocation coverage or sufficient scope for every Braid use case. Owners must decide whether those broader inputs require signatures before freeze.

Actual effective-context capture must freeze or explicitly refuse mutable resolver/package capabilities, reject ambiguous legacy/native schema settings, bind after prolog resolution, and validate all dynamic input node graphs against the admitted snapshot set. XSLT Operation.Evaluate must refuse host misuse; transformations use the host executable. Raw result graphs remain borrowed; no automatic graph detachment or serialization roundtrip is supplied. Existing close/drain, compatibility and multi-source operation obligations remain.

[Ten new executable acceptance cases](ACCEPTANCE.json) are NotRun. Earlier acceptance cases and all 43 owner requirement rows remain pending; this shape compilation does not resolve them. Construction costs, private marker integration, actual arithmetic/parser/map-key/serialization/database routing, real typed transforms and consumer qualification remain separate implementation gates. Matching declaration shapes establishes neither field layout nor runtime behavior.

Next build an isolated all-host adapter that captures actual effective context and exercises the selected session/operation contract over the qualified binaries, including source-free and distinct-input XSLT cases. Keep public proposal and authenticated production integration claims separate. Broader XSLT profile decisions must be settled before accepting final API scope. C45, publication and API freeze remain pending.

Replay: `python -B docs/foundation-c45-revised-shape/tools/build.py`. Verify: `python -B docs/foundation-c45-revised-shape/tools/verify.py`. Full product unit/conformance gates were not rerun: only scratch declarations and metadata tooling were built.
