<div align="center">
  <img src="../assets/logos/fytala-logo-color-dark.svg" width="100" alt="Fytala Bosak 1.25.1 release notes">
  <h1>Bosak 1.25.1 release notes</h1>
  <p>24 October 2026 — published and independently verified</p>
</div>

Bosak 1.25.1 consolidates the REQ-126 numeric and Unix-time corrections since 1.25.0. No new public API is introduced. All ten signed 1.25.1 packages are published and independently verified. Install `dotnet add package Bosak.Xslt --version 1.25.1` or `dotnet add package Bosak.XPath.Api --version 1.25.1`.

- `XPath40Experimental` accepts signed Unix milliseconds within the supported calendar. Frozen `XPath40` and XSLT hosts retain the nonnegative domain. Valid decimal-backed integer epochs beyond the calendar now report FODT0001 before Int64 narrowing.
- Binary integer addition, subtraction and multiplication avoid constant-folding wraparound and preserve integer identity through decimal promotion. Subtraction safely handles Int64.MinValue without negating it first.
- Floating integer conversion rejects the exact signed 2⁶³ and unsigned 2⁶⁴ overflow thresholds. Accepted large unsigned values retain their exact magnitude. Unsigned numeric decimals, floats and doubles discard fractions before range checks; values in (-1,0) become unsigned zero. Fractional lexical strings remain invalid, and NaN/infinity retain FOCA0002.

This release adds 431 regression cases. Fresh validation passes 11,675 solution tests plus 84 language-server tests. QT3 remains 31,142/0/679; frozen qt4 is 8,436/0/1,894 across 224 unchanged gates. Two former compile-overflow skips now reach runtime errors accepted by their existing assertions. Current upstream Unix-time tests remain 22/2/0 when evaluated by the frozen runner; this does not certify full current-draft conformance.

The XSLT authoring source is unchanged from 1.25.0, including REQ-137 Move up/down. Fresh signed public package validation retains 153 broad checks and 1,045 focused checks, including the 431 new regressions; these overlap source coverage. Generic wide constructors/storage/error policy, bounded .NET decimal precision and the years 1–9999 Unix-time calendar remain explicit limits. REQ-126 stays partial; REQ-138 is outside this release.

[Exact source, packages and consolidated validation](RELEASE-1.25.1-CANDIDATE.md). [Verified signed-package publication](RELEASE-1.25.1-PUBLICATION.md) records the completed tag, Release run and public consumer checks.
