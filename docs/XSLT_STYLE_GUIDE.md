# Bosak.Exslt — XSLT & Fixture Style Guide

> **Bosak applicability (2026-10-11):** This guide was copied from Bosak.Exslt
> as an authoring reference. Bosak's worked-example stylesheets adopt its file
> header/change-history conventions, English comments, two-space indentation,
> and helper documentation of purpose, inputs, results, and edge cases. Document
> templates with their context, output, and preconditions as well. The invoice
> chapter is the first application of these conventions.
>
> The order-routing chapter adds an explicitly opted-in XPath40 planning expression,
> passed to a portable XSLT 3.0 stylesheet; source 1.1.1 also supplies a standalone
> version="4.0" stylesheet using the same fixtures. Helper comments apply to the planning
> expression as well.
>
> The EXSLT tier/migration rules, `src/` modules, `tests/cases/` layout,
> attribution paths, training harnesses, and `tools/check-docs.ps1` below describe
> **Bosak.Exslt**, not tools or directories in Bosak. Bosak's invoice fixtures live
> under `docs/xpath40-catalog/worked-examples/invoice-to-erp/` and are verified by
> `InvoiceReferenceGuideTests` in `tests/Bosak.Xslt.Tests/Documentation/`.
> Bosak's local coding and documentation obligations are in [AGENTS.md](../AGENTS.md).
>
> The first three worked chapters use portable XSLT 3.0 / XPath 3.1. Catalog instruction
> comparisons additionally include explicitly labeled version="4.0" draft examples
> and version="3.0" baselines. That declared compatibility is intentional; the
> Exslt-only version rule below is not a ban on documenting implemented 4.0 surfaces.

> **Kind:** Working companion — Last updated: 2026-10-06
> Rules for every stylesheet comment and test fixture in this repository.
> General documentation branding follows the Fytala Docs Kit contract in
> [`DOCUMENTATION_STYLE_GUIDE.md`](./DOCUMENTATION_STYLE_GUIDE.md); document
> sync obligations live in [`AGENTS.md`](../AGENTS.md) §7. Enforcement:
> `tools/check-docs.ps1` runs the mechanical checks; review enforces the rest.

---

## 1. XSLT File Rules

### 1.1 File Header

Every `.xsl` file begins with an XML comment before the `xsl:stylesheet` element
carrying at minimum these markers (see `src/strings.xsl` for the canonical shape):

```
AUTHOR, CREATE DATE, PURPOSE, SPECIAL NOTES, COPYRIGHT (Fytala), LICENSE (Apache-2.0)
```

`tools/check-docs.ps1` checks for the AUTHOR / PURPOSE / LICENSE markers in every
`src/**/*.xsl` file and that the file is well-formed XML. When modifying an existing
file, append a change-history entry with the current date, a bumped version, and a
brief note.

### 1.2 Function Doc Blocks

Every `xsl:function` that implements (or wraps) an EXSLT function is preceded by a
comment block covering:

1. **Purpose** — what the function computes, in one or two sentences.
2. **Parameters and return** — types and semantics, including EXSLT edge cases
   (e.g. `date:*` returning `NaN`/`''` on invalid input).
3. **Error/edge posture** — what happens on empty input, missing arguments,
   invalid values.
4. **Tier** — tier 1 wrappers state the native XPath 3.1 equivalent by name;
   tier 2 implementations note where semantics are exact vs. documented-divergent;
   tier 3 slots carry the terminating-message contract.
5. **Migration pointer** (tier 1) — the native form to use when modernizing.

Private helpers (`str:replace-scan`, `date:_parse-duration`) need shorter blocks:
purpose, parameters, and any non-obvious invariant.

### 1.3 Language Rules

- Standard XSLT 3.0 / XPath 3.1 + F&O only. No vendor extensions, no processor
  conditionals. Engine workarounds must be standards-compliant and noted in
  `SPECIAL NOTES` or the function's doc block as a "candidate core bug/gap".
- English identifiers and comments; `lower-case-with-hyphens` for EXSLT function
  names (they are fixed by the spec); `kebab-case` private helpers with a leading
  underscore for module-internal utilities.
- Two-space indentation in `.xsl` / `.xml` files (enforced by `.editorconfig`).
- `version="3.0"` on every `xsl:stylesheet`; declare namespaces on the stylesheet
  element and list non-output prefixes in `exclude-result-prefixes`.

## 2. Test Fixture Rules

- **Layout:** `tests/cases/<namespace>/<case>/` with `transform.xsl`, exactly one of
  `expected.xml` / `expected.txt`, `meta.json`, and optionally `input.xml`.
- **`meta.json` shape:**

```json
{
  "function": "math:min",
  "namespace": "http://exslt.org/math",
  "tier": 1,
  "source": { "project": "libxslt", "license": "MIT", "path": "tests/exslt/math/min.1" },
  "adaptations": "what was changed relative to upstream",
  "notes": "optional: divergences, engine-bug context"
}
```

  `source` is mandatory for imported fixtures (project, license, upstream path);
  hand-written cases use `"source": { "project": "hand-written", "license": "Apache-2.0" }`.
- **Provenance:** every imported fixture is recorded in its `meta.json` *and* in
  `tests/ATTRIBUTION.md` in Bosak.Exslt (project, license, upstream path;
  the attribution file carries the verbatim upstream license notice). Never fabricate
  attribution.
- **Divergence:** when a golden is re-goldened because the implementation
  deliberately diverges from libxslt, record it in the case's `meta.json` `notes`,
  `tests/ATTRIBUTION.md`, and
  `docs/COMPATIBILITY.md` in Bosak.Exslt — in the same step. A golden
  may only be *changed* (never deleted) this way.
- **Include path:** case transforms include library modules via
  `../../../src/<module>.xsl` (three levels up from the case directory).
- **Training sessions** (`training/NN-slug/`) are fixtures for learners, not golden
  cases: `case/meta.json` uses the same shape with
  `"source": { "project": "hand-written", "license": "Apache-2.0" }` and a `notes`
  field marking the case as training material. Session transforms (`starter/`,
  `solution/`) are **self-contained** — they never include `src/`; the library is
  referenced only in lesson prose, as read-along material. Training cases are
  exercised by `training/TrainingTests/`, never by the golden harness.
- **XPath training sessions** (`training/xpath/NN-slug/`) follow the same rules
  with `starter|solution/exercise.xpath` (exactly one pure XPath 3.1 expression,
  no XSLT instructions or extension functions) and text goldens
  (`case/expected.txt`). The harness renders results as a token stream
  (`value | value | …`, `()` for the empty sequence).

---

*Last updated: 2026-10-06*
