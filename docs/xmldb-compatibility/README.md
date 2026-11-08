# Isolated XmlDb compatibility qualification — 29 October 2026

The actual committed XmlDb Xdm adapter builds against current core 1.34.1 without a production source fix. All **27 existing Xdm tests pass on both the pinned 1.26.2 baseline and current core**, with identical test identities. A bounded diagnostic records **26 differential query pairs, seven exact persisted XML node-kind mappings and snapshot identity/lifetime controls per version**. All 44 diagnostic observations agree across versions; no persistent-versus-memory query difference was observed.

## Source selection and isolation

Consumer source is committed XmlDb `3f722daf6ae38f0a5b67de1c678f3175768b28dd`; core is `2e33d3b7c02eb20a1bfe757a73bb78ebb034c980`. The original consumer has unrelated uncommitted storage/enrollment work, explicitly excluded. Two ignored archive workspaces contain identical committed Core/Storage/Xdm and existing Xdm test sources. Selected source hashes are verified against `git archive`; [selection receipt](evidence/source-selection.json) records the original status. No sibling source, dependency, store or working-tree file was edited.

The baseline uses original exact 1.26.2 packages. Candidate `Directory.Build.targets` removes only the three relevant engine package references and supplies current source project references. Original project files, linked project-under-test XML and test bodies stay unchanged. Runtime deps receipts confirm 1.26.2 versus 1.34.1. `MinVerSkip=true` suppresses archive-parent Git version discovery; no product version is assigned by this harness.

## Observations

| Boundary | Result |
|---|---|
| Actual adapter/source tests | 27/27 baseline and 27/27 candidate; existing query, namespaces, mixed content, identity, deferred lifetime and transactional snapshot cases remain green. Storage/Core compile transitively; their full suites were not run. |
| Differential queries | 26 per version: element/attribute/text atomization and type tests, sum/arithmetic/value and general comparisons, invalid numeric/cardinality errors, presence, axes, ordering, identity and duplicate elimination. Both provider outcomes match within each version, including errors, and both versions agree. |
| Persisted kinds | All seven StorageNodeKind values exactly match engine XML kinds; no persisted renumbering or format migration. |
| Snapshot identity | Repeated wrappers in one snapshot are identical; independent snapshots remain distinct. |
| Disposal | Node access, beginning deferred enumeration, continuing enumeration, and nodes retained in map/array results all raise ObjectDisposedException after the owning snapshot closes. The independent live snapshot remains usable. |
| Process bounds | Final baseline/current probes each finish under an external 90-second watchdog; no timeout. |

## Receipt corrections and limits

Initial restore attempts failed with NU1900 because sandbox networking could not retrieve NuGet vulnerability data. Network-enabled retries restored and ran green without changing warning policy or disabling audit. Those original failures are preserved as environment receipts, not compatibility defects.

The first diagnostic harness incorrectly treated an unboxed sequence enumerator as IDisposable; the second used a nonexistent map accessor. Both compilation receipts are retained. An initial runtime diagnostic also accessed a singleton sequence as a direct node, yielding InvalidOperationException in nested map/array checks. The final harness unwraps singleton results and reaches the actual disposed-node guards; both initial and corrected outcomes remain separate. No consumer/core fix was made to resolve these harness errors.

This is Windows source/provider qualification for committed source and default XPath 3.1. It does not qualify the excluded dirty storage work, complete Storage/Integration suites, unchanged-binary replacement, native Linux, concurrent snapshot disposal, cancellation propagation, all 4.0 semantics, schema-typed persistence or C45. Evaluation exposes no cancellation parameter on the inspected public Evaluate overloads; no cancellation guarantee is inferred. Independent live snapshots were checked sequentially; parallel evaluation safety was not tested.

## Next step

Proceed with the separately isolated Braid authoring/preview source-compatibility slice. Keep actual consumer pins and runtime checkouts unchanged; distinguish source-suite results from unchanged-binary and native host acceptance. No XmlDb fix is currently justified by this bounded evidence.

Production source and versions remain unchanged. Schema's 451 passes and full core 24,214 passes / QT3 31,397/0/424 / qt4 8,941/0/1,918 (230 sets) remain inherited; only the consumer qualification above is fresh. Prior sealed dossiers remain unchanged. This qualification and preceding priority documentation are uncommitted.
