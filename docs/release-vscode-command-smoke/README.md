# VS Code registered-command smoke tests - 8 November 2026

**Two commands passed; three failed. The current extension candidate is not ready for publication.** [Results](RESULTS.json) and [all five command checks](COMMANDS-COMPLETE.json) retain exact expected/observed outputs against the unchanged installed0.2.6 VSIX.

| Registered command | Expected output | Result |
|---|---|---|
| `bosak.evaluateXPath` | `6` | Failed: no result document; server reports document not open. |
| `bosak.runXQuery` | `q1|q2|q3` | Failed: no result document; server reports document not open. |
| `bosak.evaluateXQuery` | `42` | Failed: no result document; server reports document not open. |
| `bosak.transformXslt` | `<result>23</result>` | Passed with real XML input and stylesheet. |
| `bosak.runInitialTemplate` | `initial-42` | Passed with named template `start`. |

The harness invokes VS Code's actual registered commands and observes the resulting untitled editor contents. It loads the installed final candidate and default bundled server, with no engine/extension mocking. Transformation source and template names are supplied as command arguments; file-picker and cancellation interaction remain untested. The isolated host's exit0 means the harness completed collecting results, not that all commands passed.

## Reproduced defect

[The command protocol trace](command-protocol-trace.txt) shows synchronized documents and executeCommand arguments using `file:///d%3A/...`, followed by `Document is not open.` notifications. Synchronization parses a typed DocumentUri, but ExecuteCommandHandler passes its raw URI string into DocumentManager's string-key lookup.

[The protocol reproduction](URI-REPRODUCTION.json) proves the mismatch without editor timing: open one XPath document using the escaped drive colon, execute its URI unchanged and receive the error; execute the equivalent URI with an unescaped colon and receive6. No document content or engine changes between calls. The first reproduction omitted workspace execute-command capability and got method-not-found; its original protocol messages are preserved. The corrected reproduction advertises that capability and reproduces the actual defect.

The first UI harness stopped at XPath failure; the independent trace attempt supplied an explicit URI and also failed. Both original partial reports and logs remain. The final harness records every command independently, preserving failures rather than turning missing output into a pass. [Earlier live diagnostics/correction checks](../release-vscode-editor-unblock/README.md) remain valid and do not establish command success.

## Next implementation

Normalize URI-valued execute-command arguments through the same DocumentUri parser used by document synchronization. Add happy-path and edge regressions covering escaped Windows drive letters, spaces/Unicode/percent signs and missing or foreign open documents. Do not change typed input, provider lifetime or language semantics to fix editor document lookup. Build a separately identified server/VSIX cohort and repeat server, installed-command and live editor qualification; current receipts cannot be relabeled.

The [release checklist](../RELEASE-READINESS-1.81.1.md) records this concrete blocker. No production source/version edit, machine permission change, active-profile change, tag or publication occurred. Only disposable-profile protocol tracing was enabled. Full engine suites were not rerun because no engine or production implementation changed.

Run `python docs/release-vscode-command-smoke/tools/verify.py` for read-only verification of these retained failure and passing receipts.
