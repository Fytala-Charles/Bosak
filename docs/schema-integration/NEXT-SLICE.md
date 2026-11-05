# Next slice: a real assessed-document boundary

The working XSD 1.0 price example proves the existing public surface can deliver a useful typed transformation. Avoid adding the C44-R2 session surface merely to reproduce that path.

The next implementation should address the missing boundary in **Bosak.Schema**:

1. Inventory the exact structural, declaration, type and assertion coverage of the real backend. Define a narrow supported document/schema profile and explicit unsupported findings before claiming assessment success.
2. Design an assessment result that distinguishes assertion evaluation from structural assessment. Retain the assessed snapshot, governing declaration/type, original PSVI and diagnostics; describe caller ownership and any retained resources. Neither a successful assertion result nor a root-document digest establishes complete component authority.
3. Implement one structurally supported XSD 1.1 document shape with an assertion. Test valid input, assertion failure, invalid lexical content, invalid structure and unsupported constructs. Do not remove 1.1 syntax and send the schema to `XmlSchemaSet` as a substitute.
4. Feed that genuine assessed result into a Bosak query/transformation. Prove typed atomization, unchanged original input, preserving/stripping copies and explicit refusal when the engine requests unsupported coverage.
5. Revisit the smallest core-facing contract from that evidence. Independently compiled compatible schemas, anonymous/list/union types, source/snapshot identity, cancellation, lazy results and actual consumer ABI remain requirements before a broad API freeze.

Keep the existing frozen profiles and exact conformance skips unchanged until their original assertions actually pass through the real provider and any baseline change is separately reviewed. No release or consumer dependency upgrade is implied by this development plan.
