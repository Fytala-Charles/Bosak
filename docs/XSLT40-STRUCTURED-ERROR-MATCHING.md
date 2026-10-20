# Exact structured error matching -20 October2026

Native exact-error mode previously returned false before examining XPathErrorException.CodeNamespaceUri/CodeLocalName. Consequently `error()` raised the correct FOER0000 but failed the harness when its human message contained no code. This is a test-adapter correction; engine behavior and package version are unchanged.

Structured identity matching now precedes human message matching and legacy-alias exclusion; in exact mode it is authoritative even if the description mentions another code. In exact mode, plain codes require the standard error namespace and Clark-notation codes require the declared namespace, including an explicitly empty namespace. Unresolved prefixed strings remain rejected in exact mode; legacy matching retains its historical behavior. Legacy label/code aliases remain disabled by --xslt40.

Seven fixture cases cover standard FOER0000, custom namespace, empty namespace, wrong local code, wrong custom namespace a custom error masquerading as standard FOER0000, and a message mentioning a different error code. In both compiler profiles the original adapter reports2/5/0 (two false passes from misleading descriptions); the corrected adapter reports3/4/0, with precisely the four intentional mismatches still failing. Two process controls verify this aggregate, bringing the permanent controls to16; all16 pass. The final negative child status is normalized to explicit exit0 only after every assertion passes.

The separate array/select batch's corrected native fixtures improve23/1/0 to24/0/0 in both profiles. Their48 public-API checks already pass against unchanged engine main22cc7d30. The earlier21/3/0 receipts include two independently corrected nonportable unused-template assumptions; they are not claimed as harness recoveries. No existing expectations, engine behavior, skip lists, legacy aliases or gates are changed by this adapter patch. CI must verify the exact review head before merge.

The process controls verify every accepted and rejected case identifier in both profiles, as well as the aggregate totals.
