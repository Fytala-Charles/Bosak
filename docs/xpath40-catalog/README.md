<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak XPath 4.0 feature catalog">
  <br><br>
  <h1>The Bosak XPath/XSLT 4.0 Feature Catalog</h1>
  <p>Structured reference for every XPath/XSLT 4.0 syntax decision in Bosak — where each feature comes from, why it exists, what it replaces, and what it costs</p>
</div>

<!-- The Bosak XPath/XSLT 4.0 Feature Catalog -->
<!-- Living document: one file per feature; index updated in the same PR as any entry. -->

> **Purpose:** A catalog, in the spirit of the Design Patterns template (Gamma, Helm, Johnson, Vlissides, pp. 6–7), of every XPath/XSLT 4.0 feature Bosak ships, defers, or rejects. **Audience:** engine users choosing which 4.x idioms to adopt, Bosak.Braid (which syntax it may generate per compatibility level), and contributors extending the engine. **Living document:** last updated 2026-10-11. **Governing REQ:** [REQ-126](../FEATURE_REQUESTS.md); adoption plan and stability tiers: [REQ-118 dossier](../REQ-118-xpath-xslt-40.md); experimental track: [REQ-123](../FEATURE_REQUESTS.md).

## Direction: a book and a working reference

This catalog is intended to develop into a book or reference guide. Readers should be able to follow chapters to learn how features solve document-processing problems, or look up a single entry while implementing a transformation. The current four seed entries establish the structure; they do not yet constitute a complete guide.

Examples must reflect real work. Use synthetic but plausible fixtures: supplier invoices mapped into an ERP format, order lines grouped into a reconciliation report, XML manuals published as HTML, or transaction amounts accumulated into running balances. State the business problem before introducing the syntax, and explain why the feature helps. Publishing examples should preserve mixed content and cross-references; mapping examples should handle namespaces, repeated records, optional fields, and conversions.

A worked example must include representative input, a complete expression or stylesheet with its required context, expected output, the compatibility level, and at least one meaningful edge case. Declare simplifications, such as a reduced invoice vocabulary; a small fixture must not imply compliance with an industry standard. Pair new syntax with the earlier idiom where a migration comparison helps. Short syntax probes can supplement the worked example.

Worked-example stylesheets must be understandable independently of the chapter:
include a file contract and change history, helper purpose/parameter/return/edge
blocks, and template context/output/precondition blocks. Explain business choices
and non-obvious invariants rather than narrating each instruction. See the
[XSLT style-guide applicability note](../XSLT_STYLE_GUIDE.md) and the annotated
[invoice stylesheet](worked-examples/invoice-to-erp/invoice-to-erp.xsl).

Keep reader-facing explanations before engine implementation details. Chapter introductions and cross-links will connect the individual entries into a learning path; the feature index remains available for quick lookup. Runnable examples must meet REQ-126's engine-verification requirement before their results are presented as verified.

## Start with a worked chapter

[Supplier invoices to ERP XML](worked-examples/invoice-to-erp/README.md) is the first complete case: a 3.0 stylesheet, realistic inputs, expected principal/exception outputs, and seven executable checks. It includes a separate verified 3.1/4.0 map-destructuring comparison. The [chapter plan](worked-examples/README.md) records XML-manual publishing as the next case.

## 1. How to read this catalog

Each feature is one file, written against the template below. Fields may not be omitted, but may be *empty by declaration* — see the field contracts.

### 1.1 Entry template

1. **Name and Classification** — feature name + tier (**Frozen 4.0** / **Experimental** / **Deferred** / **Rejected–Superseded** / **Rejected–Never Existed**), the spec section, and the REQ slice/PR that landed it (traceability — every entry links back to its delivery).
2. **Intent** — one paragraph: what the construct does.
3. **Also Known As** — spec PR/issue numbers and former names. The 4.x drafts rename things; this field is the antidote.
4. **History / Supersedes** — which 1.x–3.x idiom this replaces, with a migration pair. If the feature is genuinely new, declare: *"None — new capability; 3.1 workaround: …"* — emptiness must be informative, not accidental.
5. **Motivation** — the problem in 4.x terms: why the pre-existing idioms were not enough.
6. **Language Alignment** — prior art in other languages (F#, Haskell, JS, Python…), **including semantic-drift warnings** where XPath deliberately differs from the language it borrowed from (argument position, null-vs-empty, renames).
7. **Applicability** — when to use it; when *not* (library code targeting 3.1, hot paths, …).
8. **Structure** — the grammar fragment or syntax shape.
9. **Participants** — how Bosak models it: parser flag, AST node(s), opcodes, version-gate entry.
10. **Collaborations** — interactions with other features (e.g., destructuring × typed-let coercion × lexical scoping).
11. **Consequences** — benefits *and* liabilities, including any deliberate breaking change.
12. **Implementation** — files, versions, and where the version gate rejects (XPST0003 at 3.1, XPST0017, …).
13. **Sample Code** — a worked real-world case with input, complete code, required context, expected output, compatibility level, and a meaningful edge case; syntax probes supplement the case and cover each form.
14. **Performance Notes** — cost class (zero-cost desugar / small constant overhead / hot-path relevant), plus measured deltas **only** when they carry revision + date from `benchmarks/Bosak.Benchmarks` (BenchmarkDotNet). Ad-hoc timings do not belong here; the harness owns numbers.
15. **Known Uses** — the qt4tests sets that pin the behavior and their gate status.
16. **Related Features** — cross-links to other catalog entries.

For book-style reading, begin with Intent, Motivation, Applicability, and Sample Code, then use History, Consequences, and Related Features to evaluate alternatives. Participants, Collaborations, and Implementation provide a separate contributor reference. Authors should open with a practical scenario and link its runnable case before presenting engine details. The numbered fields remain stable for traceability; a chapter can introduce them in the order that best teaches the task.

### 1.2 Compatibility tiers

| Tier | `XPathCompatibility` | Meaning |
|------|----------------------|---------|
| **Frozen 4.0** | `XPath40` (40) | Stabilized draft features. Static XPST0003/XPST0017 at `XPath31`; legal at `XPath40`. |
| **Experimental** | `XPath40Experimental` (50) | Not-yet-stabilized additions (post-June-2026 churn-risk sections). XPST0017 at *both* 3.1 and frozen 4.0. Semantics may change as the draft moves. |
| **Deferred** | — | Spec sections still moving; deliberately not implemented. Entries exist so the decision is recorded. |
| **Rejected** | — | Superseded renames and shapes that never existed; implemented old shapes are a defect. |

The 3.1 default is untouched by every tier: consumers on `XPath31` see zero change.

### 1.3 Structural rules

1. One feature = one file, named short-kebab (no numbering — the index owns ordering).
2. Every entry links back to its REQ slice/PR (field 1).
3. The index table (§2) is updated **in the same PR** as any entry add, move, or tier change. No exceptions.
4. Entries are never deleted when a feature is rejected or superseded — the file moves to `rejected/` and the index records the disposition.

## 2. Index

### Operators & syntax
| Feature | File | Tier | Spec | Since | Supersedes |
|---------|------|------|------|-------|------------|
| Destructuring `let` | [operators-and-syntax/destructuring-let.md](operators-and-syntax/destructuring-let.md) | Frozen 4.0 | XPath 4.0 §4.14 | 1.1.0-dev (PR #129) | positional `$seq[1]` / `map:get` idioms |

### Functions
*(to be distilled — fn:scan seeded below; fn:atomic-equal, fn:sort-with, CSV, element↔map, JNode accessors pending)*

| Feature | File | Tier | Spec | Since | Supersedes |
|---------|------|------|------|-------|------------|
| `fn:scan` | [experimental/fn-scan.md](experimental/fn-scan.md) | Experimental | F&O 4.0 §2.5.15 | 1.1.0-dev (REQ-123 S1) | hand-threaded fold state |

### Type system
*(to be distilled — enums, choice item types, structural records, §3.4.2 coercion pending)*

### Data model
*(to be distilled — JNode pending)*

### XSLT instructions
*(to be distilled — `xsl:note`, `xsl:if` then/else, `xsl:array`, `xsl:switch`, separators, `xsl:map` select/duplicates pending)*

### Deferred
| Feature | File | Spec | Reason |
|---------|------|------|--------|
| Method-call operator `=?>` | [operators-and-syntax/method-call-operator.md](operators-and-syntax/method-call-operator.md) | XPath 4.0 §4.22.3 | Semantics changed 2026-06-25 (Issue 2219); do not build yet |

### Rejected / superseded
| Feature | File | Disposition |
|---------|------|-------------|
| Mapping arrow `=>>` | [rejected/mapping-arrow-gtgt.md](rejected/mapping-arrow-gtgt.md) | Renamed to `=!>` before stabilization; Bosak never shipped the old shape |

## 3. Benchmark provenance

Measured performance claims in this catalog must cite `benchmarks/Bosak.Benchmarks` (BenchmarkDotNet) output and carry the git revision + measurement date. Qualitative cost classes (zero-cost desugar / constant overhead / hot-path relevant) need no citation but must be stated as expectations, not measurements. See field 14 contract in §1.1.

*Last updated: 2026-10-11: first invoice chapter and seven executable checks supplied; book-style reading path and publishing chapter plan recorded. Feature coverage remains incomplete.*
