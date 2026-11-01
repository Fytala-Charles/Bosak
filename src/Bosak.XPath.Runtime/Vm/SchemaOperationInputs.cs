// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 01 November 2026
// PURPOSE              : Holds bounded immutable input bindings for isolated private host evaluation.
// SPECIAL NOTES        : Part of the register-based virtual machine execution engine.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 01-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Core.Xdm;

namespace Bosak.XPath.Runtime.Vm;

/// <summary>Copied bounded data; node graphs remain borrowed from their admitted owner.</summary>
internal sealed class SchemaOperationInputs
{
    private readonly XdmValue _focus;
    private readonly int _position, _size;
    private readonly KeyValuePair<(string LocalName, string NamespaceUri), XdmValue>[] _variables;
    private readonly KeyValuePair<string, string>[] _namespaces;
    private readonly string? _baseUri, _defaultNamespace, _definingNamespace;
    private readonly string _collation;
    internal SchemaEvidenceKernel.Snapshot? Snapshot { get; }

    // Capture owns these freshly copied arrays; no caller-owned array is retained.
    internal SchemaOperationInputs(XdmValue focus, int position, int size,
        KeyValuePair<(string LocalName, string NamespaceUri), XdmValue>[] variables,
        KeyValuePair<string, string>[] namespaces, string? baseUri,
        string? defaultNamespace, string? definingNamespace, string collation, SchemaEvidenceKernel.Snapshot? snapshot)
    {
        _focus = focus; _position = position; _size = size;
        _variables = variables; _namespaces = namespaces;
        _baseUri = baseUri; _defaultNamespace = defaultNamespace;
        _definingNamespace = definingNamespace; _collation = collation;
        Snapshot = snapshot;
    }

    internal EvaluationContext CreateContext()
    {
        var context = new EvaluationContext
        {
            BaseUri = _baseUri, DefaultElementNamespace = _defaultNamespace,
            DefiningElementDefaultNamespace = _definingNamespace, DefaultCollation = _collation
        };
        context.WithFocus(_focus, _position, _size);
        foreach (var pair in _variables) context.WithVariable(pair.Key.LocalName, pair.Value, pair.Key.NamespaceUri);
        foreach (var pair in _namespaces) context.WithNamespace(pair.Key, pair.Value);
        return context;
    }
}

/// <summary>Borrowed flat atomic visitation guarded by the actual operation, without detachment.</summary>
internal readonly struct SchemaAtomicResult
{
    private readonly SchemaQuerySession.Operation? _operation;
    private readonly XdmValue _value;
    private readonly int _maxAtoms;
    internal SchemaAtomicResult(SchemaQuerySession.Operation operation, XdmValue value, int maxAtoms)
    { _operation = operation; _value = value; _maxAtoms = maxAtoms; }

    internal void ForEachAtomic(Action<XdmValue> visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        var operation = _operation ?? throw new InvalidOperationException("Uninitialized atomic result.");
        operation.ValidateAccess();
        if (_value.IsUndefined) return;
        int maxAtoms = _maxAtoms;
        int count = 0;
        if (!_value.IsSequence) { Visit(_value); return; }
        var enumerator = XdmSequence.FromSource(_value.SequenceValue!).GetEnumerator();
        while (true)
        {
            operation.ValidateAccess();
            if (!enumerator.MoveNext()) break;
            operation.ValidateAccess();
            Visit(enumerator.Current);
        }
        void Visit(XdmValue value)
        {
            operation.ValidateAccess();
            if (!value.IsAtomic) throw new NotSupportedException("Only flat atomic results are admitted.");
            if (count == maxAtoms) throw new NotSupportedException("Atomic result limit reached.");
            count++;
            visitor(value);
        }
    }
}
