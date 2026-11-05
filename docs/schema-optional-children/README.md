# Optional decimal children: scope checkpoint — 29 October 2026

The next native Schema slice is scoped and pinned before implementation. Optional children are **not yet supported**. The [adoption record](OPTIONAL-DECIMAL-ADOPTION-2026-10-29.md) selects exact dated XSD/XPath/XDM Recommendations and intentional finite restrictions; the [implementation plan](OPTIONAL-DECIMAL-PLAN.md) defines matching, assessment identity, assertions, copies and acceptance controls.

The owning editable documents are in the Schema feature worktree's `docs` directory; the copies here are review snapshots. Core base is `a163ff11895038ceb2b718ecfba77383384a3b35`; Schema base is `766eff7a77ee08e9c38cf479994e94eaf232d242`. Earlier [bridge API-review evidence](../schema-document-bridge/api-review/README.md) remains sealed.

## Decisions prepared

- Admit only optional local decimal particles, with one enclosing sequence and maximum occurrence one; retain exact values and existing resource bounds.
- Keep particle occurrence separate from declaration identity. Retain all original declarations, but create instance states only for actual present elements.
- Apply effective Boolean conversion to assertions: an absent-child comparison is false, rather than an unsupported Boolean-payload access.
- Preserve original state through whole-document/copy paths and refuse requests to project an absent child. Root atomization remains FOTY0012.
- Propose a v2 optional profile and Schema 0.6 development line for the future implementation; required-only v1 stays compatible. A bounded core empty-root whitespace fix also proposes patch candidate 1.34.1 and fresh full core/gate runs. No version change is made now.

## Qualification of the scope

Thirteen unique isolated observations confirm the current required-only success, five optional-spelling refusals, six engine empty-comparison/logical outcomes, and the empty-root whitespace binding gap. These do not implement or qualify optional assessment. A fresh complete Schema baseline passes 204 tests. Complete core/gate results are inherited from the source-identical reviewed checkpoint: 24,209 core passes, QT3 31,397/0/424, qt4 8,941/0/1,918, 230 sets. No fresh full-core/gate run is claimed for documentation and scratch diagnostics.

See [results](RESULTS.json), [source revisions](SOURCES.json) and the evidence directory. No product source, package, version, consumer pin, fixture, admission filter or gate baseline changes. The original scope was captured before commit; see the [current delivery checkpoint](delivery/README.md). Next: implement the bounded plan with before/after regressions and original-state/copy controls.
