// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Outcome taxonomy and declared coverage kinds for static stylesheet validation (REQ-125).
// SPECIAL NOTES        : Part of the Bosak XSLT 3.0 processor — controlled validation and preview API.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-10-2026     | Creation (REQ-125 Slice A)                                                               |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 10-10-2026     | REQ-125 review F1: StructuralChecks coverage kind (unknown instructions and the safe      |
//                      |                  |       |                | static XSLT error subset are part of the declared checked coverage)                      |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.Xslt.Validation;

/// <summary>
/// The distinct outcome of one static validation run. Every outcome is reported as data; validation
/// never throws for bad source. The taxonomy is shaped for the full REQ-125 controlled profile:
/// cancellation is reserved for Slice D and cannot be produced by this build.
/// </summary>
public enum XsltValidationOutcome
{
    /// <summary>
    /// Every applicable static check of the declared supported coverage completed without
    /// diagnostics: all expression, pattern and AVT slots in all resolved modules compiled in their
    /// real static context, the structural/static XSLT checks passed, and no construct with
    /// explicitly deferred coverage is present. Exclusions of the declared coverage (for example
    /// QName slot resolution against declarations) are documented on
    /// <see cref="XsltValidation"/>; constructs outside the checks entirely are either plain data
    /// or reported through <see cref="XsltValidationResult.UnsupportedCoverage"/>.
    /// </summary>
    Valid,

    /// <summary>
    /// All modules are structurally well-formed, but at least one XPath expression, match pattern,
    /// attribute value template or structural/static XSLT check failed. See the diagnostics.
    /// </summary>
    Invalid,

    /// <summary>
    /// A module's bytes could not be used as stylesheet source at all: not well-formed XML, an
    /// unsupported or invalid encoding, or otherwise unreadable source. Expression-level checks were
    /// not (or only partially) performed; see the diagnostics.
    /// </summary>
    InvalidSource,

    /// <summary>
    /// No diagnostics were produced, but the stylesheet uses constructs whose static analysis is
    /// explicitly deferred (see <see cref="XsltValidationResult.UnsupportedCoverage"/>). This outcome
    /// is never presented as a complete pass.
    /// </summary>
    UnsupportedCoverage,

    /// <summary>
    /// A module reference was refused during validation: the module resolver denied an
    /// include/import, the resolver itself failed, or a cycle/module limit stopped the walk. The
    /// modules that could not be obtained were not validated.
    /// </summary>
    Refused,

    /// <summary>
    /// Reserved for REQ-125 Slice D (execution control): validation was cancelled through the
    /// cooperative cancellation contract. No build up to and including Slice A can produce this
    /// outcome; it exists so consumers can code against the final taxonomy now.
    /// </summary>
    Cancelled,
}

/// <summary>
/// One kind of static check the validator performs. The declared coverage is fixed per engine build
/// and is returned on every <see cref="XsltValidationResult"/>; constructs outside this set are
/// either plain data (never analyzed) or reported explicitly through
/// <see cref="XsltValidationResult.UnsupportedCoverage"/>.
/// </summary>
public enum XsltValidationCoverage
{
    /// <summary>
    /// All attribute slots classified as XPath expressions (for example <c>select</c>, <c>test</c>,
    /// <c>use-when</c>, <c>group-by</c>, <c>count</c>, <c>from</c>, <c>value</c>, <c>use</c>,
    /// <c>initial-value</c>) are compiled in their real static context at the slot's effective
    /// XPath version.
    /// </summary>
    ExpressionSlots,

    /// <summary>
    /// All attribute slots classified as match patterns (for example <c>xsl:template/@match</c>,
    /// <c>xsl:key/@match</c>) are compiled by the engine's pattern compiler, including pattern
    /// construct validation (XTSE0340) and predicate expression compilation.
    /// </summary>
    PatternSlots,

    /// <summary>
    /// All attribute slots classified as attribute value templates (for example
    /// <c>xsl:element/@name</c>, literal result element attributes containing <c>{...}</c>) are
    /// decomposed into literal and expression segments; every segment expression is compiled in the
    /// slot's static context, and unmatched left braces are reported (XTSE0350).
    /// </summary>
    AvtSlots,

    /// <summary>
    /// Variable references in checked expressions are matched against the declarations visibly in
    /// scope at the slot: local <c>xsl:variable</c>/<c>xsl:param</c> declarations, template,
    /// function and iterate parameters, and global declarations across all resolved modules.
    /// Undeclared references are reported (XPST0008); <c>use-when</c> slots see only static
    /// globals, per the XSLT static context rules. Parameter defaults use declaration-order scope:
    /// a default may reference only parameters declared before it and globals.
    /// </summary>
    StaticVariableScope,

    /// <summary>
    /// The structural/static XSLT surface is checked without executing anything: unknown XSLT
    /// instructions (XTSE0010, with the forwards-compatible and top-level vendor-extension
    /// tolerance rules), top-level placement of declarations, must-be-empty element content
    /// (XTSE0260), static variable/parameter placement (XTSE0090), required attributes such as
    /// <c>xsl:if/@test</c>, and XSLT-namespaced attribute rules (XTSE0090/XTSE0805). The checked
    /// set is the safe, IO-free subset mirrored from the engine compiler; no expression is
    /// evaluated beyond the supported <c>use-when</c> literal subset and no resource is acquired.
    /// </summary>
    StructuralChecks,
}
