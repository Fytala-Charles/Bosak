// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 30 October 2026
// PURPOSE              : Probes existing lazy-result ownership using disposable in-memory fixtures.
// SPECIAL NOTES        : Observation only; no engine changes, stores, external resources or API acceptance.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 30-10-2026     | Creation; bounded result ownership observations                                   |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using System.Text.Json;
using System.Xml;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Streaming;

if (args.Length != 1) throw new ArgumentException("Usage: ownership-probe <output-json>");
var results = new List<object>();
void Check(string id, bool condition, object observation)
{
    results.Add(new { id, observedAsExpected = condition, observation });
    if (!condition) throw new InvalidOperationException("Observation changed: " + id);
}
MemoryStream Input() => new(Encoding.UTF8.GetBytes("<root><item>1</item><item>2</item></root>"));
StreamingLoadOptions Options(bool close = true) => new() { ReaderSettings = new XmlReaderSettings { CloseInput = close, DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null } };
XdmSequence Records(IXdmNode doc)
{
    var roots = doc.Children(XdmNodeKind.Element).GetEnumerator();
    if (!roots.MoveNext()) throw new InvalidOperationException("Missing fixture root");
    return roots.Current.NodeValue.Children(XdmNodeKind.Element);
}

var source = new CountingSequence();
var sequence = XdmSequence.FromSource(source);
var deferred = sequence.GetEnumerator();
Check("LO-01", source.Started == 0, new { source.Started, note = "Public enumerator creation does not start the source" });
foreach (var value in sequence) { break; }
Check("LO-02", source.Started == 1 && source.Disposed == 0, new { source.Started, source.Disposed, note = "Break does not call optional concrete IDisposable" });
source.Last!.Dispose();

using (var input = Input())
{
    var doc = XmlStreamingProvider.Load(input, Options());
    var count = 0; foreach (var value in Records(doc)) count++;
    Check("LO-03", count == 2 && !input.CanRead, new { count, input.CanRead, note = "Normal completion closes owned reader/input with CloseInput=true" });
}
using (var input = Input())
{
    var doc = XmlStreamingProvider.Load(input, Options());
    foreach (var value in Records(doc)) { break; }
    Check("LO-04", input.CanRead, new { input.CanRead, note = "Early direct-axis exit does not finish or close the stream; fixture caller disposes it" });
}
using (var input = Input())
{
    var doc = XmlStreamingProvider.Load(input, Options(false));
    foreach (var value in Records(doc)) { }
    Check("LO-05", input.CanRead, new { input.CanRead, note = "CloseInput=false preserves caller stream even after completion" });
}
using (var input = Input())
using (var reader = XmlReader.Create(input, new XmlReaderSettings { CloseInput = true, DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null }))
{
    var doc = XmlStreamingProvider.Load(reader);
    foreach (var value in Records(doc)) { }
    Check("LO-06", reader.ReadState != ReadState.Closed && input.CanRead, new { readState = reader.ReadState.ToString(), input.CanRead, note = "Supplied reader remains caller-owned at completion" });
}
using (var input = Input())
{
    var doc = XmlStreamingProvider.Load(input, Options());
    var callbacks = 0;
    ((IStreamingDocument)doc).StreamCompleted += () => { callbacks++; throw new InvalidOperationException("owned completion fixture"); };
    var caught = false;
    try { foreach (var value in Records(doc)) { } }
    catch (InvalidOperationException ex) when (ex.Message == "owned completion fixture") { caught = true; }
    Check("LO-07", caught && callbacks == 1 && input.CanRead, new { caught, callbacks, input.CanRead, note = "Throwing completion callback precedes reader disposal; fixture caller closes input" });
}
using (var input = Input())
{
    var options = Options();
    options.RecordPostProcessor = (_, _) => throw new InvalidOperationException("owned record fixture");
    var doc = XmlStreamingProvider.Load(input, options);
    var caught = false;
    try { foreach (var value in Records(doc)) { } }
    catch (InvalidOperationException ex) when (ex.Message == "owned record fixture") { caught = true; }
    Check("LO-08", caught && input.CanRead, new { caught, input.CanRead, note = "Record callback failure leaves source unfinished; caller closes fixture" });
}
using (var input = Input())
{
    var doc = XmlStreamingProvider.Load(input, Options());
    var result = XPath31Expression.Compile("/root/item").EvaluateNodes(doc);
    foreach (var value in result) { break; }
    Check("LO-09", result.IsSinglePass && input.CanRead, new { result.IsSinglePass, input.CanRead, note = "VM lazy path result survives EvaluateNodes; early exit does not close input" });
}
using (var input = Input())
{
    var doc = XmlStreamingProvider.Load(input, Options());
    var map = new XdmMap();
    IXdmNode? saved = null;
    foreach (var value in Records(doc))
    {
        if (saved is null) { saved = value.NodeValue; map.Add(XdmValue.FromString("node"), value); }
    }
    var found = map.TryGetValue(XdmValue.FromString("node"), out var escaped);
    Check("LO-10", found && ReferenceEquals(saved, escaped.NodeValue) && escaped.NodeValue.StringValue == "1" && !input.CanRead,
        new { found, sameReference = ReferenceEquals(saved, escaped.NodeValue), text = escaped.NodeValue.StringValue, input.CanRead,
            note = "Map retains the materialized in-memory node after reader completion; not a persistent snapshot lifetime proof" });
}
File.WriteAllText(args[0], JsonSerializer.Serialize(new { status = "Observed existing behavior; not freeze/consumer acceptance", passed = results.Count, failed = 0, results }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
Console.WriteLine($"{results.Count} memory-only ownership observations matched existing behavior.");

internal sealed class CountingSequence : IXdmSequence
{
    internal int Started;
    internal int Disposed;
    internal Counter? Last;
    /// <inheritdoc/>
    public bool TryGetLength(out int length) { length = 0; return false; }
    /// <inheritdoc/>
    public IXdmSequenceEnumerator GetEnumerator() { Started++; return Last = new Counter(this); }
    internal sealed class Counter(CountingSequence owner) : IXdmSequenceEnumerator, IDisposable
    {
        private int _position;
        /// <inheritdoc/>
        public XdmValue Current => XdmValue.FromInteger(_position);
        /// <inheritdoc/>
        public bool MoveNext() => ++_position <= 2;
        /// <inheritdoc/>
        public void Dispose() => owner.Disposed++;
    }
}
