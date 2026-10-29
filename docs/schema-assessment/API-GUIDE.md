# Native document assessment — 0.5 development candidate

The activation-free `SchemaServices.CreateDocumentAssessor` API provides a **bounded native XSD 1.1 assessment**, separately from the existing assertion-only descriptor service. The source is an unpublished feature candidate; this is not full XSD 1.1 conformance or a general core schema-session API.

## Run the example

```sh
dotnet run --project examples/Bosak.Schema.Assessment.Example -c Release
dotnet test Bosak.Schema.slnx -c Release
```

The example compiles a schema with `amount`/`shipped` decimal children and an assertion, assesses the XML structure and values, then queries/transforms an assessed decimal copy. The output reports a valid assessment, decimal typing, total 15, and successful annotation-preserve/strip checks. The consumer supplies an empty BCL schema set solely as the legacy annotation-copy context; **no XSD 1.1 schema is compiled or validated through it**.

## Use the result

```csharp
var assessor = SchemaServices.CreateDocumentAssessor(schemaDocument);
var result = assessor.Assess(instanceDocument, cancellationToken);
if (result.Outcome == SchemaAssessmentOutcome.Valid)
{
    var assessed = result.Document!;
    var rootState = assessed.Elements[0];
    var originalComponents = rootState.Declaration;
    var preserved = assessed.Copy();
    var untypedXml = assessed.CreateDocumentCopy(preserveTypeAnnotations: false);
    var decimalLeaf = assessed.CreateTypedElementCopy("amount");
}
```

`IsSupported` describes compilation of this finite profile. It does not certify that an arbitrary schema is valid or fully implemented. `Invalid` means a supported structure, lexical constraint or assertion failed. `Unsupported` means coverage/representation was refused. Neither outcome exposes an `AssessedDocument`. Cancellation throws; it does not produce successful partial output.

## Exact profile

| Area | Supported coverage |
|---|---|
| Schema | One XML 1.0, DTD-free, no-target-namespace schema; one named global element with one anonymous element-only complex type. |
| Structure | One sequence of 1–16 unique local xs:decimal declarations, each occurring exactly once, followed by 1–8 assertions. |
| Attributes | Namespace declarations; schema `version` and `xpathDefaultNamespace="##local"`; documented declaration name/type and occurrence-one attributes; assertion test/local default namespace. Everything else is refused. |
| Assertions | Declared child names or exact decimal constants compared with eq/ne/lt/le/gt/ge, combined with and/or and parentheses under XPath 2.0. No functions, variables, axes, arithmetic or external access. |
| Values | XML-whitespace collapse, exact decimal lexical/value assessment. Non-decimal syntax is Invalid; valid values requiring rounding/overflow/underflow are Unsupported. |
| Instance | Exact root, child count/order/names, element-only root text and decimal leaf content. No application attributes or xsi requests. Root comments/PIs and decimal CDATA are supported; comments/PIs inside decimal leaves are Unsupported. |
| Bounds | 32 KiB serialized schema, 64 KiB serialized instance, 256 descendant nodes, 256 decimal lexical characters, 512 assertion characters, 128 assertion tokens and sixteen parenthesis levels. |

Imports/includes, named types, attributes, facets, alternatives, nil/default/fixed, identity constraints, wildcards, open content and other constructs are explicitly outside the profile. Unsupported constructs are never dropped and sent to a 1.0 validator. The native implementation uses no `XmlSchemaSet` structural-validation path.

## Ownership and original state

The assessor captures immutable schema text, original declaration/type objects and assertion plans. Each successful assessment owns a separate normalized XML snapshot and immutable element states. The root retains its anonymous complex type and **has no typed value**; decimal children retain their actual built-in type, governing local declaration and exact typed value. This profile never admits nilled elements.

Native `Copy()` preserves original PSVI, component references and `AssessmentIdentity` without revalidation; it creates an independent snapshot and `SnapshotIdentity`. These are process-local instance labels, not persistent source identities or compatibility proofs. Independent compilation creates independent anonymous component authority even when schema digests match.

The result cannot be mutated through the original schema/instance, its read-only collections or exported XML. Exported XML is caller-owned; later edits invalidate any assumptions about that export's assessment and do not modify the retained result. Inputs must not be concurrently edited while being captured. Immutable compiled assessors can process independent documents concurrently; no session/resource is disposed on behalf of the caller.

## Engine projection boundary

`CreateTypedElementCopy` exports a **detached built-in decimal value view**, not local-declaration authority or a complete PSVI document. Use it with `XDocumentNode` for real typed XPath and wrap it in an `XDocument` for the example's XSLT initial-template invocation. Native and engine preserve/strip copies are tested separately.

`CreateDocumentCopy(preserveTypeAnnotations: true)` throws explicit `BSK-XSD-016` unsupported coverage. Its false branch returns a stripped XML snapshot. The service does not pretend the anonymous complex root is `xs:anyType`, synthesize a named type, admit another schema by QName/digest, or silently lose annotations on a requested full typed export.

Before full document integration, define and qualify a minimal bridge that retains original anonymous-root/component state, atomization errors, copy semantics, cancellation and lifetime. Full schema-element matching, independent compatible universes, general PSVI serialization and actual product adoption remain pending.

## Qualification

The complete candidate Schema suite passes **191 cases**: the original 114 plus 77 new assessment cases. It passes against the unchanged published 1.26.3 package pins and against current core source in an isolated source wrapper. Existing 114-case test binaries also pass with only the Schema library replaced by the candidate. These scopes overlap and must not be added together.

The general core production code, frozen conformance gates and consumer dependency pins are unchanged. No new hosted CI result, package/tag publication or public API freeze is claimed. See the [adoption record](ADOPTION.md) and [ADR-008](ADR.md).
