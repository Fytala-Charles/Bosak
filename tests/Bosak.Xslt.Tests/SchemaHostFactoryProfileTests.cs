// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 01 November 2026
// PURPOSE              : Checks private compiler profile refusal before provider binding or callbacks.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
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
using System.Xml.Linq;
using System.Xml.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Bosak.Xslt.Api;
using Xunit;
namespace Bosak.Xslt.Tests;
/// <summary>Checks unsupported factory profiles before provider binding or mutable callbacks.</summary>
public sealed class SchemaHostFactoryProfileTests
{
    private const string Start = "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>";
    private const string Main = "<xsl:template name='main'><xsl:sequence select='1'/></xsl:template>";
    private sealed class NeverBind : ISchemaSessionProvider
    { ISchemaSessionBinding ISchemaSessionProvider.Bind(SchemaQueryEnvironment environment) => throw new Exception("Binding must not run"); }
    private sealed class NeverCall : IXsltUriResolver, IXsltMessageListener
    {
        XDocument IXsltUriResolver.Resolve(string href, string? baseUri) => throw new Exception("Resolver must not run");
        void IXsltMessageListener.OnMessage(string message) => throw new Exception("Listener must not run");
        void IXsltMessageListener.OnWarning(string message) => throw new Exception("Listener must not run");
    }
    /// <summary>Configured mutable compiler capabilities are refused without invoking them.</summary>
    /// <param name="setting">The unsupported configured capability.</param>
    [Theory][InlineData("resolver")][InlineData("resource")][InlineData("schema-aware")][InlineData("schema-set")]
    [InlineData("environment-set")][InlineData("schema-resolver")][InlineData("listener")][InlineData("processor")][InlineData("static-node")]
    public void RefusesMutableCompilerSettings(string setting)
    {
        var compiler = new XsltCompiler();
        switch (setting)
        {
            case "resolver": compiler.UriResolver = new NeverCall(); break;
            case "resource": compiler.ResourcePolicy = SchemaProviderSessionBinding.ClosedResources(); break;
            case "schema-aware": compiler.SchemaAware = true; break;
            case "schema-set": compiler.SchemaSet = new XmlSchemaSet(); break;
            case "environment-set": compiler.EnvironmentSchemaSet = new XmlSchemaSet(); break;
            case "schema-resolver": compiler.SchemaResolver = (_, _) => throw new Exception("Must not run"); break;
            case "listener": compiler.MessageListener = new NeverCall(); break;
            case "processor": compiler.ProcessorProfile = XsltProcessorProfile.Xslt40Draft; break;
            case "static-node": compiler.StaticParameters = new() { [("v", "")] = XdmValue.FromNode(new XDocumentNode(XElement.Parse("<n/>"))) }; break;
        }
        Refused(Start + Main + "</xsl:stylesheet>", compiler);
    }
    /// <summary>Uncaptured static/resource profiles refuse before binding.</summary>
    /// <param name="source">The unsupported source profile.</param>
    [Theory][InlineData("include")][InlineData("schema-import")][InlineData("use-when")][InlineData("shadow")][InlineData("static-acquisition")][InlineData("effective-values")]
    public void RefusesUncapturedSourceProfile(string source)
    {
        string declarations = source switch
        {
            "include" => "<xsl:include href='file:///D:/__uncaptured_module__.xsl'/>",
            "schema-import" => "<xsl:import-schema namespace='urn:uncaptured' schema-location='file:///D:/__uncaptured__.xsd'/>",
            "use-when" => "<xsl:variable name='v' select='1' use-when='true()'/>",
            "shadow" => "<xsl:variable name='v' _select=\"'1'\"/>",
            "static-acquisition" => "<xsl:param name='v' static='yes' select=\"doc-available('file:///D:/__uncaptured_static__.xml')\"/>",
            _ => "<xsl:variable name='v' static='yes' select='(1,2)'/>"
        };
        Refused(Start + declarations + Main + "</xsl:stylesheet>", new XsltCompiler());
    }
    private static void Refused(string source, XsltCompiler compiler)
    {
        Assert.False(SchemaHostSessionCompilation.TryCompileXslt(source, compiler, null, null, null, "main", _ => false, new NeverBind(), 4, 16, out var session, out var reason));
        Assert.Null(session); Assert.False(string.IsNullOrEmpty(reason));
    }
}
