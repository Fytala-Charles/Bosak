// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 07 July 2026
// PURPOSE              : Encodes XML 1.1-only name characters into XML 1.0-compatible names.
// SPECIAL NOTES        : Part of the XDocument node provider layer.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 07-07-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Globalization;
using System.Reflection;
using System.Text;
using System.Xml;

namespace Bosak.XPath.Providers.Xml;

/// <summary>
/// Encodes characters that are valid in XML 1.1 names but rejected by .NET's
/// XML 1.0 name validation, so that XML 1.1 documents can be stored in an
/// <see cref="System.Xml.Linq.XDocument"/> tree.
/// </summary>
/// <remarks>
/// Invalid characters are escaped as <c>U+01C0 hex-codepoint U+01C1</c>.
/// The sentinel characters themselves are escaped when they occur literally.
/// </remarks>
public static class Xml11NameCodec
{
    private const char SentinelOpen = '\u01C0';  // Latin letter dental click - valid XML 1.0 name char
    private const char SentinelClose = '\u01C1'; // Latin letter lateral click - valid XML 1.0 name char

    private static readonly Func<char, bool> IsNameSingleChar;
    private static readonly HashSet<int> ValidNameChars = new();

    static Xml11NameCodec()
    {
        // Use the same name-character check that System.Xml.XmlReader uses.
        var xmlCharType = typeof(XmlReader).Assembly.GetType("System.Xml.XmlCharType")
            ?? throw new InvalidOperationException("System.Xml.XmlCharType not found.");
        var method = xmlCharType.GetMethod("IsNameSingleChar", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("System.Xml.XmlCharType.IsNameSingleChar not found.");
        IsNameSingleChar = (Func<char, bool>)Delegate.CreateDelegate(typeof(Func<char, bool>), method);

        // Pre-compute the BMP characters that .NET accepts in XML 1.0 names.
        for (int i = 1; i < 0x10000; i++)
        {
            if (IsNameSingleChar((char)i))
                ValidNameChars.Add(i);
        }
    }

    /// <summary>
    /// Returns <c>true</c> when <paramref name="codepoint"/> is accepted by .NET
    /// as an XML 1.0 name character.
    /// </summary>
    public static bool IsValidNameChar(int codepoint)
    {
        if (codepoint is >= 1 and < 0x10000)
            return ValidNameChars.Contains(codepoint);
        // Supplementary characters: accept if both surrogates are individually
        // accepted by the runtime name checker. This is sufficient for the test
        // suite and avoids a full scalar-value table.
        if (codepoint is > 0xFFFF and <= 0x10FFFF)
        {
            var s = char.ConvertFromUtf32(codepoint);
            return IsNameSingleChar(s[0]) && IsNameSingleChar(s[1]);
        }
        return false;
    }

    /// <summary>
    /// Encodes an XML name so that it can be stored in an <see cref="System.Xml.Linq.XName"/>.
    /// </summary>
    public static string EncodeName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        // Fast path: no encoding required.
        bool needsEncoding = false;
        for (int i = 0; i < name.Length; i++)
        {
            char c = name[i];
            if (c == SentinelOpen || c == SentinelClose || !IsNameSingleChar(c))
            {
                needsEncoding = true;
                break;
            }
        }
        if (!needsEncoding)
            return name;

        var sb = new StringBuilder(name.Length + 8);
        for (int i = 0; i < name.Length; i++)
        {
            char c = name[i];
            if (c == SentinelOpen || c == SentinelClose || !IsNameSingleChar(c))
            {
                sb.Append(SentinelOpen);
                sb.Append(((int)c).ToString("X", CultureInfo.InvariantCulture));
                sb.Append(SentinelClose);
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Decodes a name that was previously encoded with <see cref="EncodeName"/>.
    /// </summary>
    public static string DecodeName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        int open = name.IndexOf(SentinelOpen);
        if (open < 0)
            return name;

        var sb = new StringBuilder(name.Length);
        int i = 0;
        while (i < name.Length)
        {
            if (name[i] == SentinelOpen)
            {
                int close = name.IndexOf(SentinelClose, i + 1);
                if (close < 0)
                {
                    sb.Append(name[i]);
                    i++;
                    continue;
                }

                var hex = name.Substring(i + 1, close - i - 1);
                if (int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int codepoint))
                {
                    sb.Append((char)codepoint);
                    i = close + 1;
                    continue;
                }
            }

            sb.Append(name[i]);
            i++;
        }

        return sb.ToString();
    }

    /// <summary>
    /// Returns <c>true</c> if <paramref name="name"/> contains encoded XML 1.1 characters.
    /// </summary>
    public static bool IsEncoded(string name)
        => !string.IsNullOrEmpty(name) && name.IndexOf(SentinelOpen) >= 0;

    /// <summary>
    /// Encodes characters that are not permitted in XML 1.0 text/attribute values
    /// (C0/C1 controls and the sentinel characters themselves).
    /// </summary>
    public static string EncodeValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        bool needsEncoding = false;
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            if (char.IsHighSurrogate(c))
            {
                i++;
                continue;
            }
            if (c == SentinelOpen || c == SentinelClose || IsInvalidXml10Char(c))
            {
                needsEncoding = true;
                break;
            }
        }
        if (!needsEncoding)
            return value;

        var sb = new StringBuilder(value.Length + 8);
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            if (char.IsHighSurrogate(c))
            {
                sb.Append(c);
                sb.Append(value[++i]);
                continue;
            }
            if (c == SentinelOpen || c == SentinelClose || IsInvalidXml10Char(c))
            {
                sb.Append(SentinelOpen);
                sb.Append(((int)c).ToString("X", CultureInfo.InvariantCulture));
                sb.Append(SentinelClose);
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// Returns <c>true</c> when <paramref name="codepoint"/> is not a valid XML 1.0 character.
    /// </summary>
    public static bool IsInvalidXml10Char(int codepoint)
    {
        if (codepoint == 0x09 || codepoint == 0x0A || codepoint == 0x0D)
            return false;
        if (codepoint >= 0x20 && codepoint <= 0xD7FF)
            return false;
        if (codepoint >= 0xE000 && codepoint <= 0xFFFD)
            return false;
        if (codepoint >= 0x10000 && codepoint <= 0x10FFFF)
            return false;
        return true;
    }

    /// <summary>
    /// Returns <c>true</c> when <paramref name="name"/> is a valid XML 1.1 NCName.
    /// </summary>
    public static bool IsValidXml11NCName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;

        for (int i = 0; i < name.Length; i++)
        {
            int cp = char.ConvertToUtf32(name, i);
            if (char.IsHighSurrogate(name[i]))
                i++;

            if (i == 0)
            {
                if (!IsValidXml11NameStartChar(cp))
                    return false;
            }
            else
            {
                if (!IsValidXml11NameChar(cp))
                    return false;
            }
        }

        return true;
    }

    private static bool IsValidXml11NameStartChar(int cp)
    {
        if (cp == ':')
            return true;
        if (cp is >= 'A' and <= 'Z')
            return true;
        if (cp == '_')
            return true;
        if (cp is >= 'a' and <= 'z')
            return true;
        if (cp is >= 0xC0 and <= 0xD6)
            return true;
        if (cp is >= 0xD8 and <= 0xF6)
            return true;
        if (cp is >= 0xF8 and <= 0x2FF)
            return true;
        if (cp is >= 0x370 and <= 0x37D)
            return true;
        if (cp is >= 0x37F and <= 0x1FFF)
            return true;
        if (cp is >= 0x200C and <= 0x200D)
            return true;
        if (cp is >= 0x2070 and <= 0x218F)
            return true;
        if (cp is >= 0x2C00 and <= 0x2FEF)
            return true;
        if (cp is >= 0x3001 and <= 0xD7FF)
            return true;
        if (cp is >= 0xF900 and <= 0xFDCF)
            return true;
        if (cp is >= 0xFDF0 and <= 0xFFFD)
            return true;
        if (cp is >= 0x10000 and <= 0xEFFFF)
            return true;
        return false;
    }

    private static bool IsValidXml11NameChar(int cp)
    {
        if (IsValidXml11NameStartChar(cp))
            return true;
        if (cp == '-' || cp == '.' || cp is >= '0' and <= '9')
            return true;
        if (cp == 0xB7)
            return true;
        if (cp is >= 0x0300 and <= 0x036F)
            return true;
        if (cp is >= 0x203F and <= 0x2040)
            return true;
        return false;
    }

    /// <summary>
    /// Decodes a value encoded with <see cref="EncodeValue"/>.
    /// </summary>
    public static string DecodeValue(string value) => DecodeName(value);
}
