# Count assertion implementation plan — 29 October 2026

This is a future finite grammar slice, not implemented count assertion support. Begin from the reviewed repetition checkpoint and the [exact adoption record](COUNT-ASSERTION-ADOPTION-2026-10-29.md). Current versions remain Schema 0.7/core 1.34.1; the source's host function support does not imply native assertion admission.

## Intended behavior

An admitted assertion `count(amount) eq 2` validates two actual amount elements, including equal-valued occurrences, and fails for zero or one. `count(amount) eq 0` can validate absence, unlike `amount eq 0`. `count(amount) eq count(shipped)` compares two scalar cardinalities. A bare operand `count` still selects a declared child named count; `count(count)` invokes the function on that child selection.

Count does not replace native assessment: malformed decimals yield Invalid and unrepresentable valid decimals yield Unsupported before assertions, even when their node count would satisfy the expression. Occurrence counts/order/budgets, namespace-free declarations, original state, copy semantics and no-typed-value root behavior remain unchanged.

## Implementation changes

1. **Extend only Operand admission.** Add the finite `CountCall := count ( DeclaredChildName )` alternative. Recognize it when the current token is count and the next token is an opening parenthesis; otherwise retain normal decimal/name parsing. Require a single declared child name and a closing parenthesis. No prefixed function, alias, nested call, wildcard, predicate, literal/sequence argument or arbitrary path. Use XML whitespace and the existing tokenizer bounds; function parentheses count toward the existing total depth bound. Do not loosen comparison/operator whitespace or broaden other grammar accidentally.
2. **Track admitted features during parsing.** Return a small internal grammar/admission result recording whether a count call was actually consumed. Only successful full parses may supply this marker. Pass that marker through compiled assessor construction to select v4; no text Contains/regex marker or relabeling of legacy schemas. Bare count children preserve their prior structural profile. Multiple assertions use v4 when any admitted assertion contains a count call.
3. **Compile the validated original expression under XPath 2.0.** Use the existing standard count implementation/default function namespace. Do not remove prefixes, rewrite unsupported syntax or accept a custom function by name alone. The first slice rejects every prefixed call before invoking the general compiler. Keep the parentless native assertion candidate, typed present-child annotations, effective Boolean conversion and narrow error policy. Do not replace real expression evaluation with a synthetic count predicate.
4. **Preserve validation order and state.** All native structural/lexical/exact-value checks remain before assertion evaluation. Count sees actual child nodes, ignores comments/whitespace by name selection and does not deduplicate values/states. Uncounted multi-item value comparisons still yield assertion Invalid; count produces a scalar, but does not mask errors on another operand. Preserve actual and/or short-circuit semantics.
5. **Version and evidence.** Advance only the Schema feature minimum to 0.8 and select v4 for admitted count schemas. Keep core 1.34.1 unless a separately recorded correction becomes necessary. Update C# histories/XML comments, current grammar guide, registry/status, example and handover. The existing `count(a) eq 2` refusal case is the one explicit expectation adoption; preserve originals and add before/after feature tests. No merge, tag, package, public overload or product consumer upgrade.

## Acceptance matrix

| Boundary | Required controls |
|---|---|
| Values/types | Cardinality 0/1/2/16 as xs:integer; equal decimal values still count twice; scalar count comparisons with integer/decimal constants, negative/fractional constants and another count; absence versus bare absent comparison. |
| Names/calls | Declared child count as bare operand; count(count); function call spacing/XML whitespace; count on another valid declared child; lowercase exact function spelling; prefix/alias/rebound namespace refusal without changing schema type namespace handling. |
| Syntax/refusal | Empty/two/nested arguments; unknown child; predicates, wildcard, path, literal/sequence/variable argument; sum/avg/exists/error; general comparison/arithmetic; missing closing parenthesis; no partial-parse acceptance. |
| Bounds/features | Character/token/depth boundaries, including call-parenthesis depth; multiple count calls in and/or; count in one of several assertions; no v4 marker on a refused parse or a bare count child; original v1/v2/v3 schemas retain profiles. |
| Native validity | Count would pass but child lexical value is invalid/unrepresentable; missing required/order/excess occurrence failure wins; comments/PIs/root whitespace do not count; cancellation and aggregate occurrence/input bounds remain. |
| Error/copy | Counted multi-node operand is scalar; another bare multi-node operand still produces assertion Invalid; no new exception swallowing; real whole-document XPath/XSLT copy/typing/identity and source immutability controls on a count-validated document. |

The baseline scratch observations show current schema grammar refusal, actual host count integer behavior on native 0/1/2/16-node documents, a bare count-name collision control, and native value failure despite host count success. They are scope diagnostics, not native count-schema acceptance.

## Qualification and delivery

Run the entire Schema suite, a real count-assertion assessor example, actual copy/error-policy controls, source-override build and strict docs checks. Capture the original refusal/new expectation transition, raw receipts and exact changed source identities. Core/harness/fixture/configuration must be verified source-identical before inheriting 24,214 core passes and exact QT3 31,397/0/424 / qt4 8,941/0/1,918, 230 sets. Any core production change requires its own adoption/version disposition and fresh full core/gate qualification.

Owning source remains the persistent Schema feature worktree; core `docs/schema-count-assertions` holds scope snapshots, normative representation locks and diagnostics. The user authorized review/checkpoint then scope preparation; count implementation is the next step. Current production source/versions and tests are unchanged by this planning checkpoint. No API freeze or release is inferred.
