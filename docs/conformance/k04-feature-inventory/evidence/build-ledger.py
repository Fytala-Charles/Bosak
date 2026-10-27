"""Separate frozen admission decisions, original gate observations and capability evidence."""
from pathlib import Path
import json,xml.etree.ElementTree as X,collections
E=Path(__file__).resolve().parent;R=E.parents[3];ns={'t':'http://www.w3.org/2010/09/qt-fots-catalog'}
pol=json.loads((E/'admission-policy.json').read_text());inv=json.loads((E/'inventory.json').read_text())
def skips(p):
 b=p.read_bytes();s=b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig');return {l for l in s.splitlines() if l.strip() and not l.startswith('=== ')}
notes={
 'schemaImport':('src/Bosak.XPath.Providers/XDocument/XdmSchemaAnnotator.cs','Bounded schema-import admission;not full schema-awareness or XSD1.1 certification. Bosak.Schema owns staged XSD1.1.'),
 'schemaValidation':('src/Bosak.XPath.Providers/XDocument/XdmSchemaAnnotator.cs','Bounded schema-validation admission;actual positive and expected-error assertions differ;no general XSD1.1 capability inferred.'),
 'typedData':('src/Bosak.XPath.Providers/XDocument/XdmSchemaAnnotator.cs','QT3 legacy admission and qt4 unsupported declaration differ. Harness PSVI source support requires separate evidence.'),
 'higherOrderFunctions':('src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs','Function registry and higher-order function implementation provide a mechanism;gate observations cover only original admitted cases.'),
 'advanced-uca-fallback':('src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs','Collation functions use the configured implementation;the profile is not certification of every UCA option.'),
 'simple-uca-fallback':('src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs','Collation functions and documented fallback behavior;only original assertion observations are counted.'),
 'non_unicode_codepoint_collation':('src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs','Configured collation routes exist;QT3 legacy admission and qt4 unsupported declaration differ.'),
 'fn-format-integer-CLDR':('src/Bosak.XPath.Standard/Functions/FormatIntegerEngine.cs','Formatting/word handling exists;it does not prove complete CLDR numbering-scheme support. QT3 and qt4 declarations differ.'),
 'fn-load-xquery-module':('src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs','LoadXQueryModuleStub delegates through EvaluationContext.XQueryModuleLoader;absence of the hook raises FOQM0001.'),
 'moduleImport':('src/Bosak.XQuery/Api/XQueryCompiler.cs','XQuery module integration is separate from the XPath-only qt4 routing profile.'),
 'fn-transform-XSLT':('src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs','Transformation dispatch depends on the configured processor/hook;profile admission is not edition-wide certification.'),
 'fn-transform-XSLT30':('src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs','XSLT3 processor integration has bounded tests;this inventory does not certify full XSLT3 compliance.'),
 'namespace-axis':('src/Bosak.XPath.Providers/XDocument/XDocumentNode.cs','Provider namespace-axis implementation;only original observed assertions are counted.'),
 'infoset-dtd':('src/Bosak.XPath.Providers/XDocument/XDocumentNode.cs','Provider node/attribute metadata exists;full DTD Infoset capability is not certified by a metadata flag.'),
 'serialization':('src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs','Serializer/output dispatch exists;the feature declaration is stronger than merely producing a string.'),
 'directory-as-collection-uri':('src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs','ResolveCollection has a directory fallback after configured environment/host routes.'),
 'non_empty_sequence_collection':('src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs','ResolveCollection uses registered and host collections;depends on the actual test environment.'),
 'collection-stability':('src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs','Current positive legacy admission makes negative cases skip;stability behavior needs an independent review.'),
 'remote_http':('src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs','Document/resource loading routes exist;the timeanddate.com Cloudflare exclusion remains an environment limitation.'),
 'expath-file':(None,'Legacy qt4 admission retained for exploratory cases;implementation support is unverified. This is an explicit follow-up, not a support claim.')
}
rows=[]
for suite,dirname,baseline in [('qt3','qt3tests','docs/conformance/k04-profile-review/evidence/qt3-skips.txt'),('qt4','qt4tests','docs/conformance/k04-profile-review/evidence/qt4-skips.txt')]:
 rejected=skips(R/baseline);casegroups=collections.defaultdict(set);gategroups=collections.defaultdict(set)
 gatefiles={e['setFile'] for g in inv['groups'] if g['suite']==suite and g['gated'] for e in g['examples']}
 # Determine gated membership from the pinned catalog's set names via the runner source.
 import re
 source=(R/'tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs').read_text(encoding='utf-8');source=source[source.index('private static readonly HashSet<string> GatedSets'):];gated=set(re.findall(r'"([^"]+)"',source[:source.index('};')]))
 root=R/'tests'/dirname
 for ref in X.parse(root/'catalog.xml').getroot().findall('t:test-set',ns):
  doc=X.parse(root/ref.get('file')).getroot();isgate=suite=='qt3' or ref.get('name') in gated
  for c in doc.findall('t:test-case',ns):
   for dep in doc.findall('t:dependency',ns)+c.findall('t:dependency',ns):
    if dep.get('type')!='feature':continue
    key=dep.get('value');identity=c.get('name');casegroups[key].add(identity)
    if isgate:gategroups[key].add(identity)
 before={r['id']:r for r in map(json.loads,(E/(suite+'-before.jsonl')).read_text(encoding='utf-8-sig').splitlines())}
 for feature in inv['catalogs'][suite]['featureValues']:
  positive=before[f'feature-{suite}-{feature}-True']['xpathSupported'];negative=before[f'feature-{suite}-{feature}-False']['xpathSupported'];source,note=notes.get(feature,(pol[suite]['source'],'Explicit unsupported declaration in the current harness;engine capability is not inferred independently.'))
  rows.append(dict(suite=suite,feature=feature,positiveAdmission=positive,negativeAdmission=negative,profileStatus='frozen-legacy-admitted' if positive else 'declared-unsupported',catalogCases=len(casegroups[feature]),gatedCases=len(gategroups[feature]),gatedOriginalAssertionsPassed=len(gategroups[feature]-rejected),gatedOriginalSkipped=len(gategroups[feature]&rejected),exploratoryCasesNotExecuted=len(casegroups[feature]-gategroups[feature]),sourceEvidence=source,scopeNote=note))
(E/'capability-ledger.json').write_text(json.dumps(dict(sourceBase='8ea5e4ae',admissionIsNotCapabilityCertification=True,observations='Original accepted gates at source base;expected-error assertions may pass without full feature support;counts overlap across features',features=rows),indent=2)+'\n',encoding='utf-8');print('Feature ledger rows:',len(rows))
