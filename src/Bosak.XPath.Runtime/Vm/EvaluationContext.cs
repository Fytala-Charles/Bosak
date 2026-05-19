// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Runtime evaluation context for XPath expressions. Holds the focus (context item, position, size),...
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Functions;

namespace Bosak.XPath.Runtime.Vm;

/// <summary>
/// Runtime evaluation context for XPath expressions.
/// Holds the focus (context item, position, size), variable bindings,
/// namespace resolution, and function libraries.
/// </summary>
public sealed class EvaluationContext
{
    // Focus
    private XdmValue _contextItem;
    private int _contextPosition;
    private int _contextSize;

    // Variable bindings: QName key -> XdmValue
    private readonly Dictionary<(string LocalName, string NamespaceUri), XdmValue> _variables;

    // Namespace prefixes
    private readonly Dictionary<string, string> _namespaces;

    // Function libraries, indexed by (namespace, localName, arity)
    private readonly Dictionary<(string, string, int), FunctionSignature> _functions;

    public EvaluationContext()
    {
        _contextItem = XdmValue.Undefined;
        _contextPosition = 0;
        _contextSize = 0;
        _variables = new Dictionary<(string, string), XdmValue>();
        _namespaces = new Dictionary<string, string>
        {
            ["xml"] = "http://www.w3.org/XML/1998/namespace",
            ["xs"] = "http://www.w3.org/2001/XMLSchema",
            ["fn"] = "http://www.w3.org/2005/xpath-functions",
            ["math"] = "http://www.w3.org/2005/xpath-functions/math",
            ["map"] = "http://www.w3.org/2005/xpath-functions/map",
            ["array"] = "http://www.w3.org/2005/xpath-functions/array"
        };
        _functions = new Dictionary<(string, string, int), FunctionSignature>();
    }

    // ------------------------------------------------------------------
    // Focus
    // ------------------------------------------------------------------

    public XdmValue ContextItem => _contextItem;
    public int ContextPosition => _contextPosition;
    public int ContextSize => _contextSize;

    public EvaluationContext WithFocus(XdmValue item, int position, int size)
    {
        _contextItem = item;
        _contextPosition = position;
        _contextSize = size;
        return this;
    }

    // ------------------------------------------------------------------
    // Variables
    // ------------------------------------------------------------------

    public EvaluationContext WithVariable(string localName, XdmValue value, string namespaceUri = "")
    {
        _variables[(localName, namespaceUri)] = value;
        return this;
    }

    public bool TryGetVariable(string localName, out XdmValue value, string namespaceUri = "")
        => _variables.TryGetValue((localName, namespaceUri), out value);

    public bool RemoveVariable(string localName, string namespaceUri = "")
        => _variables.Remove((localName, namespaceUri));

    // ------------------------------------------------------------------
    // Namespaces
    // ------------------------------------------------------------------

    public EvaluationContext WithNamespace(string prefix, string namespaceUri)
    {
        _namespaces[prefix] = namespaceUri;
        return this;
    }

    public bool TryResolveNamespace(string prefix, out string namespaceUri)
        => _namespaces.TryGetValue(prefix, out namespaceUri!);

    // ------------------------------------------------------------------
    // Functions
    // ------------------------------------------------------------------

    public EvaluationContext RegisterFunction(FunctionSignature signature)
    {
        var key = (signature.NamespaceUri, signature.LocalName, signature.Arity);
        _functions[key] = signature;
        return this;
    }

    public bool TryResolveFunction(string namespaceUri, string localName, int arity, out FunctionSignature signature)
        => _functions.TryGetValue((namespaceUri, localName, arity), out signature!);
}
