# Isolate XSLT conformance outputs and preserve principal raw results

The standalone runner could overwrite a golden fixture with produced output and then accept the overwritten file as its expectation. Capture secondary outputs in memory with their logical URIs and effective serialization properties; assertions now require output from the current transform. Serialization stays inside result production so `xsl:try` can catch its errors. Synthetic controls prove the old wrong/missing-output acceptance and the corrected refusals without changing fixture bytes.

Also isolate explicit principal result-document content from an enclosing raw-template accumulator and return that principal value before the enclosing template result. This resolves all 12 original result-document failures. Public API signatures stay unchanged. Core/server development metadata advances to1.81.3; no publication or extension-version change is included.

Validation:

- Clean Release solution build; 26,349 implementation cases and 90 separate language-server cases pass.
- Frozen QT3 remains 31,397/0/424; selected qt4 remains 8,941/0/1,918, with 11 experimental passes included.
- Four new unit cases, eleven output-isolation controls and sixteen existing profile/error controls pass their expected outcomes. The three deliberately wrong/missing/stale outputs are assertion failures, not accepted cases.
- Full pinned W3C XSLT 3.0 catalog: 10,211 passed / 10 failed / 4,380 skipped, all 14,601 original cases accounted for. No corpus writes; every skip identity and explanation is unchanged. All source/runtime/corpus hashes and unsuccessful preliminary attempts are retained separately.

The full standalone qualification remains unsuccessful: eight XML-version cases, `output-0707` and `try-022` still fail. This change introduces no skip admission, frozen-gate adoption, full-conformance or schema-coverage claim. Original reference receipts remain unchanged.
