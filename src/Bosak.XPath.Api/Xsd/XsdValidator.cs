// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 27 mei 2026
// PURPOSE              : Default implementation of IXsdValidator using System.Xml.Schema.
// SPECIAL NOTES        : Public surface API for compiling and evaluating XPath 3.1 expressions.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 27-05-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Xml;
using System.Xml.Schema;

namespace Bosak.XPath.Api.Xsd;

/// <summary>
/// Default implementation of <see cref="IXsdValidator"/> using <see cref="System.Xml.Schema.XmlSchemaSet"/>.
/// </summary>
public sealed class XsdValidator : IXsdValidator
{
    public XsdValidationResult Validate(string xml, Stream xsdStream, XsdValidatorOptions? options = null)
    {
        var result = TryValidate(xml, xsdStream, options);
        if (!result.IsValid)
            throw new XmlSchemaValidationException(string.Join("\n", result.OnlyErrors.Select(e => e.ToString())));
        return result;
    }

    public XsdValidationResult Validate(string xml, IEnumerable<Stream> xsdStreams, XsdValidatorOptions? options = null)
    {
        var result = TryValidate(xml, xsdStreams, options);
        if (!result.IsValid)
            throw new XmlSchemaValidationException(string.Join("\n", result.OnlyErrors.Select(e => e.ToString())));
        return result;
    }

    public XsdValidationResult TryValidate(string xml, Stream xsdStream, XsdValidatorOptions? options = null)
        => TryValidate(xml, new[] { xsdStream }, options);

    public XsdValidationResult TryValidate(string xml, IEnumerable<Stream> xsdStreams, XsdValidatorOptions? options = null)
    {
        options ??= XsdValidatorOptions.Default;
        var errors = new List<XsdValidationError>();
        var schemaSet = new XmlSchemaSet();

        if (!string.IsNullOrEmpty(options.BaseUri))
            schemaSet.XmlResolver = new XmlUrlResolver();

        foreach (var stream in xsdStreams)
        {
            using var reader = XmlReader.Create(stream);
            var schema = XmlSchema.Read(reader, null)!;
            schemaSet.Add(schema);
        }

        schemaSet.ValidationEventHandler += (sender, e) =>
        {
            if (options.MaxErrorCount > 0 && errors.Count >= options.MaxErrorCount)
                return;

            var severity = e.Severity == XmlSeverityType.Error ? XsdValidationSeverity.Error : XsdValidationSeverity.Warning;
            if (severity == XsdValidationSeverity.Warning && options.TreatWarningsAsErrors)
                severity = XsdValidationSeverity.Error;

            var exception = e.Exception;
            errors.Add(new XsdValidationError(
                severity,
                e.Message,
                exception?.LineNumber ?? 0,
                exception?.LinePosition ?? 0,
                exception?.SourceUri ?? ""));
        };

        schemaSet.Compile();

        var settings = new XmlReaderSettings
        {
            ValidationType = ValidationType.Schema,
            Schemas = schemaSet,
            ValidationFlags = XmlSchemaValidationFlags.ReportValidationWarnings
        };

        try
        {
            using var xmlReader = XmlReader.Create(new StringReader(xml), settings);
            while (xmlReader.Read()) { }
        }
        catch (XmlSchemaValidationException ex)
        {
            errors.Add(new XsdValidationError(
                XsdValidationSeverity.Error,
                ex.Message,
                ex.LineNumber,
                ex.LinePosition,
                ex.SourceUri ?? ""));
        }
        catch (XmlException ex)
        {
            errors.Add(new XsdValidationError(
                XsdValidationSeverity.Error,
                $"XML parse error: {ex.Message}",
                ex.LineNumber,
                ex.LinePosition,
                ""));
        }

        return new XsdValidationResult(errors);
    }
}
