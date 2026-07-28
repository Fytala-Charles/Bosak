// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 27 July 2026
// PURPOSE              : Structured error representation and catch-clause matching for try/catch expressions.
// SPECIAL NOTES        : Part of the register-based virtual machine execution engine.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 27-07-2026     | Creation — XPathErrorException, GetErrorDetails, catch pattern matching, err:* binding   |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Parser.Ast;

namespace Bosak.XPath.Runtime.Vm;

/// <summary>
/// A dynamic error with a structured XPath error code, description, and optional error
/// value, thrown by <c>fn:error</c> and matched by try/catch code patterns.
/// </summary>
public sealed class XPathErrorException : InvalidOperationException
{
    /// <summary>The error code namespace URI (the xqt-errors namespace for standard codes).</summary>
    public string CodeNamespaceUri { get; }

    /// <summary>The error code local name (e.g. <c>FOER0000</c>).</summary>
    public string CodeLocalName { get; }

    /// <summary>The error code prefix, if one was supplied (empty otherwise).</summary>
    public string CodePrefix { get; }

    /// <summary>The error value (third argument of <c>fn:error</c>), or undefined.</summary>
    public XdmValue ErrorValue { get; }

    /// <summary>Creates an XPath error with a structured code and description.</summary>
    /// <param name="namespaceUri">The error code namespace URI.</param>
    /// <param name="localName">The error code local name.</param>
    /// <param name="prefix">The error code prefix (empty when unknown).</param>
    /// <param name="description">The error description.</param>
    /// <param name="errorValue">The optional error value.</param>
    public XPathErrorException(string namespaceUri, string localName, string? prefix, string description, XdmValue errorValue = default)
        : base(description)
    {
        CodeNamespaceUri = namespaceUri;
        CodeLocalName = localName;
        CodePrefix = prefix ?? string.Empty;
        ErrorValue = errorValue;
    }
}

/// <summary>
/// Marks an exception raised while lazily evaluating a global (prolog) variable initializer.
/// Per the XQuery specification, errors occurring during global variable evaluation are NOT
/// caught by try/catch expressions (try-006/007); the VM rethrows these immediately. The
/// <see cref="InvalidOperationException.Message"/> mirrors the wrapped exception so error
/// reporting is unaffected.
/// </summary>
public sealed class GlobalVariableEvaluationException : InvalidOperationException
{
    /// <summary>Creates the marker around the exception thrown by the initializer.</summary>
    /// <param name="inner">The exception raised by the global variable initializer.</param>
    public GlobalVariableEvaluationException(Exception inner)
        : base(inner.Message, inner)
    {
    }
}

/// <summary>The decomposed parts of an error exposed to catch clauses and the <c>err:*</c> variables.</summary>
public readonly record struct ErrorDetails(
    string NamespaceUri,
    string LocalName,
    string? Prefix,
    string Description,
    XdmValue Value);

/// <summary>
/// Helpers for try/catch error handling: extracting structured error details from
/// exceptions, matching catch-clause code patterns, and binding the <c>err:*</c> variables.
/// </summary>
public static class XPathError
{
    /// <summary>The standard XPath/XQuery error namespace.</summary>
    public const string ErrNs = "http://www.w3.org/2005/xqt-errors";

    /// <summary>
    /// Parses an exception into structured error details. <see cref="XPathErrorException"/>
    /// is used directly; <c>fn:error(Q{uri}local): description</c>, <c>CODE: description</c>,
    /// and bare <c>CODE</c> messages are parsed back; anything else falls back to the CLR
    /// type name in no namespace.
    /// </summary>
    /// <param name="ex">The exception to decompose.</param>
    /// <returns>The structured error details.</returns>
    public static ErrorDetails GetErrorDetails(Exception ex)
    {
        if (ex is XPathErrorException xee)
        {
            return new ErrorDetails(xee.CodeNamespaceUri, xee.CodeLocalName, xee.CodePrefix, xee.Message, xee.ErrorValue);
        }

        if (ex is InvalidOperationException ioe)
        {
            var message = ioe.Message;

            // fn:error() messages are formatted as "fn:error(Q{uri}local): description".
            if (message.StartsWith("fn:error(", StringComparison.Ordinal))
            {
                var qnameStart = "fn:error(".Length;
                var qnameEnd = message.IndexOf(')', qnameStart);
                if (qnameEnd > qnameStart)
                {
                    var qname = message[qnameStart..qnameEnd];
                    string ns;
                    string local;
                    if (qname.Length > 2 && qname[0] == 'Q' && qname[1] == '{')
                    {
                        var close = qname.IndexOf('}');
                        ns = close > 2 ? qname[2..close] : string.Empty;
                        local = close >= 0 && close < qname.Length - 1 ? qname[(close + 1)..] : qname;
                    }
                    else
                    {
                        ns = string.Empty;
                        local = qname;
                    }

                    var descriptionStart = qnameEnd + 1;
                    if (descriptionStart < message.Length && message[descriptionStart] == ':')
                        descriptionStart++;
                    if (descriptionStart < message.Length && message[descriptionStart] == ' ')
                        descriptionStart++;
                    var description = descriptionStart < message.Length ? message[descriptionStart..] : string.Empty;

                    return new ErrorDetails(ns, local, null, description, XdmValue.Undefined);
                }
            }

            // Standard "CODE: description" format used for XPath/XQuery dynamic errors.
            var colon = message.IndexOf(':');
            if (colon > 0 && IsErrorCode(message[..colon]))
            {
                var code = message[..colon];
                var descStart = colon + 1;
                if (descStart < message.Length && message[descStart] == ' ')
                    descStart++;
                var description = descStart < message.Length ? message[descStart..] : string.Empty;
                return new ErrorDetails(ErrNs, code, "err", description, XdmValue.Undefined);
            }

            // Some functions throw a bare error code (e.g. "FOUT1190").
            if (IsErrorCode(message))
                return new ErrorDetails(ErrNs, message, "err", message, XdmValue.Undefined);
        }

        return new ErrorDetails(string.Empty, ex.GetType().Name, null, ex.Message, XdmValue.Undefined);
    }

    /// <summary>
    /// True when an error carries a static error code (XPST/XQST) and was NOT raised by
    /// <c>fn:error</c>. try/catch catches dynamic errors only; static errors propagate
    /// even when a catch pattern would match them (try-catch-static-error-1..4).
    /// </summary>
    /// <param name="ex">The original exception.</param>
    /// <param name="details">The parsed error details.</param>
    /// <returns>True when the error must bypass catch clauses.</returns>
    public static bool IsUncatchableStaticError(Exception ex, in ErrorDetails details)
        => ex is not XPathErrorException
           && details.NamespaceUri == ErrNs
           && (details.LocalName.StartsWith("XPST", StringComparison.Ordinal)
               || details.LocalName.StartsWith("XQST", StringComparison.Ordinal));

    /// <summary>
    /// Tests whether one catch-clause code pattern matches an error. Pattern prefixes are
    /// resolved against the evaluation context's in-scope namespaces.
    /// </summary>
    /// <param name="pattern">The catch code pattern.</param>
    /// <param name="error">The error details to test.</param>
    /// <param name="context">The evaluation context (for prefix resolution).</param>
    /// <returns>True when the pattern selects the error.</returns>
    public static bool CatchPatternMatches(CatchCodePattern pattern, in ErrorDetails error, EvaluationContext context)
    {
        // '*' matches every error.
        if (pattern.Prefix is null && pattern.LocalName is null && pattern.NamespaceUri is null)
            return true;

        string? nsPattern;
        if (pattern.Prefix == "*")
        {
            nsPattern = null; // '*:local' — any namespace
        }
        else if (pattern.NamespaceUri is not null)
        {
            nsPattern = pattern.NamespaceUri;
        }
        else if (pattern.Prefix is not null)
        {
            if (!context.TryResolveNamespace(pattern.Prefix, out var resolved))
                return false; // an unresolvable prefix never matches
            nsPattern = resolved;
        }
        else
        {
            nsPattern = null;
        }

        if (nsPattern is not null && nsPattern != error.NamespaceUri)
            return false;
        if (pattern.LocalName is not null && pattern.LocalName != error.LocalName)
            return false;
        // A pattern with neither a namespace nor a local constraint can only be '*'
        // (handled above); any other constraint form that reached here matched.
        return true;
    }

    /// <summary>
    /// Binds the seven <c>err:*</c> variables for a caught error, returning the previous
    /// bindings for later restore (nested try/catch). <c>err:additional</c> holds
    /// implementation-defined error information and is bound to the empty sequence here.
    /// </summary>
    /// <param name="context">The evaluation context.</param>
    /// <param name="error">The caught error.</param>
    /// <returns>The previous values of the error variables.</returns>
    public static (XdmValue Code, XdmValue Description, XdmValue Value, XdmValue Module, XdmValue Line, XdmValue Column, XdmValue Additional)
        BindCatchErrorVariables(EvaluationContext context, in ErrorDetails error)
    {
        context.TryGetVariable("code", out var prevCode, ErrNs);
        context.TryGetVariable("description", out var prevDesc, ErrNs);
        context.TryGetVariable("value", out var prevValue, ErrNs);
        context.TryGetVariable("module", out var prevModule, ErrNs);
        context.TryGetVariable("line-number", out var prevLine, ErrNs);
        context.TryGetVariable("column-number", out var prevColumn, ErrNs);
        context.TryGetVariable("additional", out var prevAdditional, ErrNs);

        var codePrefix = string.IsNullOrEmpty(error.NamespaceUri)
            ? string.Empty
            : (error.Prefix ?? "err");
        context.WithVariable("code", XdmValue.FromQName(new XsQName(error.LocalName, error.NamespaceUri, codePrefix)), ErrNs);
        context.WithVariable("description", XdmValue.FromString(error.Description), ErrNs);
        context.WithVariable("value", error.Value, ErrNs);
        // Source module/line/column information is not tracked by the VM; the variables
        // are defined as empty/zero per the XQuery error vocabulary.
        context.WithVariable("module", XdmValue.FromString(string.Empty), ErrNs);
        context.WithVariable("line-number", XdmValue.FromInteger(0), ErrNs);
        context.WithVariable("column-number", XdmValue.FromInteger(0), ErrNs);
        // err:additional is implementation-defined; this implementation provides none.
        context.WithVariable("additional", XdmValue.Undefined, ErrNs);

        return (prevCode, prevDesc, prevValue, prevModule, prevLine, prevColumn, prevAdditional);
    }

    /// <summary>
    /// Restores the error-variable bindings captured by <see cref="BindCatchErrorVariables"/>.
    /// </summary>
    /// <param name="context">The evaluation context.</param>
    /// <param name="previous">The previous bindings.</param>
    public static void RestoreCatchErrorVariables(
        EvaluationContext context,
        (XdmValue Code, XdmValue Description, XdmValue Value, XdmValue Module, XdmValue Line, XdmValue Column, XdmValue Additional) previous)
    {
        context.WithVariable("code", previous.Code, ErrNs);
        context.WithVariable("description", previous.Description, ErrNs);
        context.WithVariable("value", previous.Value, ErrNs);
        context.WithVariable("module", previous.Module, ErrNs);
        context.WithVariable("line-number", previous.Line, ErrNs);
        context.WithVariable("column-number", previous.Column, ErrNs);
        context.WithVariable("additional", previous.Additional, ErrNs);
    }

    private static bool IsErrorCode(string token)
    {
        if (token.Length != 8)
            return false;
        for (int i = 0; i < 4; i++)
            if (!char.IsUpper(token[i]))
                return false;
        for (int i = 4; i < 8; i++)
            if (!char.IsDigit(token[i]))
                return false;
        return true;
    }
}
