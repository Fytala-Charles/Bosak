// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 14 October 2026
// PURPOSE              : Serializes full diagnostic reports for the editor protocol.
// SPECIAL NOTES        : Works around the unimplemented pull-diagnostic converter in OmniSharp 0.19.9.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 14-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Newtonsoft.Json;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Bosak.LanguageServer;

// OmniSharp 0.19.9's inherited converter throws NotImplementedException for
// pull diagnostic reports. Scope the working converter to our full reports.
[JsonConverter(typeof(FullDiagnosticReportConverter))]
internal sealed record SerializableDiagnosticReport : RelatedFullDocumentDiagnosticReport;

internal sealed class FullDiagnosticReportConverter : JsonConverter
{
    /// <inheritdoc />
    public override bool CanRead => false;

    /// <inheritdoc />
    public override bool CanConvert(Type objectType) => objectType == typeof(SerializableDiagnosticReport);

    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        var report = (SerializableDiagnosticReport)value!;
        writer.WriteStartObject();
        writer.WritePropertyName("kind");
        writer.WriteValue("full");
        writer.WritePropertyName("items");
        serializer.Serialize(writer, report.Items);
        writer.WriteEndObject();
    }

    /// <inheritdoc />
    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        => throw new NotSupportedException("Diagnostic reports are server output only.");
}
