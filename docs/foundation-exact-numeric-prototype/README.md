# Exact numeric representation prototype

31 October 2026. Isolated representation/accessor experiment following the [numeric and host characterization](../foundation-c45-domains-hosts/README.md). No engine source, version, public API, baseline or owner pin changed. [Specification basis](SPEC-BASIS.md) preceded execution. The [prototype source](tools/Prototype.cs.txt) compiles only into a scratch executable.

Two guarded processes produce identical [results](evidence/run-0.json) and [repeat results](evidence/run-1.json): 31 passing checks, zero build warnings/errors, a 48-byte model carrier and a 48-byte actual XdmValue. Actual loaded Core bytes match the qualified cohort. All ten qualified engine DLL hashes remain unchanged; see [pins](evidence/INPUTS.json). Earlier measurements before the binding observer and final narrowing boundary review are retained separately. The final review adds decimal endpoints and a 128-digit overflow refusal, ensuring legacy narrowing rejects range before attempting bounded formatting.

The model stores small integers inline and large integers or exact decimals in a distinct payload holder alongside an optional evidence token. The token is an ordinary object: this does not implement issuer authentication, ownership, liveness or production private evidence. Matching size alone does not prove layout or ABI compatibility with production XdmValue.

Exact decimals store a BigInteger coefficient and nonnegative scale. Trailing zeros normalize; zero has scale zero; numeric equivalents compare and hash equally. Supplied digit/scale capacity is checked before normalization. Default limits and invalid arguments reject separately from CapacityException. Formatting checks the final output length before padding. Legacy integer narrowing is checked; decimal narrowing verifies an exact roundtrip so a successful but rounded decimal parse is refused. Tests include large integers, Int64 endpoints, positive/negative/zero decimals, normalization, scale/digit/output refusal and inexact legacy narrowing.

## Measured allocation costs

Warmed, single-thread allocated bytes per operation, 10,000 iterations per measurement in each process. Input BigInteger, decimal and evidence token already exist. These are allocation measurements, not throughput or consumer budgets.

| Model operation | Without token | With token |
|---|---:|---:|
| Inline integer creation, including quota check | 0 | 0 |
| Selected 40-digit integer creation, including quota check | 168 | 168 |
| Cached large integer read | 0 | 0, including token read |
| Wrapping a prebuilt exact decimal | 72 | 72 |

Large-integer creation includes digit-count validation using a temporary decimal string, boxing and holder allocation. Decimal wrapping excludes coefficient construction, normalization and formatting. A caller can allocate an oversized BigInteger before this interface receives it; the prototype limits do not bound upstream parsing memory. No latency, concurrent-use, arithmetic or end-to-end workload claim follows from these numbers.

## Disposition and next step

The distinct payload/token design is viable enough to continue declaration review while preserving the inline fast path. It does not select final production storage or numeric quotas. The 168-byte construction path needs improvement or explicit acceptance against actual workloads before promotion; a bounded lexical parser must enforce limits before allocating full coefficients. Existing private marker handling, decimal-backed integer paths, equality, hashing and legacy accessors still require a production integration design.

Next compile the revised isolated C45 numeric and all-host declarations, using these exact representation/narrowing rules and explicit digit/scale limits. Keep ordinary host semantics unchanged until separately reviewed implementation and full engine qualification. Parser/arithmetic/comparison/map keys/serialization/database integration, typed transforms, multi-source leases and owner acceptance remain pending. C45 and API freeze are not accepted by this experiment.

Replay with `python -B docs/foundation-exact-numeric-prototype/tools/run.py`; verify sealed evidence without executing .NET using `python -B docs/foundation-exact-numeric-prototype/tools/verify.py`. All deliberate refusals are caught; the runner uses scoped process error mode and a timeout. Full solution/conformance gates were not rerun because product code and binaries did not change.
