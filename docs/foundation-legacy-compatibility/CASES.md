# Legacy compatibility cases

30 October 2026. Primary qualification uses twenty unique cases in three modes (60 case-executions); retained bundles also replay the same cases. All pass with identical observations. These are core-owned fixtures, not actual consumer qualification. [Receipts and scope](README.md).

| ID | Existing path | Expected/observed contract |
|---|---|---|
| LC-01 | Compile/validator defaults | XPath31, no schema, default typing/warning/error limits |
| LC-02 | No-schema context | Null set and missing declaration/type lookups |
| LC-03 | Assigned schema context | Retained set and element/attribute/type lookup identity |
| LC-04 | Missing names with a set | Null lookup results |
| LC-05 | Clearing schema context | Lookups clear; independent schema-aware flag retained |
| LC-06 | CompileOptions.SchemaSet | Retained reference and declared schema-element compilation |
| LC-07 | No-schema schema-element | XPST0008 |
| LC-08 | Absent schema declaration | XPST0008 |
| LC-09 | Unbound prefix | XPST0081 |
| LC-10 | Schema attribute compilation | Declared schema-attribute accepted |
| LC-11 | Ordinary untyped XML | String/count query results without schema |
| LC-12 | Single-stream validation success | Valid, no errors, input remains caller-readable |
| LC-13 | Invalid lexical XML value | Invalid result with one error |
| LC-14 | Malformed XML | Invalid result/error collection |
| LC-15 | Throwing validation API | XmlSchemaValidationException |
| LC-16 | Stream collection overloads | Valid nonthrowing/throwing results |
| LC-17 | Legacy annotation/atomization | Custom type annotation, decimal typed data |
| LC-18 | Invalid annotation paths | Invalid result and throwing option |
| LC-19 | Preserve and strip | Same PSVI on preserve, annotation removed on strip |
| LC-20 | Named type validation | Explicit named decimal type accepted |

The exact ordered values and exception types/codes are retained in the three execution JSON files. Framework-localized error messages are not comparison keys. Existing optional arguments are omitted in selected calls so retained binaries exercise their embedded/default call behavior. All XML/schema inputs are owned memory fixtures with no imports, external entities or database access.
