<div align="center">
  <img src="../../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Experimental signed Unix epochs">
  <h1>Experimental signed Unix epochs</h1>
  <p>24 October 2026 — source 1.25.1 unpublished; public 1.25.0</p>
</div>

Following the [contract review](UNIX-DATETIME-CONTRACT.md), this bounded REQ-126 correction accepts negative milliseconds only under `XPath40Experimental`. For example, `unix-dateTime(-1)` returns `1969-12-31T23:59:59.999Z`; frozen `XPath40` retains FOCA0002. The user authorized the recommended next implementation slice. This does not adopt every newer draft contract. PR433 merged at `07db3b57e5eb2d60ab7f02315bd60efe62284408`, the implementation base.

The integer fast path and UTC epoch arithmetic remain intact; the non-integer conversion selects `integer` only for Experimental. The result remains bounded to .NET calendar years 1–9999: `-62135596800000` is year 1, and the preceding millisecond raises FODT0001. Larger positive values beyond Int64 still raise FORG0001; their error classification remains unresolved. Experimental conversion diagnostics can name `integer` rather than `nonNegativeInteger`.

The minimum negative literal still does not have integer representation, whereas an equivalent computed integer does. Experimental evaluation now reaches the calendar-range error for both; frozen retains the prior literal FORG0001 versus computed FOCA0002 distinction. General integer representation, permissive conversion/cardinality and keyword behavior are separate work.

## Verification

The new 25-case API suite first reproduced 20 failures with five controls passing. After correction, all 80 Unix-time API tests and two new XSLT host isolation tests pass. Existing profile-parameterized tests distinguish the two explicit contracts. Coverage includes exact UTC values, dynamic/reference/partial calls, context reuse, overflow and the year-1 boundary. Legacy and Xslt40Draft hosts continue to reject negative epochs.

Release build passed with zero errors; existing CS8604 and NU1900 vulnerability-metadata availability warnings remain. All **11,323 solution tests plus 84 language-server tests pass**. Fresh QT3 is **31,142/0/679**; frozen qt4 is **8,434/0/1,896**, with 224 configured/executed sets. Skips and existing Experimental retries do not certify full conformance.

The current native Unix-time set remains **20/4/0, exit 2**: its runner uses frozen compatibility and does not retry FOCA0002. The two negative-domain failures and two wider-integer failures remain explicit. No fixture, gate membership or retry-policy change was made. Direct Experimental tests verify the new opt-in behavior separately. [Machine-readable results](UNIX-DATETIME-EXPERIMENTAL.json) and [raw gate receipts](evidence/unix-datetime-experimental/) preserve the evidence.

Catalog inventory stays 76 entries, 3,148 feature checks plus 24 chapter checks, 224 gates and 28 unavailable historical pins. REQ-126/D2/D3/D4/AC-03 remain partial. Source 1.25.1 is unpublished; public/Braid 1.25.0 and pending consumer acceptance remain separate. Next: review this bounded correction, then investigate wide-integer conversion as a separate slice. No release performed.
