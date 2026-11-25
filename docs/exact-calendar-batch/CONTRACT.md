# Exact calendar foundation contract

25 November 2026. New authorized batch: maximum 20 contracts with implementation, commit/push, PR review and automatic passing merges. No package publication. Immutable adoption is recorded in [ADOPTION.json](ADOPTION.json) before implementation.

Provide opt-in immutable exact decimal-second and calendar value objects. Existing XdmValue/DateTimeOffset and XPathDateTime behavior stays compatible. The new objects are not authenticated schema facts or expression-engine atomic values. Review public API additions as one cohesive foundation change rather than claiming unrelated engine support.

Second storage uses normalized exact decimal coefficient/scale and supplied bounds (1..256 digits, 0..256 scale), enforcing capacity before normalization. Values are in [0,60); lexical seconds require exactly two ASCII whole digits plus an optional nonempty fraction. Conversions to integer ticks/milliseconds refuse inexact values.

Calendar kind distinguishes dateTime/time/dateTimeStamp. Validate proleptic Gregorian dates including year zero, signed extended years, time fields and explicit timezone bounds. Absent timezone has offset zero internally and remains distinct from UTC. Normalize 24:00:00 only for exact zero fraction; roll date forward with checked year capacity. Reject leap seconds. Lexical inputs are bounded to 1024 characters and use XML whitespace collapse only around the whole token. DateTimeStamp requires timezone. Format normalized component strings with explicit capacity; do not call this XSD canonical UTC serialization.

DTO conversion requires an explicit timezone, representable year and exact tick resolution. Legacy conversion requires exact milliseconds. No rounding, implicit timezone, calendar arithmetic, instant equality, XdmValue attachment or Schema admission. Structural identity includes kind, fields and timezone presence/offset; second equality is numeric. Future transport and signed-package qualification are separate.
