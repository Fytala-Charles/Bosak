// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 08 October 2026
// PURPOSE              : A structural record-type annotation describing the fields a record (annotated map) must have.
// SPECIAL NOTES        : Foundation types for the XQuery Data Model; used by all higher layers.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 08-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.XPath.Core.Xdm;

/// <summary>
/// A structural record-type annotation as defined by XPath 4.0 §3.2.10. A record is a map
/// whose structure is constrained by a record type; the annotation stores the declared
/// fields (in declaration order) and the verbatim type text.
/// </summary>
public sealed class XdmRecordType
{
    private readonly Dictionary<string, XdmRecordField> _fieldsByName;

    /// <summary>
    /// Creates a record-type annotation for a typed record declaration such as
    /// <c>record(first as xs:string, last as xs:string)</c>.
    /// </summary>
    /// <param name="fields">The declared fields in declaration order.</param>
    /// <param name="typeText">The verbatim, normalized type text (without surrounding whitespace).</param>
    public XdmRecordType(IReadOnlyList<XdmRecordField> fields, string typeText)
    {
        Fields = fields;
        TypeText = typeText;
        _fieldsByName = new Dictionary<string, XdmRecordField>(StringComparer.Ordinal);
        foreach (var field in fields)
            _fieldsByName[field.Name] = field;
    }

    /// <summary>
    /// Creates the wildcard record-type annotation <c>record(*)</c>, which matches any map
    /// that carries a record annotation.
    /// </summary>
    private XdmRecordType()
    {
        Fields = Array.Empty<XdmRecordField>();
        TypeText = "record(*)";
        _fieldsByName = new Dictionary<string, XdmRecordField>(StringComparer.Ordinal);
    }

    /// <summary>Gets the shared <c>record(*)</c> annotation.</summary>
    public static XdmRecordType Any { get; } = new XdmRecordType();

    /// <summary>Gets whether this annotation is the wildcard <c>record(*)</c>.</summary>
    public bool IsAny => ReferenceEquals(this, Any);

    /// <summary>Gets the declared fields in declaration order.</summary>
    public IReadOnlyList<XdmRecordField> Fields { get; }

    /// <summary>Gets the verbatim, normalized record-type text, e.g. <c>record(first as xs:string)</c>.</summary>
    public string TypeText { get; }

    /// <summary>Returns whether a field with the given name is declared.</summary>
    /// <param name="name">The field name (NCName or string-literal contents).</param>
    public bool HasField(string name) => _fieldsByName.ContainsKey(name);

    /// <summary>Attempts to retrieve the declared field with the given name.</summary>
    /// <param name="name">The field name (NCName or string-literal contents).</param>
    /// <param name="field">The declared field, or <see langword="null"/> when absent.</param>
    public bool TryGetField(string name, out XdmRecordField? field) => _fieldsByName.TryGetValue(name, out field);
}
