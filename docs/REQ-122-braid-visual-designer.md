# REQ-122 — Bosak.Braid: Visual XSLT Transformation Designer

**Status:** Accepted (feasibility assessed, name decided — awaiting scheduling)
**Date:** 2026-10-09
**Track:** Post-1.0, separate commercial repo/product (companion to Bosak.Schema)
**Name decision:** **Bosak.Braid** — package `Bosak.Braid` on the reserved NuGet
prefix. A braid is two strands (XSLT text, visual graph) interwoven around one core
(the transformation model). Tagline: *"Two views. One transformation."*

---

## 1. Goal

A commercial visual designer for XSLT transformations in which the **visual graph and
the XSLT 3.0/4.0 source text are two representations of the same transformation
model** — not two independent systems that translate into each other. Bidirectional
by construction:

- **Visual → XSLT:** construct a transformation graph, serialize valid XSLT 3.0/4.0.
- **XSLT → Visual:** load existing XSLT, reconstruct the transformation graph.
- **Continuous synchronization:** editing either representation updates the other
  without information loss.

## 2. The core architectural decision

**One canonical model, two projections (projectional editor).**

The transformation graph is the *only* artifact. The XSLT text pane is a rendered
view of the model; the diagram pane is another rendered view of the same model.
This is the decision that makes the product feasible — every tool that kept text and
diagram as separate artifacts with bridges between them (visual → code generation,
code → reverse engineering) failed at round-trip fidelity and drifted.

Consequences:

| Direction | Mechanism | Fidelity |
|-----------|-----------|----------|
| Visual → XSLT | Graph walker serializes the model | Lossless by construction (generated from the model) |
| XSLT → Visual | Parse with the **existing Bosak front-end** (parser / AST / `Stylesheet` model) | Lossless where the visual vocabulary covers the construct; verbatim fallback otherwise |
| Continuous sync | Both panes edit the same in-memory model; each re-renders on change | Nothing to merge — there is one artifact |

The visual graph **is** the compiler's model plus layout metadata. No new XSLT
parser is needed: `Bosak.Xslt`'s stylesheet model, `PatternCompiler`, and the
`Bosak.XPath.Parser` AST are consumed directly.

## 3. Feasibility — the three genuinely hard parts

### 3.1 Arbitrary XPath has no visual idiom

XSLT is Turing-complete; a predicate like
`//a[contains(@id, $x)]/b[@c > avg(//d)]` has no sensible box-and-arrow form.

**Mitigation — expression nodes.** Nodes in the graph that contain a full XPath
text editor with the Bosak language server's completion/diagnostics/hover inside
(LSP machinery already exists and parses on keystroke). The graph expresses
*structure* (templates, modes, flows, joins, source/target shapes); expressions
remain text. This is how every successful visual data tool works (MapForce, Kettle);
users accept it readily.

### 3.2 Constructs with no visual metaphor

`xsl:iterate`, accumulators, `use-when`, template priorities, packages, character
maps, etc.

**Mitigation — fallback container nodes.** A "raw XSLT block" node holds an
arbitrary subtree verbatim. Import never fails: anything the visual vocabulary
cannot express lands in a container node, remains fully editable, and serializes
back faithfully. Losslessness is preserved by design; visual vocabulary coverage
grows incrementally, driven by usage telemetry.

### 3.3 Transient invalidity while typing XSLT

While the user types, the text is often unparseable; there is no clean model to
project to the diagram.

**Mitigation.** Keep the **last-good model**; show parse errors as an overlay on
the text pane; project only valid states to the diagram, with a clear
"diagram updates when errors are resolved" indicator. Trivia (comments, original
formatting) is either preserved via per-node source ranges or reformatted on
serialize, as a user preference. These are UX-tuning problems, not architecture
problems.

## 4. Reuse inventory (the Bosak head start)

- **Complete XSLT 3.0 front-end** — hand-written parser, full pattern compiler,
  strict error codes: the XSLT→Visual direction consumes the same model the
  compiler builds. A competitor built on Saxon fights someone else's AST; Bosak
  owns the whole pipeline (parse → AST → IR → VM), and the AST is exactly what the
  graph needs.
- **Language server (LSP)** — parse-on-type, diagnostics, outline/semantic tokens:
  the same engine powers the embedded expression editors and the text pane.
- **4.0 wave** — the designer targets XSLT 3.0 now and 4.0 surfaces
  (`xsl:array`, `xsl:switch`, etc.) come along behind the same version gate.
- **Commercial machinery** — Bosak.Schema's licensing (BSK offline keys, tiers,
  grace), Stripe issuance pipeline, and support contracts apply unchanged.
- **Conformance credibility** — QT3 31,142/0/679, XSLT sweeps at 100.0%/near-100%
  mean the imported model is *the* correct reading of the stylesheet.

## 5. Phasing

| Phase | Deliverable | Notes |
|-------|-------------|-------|
| **P1** | Designer as generator + importer: build graphs, emit XSLT 3.0; import existing XSLT into graph + fallback nodes (one-shot, no live sync) | Proves the model, serializer, and parser reuse; sellable early; bulk of the work (graph data model, layout engine, serializer) lands here — realistically months |
| **P2** | Live dual view: text pane as a live projection of the model; diagram edits update text and vice versa (reparse on change, last-good overlay) | The core market differentiator |
| **P3** | Polish: incremental reparse for large stylesheets, expression-node editor depth, layout persistence, 4.0 surface vocabulary | Scale and feel |

UI stack: **WPF or Avalonia** (Avalonia if cross-platform from the start).

## 6. Market differentiation

Altova MapForce generates XSLT but does not round-trip it — import loses hand-written
parts and the views drift. Stylus Studio and Oxygen are editor-first with visual
assists. A **true single-model bidirectional designer does not exist in this
market**. The pitch — *"draw it or type it, it's the same thing; import your
existing XSLT without loss"* — is only deliverable by an engine owner.

**Hard product constraint:** designer output must remain plain standard XSLT — no
vendor extensions — or the round-trip promise that defines the product dies.

## 7. Risks

- **UX complexity** is the real cost center (expression-node ergonomics, sync
  indicators, layout stability); mitigated by the P1→P2 phasing.
- **Large-stylesheet performance** — mitigated by incremental reparse in P3.
- **Scope creep in visual vocabulary** — mitigated by fallback container nodes;
  coverage is additive forever.
- **Name/trademark** — verify `Bosak.Braid` package ID, domain, and trademark
  registers before public launch (NuGet `Bosak.` prefix already reserved).

## 8. Open questions (owner decisions, not blockers)

1. WPF-first or Avalonia (cross-platform) for the UI shell?
2. Standalone desktop app, or embedded panel in the VS Code extension (or both)?
3. Does Braid reuse the Bosak.Schema license tiers as-is, or a separate product key
   family (e.g. `BRD1`)?
4. Target: which XSLT 4.0 instructions get visual idioms in P1 (recommend:
   `xsl:array`/`xsl:switch` only; everything else falls back)?
