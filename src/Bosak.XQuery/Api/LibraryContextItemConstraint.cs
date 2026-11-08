// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 28 October 2026
// PURPOSE              : Capture and match library context types within their declaring namespace context.
// SPECIAL NOTES        : Part of the Bosak XQuery 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 28-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Text;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Bosak.XQuery.Compiler;

namespace Bosak.XQuery.Api;

/// <summary>A library context-item constraint with its immutable declaration environment.</summary>
internal sealed class LibraryContextItemConstraint
{
    private const string SchemaNamespace = "http://www.w3.org/2001/XMLSchema";
    private readonly Dictionary<string, string> _namespaces;
    private readonly string? _defaultElementNamespace;
    private readonly string _matchingType;

    /// <summary>The original declared type text, retained for diagnostics.</summary>
    internal string TypeName { get; }

    /// <summary>Captures a parsed library declaration and canonicalizes its atomic type names.</summary>
    /// <param name="declaration">The declaring module's static context.</param>
    internal LibraryContextItemConstraint(XQueryStaticContext declaration)
    {
        TypeName = declaration.ContextItemTypeName ?? throw new ArgumentException("A typed library declaration is required.", nameof(declaration));
        _namespaces = new Dictionary<string, string>(declaration.Namespaces, StringComparer.Ordinal);
        _defaultElementNamespace = declaration.DefaultElementNamespace;
        _matchingType = CanonicalizeType(TypeName);
    }

    /// <summary>Matches with declaration bindings and restores the importing context in all cases.</summary>
    /// <param name="value">The supplied initial context item.</param>
    /// <param name="context">The prepared schema/type matching context.</param>
    /// <returns>Whether the value satisfies this module's constraint.</returns>
    internal bool Matches(XdmValue value, EvaluationContext context)
    {
        var namespaces = context.SnapshotNamespaces();
        var defaultNamespace = context.DefaultElementNamespace;
        try
        {
            context.RestoreNamespaces(_namespaces);
            context.DefaultElementNamespace = _defaultElementNamespace;
            return XdmConversions.ValueMatchesType(value, _matchingType, context);
        }
        finally
        {
            context.RestoreNamespaces(namespaces);
            context.DefaultElementNamespace = defaultNamespace;
        }
    }

    // The source parser has already accepted ItemType syntax and removed comments.
    // Walk QName tokens without changing quoted targets, EQName URIs or node-name roles.
    private string CanonicalizeType(string source)
    {
        var result = new StringBuilder(source.Length);
        var frames = new List<(string Kind, int Argument)>();
        string? pendingKind = null;
        char previous = '\0';
        for (int i = 0; i < source.Length;)
        {
            char c = source[i];
            if (char.IsWhiteSpace(c)) { result.Append(c); i++; continue; }
            if (c is '\'' or '"')
            {
                char quote = c;
                result.Append(c); i++;
                while (i < source.Length)
                {
                    c = source[i++]; result.Append(c);
                    if (c != quote) continue;
                    if (i < source.Length && source[i] == quote) { result.Append(source[i++]); continue; }
                    break;
                }
                previous = quote;
                pendingKind = null;
                continue;
            }
            if (IsNameStart(c))
            {
                int start = i;
                if (c == 'Q' && i + 1 < source.Length && source[i + 1] == '{')
                {
                    int close = source.IndexOf('}', i + 2);
                    if (close < 0) throw new InvalidOperationException("XPST0003: Unterminated type EQName.");
                    i = close + 1;
                    while (i < source.Length && IsNamePart(source[i])) i++;
                }
                else
                {
                    while (i < source.Length && IsNamePart(source[i])) i++;
                    if (i < source.Length && source[i] == ':')
                    {
                        i++;
                        if (i < source.Length && source[i] == '*') i++;
                        else while (i < source.Length && IsNamePart(source[i])) i++;
                    }
                }
                string token = source[start..i];
                int next = i;
                while (next < source.Length && char.IsWhiteSpace(source[next])) next++;
                bool constructor = next < source.Length && source[next] == '(' && IsConstructor(token);
                bool returnKeyword = token == "as" && previous == ')';
                bool typeRole = frames.Count == 0 || !IsNodeNameRole(frames[^1]);
                result.Append(constructor || returnKeyword || !typeRole ? token : ExpandAtomicName(token));
                pendingKind = constructor ? token : null;
                previous = token[^1];
                continue;
            }
            if (c == '(') { frames.Add((pendingKind ?? "group", 0)); pendingKind = null; }
            else if (c == ')' && frames.Count != 0) frames.RemoveAt(frames.Count - 1);
            else if (c == ',' && frames.Count != 0)
            {
                var frame = frames[^1];
                frames[^1] = (frame.Kind, frame.Argument + 1);
            }
            result.Append(c); i++; previous = c;
        }
        return result.ToString();
    }

    private string ExpandAtomicName(string token)
    {
        string ns, local;
        if (token.StartsWith("Q{", StringComparison.Ordinal))
        {
            int close = token.IndexOf('}');
            ns = token[2..close]; local = token[(close + 1)..];
        }
        else if (token.IndexOf(':') is int colon && colon >= 0)
        {
            string prefix = token[..colon];
            if (!_namespaces.TryGetValue(prefix, out ns!))
                throw new InvalidOperationException($"XPST0081: Prefix '{prefix}' is not declared in the library context type.");
            local = token[(colon + 1)..];
        }
        else
        {
            ns = _defaultElementNamespace ?? string.Empty;
            local = token;
        }
        return ns == SchemaNamespace ? "xs:" + local : "Q{" + ns + "}" + local;
    }

    private static bool IsNodeNameRole((string Kind, int Argument) frame) =>
        frame.Kind == "processing-instruction" || frame.Argument == 0 && frame.Kind is "element" or "attribute" or "schema-element" or "schema-attribute";

    private static bool IsConstructor(string token) => token is "item" or "node" or "text" or "comment" or
        "processing-instruction" or "element" or "attribute" or "schema-element" or "schema-attribute" or
        "document-node" or "function" or "map" or "array" or "empty-sequence";

    private static bool IsNameStart(char c) => char.IsLetter(c) || c == '_' || c >= 0x80;
    private static bool IsNamePart(char c) => IsNameStart(c) && !char.IsWhiteSpace(c) || char.IsDigit(c) || c is '-' or '.';
}
