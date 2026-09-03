// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 mei 2026
// PURPOSE              : In-memory representation of a loaded XSLT stylesheet with template rules and imports.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 25-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 24-05-2026     | Added named template dictionary for call-template dispatch                               |
//                      | Charles Korthout | 0.3   | 24-05-2026     | Added import/include resolution, import precedence, flattened rule collection            |
//                      | Charles Korthout | 0.4   | 26-05-2026     | Added global variable and parameter collection with import/include precedence            |
//                      | Charles Korthout | 0.5   | 26-05-2026     | Added xsl:strip-space and xsl:preserve-space parsing with SpaceHandlingRule              |
//                      | Charles Korthout | 0.6   | 27-05-2026     | Added xsl:function parsing and GetAllFunctionDefinitions / GetAllNamespaces              |
//                      | Charles Korthout | 0.7   | 27-05-2026     | Added required version attribute validation (XTSE0010)                                   |
//                      | Charles Korthout | 0.8   | 27-05-2026     | Exposed Version property for runtime backwards-compatibility checks                    |
//                      | Charles Korthout | 0.9   | 31-05-2026     | Decimal-format merging from imports/includes; descendant namespace collection            |
//                      | Charles Korthout | 1.0   | 31-05-2026     | Added exclude-result-prefixes parsing and GetAllExcludedResultPrefixes                   |
//                      | Charles Korthout | 1.1   | 31-05-2026     | Added literal result element stylesheet support (WrapLiteralResultElement)               |
//                      | Charles Korthout | 1.2   | 07-06-2026     | Fix import+include same file: separate _includedUris, copy _resolvedUris to children     |
//                      | Charles Korthout | 1.3   | 10-06-2026     | Added ValidateInstructionTree for xsl:copy-of static validation (XTSE0090/0260)        |
//                      | Charles Korthout | 1.4   | 10-06-2026     | ValidateInstructionTree checks xsl:copy attributes and copy-namespaces values (XTSE0020) |
//                      | Charles Korthout | 1.5   | 11-06-2026     | Made GetXPathDefaultNamespace internal for xsl:key index building                       |
//                      | Charles Korthout | 1.6   | 11-06-2026     | Added DeclaredModes parsing                                                             |
//                      | Charles Korthout | 1.7   | 11-06-2026     | XTSE0130 for no-namespace top-level elements; fixes accumulator-078                     |
//                      | Charles Korthout | 1.8   | 13-06-2026     | Empty-URI EQName support for variable/param names (Q{}local)                             |
//                      | Charles Korthout | 1.9   | 13-06-2026     | Parse xsl:global-context-item; XTSE3089 for use=absent with as                         |
//                      | Charles Korthout | 2.0   | 13-06-2026     | XTSE0710 validation for xsl:attribute-set/@use-attribute-sets                          |
//                      | Charles Korthout | 2.1   | 13-06-2026     | xsl:function static validation: attributes, duplicate names, required params           |
//                      | Charles Korthout | 2.2   | 13-06-2026     | Added xsl:merge static validation (required children, merge-key placement)             |
//                      | Charles Korthout | 2.3   | 13-06-2026     | XTSE1650 import-schema; merge-source validation/type and sort-before-merge checks      |
//                      | Charles Korthout | 2.4   | 24-06-2026     | expand-text on xsl:message; package-version XTSE0090; XTSE1660 for strict/lax/type    |
//                      | Charles Korthout | 2.5   | 24-06-2026     | Restrict XTSE1660 to strict validation; allow lax on basic processors                  |
//                      | Charles Korthout | 2.6   | 24-06-2026     | Reject extension-element-prefixes bound to reserved namespaces (XTSE0800)              |
//                      | Charles Korthout | 2.7   | 24-06-2026     | use-when walks ancestor namespace declarations; propagates XTDE1400/1410 errors        |
//                      | Charles Korthout | 2.8   | 25-06-2026     | XTSE0080 validation for xsl:template/@name; added xs/fn namespace constants             |
//                      | Charles Korthout | 2.9   | 26-06-2026     | Whitelist use-attribute-sets on literal result elements (XTSE0805)                    |
//                      | Charles Korthout | 2.9   | 25-06-2026     | Static function-available hides dynamic XSLT functions; skip descendants of false use-when |
//                      | Charles Korthout | 2.10  | 26-06-2026     | Guard XTSE1660 check so it does not fire on literal result elements                     |
//                      | Charles Korthout | 2.11  | 26-06-2026     | Static variable validation and default-value handling for static cluster              |
//                      | Charles Korthout | 2.12  | 26-06-2026     | XTSE0090 for non-global static vars/params; XTSE0090 visibility; XTSE3450 var/param   |
//                      | Charles Korthout | 2.13  | 26-06-2026     | Document-order use-when evaluation; precedence-aware static variable conflict detection |
//                      | Charles Korthout | 2.14  | 26-06-2026     | Added xsl:namespace-alias parsing and effective alias mapping                           |
//                      | Charles Korthout | 2.16  | 07-07-2026     | XTSE0680 validation uses the root stylesheet's named-template set so imports see overrides |
//                      | Charles Korthout | 2.15  | 29-06-2026     | Static validation for xsl:variable/param/with-param attributes; forwards-compatible mode |
//                      | Charles Korthout | 2.16  | 26-06-2026     | Shadow attribute support for version, href, use-when, and xpath-default-namespace        |
//                      | Charles Korthout | 2.17  | 26-06-2026     | Static context hides XSLT dynamic functions such as fn:current-output-uri               |
//                      | Charles Korthout | 2.18  | 03-07-2026     | Validate expand-text values (XTSE0020); allow expand-text on xsl:function               |
//                      | Charles Korthout | 2.19  | 03-07-2026     | Import/include precedence, apply-imports context, and duplicate includes; clears import |
//                      | Charles Korthout | 2.20  | 05-07-2026     | Version cluster: per-element version, known-element set, forwards-compat skip         |
//                      | Charles Korthout | 2.21  | 26-06-2026     | Added assert to known XSLT element set                                                  |
//                      | Charles Korthout | 2.22  | 06-07-2026     | Merge multiple xsl:output declarations instead of using only the first                 |
//                      | Charles Korthout | 2.23  | 08-07-2026     | Forward-compatible handling for unknown elements, attributes, and use-when             |
//                      | Charles Korthout | 2.24  | 26-06-2026     | TransitiveImports now includes modules included by imported modules (apply-imports)   |
//                      | Charles Korthout | 2.25  | 11-07-2026     | Parse xsl:character-map declarations and resolve effective character maps.             |
//                      | Charles Korthout | 2.26  | 11-07-2026     | Character-map resolution now uses first-wins across the effective map list.            |
//                      | Charles Korthout | 2.27  | 11-07-2026     | Named-output import-precedence merge now puts the importing stylesheet last            |
//                      | Charles Korthout | 2.28  | 11-07-2026     | Load xsl:output parameter-document defaults and merge them with explicit attributes.    |
//                      | Charles Korthout | 2.29  | 12-07-2026     | Last map in use-character-maps list wins for duplicate characters.                      |
//                      | Charles Korthout | 2.30  | 13-07-2026     | Resolve include/import hrefs against the element base URI (external entities).          |
//                      | Charles Korthout | 2.31   | 14-07-2026     | xsl:package root support (name/version); fn:transform registered in the static context |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.32   | 01-08-2026     | use-when/static contexts run with IsXsltMode so fn:system-property is available        |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.33  | 23-08-2026     | Pass use-when context to ConvertVariableValue for static param @as prefix resolution   |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.34  | 24-08-2026     | Static structural validation for XTSE0010 error cluster                               |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.35  | 24-08-2026     | XTSE0020 name and attribute value validation (decimal-format, names, tunnel, mode)     |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.36  | 24-08-2026     | Fix XTSE0020 regressions: EQName/AVT ordering, XML 1.1 NCNames, decimal-format symbols   |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.37  | 24-08-2026     | XTSE0090 attribute validation for XSLT elements (stylesheet, template, apply-* etc.)   |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.38  | 24-08-2026     | XTSE0120 top-level text-node validation for xsl:stylesheet/transform/package           |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.39  | 24-08-2026     | XTSE0500/0550 template attribute validation; accept #unnamed in template mode list      |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.40  | 24-08-2026     | XTSE0280 prefix binding validation for XSLT names and mode tokens                     |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.41  | 24-08-2026     | XTSE0710 use-attribute-sets validation for copy/element/LRE                             |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.42  | 24-08-2026     | XTSE0808 exclude-result-prefixes prefix binding validation                            |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.43  | 25-08-2026     | XTSE0809 #default in exclude-result-prefixes requires default namespace               |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.44  | 25-08-2026     | XTSE0340 early pattern validation for template/key match and number count/from         |
//                      | Charles Korthout | 2.45  | 25-08-2026     | XTSE0260 validation for XSLT elements required to be empty                             |
//                      | Charles Korthout | 2.46  | 25-08-2026     | XTSE0350 validation for unbalanced AVT braces                                          |
//                      | Charles Korthout | 2.47  | 25-08-2026     | XTSE0370 validation for unescaped right braces in AVTs                                |
//                      | Charles Korthout | 2.48  | 25-08-2026     | XTSE0530 validation for xsl:template/@priority as xs:decimal                           |
//                      | Charles Korthout | 2.49  | 25-08-2026     | XTSE0125 validation for default-collation collation URIs                |
//                      | Charles Korthout | 2.50  | 25-08-2026     | XTSE0840 validation for xsl:attribute/@select with non-empty content    |
//                      | Charles Korthout | 2.51  | 25-08-2026     | XTSE0870 validation for xsl:value-of/@select and content                |
//                      | Charles Korthout | 2.52  | 25-08-2026     | XTSE0880 validation for xsl:processing-instruction/@select with content |
//                      | Charles Korthout | 2.53  | 25-08-2026     | XTSE0910 validation for xsl:namespace/@select and content              |
//                      | Charles Korthout | 2.54  | 25-08-2026     | XTSE0940 validation for xsl:comment/@select with non-empty content     |
//                      | Charles Korthout | 2.55  | 25-08-2026     | XTSE1015 validation for xsl:sort/@select with non-empty content         |
//                      | Charles Korthout | 2.56  | 25-08-2026     | XTSE1040 validation for xsl:perform-sort/@select content               |
//                      | Charles Korthout | 2.57  | 25-08-2026     | XTSE1222 validation for conflicting xsl:key @composite values          |
//                      | Charles Korthout | 2.58  | 25-08-2026     | XTSE1430 validation for unbound extension-element-prefixes            |
//                      | Charles Korthout | 2.59  | 25-08-2026     | XTSE1660 validation for xsl:type on literal result elements            |
//                      | Charles Korthout | 2.60  | 25-08-2026     | XTSE3140 validation for xsl:try/@select content                        |
//                      | Charles Korthout | 2.61  | 25-08-2026     | XTSE3150 validation for xsl:catch/@select content                      |
//                      | Charles Korthout | 2.62  | 25-08-2026     | XTSE3190 validation for duplicate xsl:merge-source names               |
//                      | Charles Korthout | 2.63  | 25-08-2026     | XTSE3350 validation for duplicate xsl:accumulator names                 |
//                      | Charles Korthout | 2.64  | 25-08-2026     | XTSE0760 validation for xsl:param inside xsl:function                  |
//                      | Charles Korthout | 2.65  | 25-08-2026     | XTSE1295 validation for xsl:decimal-format/@zero-digit numeric value    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.66  | 25-08-2026     | XTSE1290 validation for conflicting xsl:decimal-format declarations    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.67  | 26-08-2026     | Ignore whitespace-only text in @select+content validation; unique default merge-source names |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.68  | 26-08-2026     | XTSE1560 validation for conflicting xsl:output attribute values                          |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.69  | 26-08-2026     | XTSE1590 validation for unresolved use-character-maps references                       |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.70  | 26-08-2026     | XTSE1600 validation for circular use-character-maps references                         |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.71  | 26-08-2026     | XTSE0265 validation for conflicting input-type-annotations across modules              |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.72  | 26-08-2026     | XTSE0620 validation for variable-binding elements with @select and content               |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.73  | 26-08-2026     | XTSE0630 validation for duplicate global variable/param bindings                        |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.74  | 26-08-2026     | XTSE0660 validation for duplicate named template bindings                                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.75  | 27-08-2026     | XTSE0670 validation for duplicate sibling xsl:with-param names                           |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.76  | 27-08-2026     | XTSE0720 validation for circular xsl:attribute-set use-attribute-sets references        |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.77  | 27-08-2026     | XTSE0975 validation for xsl:number/@value exclusivity                                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.78  | 28-08-2026     | XTSE0020 validation for xsl:text/xsl:value-of disable-output-escaping values           |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.79  | 28-08-2026     | Support fragment identifiers in xsl:include/xsl:import href for embedded modules        |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.80  | 29-08-2026     | Basic xsl:package/xsl:use-package parsing; accept/override known elements                |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.81  | 30-08-2026     | Resolve xsl:use-package to registered packages and merge exported components            |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.82  | 30-08-2026     | Added includePrivate option to GetAllFunctionDefinitions for package internal view      |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.83  | 30-08-2026     | GetAllFunctionDefinitions excludes used-package private functions; allow streamability attr |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.84  | 30-08-2026     | OwningPackage propagation; yield all used-package template rules for runtime filtering |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.85  | 30-08-2026     | Parse xsl:accept/xsl:override; apply accept visibility and template rule overrides      |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.86  | 30-08-2026     | Pass CollectingScope through global collection and validation; scope-isolate use-package globals |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.87  | 30-08-2026     | Filter used-package private components; only accepted-as-private are visible in package scope |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.88  | 31-08-2026     | XTSE3085 validation for undeclared modes in packages                                  |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.89  | 31-08-2026     | xsl:expose partial-wildcard validation; skip default mode in declared-modes check       |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.90  | 31-08-2026     | Mode-aware template visibility; fixes use-package regressions for public-mode rules   |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.91  | 31-08-2026     | Fix xsl:expose component="*" wildcard-name validation (XTSE3025/3010)                  |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.92  | 31-08-2026     | XTSE3008 validation for xsl:use-package in imported modules; add IsPrincipalLevel flag |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.93  | 01-09-2026     | Override contributions: package-scope function/global views see xsl:override declarations |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.94  | 01-09-2026     | XTSE3051 validation for accept/override name overlap; strict accept visibility table   |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.95  | 01-09-2026     | XTSE0020 validation for xsl:package and xsl:use-package package-version (REQ-082)         |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.96  | 01-09-2026     | XTSE0010 for misplaced xsl:use-package and xsl:expose (REQ-082)                         |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.97  | 01-09-2026     | XTSE0010 for required xsl:param with non-empty sequence constructor (REQ-082)            |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.97  | 01-09-2026     | XTSE0020 for undeclared prefix in xsl:expose/@names and xsl:accept/@names (REQ-082)   |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.96  | 01-09-2026     | XTSE0010/0020 for static param sequence constructor and tunnel attribute (REQ-082)      |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.98  | 02-09-2026     | XTSE0010 for disallowed content in xsl:override; on-completion placement pre-pass       |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.99  | 02-09-2026     | REQ-082 phase 3: override validators enforce XTSE3058/3060/3070 (incl. new-each-time,  |
//                      |                  |       |                | template-rule modes XTSE3440/3060, transitive targets); XTSE3050 local-vs-accepted     |
//                      |                  |       |                | conflicts + implicit mode redeclaration; xsl:expose precedence (declared over          |
//                      |                  |       |                | wildcard); use-when honored in XTSE0630 collection; override wins GetAllNamedTemplates |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.100 | 02-09-2026     | xsl:original variables: overridden globals kept under an unspellable alias namespace    |
//                      |                  |       |                | for $xsl:original resolution; template override contributions registered for named    |
//                      |                  |       |                | templates/attribute-sets; GetPackageScopeNamedTemplates applies contributions;        |
//                      |                  |       |                | xsl:param permitted in xsl:override                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.101 | 02-09-2026     | XTSE2200 for merge-key count mismatch; XTSE3087 for multiple/inconsistent             |
//                      |                  |       |                | xsl:global-context-item declarations; XTTE0590 for use=required in a library package   |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Bosak.Xslt.Api;
using Bosak.Xslt.Patterns;
using Bosak.Xslt.Runtime;

namespace Bosak.Xslt.Stylesheet;

/// <summary>
/// Represents a loaded XSLT stylesheet, including all imported and included modules.
/// </summary>
public sealed class Stylesheet
{
    private XDocument _document;
    private readonly string? _baseUri;
    private readonly IXsltUriResolver _resolver;
    private readonly HashSet<string> _resolvedUris;

    private readonly List<TemplateRule> _templateRules = new();
    private readonly Dictionary<string, TemplateRule> _namedTemplates = new();
    private readonly List<Stylesheet> _imports = new();
    private readonly List<Stylesheet> _includes = new();
    private readonly List<Stylesheet> _usedPackages = new();
    private readonly Dictionary<Stylesheet, PackageUseOptions?> _usedPackageOptions = new();
    // xsl:override contributions from packages that use THIS package. Registered on the used
    // package by each using package so that this package's own components see the overriding
    // declarations when they execute (XSLT 3.0 §3.5.7.2).
    private List<(Stylesheet User, PackageUseOptions Options)>? _packageOverrideContributions;
    private readonly List<ExposeRule> _exposeRules = new();
    private readonly List<KeyDefinition> _keyDefinitions = new();
    private readonly List<AccumulatorDefinition> _accumulators = new();
    private readonly List<XElement> _globalVariables = new();
    private readonly List<XElement> _globalParameters = new();
    private readonly List<SpaceHandlingRule> _spaceRules = new();
    private readonly Dictionary<string, ModeDefinition> _modeDefinitions = new();
    private readonly List<XsltFunctionDefinition> _functionDefinitions = new();
    private readonly List<DecimalFormatDefinition> _decimalFormats = new();
    private readonly List<AttributeSetDefinition> _attributeSets = new();
    private readonly HashSet<string> _excludedResultPrefixes = new();
    private readonly List<NamespaceAliasDefinition> _namespaceAliases = new();
    private OutputProperties? _outputProperties;
    private readonly Dictionary<string, OutputProperties> _namedOutputProperties = new();
    private readonly Dictionary<string, CharacterMapDefinition> _characterMaps = new();
    private readonly bool _isRootStylesheet;
    private readonly Stylesheet _rootStylesheet;
    private readonly StaticContext _staticContext = new();
    private readonly IReadOnlyDictionary<(string LocalName, string NamespaceUri), XdmValue> _externalStaticParameters;
    private readonly Api.PackageVersionResolutionStrategy _packageVersionResolutionStrategy;

    /// <summary>
    /// Empty dictionary used when no external static parameters are supplied.
    /// </summary>
    private static readonly IReadOnlyDictionary<(string LocalName, string NamespaceUri), XdmValue> EmptyExternalStaticParameters =
        new Dictionary<(string LocalName, string NamespaceUri), XdmValue>();

    /// <summary>
    /// Holds the static variables and parameters that are in scope for evaluating
    /// <c>use-when</c> expressions in this stylesheet module.
    /// </summary>
    private sealed class StaticContext
    {
        /// <summary>Static variable bindings keyed by (local-name, namespace-uri).</summary>
        public Dictionary<(string LocalName, string NamespaceUri), (XdmValue Value, bool IsParam, int Precedence)> Variables { get; } = new();

        /// <summary>
        /// Same-precedence declarations with different values. A later higher-precedence
        /// declaration resolves the conflict; otherwise it is reported as XTSE3450.
        /// </summary>
        public Dictionary<(string LocalName, string NamespaceUri), (XdmValue First, XdmValue Second)> PendingConflicts { get; } = new();
    }

    /// <summary>
    /// Builds the evaluation context used for static <c>use-when</c> expressions.
    /// </summary>
    private EvaluationContext CreateUseWhenContext(XElement elem, string? explicitBaseUri = null)
    {
        var ctx = new EvaluationContext();

        // The static base URI for use-when is the base URI of the element's
        // containing stylesheet module, taking xml:base into account.
        ctx.BaseUri = explicitBaseUri ?? GetEffectiveBaseUri(elem);
        ctx.IsStaticEvaluation = true;
        // use-when/static expressions are evaluated by the XSLT processor: fn:system-property
        // must be available (XSLT 3.0 §3.13.3 — use-when-0103/0104, shadow-002..004).
        ctx.IsXsltMode = true;
        Bosak.XPath.Standard.Functions.FunctionLibrary.Populate(ctx);
        // fn:transform is available in static expressions (XSLT 3.0 §24.1), e.g. a
        // static variable whose value is used in an xsl:use-when attribute.
        Api.XsltFunctionLibrary.Populate(ctx);

        // Add in-scope namespace declarations so prefixes in use-when resolve correctly.
        var currentNs = elem;
        while (currentNs != null)
        {
            foreach (var attr in currentNs.Attributes().Where(a => a.IsNamespaceDeclaration))
            {
                var prefix = attr.Name.LocalName;
                if (prefix == "xmlns") prefix = "";
                // Inner declarations take precedence; only add if not already present.
                if (!ctx.TryResolveNamespace(prefix, out _))
                    ctx.WithNamespace(prefix, attr.Value);
            }
            currentNs = currentNs.Parent;
        }

        // Apply the effective xpath-default-namespace for element/type names.
        var defaultNs = GetXPathDefaultNamespace(elem);
        if (!string.IsNullOrEmpty(defaultNs))
            ctx.DefaultElementNamespace = defaultNs;

        // Make static variables/parameters visible to the expression.
        foreach (var (key, entry) in _staticContext.Variables)
            ctx.WithVariable(key.LocalName, entry.Value, key.NamespaceUri);

        return ctx;
    }

    /// <summary>
    /// Evaluates the <c>use-when</c> attribute on the given element.
    /// Returns <c>true</c> if the attribute is absent or evaluates to true.
    /// Any error while evaluating the expression is propagated as a static error.
    /// A shadow attribute <c>_use-when</c> is expanded to <c>use-when</c> first.
    /// </summary>
    private bool UseWhen(XElement elem, string? explicitBaseUri = null)
    {
        // In forward-compatible mode, an unknown XSLT element whose use-when expression
        // references a future function is treated as excluded; elements without use-when
        // are kept so that their xsl:fallback children can be processed.
        if (elem.Name.NamespaceName == XslNamespace &&
            !KnownXsltElementNames.Contains(elem.Name.LocalName) &&
            IsForwardsCompatibleElement(elem) &&
            elem.Attribute("use-when") != null)
        {
            return false;
        }

        // Expand a shadow use-when attribute before evaluation.
        ExpandShadowAttribute(elem, "use-when");

        string? useWhen = null;
        bool isXsltElement = elem.Name.NamespaceName == XslNamespace;

        if (isXsltElement)
        {
            // On XSLT elements the controlling use-when is the no-namespace attribute.
            var noNsAttr = elem.Attribute("use-when");
            if (noNsAttr != null)
            {
                useWhen = noNsAttr.Value;
            }
            else
            {
                // A use-when attribute in the XSLT namespace is never permitted on
                // XSLT elements.
                if (elem.Attribute(XName.Get("use-when", XslNamespace)) != null)
                    throw new InvalidOperationException("XTSE0090: use-when must not be in the XSLT namespace on XSLT elements.");
                return true;
            }
        }
        else
        {
            // On literal result elements (and data elements) only the XSLT-namespace
            // form is recognized; the no-namespace form is a normal output attribute.
            var xslAttr = elem.Attribute(XName.Get("use-when", XslNamespace));
            if (xslAttr != null)
                useWhen = xslAttr.Value;
            else
                return true;
        }

        if (string.IsNullOrEmpty(useWhen))
            return true;

        var compiled = XPath31Expression.Compile(useWhen);
        var ctx = CreateUseWhenContext(elem, explicitBaseUri);
        var result = compiled.Evaluate(ctx);
        bool include = result.EffectiveBooleanValue();

        if (isXsltElement)
        {
            // If the no-namespace use-when excludes the element, the (erroneous)
            // XSLT-namespace use-when attribute is ignored. Otherwise it is an error.
            if (include && elem.Attribute(XName.Get("use-when", XslNamespace)) != null)
                throw new InvalidOperationException("XTSE0090: use-when must not be in the XSLT namespace on XSLT elements.");
        }

        return include;
    }

    /// <summary>
    /// Expands a single shadow attribute <c>_{baseName}</c> to <c>{baseName}</c> by
    /// evaluating its value as an attribute value template in the current static context.
    /// </summary>
    private void ExpandShadowAttribute(XElement elem, string baseName)
    {
        var shadow = elem.Attribute("_" + baseName);
        if (shadow == null)
            return;

        var expanded = EvaluateStaticAvt(shadow.Value, elem);
        shadow.Remove();
        elem.SetAttributeValue(baseName, expanded);
    }

    /// <summary>
    /// Expands all shadow attributes on the given XSLT element and its XSLT descendants.
    /// Shadow attributes on literal result elements and data elements are ignored.
    /// </summary>
    private void ExpandAllShadowAttributes(XElement root)
    {
        foreach (var elem in root.DescendantsAndSelf())
        {
            // Shadow attributes only apply to XSLT instructions.
            if (elem.Name.NamespaceName != XslNamespace)
                continue;

            var shadows = elem.Attributes()
                .Where(a => string.IsNullOrEmpty(a.Name.NamespaceName) && a.Name.LocalName.StartsWith("_"))
                .ToList();

            foreach (var attr in shadows)
            {
                var baseName = attr.Name.LocalName.Substring(1);
                if (string.IsNullOrEmpty(baseName))
                    continue;

                var expanded = EvaluateStaticAvt(attr.Value, elem);
                attr.Remove();
                elem.SetAttributeValue(baseName, expanded);
            }
        }
    }

    /// <summary>
    /// Evaluates a static attribute value template using the current static context.
    /// </summary>
    private string EvaluateStaticAvt(string avt, XElement element)
    {
        if (string.IsNullOrEmpty(avt) || !avt.Contains('{'))
            return avt;

        var ctx = CreateUseWhenContext(element);
        var nsMap = ExtractInScopeNamespaces(element);
        var sb = new StringBuilder();

        int i = 0;
        while (i < avt.Length)
        {
            if (i + 1 < avt.Length && avt[i] == '{' && avt[i + 1] == '{')
            {
                sb.Append('{');
                i += 2;
                continue;
            }
            if (i + 1 < avt.Length && avt[i] == '}' && avt[i + 1] == '}')
            {
                sb.Append('}');
                i += 2;
                continue;
            }
            if (avt[i] == '}')
            {
                throw new InvalidOperationException("XTSE0370: An unescaped right curly bracket in an attribute value template does not have a matching left curly bracket.");
            }
            if (avt[i] == '{')
            {
                int end = FindMatchingAvtBrace(avt, i + 1);
                if (end < 0)
                    throw new InvalidOperationException("XTSE0350: An unescaped left curly bracket in an attribute value template does not have a matching right curly bracket.");

                var expr = avt.Substring(i + 1, end - i - 1);
                if (!string.IsNullOrEmpty(expr))
                {
                    var compiled = XPath31Expression.Compile(expr, new CompileOptions { Namespaces = nsMap });
                    var result = compiled.Evaluate(ctx);
                    sb.Append(AtomizedAvtString(result));
                }
                i = end + 1;
                continue;
            }

            sb.Append(avt[i]);
            i++;
        }

        return sb.ToString();
    }

    /// <summary>
    /// Finds the matching closing brace for an AVT expression, skipping string literals.
    /// </summary>
    private static int FindMatchingAvtBrace(string value, int start)
    {
        char inString = '\0';
        int braceDepth = 1;
        for (int i = start; i < value.Length; i++)
        {
            char c = value[i];
            if (inString != '\0')
            {
                if (c == inString)
                {
                    // Handle doubled quote characters inside string literals.
                    if (i + 1 < value.Length && value[i + 1] == inString)
                    {
                        i++;
                        continue;
                    }
                    inString = '\0';
                }
                continue;
            }

            if (c == '\'' || c == '"')
            {
                inString = c;
                continue;
            }

            if (c == '{')
            {
                braceDepth++;
                continue;
            }

            if (c == '}')
            {
                braceDepth--;
                if (braceDepth == 0)
                    return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Atomizes a value for AVT concatenation, returning the string value of each item
    /// without separators.
    /// </summary>
    private static string AtomizedAvtString(XdmValue value)
    {
        if (value.IsUndefined)
            return string.Empty;

        if (value.IsSequence && value.SequenceValue != null)
        {
            var sb = new StringBuilder();
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                sb.Append(item.ToString());
            return sb.ToString();
        }

        return value.ToString();
    }

    /// <summary>
    /// Collects the in-scope namespace declarations for an element and its ancestors.
    /// </summary>
    private static Dictionary<string, string> ExtractInScopeNamespaces(XElement element)
    {
        var dict = new Dictionary<string, string>();
        var current = element;
        while (current != null)
        {
            foreach (var attr in current.Attributes().Where(a => a.IsNamespaceDeclaration))
            {
                var prefix = attr.Name.LocalName;
                if (prefix == "xmlns")
                    prefix = "";
                if (!string.IsNullOrEmpty(prefix) && !dict.ContainsKey(prefix))
                    dict[prefix] = attr.Value;
            }
            current = current.Parent;
        }
        return dict;
    }

    public Stylesheet(XDocument document, string? baseUri, IXsltUriResolver resolver, int importPrecedence = 0, HashSet<string>? resolvedUris = null, object? inheritedStaticContext = null, IReadOnlyDictionary<(string LocalName, string NamespaceUri), XdmValue>? externalStaticParameters = null, Stylesheet? rootStylesheet = null, Stylesheet? owningPackage = null, Api.PackageVersionResolutionStrategy packageVersionResolutionStrategy = Api.PackageVersionResolutionStrategy.Highest, bool isPrincipalLevel = true)
    {
        _document = document;
        _baseUri = baseUri;
        _resolver = resolver;
        ImportPrecedence = importPrecedence;
        ApplyImportsContextModule = this;
        _resolvedUris = resolvedUris ?? new HashSet<string>();
        _isRootStylesheet = _resolvedUris.Count == 0;
        _externalStaticParameters = externalStaticParameters ?? EmptyExternalStaticParameters;
        _rootStylesheet = rootStylesheet ?? this;
        _packageVersionResolutionStrategy = packageVersionResolutionStrategy;
        IsPrincipalLevel = isPrincipalLevel;

        // Add this stylesheet's own URI to the resolved set for circular-reference detection
        if (!string.IsNullOrEmpty(baseUri))
            _resolvedUris.Add(baseUri);

        // Initialise the static context from the including module (for xsl:include).
        if (inheritedStaticContext is StaticContext inherited)
        {
            foreach (var kv in inherited.Variables)
                _staticContext.Variables[kv.Key] = (kv.Value.Value, kv.Value.IsParam, kv.Value.Precedence);
        }

        // Handle literal result element stylesheets before loading
        var root = _document.Root;
        if (root != null && root.Name.NamespaceName != XslNamespace)
        {
            var xslVersion = root.Attributes()
                .FirstOrDefault(a => a.Name.NamespaceName == XslNamespace && a.Name.LocalName == "version");
            if (xslVersion != null)
            {
                _document = WrapLiteralResultElement(root, xslVersion.Value);
            }
        }

        Load();

        // OwningPackage identifies the package root that owns this module. A package root
        // owns itself; imports and includes inherit the package of the module that contains them.
        OwningPackage = IsPackage ? this : owningPackage;
    }

    /// <summary>The root element of the stylesheet (xsl:stylesheet or xsl:transform).</summary>
    public XElement Root => _document.Root!;

    /// <summary>The base URI of this stylesheet, used for resolving relative xml:base values.</summary>
    public string? BaseUri => _baseUri;

    /// <summary>All template rules defined in this stylesheet, ordered by priority.</summary>
    public IReadOnlyList<TemplateRule> TemplateRules => _templateRules;

    /// <summary>Named templates indexed by name.</summary>
    public IReadOnlyDictionary<string, TemplateRule> NamedTemplates => _namedTemplates;

    /// <summary>Accumulator declarations defined in this stylesheet.</summary>
    public IReadOnlyList<AccumulatorDefinition> Accumulators => _accumulators;

    /// <summary>Stylesheets imported via xsl:import (lower precedence).</summary>
    public IReadOnlyList<Stylesheet> Imports => _imports;

    /// <summary>Stylesheets included via xsl:include (same precedence).</summary>
    public IReadOnlyList<Stylesheet> Includes => _includes;

    /// <summary>Packages used via xsl:use-package (lower precedence than this module).</summary>
    public IReadOnlyList<Stylesheet> UsedPackages => _usedPackages;

    /// <summary>The import precedence of this stylesheet (0 = main, higher = deeper import).</summary>
    public int ImportPrecedence { get; private set; }

    /// <summary>
    /// The stylesheet module whose import tree is used by <c>xsl:apply-imports</c>
    /// inside templates in this module. For the root and imported modules this is
    /// the module itself; for included modules it is the module that included it
    /// (propagated through nested includes).
    /// </summary>
    public Stylesheet ApplyImportsContextModule { get; private set; } = null!;

    /// <summary>The default mode for xsl:apply-templates within this stylesheet (empty string = unnamed mode).</summary>
    public string DefaultMode { get; private set; } = "";

    /// <summary>
    /// Whether every mode used in the stylesheet must be explicitly declared.
    /// A value of <c>false</c> (from xsl:package/@declared-modes="no") means implicit
    /// mode declarations are allowed.
    /// </summary>
    public bool DeclaredModes { get; private set; } = true;

    /// <summary>The value of the xsl:global-context-item/@use attribute, or null if absent.</summary>
    public string? GlobalContextItemUse { get; private set; }

    /// <summary>Whether the root element of this module is <c>xsl:package</c>.</summary>
    public bool IsPackage { get; private set; }

    /// <summary>The package name (URI) from xsl:package/@name, or null for a stylesheet module.</summary>
    public string? PackageName { get; private set; }

    /// <summary>The package version from xsl:package/@package-version, or null when absent.</summary>
    public string? PackageVersion { get; private set; }

    /// <summary>The package root that owns this module, or null for non-package stylesheets.</summary>
    public Stylesheet? OwningPackage { get; private set; }

    /// <summary>
    /// Whether this module is at the same stylesheet level as its principal stylesheet module
    /// (the principal module itself and any modules reached by xsl:include). Modules reached by
    /// xsl:import are at a lower stylesheet level.
    /// </summary>
    public bool IsPrincipalLevel { get; private set; } = true;

    /// <summary>The value of the xsl:global-context-item/@as attribute, or null if absent.</summary>
    public string? GlobalContextItemAs { get; private set; }

    /// <summary>
    /// Recursively collects all template rules from this stylesheet, its includes, and its imports.
    /// Order: local first, then includes (same precedence), then imports (lower precedence).
    /// </summary>
    public IEnumerable<TemplateRule> GetAllTemplateRules()
    {
        // Group local template rules by their source element so we can emit them
        // in true document order while interleaving imported/included modules.
        var rulesByElement = new Dictionary<XElement, List<TemplateRule>>();
        foreach (var rule in _templateRules)
        {
            if (!rulesByElement.TryGetValue(rule.Element, out var list))
            {
                list = new List<TemplateRule>();
                rulesByElement[rule.Element] = list;
            }
            list.Add(rule);
        }

        foreach (var element in Root.Elements())
        {
            var ns = element.Name.NamespaceName;
            var localName = element.Name.LocalName;

            if (ns == XslNamespace && localName == "import")
            {
                if (element.Annotation<ResolvedModuleAnnotation>() is { Module: { } imported })
                {
                    foreach (var rule in imported.GetAllTemplateRules())
                        yield return rule;
                }
            }
            else if (ns == XslNamespace && localName == "include")
            {
                if (element.Annotation<ResolvedModuleAnnotation>() is { Module: { } included })
                {
                    foreach (var rule in included.GetAllTemplateRules())
                        yield return rule;
                }
            }
            else if (ns == XslNamespace && localName == "use-package")
            {
                if (element.Annotation<ResolvedModuleAnnotation>() is { Module: { } package })
                {
                    var options = element.Annotation<PackageUseOptions>();
                    var overrideRules = new List<TemplateRule>();
                    foreach (var overrideElem in options?.OverrideTemplates ?? Enumerable.Empty<XElement>())
                    {
                        foreach (var rule in TemplateRule.FromElement(overrideElem, this))
                            overrideRules.Add(rule);
                    }

                    var childRules = package.GetAllTemplateRules().ToList();
                    foreach (var rule in childRules)
                    {
                        var (local, nsUri) = string.IsNullOrEmpty(rule.Name)
                            ? (null, null)
                            : ExpandVariableName(rule.Element, rule.Name);
                        rule.EffectiveVisibility = GetEffectiveVisibility(package, rule.Element, "template", options, local, nsUri);
                        if (rule.EffectiveVisibility is not "public" and not "final")
                            continue;
                        yield return rule;
                    }

                    foreach (var rule in overrideRules)
                    {
                        rule.EffectiveVisibility = GetExposedVisibility("template", rule.Name, null) ?? GetLocalVisibility(rule.Element, "template", this);
                        yield return rule;
                    }
                }
            }
            else if (ns == XslNamespace && localName == "template" && rulesByElement.TryGetValue(element, out var rules))
            {
                foreach (var rule in rules)
                {
                    var (local, nsUri) = string.IsNullOrEmpty(rule.Name)
                            ? (null, null)
                            : ExpandVariableName(rule.Element, rule.Name);
                    rule.EffectiveVisibility = GetExposedVisibility("template", local, nsUri) ?? GetLocalVisibility(rule.Element, "template", this);
                    yield return rule;
                }
            }
        }
    }

    /// <summary>
    /// Recursively collects all accumulator declarations from this stylesheet, its includes, and its imports.
    /// </summary>
    public IEnumerable<AccumulatorDefinition> GetAllAccumulators()
    {
        foreach (var acc in _accumulators)
            yield return acc;

        foreach (var included in _includes)
        {
            foreach (var acc in included.GetAllAccumulators())
                yield return acc;
        }

        foreach (var imported in _imports)
        {
            foreach (var acc in imported.GetAllAccumulators())
                yield return acc;
        }
    }

    /// <summary>
    /// Recursively collects all named templates from this stylesheet, its includes, and its imports.
    /// Later definitions override earlier ones (local &gt; included &gt; imported).
    /// </summary>
    public Dictionary<string, TemplateRule> GetAllNamedTemplates()
    {
        var result = new Dictionary<string, TemplateRule>();

        // Imported first (lowest precedence, can be overridden)
        foreach (var imported in _imports)
        {
            foreach (var (name, rule) in imported.GetAllNamedTemplates())
            {
                result[name] = rule;
                rule.EffectiveVisibility = imported.GetExposedVisibility("template", rule.Name, null) ?? GetLocalVisibility(rule.Element, "template", imported);
            }
        }

        // Included next (same precedence)
        foreach (var included in _includes)
        {
            foreach (var (name, rule) in included.GetAllNamedTemplates())
            {
                result[name] = rule;
                rule.EffectiveVisibility = included.GetExposedVisibility("template", rule.Name, null) ?? GetLocalVisibility(rule.Element, "template", included);
            }
        }

        // Used packages next: only exported components are visible.
        foreach (var package in _usedPackages)
        {
            var options = _usedPackageOptions.GetValueOrDefault(package);
            var packageView = package.GetAllNamedTemplates();

            foreach (var (name, rule) in packageView)
            {
                if (!IsExportedFromPackage(package, rule))
                    continue;
                var (local, ns) = string.IsNullOrEmpty(rule.Name) ? (null, null) : ExpandVariableName(rule.Element, rule.Name);
                var exposed = package.GetExposedVisibility("template", local, ns);
                var baseVisibility = exposed ?? GetLocalVisibility(rule.Element, "template", package);
                var effectiveRule = GetEffectiveAcceptRule(options, "template", local, ns, -1, baseVisibility);
                rule.EffectiveVisibility = effectiveRule?.Visibility ?? GetDefaultUsedPackageVisibility("template", local, ns, baseVisibility, options);
                // Private templates from a used package are visible only to the owning
                // package itself or to the package that explicitly accepted them as private.
                if (rule.EffectiveVisibility == "private" && effectiveRule != null)
                    rule.AcceptedBy = this;
                // Include public/final templates and private templates from used packages.
                // Hidden and abstract templates are invisible. xsl:initial-template is
                // included even when private so the initial-template selection can raise
                // XTDE0040 when it is not explicitly accepted.
                var eff = rule.EffectiveVisibility ?? "private";
                if (eff is "hidden" or "abstract")
                    continue;
                result[name] = rule;
            }

            // xsl:override template definitions take precedence over used-package templates.
            foreach (var overrideElem in options?.OverrideTemplates ?? Enumerable.Empty<XElement>())
            {
                var nameAttr = overrideElem.Attribute("name")?.Value;
                if (string.IsNullOrEmpty(nameAttr))
                    continue;
                foreach (var rule in TemplateRule.FromElement(overrideElem, this))
                {
                    rule.EffectiveVisibility = GetExposedVisibility("template", rule.Name, null) ?? GetLocalVisibility(rule.Element, "template", this);
                    // Link the overriding template to the used-package declaration it
                    // replaces so xsl:original can dispatch to it at runtime.
                    var (oLocal, oNs) = ExpandVariableName(overrideElem, nameAttr);
                    rule.OverriddenTemplate = packageView.Values.FirstOrDefault(candidate =>
                    {
                        if (string.IsNullOrEmpty(candidate.Name))
                            return false;
                        var (cLocal, cNs) = ExpandVariableName(candidate.Element, candidate.Name);
                        return cLocal == oLocal && cNs == oNs;
                    });
                    result[nameAttr] = rule;
                }
            }
        }

        // Local last (highest precedence)
        foreach (var (name, rule) in _namedTemplates)
        {
            var (local, ns) = string.IsNullOrEmpty(rule.Name) ? (null, null) : ExpandVariableName(rule.Element, rule.Name);
            rule.EffectiveVisibility = GetExposedVisibility("template", local, ns) ?? GetLocalVisibility(rule.Element, "template", this);
            result[name] = rule;
        }

        return result;
    }

    /// <summary>
    /// Returns the named templates visible in this package's own execution scope, with
    /// <c>xsl:override</c> template declarations contributed by packages that use this
    /// package applied on top (XSLT 3.0 §3.5.7.2): references to an overridden named
    /// template inside this package's components bind to the overriding declaration.
    /// Each contributed override is linked to the declaration it replaces
    /// (<see cref="TemplateRule.OverriddenTemplate"/>) so <c>xsl:original</c> dispatches
    /// correctly (override-t-002/007/015).
    /// </summary>
    public Dictionary<string, TemplateRule> GetPackageScopeNamedTemplates()
    {
        var result = GetAllNamedTemplates();
        if (_packageOverrideContributions == null)
            return result;

        foreach (var (user, options) in _packageOverrideContributions)
        {
            foreach (var overrideElem in options.OverrideTemplates)
            {
                var nameAttr = overrideElem.Attribute("name")?.Value;
                if (string.IsNullOrEmpty(nameAttr))
                    continue;
                foreach (var rule in TemplateRule.FromElement(overrideElem, user))
                {
                    var (oLocal, oNs) = ExpandVariableName(overrideElem, nameAttr);
                    rule.EffectiveVisibility = GetLocalVisibility(rule.Element, "template", user);
                    // Replace the entry with the same expanded name (the raw lexical keys
                    // may differ when the two packages bind different prefixes).
                    string? existingKey = null;
                    foreach (var (key, candidate) in result)
                    {
                        if (string.IsNullOrEmpty(candidate.Name))
                            continue;
                        var (cLocal, cNs) = ExpandVariableName(candidate.Element, candidate.Name);
                        if (cLocal == oLocal && cNs == oNs)
                        {
                            existingKey = key;
                            rule.OverriddenTemplate = candidate;
                            break;
                        }
                    }
                    result[existingKey ?? nameAttr] = rule;
                }
            }
        }
        return result;
    }

    /// <summary>
    /// Returns true when a used-package component was explicitly accepted with
    /// <c>private</c> visibility by the using package. Components accepted as <c>hidden</c>
    /// are not visible anywhere in the using package.
    /// </summary>
    private static bool IsAcceptedAsPrivate(PackageUseOptions? options, string componentType, string? localName, string? namespaceUri, int arity = -1)
    {
        if (options == null)
            return false;

        foreach (var rule in options.AcceptRules)
        {
            if (rule.Visibility != "private")
                continue;
            if (rule.Component != componentType && rule.Component != "*")
                continue;
            if (rule.Matches(localName, namespaceUri, arity))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Returns true when the given component is visible outside its declaring package.
    /// Imports and includes are always visible; used packages filter by visibility
    /// and by any <c>xsl:expose</c> declarations on the package root.
    /// </summary>
    private static bool IsExportedFromPackage(Stylesheet package, TemplateRule rule)
    {
        if (!package.IsPackage) return true;
        var (local, ns) = string.IsNullOrEmpty(rule.Name) ? (null, null) : ExpandVariableName(rule.Element, rule.Name);
        var exposed = package.GetExposedVisibility("template", local, ns);
        var effective = exposed ?? GetLocalVisibility(rule.Element, "template", package);
        return effective is "public" or "final";
    }

    /// <summary>
    /// Returns true when the given component is visible outside its declaring package.
    /// Imports and includes are always visible; used packages filter by visibility
    /// and by any <c>xsl:expose</c> declarations on the package root.
    /// </summary>
    private static bool IsExportedFromPackage(Stylesheet package, XsltFunctionDefinition def)
    {
        if (!package.IsPackage) return true;
        var exposed = package.GetExposedVisibility("function", def.LocalName, def.NamespaceUri, def.Arity);
        var effective = exposed ?? def.Visibility;
        return effective is "public" or "final";
    }

    /// <summary>
    /// Returns true when the given top-level element is visible outside its declaring package.
    /// Imports and includes are always visible; used packages filter by visibility
    /// and by any <c>xsl:expose</c> declarations on the package root.
    /// </summary>
    private static bool IsExportedFromPackage(Stylesheet package, XElement element)
    {
        if (!package.IsPackage) return true;
        var (componentType, localName, namespaceUri) = GetElementComponentIdentity(element);
        var exposed = package.GetExposedVisibility(componentType, localName, namespaceUri);
        var effective = exposed ?? GetLocalVisibility(element, componentType, package.IsPackage);
        return effective is "public" or "final";
    }

    /// <summary>
    /// Extracts the component type and expanded name for common named top-level XSLT
    /// elements, used when applying <c>xsl:expose</c> visibility rules.
    /// </summary>
    private static (string ComponentType, string? LocalName, string? NamespaceUri) GetElementComponentIdentity(XElement element)
    {
        var componentType = element.Name.LocalName;
        string? localName = null;
        string? namespaceUri = null;
        if (componentType is "variable" or "param" or "attribute-set" or "key" or "decimal-format"
            or "namespace-alias" or "character-map" or "output" or "strip-space" or "preserve-space")
        {
            var nameAttr = element.Attribute("name")?.Value;
            if (!string.IsNullOrEmpty(nameAttr))
                (localName, namespaceUri) = ExpandVariableName(element, nameAttr);
        }
        return (componentType, localName, namespaceUri);
    }

    /// <summary>
    /// Returns the effective visibility of a used-package component after applying any
    /// matching <c>xsl:accept</c> rule from the using package and any <c>xsl:expose</c>
    /// rules from the used package. If no rule matches, the component keeps its
    /// original declared visibility. For non-package stylesheets the effective visibility
    /// is always <c>public</c>.
    /// </summary>
    private static string? GetEffectiveVisibility(
        Stylesheet package,
        XElement componentElement,
        string componentType,
        PackageUseOptions? options,
        string? localName,
        string? namespaceUri,
        int arity = -1)
    {
        if (!package.IsPackage)
            return "public";

        var exposed = package.GetExposedVisibility(componentType, localName, namespaceUri, arity);
        var baseVisibility = exposed ?? GetLocalVisibility(componentElement, componentType, package);

        if (options != null)
        {
            if (baseVisibility != "private")
            {
                var effectiveRule = GetEffectiveAcceptRule(options, componentType, localName, namespaceUri, arity, baseVisibility);
                if (effectiveRule != null)
                    return effectiveRule.Visibility;
            }

            // xsl:initial-template from a used package is only visible to the using package
            // when explicitly accepted as public/final. The package-local default is private
            // from the using package's perspective (accept-913/914).
            if (componentType == "template" && localName == "initial-template" && namespaceUri == XslNamespace)
                return "private";

            // Abstract components default to hidden unless an accept rule explicitly
            // accepts them as abstract (XSLT 3.0 §3.5.6.1).
            if (baseVisibility == "abstract")
                return "hidden";
        }

        return baseVisibility;
    }

    /// <summary>
    /// Applies the most specific matching <c>xsl:accept</c> rule to the supplied current
    /// visibility. Returns the supplied visibility unchanged if no accept rule matches.
    /// </summary>
    private static string? ApplyAcceptVisibility(
        string? currentVisibility,
        PackageUseOptions? options,
        string componentType,
        string? localName,
        string? namespaceUri,
        int arity = -1)
    {
        if (options == null)
            return currentVisibility;

        // Accept rules only apply to components that are visible in the used package.
        // Private components remain private regardless of any matching accept rule.
        if (currentVisibility == "private")
            return "private";

        var effectiveRule = GetEffectiveAcceptRule(options, componentType, localName, namespaceUri, arity, currentVisibility);
        if (effectiveRule != null)
            return effectiveRule.Visibility;

        // Abstract components default to hidden unless an accept rule explicitly
        // accepts them as abstract (XSLT 3.0 §3.5.6.1).
        if (currentVisibility == "abstract")
            return "hidden";

        return currentVisibility;
    }

    /// <summary>
    /// Determines whether a component from a used package is visible to the using package.
    /// </summary>
    private static bool IsVisibleFromUsedPackage(
        Stylesheet package,
        XElement componentElement,
        string componentType,
        PackageUseOptions? options,
        string? localName,
        string? namespaceUri)
    {
        if (!package.IsPackage)
            return true;

        var effective = GetEffectiveVisibility(package, componentElement, componentType, options, localName, namespaceUri);
        if (effective is "public" or "final")
            return true;

        // xsl:initial-template from a used package is only visible when explicitly
        // accepted as public/final; otherwise it defaults to private.
        if (componentType == "template" && localName == "initial-template" && namespaceUri == XslNamespace)
            return false;

        return false;
    }

    /// <summary>
    /// Returns the original visibility of a component declared locally in a stylesheet.
    /// Package components default to private; non-package stylesheets default to public.
    /// For template rules the visibility is mode-aware: match-only templates inherit the
    /// visibility of the mode they belong to.
    /// </summary>
    private static string? GetLocalVisibility(XElement element, string componentType, Stylesheet stylesheet)
    {
        if (componentType == "template")
            return GetTemplateLocalVisibility(element, stylesheet);
        return GetLocalVisibility(element, componentType, stylesheet.IsPackage);
    }

    /// <summary>
    /// Returns the original visibility of a component declared locally in a stylesheet.
    /// Package components default to private; non-package stylesheets default to public.
    /// </summary>
    private static string? GetLocalVisibility(XElement element, string componentType, bool isPackage)
    {
        if (!isPackage)
            return "public";

        var vis = element.Attribute("visibility")?.Value?.Trim()?.ToLowerInvariant();
        if (string.IsNullOrEmpty(vis))
            vis = "private";
        return vis;
    }

    /// <summary>
    /// Returns the local visibility of a top-level xsl:variable or xsl:param. Per XSLT 3.0
    /// §3.5.7.1, a stylesheet parameter's implicit visibility is private when static and
    /// public when non-static; variables default to private in a package.
    /// </summary>
    private static string? GetParamAwareLocalVisibility(XElement element, bool isPackage)
    {
        var vis = element.Attribute("visibility")?.Value?.Trim()?.ToLowerInvariant();
        if (!string.IsNullOrEmpty(vis))
            return vis;
        if (element.Name.LocalName == "param")
        {
            bool isStatic = element.Attribute("static")?.Value?.Trim() is "yes" or "true" or "1";
            return isStatic ? "private" : "public";
        }
        return GetLocalVisibility(element, "variable", isPackage);
    }

    /// <summary>
    /// XTSE3050: a template rule declared in this package (outside <c>xsl:override</c>) that
    /// names a mode accepted from a used package implicitly redeclares that mode, which
    /// conflicts with the accepted component (override-m-018; XSLT 3.0 §6.6.1: two modes
    /// with the same name must not be visible within a package).
    /// </summary>
    private void ValidateLocalTemplateRuleModeConflicts()
    {
        if (_usedPackages.Count == 0)
            return;
        var localRules = new List<TemplateRule>();
        CollectLocalTemplateRules(localRules);
        foreach (var rule in localRules)
        {
            var modeAttr = rule.Element.Attribute("mode")?.Value;
            if (string.IsNullOrWhiteSpace(modeAttr))
                continue;
            foreach (var token in modeAttr.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (token.StartsWith('#'))
                    continue;
                var expanded = ExpandModeNameForExpose(token, rule.Element);
                // A mode declared locally (explicitly or by an earlier implicit
                // declaration) is this package's own component; only conflicts with an
                // accepted mode matter here.
                if (TryGetMode(expanded, out _))
                    continue;
                foreach (var package in _usedPackages)
                {
                    if (!package.TryGetMode(expanded, out var mode) || mode == null)
                        continue;
                    var options = _usedPackageOptions.GetValueOrDefault(package);
                    var exposed = package.GetExposedVisibility("mode", mode.Name, null);
                    var modeElement = package.FindModeElement(expanded);
                    var baseVis = exposed ?? GetLocalVisibility(modeElement, "mode", package.IsPackage);
                    var effectiveRule = options == null
                        ? null
                        : GetEffectiveAcceptRule(options, "mode", mode.Name, null, -1, baseVis);
                    var effective = effectiveRule?.Visibility ?? baseVis;
                    if (effective is not "hidden" and not "absent")
                        throw new InvalidOperationException($"XTSE3050: The template rule in mode '{token}' implicitly redeclares a mode accepted from a used package; declare it inside xsl:override instead.");
                }
            }
        }
    }

    /// <summary>
    /// Collects the template rules declared in this module and, recursively, in its imported
    /// and included modules. Template rules from used packages and <c>xsl:override</c>
    /// children are not included.
    /// </summary>
    private void CollectLocalTemplateRules(List<TemplateRule> result)
    {
        result.AddRange(_templateRules);
        foreach (var imported in _imports)
            imported.CollectLocalTemplateRules(result);
        foreach (var included in _includes)
            included.CollectLocalTemplateRules(result);
    }

    /// <summary>
    /// Returns the local visibility of a template rule. A named template uses its declared
    /// visibility (defaulting to private in a package). A match-only template inherits the
    /// visibility of the most visible mode it participates in; if it belongs to a public or
    /// final mode it is itself public/final. This mirrors XSLT 3.0 package visibility rules
    /// so that templates in exported modes are not hidden by the package-private default.
    /// </summary>
    private static string? GetTemplateLocalVisibility(XElement element, Stylesheet stylesheet)
    {
        var vis = element.Attribute("visibility")?.Value?.Trim()?.ToLowerInvariant();
        if (!string.IsNullOrEmpty(vis))
            return vis;

        if (!stylesheet.IsPackage)
            return "public";

        // Named-only templates default to private in a package.
        var match = element.Attribute("match")?.Value;
        if (string.IsNullOrEmpty(match))
            return "private";

        // Match-only templates take their visibility from the modes they participate in.
        var modeAttr = element.Attribute("mode")?.Value;
        var modes = TemplateRule.ParseModes(modeAttr, element, stylesheet.DefaultMode);
        foreach (var mode in modes)
        {
            if (mode == "#all" || mode == "#current")
                continue;
            var modeDef = stylesheet.GetModeDefinition(mode);
            if (modeDef != null)
            {
                var modeVis = modeDef.Visibility.ToString().ToLowerInvariant();
                if (modeVis is "public" or "final")
                    return modeVis;
            }
        }

        return "private";
    }

    /// <summary>
    /// Returns the expanded names of all xsl:override variable or parameter declarations
    /// in the supplied <c>xsl:use-package</c> options.
    /// </summary>
    private static HashSet<(string LocalName, string NamespaceUri)> GetOverrideVariableNames(PackageUseOptions? options, bool isParam)
    {
        var result = new HashSet<(string, string)>();
        var overrides = isParam ? options?.OverrideParams : options?.OverrideVariables;
        foreach (var overrideElem in overrides ?? Enumerable.Empty<XElement>())
        {
            var nameAttr = overrideElem.Attribute("name")?.Value;
            if (!string.IsNullOrEmpty(nameAttr))
            {
                var (local, ns) = ExpandVariableName(overrideElem, nameAttr);
                result.Add((local, ns));
            }
        }
        return result;
    }

    /// <summary>
    /// Checks whether an overriding template rule replaces a given used-package rule.
    /// An override with a <c>@name</c> replaces a named template with the same expanded name;
    /// an override with a <c>@match</c> replaces a template rule with the same stripped
    /// match pattern and the same modes.
    /// </summary>
    private static bool TemplateRuleOverrides(TemplateRule overrideRule, TemplateRule baseRule)
    {
        if (!string.IsNullOrEmpty(overrideRule.Name))
        {
            if (string.IsNullOrEmpty(baseRule.Name))
                return false;
            var (oLocal, oNs) = ExpandVariableName(overrideRule.Element, overrideRule.Name);
            var (bLocal, bNs) = ExpandVariableName(baseRule.Element, baseRule.Name);
            return oLocal == bLocal && oNs == bNs;
        }

        if (!string.IsNullOrEmpty(overrideRule.Match) && !string.IsNullOrEmpty(baseRule.Match))
        {
            var oMatch = Patterns.PatternCompiler.StripXPathComments(overrideRule.Match).Trim();
            var bMatch = Patterns.PatternCompiler.StripXPathComments(baseRule.Match).Trim();
            if (!string.Equals(oMatch, bMatch, StringComparison.Ordinal))
                return false;
            var oModes = new HashSet<string>(overrideRule.Modes);
            var bModes = new HashSet<string>(baseRule.Modes);
            return oModes.SetEquals(bModes);
        }

        return false;
    }

    private void Load()
    {
        var root = _document.Root;
        if (root == null)
            throw new InvalidOperationException("Stylesheet document has no root element.");

        var rootName = root.Name;

        if (rootName.NamespaceName != XslNamespace)
            throw new InvalidOperationException($"Expected xsl:stylesheet or xsl:transform, got {rootName}.");

        if (rootName.LocalName != "stylesheet" && rootName.LocalName != "transform" && rootName.LocalName != "package")
            throw new InvalidOperationException($"Expected xsl:stylesheet or xsl:transform, got {rootName}.");

        IsPackage = rootName.LocalName == "package";
        if (IsPackage)
        {
            var packageNameAttr = root.Attribute("name");
            // Unnamed packages are allowed when they are the principal stylesheet. Packages
            // referenced via xsl:use-package must have a name so they can be targeted.
            if (!_isRootStylesheet && (packageNameAttr == null || string.IsNullOrWhiteSpace(packageNameAttr.Value)))
                throw new InvalidOperationException("XTSE0010: The name attribute is required on xsl:package.");
            PackageName = packageNameAttr?.Value.Trim();
            PackageVersion = root.Attribute("package-version")?.Value;
        }

        // Expand a shadow version attribute before the effective version is determined.
        ExpandShadowAttribute(root, "version");

        // Expand a shadow package-version attribute before validating the package version.
        if (IsPackage)
            ExpandShadowAttribute(root, "package-version");

        // Required version attribute (XTSE0010)
        var versionAttr = root.Attribute("version");
        if (versionAttr == null || string.IsNullOrWhiteSpace(versionAttr.Value))
            throw new InvalidOperationException("XTSE0010: The version attribute is required on xsl:stylesheet or xsl:transform.");

        var versionValue = versionAttr.Value.Trim();
        if (!decimal.TryParse(versionValue, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out _))
            throw new InvalidOperationException("XTSE0110: The version attribute must be a valid decimal number.");

        Version = versionValue;

        // Validate xsl:package/@package-version after any shadow expansion.
        // XTSE0020 if the value does not match the PackageVersion grammar.
        if (IsPackage)
        {
            var packageVersionAttr = root.Attribute("package-version");
            if (packageVersionAttr != null)
            {
                var pv = packageVersionAttr.Value.Trim();
                if (string.IsNullOrEmpty(pv) || !Api.PackageVersion.IsValidVersion(pv))
                    throw new InvalidOperationException($"XTSE0020: Invalid package-version '{pv}'.");
                PackageVersion = pv;
            }
        }

        // Parse xsl:stylesheet/@input-type-annotations (strip, preserve, or unspecified).
        var inputTypeAnnotationsAttr = root.Attribute("input-type-annotations")?.Value?.Trim()?.ToLowerInvariant();
        if (!string.IsNullOrEmpty(inputTypeAnnotationsAttr))
        {
            if (inputTypeAnnotationsAttr != "strip" && inputTypeAnnotationsAttr != "preserve" && inputTypeAnnotationsAttr != "unspecified")
                throw new InvalidOperationException($"XTSE0020: Invalid value '{inputTypeAnnotationsAttr}' for xsl:stylesheet/@input-type-annotations.");
            InputTypeAnnotations = inputTypeAnnotationsAttr;
        }

        // Parse xsl:declared-modes on stylesheet/package root. The attribute is an
        // xs:boolean, so "yes"/"true"/"1" enables the check and "no"/"false"/"0"
        // disables it. For xsl:package the default is "yes"; for xsl:stylesheet it
        // is "no".
        var declaredModesAttr = root.Attribute("declared-modes")?.Value?.Trim()?.ToLowerInvariant();
        DeclaredModes = declaredModesAttr switch
        {
            "no" or "false" or "0" => false,
            "yes" or "true" or "1" or null or "" => true,
            _ => throw new InvalidOperationException($"XTSE0020: Invalid value '{declaredModesAttr}' for @declared-modes. Must be yes/no/true/false/0/1.")
        };

        // Parse xsl:default-mode on stylesheet root
        var defaultModeAttr = root.Attribute("default-mode")?.Value?.Trim() ?? "";
        DefaultMode = defaultModeAttr;
        if (defaultModeAttr == "#unnamed" || defaultModeAttr == "#default")
        {
            DefaultMode = ""; // the unnamed mode
        }
        else if (!string.IsNullOrEmpty(defaultModeAttr) && defaultModeAttr != "#current" && defaultModeAttr != "#all")
        {
            int colon = defaultModeAttr.IndexOf(':');
            if (colon >= 0)
            {
                var prefix = defaultModeAttr.Substring(0, colon);
                var local = defaultModeAttr.Substring(colon + 1);
                var current = root;
                while (current != null)
                {
                    foreach (var attr in current.Attributes())
                    {
                        if (attr.IsNamespaceDeclaration && attr.Name.LocalName == prefix)
                        {
                            DefaultMode = $"{{{attr.Value}}}{local}";
                            break;
                        }
                    }
                    if (DefaultMode != defaultModeAttr)
                        break;
                    current = current.Parent;
                }
            }
        }

        // Parse exclude-result-prefixes
        var excludePrefixesAttr = root.Attribute("exclude-result-prefixes")?.Value;
        if (!string.IsNullOrWhiteSpace(excludePrefixesAttr))
        {
            foreach (var token in excludePrefixesAttr.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
            {
                _excludedResultPrefixes.Add(token.Trim());
            }

            // XTSE0808: every prefix token must be bound in scope (XSLT 2.0+).
            if (GetEffectiveVersion(root) >= 2.0)
                ValidateExcludeResultPrefixesValue(root, excludePrefixesAttr, "xsl:stylesheet/@exclude-result-prefixes");
        }

        // Validate extension-element-prefixes: reserved namespaces are not permitted.
        var extensionPrefixesAttr = root.Attribute("extension-element-prefixes")?.Value;
        if (!string.IsNullOrWhiteSpace(extensionPrefixesAttr))
        {
            foreach (var token in extensionPrefixesAttr.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var prefix = token.Trim();
                string nsUri;
                if (prefix == "#default")
                    nsUri = root.GetDefaultNamespace().NamespaceName;
                else
                    nsUri = root.GetNamespaceOfPrefix(prefix)?.NamespaceName ?? string.Empty;

                if (nsUri == XslNamespace ||
                    nsUri == System.Xml.Linq.XNamespace.Xml.NamespaceName ||
                    nsUri == "http://www.w3.org/2001/XMLSchema" ||
                    nsUri == "http://www.w3.org/2001/XMLSchema-instance")
                {
                    throw new InvalidOperationException("XTSE0800");
                }
            }
        }

        /// <summary>
        /// Recursively strips elements whose <c>use-when</c> attribute evaluates to <c>false()</c>.
        /// This is applied to the entire stylesheet tree, including nested elements inside
        /// template bodies, so that <c>use-when</c> on instructions and LREs is respected.
        /// Descendants of an excluded element are not evaluated.
        /// </summary>
        void StripUseWhenElements(XElement parent)
        {
            var children = parent.Elements().ToList();
            foreach (var child in children)
            {
                if (UseWhen(child))
                {
                    StripUseWhenElements(child);
                }
                else
                {
                    child.Remove();
                }
            }
        }

        // Build the static context (static variables/parameters, imports and includes)
        // before evaluating any use-when expressions.
        BuildStaticContext();

        // Apply use-when stripping to the entire tree (nested elements inside templates,
        // literal result elements, etc.) after imports/includes are resolved.
        StripUseWhenElements(root);

        // Expand any remaining shadow attributes (e.g. _xpath-default-namespace on
        // xsl:template) now that the static context is fully built.
        ExpandAllShadowAttributes(root);

        // Parse top-level xsl:expose declarations on a package root.
        foreach (var expose in root.Elements(XName.Get("expose", XslNamespace)))
        {
            _exposeRules.Add(ParseExposeRule(expose));
        }

        // Parse top-level xsl:param declarations
        foreach (var param in root.Elements(XName.Get("param", XslNamespace)))
        {
            if (!UseWhen(param)) continue;
            _globalParameters.Add(param);
        }

        // Parse top-level xsl:variable declarations
        foreach (var variable in root.Elements(XName.Get("variable", XslNamespace)))
        {
            if (!UseWhen(variable)) continue;
            _globalVariables.Add(variable);
        }

        // Parse xsl:key declarations
        foreach (var key in root.Elements(XName.Get("key", XslNamespace)))
        {
            if (!UseWhen(key)) continue;
            var def = KeyDefinition.FromElement(key, this);
            _keyDefinitions.Add(def);
        }

        // Parse xsl:accumulator declarations
        foreach (var acc in root.Elements(XName.Get("accumulator", XslNamespace)))
        {
            if (!UseWhen(acc)) continue;
            var def = AccumulatorDefinition.FromElement(acc, this);
            if (def != null)
                _accumulators.Add(def);
        }

        // XTSE3350: two accumulators in the same stylesheet module must not share the same expanded name.
        if (_isRootStylesheet)
        {
            var seenAccumulators = new HashSet<string>();
            foreach (var def in _accumulators)
            {
                if (!seenAccumulators.Add(def.ClarkName))
                    throw new InvalidOperationException($"XTSE3350: duplicate xsl:accumulator name '{def.ClarkName}'.");
            }
        }

        // Parse xsl:strip-space and xsl:preserve-space declarations.
        // Rules are collected in document order so that same-precedence conflicts
        // are resolved by last-match-wins.
        foreach (var decl in root.Elements()
            .Where(e => e.Name.NamespaceName == XslNamespace
                     && (e.Name.LocalName == "strip-space" || e.Name.LocalName == "preserve-space")))
        {
            if (!UseWhen(decl)) continue;
            bool isStrip = decl.Name.LocalName == "strip-space";
            var elements = decl.Attribute("elements")?.Value;
            var defaultNs = GetXPathDefaultNamespace(decl);
            if (!string.IsNullOrEmpty(elements))
            {
                foreach (var nameTest in elements.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var rule = SpaceHandlingRule.FromNameTest(nameTest, decl, this, defaultNs, isStrip, ImportPrecedence);
                    _spaceRules.Add(rule);
                }
            }
        }

        // Parse xsl:mode declarations. Collect all local declarations first so that
        // duplicate or conflicting declarations at the same import precedence can be
        // detected before the dictionary overwrites earlier entries.
        var localModes = new List<ModeDefinition>();
        foreach (var mode in root.Elements(XName.Get("mode", XslNamespace)))
        {
            if (!UseWhen(mode)) continue;
            var def = ModeDefinition.FromElement(mode, IsPackage);
            if (def != null)
                localModes.Add(def);
        }

        // Check for duplicate/conflicting local declarations in this stylesheet module.
        // For non-root modules this is deferred to the root-level validation, because
        // conflicts in imported modules may be overridden by a higher-precedence declaration.
        if (_isRootStylesheet)
        {
            var seenLocalModes = new Dictionary<string, ModeDefinition>();
            foreach (var def in localModes)
            {
                if (seenLocalModes.TryGetValue(def.Name, out var existing))
                {
                    if (!AreModesEquivalent(existing, def))
                        throw new InvalidOperationException($"XTSE0545: Conflicting xsl:mode declarations for mode '{def.Name}' at the same import precedence.");
                }
                else
                {
                    seenLocalModes[def.Name] = def;
                }
            }
        }

        foreach (var def in localModes)
        {
            _modeDefinitions[def.Name] = def;
        }

        // Validate mode definitions across the complete stylesheet tree. Conflicting
        // declarations at the winning import precedence are a static error; declarations
        // overridden by a higher-precedence declaration are ignored.
        if (_isRootStylesheet)
            ValidateModeDefinitions();

        // Parse xsl:global-context-item declaration (XSLT 3.0)
        var gciElements = root.Elements(XName.Get("global-context-item", XslNamespace)).Where(e => UseWhen(e)).ToList();
        // XTSE3087: at most one xsl:global-context-item declaration per stylesheet module.
        if (gciElements.Count > 1)
            throw new InvalidOperationException("XTSE3087: More than one xsl:global-context-item declaration in a stylesheet module.");
        foreach (var gci in gciElements)
        {
            var use = gci.Attribute("use")?.Value?.Trim();
            var asType = gci.Attribute("as")?.Value?.Trim();
            // XTSE3089: use="absent" and as must not both be present.
            if (use == "absent" && !string.IsNullOrEmpty(asType))
                throw new InvalidOperationException("XTSE3089: xsl:global-context-item must not have an as attribute when use is absent.");
            GlobalContextItemUse = use;
            GlobalContextItemAs = asType;
        }

        // XTSE3087: xsl:global-context-item declarations must be consistent across the
        // modules of a package (glob-cxt-item-005/007). Checked here, after this module's
        // own declaration is parsed and after includes/imports are resolved.
        if (_isRootStylesheet)
            ValidateGlobalContextItemConsistency();

        // Parse xsl:output properties. Multiple xsl:output declarations are merged,
        // with later declarations overriding earlier ones for the same property.
        // Conflicting explicit values for the same scalar attribute raise XTSE1560.
        // Named outputs are stored separately by expanded QName and are used by
        // xsl:result-document via its @format attribute.
        var outputElems = root.Elements(XName.Get("output", XslNamespace)).Where(e => UseWhen(e)).ToList();
        foreach (var oe in outputElems)
        {
            var explicitProps = OutputProperties.FromElement(oe);

            // A parameter document supplies default values; explicit xsl:output attributes
            // override values from the parameter document.
            var props = explicitProps.Clone();
            var paramDocAttr = oe.Attribute("parameter-document")?.Value;
            if (!string.IsNullOrEmpty(paramDocAttr))
            {
                var paramDoc = _resolver.Resolve(paramDocAttr, _baseUri);
                var paramProps = OutputProperties.FromSerializationParameters(paramDoc);
                var merged = paramProps.Clone();
                OutputProperties.Merge(merged, props);
                props = merged;
            }

            var nameAttr = oe.Attribute("name")?.Value;
            if (!string.IsNullOrEmpty(nameAttr))
            {
                var expandedName = ExpandQName(oe, nameAttr);
                if (_namedOutputProperties.TryGetValue(expandedName, out var existing))
                {
                    OutputProperties.MergeChecked(existing, explicitProps);
                    OutputProperties.Merge(existing, props);
                }
                else
                {
                    _namedOutputProperties[expandedName] = props.Clone();
                }
            }
            else
            {
                if (_outputProperties == null)
                    _outputProperties = new OutputProperties();
                OutputProperties.MergeChecked(_outputProperties, explicitProps);
                OutputProperties.Merge(_outputProperties, props);
            }
        }

        // Parse xsl:character-map declarations.
        foreach (var cm in root.Elements(XName.Get("character-map", XslNamespace)))
        {
            if (!UseWhen(cm)) continue;
            var def = CharacterMapDefinition.FromElement(cm, this);
            if (_characterMaps.ContainsKey(def.ExpandedName))
                throw new InvalidOperationException($"XTSE1580: Duplicate character-map name '{def.ExpandedName}'.");
            _characterMaps[def.ExpandedName] = def;
        }

        // Parse xsl:namespace-alias declarations
        foreach (var alias in root.Elements(XName.Get("namespace-alias", XslNamespace)))
        {
            if (!UseWhen(alias)) continue;
            _namespaceAliases.Add(NamespaceAliasDefinition.FromElement(alias, this));
        }

        // XTSE0130: top-level elements in no namespace are not permitted unless they are
        // XSLT instructions. Data elements in other namespaces are allowed.
        foreach (var topLevel in root.Elements())
        {
            if (topLevel.Name.NamespaceName != XslNamespace && string.IsNullOrEmpty(topLevel.Name.NamespaceName))
                throw new InvalidOperationException($"XTSE0130: Top-level element '{topLevel.Name.LocalName}' is not permitted in no namespace.");
        }

        // Collect template rules from this stylesheet
        foreach (var template in root.Elements(XName.Get("template", XslNamespace)))
        {
            if (!UseWhen(template)) continue;
            var rules = TemplateRule.FromElement(template, this);
            if (rules.Count > 0)
            {
                foreach (var rule in rules)
                {
                    if (!string.IsNullOrEmpty(rule.Match))
                        _templateRules.Add(rule);
                }
                // Named templates: register only the first rule (all share the same body)
                var firstNamed = rules.FirstOrDefault(r => !string.IsNullOrEmpty(r.Name));
                if (firstNamed != null && firstNamed.Name != null)
                {
                    // XTSE0080: template names must not use a reserved namespace.
                    ValidateNamedQName(firstNamed.Element, firstNamed.Name, "xsl:template");
                    _namedTemplates[firstNamed.Name] = firstNamed;
                }
            }
        }

        // Parse xsl:function declarations
        foreach (var func in root.Elements(XName.Get("function", XslNamespace)))
        {
            if (!UseWhen(func)) continue;
            var def = XsltFunctionDefinition.FromElement(func, this);
            if (def != null)
                _functionDefinitions.Add(def);
        }

        // XTSE0770: duplicate xsl:function declarations with the same expanded QName and arity
        {
            var seenFunctions = new HashSet<(string ns, string local, int arity)>();
            foreach (var def in _functionDefinitions)
            {
                var key = (def.NamespaceUri, def.LocalName, def.Arity);
                if (!seenFunctions.Add(key))
                    throw new InvalidOperationException($"XTSE0770: Duplicate function declaration '{{{def.NamespaceUri}}}{def.LocalName}#{def.Arity}'.");
            }
        }

        // Parse xsl:decimal-format declarations
        foreach (var df in root.Elements(XName.Get("decimal-format", XslNamespace)))
        {
            if (!UseWhen(df)) continue;
            var def = DecimalFormatDefinition.FromElement(df, this);
            if (def != null)
                _decimalFormats.Add(def);
        }

        // Parse xsl:attribute-set declarations
        foreach (var attrSet in root.Elements(XName.Get("attribute-set", XslNamespace)))
        {
            if (!UseWhen(attrSet)) continue;
            var def = AttributeSetDefinition.FromElement(attrSet, this);
            if (def != null)
                _attributeSets.Add(def);
        }

        // Static validation: xsl:attribute-set/@use-attribute-sets must reference
        // existing attribute sets (XTSE0710). Only enforced in XSLT 2.0 and later.
        foreach (var attrSet in _attributeSets)
        {
            if (!string.IsNullOrWhiteSpace(attrSet.UseAttributeSets) && GetEffectiveVersion(attrSet.Element) >= 2.0)
                ValidateUseAttributeSetsValue(attrSet.Element, attrSet.UseAttributeSets, "xsl:attribute-set/@use-attribute-sets");
        }

        // XTSE0720: xsl:attribute-set must not directly or indirectly reference itself
        // via use-attribute-sets. Checked at the root stylesheet so imports/includes
        // are visible.
        if (_isRootStylesheet)
            ValidateAttributeSetCircularity();

        // XTSE3060: an xsl:override must not override a final component from a used package.
        if (_isRootStylesheet)
        {
            ValidateAttributeSetOverrides();
            ValidateVariableOverrides();
            ValidateFunctionOverrides();
            ValidateModeOverrides();
            ValidateTemplateOverrides();
        }

        // Validate xsl:accept rules against the components declared in used packages.
        // Runs before ValidateInstructionTree so that an xsl:accept name matching no
        // used-package component (XTSE3030) is reported before instruction-level checks
        // such as xsl:use-attribute-sets resolution (accept-004).
        if (_isRootStylesheet)
            ValidateAcceptRules();

        // Static validation: check for disallowed attributes and children on XSLT instructions
        ValidateInstructionTree(root);

        // XTSE1222: all xsl:key declarations with the same expanded name must agree on @composite.
        if (_isRootStylesheet)
        {
            var allKeyDefs = GetAllKeyDefinitions();
            foreach (var group in allKeyDefs.GroupBy(k => k.Name))
            {
                if (group.Select(k => k.Composite).Distinct().Count() > 1)
                    throw new InvalidOperationException($"XTSE1222: xsl:key definitions for '{group.Key}' have conflicting @composite values.");
            }

            // XTSE1290: decimal-format declarations with the same name must not supply
            // conflicting values for the same attribute at the same import precedence.
            GetAllDecimalFormats();

            // XTSE1590: every name in xsl:output/@use-character-maps and
            // xsl:character-map/@use-character-maps must resolve to a declared character map.
            ValidateCharacterMapReferences();

            // XTSE1600: character maps must not reference themselves, directly or indirectly.
            ValidateCharacterMapCycles();

            // XTSE0265: conflicting xsl:stylesheet/@input-type-annotations values across modules.
            ValidateInputTypeAnnotations();

            // Validate xsl:expose rules against the components declared in this package.
            ValidateExposeRules();

            // Detect conflicting visible components exported by multiple used packages
            // when no xsl:accept rule hides one of them.
            ValidateUsedPackageConflicts();

            // XTSE3050: local template rules must not implicitly redeclare a mode accepted
            // from a used package (override-m-018).
            ValidateLocalTemplateRuleModeConflicts();

            // XTSE3080: a top-level package must not contain components whose effective
            // visibility is abstract, whether or not they are referenced. Applies only
            // to the principal module of the compilation, not to used packages.
            if (ReferenceEquals(_rootStylesheet, this))
                ValidateTopLevelPackageAbstractComponents();
        }
    }

    /// <summary>
    /// Builds the static context for this module by resolving imports/includes and
    /// evaluating static variables/parameters in document order. Top-level
    /// <c>use-when</c> attributes are evaluated as each element is encountered so
    /// that forward references to imported static variables are detected.
    /// </summary>
    private void BuildStaticContext()
    {
        var root = _document.Root;
        if (root == null) return;

        foreach (var child in root.Elements().ToList())
        {
            // Expand shadow attributes on XSLT elements before any of their real
            // attribute values are consumed (e.g. _static on xsl:variable, _href on
            // xsl:include, _use-when on any top-level element).
            if (child.Name.NamespaceName == XslNamespace)
                ExpandAllShadowAttributes(child);

            var ns = child.Name.NamespaceName;
            var localName = child.Name.LocalName;

            if (ns == XslNamespace && localName == "import")
            {
                if (UseWhen(child))
                {
                    var href = child.Attribute("href")?.Value;
                    if (string.IsNullOrEmpty(href))
                        throw new InvalidOperationException("XTSE0010: Missing required href attribute on xsl:import.");
                    ResolveImport(child, href);
                }
                else
                {
                    child.Remove();
                }
            }
            else if (ns == XslNamespace && localName == "include")
            {
                if (UseWhen(child))
                {
                    var href = child.Attribute("href")?.Value;
                    if (string.IsNullOrEmpty(href))
                        throw new InvalidOperationException("XTSE0010: Missing required href attribute on xsl:include.");
                    ResolveInclude(child, href);
                }
                else
                {
                    child.Remove();
                }
            }
            else if (ns == XslNamespace && localName == "use-package")
            {
                // XTSE3008: xsl:use-package is only allowed at the same stylesheet level as the
                // principal stylesheet module of the package. Modules reached by xsl:import are
                // at a lower level and cannot contain xsl:use-package.
                if (!IsPrincipalLevel)
                    throw new InvalidOperationException("XTSE3008: xsl:use-package is not allowed in a stylesheet module that is not at the same stylesheet level as the principal stylesheet module of the package.");

                if (UseWhen(child))
                {
                    var name = child.Attribute("name")?.Value;
                    if (string.IsNullOrEmpty(name))
                        throw new InvalidOperationException("XTSE0010: Missing required name attribute on xsl:use-package.");
                    var packageVersion = child.Attribute("package-version")?.Value?.Trim();
                    if (!string.IsNullOrEmpty(packageVersion) && packageVersion != "*" && !Api.PackageVersion.IsValidVersionRange(packageVersion))
                        throw new InvalidOperationException($"XTSE0020: Invalid package-version range '{packageVersion}'.");
                    ResolveUsePackage(child, name, packageVersion);
                    ParsePackageUseOptions(child);
                    if (child.Annotation<ResolvedModuleAnnotation>()?.Module is { } usedModule)
                    {
                        // XTTE0590: an xsl:global-context-item in a library package is
                        // ignored unless it specifies use="required", which is an error
                        // (glob-cxt-item-009).
                        if (usedModule.GlobalContextItemUse == "required")
                            throw new InvalidOperationException($"XTTE0590: The used package '{name}' declares xsl:global-context-item with use='required'.");
                        var useOptions = child.Annotation<PackageUseOptions>();
                        _usedPackageOptions[usedModule] = useOptions;
                        if (useOptions != null)
                            usedModule.RegisterPackageOverrideContribution(this, useOptions);
                    }
                }
                else
                {
                    child.Remove();
                }
            }
            else if (ns == XslNamespace && (localName == "variable" || localName == "param"))
            {
                ProcessStaticVariable(child);
            }
            else
            {
                // Evaluate use-when on all other top-level elements immediately, using
                // only the static context established up to this point.
                if (!UseWhen(child))
                    child.Remove();
            }
        }

        // Any same-precedence conflicts not resolved by a higher-precedence declaration
        // are reported now.
        if (_staticContext.PendingConflicts.Count > 0)
        {
            var first = _staticContext.PendingConflicts.First();
            throw new InvalidOperationException($"XTSE3450: Inconsistent static declarations: conflicting values for '{{{first.Key.NamespaceUri}}}{first.Key.LocalName}' at the same import precedence.");
        }

        // Recompute import precedence numbers so that later sibling imports have higher
        // precedence than earlier ones, and imported modules are always lower than their
        // importer. This produces the total order required by XSLT §6.4.
        AssignImportPrecedences();

        // XTSE0630: more than one binding of a global variable with the same name and
        // same import precedence is an error unless a higher-precedence binding exists.
        if (_isRootStylesheet)
            ValidateGlobalVariableBindings();

        // XTSE0660: more than one named template with the same name and the same
        // import precedence is an error unless a higher-precedence template exists.
        if (_isRootStylesheet)
            ValidateNamedTemplateBindings();
    }

    /// <summary>
    /// Processes a single static variable or parameter declaration, validating its
    /// attributes, evaluating its select expression, and adding it to the static context.
    /// </summary>
    private void ProcessStaticVariable(XElement elem)
    {
        var staticAttr = elem.Attribute("static")?.Value;
        if (staticAttr is null)
            return;

        var trimmed = staticAttr.Trim();
        if (IsStaticYes(trimmed))
        {
            // Static declaration: evaluate if not excluded by its own use-when.
            if (!UseWhen(elem))
                return;

            if (!IsStaticBodyEmpty(elem))
                throw new InvalidOperationException("XTSE0010: Static variable or parameter must not have a sequence constructor.");

            // XTSE0090: tunnel is not permitted on static variables/parameters.
            if (elem.Attribute("tunnel") != null)
                throw new InvalidOperationException("XTSE0020: The tunnel attribute is not permitted on a static variable or parameter.");

            // XTSE0090: visibility is not permitted on static variables/parameters.
            if (elem.Attribute("visibility") != null)
                throw new InvalidOperationException("XTSE0090: The visibility attribute is not permitted on a static variable or parameter.");

            var select = elem.Attribute("select")?.Value;
            var requiredAttr = elem.Attribute("required")?.Value;
            bool isRequired = requiredAttr != null && IsStaticYes(requiredAttr.Trim());

            // XTSE0010: a required static parameter must not have a select attribute.
            if (isRequired && !string.IsNullOrEmpty(select))
                throw new InvalidOperationException("XTSE0010: A required static parameter must not have a select attribute.");

            bool isParam = elem.Name.LocalName == "param";

            // Determine the effective value. Externally supplied parameter values override
            // the default select expression, which avoids forward-reference errors when a
            // static param is referenced before it is declared.
            var name = elem.Attribute("name")?.Value;
            var (local, ns) = string.IsNullOrEmpty(name) ? (string.Empty, string.Empty) : ExpandVariableName(elem, name);
            var key = (local, ns);

            XdmValue value;
            if (isParam && _externalStaticParameters.TryGetValue(key, out var externalValue))
            {
                value = externalValue;
            }
            else if (string.IsNullOrEmpty(select))
            {
                // No select attribute: required parameters have no default value;
                // all other static declarations default to the empty sequence.
                value = isRequired ? XdmValue.Undefined : XdmValue.FromSequence(XdmSequence.Empty);
            }
            else
            {
                value = EvaluateStaticExpression(select, elem);
            }

            // Validate externally supplied parameter values against the declared @as type
            // at compile time (XTTE0590). Default values are validated at runtime so that
            // empty-sequence defaults for optional types are preserved correctly.
            var asType = elem.Attribute("as")?.Value;
            if (isParam && _externalStaticParameters.ContainsKey(key) && !string.IsNullOrEmpty(asType))
            {
                var ctx = CreateUseWhenContext(elem);
                value = TransformEngine.ConvertVariableValue(value, asType, isParam, ctx);
            }

            AddStaticVariable(elem, value, ImportPrecedence);
        }
        else if (IsStaticNo(trimmed))
        {
            // Non-static declaration: not part of the static context.
        }
        else
        {
            throw new InvalidOperationException("XTSE0020: Invalid value for static attribute.");
        }
    }

    /// <summary>
    /// Returns true if the given static attribute value means "static".
    /// </summary>
    private static bool IsStaticYes(string value)
        => value.Trim() is "yes" or "true" or "1";

    /// <summary>
    /// Returns true if the given static attribute value means "non-static".
    /// </summary>
    private static bool IsStaticNo(string value)
        => value.Trim() is "no" or "false" or "0";

    /// <summary>
    /// Evaluates an XPath expression in the current static context.
    /// </summary>
    private XdmValue EvaluateStaticExpression(string expression, XElement elem)
    {
        var ctx = CreateUseWhenContext(elem);
        var compiled = XPath31Expression.Compile(expression);
        return compiled.Evaluate(ctx);
    }

    /// <summary>
    /// Returns true if the content of a static variable or parameter is empty after
    /// applying use-when to its children.
    /// </summary>
    private bool IsStaticBodyEmpty(XElement elem)
    {
        foreach (var node in elem.Nodes())
        {
            if (node is XText text && string.IsNullOrWhiteSpace(text.Value))
                continue;
            if (node is XComment or XProcessingInstruction)
                continue;
            if (node is XElement child)
            {
                // If the child is excluded by use-when, it contributes no content.
                if (UseWhen(child))
                    return false;
                continue;
            }
            return false;
        }
        return true;
    }

    /// <summary>
    /// Adds a static variable or parameter to the static context, detecting conflicts
    /// with existing declarations (XTSE3450).
    /// </summary>
    private void AddStaticVariable(XElement elem, XdmValue value, int precedence)
    {
        var name = elem.Attribute("name")?.Value;
        if (string.IsNullOrEmpty(name))
            return;

        var (local, ns) = ExpandVariableName(elem, name);
        AddStaticVariable((local, ns), value, elem.Name.LocalName == "param", precedence);
    }

    /// <summary>
    /// Adds a static variable or parameter to the static context, detecting conflicts
    /// with existing declarations (XTSE3450).
    /// </summary>
    private void AddStaticVariable((string LocalName, string NamespaceUri) key, XdmValue value, bool isParam, int precedence)
    {
        if (_staticContext.Variables.TryGetValue(key, out var existing))
        {
            if (existing.IsParam != isParam)
            {
                // XTSE3450: a static variable and static parameter with the same expanded
                // name are inconsistent unless the lower-precedence declaration is
                // overridden by a higher-precedence declaration that was processed first.
                if (precedence > existing.Precedence)
                    return; // new declaration has lower precedence: overridden

                // New declaration has same or higher precedence: conflicting kinds.
                throw new InvalidOperationException($"XTSE3450: Inconsistent static declarations: variable and parameter have the same name '{{{key.NamespaceUri}}}{key.LocalName}'.");
            }

            if (precedence > existing.Precedence)
            {
                // New declaration has lower import precedence: it is overridden.
                return;
            }

            if (precedence < existing.Precedence)
            {
                // New declaration has higher import precedence. If it resolves a
                // pending same-precedence conflict, the conflict is settled. Otherwise
                // a change in the effective value is inconsistent.
                if (!_staticContext.PendingConflicts.ContainsKey(key) &&
                    !XdmValueEqualityComparer.Instance.Equals(existing.Value, value))
                {
                    throw new InvalidOperationException($"XTSE3450: Inconsistent static declarations: conflicting values for '{{{key.NamespaceUri}}}{key.LocalName}'.");
                }

                _staticContext.Variables[key] = (value, isParam, precedence);
                _staticContext.PendingConflicts.Remove(key);
                return;
            }

            // Same import precedence: values must be identical.
            if (XdmValueEqualityComparer.Instance.Equals(existing.Value, value))
                return;

            _staticContext.PendingConflicts[key] = (existing.Value, value);
            return;
        }

        _staticContext.Variables[key] = (value, isParam, precedence);
    }

    /// <summary>
    /// Merges the static context of an imported or included child module into this
    /// module's static context.
    /// </summary>
    private void MergeChildStaticContext(StaticContext child)
    {
        foreach (var (key, entry) in child.Variables)
            AddStaticVariable(key, entry.Value, entry.IsParam, entry.Precedence);
    }

    /// <summary>
    /// Performs static validation of the stylesheet tree, checking for disallowed
    /// attributes and children on XSLT instructions.
    /// </summary>
    /// <summary>
    /// Returns false when the element is inside an unknown XSLT element that is in
    /// forwards-compatible mode, unless the element is a descendant of an
    /// <c>xsl:fallback</c> child of that unknown element.
    /// </summary>
    private bool ShouldValidateElement(XElement element)
    {
        var current = element.Parent;
        while (current != null)
        {
            if (current.Name.NamespaceName == XslNamespace &&
                !KnownXsltElementNames.Contains(current.Name.LocalName) &&
                IsForwardsCompatibleElement(current))
            {
                // Walk up from the element to the unknown ancestor to find the
                // immediate child of the unknown ancestor on that path.
                var childOnPath = element;
                while (childOnPath.Parent != null && childOnPath.Parent != current)
                    childOnPath = childOnPath.Parent;

                if (childOnPath.Name.NamespaceName == XslNamespace && childOnPath.Name.LocalName == "fallback")
                    return true;

                return false;
            }
            current = current.Parent;
        }
        return true;
    }

    private void ValidateInstructionTree(XElement root)
    {
        // XTSE0010: xsl:on-completion must be a direct child of xsl:iterate. This pre-pass
        // runs before the per-element walk so that a misplaced xsl:on-completion reports
        // the structural error even when an earlier element carries its own attribute error.
        foreach (var onCompletion in root.Descendants(XName.Get("on-completion", XslNamespace)))
        {
            if (!ShouldValidateElement(onCompletion))
                continue;
            var parent = onCompletion.Parent;
            if (parent == null || parent.Name.NamespaceName != XslNamespace || parent.Name.LocalName != "iterate")
                throw new InvalidOperationException("XTSE0010: xsl:on-completion must be a child of xsl:iterate.");
        }

        foreach (var elem in root.DescendantsAndSelf())
        {
            if (!ShouldValidateElement(elem))
                continue;

            bool isXsltElement = elem.Name.NamespaceName == XslNamespace;
            var localName = elem.Name.LocalName;

            // XTSE0260: XSLT elements that must be empty must not contain text nodes
            // or element children; comments and processing instructions are allowed.
            if (isXsltElement && EmptyXsltElementNames.Contains(localName))
            {
                foreach (var node in elem.Nodes())
                {
                    if (node is XText || node is XElement)
                        throw new InvalidOperationException("XTSE0260");
                }
            }

            // XTSE0090: static variables and parameters must be declared at the top level.
            if (isXsltElement && localName is "param" or "variable")
            {
                var staticAttr = elem.Attribute("static")?.Value;
                if (!string.IsNullOrEmpty(staticAttr) && IsStaticYes(staticAttr.Trim()))
                {
                    var parent = elem.Parent;
                    bool isTopLevel = parent != null &&
                        parent.Name.NamespaceName == XslNamespace &&
                        parent.Name.LocalName is "transform" or "stylesheet" or "package";
                    if (!isTopLevel)
                        throw new InvalidOperationException("XTSE0090: A static variable or parameter must be declared at the top level of the stylesheet.");
                }
            }

            // XTSE0805 / XTSE0090: validate attributes in the XSLT namespace.
            foreach (var attr in elem.Attributes())
            {
                if (attr.Name.NamespaceName != XslNamespace)
                    continue;

                var attrLocal = attr.Name.LocalName;
                if (isXsltElement)
                {
                    // XSLT-namespaced attributes are not permitted on XSLT elements.
                    throw new InvalidOperationException("XTSE0090");
                }
                else
                {
                    // On literal result elements only the defined XSLT attributes are allowed.
                    var allowed = attrLocal is "use-when" or "expand-text" or "type" or "validation"
                        or "default-mode" or "default-collation" or "default-validation"
                        or "exclude-result-prefixes" or "extension-element-prefixes"
                        or "version" or "xpath-default-namespace"
                        or "use-attribute-sets"
                        or "inherit-namespaces";
                    if (!allowed && !IsForwardsCompatibleElement(elem))
                        throw new InvalidOperationException("XTSE0805");
                }
            }

            // XTSE0808: validate exclude-result-prefixes tokens (XSLT 2.0+).
            if (GetEffectiveVersion(elem) >= 2.0)
            {
                if (isXsltElement && localName is "stylesheet" or "transform")
                {
                    var erpAttr = elem.Attribute("exclude-result-prefixes");
                    if (erpAttr != null)
                        ValidateExcludeResultPrefixesValue(elem, erpAttr.Value, $"xsl:{localName}/@exclude-result-prefixes");
                }
                else if (!isXsltElement)
                {
                    var erpAttr = elem.Attribute(XName.Get("exclude-result-prefixes", XslNamespace))
                        ?? elem.Attribute("exclude-result-prefixes");
                    if (erpAttr != null)
                        ValidateExcludeResultPrefixesValue(elem, erpAttr.Value, $"literal result element @{erpAttr.Name}");
                }
            }

            // XTSE1430: validate extension-element-prefixes tokens are bound to namespaces.
            {
                XAttribute? eepAttr;
                string construct;
                if (isXsltElement)
                {
                    eepAttr = elem.Attribute("extension-element-prefixes");
                    construct = $"xsl:{localName}/@extension-element-prefixes";
                }
                else
                {
                    eepAttr = elem.Attribute(XName.Get("extension-element-prefixes", XslNamespace))
                        ?? elem.Attribute("extension-element-prefixes");
                    construct = "literal result element @xsl:extension-element-prefixes";
                }

                if (eepAttr != null && !string.IsNullOrWhiteSpace(eepAttr.Value))
                {
                    foreach (var token in eepAttr.Value.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var prefix = token.Trim();
                        if (prefix == "#default")
                        {
                            if (string.IsNullOrEmpty(elem.GetDefaultNamespace().NamespaceName))
                                throw new InvalidOperationException($"XTSE1430: #default in {construct} is not bound to a default namespace.");
                        }
                        else
                        {
                            var ns = elem.GetNamespaceOfPrefix(prefix);
                            if (ns == null || string.IsNullOrEmpty(ns.NamespaceName))
                                throw new InvalidOperationException($"XTSE1430: Prefix '{prefix}' in {construct} is not bound to a namespace.");
                        }
                    }
                }
            }

            // XTSE0125: default-collation must resolve to a list containing at least
            // one collation URI recognized by this implementation (XSLT 2.0+).
            if (GetEffectiveVersion(elem) >= 2.0)
            {
                XAttribute? dcAttr;
                string construct;
                if (isXsltElement)
                {
                    dcAttr = elem.Attribute("default-collation");
                    construct = $"xsl:{localName}/@default-collation";
                }
                else
                {
                    dcAttr = elem.Attribute(XName.Get("default-collation", XslNamespace));
                    construct = "literal result element @xsl:default-collation";
                }

                if (dcAttr != null && !string.IsNullOrWhiteSpace(dcAttr.Value))
                {
                    var baseUri = GetEffectiveBaseUri(dcAttr.Parent ?? elem);
                    var tokens = dcAttr.Value.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    bool hasRecognized = false;
                    foreach (var token in tokens)
                    {
                        if (IsSupportedCollationUri(token, baseUri))
                        {
                            hasRecognized = true;
                            break;
                        }
                    }
                    if (!hasRecognized)
                        throw new InvalidOperationException($"XTSE0125: The value of {construct} contains no recognized collation URI.");
                }
            }

            // XTSE0020: validate expand-text attribute values. The attribute may
            // appear as no-namespace expand-text on XSLT elements or as
            // xsl:expand-text on literal result elements; it must not be an AVT.
            foreach (var attr in elem.Attributes())
            {
                if (attr.Name.LocalName != "expand-text")
                    continue;
                if (attr.IsNamespaceDeclaration)
                    continue;
                var val = attr.Value;
                if (IsAvtValue(val) || !IsYesNoValue(val))
                    throw new InvalidOperationException("XTSE0020");
            }

            // XTSE0010 / XTSE0090: xsl:element does not allow a select attribute.
            if (isXsltElement && localName == "element" && elem.Attribute("select") != null)
                throw new InvalidOperationException("XTSE0090");

            if (isXsltElement && localName == "element")
            {
                var allowedElementAttributes = new HashSet<string>(StringComparer.Ordinal)
                {
                    "name", "namespace", "inherit-namespaces", "use-attribute-sets",
                    "type", "validation", "select", "use-when"
                };
                foreach (var attr in elem.Attributes())
                {
                    if (attr.IsNamespaceDeclaration)
                        continue;
                    if (!string.IsNullOrEmpty(attr.Name.NamespaceName))
                        continue;
                    var baseName = attr.Name.LocalName;
                    if (baseName.StartsWith("_"))
                        baseName = baseName.Substring(1);
                    if (!allowedElementAttributes.Contains(baseName))
                        throw new InvalidOperationException($"XTSE0090: Attribute '{attr.Name.LocalName}' is not permitted on xsl:element.");
                }

                // XTSE0710: xsl:element/@use-attribute-sets must reference existing attribute sets (XSLT 2.0+).
                // Only validated on the root stylesheet, where all imports/includes are known.
                var elementUseAttrSets = elem.Attribute("use-attribute-sets");
                if (elementUseAttrSets != null && this == _rootStylesheet && GetEffectiveVersion(elem) >= 2.0)
                    ValidateUseAttributeSetsValue(elem, elementUseAttrSets.Value, "xsl:element/@use-attribute-sets");
            }

            // XTSE0840: xsl:attribute/@select is allowed only when the element has empty content.
            if (isXsltElement && localName == "attribute" && elem.Attribute("select") != null)
            {
                bool hasContent = false;
                foreach (var node in elem.Nodes())
                {
                    if (node is XElement || (node is XText text && !string.IsNullOrWhiteSpace(text.Value)))
                    {
                        hasContent = true;
                        break;
                    }
                }
                if (hasContent)
                    throw new InvalidOperationException("XTSE0840: xsl:attribute must have empty content when the select attribute is present.");
            }

            // XTSE0870: xsl:value-of/@select must be present iff the element has empty content.
            if (isXsltElement && localName == "value-of")
            {
                bool hasSelect = elem.Attribute("select") != null;
                bool hasContent = false;
                foreach (var node in elem.Nodes())
                {
                    if (node is XElement || (node is XText text && !string.IsNullOrWhiteSpace(text.Value)))
                    {
                        hasContent = true;
                        break;
                    }
                }
                if (hasSelect && hasContent)
                    throw new InvalidOperationException("XTSE0870: xsl:value-of must have empty content when the select attribute is present.");

                // XTSE0020: disable-output-escaping must be a valid yes/no value when not an AVT.
                var doeAttr = elem.Attribute("disable-output-escaping");
                if (doeAttr != null && !IsAvtValue(doeAttr.Value) && !IsYesNoValue(doeAttr.Value))
                    throw new InvalidOperationException("XTSE0020: invalid value for disable-output-escaping attribute.");
            }

            // XTSE0020: xsl:text/@disable-output-escaping must be a valid yes/no value.
            if (isXsltElement && localName == "text")
            {
                var doeAttr = elem.Attribute("disable-output-escaping");
                if (doeAttr != null && !IsAvtValue(doeAttr.Value) && !IsYesNoValue(doeAttr.Value))
                    throw new InvalidOperationException("XTSE0020: invalid value for disable-output-escaping attribute.");
            }

            // XTSE0880: xsl:processing-instruction/@select is allowed only when the element has empty content.
            if (isXsltElement && localName == "processing-instruction" && elem.Attribute("select") != null)
            {
                bool hasContent = false;
                foreach (var node in elem.Nodes())
                {
                    if (node is XElement || (node is XText text && !string.IsNullOrWhiteSpace(text.Value)))
                    {
                        hasContent = true;
                        break;
                    }
                }
                if (hasContent)
                    throw new InvalidOperationException("XTSE0880: xsl:processing-instruction must have empty content when the select attribute is present.");
            }

            // XTSE0910: xsl:namespace/@select is allowed only with empty content or xsl:fallback children;
            // if select is absent, the element must have non-empty content.
            if (isXsltElement && localName == "namespace")
            {
                bool hasSelect = elem.Attribute("select") != null;
                bool hasNonFallbackContent = false;
                foreach (var node in elem.Nodes())
                {
                    if (node is XElement xelem && xelem.Name.NamespaceName == XslNamespace && xelem.Name.LocalName == "fallback")
                        continue;
                    if (node is XElement || (node is XText text && !string.IsNullOrWhiteSpace(text.Value)))
                    {
                        hasNonFallbackContent = true;
                        break;
                    }
                }

                if (hasSelect && hasNonFallbackContent)
                    throw new InvalidOperationException("XTSE0910: xsl:namespace must have empty content or only xsl:fallback children when the select attribute is present.");
                if (!hasSelect && !hasNonFallbackContent)
                    throw new InvalidOperationException("XTSE0910: xsl:namespace must have a select attribute when its content is empty.");
            }

            // XTSE0940: xsl:comment/@select is allowed only when the element has empty content.
            if (isXsltElement && localName == "comment" && elem.Attribute("select") != null)
            {
                bool hasContent = false;
                foreach (var node in elem.Nodes())
                {
                    if (node is XElement || (node is XText text && !string.IsNullOrWhiteSpace(text.Value)))
                    {
                        hasContent = true;
                        break;
                    }
                }
                if (hasContent)
                    throw new InvalidOperationException("XTSE0940: xsl:comment must have empty content when the select attribute is present.");
            }

            // XTSE1015: xsl:sort/@select is allowed only when the element has empty content.
            if (isXsltElement && localName == "sort" && elem.Attribute("select") != null)
            {
                bool hasContent = false;
                foreach (var node in elem.Nodes())
                {
                    if (node is XElement || (node is XText text && !string.IsNullOrWhiteSpace(text.Value)))
                    {
                        hasContent = true;
                        break;
                    }
                }
                if (hasContent)
                    throw new InvalidOperationException("XTSE1015: xsl:sort must have empty content when the select attribute is present.");
            }

            // XTSE1040: xsl:perform-sort/@select may only have xsl:sort and xsl:fallback content.
            if (isXsltElement && localName == "perform-sort" && elem.Attribute("select") != null)
            {
                foreach (var node in elem.Nodes())
                {
                    if (node is XElement child)
                    {
                        if (child.Name.NamespaceName == XslNamespace &&
                            child.Name.LocalName is "sort" or "fallback")
                        {
                            continue;
                        }
                        throw new InvalidOperationException("XTSE1040: xsl:perform-sort with a select attribute may only contain xsl:sort and xsl:fallback instructions.");
                    }
                    else if (node is XText text && !string.IsNullOrWhiteSpace(text.Value))
                    {
                        throw new InvalidOperationException("XTSE1040: xsl:perform-sort with a select attribute may only contain xsl:sort and xsl:fallback instructions.");
                    }
                }
            }

            // XTSE3140: xsl:try/@select may only contain xsl:catch and xsl:fallback instructions.
            if (isXsltElement && localName == "try" && elem.Attribute("select") != null)
            {
                foreach (var node in elem.Nodes())
                {
                    if (node is XElement child)
                    {
                        if (child.Name.NamespaceName == XslNamespace &&
                            child.Name.LocalName is "catch" or "fallback")
                        {
                            continue;
                        }
                        throw new InvalidOperationException("XTSE3140: xsl:try with a select attribute may only contain xsl:catch and xsl:fallback instructions.");
                    }
                    else if (node is XText text && !string.IsNullOrWhiteSpace(text.Value))
                    {
                        throw new InvalidOperationException("XTSE3140: xsl:try with a select attribute may only contain xsl:catch and xsl:fallback instructions.");
                    }
                }
            }

            // XTSE3150: xsl:catch/@select is allowed only when the element has empty content.
            if (isXsltElement && localName == "catch" && elem.Attribute("select") != null)
            {
                bool hasContent = false;
                foreach (var node in elem.Nodes())
                {
                    if (node is XElement || (node is XText text && !string.IsNullOrWhiteSpace(text.Value)))
                    {
                        hasContent = true;
                        break;
                    }
                }
                if (hasContent)
                    throw new InvalidOperationException("XTSE3150: xsl:catch must have empty content when the select attribute is present.");
            }

            // XTSE0760: xsl:param inside xsl:function must be empty and must not have a select attribute.
            if (isXsltElement && localName == "param")
            {
                var parent = elem.Parent;
                if (parent != null && parent.Name.NamespaceName == XslNamespace && parent.Name.LocalName == "function")
                {
                    if (elem.Attribute("select") != null)
                        throw new InvalidOperationException("XTSE0760: xsl:param inside xsl:function must not have a select attribute.");
                    foreach (var node in elem.Nodes())
                    {
                        if (node is XElement || (node is XText text && !string.IsNullOrWhiteSpace(text.Value)))
                            throw new InvalidOperationException("XTSE0760: xsl:param inside xsl:function must be empty.");
                    }
                }
            }

            // XTSE0010 / XTSE0090 / XTSE0020: validate xsl:variable, xsl:param and xsl:with-param.
            if (isXsltElement && localName is "variable" or "param" or "with-param")
            {
                var nameAttr = elem.Attribute("name") ?? elem.Attribute("_name");
                if (nameAttr == null || string.IsNullOrWhiteSpace(nameAttr.Value))
                    throw new InvalidOperationException($"XTSE0010: Missing name attribute on xsl:{localName}.");

                var allowedAttributes = localName switch
                {
                    "variable" => new HashSet<string>(StringComparer.Ordinal)
                    {
                        "name", "select", "as", "static", "use-when", "visibility", "version", "expand-text"
                    },
                    "param" => new HashSet<string>(StringComparer.Ordinal)
                    {
                        "name", "select", "as", "required", "tunnel", "static", "use-when", "version", "expand-text"
                    },
                    "with-param" => new HashSet<string>(StringComparer.Ordinal)
                    {
                        "name", "select", "as", "tunnel", "use-when"
                    },
                    _ => new HashSet<string>(StringComparer.Ordinal)
                };

                // In forwards-compatible mode unknown attributes on XSLT elements are ignored.
                if (!IsForwardsCompatible)
                {
                    foreach (var attr in elem.Attributes())
                    {
                        if (attr.IsNamespaceDeclaration)
                            continue;
                        if (!string.IsNullOrEmpty(attr.Name.NamespaceName))
                            continue;

                        var baseName = attr.Name.LocalName;
                        if (baseName.StartsWith("_"))
                            baseName = baseName.Substring(1);

                        if (!allowedAttributes.Contains(baseName))
                            throw new InvalidOperationException($"XTSE0090: Attribute '{attr.Name.LocalName}' is not permitted on xsl:{localName}.");
                    }
                }

                // XTSE0620: a variable-binding element must not have both a select attribute
                // and non-empty content (text nodes, element children, etc.).
                if ((elem.Attribute("select") != null || elem.Attribute("_select") != null) && !IsStaticBodyEmpty(elem))
                    throw new InvalidOperationException($"XTSE0620: xsl:{localName} must not have both a select attribute and content.");

                if (localName == "param")
                {
                    if (elem.Attribute("visibility") != null || elem.Attribute("_visibility") != null)
                        throw new InvalidOperationException("XTSE0090: visibility is not permitted on xsl:param.");

                    var requiredAttr = elem.Attribute("required") ?? elem.Attribute("_required");
                    if (requiredAttr != null)
                    {
                        var reqVal = requiredAttr.Value.Trim();
                        if (reqVal != "yes" && reqVal != "no" &&
                            reqVal != "true" && reqVal != "false" &&
                            reqVal != "1" && reqVal != "0")
                        {
                            throw new InvalidOperationException("XTSE0020: Invalid value for required attribute on xsl:param.");
                        }

                        if (reqVal == "yes" && (elem.Attribute("select") != null || elem.Attribute("_select") != null))
                            throw new InvalidOperationException("XTSE0010: A required xsl:param must not have a select attribute.");

                        if (reqVal == "yes" && !IsStaticBodyEmpty(elem))
                            throw new InvalidOperationException("XTSE0010: A required xsl:param must be empty.");
                    }
                }

                if (localName == "with-param")
                {
                    if (elem.Attribute("required") != null || elem.Attribute("_required") != null)
                        throw new InvalidOperationException("XTSE0090: required is not permitted on xsl:with-param.");
                }
            }

            // XTSE0580: duplicate parameter names within a template or function
            if (localName is "template" or "function")
            {
                var seenParams = new HashSet<string>();
                foreach (var param in elem.Elements(XName.Get("param", XslNamespace)))
                {
                    var paramName = param.Attribute("name")?.Value;
                    if (string.IsNullOrEmpty(paramName))
                        continue;
                    var (pLocal, pNs) = ExpandVariableName(param, paramName);
                    var key = string.IsNullOrEmpty(pNs) ? pLocal : $"{{{pNs}}}{pLocal}";
                    if (!seenParams.Add(key))
                        throw new InvalidOperationException($"XTSE0580: Duplicate parameter name '{paramName}'.");
                }
            }

            // XTSE0680: xsl:call-template with a parameter not declared by the named template.
            // This is only an error in XSLT 2.0 and later; XSLT 1.0 backwards-compatible
            // stylesheets silently ignore unknown parameters.
            if (localName == "call-template" && !IsVersion10(root))
            {
                var calledName = elem.Attribute("name")?.Value;
                if (!string.IsNullOrEmpty(calledName))
                {
                    var allNamed = _rootStylesheet.GetAllNamedTemplates();
                    if (allNamed.TryGetValue(calledName, out var rule))
                    {
                        var declaredParams = new HashSet<string>();
                        foreach (var param in rule.Element.Elements(XName.Get("param", XslNamespace)))
                        {
                            var paramName = param.Attribute("name")?.Value;
                            if (!string.IsNullOrEmpty(paramName))
                            {
                                var (pLocal, pNs) = ExpandVariableName(param, paramName);
                                var key = string.IsNullOrEmpty(pNs) ? pLocal : $"{{{pNs}}}{pLocal}";
                                declaredParams.Add(key);
                            }
                        }
                        foreach (var wp in elem.Elements(XName.Get("with-param", XslNamespace)))
                        {
                            if (wp.Attribute("tunnel")?.Value == "yes")
                                continue;
                            var wpName = wp.Attribute("name")?.Value;
                            if (string.IsNullOrEmpty(wpName))
                                continue;
                            var (wpLocal, wpNs) = ExpandVariableName(wp, wpName);
                            var wpKey = string.IsNullOrEmpty(wpNs) ? wpLocal : $"{{{wpNs}}}{wpLocal}";
                            if (!declaredParams.Contains(wpKey))
                                throw new InvalidOperationException($"XTSE0680: Parameter '{wpName}' is not declared by template '{calledName}'.");
                        }
                    }
                }
            }

            // xsl:copy-of attribute validation
            if (localName == "copy-of")
            {
                // XTSE0090: xsl:copy-of does not allow invalid attributes
                foreach (var attr in elem.Attributes())
                {
                    var attrName = attr.Name.LocalName;
                    // Strip leading underscore for AVT forms (e.g. _copy-namespaces)
                    var baseName = attrName.StartsWith("_") ? attrName.Substring(1) : attrName;
                    if (attr.Name.NamespaceName == "" &&
                        baseName != "select" &&
                        baseName != "copy-accumulators" &&
                        baseName != "copy-namespaces" &&
                        baseName != "type" &&
                        baseName != "validation")
                    {
                        throw new InvalidOperationException("XTSE0090");
                    }

                    // XTSE0020: validate copy-namespaces value if it's a literal (not AVT)
                    if (baseName == "copy-namespaces" && !attrName.StartsWith("_"))
                    {
                        var val = attr.Value.Trim();
                        if (val != "yes" && val != "no" && val != "true" && val != "false" && val != "1" && val != "0")
                        {
                            throw new InvalidOperationException("XTSE0020");
                        }
                    }
                }
            }

            // XTSE0010: xsl:on-empty must be the last significant child of its sequence constructor
            if (localName == "on-empty")
            {
                var parent = elem.Parent;
                if (parent != null)
                {
                    bool hasSignificantFollowing = false;
                    bool seen = false;
                    foreach (var node in parent.Nodes())
                    {
                        if (!seen)
                        {
                            if (node == elem) seen = true;
                            continue;
                        }
                        if (node is XElement || node is XComment || node is XProcessingInstruction)
                        {
                            hasSignificantFollowing = true;
                            break;
                        }
                        if (node is XText text && !string.IsNullOrWhiteSpace(text.Value))
                        {
                            hasSignificantFollowing = true;
                            break;
                        }
                    }
                    if (hasSignificantFollowing)
                        throw new InvalidOperationException("XTSE0010: xsl:on-empty must be the last child of its sequence constructor");
                }
            }

            // xsl:copy attribute validation
            if (localName == "copy")
            {
                // XTSE0090: xsl:copy does not allow invalid attributes
                foreach (var attr in elem.Attributes())
                {
                    var attrName = attr.Name.LocalName;
                    var baseName = attrName.StartsWith("_") ? attrName.Substring(1) : attrName;
                    if (attr.Name.NamespaceName == "" &&
                        baseName != "select" &&
                        baseName != "copy-namespaces" &&
                        baseName != "inherit-namespaces" &&
                        baseName != "use-attribute-sets" &&
                        baseName != "type" &&
                        baseName != "validation")
                    {
                        throw new InvalidOperationException("XTSE0090");
                    }

                    // XTSE0020: validate copy-namespaces value if it's a literal (not AVT)
                    if (baseName == "copy-namespaces" && !attrName.StartsWith("_"))
                    {
                        var val = attr.Value.Trim();
                        if (val != "yes" && val != "no" && val != "true" && val != "false" && val != "1" && val != "0")
                        {
                            throw new InvalidOperationException("XTSE0020");
                        }
                    }
                }

                // XTSE0710: xsl:copy/@use-attribute-sets must reference existing attribute sets (XSLT 2.0+).
                // Only validated on the root stylesheet, where all imports/includes are known.
                var copyUseAttrSets = elem.Attribute("use-attribute-sets");
                if (copyUseAttrSets != null && this == _rootStylesheet && GetEffectiveVersion(elem) >= 2.0)
                    ValidateUseAttributeSetsValue(elem, copyUseAttrSets.Value, "xsl:copy/@use-attribute-sets");
            }

            // xsl:function static validation
            if (localName == "function")
            {
                foreach (var attr in elem.Attributes())
                {
                    if (attr.Name.NamespaceName != "")
                        continue;
                    var attrName = attr.Name.LocalName;
                    var baseName = attrName.StartsWith("_") ? attrName.Substring(1) : attrName;
                    if (baseName != "name" &&
                        baseName != "as" &&
                        baseName != "visibility" &&
                        baseName != "override" &&
                        baseName != "override-extension-function" &&
                        baseName != "new-each-time" &&
                        baseName != "cache" &&
                        baseName != "identity-sensitive" &&
                        baseName != "expand-text" &&
                        baseName != "streamability")
                    {
                        throw new InvalidOperationException("XTSE0090");
                    }

                    if (!attrName.StartsWith("_"))
                    {
                        if (baseName == "override" || baseName == "override-extension-function" ||
                            baseName == "cache" || baseName == "identity-sensitive" || baseName == "expand-text")
                        {
                            if (!IsYesNoValue(attr.Value))
                                throw new InvalidOperationException("XTSE0020");
                        }
                        else if (baseName == "new-each-time")
                        {
                            if (!IsNewEachTimeValue(attr.Value))
                                throw new InvalidOperationException("XTSE0020");
                        }
                    }
                }

                var overrideAttr = elem.Attribute("override");
                var overrideExtAttr = elem.Attribute("override-extension-function");
                if (overrideAttr != null && overrideExtAttr != null)
                {
                    var o1 = TryParseYesNo(overrideAttr.Value);
                    var o2 = TryParseYesNo(overrideExtAttr.Value);
                    if (o1.HasValue && o2.HasValue && o1.Value != o2.Value)
                        throw new InvalidOperationException("XTSE0020");
                }

                // xsl:param children of xsl:function may have @required='yes' but not @required='no'
                foreach (var param in elem.Elements(XName.Get("param", XslNamespace)))
                {
                    var requiredVal = param.Attribute("required")?.Value.Trim();
                    if (requiredVal == "no")
                        throw new InvalidOperationException("XTSE0020");
                }
            }

            // xsl:message static validation
            if (localName == "message")
            {
                foreach (var attr in elem.Attributes())
                {
                    if (attr.Name.NamespaceName != "")
                        continue;
                    var attrName = attr.Name.LocalName;
                    var baseName = attrName.StartsWith("_") ? attrName.Substring(1) : attrName;
                    if (baseName != "select" &&
                        baseName != "terminate" &&
                        baseName != "error-code" &&
                        baseName != "expand-text" &&
                        baseName != "use-when")
                    {
                        throw new InvalidOperationException("XTSE0090");
                    }

                    // XTSE0020: literal terminate value must be a valid boolean/yes-no.
                    // Attribute value templates (containing unescaped '{') are evaluated at
                    // runtime and are not validated here.
                    if (baseName == "terminate" && !attrName.StartsWith("_"))
                    {
                        var val = attr.Value;
                        if (!IsAvtValue(val) && !IsYesNoValue(val))
                            throw new InvalidOperationException("XTSE0020");
                    }
                }
            }

            // xsl:sequence does not allow @as
            if (localName == "sequence" && elem.Attribute("as") != null)
                throw new InvalidOperationException("XTSE0090");

            // xsl:import-schema is only supported by schema-aware processors
            if (localName == "import-schema")
                throw new InvalidOperationException("XTSE1650: xsl:import-schema requires a schema-aware processor");

            // XTSE0090: package-version is only permitted on xsl:package
            if ((localName == "stylesheet" || localName == "transform") && elem.Attribute("package-version") != null && !IsForwardsCompatibleElement(elem))
                throw new InvalidOperationException("XTSE0090");

            // XTSE1660: non-schema-aware processors do not support validation/type attributes
            // that require schema awareness. Only strict requires a schema-aware processor;
            // lax is permitted (it behaves like skip on a basic processor).
            var validationAttr = elem.Attribute("validation") ?? elem.Attribute(XName.Get("validation", XslNamespace));
            if (validationAttr != null)
            {
                var val = validationAttr.Value.Trim();
                if (val == "strict")
                    throw new InvalidOperationException("XTSE1660");
            }
            if (localName is "stylesheet" or "transform" or "package")
            {
                var defaultValidationAttr = elem.Attribute("default-validation") ?? elem.Attribute(XName.Get("default-validation", XslNamespace));
                if (defaultValidationAttr != null)
                {
                    var val = defaultValidationAttr.Value.Trim();
                    if (val == "strict")
                        throw new InvalidOperationException("XTSE1660");
                }
            }
            var typeAttr = elem.Attribute("type") ?? elem.Attribute(XName.Get("type", XslNamespace));
            if (isXsltElement && typeAttr != null && localName != "merge-source")
                throw new InvalidOperationException("XTSE1660");
            if (!isXsltElement && elem.Attribute(XName.Get("type", XslNamespace)) != null)
                throw new InvalidOperationException("XTSE1660");

            // xsl:merge validation
            if (localName == "merge")
            {
                var mergeSources = elem.Elements(XName.Get("merge-source", XslNamespace)).ToList();
                var mergeActions = elem.Elements(XName.Get("merge-action", XslNamespace)).ToList();

                // XTSE0010: at least one merge-source and exactly one merge-action
                if (mergeSources.Count == 0)
                    throw new InvalidOperationException("XTSE0010: xsl:merge must contain at least one xsl:merge-source");
                if (mergeActions.Count != 1)
                    throw new InvalidOperationException("XTSE0010: xsl:merge must contain exactly one xsl:merge-action");

                // XTSE0090: xsl:merge allows only use-when (and xml:*)
                foreach (var attr in elem.Attributes())
                {
                    if (attr.Name.NamespaceName == "" &&
                        attr.Name.LocalName != "use-when" &&
                        attr.Name.LocalName != "_use-when")
                    {
                        throw new InvalidOperationException("XTSE0090");
                    }
                }

                // All merge-sources must specify the same number of merge keys
                int? keyCount = null;
                foreach (var source in mergeSources)
                {
                    var count = source.Elements(XName.Get("merge-key", XslNamespace)).Count();
                    if (keyCount == null)
                        keyCount = count;
                    else if (keyCount != count)
                        throw new InvalidOperationException("XTSE2200: all xsl:merge-source elements must have the same number of xsl:merge-key children");
                }

                // XTSE3190: sibling xsl:merge-source elements must have distinct names.
                // Sources without an explicit name receive unique default names so that
                // ordinary multi-source merges are not rejected.
                var mergeSourceNames = new HashSet<string>();
                int defaultNameIndex = 0;
                foreach (var source in mergeSources)
                {
                    var name = source.Attribute("name")?.Value;
                    if (string.IsNullOrEmpty(name))
                        name = $"~default{defaultNameIndex++}";
                    if (!mergeSourceNames.Add(name))
                        throw new InvalidOperationException($"XTSE3190: duplicate xsl:merge-source name '{name}'.");
                }

                // Validate child order: merge-source*, merge-action, fallback*
                bool actionSeen = false;
                foreach (var child in elem.Elements())
                {
                    if (child.Name.NamespaceName != XslNamespace)
                        throw new InvalidOperationException("XTSE0010: xsl:merge may only contain XSLT namespace children");

                    var childName = child.Name.LocalName;
                    if (childName == "merge-source")
                    {
                        if (actionSeen)
                            throw new InvalidOperationException("XTSE0010: xsl:merge-source must appear before xsl:merge-action");
                    }
                    else if (childName == "merge-action")
                    {
                        actionSeen = true;
                    }
                    else if (childName == "fallback")
                    {
                        if (!actionSeen)
                            throw new InvalidOperationException("XTSE0010: xsl:fallback must follow xsl:merge-action");
                    }
                    else
                    {
                        throw new InvalidOperationException("XTSE0010: xsl:merge may only contain xsl:merge-source, xsl:merge-action, and xsl:fallback children");
                    }
                }
            }

            // xsl:merge-source validation
            if (localName == "merge-source")
            {
                // Must be child of xsl:merge
                if (elem.Parent?.Name.LocalName != "merge" || elem.Parent?.Name.NamespaceName != XslNamespace)
                    throw new InvalidOperationException("XTSE0010: xsl:merge-source must be a child of xsl:merge");

                var hasSelect = elem.Attribute("select") != null || elem.Attribute("_select") != null;
                var hasForEachItem = elem.Attribute("for-each-item") != null || elem.Attribute("_for-each-item") != null;
                var hasForEachSource = elem.Attribute("for-each-source") != null || elem.Attribute("_for-each-source") != null;

                // XTSE3195: @for-each-item and @for-each-source are mutually exclusive
                if (hasForEachItem && hasForEachSource)
                    throw new InvalidOperationException("XTSE3195: xsl:merge-source cannot have both for-each-item and for-each-source");

                // Must have @select or @for-each-item or @for-each-source
                if (!hasSelect && !hasForEachItem && !hasForEachSource)
                    throw new InvalidOperationException("XTSE0010: xsl:merge-source must have a select, for-each-item, or for-each-source attribute");

                // Must contain at least one xsl:merge-key
                var mergeKeys = elem.Elements(XName.Get("merge-key", XslNamespace)).ToList();
                if (mergeKeys.Count == 0)
                    throw new InvalidOperationException("XTSE0010: xsl:merge-source must contain at least one xsl:merge-key");

                // XTSE0010: only xsl:merge-key children are permitted (whitespace text is ignored)
                foreach (var child in elem.Elements())
                {
                    if (child.Name.NamespaceName != XslNamespace || child.Name.LocalName != "merge-key")
                        throw new InvalidOperationException("XTSE0010: xsl:merge-source may only contain xsl:merge-key children");
                }

                // XTSE0090 / XTSE0020: attribute validation
                var hasValidation = false;
                var hasType = false;
                string? validationValue = null;
                foreach (var attr in elem.Attributes())
                {
                    if (attr.Name.NamespaceName != "")
                        continue;
                    var attrName = attr.Name.LocalName;
                    var baseName = attrName.StartsWith("_") ? attrName.Substring(1) : attrName;
                    if (!IsMergeSourceAttribute(baseName))
                        throw new InvalidOperationException("XTSE0090");

                    if (!attrName.StartsWith("_"))
                    {
                        if (baseName == "streamable")
                        {
                            if (!IsYesNoValue(attr.Value))
                                throw new InvalidOperationException("XTSE0020: invalid value for streamable");
                        }
                        else if (baseName == "sort-before-merge")
                        {
                            if (!IsYesNoValue(attr.Value))
                                throw new InvalidOperationException("XTSE0020: invalid value for sort-before-merge");
                        }
                        else if (baseName == "validation")
                        {
                            hasValidation = true;
                            validationValue = attr.Value.Trim();
                            if (validationValue is not "strict" and not "lax" and not "preserve" and not "strip")
                                throw new InvalidOperationException("XTSE0020: invalid value for validation");
                        }
                        else if (baseName == "type")
                        {
                            hasType = true;
                        }
                    }
                }

                // XTSE1505: validation and type are mutually exclusive
                if (hasValidation && hasType)
                    throw new InvalidOperationException("XTSE1505: xsl:merge-source cannot have both validation and type attributes");

                // XTSE1660: non-schema-aware processors do not support the type attribute
                if (hasType)
                    throw new InvalidOperationException("XTSE1660: xsl:merge-source/@type requires a schema-aware processor");

                // Validate @name is a valid NCName/EQName if present
                var nameAttr = elem.Attribute("name")?.Value;
                if (!string.IsNullOrEmpty(nameAttr))
                {
                    if (!IsValidMergeSourceName(nameAttr))
                        throw new InvalidOperationException("XTSE0020: invalid xsl:merge-source name");
                }
            }

            // xsl:merge-key validation
            if (localName == "merge-key")
            {
                // Must be child of xsl:merge-source
                if (elem.Parent?.Name.LocalName != "merge-source" || elem.Parent?.Name.NamespaceName != XslNamespace)
                    throw new InvalidOperationException("XTSE0010: xsl:merge-key must be a child of xsl:merge-source");

                // XTSE0090: disallowed attributes
                foreach (var attr in elem.Attributes())
                {
                    if (attr.Name.NamespaceName != "")
                        continue;
                    var attrName = attr.Name.LocalName;
                    var baseName = attrName.StartsWith("_") ? attrName.Substring(1) : attrName;
                    if (baseName != "select" &&
                        baseName != "order" &&
                        baseName != "data-type" &&
                        baseName != "lang" &&
                        baseName != "case-order" &&
                        baseName != "collation" &&
                        baseName != "use-when")
                    {
                        throw new InvalidOperationException("XTSE0090");
                    }
                }

                // XTSE3200: select attribute and sequence-constructor content are mutually exclusive
                if (elem.Attribute("select") != null || elem.Attribute("_select") != null)
                {
                    bool hasNonWhitespaceContent = false;
                    foreach (var node in elem.Nodes())
                    {
                        if (node is XElement)
                        {
                            hasNonWhitespaceContent = true;
                            break;
                        }
                        if (node is XText text && !string.IsNullOrWhiteSpace(text.Value))
                        {
                            hasNonWhitespaceContent = true;
                            break;
                        }
                    }
                    if (hasNonWhitespaceContent)
                        throw new InvalidOperationException("XTSE3200: xsl:merge-key cannot have both a select attribute and content");
                }
            }

            // xsl:merge-action validation
            if (localName == "merge-action")
            {
                // Must be child of xsl:merge
                if (elem.Parent?.Name.LocalName != "merge" || elem.Parent?.Name.NamespaceName != XslNamespace)
                    throw new InvalidOperationException("XTSE0010: xsl:merge-action must be a child of xsl:merge");

                // XTSE0090: xsl:merge-action does not allow attributes
                foreach (var attr in elem.Attributes())
                {
                    if (attr.Name.NamespaceName == "" &&
                        attr.Name.LocalName != "use-when" &&
                        attr.Name.LocalName != "_use-when")
                    {
                        throw new InvalidOperationException("XTSE0090");
                    }
                }
            }

            // XTSE0010: structural parent/child validation for XSLT elements.
            // These checks catch misplaced instructions, required attributes, and unknown
            // XSLT elements that are not permitted in a non-forwards-compatible stylesheet.
            if (isXsltElement)
            {
                var parent = elem.Parent;
                bool isTopLevel = parent != null &&
                    parent.Name.NamespaceName == XslNamespace &&
                    parent.Name.LocalName is "stylesheet" or "transform" or "package";

                // Unknown XSLT elements are normally a static error. They are ignored when the
                // stylesheet is in forwards-compatible mode, or when they appear at the top level
                // of an XSLT 3.0 stylesheet (where unrecognized elements are tolerated as vendor
                // extensions). In earlier XSLT versions an unrecognized top-level element is an error.
                if (!KnownXsltElementNames.Contains(localName))
                {
                    if (IsForwardsCompatibleElement(elem))
                    {
                        // Ignored in forwards-compatible mode.
                    }
                    else if (isTopLevel && GetEffectiveVersion(elem) >= 3.0)
                    {
                        // Ignored as a possible vendor extension at the top level of an XSLT 3.0 stylesheet.
                    }
                    else
                    {
                        throw new InvalidOperationException($"XTSE0010: Unknown XSLT element xsl:{localName}.");
                    }
                }

                // Top-level context checks.
                if (parent != null)
                {
                    if (isTopLevel)
                    {
                        if (!AllowedTopLevelDeclarations.Contains(localName) && !IsForwardsCompatibleElement(elem))
                            throw new InvalidOperationException($"XTSE0010: xsl:{localName} is not permitted at the top level.");
                    }
                    else if (TopLevelOnlyDeclarations.Contains(localName))
                    {
                        var parentName = parent.Name;
                        bool insideUsePackage = parentName.NamespaceName == XslNamespace && parentName.LocalName == "use-package";
                        bool insideOverride = parentName.NamespaceName == XslNamespace && parentName.LocalName == "override";
                        if (!insideUsePackage && !insideOverride)
                            throw new InvalidOperationException($"XTSE0010: xsl:{localName} must appear at the top level.");
                    }
                }

                // xsl:use-package requires a name attribute; resolution is handled during
                // BuildStaticContext so visibility/expose/accept/override can be applied.
                if (localName == "use-package")
                {
                    if (elem.Attribute("name") == null && elem.Attribute("_name") == null)
                        throw new InvalidOperationException("XTSE0010: xsl:use-package requires a name attribute.");
                }

                // xsl:if requires a test attribute.
                if (localName == "if" && elem.Attribute("test") == null && elem.Attribute("_test") == null)
                    throw new InvalidOperationException("XTSE0010: xsl:if requires a test attribute.");

                // xsl:call-template requires a name attribute.
                if (localName == "call-template" && elem.Attribute("name") == null && elem.Attribute("_name") == null)
                    throw new InvalidOperationException("XTSE0010: xsl:call-template requires a name attribute.");

                // xsl:attribute-set requires a name attribute and only xsl:attribute children.
                if (localName == "attribute-set")
                {
                    if (elem.Attribute("name") == null && elem.Attribute("_name") == null)
                        throw new InvalidOperationException("XTSE0010: xsl:attribute-set requires a name attribute.");

                    foreach (var node in elem.Nodes())
                    {
                        if (node is XText text && !string.IsNullOrWhiteSpace(text.Value))
                            throw new InvalidOperationException("XTSE0010: xsl:attribute-set may only contain xsl:attribute children.");
                    }

                    foreach (var child in elem.Elements())
                    {
                        if (child.Name.NamespaceName != XslNamespace || child.Name.LocalName != "attribute")
                            throw new InvalidOperationException("XTSE0010: xsl:attribute-set may only contain xsl:attribute children.");
                    }
                }

                // Allowed attributes on xsl:strip-space / xsl:preserve-space.
                if (localName is "strip-space" or "preserve-space")
                {
                    foreach (var attr in elem.Attributes())
                    {
                        if (attr.IsNamespaceDeclaration) continue;
                        var baseName = attr.Name.LocalName;
                        if (baseName.StartsWith("_")) baseName = baseName.Substring(1);
                        if (attr.Name.NamespaceName == "" && baseName is not "elements" and not "use-when" and not "version" and not "xpath-default-namespace")
                            throw new InvalidOperationException($"XTSE0090: Attribute '{attr.Name.LocalName}' is not permitted on xsl:{localName}.");
                    }
                }

                // Allowed attributes on xsl:include / xsl:import (also enforces @href presence).
                if (localName is "include" or "import")
                {
                    foreach (var attr in elem.Attributes())
                    {
                        if (attr.IsNamespaceDeclaration) continue;
                        var baseName = attr.Name.LocalName;
                        if (baseName.StartsWith("_")) baseName = baseName.Substring(1);
                        if (attr.Name.NamespaceName == "" && baseName is not "href" and not "use-when")
                            throw new InvalidOperationException($"XTSE0090: Attribute '{attr.Name.LocalName}' is not permitted on xsl:{localName}.");
                    }
                }

                // XTSE0090: validate allowed attributes on XSLT elements that do not yet
                // have element-specific attribute whitelists elsewhere in this method.
                if (localName is "stylesheet" or "transform")
                {
                    ValidateAllowedAttributes(elem, localName, AllowedXsltAttributes(
                        "id", "default-mode", "default-validation",
                        "input-type-annotations", "extension-element-prefixes", "exclude-result-prefixes"),
                        IsForwardsCompatibleElement(elem));
                }
                else if (localName == "template")
                {
                    ValidateAllowedAttributes(elem, localName, AllowedXsltAttributes(
                        "match", "name", "mode", "priority", "as", "visibility", "streamable"),
                        IsForwardsCompatibleElement(elem));
                }
                else if (localName == "apply-templates")
                {
                    ValidateAllowedAttributes(elem, localName, AllowedXsltAttributes(
                        "select", "mode"),
                        IsForwardsCompatibleElement(elem));
                }
                else if (localName == "apply-imports")
                {
                    ValidateAllowedAttributes(elem, localName, AllowedXsltAttributes(),
                        IsForwardsCompatibleElement(elem));
                }
                else if (localName == "call-template")
                {
                    ValidateAllowedAttributes(elem, localName, AllowedXsltAttributes(
                        "name"),
                        IsForwardsCompatibleElement(elem));
                }
                else if (localName == "attribute-set")
                {
                    ValidateAllowedAttributes(elem, localName, AllowedXsltAttributes(
                        "name", "use-attribute-sets", "visibility", "streamable"),
                        IsForwardsCompatibleElement(elem));
                }
                else if (localName == "key")
                {
                    ValidateAllowedAttributes(elem, localName, AllowedXsltAttributes(
                        "name", "match", "use", "collation", "composite"),
                        IsForwardsCompatibleElement(elem));

                    // XTSE1205: xsl:key must have either @use or non-empty content, but not both.
                    bool hasUse = elem.Attribute("use") != null && !string.IsNullOrWhiteSpace(elem.Attribute("use")!.Value);
                    bool hasContent = elem.Nodes().Any(n => n is XElement || (n is XText t && !string.IsNullOrWhiteSpace(t.Value)));
                    if (hasUse && hasContent)
                        throw new InvalidOperationException("XTSE1205: xsl:key must not have both a use attribute and content.");
                    if (!hasUse && !hasContent)
                        throw new InvalidOperationException("XTSE1205: xsl:key must have either a use attribute or non-empty content.");

                    // XTSE1210: xsl:key/@collation must be a URI recognized by this implementation.
                    var collationAttr = elem.Attribute("collation");
                    if (collationAttr != null && !string.IsNullOrWhiteSpace(collationAttr.Value))
                    {
                        var baseUri = GetEffectiveBaseUri(elem);
                        if (!IsSupportedCollationUri(collationAttr.Value, baseUri))
                            throw new InvalidOperationException("XTSE1210: The collation URI specified on xsl:key is not recognized by this implementation.");
                    }
                }

                // XTSE0120: xsl:stylesheet / xsl:transform / xsl:package must not have
                // non-whitespace text node children.
                if (localName is "stylesheet" or "transform" or "package")
                {
                    foreach (var node in elem.Nodes())
                    {
                        if (node is XText text && !string.IsNullOrWhiteSpace(text.Value))
                            throw new InvalidOperationException("XTSE0120: xsl:stylesheet must not have text node children.");
                    }
                }

                // xsl:param parent and position validation.
                if (localName == "param")
                {
                    var paramParent = elem.Parent;
                    if (paramParent != null)
                    {
                        bool parentIsXslt = paramParent.Name.NamespaceName == XslNamespace;
                        var parentName = parentIsXslt ? paramParent.Name.LocalName : null;

                        // xsl:param is only permitted in a small set of parent elements.
                        if (!parentIsXslt || parentName is not "stylesheet" and not "transform" and not "package" and not "template" and not "function" and not "iterate" and not "override")
                        {
                            throw new InvalidOperationException("XTSE0010: xsl:param is not permitted in this context.");
                        }

                        if (parentName is "template" or "function")
                        {
                            foreach (var node in paramParent.Nodes())
                            {
                                if (node == elem) break;
                                if (node is XElement preceding)
                                {
                                    // xsl:context-item and other xsl:param elements are the only XSLT
                                    // elements that may precede this xsl:param in a template or function.
                                    if (preceding.Name.NamespaceName == XslNamespace &&
                                        (preceding.Name.LocalName == "param" || preceding.Name.LocalName == "context-item"))
                                        continue;

                                    throw new InvalidOperationException("XTSE0010: xsl:param must appear first inside xsl:template or xsl:function.");
                                }
                                if (node is XText text && !string.IsNullOrWhiteSpace(text.Value))
                                    throw new InvalidOperationException("XTSE0010: xsl:param must appear first inside xsl:template or xsl:function.");
                            }
                        }
                    }
                }

                // xsl:choose validation.
                if (localName == "choose")
                {
                    int whenCount = 0;
                    int otherwiseCount = 0;
                    bool otherwiseSeen = false;
                    foreach (var node in elem.Nodes())
                    {
                        if (node is XText text)
                        {
                            if (!string.IsNullOrWhiteSpace(text.Value))
                                throw new InvalidOperationException("XTSE0010: xsl:choose must not contain text outside xsl:when or xsl:otherwise.");
                            continue;
                        }

                        if (node is not XElement child) continue;
                        if (child.Name.NamespaceName != XslNamespace)
                            continue;

                        var childName = child.Name.LocalName;
                        if (childName == "when")
                        {
                            if (otherwiseSeen)
                                throw new InvalidOperationException("XTSE0010: xsl:when must precede xsl:otherwise.");
                            whenCount++;
                        }
                        else if (childName == "otherwise")
                        {
                            otherwiseCount++;
                            otherwiseSeen = true;
                        }
                        else
                        {
                            throw new InvalidOperationException($"XTSE0010: xsl:choose may not contain xsl:{childName}.");
                        }
                    }

                    if (whenCount == 0)
                        throw new InvalidOperationException("XTSE0010: xsl:choose must contain at least one xsl:when.");
                    if (otherwiseCount > 1)
                        throw new InvalidOperationException("XTSE0010: xsl:choose may contain at most one xsl:otherwise.");
                }

                // xsl:apply-templates may only contain xsl:sort and xsl:with-param.
                if (localName == "apply-templates")
                {
                    foreach (var node in elem.Nodes())
                    {
                        if (node is XText text && !string.IsNullOrWhiteSpace(text.Value))
                            throw new InvalidOperationException("XTSE0010: xsl:apply-templates may not contain text nodes.");
                    }

                    foreach (var child in elem.Elements())
                    {
                        if (child.Name.NamespaceName == XslNamespace &&
                            child.Name.LocalName is not "sort" and not "with-param")
                        {
                            throw new InvalidOperationException($"XTSE0010: xsl:apply-templates may not contain xsl:{child.Name.LocalName}.");
                        }
                    }

                    ValidateDuplicateWithParamNames(elem);
                }

                // xsl:apply-imports may only contain xsl:with-param.
                if (localName == "apply-imports")
                {
                    foreach (var node in elem.Nodes())
                    {
                        if (node is XText text && !string.IsNullOrWhiteSpace(text.Value))
                            throw new InvalidOperationException("XTSE0010: xsl:apply-imports may not contain text nodes.");
                    }

                    foreach (var child in elem.Elements())
                    {
                        if (child.Name.NamespaceName != XslNamespace || child.Name.LocalName != "with-param")
                            throw new InvalidOperationException($"XTSE0010: xsl:apply-imports may not contain xsl:{child.Name.LocalName}.");
                    }

                    ValidateDuplicateWithParamNames(elem);
                }

                // xsl:call-template may only contain xsl:with-param.
                if (localName == "call-template")
                {
                    foreach (var node in elem.Nodes())
                    {
                        if (node is XText text && !string.IsNullOrWhiteSpace(text.Value))
                            throw new InvalidOperationException("XTSE0010: xsl:call-template may not contain text nodes.");
                    }

                    foreach (var child in elem.Elements())
                    {
                        if (child.Name.NamespaceName != XslNamespace || child.Name.LocalName != "with-param")
                            throw new InvalidOperationException($"XTSE0010: xsl:call-template may only contain xsl:with-param children.");
                    }

                    ValidateDuplicateWithParamNames(elem);
                }

                // xsl:template children may not include xsl:sort or xsl:with-param.
                if (localName == "template")
                {
                    foreach (var child in elem.Elements())
                    {
                        if (child.Name.NamespaceName == XslNamespace &&
                            child.Name.LocalName is "sort" or "with-param")
                        {
                            throw new InvalidOperationException($"XTSE0010: xsl:{child.Name.LocalName} is not permitted as a child of xsl:template.");
                        }
                    }

                    // XTSE0500: xsl:template must have match or name; constraints on mode/priority/visibility.
                    // XTSE0550: mode attribute list validation.
                    var matchAttr = elem.Attribute("match") ?? elem.Attribute("_match");
                    var nameAttr = elem.Attribute("name") ?? elem.Attribute("_name");
                    var hasMatch = matchAttr != null && !string.IsNullOrWhiteSpace(matchAttr.Value);
                    var hasName = nameAttr != null && !string.IsNullOrWhiteSpace(nameAttr.Value);

                    if (!hasMatch && !hasName)
                        throw new InvalidOperationException("XTSE0500: xsl:template must have a match or name attribute.");

                    var modeAttr = elem.Attribute("mode") ?? elem.Attribute("_mode");
                    var priorityAttr = elem.Attribute("priority") ?? elem.Attribute("_priority");
                    var visibilityAttr = elem.Attribute("visibility") ?? elem.Attribute("_visibility");

                    if (!hasMatch && modeAttr != null)
                        throw new InvalidOperationException("XTSE0500: xsl:template with no match attribute must not have a mode attribute.");

                    if (!hasMatch && priorityAttr != null)
                        throw new InvalidOperationException("XTSE0500: xsl:template with no match attribute must not have a priority attribute.");

                    if (priorityAttr != null && !IsValidXsDecimal(priorityAttr.Value))
                        throw new InvalidOperationException($"XTSE0530: The priority attribute value '{priorityAttr.Value}' is not a valid xs:decimal.");

                    if (!hasName && visibilityAttr != null)
                        throw new InvalidOperationException("XTSE0500: xsl:template with no name attribute must not have a visibility attribute.");

                    if (modeAttr != null)
                        ValidateTemplateModeAttribute(elem, modeAttr.Value);
                }

                // XTSE0020: validate lexical names and disallowed attribute values.
                // Name attributes on declarations and instructions must be legal QNames/EQNames
                // and must not be attribute value templates.
                if (localName is "attribute-set" or "key" or "template" or "call-template" or "function" or "decimal-format")
                {
                    var nameAttr = elem.Attribute("name");
                    if (nameAttr != null && !string.IsNullOrWhiteSpace(nameAttr.Value))
                        ValidateXsltName(elem, nameAttr.Value, $"xsl:{localName}/@name");
                }

                // XTSE0340: validate match patterns on xsl:template and xsl:key, and
                // count/from patterns on xsl:number. Only literal (non-AVT) attributes are
                // validated here; AVT values are validated after expansion at compile time.
                if (localName is "template" or "key")
                {
                    var matchAttr = elem.Attribute("match");
                    if (matchAttr != null && !string.IsNullOrWhiteSpace(matchAttr.Value))
                    {
                        var strippedMatch = PatternCompiler.StripXPathComments(matchAttr.Value).Trim();
                        if (!string.IsNullOrEmpty(strippedMatch))
                            PatternCompiler.ValidatePatternSyntax(strippedMatch);
                    }
                }
                if (localName == "number")
                {
                    var countAttr = elem.Attribute("count");
                    if (countAttr != null && !string.IsNullOrWhiteSpace(countAttr.Value))
                    {
                        var strippedCount = PatternCompiler.StripXPathComments(countAttr.Value).Trim();
                        if (!string.IsNullOrEmpty(strippedCount))
                            PatternCompiler.ValidatePatternSyntax(strippedCount);
                    }
                    var fromAttr = elem.Attribute("from");
                    if (fromAttr != null && !string.IsNullOrWhiteSpace(fromAttr.Value))
                    {
                        var strippedFrom = PatternCompiler.StripXPathComments(fromAttr.Value).Trim();
                        if (!string.IsNullOrEmpty(strippedFrom))
                            PatternCompiler.ValidatePatternSyntax(strippedFrom);
                    }

                    // XTSE0975: @value is mutually exclusive with @select, @level, @count, and @from.
                    var valueAttr = elem.Attribute("value");
                    if (valueAttr != null && !string.IsNullOrWhiteSpace(valueAttr.Value))
                    {
                        if (elem.Attribute("select") != null ||
                            elem.Attribute("level") != null ||
                            elem.Attribute("count") != null ||
                            elem.Attribute("from") != null)
                        {
                            throw new InvalidOperationException("XTSE0975: The value attribute of xsl:number must not be used with select, level, count, or from.");
                        }
                    }
                }

                if (localName is "variable" or "param" or "with-param")
                {
                    var nameAttr = elem.Attribute("name") ?? elem.Attribute("_name");
                    if (nameAttr != null && !string.IsNullOrWhiteSpace(nameAttr.Value))
                        ValidateXsltName(elem, nameAttr.Value, $"xsl:{localName}/@name");
                }

                // xsl:apply-templates/@mode must be a valid mode token (QName or #current/#default/#unnamed/#all).
                if (localName == "apply-templates")
                {
                    var modeAttr = elem.Attribute("mode") ?? elem.Attribute("_mode");
                    if (modeAttr != null)
                    {
                        var modeVal = modeAttr.Value.Trim();
                        if (!IsValidModeValue(modeVal))
                            throw new InvalidOperationException($"XTSE0020: Invalid mode value '{modeVal}' on xsl:apply-templates.");
                        if (modeVal is not ("#current" or "#default" or "#unnamed" or "#all"))
                            ValidateXsltName(elem, modeVal, "xsl:apply-templates/@mode");
                    }
                }

                // xsl:param tunnel restrictions.
                if (localName == "param")
                {
                    var tunnelAttr = elem.Attribute("tunnel") ?? elem.Attribute("_tunnel");
                    if (tunnelAttr != null)
                    {
                        var tunnelVal = TryParseYesNo(tunnelAttr.Value.Trim());
                        if (tunnelVal == true)
                        {
                            var paramParent = elem.Parent;
                            if (paramParent != null && paramParent.Name.NamespaceName == XslNamespace &&
                                paramParent.Name.LocalName is "stylesheet" or "transform" or "function")
                            {
                                throw new InvalidOperationException("XTSE0020: xsl:tunnel='yes' is not permitted on a stylesheet or function parameter.");
                            }
                        }
                    }
                }

                // xsl:decimal-format attribute value validation.
                if (localName == "decimal-format")
                {
                    var forwardsCompatible = IsForwardsCompatibleElement(elem);
                    foreach (var attr in elem.Attributes())
                    {
                        if (attr.IsNamespaceDeclaration) continue;
                        var baseName = attr.Name.LocalName;
                        if (baseName.StartsWith("_")) baseName = baseName.Substring(1);
                        if (attr.Name.NamespaceName != "") continue;

                        if (baseName is "name" or "infinity" or "NaN" or "use-when" or "version")
                            continue;

                        if (baseName is "decimal-separator" or "grouping-separator" or "minus-sign" or "percent" or "per-mille" or "zero-digit" or "digit" or "pattern-separator" or "exponent-separator")
                        {
                            // Use Unicode code point count so non-BMP characters count as one.
                            if (GetUnicodeCodePointCount(attr.Value) != 1)
                                throw new InvalidOperationException($"XTSE0020: xsl:decimal-format @{baseName} must be a single character.");
                        }
                        else if (!forwardsCompatible)
                        {
                            throw new InvalidOperationException($"XTSE0090: Attribute '{attr.Name.LocalName}' is not permitted on xsl:decimal-format.");
                        }
                    }
                }
            }

            // XTSE0710: xsl:use-attribute-sets on literal result elements must reference existing attribute sets (XSLT 2.0+).
            // Only validated on the root stylesheet, where all imports/includes are known.
            if (!isXsltElement && this == _rootStylesheet && GetEffectiveVersion(elem) >= 2.0)
            {
                var lreUseAttrSets = elem.Attribute(XName.Get("use-attribute-sets", XslNamespace));
                if (lreUseAttrSets != null)
                    ValidateUseAttributeSetsValue(elem, lreUseAttrSets.Value, "literal result element/@xsl:use-attribute-sets");
            }
        }
    }

    /// <summary>
    /// Validates a <c>use-attribute-sets</c> value: a whitespace-separated list of EQNames,
    /// each of which must match the name of a declared <c>xsl:attribute-set</c>.
    /// Throws <c>XTSE0710</c> for malformed tokens, undeclared prefixes, or unknown attribute sets.
    /// </summary>
    private void ValidateUseAttributeSetsValue(XElement element, string rawValue, string construct)
    {
        var allAttrSets = GetAllAttributeSets();
        foreach (var (localName, nsUri, rawName) in ParseUseAttributeSetNames(element, rawValue, construct))
        {
            if (!allAttrSets.TryGetValue((localName, nsUri), out var defs) || defs.Count == 0)
                throw new InvalidOperationException($"XTSE0710: Attribute set '{rawName}' referenced by {construct} is not defined.");
        }
    }

    /// <summary>
    /// Parses a whitespace-separated list of attribute-set EQName/QName references
    /// into expanded name tuples. Throws <c>XTSE0710</c> for malformed tokens,
    /// undeclared prefixes, or invalid names.
    /// </summary>
    private static IEnumerable<(string LocalName, string NamespaceUri, string RawName)> ParseUseAttributeSetNames(XElement element, string rawValue, string construct)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            yield break;

        var tokens = rawValue.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var token in tokens)
        {
            var trimmed = token.Trim();
            if (string.IsNullOrEmpty(trimmed))
                continue;

            string localName;
            string nsUri;

            // EQName form: Q{uri}local
            if (trimmed.Length > 2 && trimmed[0] == 'Q' && trimmed[1] == '{')
            {
                int closeBrace = trimmed.IndexOf('}');
                if (closeBrace < 2 || closeBrace == trimmed.Length - 1)
                    throw new InvalidOperationException($"XTSE0710: Invalid attribute-set name '{trimmed}' in {construct}.");

                nsUri = trimmed.Substring(2, closeBrace - 2);
                localName = trimmed.Substring(closeBrace + 1);
                if (!IsValidNCName(localName))
                    throw new InvalidOperationException($"XTSE0710: Invalid attribute-set name '{trimmed}' in {construct}.");
            }
            else
            {
                // Lexical QName: [prefix:]local
                int colon = trimmed.IndexOf(':');
                if (colon >= 0)
                {
                    var prefix = trimmed.Substring(0, colon);
                    localName = trimmed.Substring(colon + 1);
                    if (!IsValidNCName(prefix))
                        throw new InvalidOperationException($"XTSE0710: Invalid prefix '{prefix}' in attribute-set name '{trimmed}' in {construct}.");
                    if (!IsValidNCName(localName))
                        throw new InvalidOperationException($"XTSE0710: Invalid local name '{localName}' in attribute-set name '{trimmed}' in {construct}.");

                    var nsDecl = element.GetNamespaceOfPrefix(prefix);
                    if (nsDecl == null)
                        throw new InvalidOperationException($"XTSE0710: Prefix '{prefix}' in attribute-set name '{trimmed}' in {construct} is not declared.");
                    nsUri = nsDecl.NamespaceName;
                }
                else
                {
                    localName = trimmed;
                    nsUri = "";
                    if (!IsValidNCName(localName))
                        throw new InvalidOperationException($"XTSE0710: Invalid attribute-set name '{trimmed}' in {construct}.");
                }
            }

            yield return (localName, nsUri, trimmed);
        }
    }

    /// <summary>
    /// Returns whether any token in a <c>use-attribute-sets</c> value is the special
    /// <c>xsl:original</c> reference used inside <c>xsl:override</c>.
    /// </summary>
    private static bool UseAttributeSetsReferencesOriginal(XElement context, string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            return false;

        foreach (var token in rawValue.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = token.Trim();
            if (string.IsNullOrEmpty(trimmed))
                continue;

            string nsUri;
            string localName;
            if (trimmed.Length > 2 && trimmed[0] == 'Q' && trimmed[1] == '{')
            {
                int closeBrace = trimmed.IndexOf('}');
                if (closeBrace < 2 || closeBrace == trimmed.Length - 1)
                    continue;
                nsUri = trimmed.Substring(2, closeBrace - 2);
                localName = trimmed.Substring(closeBrace + 1);
            }
            else
            {
                int colon = trimmed.IndexOf(':');
                if (colon >= 0)
                {
                    var prefix = trimmed.Substring(0, colon);
                    localName = trimmed.Substring(colon + 1);
                    nsUri = context.GetNamespaceOfPrefix(prefix)?.NamespaceName ?? "";
                }
                else
                {
                    localName = trimmed;
                    nsUri = "";
                }
            }

            if (localName == "original" && nsUri == XslNamespace)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Detects direct or indirect circular references among <c>xsl:attribute-set</c>
    /// declarations via their <c>use-attribute-sets</c> attributes (XTSE0720).
    /// </summary>
    private void ValidateAttributeSetCircularity()
    {
        var allAttrSets = GetAllAttributeSets();

        // Build name-level dependency graph: an attribute-set name depends on every
        // name referenced in the use-attribute-sets of any of its definitions.
        var dependencies = new Dictionary<(string LocalName, string NamespaceUri), HashSet<(string LocalName, string NamespaceUri)>>();
        foreach (var (name, defs) in allAttrSets)
        {
            if (!dependencies.ContainsKey(name))
                dependencies[name] = new HashSet<(string, string)>();

            foreach (var def in defs)
            {
                if (string.IsNullOrWhiteSpace(def.UseAttributeSets))
                    continue;
                foreach (var (refLocal, refNs, _) in ParseUseAttributeSetNames(def.Element, def.UseAttributeSets, "xsl:attribute-set/@use-attribute-sets"))
                {
                    dependencies[name].Add((refLocal, refNs));
                }
            }
        }

        var visiting = new HashSet<(string LocalName, string NamespaceUri)>();
        var visited = new HashSet<(string LocalName, string NamespaceUri)>();

        void Visit((string LocalName, string NamespaceUri) current)
        {
            if (visiting.Contains(current))
                throw new InvalidOperationException("XTSE0720: An xsl:attribute-set directly or indirectly references itself via use-attribute-sets.");
            if (visited.Contains(current))
                return;

            visiting.Add(current);
            if (dependencies.TryGetValue(current, out var refs))
            {
                foreach (var next in refs)
                    Visit(next);
            }
            visiting.Remove(current);
            visited.Add(current);
        }

        foreach (var name in dependencies.Keys)
            Visit(name);
    }

    /// <summary>
    /// Validates <c>xsl:override</c> attribute-set declarations against the used package:
    /// the target must exist (XTSE3058) and must be public or abstract (XTSE3060).
    /// </summary>
    private void ValidateAttributeSetOverrides()
    {
        foreach (var package in _usedPackages)
        {
            var options = _usedPackageOptions.GetValueOrDefault(package);
            if (options == null)
                continue;
            var packageSets = package.GetAllAttributeSets();
            foreach (var overrideElem in options.OverrideAttributeSets)
            {
                var nameAttr = overrideElem.Attribute("name")?.Value;
                if (string.IsNullOrEmpty(nameAttr))
                    continue;
                var (local, ns) = ExpandVariableName(overrideElem, nameAttr);
                // XTSE3058: the override must be homonymous with a component of the used package.
                if (!packageSets.TryGetValue((local, ns), out var defs))
                    throw new InvalidOperationException($"XTSE3058: xsl:override attribute-set '{nameAttr}' does not match any attribute-set in the used package.");
                foreach (var def in defs)
                {
                    var exposed = package.GetExposedVisibility("attribute-set", local, ns);
                    var effective = exposed ?? GetLocalVisibility(def.Element, "attribute-set", package.IsPackage);
                    // XTSE3060: only public or abstract components may be overridden.
                    if (effective is not "public" and not "abstract")
                        throw new InvalidOperationException($"XTSE3060: Cannot override attribute-set '{nameAttr}' whose visibility is '{effective}'.");
                }
            }
        }
    }

    /// <summary>
    /// Validates <c>xsl:override</c> variable and parameter declarations against the used
    /// package: the target must exist (XTSE3058), must be public or abstract (XTSE3060),
    /// and must have an identical declared type (XTSE3070; an overriding parameter for a
    /// required parameter must itself specify <c>required="yes"</c>).
    /// </summary>
    private void ValidateVariableOverrides()
    {
        foreach (var package in _usedPackages)
        {
            var options = _usedPackageOptions.GetValueOrDefault(package);
            if (options == null)
                continue;
            foreach (var overrideElem in options.OverrideVariables.Concat(options.OverrideParams))
            {
                var nameAttr = overrideElem.Attribute("name")?.Value;
                if (string.IsNullOrEmpty(nameAttr))
                    continue;
                var (local, ns) = ExpandVariableName(overrideElem, nameAttr);
                // XTSE3058: the override must be homonymous with a component of the used package.
                if (!package.TryGetVariableOrParamElement(local, ns, out var element) || element == null)
                    throw new InvalidOperationException($"XTSE3058: xsl:override variable '{nameAttr}' does not match any variable or parameter in the used package.");
                var exposed = package.GetExposedVisibility("variable", local, ns, -1, hasDeclaredVisibility: element.Attribute("visibility") != null);
                var effective = exposed ?? GetParamAwareLocalVisibility(element, package.IsPackage);
                // XTSE3060: only public or abstract components may be overridden.
                if (effective is not "public" and not "abstract")
                    throw new InvalidOperationException($"XTSE3060: Cannot override variable '{nameAttr}' whose visibility is '{effective}'.");
                // XTSE3070: the declared types must be identical (override-v-007).
                if (!TypesAreIdentical(overrideElem.Attribute("as")?.Value, element.Attribute("as")?.Value))
                    throw new InvalidOperationException($"XTSE3070: The declared type of overriding variable '{nameAttr}' differs from the overridden variable.");
            }
        }
    }

    /// <summary>
    /// Validates that no <c>xsl:override</c> function tries to override a used-package
    /// function that is exposed as <c>final</c> (raises <c>XTSE3060</c>), and that a single
    /// <c>xsl:override</c> element does not contain two functions with the same expanded
    /// QName and arity (raises <c>XTSE0770</c>).
    /// </summary>
    private void ValidateFunctionOverrides()
    {
        foreach (var package in _usedPackages)
        {
            var options = _usedPackageOptions.GetValueOrDefault(package);
            if (options == null)
                continue;
            var seenOverrides = new HashSet<(string LocalName, string NamespaceUri, int Arity)>();
            foreach (var overrideElem in options.OverrideFunctions)
            {
                var nameAttr = overrideElem.Attribute("name")?.Value;
                if (string.IsNullOrEmpty(nameAttr))
                    continue;
                var (local, ns) = ExpandVariableName(overrideElem, nameAttr);
                var arity = overrideElem.Elements(XName.Get("param", XslNamespace)).Count();
                if (!seenOverrides.Add((local, ns, arity)))
                    throw new InvalidOperationException($"XTSE0770: Duplicate overriding function declaration '{{{ns}}}{local}#{arity}'.");
                // XTSE3058: the override must be homonymous with a component of the used package.
                if (!package.TryFindFunction(local, ns, arity, out var def) || def == null)
                    throw new InvalidOperationException($"XTSE3058: xsl:override function '{nameAttr}#{arity}' does not match any function in the used package.");
                var exposed = package.GetExposedVisibility("function", local, ns, arity);
                var effective = exposed ?? def.Visibility;
                // XTSE3060: only public or abstract components may be overridden.
                if (effective is not "public" and not "abstract")
                    throw new InvalidOperationException($"XTSE3060: Cannot override function '{nameAttr}' whose visibility is '{effective}'.");
                ValidateFunctionOverrideSignature(overrideElem, def, nameAttr);
            }
        }
    }

    /// <summary>
    /// Validates the signature compatibility rules of XSLT 3.0 §3.5.7.2 (XTSE3070) for an
    /// overriding function: argument types must be pairwise identical to the overridden
    /// function's and the return types must be identical.
    /// </summary>
    private static void ValidateFunctionOverrideSignature(XElement overrideElem, XsltFunctionDefinition def, string displayName)
    {
        var overrideParams = overrideElem.Elements(XName.Get("param", XslNamespace)).ToList();
        var baseParams = def.Element.Elements(XName.Get("param", XslNamespace)).ToList();
        for (int i = 0; i < baseParams.Count && i < overrideParams.Count; i++)
        {
            if (!TypesAreIdentical(overrideParams[i].Attribute("as")?.Value, baseParams[i].Attribute("as")?.Value))
                throw new InvalidOperationException($"XTSE3070: The signature of overriding function '{displayName}' is not compatible with the overridden function (parameter '{overrideParams[i].Attribute("name")?.Value}' type differs).");
        }
        if (!TypesAreIdentical(overrideElem.Attribute("as")?.Value, def.Element.Attribute("as")?.Value))
            throw new InvalidOperationException($"XTSE3070: The return type of overriding function '{displayName}' differs from the overridden function.");

        // The effective new-each-time value must be the same on both declarations
        // (default "yes"; override-f-021).
        var overrideDet = overrideElem.Attribute("new-each-time")?.Value?.Trim()?.ToLowerInvariant() ?? "yes";
        var baseDet = def.Element.Attribute("new-each-time")?.Value?.Trim()?.ToLowerInvariant() ?? "yes";
        if (overrideDet != baseDet)
            throw new InvalidOperationException($"XTSE3070: The new-each-time value of overriding function '{displayName}' differs from the overridden function.");
    }

    /// <summary>
    /// Compares two SequenceType attribute values for identity (both absent means the default
    /// <c>item()*</c> on both sides); comparison is whitespace- and punctuation-insensitive.
    /// </summary>
    internal static bool TypesAreIdentical(string? a, string? b)
    {
        return NormalizeTypeForComparison(a) == NormalizeTypeForComparison(b);
    }

    private static string NormalizeTypeForComparison(string? type)
    {
        if (string.IsNullOrWhiteSpace(type))
            return "";
        var s = Patterns.PatternCompiler.StripXPathComments(type);
        // SequenceTypes contain no significant whitespace: remove it all so that
        // "element( * )" and "element(*)" compare equal (glob-cxt-item-008).
        return Regex.Replace(s, @"\s+", "");
    }

    /// <summary>
    /// Validates that no <c>xsl:override</c> mode tries to override a used-package
    /// mode that is exposed as <c>final</c>. Raises <c>XTSE3060</c> when such an
    /// override is detected.
    /// </summary>
    private void ValidateModeOverrides()
    {
        foreach (var package in _usedPackages)
        {
            var options = _usedPackageOptions.GetValueOrDefault(package);
            if (options == null)
                continue;
            foreach (var overrideElem in options.OverrideModes)
            {
                var nameAttr = overrideElem.Attribute("name")?.Value;
                if (string.IsNullOrEmpty(nameAttr))
                    continue;
                var expanded = ExpandModeNameForExpose(nameAttr, overrideElem);
                // XTSE3058: the override must be homonymous with a component of the used package.
                if (!package.TryGetMode(expanded, out var mode) || mode == null)
                    throw new InvalidOperationException($"XTSE3058: xsl:override mode '{nameAttr}' does not match any mode in the used package.");
                var exposed = package.GetExposedVisibility("mode", mode.Name, null);
                var element = package.FindModeElement(expanded);
                var effective = exposed ?? GetLocalVisibility(element, "mode", package.IsPackage);
                // XTSE3060: only public or abstract components may be overridden.
                if (effective is not "public" and not "abstract")
                    throw new InvalidOperationException($"XTSE3060: Cannot override mode '{nameAttr}' whose visibility is '{effective}'.");
            }
        }
    }

    /// <summary>
    /// Validates <c>xsl:override</c> named-template declarations against the used package:
    /// the target must exist (XTSE3058), must be public or abstract (XTSE3060), and the
    /// signatures must be compatible (XTSE3070: identical return types; every overridden
    /// parameter present with identical type and the same tunnel/required values; extra
    /// overriding parameters must be optional; equivalent xsl:context-item children).
    /// </summary>
    private void ValidateTemplateOverrides()
    {
        foreach (var package in _usedPackages)
        {
            var options = _usedPackageOptions.GetValueOrDefault(package);
            if (options == null)
                continue;
            foreach (var overrideElem in options.OverrideTemplates)
            {
                var nameAttr = overrideElem.Attribute("name")?.Value;
                if (string.IsNullOrEmpty(nameAttr))
                {
                    ValidateOverrideTemplateRule(overrideElem, package);
                    continue;
                }
                var (local, ns) = ExpandVariableName(overrideElem, nameAttr);
                // XTSE3058: the override must be homonymous with a component of the used package.
                // The target may also be a component the used package itself accepted from
                // its own used packages (override-t-003a), so search transitively.
                if (!package.TryFindNamedTemplateDeep(local, ns, out var rule, out var declaringPackage) || rule == null || declaringPackage == null)
                    throw new InvalidOperationException($"XTSE3058: xsl:override template '{nameAttr}' does not match any named template in the used package.");
                var exposed = declaringPackage.GetExposedVisibility("template", local, ns, -1, hasDeclaredVisibility: rule.Element.Attribute("visibility") != null);
                var effective = exposed ?? GetLocalVisibility(rule.Element, "template", declaringPackage);
                // XTSE3060: only public or abstract components may be overridden.
                if (effective is not "public" and not "abstract")
                    throw new InvalidOperationException($"XTSE3060: Cannot override template '{nameAttr}' whose visibility is '{effective}'.");
                ValidateTemplateOverrideSignature(overrideElem, rule.Element, nameAttr);
            }
        }
    }

    /// <summary>
    /// Validates an <c>xsl:override</c> template rule (a template with <c>@match</c> and no
    /// <c>@name</c>): the mode list must not contain <c>#all</c> or <c>#unnamed</c>, and must
    /// not resolve to the unnamed mode via <c>#default</c> or an omitted <c>@mode</c>
    /// (XTSE3440, XSLT 3.0 §3.5.7.2); a named mode that is not public or abstract in the
    /// used package cannot be overridden (XTSE3060).
    /// </summary>
    private void ValidateOverrideTemplateRule(XElement overrideElem, Stylesheet package)
    {
        if (overrideElem.Attribute("match") == null)
            return;

        var modeAttr = overrideElem.Attribute("mode")?.Value;
        var defaultModeRaw = overrideElem.Parent?.Attribute("default-mode")?.Value?.Trim();
        bool defaultIsUnnamed = string.IsNullOrEmpty(defaultModeRaw) || defaultModeRaw == "#unnamed";

        var tokens = (modeAttr ?? "").Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var token in tokens)
        {
            if (token is "#all" or "#unnamed")
                throw new InvalidOperationException($"XTSE3440: The mode list of a template rule in xsl:override must not contain '{token}'.");
            if (token == "#default" && defaultIsUnnamed)
                throw new InvalidOperationException("XTSE3440: A template rule in xsl:override must not use the unnamed mode (mode='#default' refers to the unnamed mode).");
        }
        if (string.IsNullOrWhiteSpace(modeAttr) && defaultIsUnnamed)
            throw new InvalidOperationException("XTSE3440: A template rule in xsl:override must not use the unnamed mode (the mode attribute is omitted and the default mode is unnamed).");

        // XTSE3060: a named mode that is not public or abstract in the used package cannot
        // be overridden (final or private modes reject template-rule overrides).
        foreach (var token in tokens)
        {
            if (token.StartsWith('#'))
                continue;
            var expanded = ExpandModeNameForExpose(token, overrideElem);
            if (!package.TryGetMode(expanded, out var mode) || mode == null)
                continue;
            var exposed = package.GetExposedVisibility("mode", mode.Name, null);
            var modeElement = package.FindModeElement(expanded);
            var effective = exposed ?? GetLocalVisibility(modeElement, "mode", package.IsPackage);
            if (effective is not "public" and not "abstract")
                throw new InvalidOperationException($"XTSE3060: Cannot override a template rule in mode '{token}' whose visibility is '{effective}'.");
        }
    }

    /// <summary>
    /// Validates the XTSE3070 signature-compatibility rules for an overriding named template
    /// (XSLT 3.0 §3.5.7.2): return types identical, parameters present on both declarations
    /// with identical types, additional overriding parameters optional, and equivalent
    /// xsl:context-item children.
    /// </summary>
    private static void ValidateTemplateOverrideSignature(XElement overrideElem, XElement baseElem, string displayName)
    {
        if (!TypesAreIdentical(overrideElem.Attribute("as")?.Value, baseElem.Attribute("as")?.Value))
            throw new InvalidOperationException($"XTSE3070: The return type of overriding template '{displayName}' differs from the overridden template.");

        var overrideParams = overrideElem.Elements(XName.Get("param", XslNamespace))
            .Select(p => (Name: p.Attribute("name")?.Value ?? "", Element: p))
            .Where(p => !string.IsNullOrEmpty(p.Name))
            .ToList();
        var baseParams = baseElem.Elements(XName.Get("param", XslNamespace))
            .Select(p => (Name: p.Attribute("name")?.Value ?? "", Element: p))
            .Where(p => !string.IsNullOrEmpty(p.Name))
            .ToList();

        // Non-tunnel parameters on the overridden template require a same-named non-tunnel
        // parameter on the overriding template with an identical type and the same required
        // value. A tunnel parameter on the overridden template may be omitted; if the
        // overriding template declares a same-named parameter it must also be a tunnel
        // parameter with an identical type (override-t-004/012/013).
        foreach (var baseParam in baseParams)
        {
            bool baseTunnel = baseParam.Element.Attribute("tunnel")?.Value?.Trim() is "yes" or "true" or "1";
            var match = overrideParams.FirstOrDefault(p => p.Name == baseParam.Name);
            if (match.Element == null)
                continue; // omitted parameters are permitted
            bool overrideTunnel = match.Element.Attribute("tunnel")?.Value?.Trim() is "yes" or "true" or "1";
            if (baseTunnel != overrideTunnel)
                throw new InvalidOperationException($"XTSE3070: The tunnel value of parameter '{baseParam.Name}' on overriding template '{displayName}' differs from the overridden template.");
            if (!TypesAreIdentical(match.Element.Attribute("as")?.Value, baseParam.Element.Attribute("as")?.Value))
                throw new InvalidOperationException($"XTSE3070: The type of parameter '{baseParam.Name}' on overriding template '{displayName}' differs from the overridden template.");
            if (!baseTunnel)
            {
                bool baseRequired = baseParam.Element.Attribute("required")?.Value?.Trim() is "yes" or "true" or "1";
                bool overrideRequired = match.Element.Attribute("required")?.Value?.Trim() is "yes" or "true" or "1";
                if (baseRequired != overrideRequired)
                    throw new InvalidOperationException($"XTSE3070: The required value of parameter '{baseParam.Name}' on overriding template '{displayName}' differs from the overridden template.");
            }
        }

        // Any parameter on the overriding template with no counterpart on the overridden
        // template must be optional (required="no", the default).
        foreach (var extra in overrideParams.Where(p => baseParams.All(b => b.Name != p.Name)))
        {
            bool required = extra.Element.Attribute("required")?.Value?.Trim() is "yes" or "true" or "1";
            if (required)
                throw new InvalidOperationException($"XTSE3070: The overriding template '{displayName}' declares required parameter '{extra.Name}' that does not exist on the overridden template.");
        }

        // xsl:context-item equivalence: the use values must be the same and the required
        // types identical; an absent xsl:context-item is equivalent to use="optional"
        // as="item()".
        var overrideCi = overrideElem.Elements(XName.Get("context-item", XslNamespace)).FirstOrDefault();
        var baseCi = baseElem.Elements(XName.Get("context-item", XslNamespace)).FirstOrDefault();
        var overrideUse = overrideCi?.Attribute("use")?.Value?.Trim() ?? "optional";
        var baseUse = baseCi?.Attribute("use")?.Value?.Trim() ?? "optional";
        if (overrideUse != baseUse)
            throw new InvalidOperationException($"XTSE3070: The xsl:context-item of overriding template '{displayName}' is not equivalent to the overridden template's.");
        var overrideCiAs = overrideCi?.Attribute("as")?.Value;
        var baseCiAs = baseCi?.Attribute("as")?.Value;
        if (!TypesAreIdentical(overrideCiAs ?? "item()", baseCiAs ?? "item()"))
            throw new InvalidOperationException($"XTSE3070: The xsl:context-item type of overriding template '{displayName}' differs from the overridden template's.");
    }

    /// <summary>
    /// Validates an <c>exclude-result-prefixes</c> value: a whitespace-separated list of
    /// namespace prefixes, <c>#all</c>, or <c>#default</c>. Throws <c>XTSE0808</c> when a
    /// prefix token has no namespace binding in scope on the owning element, and
    /// <c>XTSE0809</c> when <c>#default</c> is used but the owning element has no default
    /// namespace.
    /// </summary>
    private void ValidateExcludeResultPrefixesValue(XElement element, string rawValue, string construct)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            return;

        foreach (var token in rawValue.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var prefix = token.Trim();
            if (string.IsNullOrEmpty(prefix) || prefix == "#all")
                continue;

            if (prefix == "#default")
            {
                // XTSE0809: #default requires a default namespace in scope on the owning element.
                var defaultNs = element.GetDefaultNamespace()?.NamespaceName ?? string.Empty;
                if (string.IsNullOrEmpty(defaultNs))
                    throw new InvalidOperationException($"XTSE0809: #default used in {construct} but the owning element has no default namespace.");
                continue;
            }

            var ns = element.GetNamespaceOfPrefix(prefix);
            if (ns == null)
                throw new InvalidOperationException($"XTSE0808: Namespace prefix '{prefix}' used in {construct} has no binding in scope.");
        }
    }

    /// <summary>
    /// Returns whether <paramref name="uri"/> is a collation URI recognized by this
    /// implementation, optionally resolving a relative URI against <paramref name="baseUri"/>.
    /// </summary>
    private static bool IsSupportedCollationUri(string uri, string? baseUri)
    {
        string resolved = uri;
        if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute) &&
            !string.IsNullOrEmpty(baseUri) &&
            Uri.TryCreate(new Uri(baseUri), uri, out var resolvedUri))
        {
            resolved = resolvedUri.AbsoluteUri;
        }

        if (resolved == "http://www.w3.org/2005/xpath-functions/collation/codepoint")
            return true;
        if (resolved == "http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive")
            return true;
        if (resolved.StartsWith("http://www.w3.org/2013/collation/UCA", StringComparison.Ordinal))
            return true;
        return false;
    }

    private static bool IsMergeSourceAttribute(string baseName)
    {
        return baseName is "select" or "for-each-item" or "for-each-source"
                   or "name" or "streamable" or "sort-before-merge"
                   or "use-accumulators" or "validation" or "type" or "use-when";
    }

    /// <summary>
    /// XSLT declarations that are only permitted as top-level children of xsl:stylesheet
    /// or xsl:transform. If any of these appear deeper in the tree, it is a static error.
    /// </summary>
    private static readonly HashSet<string> TopLevelOnlyDeclarations = new(StringComparer.Ordinal)
    {
        "stylesheet", "transform", "package",
        "import", "include", "strip-space", "preserve-space", "output", "namespace-alias",
        "attribute-set", "decimal-format", "key", "mode", "accumulator", "template", "function",
        "global-context-item", "import-schema",
        "use-package", "expose", "accept", "override"
    };

    /// <summary>
    /// XSLT elements that are permitted as top-level children of xsl:stylesheet or xsl:transform.
    /// Any other XSLT element appearing at the top level is a static error (unless the stylesheet
    /// is in forwards-compatible mode or the element is an unrecognized vendor extension in XSLT 3.0).
    /// </summary>
    private static readonly HashSet<string> AllowedTopLevelDeclarations = new(StringComparer.Ordinal)
    {
        "import", "include", "strip-space", "preserve-space", "output", "namespace-alias",
        "attribute-set", "character-map", "decimal-format", "key", "mode", "accumulator", "param", "variable",
        "template", "function", "global-context-item", "use-package", "package", "expose",
        "import-schema"
    };

    private static bool IsValidMergeSourceName(string name)
    {
        // Allow Q{uri}local EQNames and simple NCNames
        if (name.Length > 2 && name[0] == 'Q' && name[1] == '{')
        {
            int closeBrace = name.IndexOf('}');
            return closeBrace >= 2 && closeBrace < name.Length - 1;
        }
        // Simple NCName check: no colons, not starting with digit
        if (string.IsNullOrEmpty(name) || name.Contains(':'))
            return false;
        var first = name[0];
        return first == '_' || char.IsLetter(first);
    }

    /// <summary>
    /// Validates that <paramref name="name"/> is a legal XSLT lexical QName or EQName.
    /// Returns the local name and namespace URI if valid; otherwise throws <c>XTSE0020</c>.
    /// </summary>
    private static (string localName, string namespaceUri) ValidateXsltName(XElement element, string name, string construct)
    {
        var trimmed = name.Trim();
        if (string.IsNullOrEmpty(trimmed))
            throw new InvalidOperationException($"XTSE0020: The name for {construct} must not be empty.");

        // EQName form: Q{uri}local. EQName syntax takes precedence over attribute value
        // template detection, so a value like Q{http://example.com/ns}set1 is a valid name.
        if (trimmed.Length > 2 && trimmed[0] == 'Q' && trimmed[1] == '{')
        {
            int closeBrace = trimmed.IndexOf('}');
            if (closeBrace < 2 || closeBrace == trimmed.Length - 1)
                throw new InvalidOperationException($"XTSE0020: The name '{name}' for {construct} is not a valid EQName.");

            var uri = trimmed.Substring(2, closeBrace - 2);
            var local = trimmed.Substring(closeBrace + 1);
            if (!IsValidNCName(local))
                throw new InvalidOperationException($"XTSE0020: The local part '{local}' in '{name}' for {construct} is not a valid NCName.");
            return (local, uri);
        }

        // Reject attribute value templates (e.g. name="{concat('a','b')}").
        if (IsAvtValue(trimmed))
            throw new InvalidOperationException($"XTSE0020: The name '{name}' for {construct} must not contain an attribute value template.");

        // Lexical QName: [prefix:]local
        int colon = trimmed.IndexOf(':');
        if (colon >= 0)
        {
            var prefix = trimmed.Substring(0, colon);
            var local = trimmed.Substring(colon + 1);
            if (!IsValidNCName(prefix))
                throw new InvalidOperationException($"XTSE0020: The prefix '{prefix}' in '{name}' for {construct} is not a valid NCName.");
            if (!IsValidNCName(local))
                throw new InvalidOperationException($"XTSE0020: The local part '{local}' in '{name}' for {construct} is not a valid NCName.");

            var nsDecl = element.GetNamespaceOfPrefix(prefix);
            if (nsDecl == null)
                throw new InvalidOperationException($"XTSE0280: The prefix '{prefix}' in '{name}' for {construct} is not declared.");
            var ns = nsDecl.NamespaceName;
            return (local, ns);
        }

        // Simple NCName
        if (!IsValidNCName(trimmed))
            throw new InvalidOperationException($"XTSE0020: The name '{name}' for {construct} is not a valid NCName.");
        return (trimmed, string.Empty);
    }

    /// <summary>
    /// Returns <c>true</c> if <paramref name="name"/> is a valid XML NCName.
    /// Uses the XML 1.0 fifth edition / XML 1.1 name rules, which are more
    /// permissive than <see cref="XmlConvert.VerifyNCName"/>.
    /// </summary>
    private static bool IsValidNCName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;

        // First character must be a NameStartChar (excluding ':').
        int i = 0;
        if (!IsNameStartChar(name, ref i))
            return false;

        // Remaining characters must be NameChars (excluding ':').
        while (i < name.Length)
        {
            if (!IsNameChar(name, ref i))
                return false;
        }

        return true;
    }

    private static bool IsNameStartChar(string name, ref int index)
    {
        if (index >= name.Length)
            return false;

        int code = char.ConvertToUtf32(name, index);
        var category = CharUnicodeInfo.GetUnicodeCategory(code);
        bool isStart = category is UnicodeCategory.UppercaseLetter
                                  or UnicodeCategory.LowercaseLetter
                                  or UnicodeCategory.TitlecaseLetter
                                  or UnicodeCategory.ModifierLetter
                                  or UnicodeCategory.OtherLetter
                                  or UnicodeCategory.LetterNumber
                       || code == '_';

        if (isStart)
            index += code > 0xFFFF ? 2 : 1;

        return isStart;
    }

    private static bool IsNameChar(string name, ref int index)
    {
        if (index >= name.Length)
            return false;

        int code = char.ConvertToUtf32(name, index);
        var category = CharUnicodeInfo.GetUnicodeCategory(code);
        bool isChar = category is UnicodeCategory.UppercaseLetter
                                or UnicodeCategory.LowercaseLetter
                                or UnicodeCategory.TitlecaseLetter
                                or UnicodeCategory.ModifierLetter
                                or UnicodeCategory.OtherLetter
                                or UnicodeCategory.LetterNumber
                                or UnicodeCategory.NonSpacingMark
                                or UnicodeCategory.SpacingCombiningMark
                                or UnicodeCategory.DecimalDigitNumber
                                or UnicodeCategory.ConnectorPunctuation
                     || code == '_' || code == '-' || code == '.';

        if (isChar)
            index += code > 0xFFFF ? 2 : 1;

        return isChar;
    }

    /// <summary>
    /// Returns <c>true</c> if <paramref name="value"/> is a valid lexical representation
    /// of <c>xs:decimal</c>. Exponent notation (e.g. <c>2.0e2</c>) is rejected.
    /// </summary>
    private static bool IsValidXsDecimal(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return decimal.TryParse(
            value.Trim(),
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out _);
    }

    /// <summary>
    /// Returns <c>true</c> if <paramref name="value"/> is a valid XSLT mode token.
    /// Mode names are lexical QNames or the special tokens <c>#current</c>, <c>#default</c>,
    /// <c>#unnamed</c>, and <c>#all</c>.
    /// </summary>
    private static bool IsValidModeValue(string value)
    {
        var trimmed = value.Trim();
        if (trimmed is "#current" or "#default" or "#unnamed" or "#all")
            return true;

        // EQName form: Q{uri}local. EQName syntax takes precedence over AVT detection.
        if (trimmed.Length > 2 && trimmed[0] == 'Q' && trimmed[1] == '{')
        {
            int closeBrace = trimmed.IndexOf('}');
            if (closeBrace < 2 || closeBrace == trimmed.Length - 1)
                return false;
            var local = trimmed.Substring(closeBrace + 1);
            return IsValidNCName(local);
        }

        if (IsAvtValue(trimmed))
            return false;

        int colon = trimmed.IndexOf(':');
        if (colon >= 0)
        {
            var prefix = trimmed.Substring(0, colon);
            var local = trimmed.Substring(colon + 1);
            return IsValidNCName(prefix) && IsValidNCName(local);
        }
        return IsValidNCName(trimmed);
    }

    /// <summary>
    /// Validates the <c>mode</c> attribute of an <c>xsl:template</c> element.
    /// The value is a whitespace-separated list of mode tokens. Throws <c>XTSE0550</c>
    /// if the list is empty, contains duplicates, contains an invalid token, or
    /// contains <c>#all</c> together with any other value.
    /// </summary>
    private static void ValidateTemplateModeAttribute(XElement element, string value)
    {
        var trimmed = value.Trim();
        if (string.IsNullOrEmpty(trimmed))
            throw new InvalidOperationException("XTSE0550: The mode attribute of xsl:template must not be empty.");

        var rawTokens = trimmed.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        if (rawTokens.Length == 0)
            throw new InvalidOperationException("XTSE0550: The mode attribute of xsl:template must not be empty.");

        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var raw in rawTokens)
        {
            var token = raw.Trim();
            if (string.IsNullOrEmpty(token))
                continue;

            if (token == "#all")
            {
                if (rawTokens.Length > 1)
                    throw new InvalidOperationException("XTSE0550: The mode attribute of xsl:template must not contain #all with other values.");
            }
            else if (token is "#default" or "#unnamed")
            {
                // Valid tokens for xsl:template/@mode.
            }
            else if (token == "#current")
            {
                throw new InvalidOperationException($"XTSE0550: Invalid mode token '{token}' in xsl:template/@mode.");
            }
            else if (!IsValidModeValue(token))
            {
                throw new InvalidOperationException($"XTSE0550: Invalid mode token '{token}' in xsl:template/@mode.");
            }
            else
            {
                // QName/EQName mode token: ensure any prefix is declared (XTSE0280).
                ValidateXsltName(element, token, "xsl:template/@mode");
            }

            if (!seen.Add(token))
                throw new InvalidOperationException($"XTSE0550: Duplicate mode token '{token}' in xsl:template/@mode.");
        }
    }

    private static bool IsYesNoValue(string value)
    {
        var v = value.Trim();
        return v == "yes" || v == "no" || v == "true" || v == "false" || v == "1" || v == "0";
    }

    private static bool IsAvtValue(string value)
    {
        // An attribute value template contains an unescaped '{'.
        for (int i = 0; i < value.Length - 1; i++)
        {
            if (value[i] == '{' && value[i + 1] != '{')
                return true;
        }
        return false;
    }

    /// <summary>
    /// Returns the number of Unicode code points in <paramref name="value"/>.
    /// Surrogate pairs representing supplementary characters are counted as one.
    /// </summary>
    private static int GetUnicodeCodePointCount(string value)
    {
        int count = 0;
        foreach (var _ in value.EnumerateRunes())
            count++;
        return count;
    }

    /// <summary>
    /// Throws <c>XTSE0090</c> if <paramref name="elem"/> has any unprefixed attribute
    /// that is not in <paramref name="allowedAttributes"/>. In forwards-compatible mode
    /// unknown attributes are ignored.
    /// </summary>
    private static void ValidateAllowedAttributes(XElement elem, string localName, HashSet<string> allowedAttributes, bool forwardsCompatible)
    {
        if (forwardsCompatible)
            return;
        foreach (var attr in elem.Attributes())
        {
            if (attr.IsNamespaceDeclaration)
                continue;
            var baseName = attr.Name.LocalName;
            if (baseName.StartsWith("_"))
                baseName = baseName.Substring(1);
            if (attr.Name.NamespaceName == "" && !allowedAttributes.Contains(baseName))
                throw new InvalidOperationException($"XTSE0090: Attribute '{attr.Name.LocalName}' is not permitted on xsl:{localName}.");
        }
    }

    /// <summary>
    /// Builds an attribute whitelist starting with the XSLT standard attributes and adding
    /// any element-specific attributes supplied in <paramref name="specificAttributes"/>.
    /// </summary>
    private static HashSet<string> AllowedXsltAttributes(params string[] specificAttributes)
    {
        var set = new HashSet<string>(StringComparer.Ordinal)
        {
            "version", "use-when", "expand-text", "default-mode", "default-collation",
            "xpath-default-namespace", "exclude-result-prefixes", "extension-element-prefixes",
            "default-validation", "input-type-annotations"
        };
        foreach (var attr in specificAttributes)
            set.Add(attr);
        return set;
    }

    private static bool IsNewEachTimeValue(string value)
    {
        var v = value.Trim();
        return v == "yes" || v == "no" || v == "true" || v == "false" || v == "1" || v == "0" ||
               v == "maybe" || v == "probably";
    }

    private static bool? TryParseYesNo(string value)
    {
        var v = value.Trim();
        if (v == "yes" || v == "true" || v == "1")
            return true;
        if (v == "no" || v == "false" || v == "0")
            return false;
        return null;
    }

    /// <summary>
    /// Wraps a literal result element stylesheet into an implicit xsl:stylesheet/xsl:template document.
    /// </summary>
    private static XDocument WrapLiteralResultElement(XElement literalRoot, string version)
    {
        var xsl = XNamespace.Get("http://www.w3.org/1999/XSL/Transform");
        var wrapper = new XElement(xsl + "stylesheet",
            new XAttribute("version", version),
            new XElement(xsl + "template",
                new XAttribute("match", "/"),
                literalRoot));

        // Copy all namespace declarations from the literal root to the wrapper
        foreach (var attr in literalRoot.Attributes())
        {
            if (attr.IsNamespaceDeclaration)
            {
                if (attr.Name.LocalName == "xmlns")
                    wrapper.SetAttributeValue("xmlns", attr.Value);
                else
                    wrapper.SetAttributeValue(XNamespace.Xmlns + attr.Name.LocalName, attr.Value);
            }
        }

        return new XDocument(wrapper);
    }

    /// <summary>
    /// Resolves a lexical variable or parameter name to an expanded QName.
    /// Handles <c>Q{uri}local</c> EQNames and prefixed QNames using the namespaces
    /// in scope on the declaring element.
    /// </summary>
    internal static (string LocalName, string NamespaceUri) ExpandVariableName(XElement element, string name)
    {
        if (string.IsNullOrEmpty(name))
            return ("", "");

        // QName-valued attributes are whitespace-collapsed by the XSLT data model,
        // so leading and trailing space around a lexical or EQName must be ignored.
        name = name.Trim();

        // The empty URI form Q{}local is permitted and means "no namespace".
        if (name.Length > 2 && name[0] == 'Q' && name[1] == '{')
        {
            int closeBrace = name.IndexOf('}');
            if (closeBrace >= 2)
            {
                string uri = name[2..closeBrace];
                string rest = name[(closeBrace + 1)..];
                int restColon = rest.IndexOf(':');
                string local = restColon < 0 ? rest : rest[(restColon + 1)..];
                return (local, uri);
            }
        }

        int colon = name.IndexOf(':');
        if (colon >= 0)
        {
            string prefix = name[..colon];
            string local = name[(colon + 1)..];
            if (prefix == "xml")
                return (local, "http://www.w3.org/XML/1998/namespace");

            var ns = element.GetNamespaceOfPrefix(prefix);
            if (ns == null)
                throw new InvalidOperationException($"XPST0081: Undefined namespace prefix '{prefix}'");
            return (local, ns.NamespaceName);
        }

        return (name, "");
    }

    /// <summary>
    /// Throws XTSE0670 if two or more sibling xsl:with-param elements have the same
    /// expanded QName. Used for xsl:call-template, xsl:apply-templates, and
    /// xsl:apply-imports.
    /// </summary>
    private static void ValidateDuplicateWithParamNames(XElement parent)
    {
        var seen = new HashSet<(string LocalName, string NamespaceUri)>();
        foreach (var child in parent.Elements())
        {
            if (child.Name.NamespaceName != XslNamespace || child.Name.LocalName != "with-param")
                continue;
            var nameAttr = child.Attribute("name")?.Value;
            if (string.IsNullOrWhiteSpace(nameAttr))
                continue;
            var expanded = ExpandVariableName(child, nameAttr);
            if (!seen.Add(expanded))
                throw new InvalidOperationException("XTSE0670: Two or more sibling xsl:with-param elements have the same expanded QName.");
        }
    }

    /// <summary>
    /// Returns true if the effective stylesheet version is 1.0 (backwards-compatible mode).
    /// </summary>
    private static bool IsVersion10(XElement root)
    {
        var version = root.Attribute("version")?.Value?.Trim();
        return version is "1.0" or "1";
    }

    /// <summary>
    /// Annotation attached to an <c>xsl:import</c> or <c>xsl:include</c> element
    /// so that document-order flattening methods can locate the resolved child
    /// stylesheet (if any).
    /// </summary>
    private sealed class ResolvedModuleAnnotation
    {
        public Stylesheet? Module { get; init; }
    }

    /// <summary>
    /// Holds the <c>xsl:accept</c> and <c>xsl:override</c> information declared on a
    /// single <c>xsl:use-package</c> element. Used to adjust component visibility and
    /// to replace components from the used package with local overrides.
    /// </summary>
    private sealed class PackageUseOptions
    {
        public List<AcceptRule> AcceptRules { get; } = new();
        public List<XElement> OverrideTemplates { get; } = new();
        public List<XElement> OverrideFunctions { get; } = new();
        public List<XElement> OverrideVariables { get; } = new();
        public List<XElement> OverrideParams { get; } = new();
        public List<XElement> OverrideAttributeSets { get; } = new();
        public List<XElement> OverrideModes { get; } = new();
        public List<XElement> OverrideKeys { get; } = new();
        public List<XElement> OverrideDecimalFormats { get; } = new();
        public List<XElement> OverrideNamespaceAliases { get; } = new();
        public List<XElement> OverrideCharacterMaps { get; } = new();
        public List<XElement> OverrideOutput { get; } = new();
        public List<XElement> OverrideStripSpace { get; } = new();
        public List<XElement> OverridePreserveSpace { get; } = new();
    }

    /// <summary>
    /// Represents a single <c>xsl:accept</c> rule from a <c>xsl:use-package</c> element.
    /// </summary>
    private sealed class AcceptRule
    {
        public string Component { get; }
        public string Visibility { get; }
        public IReadOnlyList<AcceptName> Names { get; }
        public bool IsWildcard => Names.Count == 1 && Names[0].IsWildcard;

        public AcceptRule(string component, string visibility, IEnumerable<AcceptName> names)
        {
            Component = component;
            Visibility = visibility;
            Names = names.ToList();
        }

        public bool Matches(string? localName, string? namespaceUri, int arity = -1)
        {
            if (IsWildcard) return true;
            foreach (var name in Names)
            {
                if (name.LocalName != localName && name.LocalName != "*")
                    continue;
                if (name.Arity >= 0 && name.Arity != arity)
                    continue;
                if (name.IsNamespaceWildcard)
                    return true;
                if (name.LocalName == "*")
                {
                    if (name.NamespaceUri == namespaceUri)
                        return true;
                    if (string.IsNullOrEmpty(name.NamespaceUri) && string.IsNullOrEmpty(namespaceUri))
                        return true;
                    continue;
                }
                if (name.NamespaceUri == namespaceUri)
                    return true;
                if (string.IsNullOrEmpty(name.NamespaceUri) && string.IsNullOrEmpty(namespaceUri))
                    return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Represents one name entry in an <c>xsl:accept</c> <c>names</c> attribute,
    /// optionally carrying a required function arity.
    /// </summary>
    private readonly struct AcceptName
    {
        public string? NamespaceUri { get; }
        public string LocalName { get; }
        public int Arity { get; }
        public bool IsWildcard { get; }
        public bool IsNamespaceWildcard => NamespaceUri == "*" && !IsWildcard;

        public AcceptName(string? namespaceUri, string localName, int arity = -1, bool isWildcard = false)
        {
            NamespaceUri = namespaceUri;
            LocalName = localName;
            Arity = arity;
            IsWildcard = isWildcard;
        }

        /// <summary>
        /// Returns whether this name pattern matches the supplied component identity.
        /// A <c>*</c> in either the namespace URI or local name position acts as a wildcard.
        /// </summary>
        public bool Matches(string? localName, string? namespaceUri, int arity)
        {
            if (IsWildcard)
                return true;
            if (LocalName != localName && LocalName != "*")
                return false;
            if (Arity >= 0 && Arity != arity)
                return false;
            if (NamespaceUri == "*")
                return true;
            if (NamespaceUri == namespaceUri)
                return true;
            if (string.IsNullOrEmpty(NamespaceUri) && string.IsNullOrEmpty(namespaceUri))
                return true;
            return false;
        }
    }

    /// <summary>
    /// Represents a single <c>xsl:expose</c> rule from an <c>xsl:package</c> root.
    /// </summary>
    private sealed class ExposeRule
    {
        public string Component { get; }
        public string Visibility { get; }
        public IReadOnlyList<ExposeName> Names { get; }
        public bool IsWildcard => Names.Count == 1 && Names[0].IsWildcard;

        public ExposeRule(string component, string visibility, IEnumerable<ExposeName> names)
        {
            Component = component;
            Visibility = visibility;
            Names = names.ToList();
        }

        public bool Matches(string componentType, string? localName, string? namespaceUri, int arity)
        {
            if (Component != componentType && Component != "*")
                return false;
            if (IsWildcard)
                return true;
            foreach (var name in Names)
            {
                if (name.LocalName != localName && name.LocalName != "*")
                    continue;
                if (name.Arity >= 0 && name.Arity != arity)
                    continue;
                if (name.IsNamespaceWildcard)
                    return true;
                if (name.LocalName == "*")
                {
                    if (name.NamespaceUri == namespaceUri)
                        return true;
                    if (string.IsNullOrEmpty(name.NamespaceUri) && string.IsNullOrEmpty(namespaceUri))
                        return true;
                    continue;
                }
                if (name.NamespaceUri == namespaceUri)
                    return true;
                if (string.IsNullOrEmpty(name.NamespaceUri) && string.IsNullOrEmpty(namespaceUri))
                    return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Represents one name entry in an <c>xsl:expose</c> <c>names</c> attribute,
    /// optionally carrying a required function arity.
    /// </summary>
    private readonly struct ExposeName
    {
        public string? NamespaceUri { get; }
        public string LocalName { get; }
        public int Arity { get; }
        public bool IsWildcard { get; }
        public bool IsNamespaceWildcard => NamespaceUri == "*" && !IsWildcard;

        public ExposeName(string? namespaceUri, string localName, int arity = -1, bool isWildcard = false)
        {
            NamespaceUri = namespaceUri;
            LocalName = localName;
            Arity = arity;
            IsWildcard = isWildcard;
        }

        /// <summary>
        /// Returns whether this name pattern matches the supplied component identity.
        /// A <c>*</c> in either the namespace URI or local name position acts as a wildcard.
        /// </summary>
        public bool Matches(string? localName, string? namespaceUri, int arity)
        {
            if (LocalName != "*" && LocalName != localName)
                return false;
            if (Arity >= 0 && Arity != arity)
                return false;
            if (NamespaceUri == "*")
                return true;
            if (NamespaceUri == namespaceUri)
                return true;
            if (string.IsNullOrEmpty(NamespaceUri) && string.IsNullOrEmpty(namespaceUri))
                return true;
            return false;
        }
    }

    private void ResolveImport(XElement importElement, string href)
    {
        // Relative hrefs resolve against the base URI of the xsl:import element,
        // which may differ from the module base URI when the element arrived via
        // external-entity expansion (include-0101: import inside a DTD entity).
        var elementBaseUri = GetEffectiveBaseUri(importElement);
        var resolvedUri = ResolveAbsoluteUri(href, elementBaseUri);

        if (_resolvedUris.Contains(resolvedUri))
            throw new InvalidOperationException($"Circular stylesheet reference detected: {resolvedUri}");

        var childResolvedUris = new HashSet<string>(_resolvedUris) { resolvedUri };

        try
        {
            var doc = _resolver.Resolve(href, elementBaseUri);
            var (moduleDoc, moduleBaseUri) = ExtractModuleDocument(doc, href, resolvedUri);
            var root = moduleDoc.Root;
            // use-when on the root element of an imported module excludes the whole module.
            if (root != null && !UseWhen(root, moduleBaseUri))
                return;
            var child = new Stylesheet(moduleDoc, moduleBaseUri, _resolver, ImportPrecedence + 1, childResolvedUris, null, _externalStaticParameters, _rootStylesheet, this.OwningPackage, _packageVersionResolutionStrategy, isPrincipalLevel: false);
            child.ApplyImportsContextModule = child;
            _imports.Add(child);
            importElement.AddAnnotation(new ResolvedModuleAnnotation { Module = child });
            MergeChildStaticContext(child._staticContext);
        }
        catch (FileNotFoundException ex)
        {
            throw new InvalidOperationException($"XTSE0165: Failed to resolve xsl:import href '{href}'.", ex);
        }
    }

    private void ResolveInclude(XElement includeElement, string href)
    {
        // See ResolveImport: resolve against the element's base URI so includes
        // pulled in through external entities resolve relative to the entity.
        var elementBaseUri = GetEffectiveBaseUri(includeElement);
        var resolvedUri = ResolveAbsoluteUri(href, elementBaseUri);

        // Circular reference detection: if this URI is already in the ancestor chain,
        // including it would create a cycle.
        if (_resolvedUris.Contains(resolvedUri))
            throw new InvalidOperationException($"Circular stylesheet reference detected: {resolvedUri}");

        var childResolvedUris = new HashSet<string>(_resolvedUris) { resolvedUri };

        try
        {
            var doc = _resolver.Resolve(href, elementBaseUri);
            var (moduleDoc, moduleBaseUri) = ExtractModuleDocument(doc, href, resolvedUri);
            var root = moduleDoc.Root;
            // use-when on the root element of an included module excludes the whole module.
            if (root != null && !UseWhen(root, moduleBaseUri))
                return;
            var child = new Stylesheet(moduleDoc, moduleBaseUri, _resolver, ImportPrecedence, childResolvedUris, _staticContext, _externalStaticParameters, _rootStylesheet, this.OwningPackage, _packageVersionResolutionStrategy, isPrincipalLevel: this.IsPrincipalLevel);
            child.ApplyImportsContextModule = ApplyImportsContextModule;
            _includes.Add(child);
            includeElement.AddAnnotation(new ResolvedModuleAnnotation { Module = child });
            MergeChildStaticContext(child._staticContext);
        }
        catch (FileNotFoundException ex)
        {
            throw new InvalidOperationException($"XTSE0165: Failed to resolve xsl:include href '{href}'.", ex);
        }
    }

    /// <summary>
    /// Resolves an <c>xsl:use-package</c> declaration to a registered package, loads it,
    /// and adds it as a used package. The package components are later merged with the
    /// visibility/expose/accept/override rules applied.
    /// </summary>
    private void ResolveUsePackage(XElement usePackageElement, string name, string? packageVersion)
    {
        var versionRange = string.IsNullOrWhiteSpace(packageVersion) ? "*" : packageVersion.Trim();
        var location = Api.XsltFunctionLibrary.ResolvePackageLocation(name, versionRange, _packageVersionResolutionStrategy);
        if (location == null)
            throw new InvalidOperationException($"XTSE0165: Package '{name}' with version '{versionRange}' is not available.");

        try
        {
            // Load the package document using the stylesheet resolver. The registry stores
            // an absolute URI (typically file://), so no base URI resolution is needed.
            var doc = _resolver.Resolve(location, null);
            var root = doc.Root;
            if (root == null)
                throw new InvalidOperationException($"XTSE0165: Package '{name}' has no root element.");

            if (root.Name.NamespaceName != XslNamespace || root.Name.LocalName != "package")
            {
                // A package used via xsl:use-package must have an xsl:package root element.
                // Stylesheets/transforms are not usable as packages.
                throw new InvalidOperationException($"XTSE0165: Package '{name}' is not a valid xsl:package document.");
            }

            // use-when on the root element of the used package excludes the whole package.
            if (!UseWhen(root, location))
                return;

            var child = new Stylesheet(doc, location, _resolver, ImportPrecedence + 1, _resolvedUris, null, _externalStaticParameters, _rootStylesheet, this.OwningPackage, _packageVersionResolutionStrategy);
            child.ApplyImportsContextModule = child;
            _usedPackages.Add(child);
            usePackageElement.AddAnnotation(new ResolvedModuleAnnotation { Module = child });
        }
        catch (FileNotFoundException ex)
        {
            throw new InvalidOperationException($"XTSE0165: Failed to load package '{name}' from '{location}'.", ex);
        }
    }

    /// <summary>
    /// Registers an <c>xsl:use-package</c> relationship in which <paramref name="user"/> uses this
    /// package with overriding variable, parameter, template, or function declarations. The
    /// overrides become visible to this package's own components when they execute
    /// (XSLT 3.0 §3.5.7.2).
    /// </summary>
    private void RegisterPackageOverrideContribution(Stylesheet user, PackageUseOptions options)
    {
        if (options.OverrideFunctions.Count == 0 &&
            options.OverrideVariables.Count == 0 &&
            options.OverrideParams.Count == 0 &&
            options.OverrideTemplates.Count == 0 &&
            options.OverrideAttributeSets.Count == 0)
            return;
        (_packageOverrideContributions ??= new()).Add((user, options));
    }

    /// <summary>
    /// Parses the <c>xsl:accept</c> and <c>xsl:override</c> children of an
    /// <c>xsl:use-package</c> element and stores them as an annotation.
    /// </summary>
    private PackageUseOptions ParsePackageUseOptions(XElement usePackageElement)
    {
        var options = new PackageUseOptions();
        foreach (var child in usePackageElement.Elements())
        {
            if (child.Name.NamespaceName != XslNamespace)
                continue;
            var localName = child.Name.LocalName;
            if (localName == "accept")
            {
                var component = child.Attribute("component")?.Value?.Trim() ?? "";
                var visibility = child.Attribute("visibility")?.Value?.Trim()?.ToLowerInvariant() ?? "public";
                var namesAttr = child.Attribute("names")?.Value?.Trim() ?? "*";

                if (string.IsNullOrEmpty(component))
                    throw new InvalidOperationException("XTSE0010: xsl:accept must have a component attribute.");
                if (string.IsNullOrEmpty(visibility))
                    throw new InvalidOperationException("XTSE0010: xsl:accept must have a visibility attribute.");

                if (visibility is not "public" and not "private" and not "final" and not "abstract" and not "hidden")
                    throw new InvalidOperationException($"XTSE0020: Invalid visibility '{visibility}' for xsl:accept.");

                var validAcceptComponents = new HashSet<string>(StringComparer.Ordinal)
                {
                    "template", "function", "variable", "mode", "attribute-set",
                    "key", "decimal-format", "namespace-alias", "character-map",
                    "output", "strip-space", "preserve-space", "global-context-item", "*"
                };
                if (!validAcceptComponents.Contains(component))
                    throw new InvalidOperationException($"XTSE0020: Invalid component '{component}' for xsl:accept.");

                var names = ParseAcceptNames(usePackageElement, namesAttr, component).ToList();
                if (component == "*" && !names.All(n => n.IsWildcard || n.IsNamespaceWildcard || n.LocalName == "*"))
                    throw new InvalidOperationException("XTSE3032: xsl:accept with component=\"*\" must use wildcard names.");

                options.AcceptRules.Add(new AcceptRule(component, visibility, names));
            }
            else if (localName == "override")
            {
                // XTSE0010: only template, function, variable, param and attribute-set
                // declarations are permitted inside xsl:override (XSLT 3.0 §3.5.7.2).
                // Non-whitespace text, literal result elements and other XSLT declarations
                // (xsl:mode, xsl:key, xsl:accumulator, xsl:decimal-format, nested
                // xsl:override, ...) are static errors.
                foreach (var node in child.Nodes())
                {
                    if (node is XText text && !string.IsNullOrWhiteSpace(text.Value))
                        throw new InvalidOperationException("XTSE0010: Text is not permitted as a child of xsl:override.");
                }
                foreach (var overrideChild in child.Elements())
                {
                    if (overrideChild.Name.NamespaceName != XslNamespace)
                        throw new InvalidOperationException($"XTSE0010: Element '{overrideChild.Name.LocalName}' is not permitted as a child of xsl:override.");
                    switch (overrideChild.Name.LocalName)
                    {
                        case "template": options.OverrideTemplates.Add(overrideChild); break;
                        case "function": options.OverrideFunctions.Add(overrideChild); break;
                        case "variable": options.OverrideVariables.Add(overrideChild); break;
                        case "param": options.OverrideParams.Add(overrideChild); break;
                        case "attribute-set": options.OverrideAttributeSets.Add(overrideChild); break;
                        default:
                            throw new InvalidOperationException($"XTSE0010: xsl:{overrideChild.Name.LocalName} is not permitted as a child of xsl:override.");
                    }
                }
            }
        }
        usePackageElement.AddAnnotation(options);
        return options;
    }

    /// <summary>
    /// Parses the <c>names</c> attribute of an <c>xsl:accept</c> element into a list of
    /// <see cref="AcceptName"/> values. The wildcard <c>*</c> is represented as
    /// <see cref="AcceptName.IsWildcard"/>; <c>*:local</c> is represented with
    /// <see cref="AcceptName.IsNamespaceWildcard"/>.
    /// </summary>
    private static IEnumerable<AcceptName> ParseAcceptNames(XElement context, string names, string component)
    {
        foreach (var token in names.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var t = token.Trim();
            if (string.IsNullOrEmpty(t) || t == "*")
            {
                yield return new AcceptName(null, "*", isWildcard: true);
                yield break;
            }
            if (t.Length > 2 && t[0] == 'Q' && t[1] == '{' && t.IndexOf('}') is int close && close > 1)
            {
                var ns = t[2..close];
                var loc = t[(close + 1)..];
                var (fnLocal, arity) = component == "function" ? ParseFunctionNameWithArity(loc) : (loc, -1);
                yield return new AcceptName(ns, fnLocal, arity);
            }
            else if (t.StartsWith("*:"))
            {
                var local = t[2..];
                var (fnLocal, arity) = component == "function" ? ParseFunctionNameWithArity(local) : (local, -1);
                yield return new AcceptName("*", fnLocal, arity);
            }
            else
            {
                (string loc, string ns) expanded;
                try
                {
                    expanded = ExpandVariableName(context, t);
                }
                catch (InvalidOperationException ex) when (ex.Message.StartsWith("XPST0081"))
                {
                    throw new InvalidOperationException($"XTSE0020: Invalid name token '{t}' in xsl:accept/@names: {ex.Message}");
                }
                var (fnLocal, arity) = component == "function" ? ParseFunctionNameWithArity(expanded.loc) : (expanded.loc, -1);
                yield return new AcceptName(expanded.ns, fnLocal, arity);
            }
        }
    }

    /// <summary>
    /// Parses an <c>xsl:expose</c> element into an <see cref="ExposeRule"/>.
    /// Only permitted as a child of an <c>xsl:package</c> root; otherwise raises
    /// <c>XTSE0010</c>.
    /// </summary>
    private ExposeRule ParseExposeRule(XElement exposeElement)
    {
        if (!IsPackage)
            throw new InvalidOperationException("XTSE0010: xsl:expose is only allowed as a child of xsl:package.");

        var component = exposeElement.Attribute("component")?.Value?.Trim() ?? "";
        var visibility = exposeElement.Attribute("visibility")?.Value?.Trim()?.ToLowerInvariant() ?? "";
        var namesAttr = exposeElement.Attribute("names")?.Value?.Trim() ?? "";

        if (string.IsNullOrEmpty(component))
            throw new InvalidOperationException("XTSE0010: xsl:expose must have a component attribute.");
        if (string.IsNullOrEmpty(visibility))
            throw new InvalidOperationException("XTSE0010: xsl:expose must have a visibility attribute.");
        if (string.IsNullOrEmpty(namesAttr))
            throw new InvalidOperationException("XTSE0010: xsl:expose must have a names attribute.");

        if (visibility is not "public" and not "private" and not "final" and not "abstract")
            throw new InvalidOperationException($"XTSE0020: Invalid visibility '{visibility}' for xsl:expose.");

        var validComponents = new HashSet<string>(StringComparer.Ordinal)
        {
            "template", "function", "variable", "mode", "attribute-set",
            "key", "decimal-format", "namespace-alias", "character-map",
            "output", "strip-space", "preserve-space", "global-context-item", "*"
        };
        if (!validComponents.Contains(component))
            throw new InvalidOperationException($"XTSE0020: Invalid component '{component}' for xsl:expose.");

        var names = ParseExposeNames(exposeElement, namesAttr, component).ToList();
        if (names.Count == 0)
            throw new InvalidOperationException("XTSE0020: xsl:expose/@names is empty.");

        if (component == "*" && names.Any(n => !n.IsWildcard && n.NamespaceUri != "*" && n.LocalName != "*"))
            throw new InvalidOperationException("XTSE3022: xsl:expose with component=\"*\" must use wildcard names.");

        return new ExposeRule(component, visibility, names);
    }

    /// <summary>
    /// Parses the <c>names</c> attribute of an <c>xsl:expose</c> element. Function
    /// names may carry a <c>#arity</c> suffix; mode and other component names use
    /// ordinary lexical or Clark QNames. Special mode tokens such as <c>#unnamed</c>
    /// are not permitted here and raise <c>XTSE0020</c>.
    /// </summary>
    private static IEnumerable<ExposeName> ParseExposeNames(XElement context, string names, string component)
    {
        foreach (var token in names.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var t = token.Trim();
            if (string.IsNullOrEmpty(t))
                continue;
            if (t == "*")
            {
                yield return new ExposeName(null, "*", isWildcard: true);
                yield break;
            }
            if (t.StartsWith("#"))
                throw new InvalidOperationException($"XTSE0020: Invalid name token '{t}' in xsl:expose/@names.");
            if (t.StartsWith("*:"))
            {
                var local = t[2..];
                if (component == "function")
                {
                    var (fnLocal, arity) = ParseFunctionNameWithArity(local);
                    yield return new ExposeName("*", fnLocal, arity);
                }
                else
                {
                    yield return new ExposeName("*", local);
                }
                continue;
            }
            if (t.Length > 2 && t[0] == 'Q' && t[1] == '{' && t.IndexOf('}') is int close && close > 1)
            {
                var ns = t[2..close];
                var loc = t[(close + 1)..];
                if (component == "function")
                {
                    var (fnLocal, arity) = ParseFunctionNameWithArity(loc);
                    yield return new ExposeName(ns, fnLocal, arity);
                }
                else
                {
                    yield return new ExposeName(ns, loc);
                }
                continue;
            }

            (string localName, string nsUri) expandedName;
            try
            {
                expandedName = ExpandVariableName(context, t);
            }
            catch (InvalidOperationException ex) when (ex.Message.StartsWith("XPST0081"))
            {
                throw new InvalidOperationException($"XTSE0020: Invalid name token '{t}' in xsl:expose/@names: {ex.Message}");
            }
            var localName = expandedName.localName;
            var nsUri = expandedName.nsUri;
            if (component == "function")
            {
                var (fnLocal, arity) = ParseFunctionNameWithArity(localName);
                yield return new ExposeName(nsUri, fnLocal, arity);
            }
            else if (component is "template" or "variable")
            {
                yield return new ExposeName(nsUri, localName);
            }
            else
            {
                // Mode, attribute-set, key, decimal-format, etc. do not use the
                // default namespace for unprefixed names. Prefixed names keep their
                // resolved namespace; namespace-wildcard forms (*:local, prefix:*) are
                // preserved as such.
                if (t.Contains(':'))
                    yield return new ExposeName(nsUri, localName);
                else
                    yield return new ExposeName("", localName);
            }
        }
    }

    private static (string LocalName, int Arity) ParseFunctionNameWithArity(string localName)
    {
        var hash = localName.LastIndexOf('#');
        if (hash < 0)
            return (localName, -1);
        var name = localName[..hash];
        var arityStr = localName[(hash + 1)..];
        if (int.TryParse(arityStr, NumberStyles.None, CultureInfo.InvariantCulture, out var arity) && arity >= 0 && name.Length > 0)
            return (name, arity);
        return (localName, -1);
    }

    /// <summary>
    /// Returns the visibility assigned by the package's <c>xsl:expose</c> rules for
    /// the supplied component. Returns <c>null</c> when no rule matches, which means
    /// the component keeps its declared visibility.
    /// </summary>
    internal string? GetExposedVisibility(string componentType, string? localName, string? namespaceUri, int arity = -1)
        => GetExposedVisibility(componentType, localName, namespaceUri, arity, hasDeclaredVisibility: false);

    /// <summary>
    /// Returns the visibility assigned by the package's <c>xsl:expose</c> rules for the
    /// supplied component, following the precedence of XSLT 3.0 §3.5.5.2: an explicit
    /// (named) rule wins; a component with a declared visibility attribute
    /// (<paramref name="hasDeclaredVisibility"/>) ignores wildcard rules; otherwise the
    /// last matching partial wildcard beats the last matching full wildcard. Returns
    /// <c>null</c> when no applicable rule matches, meaning the declared visibility stands.
    /// </summary>
    internal string? GetExposedVisibility(string componentType, string? localName, string? namespaceUri, int arity, bool hasDeclaredVisibility)
    {
        if (!IsPackage)
            return null;
        string? namedResult = null;
        string? partialWildcardResult = null;
        string? fullWildcardResult = null;
        foreach (var rule in _exposeRules)
        {
            if (rule.Component != componentType && rule.Component != "*")
                continue;
            foreach (var name in rule.Names)
            {
                bool isFullWildcard = name.IsWildcard || (name.LocalName == "*" && name.NamespaceUri is null);
                if (!isFullWildcard && !name.Matches(localName, namespaceUri, arity))
                    continue;
                bool isPartialWildcard = !isFullWildcard && (name.IsNamespaceWildcard || name.LocalName == "*");
                if (isFullWildcard)
                    fullWildcardResult = rule.Visibility;
                else if (isPartialWildcard)
                    partialWildcardResult = rule.Visibility;
                else
                    namedResult = rule.Visibility;
            }
        }
        if (namedResult != null)
            return namedResult;
        // A declared visibility attribute takes precedence over wildcard expose rules.
        if (hasDeclaredVisibility)
            return null;
        return partialWildcardResult ?? fullWildcardResult;
    }

    /// <summary>
    /// Validates every <c>xsl:expose</c> rule against the components actually declared
    /// in this package. Raises <c>XTSE3020</c> for undeclared named components,
    /// <c>XTSE3010</c> for visibility changes that are not permitted, and
    /// <c>XTSE3025</c> for wildcard/abstract rules that match non-abstract components.
    /// </summary>
    private void ValidateExposeRules()
    {
        if (!IsPackage || _exposeRules.Count == 0)
            return;

        foreach (var rule in _exposeRules)
        {
            if (rule.IsWildcard)
            {
                ValidateWildcardExposeRule(rule);
                continue;
            }

            foreach (var name in rule.Names)
            {
                ValidateNamedExposeRule(rule, name);
            }
        }
    }

    private void ValidateWildcardExposeRule(ExposeRule rule)
    {
        if (rule.Visibility == "abstract")
        {
            // A wildcard rule with visibility="abstract" is only valid if every
            // matching component is itself abstract.
            foreach (var component in GetAllExposableComponents(rule.Component))
            {
                if (component.DeclaredVisibility != "abstract")
                    throw new InvalidOperationException($"XTSE3025: xsl:expose with visibility='abstract' matches a non-abstract {component.ComponentType} component.");
            }
        }
        else if (rule.Visibility is "public" or "final")
        {
            // An explicit private component cannot be made public or final by a wildcard rule.
            foreach (var component in GetAllExposableComponents(rule.Component))
            {
                if (component.IsExplicit && component.DeclaredVisibility == "private")
                    throw new InvalidOperationException($"XTSE3010: xsl:expose with visibility='{rule.Visibility}' matches an explicitly private {component.ComponentType} component.");
            }
        }
    }

    private void ValidateNamedExposeRule(ExposeRule rule, ExposeName name)
    {
        var componentType = rule.Component;

        // Partial wildcards (namespace="*" or local-name="*") match multiple components;
        // validate them against every matching declaration rather than a single name.
        if (name.NamespaceUri == "*" || name.LocalName == "*")
        {
            foreach (var matching in GetAllExposableComponents(componentType))
            {
                if (!name.Matches(matching.LocalName, matching.NamespaceUri, matching.Arity))
                    continue;

                if (rule.Visibility == "abstract")
                {
                    if (matching.DeclaredVisibility != "abstract")
                        throw new InvalidOperationException($"XTSE3025: xsl:expose with visibility='abstract' matches a non-abstract {matching.ComponentType} component.");
                }
                else if (rule.Visibility is "public" or "final")
                {
                    if (matching.IsExplicit && matching.DeclaredVisibility == "private")
                        throw new InvalidOperationException($"XTSE3010: xsl:expose with visibility='{rule.Visibility}' matches an explicitly private {matching.ComponentType} component.");
                }
            }
            return;
        }

        if (componentType == "function" && name.Arity < 0 && rule.Visibility is "public" or "final")
            throw new InvalidOperationException($"XTSE3020: Function name '{name.LocalName}' in xsl:expose must include an arity.");

        var component = FindExposedComponent(componentType, name);
        if (component == null)
            throw new InvalidOperationException($"XTSE3020: {ComponentDisplayName(componentType, name)} is not declared in the package.");

        var (declaredVisibility, isExplicit, _) = component.Value;
        var exposed = rule.Visibility;

        if (exposed == "abstract")
        {
            if (declaredVisibility == "abstract")
                return;
            if (isExplicit)
                throw new InvalidOperationException($"XTSE3010: Cannot expose {ComponentDisplayName(componentType, name)} as abstract because its declared visibility is '{declaredVisibility}'.");
            throw new InvalidOperationException($"XTSE3025: Cannot expose {ComponentDisplayName(componentType, name)} as abstract because it has no declared visibility.");
        }

        if (declaredVisibility == "abstract")
            throw new InvalidOperationException($"XTSE3010: Cannot change visibility of abstract {ComponentDisplayName(componentType, name)} to '{exposed}'.");

        if (exposed == "public")
        {
            if (isExplicit && declaredVisibility is "private" or "final")
                throw new InvalidOperationException($"XTSE3010: Cannot expose {ComponentDisplayName(componentType, name)} as public because its declared visibility is '{declaredVisibility}'.");
        }
        else if (exposed == "final")
        {
            if (isExplicit && declaredVisibility == "private")
                throw new InvalidOperationException($"XTSE3010: Cannot expose {ComponentDisplayName(componentType, name)} as final because its declared visibility is private.");
        }
    }

    private static string ComponentDisplayName(string componentType, ExposeName name)
    {
        var display = string.IsNullOrEmpty(name.NamespaceUri) || name.NamespaceUri == "*"
            ? name.LocalName
            : $"{{{name.NamespaceUri}}}{name.LocalName}";
        if (componentType == "function" && name.Arity >= 0)
            display += $"#{name.Arity}";
        return $"{componentType} '{display}'";
    }

    /// <summary>
    /// Validates every <c>xsl:accept</c> rule against the components declared in the
    /// corresponding used package. Raises <c>XTSE3030</c> for undeclared named components,
    /// <c>XTSE3040</c> for visibility increases that are not permitted, <c>XTSE3051</c> for
    /// accept tokens that also match an <c>xsl:override</c> declaration, and
    /// <c>XTSE3050</c>/<c>XTSE3080</c> for abstract visibility mismatches.
    /// </summary>
    private void ValidateAcceptRules()
    {
        foreach (var (package, options) in _usedPackageOptions)
        {
            if (options == null)
                continue;
            ValidateAcceptRulesForPackage(package, options);
        }
    }

    private void ValidateAcceptRulesForPackage(Stylesheet package, PackageUseOptions options)
    {
        // Validate the effective accept rule for each component exported by the used package.
        // Later/more-specific rules override earlier/more-generic ones, and only the
        // effective rule is checked for visibility compatibility. Wildcard-matched rules
        // whose combination is not permitted for a component were already treated as not
        // matching during effective-rule selection; explicitly named incompatibilities
        // (including rules naming private components) are reported here.
        foreach (var component in package.GetAllExposableComponents("*"))
        {
            var exposed = package.GetExposedVisibility(component.ComponentType, component.LocalName, component.NamespaceUri, component.Arity);
            var baseVisibility = exposed ?? component.DeclaredVisibility;

            var effectiveRule = GetEffectiveAcceptRule(options, component.ComponentType, component.LocalName, component.NamespaceUri, component.Arity, baseVisibility);
            if (effectiveRule != null)
                ValidateAcceptVisibilityCompatibility(package, component.ComponentType, baseVisibility, effectiveRule.Visibility, component.LocalName, component.NamespaceUri, component.Arity);
        }

        // For non-wildcard named accept rules, verify that each name refers to at least
        // one declared component in the used package.
        foreach (var rule in options.AcceptRules)
        {
            if (IsWildcardAcceptRule(rule))
                continue;

            var componentType = rule.Component;
            if (componentType == "*")
                continue;

            foreach (var name in rule.Names)
            {
                if (name.IsWildcard || name.IsNamespaceWildcard || name.LocalName == "*")
                    continue;

                var exposeName = new ExposeName(name.NamespaceUri, name.LocalName, name.Arity);
                if (package.FindExposedComponent(componentType, exposeName) != null)
                    continue;

                // For functions without arity, accept any function with that name.
                if (componentType == "function" && name.Arity < 0)
                {
                    var functions = new List<XsltFunctionDefinition>();
                    package.CollectFunctions(name.LocalName, name.NamespaceUri, functions);
                    if (functions.Count > 0)
                        continue;
                }

                throw new InvalidOperationException($"XTSE3030: {ComponentDisplayName(componentType, exposeName)} is not declared in the used package.");
            }
        }

        // A non-wildcard accept token must not name a component that is also declared
        // within an xsl:override child of the same xsl:use-package element.
        ValidateAcceptOverrideOverlap(options);
    }

    /// <summary>
    /// Validates that no non-wildcard token in an <c>xsl:accept</c> names attribute matches
    /// the symbolic name of a component declared within an <c>xsl:override</c> child of the
    /// same <c>xsl:use-package</c> element (XSLT 3.0 §3.5.6.1). Raises <c>XTSE3051</c> when
    /// such an overlap is detected.
    /// </summary>
    private void ValidateAcceptOverrideOverlap(PackageUseOptions options)
    {
        foreach (var rule in options.AcceptRules)
        {
            if (IsWildcardAcceptRule(rule))
                continue;

            foreach (var name in rule.Names)
            {
                if (name.IsWildcard || name.IsNamespaceWildcard || name.LocalName == "*")
                    continue;

                if (AcceptNameMatchesOverride(options, rule.Component, name, out var matchedKind))
                {
                    var displayName = new ExposeName(name.NamespaceUri, name.LocalName, name.Arity);
                    throw new InvalidOperationException($"XTSE3051: {ComponentDisplayName(matchedKind, displayName)} is declared within an xsl:override child of the same xsl:use-package element.");
                }
            }
        }
    }

    /// <summary>
    /// Returns true when a fully qualified accept token matches the symbolic name of an
    /// <c>xsl:override</c> declaration of a compatible component kind. A function token
    /// without an arity matches overriding functions of any arity.
    /// </summary>
    private static bool AcceptNameMatchesOverride(PackageUseOptions options, string componentType, AcceptName name, out string matchedKind)
    {
        matchedKind = "";

        bool NameMatches(XElement element)
        {
            var nameAttr = element.Attribute("name")?.Value;
            if (string.IsNullOrEmpty(nameAttr))
                return false;
            var (local, ns) = ExpandVariableName(element, nameAttr);
            return local == name.LocalName && string.Equals(ns ?? "", name.NamespaceUri ?? "", StringComparison.Ordinal);
        }

        if ((componentType == "template" || componentType == "*") && options.OverrideTemplates.Any(NameMatches))
        {
            matchedKind = "template";
            return true;
        }

        if (componentType == "function" || componentType == "*")
        {
            foreach (var element in options.OverrideFunctions)
            {
                if (!NameMatches(element))
                    continue;
                if (name.Arity >= 0 && name.Arity != element.Elements(XName.Get("param", XslNamespace)).Count())
                    continue;
                matchedKind = "function";
                return true;
            }
        }

        if ((componentType == "variable" || componentType == "*") &&
            (options.OverrideVariables.Any(NameMatches) || options.OverrideParams.Any(NameMatches)))
        {
            matchedKind = "variable";
            return true;
        }

        if ((componentType == "attribute-set" || componentType == "*") && options.OverrideAttributeSets.Any(NameMatches))
        {
            matchedKind = "attribute-set";
            return true;
        }

        if (componentType == "mode" || componentType == "*")
        {
            var modeName = string.IsNullOrEmpty(name.NamespaceUri) ? name.LocalName : $"{{{name.NamespaceUri}}}{name.LocalName}";
            foreach (var element in options.OverrideModes)
            {
                var nameAttr = element.Attribute("name")?.Value;
                if (!string.IsNullOrEmpty(nameAttr) && ExpandModeNameForExpose(nameAttr, element) == modeName)
                {
                    matchedKind = "mode";
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Returns true when the accept rule relies solely on wildcards and does not need
    /// a name-existence check.
    /// </summary>
    private static bool IsWildcardAcceptRule(AcceptRule rule)
    {
        if (rule.IsWildcard)
            return true;
        return rule.Names.All(n => n.IsWildcard || n.IsNamespaceWildcard || n.LocalName == "*");
    }

    /// <summary>
    /// Returns true when the <c>xsl:accept</c> visibility combination is permitted by the
    /// XSLT 3.0 §3.5.6.1 table: <c>public</c> requires base <c>public</c>; <c>final</c> and
    /// <c>private</c> require base <c>public</c>/<c>final</c>; <c>abstract</c> requires base
    /// <c>abstract</c>; <c>hidden</c> requires base <c>public</c>/<c>final</c>/<c>abstract</c>.
    /// Combinations with a <c>private</c> base are never permitted.
    /// </summary>
    private static bool IsAcceptCombinationPermitted(string acceptVisibility, string baseVisibility)
        => acceptVisibility switch
        {
            "public" => baseVisibility == "public",
            "final" => baseVisibility is "public" or "final",
            "private" => baseVisibility is "public" or "final",
            "abstract" => baseVisibility == "abstract",
            "hidden" => baseVisibility is "public" or "final" or "abstract",
            _ => false
        };

    /// <summary>
    /// Returns the most specific accept rule that matches the supplied component, or null
    /// if no accept rule matches. Specificity is determined first by the name pattern,
    /// then by the component type, then by document order (later wins). When
    /// <paramref name="baseVisibility"/> is supplied, a rule whose visibility combination is
    /// not permitted for the component is treated as not matching when the matching token is
    /// a wildcard (XSLT 3.0 §3.5.6.1); explicitly named matches remain effective so the
    /// incompatibility can be reported as XTSE3040.
    /// </summary>
    private static AcceptRule? GetEffectiveAcceptRule(PackageUseOptions options, string componentType, string? localName, string? namespaceUri, int arity, string? baseVisibility = null)
    {
        AcceptRule? bestRule = null;
        int bestNameSpecificity = -1;
        int bestComponentSpecificity = -1;
        int bestIndex = -1;

        int index = 0;
        foreach (var rule in options.AcceptRules)
        {
            if (rule.Component != componentType && rule.Component != "*")
            {
                index++;
                continue;
            }
            if (!rule.Matches(localName, namespaceUri, arity))
            {
                index++;
                continue;
            }

            int nameSpecificity = GetAcceptNameSpecificity(rule, localName, namespaceUri, arity);

            // A wildcard-matched rule whose visibility combination is not permitted for the
            // component is treated as not matching that component (XSLT 3.0 §3.5.6.1).
            if (baseVisibility != null && nameSpecificity < 2 && !IsAcceptCombinationPermitted(rule.Visibility, baseVisibility))
            {
                index++;
                continue;
            }

            int componentSpecificity = rule.Component == componentType ? 1 : 0;

            bool replace = bestRule == null ||
                           nameSpecificity > bestNameSpecificity ||
                           (nameSpecificity == bestNameSpecificity && componentSpecificity > bestComponentSpecificity) ||
                           (nameSpecificity == bestNameSpecificity && componentSpecificity == bestComponentSpecificity && index > bestIndex);

            if (replace)
            {
                bestRule = rule;
                bestNameSpecificity = nameSpecificity;
                bestComponentSpecificity = componentSpecificity;
                bestIndex = index;
            }

            index++;
        }

        return bestRule;
    }

    /// <summary>
    /// Returns the specificity of the most specific matching name pattern within the rule.
    /// A fully wildcard name has specificity 0, a partial wildcard has specificity 1,
    /// and a fully qualified name has specificity 2.
    /// </summary>
    private static int GetAcceptNameSpecificity(AcceptRule rule, string? localName, string? namespaceUri, int arity)
    {
        int best = 0;
        foreach (var name in rule.Names)
        {
            if (!name.Matches(localName, namespaceUri, arity))
                continue;

            int specificity;
            if (name.IsWildcard)
            {
                specificity = 0;
            }
            else
            {
                specificity = (name.LocalName != "*" ? 1 : 0) + (name.NamespaceUri != "*" ? 1 : 0);
            }

            if (specificity > best)
                best = specificity;
        }
        return best;
    }

    /// <summary>
    /// Returns the default visibility of a used-package component when no accept rule
    /// matches. xsl:initial-template defaults to private in the using package. Abstract
    /// components default to hidden: they are only visible in the using package when an
    /// accept rule explicitly accepts them as abstract (XSLT 3.0 §3.5.6.1).
    /// </summary>
    private static string? GetDefaultUsedPackageVisibility(string componentType, string? localName, string? namespaceUri, string? baseVisibility, PackageUseOptions? options = null)
    {
        if (componentType == "template" && localName == "initial-template" && namespaceUri == XslNamespace)
            return "private";
        if (baseVisibility == "abstract")
            return "hidden";
        return baseVisibility;
    }

    private void ValidateAcceptVisibilityCompatibility(Stylesheet package, string componentType, string baseVisibility, string? effectiveVisibility, string? localName, string? namespaceUri, int arity)
    {
        var acceptVisibility = effectiveVisibility ?? baseVisibility;

        // XTSE3040: the visibility assigned by an xsl:accept element must be compatible
        // with the component's visibility in the used package (XSLT 3.0 §3.5.6.1 table).
        // Wildcard-matched incompatible rules were already treated as not matching during
        // effective-rule selection, so only explicitly named incompatibilities reach here.
        if (!IsAcceptCombinationPermitted(acceptVisibility, baseVisibility))
            throw new InvalidOperationException($"XTSE3040: Cannot accept {ComponentDisplayName(componentType, new ExposeName(namespaceUri, localName, arity))} with visibility '{acceptVisibility}' because its visibility in the used package is '{baseVisibility}'.");
    }

    /// <summary>
    /// Detects conflicting visible components exported by multiple used packages when
    /// no <c>xsl:accept</c> rule resolves the conflict. Raises <c>XTSE3050</c>.
    /// </summary>
    private void ValidateUsedPackageConflicts()
    {
        var visibleComponents = new Dictionary<(string ComponentType, string? NamespaceUri, string LocalName, int Arity), List<(Stylesheet Package, string Visibility)>>();

        foreach (var (package, options) in _usedPackageOptions)
        {
            if (options == null)
                continue;
            foreach (var component in package.GetAllExposableComponents("*"))
            {
                // A component with an explicitly declared visibility ignores wildcard
                // xsl:expose rules (XSLT 3.0 §3.5.5.2 precedence): a private-declared
                // component matched only by a wildcard stays private (accept-001).
                var exposed = package.GetExposedVisibility(component.ComponentType, component.LocalName, component.NamespaceUri, component.Arity, component.IsExplicit);
                var baseVisibility = exposed ?? component.DeclaredVisibility;
                var effectiveRule = GetEffectiveAcceptRule(options, component.ComponentType, component.LocalName, component.NamespaceUri, component.Arity, baseVisibility);
                var effectiveVis = effectiveRule?.Visibility ?? GetDefaultUsedPackageVisibility(component.ComponentType, component.LocalName, component.NamespaceUri, baseVisibility, options);
                if (effectiveVis is null || (effectiveVis != "public" && effectiveVis != "final" && effectiveVis != "abstract"))
                    continue;

                var key = (component.ComponentType, component.NamespaceUri, component.LocalName ?? "", component.Arity);
                if (!visibleComponents.TryGetValue(key, out var list))
                    visibleComponents[key] = list = new List<(Stylesheet, string)>();
                list.Add((package, effectiveVis));
            }
        }

        foreach (var (key, list) in visibleComponents)
        {
            // XTSE3050: a component declared at the top level of this package conflicts
            // with a homonymous component accepted from a used package with a visibility
            // other than hidden (such components may only be overridden via xsl:override).
            if (FindLocalComponent(key.ComponentType, key.LocalName, key.NamespaceUri, key.Arity))
                throw new InvalidOperationException($"XTSE3050: Local {key.ComponentType} '{DisplayComponentName(key.LocalName, key.NamespaceUri, key.Arity)}' conflicts with a component accepted from a used package; use xsl:override to override it.");

            if (list.Count < 2)
                continue;

            // Duplicate entries from the same package do not constitute a conflict.
            if (list.Select(x => x.Package).Distinct().Count() == 1)
                continue;

            throw new InvalidOperationException($"XTSE3050: Conflicting visible {key.ComponentType} '{DisplayComponentName(key.LocalName, key.NamespaceUri, key.Arity)}' exported by multiple used packages.");
        }
    }

    /// <summary>
    /// Validates that a top-level (executable) package does not contain components whose
    /// effective visibility is abstract. Per the XSLT 3.0 specification (XTSE3080), it is
    /// an error for the top-level package to contain abstract components whether or not
    /// they are referenced; abstract components are only permitted in library packages.
    /// </summary>
    private void ValidateTopLevelPackageAbstractComponents()
    {
        // Components declared abstract in this package itself.
        foreach (var def in _functionDefinitions)
        {
            if (def.Visibility == "abstract")
                throw new InvalidOperationException($"XTSE3080: Top-level package contains the abstract function '{{{def.NamespaceUri}}}{def.LocalName}#{def.Arity}'.");
        }
        foreach (var rule in _namedTemplates.Values)
        {
            if (GetLocalVisibility(rule.Element, "template", this) == "abstract")
                throw new InvalidOperationException($"XTSE3080: Top-level package contains the abstract template '{rule.Name}'.");
        }
        foreach (var element in _globalVariables.Concat(_globalParameters))
        {
            var nameAttr = element.Attribute("name")?.Value;
            if (GetLocalVisibility(element, "variable", IsPackage) == "abstract")
                throw new InvalidOperationException($"XTSE3080: Top-level package contains the abstract variable '${nameAttr}'.");
        }
        foreach (var def in _attributeSets)
        {
            if (GetLocalVisibility(def.Element, "attribute-set", IsPackage) == "abstract")
                throw new InvalidOperationException($"XTSE3080: Top-level package contains the abstract attribute-set '{def.LocalName}'.");
        }

        // Components accepted as abstract from used packages.
        foreach (var (package, options) in _usedPackageOptions)
        {
            if (options == null)
                continue;
            foreach (var component in package.GetAllExposableComponents("*"))
            {
                var exposed = package.GetExposedVisibility(component.ComponentType, component.LocalName, component.NamespaceUri, component.Arity);
                var baseVisibility = exposed ?? component.DeclaredVisibility;
                var effectiveRule = GetEffectiveAcceptRule(options, component.ComponentType, component.LocalName, component.NamespaceUri, component.Arity, baseVisibility);
                var effectiveVis = effectiveRule?.Visibility ?? GetDefaultUsedPackageVisibility(component.ComponentType, component.LocalName, component.NamespaceUri, baseVisibility, options);
                if (effectiveVis == "abstract")
                    throw new InvalidOperationException($"XTSE3080: Top-level package accepts the abstract {component.ComponentType} '{DisplayComponentName(component.LocalName, component.NamespaceUri, component.Arity)}' from package '{package.Root.Attribute("name")?.Value}'.");
            }
        }
    }

    private bool FindLocalComponent(string componentType, string localName, string? namespaceUri, int arity)
    {
        return componentType switch
        {
            "template" => TryFindNamedTemplate(localName, namespaceUri, out _),
            "function" => TryFindFunction(localName, namespaceUri, arity, out _),
            "variable" => TryGetVariableElement(localName, namespaceUri, out _),
            "attribute-set" => TryGetAttributeSet(localName, namespaceUri, out _),
            "mode" => TryGetMode(string.IsNullOrEmpty(namespaceUri) ? localName : $"{{{namespaceUri}}}{localName}", out _),
            _ => false
        };
    }

    private static string DisplayComponentName(string localName, string? namespaceUri, int arity)
    {
        var display = string.IsNullOrEmpty(namespaceUri) ? localName : $"{{{namespaceUri}}}{localName}";
        if (arity >= 0)
            display += $"#{arity}";
        return display;
    }

    private (string DeclaredVisibility, bool IsExplicit, string ComponentType)? FindExposedComponent(string componentType, ExposeName name)
    {
        return componentType switch
        {
            "template" => FindExposedTemplate(name),
            "function" => FindExposedFunction(name),
            "variable" => FindExposedVariable(name),
            "mode" => FindExposedMode(name),
            "attribute-set" => FindExposedAttributeSet(name),
            "key" => FindExposedKey(name),
            "decimal-format" => FindExposedDecimalFormat(name),
            "namespace-alias" => FindExposedNamespaceAlias(name),
            "character-map" => FindExposedCharacterMap(name),
            "output" => FindExposedOutput(name),
            "strip-space" => FindExposedStripSpace(name),
            "preserve-space" => FindExposedPreserveSpace(name),
            "global-context-item" => FindExposedGlobalContextItem(name),
            _ => null
        };
    }

    private (string, bool, string)? FindExposedTemplate(ExposeName name)
    {
        if (TryFindNamedTemplate(name.LocalName, name.NamespaceUri, out var rule) && rule != null)
        {
            var vis = GetLocalVisibility(rule.Element, "template", this) ?? "private";
            var isExplicit = rule.Element.Attribute("visibility")?.Value is not null;
            return (vis, isExplicit, "template");
        }
        return null;
    }

    private (string, bool, string)? FindExposedFunction(ExposeName name)
    {
        if (name.Arity < 0)
        {
            var functions = new List<XsltFunctionDefinition>();
            CollectFunctions(name.LocalName, name.NamespaceUri, functions);
            if (functions.Count == 0)
                return null;
            var def = functions[0];
            var isExplicit = def.Element.Attribute("visibility")?.Value is not null;
            return (def.Visibility, isExplicit, "function");
        }
        if (TryFindFunction(name.LocalName, name.NamespaceUri, name.Arity, out var foundDef) && foundDef != null)
        {
            var isExplicit = foundDef.Element.Attribute("visibility")?.Value is not null;
            return (foundDef.Visibility, isExplicit, "function");
        }
        return null;
    }

    private (string, bool, string)? FindExposedVariable(ExposeName name)
    {
        if (TryGetVariableElement(name.LocalName, name.NamespaceUri, out var element) && element != null)
        {
            var vis = GetLocalVisibility(element, "variable", IsPackage) ?? "private";
            var isExplicit = element.Attribute("visibility")?.Value is not null;
            return (vis, isExplicit, "variable");
        }
        return null;
    }

    private (string, bool, string)? FindExposedMode(ExposeName name)
    {
        var modeName = string.IsNullOrEmpty(name.NamespaceUri) ? name.LocalName : $"{{{name.NamespaceUri}}}{name.LocalName}";
        if (TryGetMode(modeName, out var mode) && mode != null)
        {
            var vis = mode.Visibility.ToString().ToLowerInvariant();
            var element = FindModeElement(modeName);
            var isExplicit = element?.Attribute("visibility")?.Value is not null;
            return (vis, isExplicit, "mode");
        }

        // A package can re-export a mode accepted from a package it uses.
        foreach (var package in _usedPackages)
        {
            var options = _usedPackageOptions.GetValueOrDefault(package);
            if (options == null)
                continue;
            var effectiveRule = GetEffectiveAcceptRule(options, "mode", name.LocalName, name.NamespaceUri, -1);
            if (effectiveRule?.Visibility is not "public" and not "final")
                continue;
            var baseMode = package.FindExposedMode(name);
            if (baseMode != null)
                return baseMode.Value;
        }
        return null;
    }

    private (string, bool, string)? FindExposedAttributeSet(ExposeName name)
    {
        if (TryGetAttributeSet(name.LocalName, name.NamespaceUri, out var def) && def != null)
        {
            var vis = GetLocalVisibility(def.Element, "attribute-set", IsPackage) ?? "private";
            var isExplicit = def.Element.Attribute("visibility")?.Value is not null;
            return (vis, isExplicit, "attribute-set");
        }
        return null;
    }

    private (string, bool, string)? FindExposedKey(ExposeName name) => null;
    private (string, bool, string)? FindExposedDecimalFormat(ExposeName name) => null;
    private (string, bool, string)? FindExposedNamespaceAlias(ExposeName name) => null;
    private (string, bool, string)? FindExposedCharacterMap(ExposeName name) => null;
    private (string, bool, string)? FindExposedOutput(ExposeName name) => null;
    private (string, bool, string)? FindExposedStripSpace(ExposeName name) => null;
    private (string, bool, string)? FindExposedPreserveSpace(ExposeName name) => null;
    private (string, bool, string)? FindExposedGlobalContextItem(ExposeName name) => null;

    private IEnumerable<(string DeclaredVisibility, bool IsExplicit, string ComponentType, string? LocalName, string? NamespaceUri, int Arity)> GetAllExposableComponents(string componentType)
    {
        if (componentType is "template" or "*")
        {
            foreach (var rule in GetAllNamedTemplatesForExpose())
            {
                if (string.IsNullOrEmpty(rule.Name))
                    continue;
                var (loc, ns) = ExpandVariableName(rule.Element, rule.Name);
                var vis = GetLocalVisibility(rule.Element, "template", this) ?? "private";
                var isExplicit = rule.Element.Attribute("visibility")?.Value is not null;
                yield return (vis, isExplicit, "template", loc, ns, -1);
            }
        }
        if (componentType is "function" or "*")
        {
            foreach (var def in GetAllFunctionDefinitionsForExpose())
            {
                var isExplicit = def.Element.Attribute("visibility")?.Value is not null;
                yield return (def.Visibility, isExplicit, "function", def.LocalName, def.NamespaceUri, def.Arity);
            }
        }
        if (componentType is "variable" or "*")
        {
            foreach (var element in GetAllVariableElementsForExpose())
            {
                var nameAttr = element.Attribute("name")?.Value;
                if (string.IsNullOrEmpty(nameAttr))
                    continue;
                var (loc, ns) = ExpandVariableName(element, nameAttr);
                var vis = GetLocalVisibility(element, "variable", IsPackage) ?? "private";
                var isExplicit = element.Attribute("visibility")?.Value is not null;
                yield return (vis, isExplicit, "variable", loc, ns, -1);
            }
        }
        if (componentType is "mode" or "*")
        {
            foreach (var (mode, element) in GetAllModeDefinitionsForExpose())
            {
                var (modeNs, modeLocal) = SplitModeName(mode.Name);
                var vis = mode.Visibility.ToString().ToLowerInvariant();
                var isExplicit = element?.Attribute("visibility")?.Value is not null;
                yield return (vis, isExplicit, "mode", modeLocal, modeNs, -1);
            }
        }
        if (componentType is "attribute-set" or "*")
        {
            foreach (var def in GetAllAttributeSetDefinitionsForExpose())
            {
                var vis = GetLocalVisibility(def.Element, "attribute-set", IsPackage) ?? "private";
                var isExplicit = def.Element.Attribute("visibility")?.Value is not null;
                yield return (vis, isExplicit, "attribute-set", def.LocalName, def.NamespaceUri, -1);
            }
        }
    }

    /// <summary>
    /// Splits a stored mode name (Clark notation or empty) into namespace URI and local name.
    /// </summary>
    private static (string? NamespaceUri, string LocalName) SplitModeName(string modeName)
    {
        if (string.IsNullOrEmpty(modeName))
            return (null, "");
        if (modeName.Length > 2 && modeName[0] == '{' && modeName.IndexOf('}') is int close && close > 0)
        {
            var ns = modeName[1..close];
            var local = modeName[(close + 1)..];
            return (ns, local);
        }
        return (null, modeName);
    }

    /// <summary>
    /// Finds a named template recursively in this stylesheet, its imports and includes.
    /// </summary>
    private bool TryFindNamedTemplate(string localName, string? namespaceUri, out TemplateRule? rule)
    {
        rule = null;
        foreach (var imported in _imports)
            if (imported.TryFindNamedTemplate(localName, namespaceUri, out rule))
                return true;
        foreach (var included in _includes)
            if (included.TryFindNamedTemplate(localName, namespaceUri, out rule))
                return true;
        foreach (var candidate in _namedTemplates.Values)
        {
            if (string.IsNullOrEmpty(candidate.Name))
                continue;
            var (loc, ns) = ExpandVariableName(candidate.Element, candidate.Name);
            if (loc == localName && ns == (namespaceUri ?? ""))
            {
                rule = candidate;
                return true;
            }
        }
        return rule != null;
    }

    /// <summary>
    /// Finds a named template in this stylesheet's package or, recursively, in the packages
    /// it uses (components accepted transitively are still components of the used package,
    /// XSLT 3.0 §3.5.7.2). Also returns the package that declares the template.
    /// </summary>
    private bool TryFindNamedTemplateDeep(string localName, string? namespaceUri, out TemplateRule? rule, out Stylesheet? declaringPackage)
    {
        if (TryFindNamedTemplate(localName, namespaceUri, out rule))
        {
            declaringPackage = this;
            return true;
        }
        foreach (var used in _usedPackages)
        {
            if (used.TryFindNamedTemplateDeep(localName, namespaceUri, out rule, out declaringPackage))
                return true;
        }
        rule = null;
        declaringPackage = null;
        return false;
    }

    /// <summary>
    /// Finds a function declaration recursively in this stylesheet, its imports and includes.
    /// </summary>
    private bool TryFindFunction(string localName, string? namespaceUri, int arity, out XsltFunctionDefinition? def)
    {
        def = null;
        foreach (var imported in _imports)
            if (imported.TryFindFunction(localName, namespaceUri, arity, out def))
                return true;
        foreach (var included in _includes)
            if (included.TryFindFunction(localName, namespaceUri, arity, out def))
                return true;
        foreach (var candidate in _functionDefinitions)
        {
            if (candidate.LocalName == localName && candidate.NamespaceUri == (namespaceUri ?? "") && candidate.Arity == arity)
            {
                def = candidate;
                return true;
            }
        }
        return def != null;
    }

    private void CollectFunctions(string localName, string? namespaceUri, List<XsltFunctionDefinition> result)
    {
        foreach (var imported in _imports)
            imported.CollectFunctions(localName, namespaceUri, result);
        foreach (var included in _includes)
            included.CollectFunctions(localName, namespaceUri, result);
        foreach (var candidate in _functionDefinitions)
        {
            if (candidate.LocalName == localName && candidate.NamespaceUri == (namespaceUri ?? ""))
                result.Add(candidate);
        }
    }

    /// <summary>
    /// Finds a top-level xsl:variable element recursively in this stylesheet,
    /// its imports and includes. Parameters are not considered variables.
    /// </summary>
    private bool TryGetVariableElement(string localName, string? namespaceUri, out XElement? element)
    {
        element = null;
        foreach (var imported in _imports)
            if (imported.TryGetVariableElement(localName, namespaceUri, out element))
                return true;
        foreach (var included in _includes)
            if (included.TryGetVariableElement(localName, namespaceUri, out element))
                return true;
        foreach (var candidate in _globalVariables)
        {
            var nameAttr = candidate.Attribute("name")?.Value;
            if (string.IsNullOrEmpty(nameAttr))
                continue;
            var (loc, ns) = ExpandVariableName(candidate, nameAttr);
            if (loc == localName && ns == (namespaceUri ?? ""))
            {
                element = candidate;
                return true;
            }
        }
        return element != null;
    }

    /// <summary>
    /// Finds a top-level xsl:variable or xsl:param element recursively in this stylesheet,
    /// its imports and includes. Used by the xsl:override validation, where parameters and
    /// variables share the same symbolic identifier space.
    /// </summary>
    private bool TryGetVariableOrParamElement(string localName, string? namespaceUri, out XElement? element)
    {
        if (TryGetVariableElement(localName, namespaceUri, out element))
            return true;
        element = null;
        foreach (var imported in _imports)
            if (imported.TryGetVariableOrParamElement(localName, namespaceUri, out element))
                return true;
        foreach (var included in _includes)
            if (included.TryGetVariableOrParamElement(localName, namespaceUri, out element))
                return true;
        foreach (var candidate in _globalParameters)
        {
            var nameAttr = candidate.Attribute("name")?.Value;
            if (string.IsNullOrEmpty(nameAttr))
                continue;
            var (loc, ns) = ExpandVariableName(candidate, nameAttr);
            if (loc == localName && ns == (namespaceUri ?? ""))
            {
                element = candidate;
                return true;
            }
        }
        return element != null;
    }

    /// <summary>
    /// Finds a mode definition recursively in this stylesheet, its imports and includes.
    /// </summary>
    private bool TryGetMode(string modeName, out ModeDefinition? mode)
    {
        foreach (var imported in _imports)
            if (imported.TryGetMode(modeName, out mode))
                return true;
        foreach (var included in _includes)
            if (included.TryGetMode(modeName, out mode))
                return true;
        return _modeDefinitions.TryGetValue(modeName, out mode);
    }

    /// <summary>
    /// Finds the original xsl:mode element for a mode name.
    /// </summary>
    private XElement? FindModeElement(string modeName)
    {
        foreach (var imported in _imports)
        {
            var found = imported.FindModeElement(modeName);
            if (found != null)
                return found;
        }
        foreach (var included in _includes)
        {
            var found = included.FindModeElement(modeName);
            if (found != null)
                return found;
        }
        foreach (var mode in _document.Root?.Elements(XName.Get("mode", XslNamespace)) ?? Enumerable.Empty<XElement>())
        {
            var name = ModeDefinition.NormalizeModeName(mode.Attribute("name")?.Value?.Trim() ?? "");
            var expanded = string.IsNullOrEmpty(name) ? "" : ExpandModeNameForExpose(name, mode);
            if (expanded == modeName)
                return mode;
        }
        return null;
    }

    /// <summary>
    /// Finds an attribute-set recursively in this stylesheet, its imports and includes.
    /// </summary>
    private bool TryGetAttributeSet(string localName, string? namespaceUri, out AttributeSetDefinition? def)
    {
        def = null;
        foreach (var imported in _imports)
            if (imported.TryGetAttributeSet(localName, namespaceUri, out def))
                return true;
        foreach (var included in _includes)
            if (included.TryGetAttributeSet(localName, namespaceUri, out def))
                return true;
        foreach (var candidate in _attributeSets)
        {
            if (candidate.LocalName == localName && candidate.NamespaceUri == (namespaceUri ?? ""))
            {
                def = candidate;
                return true;
            }
        }
        return def != null;
    }

    private IEnumerable<TemplateRule> GetAllNamedTemplatesForExpose()
    {
        foreach (var imported in _imports)
            foreach (var rule in imported.GetAllNamedTemplatesForExpose())
                yield return rule;
        foreach (var included in _includes)
            foreach (var rule in included.GetAllNamedTemplatesForExpose())
                yield return rule;
        foreach (var rule in _namedTemplates.Values)
            yield return rule;
    }

    private IEnumerable<XsltFunctionDefinition> GetAllFunctionDefinitionsForExpose()
    {
        foreach (var imported in _imports)
            foreach (var def in imported.GetAllFunctionDefinitionsForExpose())
                yield return def;
        foreach (var included in _includes)
            foreach (var def in included.GetAllFunctionDefinitionsForExpose())
                yield return def;
        foreach (var def in _functionDefinitions)
            yield return def;
    }

    private IEnumerable<XElement> GetAllVariableElementsForExpose()
    {
        foreach (var imported in _imports)
            foreach (var e in imported.GetAllVariableElementsForExpose())
                yield return e;
        foreach (var included in _includes)
            foreach (var e in included.GetAllVariableElementsForExpose())
                yield return e;
        foreach (var e in _globalVariables)
            yield return e;
    }

    private IEnumerable<(ModeDefinition Mode, XElement? Element)> GetAllModeDefinitionsForExpose()
    {
        foreach (var imported in _imports)
            foreach (var pair in imported.GetAllModeDefinitionsForExpose())
                yield return pair;
        foreach (var included in _includes)
            foreach (var pair in included.GetAllModeDefinitionsForExpose())
                yield return pair;
        foreach (var kv in _modeDefinitions)
        {
            var element = FindModeElement(kv.Key);
            yield return (kv.Value, element);
        }
    }

    private IEnumerable<AttributeSetDefinition> GetAllAttributeSetDefinitionsForExpose()
    {
        foreach (var imported in _imports)
            foreach (var def in imported.GetAllAttributeSetDefinitionsForExpose())
                yield return def;
        foreach (var included in _includes)
            foreach (var def in included.GetAllAttributeSetDefinitionsForExpose())
                yield return def;
        foreach (var def in _attributeSets)
            yield return def;
    }

    /// <summary>
    /// Expands a mode name token into Clark notation. Mode names never use the
    /// default namespace, so an unprefixed name is in no namespace.
    /// </summary>
    private static string ExpandModeNameForExpose(string name, XElement context)
    {
        if (name.Length > 2 && name[0] == 'Q' && name[1] == '{')
        {
            int closeBrace = name.IndexOf('}');
            if (closeBrace >= 2)
            {
                var uri = name[2..closeBrace];
                var local = name[(closeBrace + 1)..];
                return string.IsNullOrEmpty(uri) ? local : $"{{{uri}}}{local}";
            }
        }
        int colon = name.IndexOf(':');
        if (colon >= 0)
        {
            var prefix = name[..colon];
            var local = name[(colon + 1)..];
            if (prefix == "xml")
                return $"{{{XNamespace.Xml.NamespaceName}}}{local}";
            var ns = context.GetNamespaceOfPrefix(prefix);
            if (ns == null)
                throw new InvalidOperationException($"XPST0081: Undefined namespace prefix '{prefix}'");
            return $"{{{ns.NamespaceName}}}{local}";
        }
        return name;
    }

    /// <summary>
    /// When <paramref name="href"/> contains a fragment identifier, extracts the
    /// referenced element (by <c>xml:id</c> or plain <c>id</c>) from the resolved
    /// document and returns it as a new document with the source document's base URI.
    /// Otherwise returns the original document unchanged.
    /// </summary>
    private static (XDocument document, string baseUri) ExtractModuleDocument(XDocument doc, string href, string resolvedUri)
    {
        var fragment = GetFragmentIdentifier(href);
        if (fragment == null)
            return (doc, resolvedUri);

        var sourceBaseUri = !string.IsNullOrEmpty(doc.BaseUri)
            ? doc.BaseUri
            : GetUriWithoutFragment(resolvedUri);

        var found = FindElementByFragment(doc, fragment);
        if (found == null)
            throw new InvalidOperationException($"XTSE0165: Fragment identifier '{fragment}' not found in '{href}'.");

        var elementXml = found.ToString(SaveOptions.DisableFormatting);
        var newDoc = Xml11Loader.Parse(
            elementXml,
            LoadOptions.PreserveWhitespace | LoadOptions.SetBaseUri | LoadOptions.SetLineInfo,
            sourceBaseUri);

        return (newDoc, resolvedUri);
    }

    /// <summary>
    /// Returns the fragment identifier from the <paramref name="href"/> if present
    /// and non-empty; otherwise <c>null</c>.
    /// </summary>
    private static string? GetFragmentIdentifier(string href)
    {
        var hashIndex = href.IndexOf('#');
        if (hashIndex < 0 || hashIndex == href.Length - 1)
            return null;
        return href[(hashIndex + 1)..];
    }

    /// <summary>
    /// Returns the URI without its fragment identifier.
    /// </summary>
    private static string GetUriWithoutFragment(string uri)
    {
        var hashIndex = uri.IndexOf('#');
        return hashIndex < 0 ? uri : uri[..hashIndex];
    }

    /// <summary>
    /// Finds the element identified by the given fragment using <c>xml:id</c>
    /// or a plain <c>id</c> attribute.
    /// </summary>
    private static XElement? FindElementByFragment(XDocument doc, string fragment)
    {
        foreach (var element in doc.Descendants())
        {
            var xmlId = (string?)element.Attribute(XNamespace.Xml.GetName("id"));
            if (xmlId == fragment)
                return element;

            var plainId = (string?)element.Attribute("id");
            if (plainId == fragment)
                return element;
        }
        return null;
    }

    private static string ResolveAbsoluteUri(string href, string? baseUri)
    {
        if (string.IsNullOrEmpty(baseUri))
        {
            if (Uri.IsWellFormedUriString(href, UriKind.Absolute))
                return href;
            return Path.GetFullPath(href);
        }

        if (Uri.IsWellFormedUriString(href, UriKind.Absolute))
            return href;

        var baseUriObj = new Uri(baseUri);
        var resolved = new Uri(baseUriObj, href);
        return resolved.AbsoluteUri;
    }

    /// <summary>
    /// Returns the effective base URI for the given element, resolving any
    /// <c>xml:base</c> attributes in the ancestor chain against the module base URI.
    /// </summary>
    private string? GetEffectiveBaseUri(XElement elem)
    {
        // Start from the document/entity base URI when available, otherwise the
        // module URI supplied by the resolver.
        var currentBase = !string.IsNullOrEmpty(elem.BaseUri) ? elem.BaseUri : _baseUri;

        // Apply xml:base attributes from the root down to the element.
        foreach (var ancestor in elem.AncestorsAndSelf().Reverse())
        {
            var xmlBase = ancestor.Attribute(XName.Get("base", "http://www.w3.org/XML/1998/namespace"))?.Value;
            if (string.IsNullOrEmpty(xmlBase))
                continue;

            currentBase = string.IsNullOrEmpty(currentBase)
                ? ResolveAbsoluteUri(xmlBase, null)
                : ResolveAbsoluteUri(xmlBase, currentBase);
        }

        return currentBase;
    }

    /// <summary>The parsed xsl:output properties, or null if not specified.</summary>
    public OutputProperties? OutputProperties => _outputProperties;

    /// <summary>
    /// The effective unnamed xsl:output properties, merging definitions from imported,
    /// included, and local modules in ascending order of import precedence (imports
    /// first, then includes, then this module), so higher-precedence definitions
    /// override lower-precedence ones (include-0101).
    /// </summary>
    public OutputProperties? EffectiveOutputProperties
    {
        get
        {
            var definitions = new List<(int Precedence, OutputProperties Props)>();
            CollectOutputDefinitions(this, definitions);
            if (definitions.Count == 0)
                return null;

            // Lower numeric precedence means higher XSLT import precedence, so merge
            // imported definitions first and the main stylesheet last. OrderByDescending
            // is stable, keeping includes before the including module at equal precedence.
            var result = new OutputProperties();
            foreach (var (_, props) in definitions.OrderByDescending(d => d.Precedence))
                OutputProperties.Merge(result, props);

            return result;
        }
    }

    private static void CollectOutputDefinitions(Stylesheet sheet, List<(int, OutputProperties)> definitions)
    {
        foreach (var imported in sheet._imports)
            CollectOutputDefinitions(imported, definitions);
        foreach (var included in sheet._includes)
            CollectOutputDefinitions(included, definitions);

        if (sheet._outputProperties != null)
            definitions.Add((sheet.ImportPrecedence, sheet._outputProperties));
    }

    /// <summary>Named xsl:output definitions keyed by expanded QName (Clark notation).</summary>
    public IReadOnlyDictionary<string, OutputProperties> NamedOutputProperties => _namedOutputProperties;

    /// <summary>
    /// Returns the effective xsl:output properties for a named output declaration,
    /// merging definitions from imported, included, and local modules in ascending
    /// order of import precedence. Scalar attributes are overridden by higher-precedence
    /// declarations; list-valued attributes (cdata-section-elements, etc.) are combined.
    /// </summary>
    public OutputProperties? GetEffectiveNamedOutput(string expandedName)
    {
        var definitions = new List<(int Precedence, OutputProperties Props)>();
        CollectNamedOutputDefinitions(this, expandedName, definitions);
        if (definitions.Count == 0)
            return null;

        // Lower numeric precedence means higher XSLT import precedence, so merge
        // lowest-precedence (imported) definitions first and the main stylesheet last.
        definitions.Sort((a, b) => b.Precedence.CompareTo(a.Precedence));

        var result = new OutputProperties();
        foreach (var (_, props) in definitions)
            OutputProperties.Merge(result, props);

        return result;
    }

    private static void CollectNamedOutputDefinitions(Stylesheet sheet, string expandedName, List<(int, OutputProperties)> definitions)
    {
        foreach (var imported in sheet._imports)
            CollectNamedOutputDefinitions(imported, expandedName, definitions);
        foreach (var included in sheet._includes)
            CollectNamedOutputDefinitions(included, expandedName, definitions);

        if (sheet._namedOutputProperties.TryGetValue(expandedName, out var props))
            definitions.Add((sheet.ImportPrecedence, props));
    }

    /// <summary>
    /// Looks up a character-map definition by expanded QName, searching this stylesheet
    /// module and its imports/includes.
    /// </summary>
    public CharacterMapDefinition? GetCharacterMap(string expandedName)
    {
        if (_characterMaps.TryGetValue(expandedName, out var def))
            return def;

        foreach (var import in _imports)
        {
            var imported = import.GetCharacterMap(expandedName);
            if (imported != null)
                return imported;
        }

        foreach (var include in _includes)
        {
            var included = include.GetCharacterMap(expandedName);
            if (included != null)
                return included;
        }

        return null;
    }

    /// <summary>
    /// Resolves a list of character-map names into an effective Unicode codepoint-to-string map.
    /// The maps are processed in the order supplied; for duplicate characters, the last
    /// map in the list wins. Within a single character map, explicit
    /// <c>xsl:output-character</c> mappings override mappings inherited via
    /// <c>use-character-maps</c>.
    /// </summary>
    public Dictionary<int, string> ResolveCharacterMap(IEnumerable<string> expandedNames)
    {
        var result = new Dictionary<int, string>();
        var expanded = new Dictionary<string, Dictionary<int, string>>();
        foreach (var name in expandedNames)
        {
            var map = ExpandCharacterMap(name, expanded, new HashSet<string>());
            foreach (var (cp, str) in map)
            {
                // Later maps in the supplied list override earlier ones; explicit mappings
                // within a single map already override its used maps in ExpandCharacterMap.
                result[cp] = str;
            }
        }
        return result;
    }

    private Dictionary<int, string> ExpandCharacterMap(string expandedName, Dictionary<string, Dictionary<int, string>> expanded, HashSet<string> visiting)
    {
        if (string.IsNullOrEmpty(expandedName))
            return new Dictionary<int, string>();

        if (!visiting.Add(expandedName))
            throw new InvalidOperationException($"XTSE1600: Circular reference in character-map '{expandedName}'.");

        if (expanded.TryGetValue(expandedName, out var cached))
        {
            visiting.Remove(expandedName);
            return cached;
        }

        try
        {
            var result = new Dictionary<int, string>();
            var def = GetCharacterMap(expandedName);
            if (def == null)
                throw new InvalidOperationException($"XTSE1590: Unresolved character-map reference '{expandedName}'.");

            expanded[expandedName] = result;

            foreach (var used in def.UseCharacterMaps)
            {
                var usedMap = ExpandCharacterMap(used, expanded, visiting);
                foreach (var (cp, str) in usedMap)
                    result[cp] = str;
            }

            // Explicit mappings in this character map override its used maps.
            foreach (var (cp, str) in def.Mappings)
                result[cp] = str;

            return result;
        }
        finally
        {
            visiting.Remove(expandedName);
        }
    }

    /// <summary>
    /// Recursively collects every declared character-map name from this stylesheet module,
    /// its imports, and its includes.
    /// </summary>
    public HashSet<string> GetAllCharacterMapNames()
    {
        var names = new HashSet<string>(_characterMaps.Keys);
        foreach (var import in _imports)
            names.UnionWith(import.GetAllCharacterMapNames());
        foreach (var include in _includes)
            names.UnionWith(include.GetAllCharacterMapNames());
        return names;
    }

    /// <summary>
    /// Recursively collects every declared character-map definition from this stylesheet
    /// module, its imports, and its includes. The returned dictionary maps expanded name to
    /// definition; later declarations overwrite earlier ones, but duplicate names are
    /// normally caught by XTSE1580 before this method is called.
    /// </summary>
    public Dictionary<string, CharacterMapDefinition> GetAllCharacterMaps()
    {
        var maps = new Dictionary<string, CharacterMapDefinition>(_characterMaps);
        foreach (var import in _imports)
        {
            foreach (var (name, def) in import.GetAllCharacterMaps())
                maps[name] = def;
        }
        foreach (var include in _includes)
        {
            foreach (var (name, def) in include.GetAllCharacterMaps())
                maps[name] = def;
        }
        return maps;
    }

    /// <summary>
    /// Validates that every name referenced in xsl:output/@use-character-maps and
    /// xsl:character-map/@use-character-maps resolves to a declared character map.
    /// This is a root-level static check (XTSE1590).
    /// </summary>
    private void ValidateCharacterMapReferences()
    {
        var allNames = GetAllCharacterMapNames();
        ValidateCharacterMapReferencesCore(this, allNames);
    }

    private static void ValidateCharacterMapReferencesCore(Stylesheet module, HashSet<string> allNames)
    {
        foreach (var def in module._characterMaps.Values)
        {
            foreach (var used in def.UseCharacterMaps)
            {
                if (!allNames.Contains(used))
                    throw new InvalidOperationException($"XTSE1590: Unresolved character-map reference '{used}'.");
            }
        }

        if (module._outputProperties != null)
            ValidateOutputCharacterMaps(module._outputProperties, allNames);

        foreach (var named in module._namedOutputProperties.Values)
            ValidateOutputCharacterMaps(named, allNames);

        foreach (var import in module._imports)
            ValidateCharacterMapReferencesCore(import, allNames);
        foreach (var include in module._includes)
            ValidateCharacterMapReferencesCore(include, allNames);
    }

    private static void ValidateOutputCharacterMaps(OutputProperties props, HashSet<string> allNames)
    {
        foreach (var qname in props.UseCharacterMaps)
        {
            var expanded = ExpandQName(qname);
            if (!allNames.Contains(expanded))
                throw new InvalidOperationException($"XTSE1590: Unresolved character-map reference '{expanded}'.");
        }
    }

    /// <summary>
    /// Validates that no character map references itself, directly or indirectly, via
    /// xsl:character-map/@use-character-maps. This is a root-level static check (XTSE1600).
    /// </summary>
    private void ValidateCharacterMapCycles()
    {
        var allMaps = GetAllCharacterMaps();
        var visiting = new HashSet<string>();
        foreach (var (name, def) in allMaps)
        {
            visiting.Clear();
            DetectCharacterMapCycle(name, def, allMaps, visiting);
        }
    }

    private static void DetectCharacterMapCycle(string name, CharacterMapDefinition definition,
        Dictionary<string, CharacterMapDefinition> allMaps, HashSet<string> visiting)
    {
        if (!visiting.Add(name))
            throw new InvalidOperationException($"XTSE1600: Circular reference in character-map '{name}'.");

        foreach (var used in definition.UseCharacterMaps)
        {
            if (allMaps.TryGetValue(used, out var usedDef))
                DetectCharacterMapCycle(used, usedDef, allMaps, visiting);
        }

        visiting.Remove(name);
    }

    /// <summary>
    /// Validates that all stylesheet modules agree on the value of
    /// xsl:stylesheet/@input-type-annotations. Conflicting strip/preserve values across
    /// modules raise XTSE0265.
    /// </summary>
    private void ValidateInputTypeAnnotations()
    {
        string? effective = null;
        ValidateInputTypeAnnotationsCore(this, ref effective);
    }

    private static void ValidateInputTypeAnnotationsCore(Stylesheet module, ref string? effective)
    {
        var value = module.InputTypeAnnotations;
        if (!string.IsNullOrEmpty(value) && value != "unspecified")
        {
            if (effective == null)
            {
                effective = value;
            }
            else if (effective != value)
            {
                throw new InvalidOperationException(
                    $"XTSE0265: Conflicting xsl:stylesheet/@input-type-annotations values '{effective}' and '{value}'.");
            }
        }

        foreach (var import in module._imports)
            ValidateInputTypeAnnotationsCore(import, ref effective);
        foreach (var include in module._includes)
            ValidateInputTypeAnnotationsCore(include, ref effective);
    }

    /// <summary>Top-level xsl:param elements defined in this stylesheet.</summary>
    public IReadOnlyList<XElement> GlobalParameters => _globalParameters;

    /// <summary>Top-level xsl:variable elements defined in this stylesheet.</summary>
    public IReadOnlyList<XElement> GlobalVariables => _globalVariables;

    /// <summary>Static variables and parameters evaluated during stylesheet loading.</summary>
    internal IReadOnlyDictionary<(string LocalName, string NamespaceUri), XdmValue> StaticVariables =>
        _staticContext.Variables.ToDictionary(kv => kv.Key, kv => kv.Value.Value);

    /// <summary>All key definitions defined in this stylesheet.</summary>
    public IReadOnlyList<KeyDefinition> KeyDefinitions => _keyDefinitions;

    private HashSet<Stylesheet>? _transitiveImports;

    /// <summary>
    /// All modules that are imported (directly or indirectly, including via includes)
    /// by this stylesheet. Used by <c>xsl:apply-imports</c> to restrict the search
    /// to rules imported into the stylesheet containing the current template rule.
    /// </summary>
    public IReadOnlySet<Stylesheet> TransitiveImports
        => _transitiveImports ??= ComputeTransitiveImports();

    private HashSet<Stylesheet> ComputeTransitiveImports()
    {
        var set = new HashSet<Stylesheet>();
        foreach (var imported in _imports)
            CollectReachableImportsAndIncludes(imported, set);
        foreach (var included in _includes)
            CollectReachableImportsAndIncludes(included, set);
        return set;
    }

    /// <summary>
    /// Recursively collects a module and every module reachable from it through
    /// <c>xsl:import</c> or <c>xsl:include</c> edges. Included modules are transparent
    /// for <c>xsl:apply-imports</c>, so rules declared in modules included by an imported
    /// module must be visible to the importer.
    /// </summary>
    private static void CollectReachableImportsAndIncludes(Stylesheet module, HashSet<Stylesheet> set)
    {
        if (!set.Add(module))
            return;
        foreach (var imported in module._imports)
            CollectReachableImportsAndIncludes(imported, set);
        foreach (var included in module._includes)
            CollectReachableImportsAndIncludes(included, set);
    }

    /// <summary>
    /// Assigns import-precedence values to all imported modules. The main stylesheet
    /// has precedence 0; imported modules are numbered so that later sibling imports
    /// have lower numbers (higher XSLT precedence) than earlier ones. Includes keep
    /// the same precedence as their parent.
    /// </summary>
    private void AssignImportPrecedences()
    {
        var order = new List<Stylesheet>();
        CollectImportedModulesHighToLow(this, order);
        int rank = 1;
        foreach (var module in order)
            module.ImportPrecedence = rank++;
    }

    private static void CollectImportedModulesHighToLow(Stylesheet module, List<Stylesheet> order)
    {
        // Traverse top-level elements in reverse document order so that later
        // imports (which have higher XSLT precedence) are emitted first.
        foreach (var element in module.Root.Elements().Reverse())
        {
            if (element.Name.NamespaceName != XslNamespace)
                continue;

            if (element.Annotation<ResolvedModuleAnnotation>() is { Module: { } child })
            {
                if (element.Name.LocalName == "import")
                {
                    // The imported module itself is higher than its own imports.
                    order.Add(child);
                }
                // Includes share the parent's precedence; their local content is not
                // added, but their imports are still lower than the parent.
                CollectImportedModulesHighToLow(child, order);
            }
        }
    }

    /// <summary>
    /// Collects global parameters and variables in document order, recursing into
    /// imported and included modules at the point where their <c>xsl:import</c> or
    /// <c>xsl:include</c> element occurs. Each declaration is tagged with its
    /// import precedence and a monotonic document-order index so callers can sort
    /// by precedence (lower first) and then by document order, matching XSLT
    /// import-precedence / last-wins semantics.
    /// </summary>
    public void CollectGlobalsInDocumentOrder(List<(int Precedence, int Order, (string LocalName, string NamespaceUri) Name, XElement Element, bool IsParam, Stylesheet SourceStylesheet, Stylesheet CollectingScope, string? EffectiveVisibility)> globals, ref int order, int precedenceOffset = 0, bool includePrivateUsedPackageGlobals = false)
    {
        int precedence = ImportPrecedence + precedenceOffset;
        foreach (var element in Root.Elements())
        {
            var ns = element.Name.NamespaceName;
            var localName = element.Name.LocalName;

            if (ns == XslNamespace && localName == "import")
            {
                if (element.Annotation<ResolvedModuleAnnotation>() is { Module: { } imported })
                    imported.CollectGlobalsInDocumentOrder(globals, ref order, precedenceOffset, includePrivateUsedPackageGlobals);
            }
            else if (ns == XslNamespace && localName == "include")
            {
                if (element.Annotation<ResolvedModuleAnnotation>() is { Module: { } included })
                    included.CollectGlobalsInDocumentOrder(globals, ref order, precedenceOffset, includePrivateUsedPackageGlobals);
            }
            else if (ns == XslNamespace && localName == "use-package")
            {
                if (element.Annotation<ResolvedModuleAnnotation>() is { Module: { } package })
                {
                    var options = element.Annotation<PackageUseOptions>();
                    CollectVisibleGlobalsFromUsedPackage(package, options, precedence, globals, ref order, includePrivateUsedPackageGlobals);
                }
            }
            else if (ns == XslNamespace && localName == "param")
            {
                if (!UseWhen(element)) continue;
                var name = ExpandVariableName(element, element.Attribute("name")?.Value ?? "");
                var exposed = GetExposedVisibility("variable", name.LocalName, name.NamespaceUri);
                var visibility = exposed ?? GetLocalVisibility(element, "variable", IsPackage);
                globals.Add((precedence, order++, name, element, true, this, this, visibility));
            }
            else if (ns == XslNamespace && localName == "variable")
            {
                if (!UseWhen(element)) continue;
                var name = ExpandVariableName(element, element.Attribute("name")?.Value ?? "");
                var exposed = GetExposedVisibility("variable", name.LocalName, name.NamespaceUri);
                var visibility = exposed ?? GetLocalVisibility(element, "variable", IsPackage);
                globals.Add((precedence, order++, name, element, false, this, this, visibility));
            }
        }
    }

    /// <summary>
    /// Collects the global variables and parameters that the given used package exposes
    /// to this stylesheet, applying this use-package's <c>xsl:accept</c> and
    /// <c>xsl:override</c> rules. The collected declarations are given the same import
    /// precedence as the <c>xsl:use-package</c> declaration itself so that overrides win
    /// by document order without creating spurious same-precedence conflicts.
    /// </summary>
    private void CollectVisibleGlobalsFromUsedPackage(Stylesheet package, PackageUseOptions? options, int precedence, List<(int Precedence, int Order, (string LocalName, string NamespaceUri) Name, XElement Element, bool IsParam, Stylesheet SourceStylesheet, Stylesheet CollectingScope, string? EffectiveVisibility)> globals, ref int order, bool includePrivateUsedPackageGlobals)
    {
        var baseGlobals = new List<(int Precedence, int Order, (string LocalName, string NamespaceUri) Name, XElement Element, bool IsParam, Stylesheet SourceStylesheet, Stylesheet CollectingScope, string? EffectiveVisibility)>();
        int baseOrder = 0;
        int usedPackageOffset = ImportPrecedence == package.ImportPrecedence ? 0 : (precedence - package.ImportPrecedence);
        package.CollectGlobalsInDocumentOrder(baseGlobals, ref baseOrder, usedPackageOffset, includePrivateUsedPackageGlobals);

        var overrideParamNames = GetOverrideVariableNames(options, isParam: true);
        var overrideVariableNames = GetOverrideVariableNames(options, isParam: false);

        foreach (var g in baseGlobals)
        {
            var effectiveVis = ApplyAcceptVisibility(g.EffectiveVisibility, options, g.IsParam ? "variable" : "variable", g.Name.LocalName, g.Name.NamespaceUri);
            bool acceptedAsPrivate = effectiveVis == "private" &&
                IsAcceptedAsPrivate(options, "variable", g.Name.LocalName, g.Name.NamespaceUri);
            if (effectiveVis is "hidden" || (effectiveVis is not "public" and not "final" && !acceptedAsPrivate))
                continue;

            if (g.IsParam && overrideParamNames.Contains(g.Name))
            {
                // Keep the overridden original reachable under an unspellable alias so a
                // $xsl:original reference inside the overriding initializer resolves to it
                // (override-v-003; XSLT 3.0 §3.5.7.2).
                globals.Add((g.Precedence, order++, (g.Name.LocalName, Stylesheet.OriginalVariableNamespace), g.Element, g.IsParam, g.SourceStylesheet, this, g.EffectiveVisibility));
                continue;
            }
            if (!g.IsParam && overrideVariableNames.Contains(g.Name))
            {
                globals.Add((g.Precedence, order++, (g.Name.LocalName, Stylesheet.OriginalVariableNamespace), g.Element, g.IsParam, g.SourceStylesheet, this, g.EffectiveVisibility));
                continue;
            }

            globals.Add((precedence, order++, g.Name, g.Element, g.IsParam, g.SourceStylesheet, this, effectiveVis));
        }

        foreach (var overrideElem in options?.OverrideParams ?? Enumerable.Empty<XElement>())
        {
            var nameAttr = overrideElem.Attribute("name")?.Value;
            if (!string.IsNullOrEmpty(nameAttr))
            {
                var name = ExpandVariableName(overrideElem, nameAttr);
                globals.Add((precedence, order++, name, overrideElem, true, this, this, null));
            }
        }

        foreach (var overrideElem in options?.OverrideVariables ?? Enumerable.Empty<XElement>())
        {
            var nameAttr = overrideElem.Attribute("name")?.Value;
            if (!string.IsNullOrEmpty(nameAttr))
            {
                var name = ExpandVariableName(overrideElem, nameAttr);
                globals.Add((precedence, order++, name, overrideElem, false, this, this, null));
            }
        }
    }

    /// <summary>
    /// Collects the global variables and parameters visible to components executing in this
    /// package's own scope: the package's own declarations with <c>xsl:override</c> replacements
    /// contributed by packages that use this package applied on top. Used-package components that
    /// reference an overridden global must see the overriding declaration (XSLT 3.0 §3.5.7.2).
    /// </summary>
    public void CollectPackageScopeGlobalsInDocumentOrder(List<(int Precedence, int Order, (string LocalName, string NamespaceUri) Name, XElement Element, bool IsParam, Stylesheet SourceStylesheet, Stylesheet CollectingScope, string? EffectiveVisibility)> globals, ref int order, bool includePrivateUsedPackageGlobals)
    {
        CollectGlobalsInDocumentOrder(globals, ref order, 0, includePrivateUsedPackageGlobals);
        if (_packageOverrideContributions == null)
            return;

        var overrideParamNames = new HashSet<(string LocalName, string NamespaceUri)>();
        var overrideVariableNames = new HashSet<(string LocalName, string NamespaceUri)>();
        var additions = new List<(XElement Element, bool IsParam, (string LocalName, string NamespaceUri) Name, Stylesheet User)>();
        foreach (var (user, options) in _packageOverrideContributions)
        {
            foreach (var name in GetOverrideVariableNames(options, isParam: true))
                overrideParamNames.Add(name);
            foreach (var name in GetOverrideVariableNames(options, isParam: false))
                overrideVariableNames.Add(name);
            foreach (var elem in options.OverrideParams)
            {
                var nameAttr = elem.Attribute("name")?.Value;
                if (!string.IsNullOrEmpty(nameAttr))
                    additions.Add((elem, true, ExpandVariableName(elem, nameAttr), user));
            }
            foreach (var elem in options.OverrideVariables)
            {
                var nameAttr = elem.Attribute("name")?.Value;
                if (!string.IsNullOrEmpty(nameAttr))
                    additions.Add((elem, false, ExpandVariableName(elem, nameAttr), user));
            }
        }

        if (additions.Count == 0)
            return;

        // Remove the overridden originals collected above; the overriding declarations
        // take their place in this package's effective scope. The originals remain
        // reachable under an unspellable alias so that $xsl:original references inside
        // overriding initializers resolve to them (override-v-003; XSLT 3.0 §3.5.7.2).
        var overridden = globals
            .Where(g => g.IsParam ? overrideParamNames.Contains(g.Name) : overrideVariableNames.Contains(g.Name))
            .ToList();
        foreach (var g in overridden)
            globals.Add((g.Precedence, g.Order, (g.Name.LocalName, OriginalVariableNamespace), g.Element, g.IsParam, g.SourceStylesheet, g.CollectingScope, g.EffectiveVisibility));
        globals.RemoveAll(g => g.IsParam ? overrideParamNames.Contains(g.Name) : overrideVariableNames.Contains(g.Name));
        int precedence = ImportPrecedence;
        foreach (var (elem, isParam, name, user) in additions)
            globals.Add((precedence, order++, name, elem, isParam, user, this, null));
    }

    /// <summary>The alias namespace under which overridden variables/parameters remain
    /// reachable for <c>$xsl:original</c> references (unspellable in source XPath).</summary>
    internal const string OriginalVariableNamespace = "\u0001original";

    /// <summary>
    /// Validates that xsl:global-context-item declarations are consistent across the
    /// modules of a package: all must specify the same use and as values (XTSE3087).
    /// </summary>
    private void ValidateGlobalContextItemConsistency()
    {
        var declarations = new List<(string? Use, string? As)>();
        CollectGlobalContextItemDeclarations(declarations);
        var first = declarations.FirstOrDefault();
        foreach (var d in declarations)
        {
            // Whitespace-insensitive comparison: "document-node(element(doc))" and
            // "document-node( element( doc ))" are the same type (glob-cxt-item-008).
            bool sameUse = (d.Use ?? "optional") == (first.Use ?? "optional");
            bool sameAs = TypesAreIdentical(d.As ?? "item()", first.As ?? "item()");
            if (!sameUse || !sameAs)
                throw new InvalidOperationException("XTSE3087: Inconsistent xsl:global-context-item declarations across the modules of a package.");
        }
    }

    /// <summary>
    /// Collects the xsl:global-context-item declarations of this module and, recursively,
    /// its included and imported modules.
    /// </summary>
    private void CollectGlobalContextItemDeclarations(List<(string? Use, string? As)> result)
    {
        if (GlobalContextItemUse != null || GlobalContextItemAs != null)
            result.Add((GlobalContextItemUse, GlobalContextItemAs));
        foreach (var included in _includes)
            included.CollectGlobalContextItemDeclarations(result);
        foreach (var imported in _imports)
            imported.CollectGlobalContextItemDeclarations(result);
    }

    /// <summary>
    /// Validates that no global variable or parameter has more than one binding at the
    /// same import precedence, unless a higher-precedence binding exists (XTSE0630).
    /// </summary>
    private void ValidateGlobalVariableBindings()
    {
        var globals = new List<(int Precedence, int Order, (string LocalName, string NamespaceUri) Name, XElement Element, bool IsParam, Stylesheet SourceStylesheet, Stylesheet CollectingScope, string? EffectiveVisibility)>();
        int order = 0;
        CollectGlobalsInDocumentOrder(globals, ref order, includePrivateUsedPackageGlobals: false);

        foreach (var group in globals.GroupBy(g => (g.Name, g.CollectingScope, g.SourceStylesheet)))
        {
            var minPrecedence = group.Min(g => g.Precedence);
            var top = group.Where(g => g.Precedence == minPrecedence).ToList();
            if (top.Count > 1)
            {
                var name = top[0].Name;
                var displayName = string.IsNullOrEmpty(name.NamespaceUri) ? name.LocalName : $"{{{name.NamespaceUri}}}{name.LocalName}";
                throw new InvalidOperationException($"XTSE0630: More than one binding for global variable '{displayName}' at the same import precedence.");
            }
        }
    }

    /// <summary>
    /// Recursively collects named xsl:template declarations in document order, recursing
    /// into imported and included modules at the point where their xsl:import or
    /// xsl:include element occurs. Each declaration is tagged with its import precedence
    /// and a monotonic document-order index.
    /// </summary>
    public void CollectNamedTemplatesInDocumentOrder(List<(int Precedence, int Order, (string LocalName, string NamespaceUri) Name, XElement Element)> named, ref int order)
    {
        foreach (var element in Root.Elements())
        {
            var ns = element.Name.NamespaceName;
            var localName = element.Name.LocalName;

            if (ns == XslNamespace && localName == "import")
            {
                if (element.Annotation<ResolvedModuleAnnotation>() is { Module: { } imported })
                    imported.CollectNamedTemplatesInDocumentOrder(named, ref order);
            }
            else if (ns == XslNamespace && localName == "include")
            {
                if (element.Annotation<ResolvedModuleAnnotation>() is { Module: { } included })
                    included.CollectNamedTemplatesInDocumentOrder(named, ref order);
            }
            else if (ns == XslNamespace && localName == "template")
            {
                var nameAttr = element.Attribute("name")?.Value;
                if (!string.IsNullOrEmpty(nameAttr))
                {
                    var name = ExpandVariableName(element, nameAttr);
                    named.Add((ImportPrecedence, order++, name, element));
                }
            }
        }
    }

    /// <summary>
    /// Validates that no named template has more than one binding at the same import
    /// precedence, unless a higher-precedence binding exists (XTSE0660).
    /// </summary>
    private void ValidateNamedTemplateBindings()
    {
        var named = new List<(int Precedence, int Order, (string LocalName, string NamespaceUri) Name, XElement Element)>();
        int order = 0;
        CollectNamedTemplatesInDocumentOrder(named, ref order);

        foreach (var group in named.GroupBy(n => n.Name))
        {
            var minPrecedence = group.Min(n => n.Precedence);
            var top = group.Where(n => n.Precedence == minPrecedence).ToList();
            if (top.Count > 1)
            {
                var name = top[0].Name;
                var displayName = string.IsNullOrEmpty(name.NamespaceUri) ? name.LocalName : $"{{{name.NamespaceUri}}}{name.LocalName}";
                throw new InvalidOperationException($"XTSE0660: More than one named template '{displayName}' at the same import precedence.");
            }
        }
    }

    /// <summary>
    /// Recursively collects all global parameters from this stylesheet, its includes, and its imports.
    /// Later definitions override earlier ones (local &gt; included &gt; imported).
    /// </summary>
    public Dictionary<string, XElement> GetAllGlobalParameters()
    {
        var result = new Dictionary<string, XElement>();

        foreach (var imported in _imports)
        {
            foreach (var (name, elem) in imported.GetAllGlobalParameters())
                result[name] = elem;
        }

        foreach (var included in _includes)
        {
            foreach (var (name, elem) in included.GetAllGlobalParameters())
                result[name] = elem;
        }

        foreach (var package in _usedPackages)
        {
            var options = _usedPackageOptions.GetValueOrDefault(package);
            var overrideNames = GetOverrideVariableNames(options, isParam: true);
            foreach (var (name, elem) in package.GetAllGlobalParameters())
            {
                var expanded = ExpandVariableName(elem, name);
                var effectiveVis = GetEffectiveVisibility(package, elem, "variable", options, expanded.LocalName, expanded.NamespaceUri);
                if (effectiveVis is not "public" and not "final")
                    continue;
                if (overrideNames.Contains(expanded))
                    continue;
                result[name] = elem;
            }
            foreach (var overrideElem in options?.OverrideParams ?? Enumerable.Empty<XElement>())
            {
                var nameAttr = overrideElem.Attribute("name")?.Value;
                if (!string.IsNullOrEmpty(nameAttr))
                    result[nameAttr] = overrideElem;
            }
        }

        foreach (var param in _globalParameters)
        {
            var name = param.Attribute("name")?.Value;
            if (!string.IsNullOrEmpty(name))
                result[name] = param;
        }

        return result;
    }

    /// <summary>
    /// Recursively collects all global variables from this stylesheet, its includes, and its imports.
    /// Later definitions override earlier ones (local &gt; included &gt; imported).
    /// </summary>
    public Dictionary<string, XElement> GetAllGlobalVariables()
    {
        var result = new Dictionary<string, XElement>();

        foreach (var imported in _imports)
        {
            foreach (var (name, elem) in imported.GetAllGlobalVariables())
                result[name] = elem;
        }

        foreach (var included in _includes)
        {
            foreach (var (name, elem) in included.GetAllGlobalVariables())
                result[name] = elem;
        }

        foreach (var package in _usedPackages)
        {
            var options = _usedPackageOptions.GetValueOrDefault(package);
            var overrideNames = GetOverrideVariableNames(options, isParam: false);
            foreach (var (name, elem) in package.GetAllGlobalVariables())
            {
                var expanded = ExpandVariableName(elem, name);
                var effectiveVis = GetEffectiveVisibility(package, elem, "variable", options, expanded.LocalName, expanded.NamespaceUri);
                if (effectiveVis is not "public" and not "final")
                    continue;
                if (overrideNames.Contains(expanded))
                    continue;
                result[name] = elem;
            }
            foreach (var overrideElem in options?.OverrideVariables ?? Enumerable.Empty<XElement>())
            {
                var nameAttr = overrideElem.Attribute("name")?.Value;
                if (!string.IsNullOrEmpty(nameAttr))
                    result[nameAttr] = overrideElem;
            }
        }

        foreach (var variable in _globalVariables)
        {
            var name = variable.Attribute("name")?.Value;
            if (!string.IsNullOrEmpty(name))
                result[name] = variable;
        }

        return result;
    }

    /// <summary>
    /// Recursively collects all key definitions from this stylesheet, its includes, and its imports.
    /// </summary>
    public IReadOnlyList<KeyDefinition> GetAllKeyDefinitions()
    {
        var result = new List<KeyDefinition>(_keyDefinitions);
        foreach (var included in _includes)
            result.AddRange(included.GetAllKeyDefinitions());
        foreach (var imported in _imports)
            result.AddRange(imported.GetAllKeyDefinitions());
        return result;
    }

    /// <summary>Function definitions declared in this stylesheet.</summary>
    public IReadOnlyList<XsltFunctionDefinition> FunctionDefinitions => _functionDefinitions;

    /// <summary>Decimal format definitions declared in this stylesheet.</summary>
    public IReadOnlyList<DecimalFormatDefinition> DecimalFormats => _decimalFormats;

    /// <summary>
    /// Recursively collects all function definitions from this stylesheet, its includes, and its imports.
    /// Later definitions override earlier ones (local &gt; included &gt; imported).
    /// The key is a tuple of (namespaceUri, localName, arity).
    /// When <paramref name="includePrivate"/> is <c>true</c> and this stylesheet is a package,
    /// private functions of the package are included in addition to public/final ones.
    /// When <paramref name="includePrivate"/> is <c>true</c> and <paramref name="includeUsedPackagePrivate"/>
    /// is <c>false</c>, private functions declared in used packages are excluded (only public/final
    /// components of a used package are visible to the using package).
    /// </summary>
    public Dictionary<(string ns, string name, int arity), XsltFunctionDefinition> GetAllFunctionDefinitions(bool includePrivate = false, bool includeUsedPackagePrivate = true)
    {
        var result = new Dictionary<(string, string, int), XsltFunctionDefinition>();

        foreach (var imported in _imports)
        {
            foreach (var (key, def) in imported.GetAllFunctionDefinitions(includePrivate, includeUsedPackagePrivate))
                result[key] = def;
        }

        foreach (var included in _includes)
        {
            foreach (var (key, def) in included.GetAllFunctionDefinitions(includePrivate, includeUsedPackagePrivate))
                result[key] = def;
        }

        foreach (var package in _usedPackages)
        {
            bool usedIncludePrivate = includePrivate && includeUsedPackagePrivate;
            var options = _usedPackageOptions.GetValueOrDefault(package);
            var overrides = new Dictionary<(string, string, int), XsltFunctionDefinition>();
            if (options != null)
            {
                foreach (var overrideElem in options.OverrideFunctions)
                {
                    var overrideDef = XsltFunctionDefinition.FromElement(overrideElem, this);
                    if (overrideDef != null)
                        overrides[(overrideDef.NamespaceUri, overrideDef.LocalName, overrideDef.Arity)] = overrideDef;
                }
            }

            foreach (var (key, def) in package.GetAllFunctionDefinitions(usedIncludePrivate, includeUsedPackagePrivate))
            {
                var effectiveVis = GetEffectiveVisibility(package, def.Element, "function", options, def.LocalName, def.NamespaceUri, def.Arity);
                bool acceptedAsPrivate = effectiveVis == "private" &&
                    IsAcceptedAsPrivate(options, "function", def.LocalName, def.NamespaceUri, def.Arity);
                if (overrides.TryGetValue(key, out var overrideDef))
                {
                    overrideDef.OverriddenFunction = def;
                    result[key] = overrideDef;
                }
                else if (effectiveVis is "public" or "final" or "abstract" ||
                         (usedIncludePrivate && acceptedAsPrivate))
                {
                    result[key] = def;
                }
            }

            foreach (var (key, overrideDef) in overrides)
            {
                if (!result.ContainsKey(key))
                    result[key] = overrideDef;
            }
        }

        foreach (var def in _functionDefinitions)
        {
            if (includePrivate || !IsPackage || IsExportedFromPackage(this, def))
                result[(def.NamespaceUri, def.LocalName, def.Arity)] = def;
        }

        return result;
    }

    /// <summary>
    /// Returns the function definitions visible to components executing in this package's own
    /// scope: every function visible locally, with <c>xsl:override</c> replacements contributed
    /// by packages that use this package applied on top. Used-package components that call an
    /// overridden function must dispatch to the overriding declaration (XSLT 3.0 §3.5.7.2).
    /// </summary>
    public Dictionary<(string ns, string name, int arity), XsltFunctionDefinition> GetPackageScopeFunctionDefinitions()
    {
        var result = GetAllFunctionDefinitions(includePrivate: true, includeUsedPackagePrivate: true);
        ApplyPackageOverrideContributions(result);
        return result;
    }

    /// <summary>
    /// Applies the <c>xsl:override</c> function declarations contributed by packages that use
    /// this package to the supplied definition map, replacing same-name/arity definitions and
    /// linking each override to the declaration it replaces. Used to build the effective
    /// function registry for this package's execution scope (XSLT 3.0 §3.5.7.2).
    /// </summary>
    internal void ApplyPackageOverrideContributions(Dictionary<(string ns, string name, int arity), XsltFunctionDefinition> result)
    {
        if (_packageOverrideContributions == null)
            return;

        foreach (var (user, options) in _packageOverrideContributions)
        {
            foreach (var overrideElem in options.OverrideFunctions)
            {
                var overrideDef = XsltFunctionDefinition.FromElement(overrideElem, user);
                if (overrideDef == null)
                    continue;
                var key = (overrideDef.NamespaceUri, overrideDef.LocalName, overrideDef.Arity);
                if (result.TryGetValue(key, out var original))
                    overrideDef.OverriddenFunction = original;
                result[key] = overrideDef;
            }
        }
    }

    /// <summary>
    /// Recursively collects all whitespace handling rules from this stylesheet, its includes, and its imports.
    /// Rules are returned in precedence order (local highest, then included, then imported).
    /// </summary>
    public List<SpaceHandlingRule> GetAllSpaceHandlingRules()
    {
        var result = new List<SpaceHandlingRule>();
        // Imported first (lowest precedence)
        foreach (var imported in _imports)
            result.AddRange(imported.GetAllSpaceHandlingRules());
        // Included next
        foreach (var included in _includes)
            result.AddRange(included.GetAllSpaceHandlingRules());
        // Local last (highest precedence)
        result.AddRange(_spaceRules);
        return result;
    }

    /// <summary>
    /// Looks up a mode definition by name, considering import/include precedence.
    /// Local definitions override included, which override imported.
    /// </summary>
    private void ValidateModeDefinitions()
    {
        var all = new Dictionary<string, List<(int Precedence, ModeDefinition Def)>>();
        CollectModeDefinitions(this, all);

        foreach (var (name, list) in all)
        {
            var minPrecedence = list.Min(x => x.Precedence);
            var top = list.Where(x => x.Precedence == minPrecedence).Select(x => x.Def).ToList();
            if (top.Count <= 1)
                continue;

            var first = top[0];
            for (int i = 1; i < top.Count; i++)
            {
                if (!AreModesEquivalent(first, top[i]))
                {
                    var details = string.Join(", ", list.Select(x => $"(p={x.Precedence},on={x.Def.OnNoMatch},vis={x.Def.Visibility},acc={string.Join("|", x.Def.UseAccumulators)})"));
                    throw new InvalidOperationException($"XTSE0545: Conflicting xsl:mode declarations for mode '{name}' at the same import precedence. [{details}]");
                }
            }
        }

        // If this is a package with declared-modes="yes", every mode used within the
        // package must be declared by an xsl:mode declaration in the same package.
        if (IsPackage && DeclaredModes)
        {
            var declaredModes = new HashSet<string>();
            CollectDeclaredModes(this, declaredModes);

            var usedModes = new HashSet<string>();
            CollectUsedModes(this, usedModes);

            foreach (var mode in usedModes)
            {
                if (!declaredModes.Contains(mode))
                    throw new InvalidOperationException($"XTSE3085: Mode '{mode}' is not declared.");
            }
        }
    }

    private static void CollectModeDefinitions(Stylesheet stylesheet, Dictionary<string, List<(int Precedence, ModeDefinition Def)>> map)
    {
        foreach (var kv in stylesheet._modeDefinitions)
        {
            if (!map.TryGetValue(kv.Key, out var list))
            {
                list = new List<(int, ModeDefinition)>();
                map[kv.Key] = list;
            }
            list.Add((stylesheet.ImportPrecedence, kv.Value));
        }
        foreach (var included in stylesheet._includes)
            CollectModeDefinitions(included, map);
        foreach (var imported in stylesheet._imports)
            CollectModeDefinitions(imported, map);
        foreach (var package in stylesheet._usedPackages)
        {
            foreach (var kv in package._modeDefinitions)
            {
                if (!IsExportedFromPackage(package, kv.Value))
                    continue;
                if (!map.TryGetValue(kv.Key, out var list))
                {
                    list = new List<(int, ModeDefinition)>();
                    map[kv.Key] = list;
                }
                list.Add((package.ImportPrecedence, kv.Value));
            }
            CollectModeDefinitions(package, map);
        }
    }

    /// <summary>
    /// Recursively collects the names of all xsl:mode declarations declared in this
    /// stylesheet and its imports/includes. Modes accepted from used packages are also
    /// considered declared because they become part of the using package's mode set.
    /// </summary>
    private static void CollectDeclaredModes(Stylesheet stylesheet, HashSet<string> declaredModes)
    {
        foreach (var name in stylesheet._modeDefinitions.Keys)
            declaredModes.Add(name);

        foreach (var included in stylesheet._includes)
            CollectDeclaredModes(included, declaredModes);
        foreach (var imported in stylesheet._imports)
            CollectDeclaredModes(imported, declaredModes);
        foreach (var package in stylesheet._usedPackages)
        {
            foreach (var kv in package._modeDefinitions)
            {
                if (!IsExportedFromPackage(package, kv.Value))
                    continue;
                declaredModes.Add(kv.Key);
            }
            CollectDeclaredModes(package, declaredModes);
        }
    }

    /// <summary>
    /// Recursively collects the names of all modes used in this stylesheet and its
    /// imports/includes (but not in used packages). Implicit default/unnamed mode
    /// usages and explicit #default/#unnamed are normalized to the empty string.
    /// </summary>
    private static void CollectUsedModes(Stylesheet stylesheet, HashSet<string> usedModes)
    {
        foreach (var element in stylesheet.Root.Descendants())
        {
            if (element.Name.Namespace != XslNamespace)
                continue;

            var localName = element.Name.LocalName;
            if (localName == "template")
            {
                var matchAttr = element.Attribute("match");
                if (matchAttr == null)
                    continue;

                var modeAttr = element.Attribute("mode");
                if (modeAttr == null)
                {
                    usedModes.Add(stylesheet.DefaultMode);
                }
                else
                {
                    foreach (var token in modeAttr.Value.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
                        CollectUsedModeToken(token, element, usedModes);
                }
            }
            else if (localName == "apply-templates")
            {
                var modeAttr = element.Attribute("mode");
                if (modeAttr == null)
                {
                    usedModes.Add(stylesheet.DefaultMode);
                }
                else
                {
                    var token = modeAttr.Value.Trim();
                    if (string.IsNullOrEmpty(token))
                        usedModes.Add(stylesheet.DefaultMode);
                    else
                        CollectUsedModeToken(token, element, usedModes);
                }
            }
        }

        foreach (var included in stylesheet._includes)
            CollectUsedModes(included, usedModes);
        foreach (var imported in stylesheet._imports)
            CollectUsedModes(imported, usedModes);
    }

    /// <summary>
    /// Expands a single mode token and adds it to the used-mode set, normalizing
    /// #default/#unnamed to the empty string and ignoring #current/#all.
    /// </summary>
    private static void CollectUsedModeToken(string mode, XElement element, HashSet<string> usedModes)
    {
        string expanded;
        if (mode == "#current" || mode == "#default" || mode == "#all" || mode == "#unnamed")
        {
            expanded = mode;
        }
        else
        {
            int colon = mode.IndexOf(':');
            if (colon < 0)
            {
                expanded = mode;
            }
            else
            {
                var prefix = mode.Substring(0, colon);
                var local = mode.Substring(colon + 1);
                string? ns = null;
                var current = element;
                while (current != null)
                {
                    foreach (var attr in current.Attributes())
                    {
                        if (attr.IsNamespaceDeclaration && attr.Name.LocalName == prefix)
                        {
                            ns = attr.Value;
                            break;
                        }
                    }
                    if (ns != null) break;
                    current = current.Parent;
                }
                expanded = ns != null ? $"{{{ns}}}{local}" : mode;
            }
        }

        if (expanded == "#current" || expanded == "#all")
            return;
        if (expanded == "#default" || expanded == "#unnamed")
            usedModes.Add("");
        else
            usedModes.Add(expanded);
    }

    /// <summary>
    /// Returns true when the given mode is visible outside its declaring package.
    /// Imports and includes are always visible; used packages filter by visibility.
    /// </summary>
    private static bool IsExportedFromPackage(Stylesheet package, ModeDefinition mode)
    {
        if (!package.IsPackage) return true;
        var exposed = package.GetExposedVisibility("mode", mode.Name, "");
        var effective = exposed ?? mode.Visibility.ToString().ToLowerInvariant();
        return effective is "public" or "final";
    }

    private static bool HasSamePrecedenceMode(Stylesheet stylesheet, string name)
    {
        if (stylesheet._modeDefinitions.ContainsKey(name))
            return true;
        foreach (var included in stylesheet._includes)
        {
            if (included.ImportPrecedence == stylesheet.ImportPrecedence && HasSamePrecedenceMode(included, name))
                return true;
        }
        return false;
    }

    private static bool AreModesEquivalent(ModeDefinition a, ModeDefinition b)
    {
        return a.OnNoMatch == b.OnNoMatch
            && a.OnMultipleMatch == b.OnMultipleMatch
            && a.Visibility == b.Visibility
            && a.Typed == b.Typed
            && a.WarningOnNoMatch == b.WarningOnNoMatch
            && a.WarningOnMultipleMatch == b.WarningOnMultipleMatch
            && a.Streamable == b.Streamable
            && a.UseAllAccumulators == b.UseAllAccumulators
            && a.UseAccumulators.SetEquals(b.UseAccumulators);
    }

    public ModeDefinition? GetModeDefinition(string name)
    {
        // Local first (highest precedence)
        if (_modeDefinitions.TryGetValue(name, out var local))
            return local;

        // Included next
        foreach (var included in _includes)
        {
            var def = included.GetModeDefinition(name);
            if (def != null)
                return def;
        }

        // Imported next
        foreach (var imported in _imports)
        {
            var def = imported.GetModeDefinition(name);
            if (def != null)
                return def;
        }

        // Used packages last: only exported modes are visible.
        foreach (var package in _usedPackages)
        {
            var def = package.GetModeDefinition(name);
            if (def != null && IsExportedFromPackage(package, def))
                return def;
        }

        return null;
    }

    /// <summary>The root element of the parsed stylesheet document.</summary>
    public XElement RootElement => _document.Root!;

    /// <summary>
    /// Collects all namespace prefix declarations from this stylesheet and its imports/includes.
    /// </summary>
    public Dictionary<string, string> GetAllNamespaces()
    {
        var result = new Dictionary<string, string>();
        foreach (var imported in _imports)
        {
            foreach (var (prefix, ns) in imported.GetAllNamespaces())
                result[prefix] = ns;
        }
        foreach (var included in _includes)
        {
            foreach (var (prefix, ns) in included.GetAllNamespaces())
                result[prefix] = ns;
        }
        if (_document.Root != null)
        {
            foreach (var attr in _document.Root.Attributes())
            {
                if (attr.IsNamespaceDeclaration)
                {
                    var prefix = attr.Name.LocalName;
                    if (prefix == "xmlns")
                        prefix = string.Empty;
                    result[prefix] = attr.Value;
                }
            }
            // Also collect namespace declarations from descendant elements,
            // but only add new prefixes — do not override root declarations.
            // This prevents literal result elements from shadowing stylesheet
            // prefixes while still making locally-declared prefixes available.
            foreach (var elem in _document.Root.Descendants())
            {
                foreach (var attr in elem.Attributes())
                {
                    if (attr.IsNamespaceDeclaration)
                    {
                        var prefix = attr.Name.LocalName;
                        if (prefix == "xmlns")
                            prefix = string.Empty;
                        if (!result.ContainsKey(prefix))
                            result[prefix] = attr.Value;
                    }
                }
            }
        }
        return result;
    }

    /// <summary>
    /// Collects all excluded result prefixes from this stylesheet, its imports, and its includes.
    /// Imported first, then included, then local.
    /// </summary>
    public HashSet<string> GetAllExcludedResultPrefixes()
    {
        var result = new HashSet<string>();
        foreach (var imported in _imports)
        {
            foreach (var prefix in imported.GetAllExcludedResultPrefixes())
                result.Add(prefix);
        }
        foreach (var included in _includes)
        {
            foreach (var prefix in included.GetAllExcludedResultPrefixes())
                result.Add(prefix);
        }
        foreach (var prefix in _excludedResultPrefixes)
            result.Add(prefix);
        return result;
    }

    /// <summary>
    /// Returns the effective <c>xsl:namespace-alias</c> mapping for this stylesheet.
    /// Higher-precedence declarations override lower-precedence ones; conflicting
    /// declarations at the same import precedence are reported as <c>XTSE0813</c>.
    /// </summary>
    public Dictionary<string, NamespaceAliasDefinition> GetEffectiveNamespaceAliases()
    {
        var all = new List<NamespaceAliasDefinition>();
        foreach (var imported in _imports)
            all.AddRange(imported.GetAllNamespaceAliases());
        foreach (var included in _includes)
            all.AddRange(included.GetAllNamespaceAliases());
        all.AddRange(_namespaceAliases);

        var result = new Dictionary<string, NamespaceAliasDefinition>();
        foreach (var group in all.GroupBy(a => a.SourceUri))
        {
            var ordered = group.OrderBy(a => a.ImportPrecedence).ToList();
            var best = ordered[0];
            foreach (var other in ordered)
            {
                if (other.ImportPrecedence == best.ImportPrecedence &&
                    (other.ResultPrefix != best.ResultPrefix || other.ResultUri != best.ResultUri))
                {
                    throw new InvalidOperationException("XTSE0813: Conflicting xsl:namespace-alias declarations for the same namespace URI at the same import precedence.");
                }
            }
            result[group.Key] = best;
        }
        return result;
    }

    /// <summary>
    /// Collects the raw <c>xsl:namespace-alias</c> definitions from this stylesheet module only.
    /// </summary>
    public IReadOnlyList<NamespaceAliasDefinition> GetAllNamespaceAliases()
    {
        return _namespaceAliases;
    }

    /// <summary>
    /// Recursively collects all decimal format definitions from this stylesheet, its includes, and its imports.
    /// Later definitions override earlier ones (local &gt; included &gt; imported).
    /// Conflicting values for the same attribute at the same import precedence raise XTSE1290.
    /// </summary>
    public Dictionary<(string localName, string nsUri), DecimalFormatDefinition> GetAllDecimalFormats()
    {
        var all = new Dictionary<(string, string), List<DecimalFormatDefinition>>();
        CollectDecimalFormats(all);

        foreach (var (key, list) in all)
            ValidateDecimalFormatConflicts(key, list);

        var result = new Dictionary<(string, string), DecimalFormat>();
        foreach (var (key, list) in all)
        {
            // Lower ImportPrecedence number = higher XSLT precedence; process lower-precedence
            // (higher-numbered) modules first so the highest-precedence module wins.
            foreach (var def in list.OrderByDescending(d => d.ImportPrecedence))
                MergeDecimalFormat(result, key, def);
        }

        // Convert back to definitions
        var defs = new Dictionary<(string, string), DecimalFormatDefinition>();
        foreach (var (key, format) in result)
        {
            defs[key] = new DecimalFormatDefinition
            {
                LocalName = key.Item1,
                NamespaceUri = key.Item2,
                Format = format
            };
        }
        return defs;
    }

    /// <summary>
    /// Gathers all <c>xsl:decimal-format</c> definitions from imports, includes, and this stylesheet,
    /// preserving each declaration's import precedence.
    /// </summary>
    private void CollectDecimalFormats(Dictionary<(string, string), List<DecimalFormatDefinition>> all)
    {
        foreach (var imported in _imports)
            imported.CollectDecimalFormats(all);

        foreach (var included in _includes)
            included.CollectDecimalFormats(all);

        foreach (var def in _decimalFormats)
        {
            var key = (def.LocalName, def.NamespaceUri);
            if (!all.TryGetValue(key, out var list))
                all[key] = list = new List<DecimalFormatDefinition>();
            list.Add(def);
        }
    }

    /// <summary>
    /// Validates that no two <c>xsl:decimal-format</c> declarations with the same name and
    /// the same import precedence supply conflicting values for the same attribute.
    /// A definition at a higher import precedence for the same attribute overrides lower ones.
    /// </summary>
    private static void ValidateDecimalFormatConflicts((string localName, string nsUri) key, List<DecimalFormatDefinition> defs)
    {
        var attrDefs = new Dictionary<string, List<DecimalFormatDefinition>>();
        foreach (var def in defs)
        {
            foreach (var attr in def.ExplicitAttributes)
            {
                if (!attrDefs.TryGetValue(attr, out var list))
                    attrDefs[attr] = list = new List<DecimalFormatDefinition>();
                list.Add(def);
            }
        }

        foreach (var (attr, list) in attrDefs)
        {
            // Lower ImportPrecedence number = higher XSLT precedence. The effective value comes
            // from the highest-precedence definition(s); conflicts among those are errors.
            var minPrecedence = list.Min(d => d.ImportPrecedence);
            var top = list.Where(d => d.ImportPrecedence == minPrecedence).ToList();
            if (top.Count < 2)
                continue;

            var first = GetDecimalFormatAttributeValue(top[0], attr);
            for (int i = 1; i < top.Count; i++)
            {
                if (!first.Equals(GetDecimalFormatAttributeValue(top[i], attr), StringComparison.Ordinal))
                {
                    var displayName = string.IsNullOrEmpty(key.localName) ? "(default)" : $"{key.localName}";
                    throw new InvalidOperationException($"XTSE1290: Conflicting values for xsl:decimal-format '{displayName}' attribute '{attr}' at the same import precedence.");
                }
            }
        }
    }

    /// <summary>
    /// Returns the value of a single <c>xsl:decimal-format</c> attribute from a definition.
    /// </summary>
    private static string GetDecimalFormatAttributeValue(DecimalFormatDefinition def, string attr)
    {
        return attr switch
        {
            "decimal-separator" => def.Format.DecimalSeparator,
            "grouping-separator" => def.Format.GroupingSeparator,
            "minus-sign" => def.Format.MinusSign,
            "percent" => def.Format.Percent,
            "per-mille" => def.Format.PerMille,
            "zero-digit" => def.Format.ZeroDigit,
            "digit" => def.Format.Digit,
            "pattern-separator" => def.Format.PatternSeparator,
            "exponent-separator" => def.Format.ExponentSeparator,
            "infinity" => def.Format.Infinity,
            "NaN" => def.Format.NaN,
            _ => ""
        };
    }

    private static void MergeDecimalFormat(Dictionary<(string, string), DecimalFormat> result, (string, string) key, DecimalFormatDefinition def)
    {
        if (!result.TryGetValue(key, out var existing))
        {
            result[key] = new DecimalFormat
            {
                DecimalSeparator = def.Format.DecimalSeparator,
                GroupingSeparator = def.Format.GroupingSeparator,
                Digit = def.Format.Digit,
                ZeroDigit = def.Format.ZeroDigit,
                PatternSeparator = def.Format.PatternSeparator,
                MinusSign = def.Format.MinusSign,
                Percent = def.Format.Percent,
                PerMille = def.Format.PerMille,
                Infinity = def.Format.Infinity,
                NaN = def.Format.NaN,
                ExponentSeparator = def.Format.ExponentSeparator
            };
            return;
        }

        // Merge explicitly-set attributes from the new definition
        foreach (var attr in def.ExplicitAttributes)
        {
            switch (attr)
            {
                case "decimal-separator": existing.DecimalSeparator = def.Format.DecimalSeparator; break;
                case "grouping-separator": existing.GroupingSeparator = def.Format.GroupingSeparator; break;
                case "infinity": existing.Infinity = def.Format.Infinity; break;
                case "minus-sign": existing.MinusSign = def.Format.MinusSign; break;
                case "NaN": existing.NaN = def.Format.NaN; break;
                case "percent": existing.Percent = def.Format.Percent; break;
                case "per-mille": existing.PerMille = def.Format.PerMille; break;
                case "zero-digit": existing.ZeroDigit = def.Format.ZeroDigit; break;
                case "digit": existing.Digit = def.Format.Digit; break;
                case "pattern-separator": existing.PatternSeparator = def.Format.PatternSeparator; break;
                case "exponent-separator": existing.ExponentSeparator = def.Format.ExponentSeparator; break;
            }
        }
    }

    /// <summary>
    /// Collects all attribute-set definitions from this stylesheet, its includes, and its imports.
    /// Attribute sets accumulate (merge) across modules: imported first, then included, then local.
    /// </summary>
    public Dictionary<(string LocalName, string NamespaceUri), List<AttributeSetDefinition>> GetAllAttributeSets()
    {
        var result = new Dictionary<(string, string), List<AttributeSetDefinition>>();

        // Imported first (lowest precedence)
        foreach (var imported in _imports)
        {
            foreach (var (key, list) in imported.GetAllAttributeSets())
            {
                if (!result.TryGetValue(key, out var existing))
                    result[key] = existing = new List<AttributeSetDefinition>();
                existing.AddRange(list);
            }
        }

        // Included next
        foreach (var included in _includes)
        {
            foreach (var (key, list) in included.GetAllAttributeSets())
            {
                if (!result.TryGetValue(key, out var existing))
                    result[key] = existing = new List<AttributeSetDefinition>();
                existing.AddRange(list);
            }
        }

        // Used packages next: only exported components are visible.
        foreach (var package in _usedPackages)
        {
            var options = _usedPackageOptions.GetValueOrDefault(package);

            // Names of attribute-sets overridden in this use-package; used-package
            // definitions with the same name are replaced, unless the override uses
            // xsl:original to retain the original attributes.
            var overrideSetNames = new HashSet<(string LocalName, string NamespaceUri)>();
            var overrideUsesOriginal = new HashSet<(string LocalName, string NamespaceUri)>();
            foreach (var overrideElem in options?.OverrideAttributeSets ?? Enumerable.Empty<XElement>())
            {
                var def = AttributeSetDefinition.FromElement(overrideElem, this);
                if (def == null)
                    continue;
                var key = (def.LocalName, def.NamespaceUri);
                overrideSetNames.Add(key);
                if (UseAttributeSetsReferencesOriginal(overrideElem, def.UseAttributeSets))
                    overrideUsesOriginal.Add(key);
            }

            foreach (var (key, list) in package.GetAllAttributeSets())
            {
                if (!result.TryGetValue(key, out var existing))
                    result[key] = existing = new List<AttributeSetDefinition>();

                if (overrideSetNames.Contains(key) && !overrideUsesOriginal.Contains(key))
                    continue;

                foreach (var def in list)
                {
                    var exposed = package.GetExposedVisibility("attribute-set", def.LocalName, def.NamespaceUri);
                    var baseVis = exposed ?? GetLocalVisibility(def.Element, "attribute-set", package);
                    var effectiveVis = ApplyAcceptVisibility(baseVis, options, "attribute-set", def.LocalName, def.NamespaceUri);
                    bool acceptedAsPrivate = effectiveVis == "private" &&
                        IsAcceptedAsPrivate(options, "attribute-set", def.LocalName, def.NamespaceUri);
                    // Hidden non-abstract attribute-sets are invisible in the using package.
                    if (effectiveVis == "hidden" && baseVis != "abstract")
                        continue;
                    if (effectiveVis != "public" && effectiveVis != "final" && effectiveVis != "abstract" && !acceptedAsPrivate)
                    {
                        // Abstract attribute-sets remain abstract even when accepted as hidden;
                        // using them at runtime raises XTDE3052.
                        if (baseVis == "abstract")
                            effectiveVis = "abstract";
                        else
                            continue;
                    }
                    def.EffectiveVisibility = effectiveVis;
                    existing.Add(def);
                }
            }

            // Add override attribute-set definitions (highest precedence in this use-package).
            foreach (var overrideElem in options?.OverrideAttributeSets ?? Enumerable.Empty<XElement>())
            {
                var def = AttributeSetDefinition.FromElement(overrideElem, this);
                if (def == null)
                    continue;
                var key = (def.LocalName, def.NamespaceUri);
                if (!result.TryGetValue(key, out var existing))
                    result[key] = existing = new List<AttributeSetDefinition>();
                existing.Add(def);
            }
        }

        // Local last (highest precedence)
        foreach (var def in _attributeSets)
        {
            var key = (def.LocalName, def.NamespaceUri);
            if (!result.TryGetValue(key, out var existing))
                result[key] = existing = new List<AttributeSetDefinition>();
            existing.Add(def);
        }

        return result;
    }

    /// <summary>The XSLT namespace URI.</summary>
    public const string XslNamespace = "http://www.w3.org/1999/XSL/Transform";

    /// <summary>The XML Schema namespace URI.</summary>
    public const string XsNamespace = "http://www.w3.org/2001/XMLSchema";

    /// <summary>The XPath functions namespace URI.</summary>
    public const string FnNamespace = "http://www.w3.org/2005/xpath-functions";

    /// <summary>The version attribute of the stylesheet root element.</summary>
    public string? Version { get; private set; }

    /// <summary>The value of xsl:stylesheet/@input-type-annotations, or null if absent.</summary>
    public string? InputTypeAnnotations { get; private set; }

    /// <summary>
    /// Whether the stylesheet is in forwards-compatible mode (declared version greater
    /// than the implementation supports). In this mode unknown attributes on XSLT
    /// elements are ignored rather than rejected.
    /// </summary>
    public bool IsForwardsCompatible =>
        !string.IsNullOrEmpty(Version) &&
        decimal.TryParse(Version, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var v) &&
        v > 3.0m;

    /// <summary>
    /// Returns the effective XSLT version for the given element, walking ancestors for
    /// an explicit <c>version</c> (XSLT elements) or <c>xsl:version</c> (literal result
    /// elements) attribute and falling back to the global stylesheet version.
    /// </summary>
    public double GetEffectiveVersion(XElement element)
    {
        var ancestor = element;
        while (ancestor != null)
        {
            XAttribute? versionAttr = null;
            if (ancestor.Name.NamespaceName == XslNamespace)
                versionAttr = ancestor.Attribute("version");
            if (versionAttr == null)
                versionAttr = ancestor.Attribute(XName.Get("version", XslNamespace));
            if (versionAttr != null)
            {
                if (double.TryParse(versionAttr.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                    return v;
                break;
            }
            ancestor = ancestor.Parent;
        }
        if (double.TryParse(Version, NumberStyles.Any, CultureInfo.InvariantCulture, out var sv))
            return sv;
        return 3.0;
    }

    /// <summary>
    /// Determines whether the given element is in XSLT forwards-compatible mode.
    /// </summary>
    public bool IsForwardsCompatibleElement(XElement element) => GetEffectiveVersion(element) > 3.0;

    /// <summary>
    /// The set of known XSLT 3.0 element names. Used during static validation to
    /// distinguish unknown XSLT elements (whose descendants may be skipped in
    /// forwards-compatible mode) from recognized ones.
    /// </summary>
    public static readonly HashSet<string> KnownXsltElementNames = new(StringComparer.Ordinal)
    {
        "stylesheet", "transform", "include", "import", "strip-space", "preserve-space",
        "output", "namespace-alias", "attribute-set", "decimal-format", "key", "mode",
        "accumulator", "accumulator-rule", "variable", "param", "with-param", "template", "function",
        "global-context-item", "context-item", "use-package", "package", "expose", "accept", "override",
        "import-schema",
        "apply-templates", "apply-imports", "call-template", "next-match",
        "value-of", "text", "element", "attribute", "namespace", "copy", "copy-of",
        "comment", "processing-instruction", "document", "result-document",
        "for-each", "for-each-group", "sort", "if", "choose", "when", "otherwise",
        "fallback", "message", "number", "sequence", "perform-sort",
        "analyze-string", "matching-substring", "non-matching-substring",
        "merge", "merge-source", "merge-key", "merge-action",
        "map", "map-entry", "array",
        "try", "catch", "evaluate", "source-document",
        "iterate", "break", "next-iteration", "on-completion",
        "where-populated", "on-empty", "on-non-empty", "assert",
        "character-map", "output-character", "fork"
    };

    /// <summary>
    /// The set of XSLT element names that must be empty (no text or element children;
    /// comments and processing instructions are permitted).
    /// </summary>
    private static readonly HashSet<string> EmptyXsltElementNames = new(StringComparer.Ordinal)
    {
        "include", "import", "strip-space", "preserve-space", "output",
        "namespace-alias", "decimal-format", "output-character",
        "copy-of", "mode", "import-schema", "expose", "accept",
        "global-context-item", "context-item"
    };

    /// <summary>
    /// Throws <c>XTSE0080</c> if the given lexical QName, when expanded in the context of
    /// <paramref name="element"/>, uses a reserved namespace (XSLT, XML Schema, or XPath
    /// functions). Used for names of named templates, attribute sets, and similar constructs.
    /// </summary>
    internal static void ValidateNamedQName(XElement element, string name, string construct)
    {
        if (string.IsNullOrEmpty(name))
            return;

        string? nsUri = null;
        string localName = name.Trim();
        var trimmed = localName;
        if (trimmed.Length > 2 && trimmed[0] == 'Q' && trimmed[1] == '{')
        {
            int closeBrace = trimmed.IndexOf('}');
            if (closeBrace >= 2)
            {
                nsUri = trimmed[2..closeBrace];
                localName = trimmed[(closeBrace + 1)..];
            }
        }
        else
        {
            int colon = trimmed.IndexOf(':');
            if (colon >= 0)
            {
                var prefix = trimmed[..colon];
                localName = trimmed[(colon + 1)..];
                if (prefix == "xml")
                    nsUri = "http://www.w3.org/XML/1998/namespace";
                else
                    nsUri = element.GetNamespaceOfPrefix(prefix)?.NamespaceName;
            }
        }

        // xsl:initial-template is the one XSLT-namespace name permitted for a named template.
        if ((nsUri == XsNamespace || nsUri == FnNamespace ||
             (nsUri == XslNamespace && localName != "initial-template")))
            throw new InvalidOperationException($"XTSE0080: The name '{name}' used in {construct} is in a reserved namespace.");
    }

    /// <summary>
    /// Expands an <see cref="XsQName"/> into Clark notation (<c>Q{namespace}local</c>).
    /// </summary>
    public static string ExpandQName(XsQName qname)
        => $"Q{{{qname.NamespaceUri}}}{qname.LocalName}";

    /// <summary>
    /// Expands a lexical QName in the context of <paramref name="element"/> and returns
    /// it in Clark notation (<c>Q{namespace}local</c>). Supports <c>Q{{uri}}local</c>,
    /// <c>prefix:local</c>, and unprefixed names.
    /// </summary>
    internal static string ExpandQName(XElement element, string name)
    {
        if (string.IsNullOrEmpty(name))
            return string.Empty;

        var trimmed = name.Trim();
        if (trimmed.Length > 2 && trimmed[0] == 'Q' && trimmed[1] == '{')
        {
            int closeBrace = trimmed.IndexOf('}');
            if (closeBrace >= 2)
            {
                var nsUri = trimmed[2..closeBrace];
                var localName = trimmed[(closeBrace + 1)..];
                return $"Q{{{nsUri}}}{localName}";
            }
        }

        int colon = trimmed.IndexOf(':');
        if (colon >= 0)
        {
            var prefix = trimmed[..colon];
            var localName = trimmed[(colon + 1)..];
            if (prefix == "xml")
                return $"Q{{http://www.w3.org/XML/1998/namespace}}{localName}";

            var nsUri = element.GetNamespaceOfPrefix(prefix)?.NamespaceName ?? string.Empty;
            return $"Q{{{nsUri}}}{localName}";
        }

        return $"Q{{}}{trimmed}";
    }

    /// <summary>
    /// Returns the effective xpath-default-namespace for the given element by walking
    /// the ancestor chain and finding the nearest xpath-default-namespace attribute.
    /// </summary>
    internal static string? GetXPathDefaultNamespace(XElement element)
    {
        var current = element;
        while (current != null)
        {
            var attr = current.Attribute(XName.Get("xpath-default-namespace", XslNamespace));
            if (attr != null)
            {
                // XTSE0090: xsl:xpath-default-namespace is not allowed on XSLT elements
                if (current.Name.NamespaceName == XslNamespace)
                    throw new InvalidOperationException("XTSE0090");
                return attr.Value;
            }
            if (current.Name.NamespaceName == XslNamespace)
            {
                attr = current.Attribute("xpath-default-namespace");
                if (attr != null) return attr.Value;
            }
            current = current.Parent;
        }
        return null;
    }

}

/// <summary>
/// Represents a parsed xsl:decimal-format declaration.
/// </summary>
public sealed class DecimalFormatDefinition
{
    public string LocalName { get; init; } = "";
    public string NamespaceUri { get; init; } = "";
    public DecimalFormat Format { get; init; } = new();
    /// <summary>Attributes explicitly set on this xsl:decimal-format element.</summary>
    public HashSet<string> ExplicitAttributes { get; init; } = new();
    /// <summary>The import precedence of the stylesheet module that declared this format.</summary>
    public int ImportPrecedence { get; init; }

    public static DecimalFormatDefinition? FromElement(XElement element, Stylesheet stylesheet)
    {
        var format = new DecimalFormat();
        var explicitAttrs = new HashSet<string>();
        string? localName = null;
        string? nsUri = null;

        foreach (var attr in element.Attributes())
        {
            var name = attr.Name.LocalName;
            var value = attr.Value;

            switch (name)
            {
                case "name":
                    {
                        var nameVal = value.Trim();
                        if (string.IsNullOrEmpty(nameVal)) break;
                        // Resolve QName using element's in-scope namespaces first
                        int colon = nameVal.IndexOf(':');
                        if (colon >= 0)
                        {
                            var prefix = nameVal.Substring(0, colon);
                            localName = nameVal.Substring(colon + 1);
                            nsUri = element.ResolveNamespace(prefix) ?? stylesheet.ResolveNamespace(prefix) ?? "";
                        }
                        else
                        {
                            localName = nameVal;
                            nsUri = "";
                        }
                        break;
                    }
                case "decimal-separator": format.DecimalSeparator = value; explicitAttrs.Add(name); break;
                case "grouping-separator": format.GroupingSeparator = value; explicitAttrs.Add(name); break;
                case "infinity": format.Infinity = value; explicitAttrs.Add(name); break;
                case "minus-sign": format.MinusSign = value; explicitAttrs.Add(name); break;
                case "NaN": format.NaN = value; explicitAttrs.Add(name); break;
                case "percent": format.Percent = value; explicitAttrs.Add(name); break;
                case "per-mille": format.PerMille = value; explicitAttrs.Add(name); break;
                case "zero-digit": format.ZeroDigit = value; explicitAttrs.Add(name); break;
                case "digit": format.Digit = value; explicitAttrs.Add(name); break;
                case "pattern-separator": format.PatternSeparator = value; explicitAttrs.Add(name); break;
                case "exponent-separator": format.ExponentSeparator = value; explicitAttrs.Add(name); break;
            }
        }

        // Validate symbol uniqueness (XTSE1300): only check attributes explicitly set on THIS element
        var explicitSymbols = new Dictionary<string, string>();
        foreach (var attr in explicitAttrs)
        {
            string? sym = attr switch
            {
                "decimal-separator" => format.DecimalSeparator,
                "grouping-separator" => format.GroupingSeparator,
                "percent" => format.Percent,
                "per-mille" => format.PerMille,
                "zero-digit" => format.ZeroDigit,
                "digit" => format.Digit,
                "pattern-separator" => format.PatternSeparator,
                "minus-sign" => format.MinusSign,
                _ => null
            };
            if (!string.IsNullOrEmpty(sym))
                explicitSymbols[attr] = sym;
        }
        foreach (var (r1, v1) in explicitSymbols)
        {
            foreach (var (r2, v2) in explicitSymbols)
            {
                if (r1 == r2) continue;
                if (v1 == v2)
                    throw new InvalidOperationException("XTSE1300");
            }
        }

        // Validate zero-digit is actually a digit with numeric value zero (XTSE1295)
        if (explicitAttrs.Contains("zero-digit") && !string.IsNullOrEmpty(format.ZeroDigit))
        {
            var category = format.ZeroDigit.Length == 1
                ? char.GetUnicodeCategory(format.ZeroDigit[0])
                : char.GetUnicodeCategory(format.ZeroDigit, 0);
            if (category != System.Globalization.UnicodeCategory.DecimalDigitNumber)
                throw new InvalidOperationException("XTSE1295");
            var numericValue = format.ZeroDigit.Length == 1
                ? char.GetNumericValue(format.ZeroDigit[0])
                : char.GetNumericValue(format.ZeroDigit, 0);
            if (numericValue != 0)
                throw new InvalidOperationException("XTSE1295");
        }

        return new DecimalFormatDefinition
        {
            LocalName = localName ?? "",
            NamespaceUri = nsUri ?? "",
            Format = format,
            ExplicitAttributes = explicitAttrs,
            ImportPrecedence = stylesheet.ImportPrecedence
        };
    }
}

/// <summary>
/// Helper methods for stylesheet parsing.
/// </summary>
public static class StylesheetExtensions
{
    /// <summary>
    /// Resolves a namespace prefix in the stylesheet's root element.
    /// </summary>
    public static string? ResolveNamespace(this Stylesheet stylesheet, string prefix)
    {
        var root = stylesheet.RootElement;
        foreach (var attr in root.Attributes())
        {
            if (attr.IsNamespaceDeclaration)
            {
                var attrPrefix = attr.Name.LocalName;
                if (attrPrefix == "xmlns" && string.IsNullOrEmpty(prefix))
                    return attr.Value;
                if (attrPrefix == prefix)
                    return attr.Value;
            }
        }
        return null;
    }

    /// <summary>
    /// Resolves a namespace prefix using the element's own and ancestor namespace declarations.
    /// </summary>
    public static string? ResolveNamespace(this XElement element, string prefix)
    {
        var current = element;
        while (current != null)
        {
            foreach (var attr in current.Attributes())
            {
                if (attr.IsNamespaceDeclaration)
                {
                    var attrPrefix = attr.Name.LocalName;
                    if (attrPrefix == "xmlns" && string.IsNullOrEmpty(prefix))
                        return attr.Value;
                    if (attrPrefix == prefix)
                        return attr.Value;
                }
            }
            current = current.Parent;
        }
        return null;
    }

    /// <summary>
    /// Expands a mode name with optional namespace prefix to Clark notation.
    /// </summary>
    private static string ExpandModeName(string mode, XElement element)
    {
        if (mode == "#current" || mode == "#default" || mode == "#all" || mode == "#unnamed")
            return mode;

        int colon = mode.IndexOf(':');
        if (colon < 0)
            return mode;

        var prefix = mode.Substring(0, colon);
        var local = mode.Substring(colon + 1);

        var current = element;
        while (current != null)
        {
            foreach (var attr in current.Attributes())
            {
                if (attr.IsNamespaceDeclaration && attr.Name.LocalName == prefix)
                {
                    return $"{{{attr.Value}}}{local}";
                }
            }
            current = current.Parent;
        }
        return mode;
    }
}

/// <summary>
/// The kind of name test stored in an <see cref="SpaceHandlingRule"/>.
/// </summary>
public enum SpaceNameTestKind
{
    /// <summary>Matches any element name.</summary>
    Any,
    /// <summary>Matches an exact QName.</summary>
    Exact,
    /// <summary>Matches any element with a specific local name regardless of namespace.</summary>
    WildcardLocal,
    /// <summary>Matches any element within a specific namespace.</summary>
    WildcardNamespace
}

/// <summary>
/// Represents a single xsl:strip-space or xsl:preserve-space rule.
/// </summary>
public readonly struct SpaceHandlingRule
{
    public SpaceNameTestKind Kind { get; }
    public string? LocalName { get; }
    public string? NamespaceUri { get; }
    public bool IsStrip { get; }
    public int Precedence { get; }

    private SpaceHandlingRule(SpaceNameTestKind kind, string? localName, string? namespaceUri, bool isStrip, int precedence)
    {
        Kind = kind;
        LocalName = localName;
        NamespaceUri = namespaceUri;
        IsStrip = isStrip;
        Precedence = precedence;
    }

    /// <summary>
    /// Parses a name test from an <c>@elements</c> value into a <see cref="SpaceHandlingRule"/>.
    /// </summary>
    public static SpaceHandlingRule FromNameTest(string nameTest, XElement declaration, Stylesheet stylesheet, string? defaultNamespace, bool isStrip, int precedence)
    {
        var nt = nameTest.Trim();

        // EQName syntax: Q{namespace-uri}local or Q{namespace-uri}*
        if (nt.StartsWith("Q{"))
        {
            int closeBrace = nt.IndexOf('}');
            if (closeBrace < 2)
                throw new InvalidOperationException($"XTSE0270: Invalid name test '{nameTest}' in xsl:{(isStrip ? "strip" : "preserve")}-space/@elements");
            var nsUri = nt[2..closeBrace];
            var rest = nt[(closeBrace + 1)..];
            if (rest == "*")
                return new SpaceHandlingRule(SpaceNameTestKind.WildcardNamespace, null, nsUri, isStrip, precedence);
            if (rest.Length == 0)
                throw new InvalidOperationException($"XTSE0270: Invalid name test '{nameTest}' in xsl:{(isStrip ? "strip" : "preserve")}-space/@elements");
            return new SpaceHandlingRule(SpaceNameTestKind.Exact, rest, nsUri, isStrip, precedence);
        }

        if (nt == "*")
        {
            return new SpaceHandlingRule(SpaceNameTestKind.Any, null, null, isStrip, precedence);
        }

        if (nt.StartsWith("*:"))
        {
            return new SpaceHandlingRule(SpaceNameTestKind.WildcardLocal, nt[2..], null, isStrip, precedence);
        }

        if (nt.EndsWith(":*"))
        {
            var prefix = nt[..^2];
            var nsUri = declaration.ResolveNamespace(prefix) ?? stylesheet.ResolveNamespace(prefix);
            if (string.IsNullOrEmpty(nsUri))
                throw new InvalidOperationException($"XTSE0280: Undeclared prefix '{prefix}' in xsl:{(isStrip ? "strip" : "preserve")}-space/@elements");
            return new SpaceHandlingRule(SpaceNameTestKind.WildcardNamespace, null, nsUri, isStrip, precedence);
        }

        int colon = nt.IndexOf(':');
        if (colon >= 0)
        {
            var prefix = nt[..colon];
            var localName = nt[(colon + 1)..];
            var nsUri = declaration.ResolveNamespace(prefix) ?? stylesheet.ResolveNamespace(prefix);
            if (string.IsNullOrEmpty(nsUri))
                throw new InvalidOperationException($"XTSE0280: Undeclared prefix '{prefix}' in xsl:{(isStrip ? "strip" : "preserve")}-space/@elements");
            return new SpaceHandlingRule(SpaceNameTestKind.Exact, localName, nsUri, isStrip, precedence);
        }

        // Unprefixed NCName: use the default namespace if one is in scope.
        return new SpaceHandlingRule(SpaceNameTestKind.Exact, nt, defaultNamespace, isStrip, precedence);
    }
}
