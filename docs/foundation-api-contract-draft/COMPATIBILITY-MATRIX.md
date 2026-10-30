# F140-01 — Planned compatibility cases

30 October 2026. This is an executable-work specification, **not executed compatibility evidence**. The [case definitions](compatibility-cases.json) contain each procedure, required evidence, owner and linked FG decision IDs. The [member draft](evidence/member-manifest.json) links applicable cases to exact baseline entries. FC-17/18/19 also apply as global release/manifest gates even where a specific member has no individual association.

| ID | Boundary | Responsible owner/input | Execution |
|---|---|---|---|
| FC-01 | Existing provider source compatibility | Core + XmlDb/Schema | NotRun |
| FC-02 | Unchanged consumer binaries | Core + both owners | NotRun |
| FC-03 | Enum and optional argument constants | Core + XmlDb | NotRun |
| FC-04 | Managed value layout and carriers | Core + both owners | NotRun |
| FC-05 | Node identity and order | XmlDb + Core | NotRun |
| FC-06 | Typed values and atomization | Schema + Core; XmlDb untyped baseline | NotRun |
| FC-07 | Lazy ownership and early termination | Core + both owners | NotRun |
| FC-08 | Nested and escaped values | Core + both owners | NotRun |
| FC-09 | Single-pass sequencing | Core + XmlDb where implemented | NotRun |
| FC-10 | Compile/evaluate environment and caches | Core + both owners | NotRun |
| FC-11 | Resource authority and cancellation | Core + both owners | NotRun |
| FC-12 | Assessment preserve/strip/copy | Schema + Core | NotRun |
| FC-13 | Component universe and compatible views | Schema + Core | NotRun |
| FC-14 | Ordinary untyped persistent XML | XmlDb | NotRun |
| FC-15 | Errors and refusals | Core + both owners | NotRun |
| FC-16 | Concurrency and disposal isolation | Both owners + Core | NotRun |
| FC-17 | Package and assembly identity | Core/release + both owners | NotRun |
| FC-18 | Full tests and frozen engine gates | Core | NotRun |
| FC-19 | API manifest and scope drift | Core | NotRun |
| FC-20 | Mixed draft and established members | Core + both owners | NotRun |

Core supplies source/ABI fixtures and a named candidate; owners execute actual consumer qualification and return exact revisions, packages, commands/results and unresolved gaps. Do not replace actual Schema/XmlDb evidence with synthetic provider success or the audit tool's ten checks. Existing product pins, original snapshots/stores and sealed historical receipts are preserved.

Positive, edge/error/refusal and lifetime cases must be tied to the accepted supported profile before implementation. In particular, missing enumerator disposal is an open ownership design question, not proof of an existing leak or authorization to add a required interface member. Draft-member exclusion is not permission to remove existing public symbols. Full XSD 1.1 validation belongs to Schema, while ordinary XmlDb XML remains independent of optional schema/JSON persistence.

FC-18 preserves the currently adopted QT3 and qt4 counts and exact skip-name/reason sets. Any promotion or semantic change first needs its own immutable specification adoption and owner disposition. All resultReceipt fields are null; source/package/binary/lifetime qualification and freeze are still pending.
