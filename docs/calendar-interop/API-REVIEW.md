# Calendar interoperability API review

Adds XdmCalendarInterop with five static methods and XdmCalendarInteropKeyComparer with its singleton and two CLR comparer methods. All use plain XdmValue operands and return data or process-local hashes. Existing public rows, layouts and default dispatch remain unchanged.

Read first rejects any internal authority marker without calling public provenance observers or provider methods. It then validates physical family and built-in labels, checks wrapper/raw timezone agreement, rejects nonzero hidden absent offsets, validates legacy seconds/milliseconds and uses the existing exact constructor for calendar rules. Time intentionally drops incidental year/month/day; dateTime retains extended Gregorian years. Valid 24:00 normalizes, including overflow refusal. Built-in and user labels are descriptive.

The comparison functions delegate to the previously qualified exact timeline kernel after explicit projection; no duplicated calendar arithmetic or millisecond narrowing of exact values occurs. XPath context offset is validated even for aware values. Same-key hashes preserve family and timezone presence. The separate CLR comparer is calendar-only and deliberately rejects non-calendar keys. It does not replace XdmValueEqualityComparer or XDM map semantics.

Typed calendars require a future guarded query projection with ownership/lifetime/type proof. Neither live nor expired authority carriers enter this API; detached components already explicitly extracted by the caller are plain data. Native Schema calendar datatypes/facets, C45, arithmetic, canonical UTC serialization and publication are outside this batch.
