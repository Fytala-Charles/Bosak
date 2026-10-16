<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak static for-each validation">
  <br><br><h1>Static validation of xsl:for-each</h1>
  <p>Unknown attributes and required selection checked during compilation</p>
</div>

## Review and scope - 16 October 2026

Unpublished source candidate **1.14.2** adds xsl:for-each checks to the existing stylesheet instruction validator. It reuses the common standard-attribute allowlist, with `select` and the already supported `separator` attribute, and checks required select presence before a transform can run. Missing select raises XTSE0010 during compilation, including in unreachable bodies. Unknown unqualified attributes raise XTSE0090 under the existing strict-version policy, even when a valid select is present. Attribute checks precede the missing-select check.

The original native arrays-021 uses a version3.0 stylesheet with illegal legacy array/bind-to attributes and no required select. More than one static error applies. The current draft permits implementation-dependent error selection, so the prior XTSE0010 versus expected XTSE0090 mismatch alone did not establish an error-precedence defect. Independent controls showed the actual gaps: unknown attributes with a valid select were accepted, and missing select could be deferred to execution or escape detection in an unreachable body. Those compilation gaps justify this bounded correction; no universal priority between static error codes is claimed.

```xml
<!-- Rejected during compilation with XTSE0090 under strict version3.0 validation. -->
<xsl:for-each select="1" bogus="x"/>

<!-- Rejected during compilation with XTSE0010 even when this body cannot run. -->
<xsl:if test="false()"><xsl:for-each/></xsl:if>
```

Standard, XML and foreign attributes remain supported. A foreign t:select is not the required unqualified select. Instructions removed by use-when are not validated. Shadow select attributes retain their existing processing. Separator support, including existing version3.0 behavior, is unchanged. The established local forward-compatibility policy remains intact: versions above3.0 continue through the current unknown-attribute fallback, and local earlier versions retain strict checks. This slice does not redefine global 4.0 forward-compatibility handling or instruction content rules. Runtime missing-select guards remain as safety checks.

No new public API, instruction syntax, sorting behavior or iteration behavior is introduced.

## Pinned rules and evidence

Reviewed qtspecs remains `72f222929768b411d8e88d96a1a8740483290078`. The pinned [for-each rules](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xslt-40/src/xslt.xml#L16076) require select. The [element catalog](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xslt-40/src/element-catalog.xml#L581), blob `1191b929fbc3dcc20bf29bc6bb7aee1552e39e61`, lists select and separator. The [multiple-error rule](https://github.com/qt4cg/qtspecs/blob/72f222929768b411d8e88d96a1a8740483290078/specifications/xslt-40/src/xslt.xml#L2323) permits either detected error to be reported when several apply. Rendered W3C page fetches exceeded browser size limits; review used pinned primary XML.

PR325 passed exact-head CI run38891357065 and merged before this slice. Refreshed unchanged xslt40-test remains `91328fbf0fed4a5d2813eb3661808ee53cb961ba`. Native files, expected codes and dependency handling are unchanged.

| Native selection | Before pass/fail/skip | After pass/fail/skip |
|---|---|---|
| array substring aggregate | 146/14/20 | 147/13/20 |
| array instruction set | 16/13/8 | 16/13/8 |
| sx-SquareArrayConstructor | 56/0/0 | 56/0/0 |
| arrays type set | 74/1/12 | 75/0/12 |
| record aggregate | 29/35/1 | 29/35/1 |
| type-alias | 22/9/1 | 22/9/1 |

Only unchanged arrays-021 newly passes; no prior pass is lost. All three aggregate family runs retain exit1. The arrays type set has no failing executed cases, but its12 skips remain skips; no complete conformance or gate promotion is claimed. Other array instruction/profile/serialization failures remain failures. No upstream contribution, expectation edit, new skip or error alias occurs.

The initial fixture had a C# quoting error, preserved with its log/bytes. The17-case diagnostic baseline then produced5 pass/12 fail, including one fixture mistake: the shadow-select text selected the literal string '1 to2' instead of the intended sequence. That fixture was corrected without changing expected output12. The remaining11 negative controls independently exposed missing static validation. Initial post-implementation selection55/1 retains the fixture error; final new cases17/0/0 and combined surface/static checks56/0/0 pass. All logs and exact fixture hashes are retained in [machine-readable evidence](XSLT40-FOR-EACH-VALIDATION.json).

Source1.14.2 remains unpublished. Public libraries1.8.0, VSCode0.2.4/engine1.8.0, REQ statuses and independent Braid acceptance are unchanged.

Next bounded review: trace native array-010/011/012 serialization failures against the declared output and result assertions before choosing a runner or engine correction. Other aggregate atomization and profile failures remain separate.

## Verification

Release build passes with zero errors and22 warnings. Solution8,933/0/0 (XSLT5,999), language server84/0/0, focused static/surface checks56/0/0 (17 new cases). Frozen QT3 31,142/0/679 and qt4 gated8,278/0/1,894, both exit0. Native array aggregate improves to147/13/20, only arrays-021 newly passing, no lost passes; arrays type set75/0/12, record29/35/1 and type-alias22/9/1 unchanged. All aggregate family exits remain1. All local jobs complete. Canonical docs are rendered with markdown-it and Fytala CSS; kit-managed assets remain unchanged.
