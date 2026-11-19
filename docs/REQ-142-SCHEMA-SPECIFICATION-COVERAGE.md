# REQ-142: shared Schema specification coverage and application interoperability

Requester: product owner and Bosak.Schema. Registered19November2026.
Status: In Progress for coverage audit/planning; semantic implementation pending
slice adoption. Scope/owner roles are in [the shared programme](specification-coverage/PROGRAM.md).

Core maintainers own engine semantics, neutral typed transport, compatibility and
the actual Core integration lane. Schema maintainers own schema assessment and
application validation. REQ-139/140 remain the separate dependency/provenance/
general API/freeze gates; this priority decision does not accept C45 or bypass them.

Deliverables: clause/case coverage ledger; M1 namespace/optional-assertion consumer
contract; reproduced missing Core capability requests if needed; exact positive/
negative/refusal and existing conformance gates; package/binary/application proofs.
The first task makes the real current coverage visible, not a full-XSD claim.
No Core source implementation, product version or dependency change in this step.

## M1 local implementation - 19 November 2026

The namespace/optional-assertion slice is adopted and implemented locally in Schema with409source,94minimum-Core package and42current-Core package cases passing. The [candidate delta](specification-coverage/M1-CANDIDATE-DELTA.json) and [authored case map](specification-coverage/M1-CASE-MAP.json) distinguish positive/invalid/profile-refusal evidence. No Core runtime/API or optional-wrapper pin changes. Delivery, broader scope and official W3C suite qualification remain pending.
