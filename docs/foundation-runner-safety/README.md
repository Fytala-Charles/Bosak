# Retained-caller runner safety ? 31 October 2026

The most likely source of the reported 0xe0434352 popup is the earlier retained-caller invocation without its required output-file argument. That executable throws an unhandled usage exception before running any cases. The accessible Windows Application event query returned no matching entry, so correlation to the exact popup is unconfirmed. Correct invocations subsequently passed all 20 cases; no Bosak product failure was demonstrated by that invocation.

Use `python docs/foundation-runner-safety/tools/run_caller.py --layout <qualified-layout> --output <fresh-workspace-json>` for further retained-caller runs. The runner validates required arguments, caller/package hashes, configuration-file presence and output destination before starting .NET. It catches runner failures, captures child output and applies a sixty-second watchdog. Negative tests use a mocked launcher; they never deliberately crash .NET.

On Windows, the fresh runner process temporarily sets SEM_FAILCRITICALERRORS and SEM_NOGPFAULTERRORBOX and restores its prior mode afterward. Children inherit that process mode, as documented by [Microsoft SetErrorMode](https://learn.microsoft.com/en-us/windows/win32/api/errhandlingapi/nf-errhandlingapi-seterrormode). This changes no machine-wide WER/registry setting and is not a promise that another process cannot override its own error mode. No actual crash is induced to test it.

This is tooling only. Production remains 1.35.0, previous evidence stays sealed, and no product test expectations, public API or freeze decisions change.
