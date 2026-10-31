# Next context-declaration slices

28 October 2026. C24 is a diagnosis; routing guards and all current baselines remain intact.

## C25 - Duplicate library declaration error

Record immutable pre-implementation adoption of XQuery 3.1 REC-20170321 section 4.17. Correct only the duplicate context-item declaration error from XQST0113 to XQST0099. Preserve XQST0113 for library initializers/defaults. Add positive and negative parser/public module controls, including typed declarations and comments; audit existing duplicate expectations before updating any test. Do not change fixtures, routing, dependency filters, GatedSets, AGENTS totals or schema capability. Run relevant regressions, full solution and current gates; preserve C24 evidence.

## Main-module implementation after C25

Adopt the pinned grammar and evaluation rules before implementation. Represent main declarations explicitly through parsing, compilation and execution. Cover typed singleton results, supplied external items/defaults, singleton focus, global-variable forward references and dependency-cycle errors. Preserve library restrictions and distinguish ItemType from SequenceType grammar. C24's 31 originals and 18 controls provide the first regression cohort; retain every original error alternative, including the ambiguous self-focus tests.

Then audit the 13 resource/module-bearing QT3 boundaries and imported-module type contracts. Shared context and global initialization require more than a parser-only acceptance change. Keep schema capability and Bosak.Schema XSD 1.1 ownership explicit.

## Admission only after qualification

Rehearse the actual normal runner only once the affected semantics and wider boundaries are green. Any removal of a QT3 guard or skip must have exact full-gate/skip-delta evidence and its own owner-approved baseline proposal. Qt4 remains XPath-only; XQuery-only declarations do not become applicable through diagnostic execution. No release is part of these scopes.
