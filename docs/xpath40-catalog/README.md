<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak XPath 4.0 feature catalog">
  <br><br>
  <h1>The Bosak XPath/XSLT 4.0 Feature Catalog</h1>
  <p>Structured reference for every XPath/XSLT 4.0 syntax decision in Bosak — where each feature comes from, why it exists, what it replaces, and what it costs</p>
</div>

<!-- The Bosak XPath/XSLT 4.0 Feature Catalog -->
<!-- Living document: one file per feature; index updated in the same PR as any entry. -->

> **Purpose:** A catalog, in the spirit of the Design Patterns template (Gamma, Helm, Johnson, Vlissides, pp. 6–7), of every XPath/XSLT 4.0 feature Bosak ships, defers, or rejects. **Audience:** engine users choosing which 4.x idioms to adopt, Bosak.Braid (which syntax it may generate per compatibility level), and contributors extending the engine. **Living document:** last updated 2026-10-10. **Governing REQ:** [REQ-126](../FEATURE_REQUESTS.md); adoption plan and stability tiers: [REQ-118 dossier](../REQ-118-xpath-xslt-40.md); experimental track: [REQ-123](../FEATURE_REQUESTS.md).

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
13. **Sample Code** — one expression per form, with expected result or error code.
14. **Performance Notes** — cost class (zero-cost desugar / small constant overhead / hot-path relevant), plus measured deltas **only** when they carry revision + date from `benchmarks/Bosak.Benchmarks` (BenchmarkDotNet). Ad-hoc timings do not belong here; the harness owns numbers.
15. **Known Uses** — the qt4tests sets that pin the behavior and their gate status.
16. **Related Features** — cross-links to other catalog entries.

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

*Last updated: 2026-10-10 — REQ-126 scaffold: README + 4 seed entries (frozen, experimental, deferred, rejected each represented).*
