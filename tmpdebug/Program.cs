using System;
using System.Linq;
using System.Xml.Linq;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;

var xslText = System.IO.File.ReadAllText("tmpdebug/test.xsl");
var compiler = new Bosak.Xslt.Api.XsltCompiler();
var executable = compiler.Compile(xslText);

var evalContext = new Bosak.XPath.Runtime.Vm.EvaluationContext();
var sourceXml = "<doc/>";
var sourceNode = new XDocumentNode(XDocument.Parse(sourceXml));

var result = executable.TransformToString(sourceNode, evalContext);
Console.WriteLine("Result: " + result);
