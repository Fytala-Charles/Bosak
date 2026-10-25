# REQ-108 design notes — user-defined type identity in PSVI typed values (PA-3 tail)

Date: 2026-10-22. Branch context: `fix/req-107-as-set-residuals` (REQ-107 lands as-2905 + as-1803).

## Failing tests in scope (6)

as-2002, as-2101, as-1806, as-1807, as-1808, as-1809 — all pass only when
`instance of <user-defined-type>` uses **subtype identity** and `@as` coercion
converts (losing the user type) unless the value is already a subtype.

## Root cause chain (verified by probes)

1. `XDocumentNode.ConvertSchemaValue` (XDocumentNode.cs ~line 660): for user-defined
   simple types the value's `SchemaTypeName` is **replaced by the ultimate built-in
   base type name** (`GetBuiltInBaseTypeName`). User type identity is discarded at
   atomization; a `partNumberType`-typed value reports SchemaTypeName "string".
2. `VmEngine.ValueMatchesType` user-defined branch (~line 11160): matches by
   **castability** (`TryCastToSchemaType`), not identity. Consequences:
   - coercion (`ConvertVariableValue`, TransformEngine.cs ~15227) treats a castable
     value as "already a subtype" and skips conversion (as-2002: untypedAtomic kept);
   - after a real conversion, `instance of U` is wrongly true when the converted
     value still validates against U's facets (as-1807 var2: float 123.45679 → de2;
     as-2101 var4).
3. `XdmConversions.TryCast` does not handle user-defined type names at all
   (untypedAtomic → U falls to XTTE0570), so fixing (2) alone breaks as-2002's cast.

## Required design (multi-touch)

- Carry the user-defined type's qualified name through `XdmValue` (second annotation
  or qualified SchemaTypeName) set in `ConvertSchemaValue` where the `XmlSchemaType`
  is in hand.
- Subtype check against `context.SchemaSet` walking `BaseXmlSchemaType`
  (`IsSchemaTypeSubtype(context, ns, local, ns2, local2)` already exists and is used
  for element/attribute type compatibility).
- All `SchemaTypeName` consumers must become base-type-aware (EBV string-family rules,
  `XdmValueEqualityComparer.StringFamilyNames`, `ItemInstanceOf` switch,
  `IsUriPromotion`, date/time formatting).
- `ValueMatchesType` user-defined branch → identity check; coercion then casts via a
  facet-validating path that tags the result with U's identity.
- Beware XSD 1.0 vs 1.1 hierarchy: the W3C as-suite expects XSD 1.0 semantics
  (anyURI NOT <: string; URI promotion converts and loses the user type). Do NOT
  "fix" the hierarchy to XDM 3.1 — as-2101 expects `falsetrue`.

## Not fixable (platform limit, same class as type-functions-0401)

as-1701 vars 4/13/14: source values `-0012-12-03-05:00` (xs:date), `-0012-05:00`
(xs:gYear), `21999-05+14:00` (xs:gYearMonth) — DateTimeOffset year range [1, 9999].
`GetTypedValue` catch-fallback yields plain string → `instance of` actual type false.
Documented in AGENTS.md Known Limitations.
