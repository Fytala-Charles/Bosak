Bosak.Braid requests confirmation of the supported path for deleting one existing literal `xsl:text` instruction as its next bounded visual authoring operation.

Current public Bosak 1.21.0 authoring provides value-of deletion and literal text body replacement. At main `0d0c4f3fa36e73cdfc51e106b1d69a5558118f30`, `ValueOfDeletionPipeline.Target` explicitly requires an empty-body XSLT `value-of` with `select`; it cannot accept `xsl:text`. No separate text-deletion capability/request was found. Please confirm whether an existing supported public operation covers this, or register/design a separate consumer request through Bosak's FEATURE_REQUESTS workflow. No new REQ identifier is assigned by this inquiry.

Requester: Bosak.Braid. Product owner asked Braid to check with Bosak on text deletion. This inquiry requests API/design guidance, not engine implementation or release authorization.

Proposed first bounded scope and acceptance criteria:

- Remove exactly one eligible principal-module literal `xsl:text` instruction, including its payload. This is distinct from replacing its body with empty text, which retains the instruction.
- Engine-owned capability discovery, opaque target, proposal and classified refusal. Bosak determines eligible forms, attributes, parents and module/profile restrictions; do not silently broaden the current value-of contract.
- Preserve exact source bytes outside the instruction: no formatting, whitespace cleanup, namespace cleanup or consumer XML splice.
- Provide complete removed instruction/payload identity evidence, surviving parent, exact source edits and complete correspondence/partitions. Any adjacent text-node merge needs explicit bounded provenance; ordinary or significant mixed-content cases without provable correspondence must refuse.
- Cover empty/nonempty paired text and explicitly accept or refuse self-closing text; document entity spelling, Unicode, namespace aliases, comments/mixed surroundings and xml:space boundaries.
- Retain source on stale/foreign targets, invalid candidates, unsupported scope, bounds failures or cancellation. Preserve all existing insertion/replacement/value-of-deletion contracts and binary compatibility.
- Braid then supplies independent evidence verification, explicit review/Apply, surviving-parent navigation, one-step exact undo/redo and fresh inspection. Final acceptance includes validation, changed expected-output preview, new-file export/hash and independent reopen/validate/preview across supported encodings.

Consumer context: [current roadmap](https://github.com/Fytala-Charles/Bosak.Braid/blob/1fd93fed2c27473d8ebfe35953fc92a4947130b5/ROADMAP.md), [existing value-of deletion contract](https://github.com/Fytala-Charles/Bosak.Braid/blob/1fd93fed2c27473d8ebfe35953fc92a4947130b5/docs/VALUE_OF_DELETION.md), and [literal text editing contract](https://github.com/Fytala-Charles/Bosak.Braid/blob/1fd93fed2c27473d8ebfe35953fc92a4947130b5/docs/LITERAL_TEXT_EDITING.md). A dedicated Braid operation design/ADR and upstream request registration should precede implementation. Bosak owns API design, engine implementation/tests and release decisions.
