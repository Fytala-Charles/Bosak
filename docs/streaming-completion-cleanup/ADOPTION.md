# Streaming completion cleanup — pre-implementation adoption

30 October 2026. Recorded before production edits at exact Bosak source `4fc353f8fbbc770cdef2618d2d56b59cfa601420`. [Source locks](ADOPTION.json) distinguish normative language source, supporting runtime implementation and the existing Bosak ownership contract.

Adopt the C# statements specification at immutable `dotnet/csharpstandard` revision `58e33cd745b9bcd92011e8afeff11c8c821ae480`, sections 13.10.6 and 13.11. Its exception propagation/finally rules motivate explicit retention of both errors when cleanup also fails. Supporting .NET v10.0.0 implementation pin `60629d14374c56f1cb51819049ad1fa529307f8d` records XmlReader Dispose/Close and XmlReaderSettings.CloseInput; it is not a claim that the installed runtime was built from that tag.

Existing Bosak Load(Stream) owns the reader it creates and delegates stream closure to CloseInput; Load(XmlReader) never owns the supplied reader. IStreamingDocument.StreamCompleted is raised once at root completion. The completion pump marks Done before callbacks; preserve that ordering, multicast short-circuiting and no-repeat behavior.

The bounded fix must always attempt disposal of an owned reader after the completion callback, whether that callback succeeds or throws. Preserve the same original exception for callback-only and disposal-only failures. If both fail, throw AggregateException containing the callback exception first and cleanup exception second; neither error is silently discarded. This explicitly differs from an unguarded finally whose cleanup error replaces the original callback. It introduces no W3C error code and no public signature or value layout.

Test normal and throwing completion, CloseInput true/false, supplied readers, callback cancellation, disposal-only and dual faults, empty roots, retained replay and repeated Drain. First execute focused regressions against the old implementation. Bump the source candidate patch to 1.34.2 after implementing the fix. Run full build/tests and both frozen conformance gates with exact skip-map comparison. Keep old lifetime observations/API snapshots and external owner projects unchanged.

Excluded: early abandonment, record-postprocessor/initial-load failures, general optional scopes, cancellation protocol, recursive disposal and complete Schema/XmlDb lifetime qualification. This contract is a core-owned resource fix, not API freeze or general C45 acceptance.
