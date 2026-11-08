"""Inventory original context declarations and select resource-bounded QT3 diagnostics."""
from pathlib import Path
import hashlib
import json
import re
import xml.etree.ElementTree as X

E = Path(__file__).resolve().parent
R = E.parents[3]
NS = 'http://www.w3.org/2010/09/qt-fots-catalog'
ns = {'t':NS}

def write(name, obj):
    (E/name).write_text(json.dumps(obj,indent=2)+'\n',encoding='utf-8')

rows, hashes = [], {}
for suite in ['qt3','qt4']:
    root = R/'tests'/(suite+'tests')
    catalog = root/'catalog.xml'
    hashes[catalog.relative_to(R).as_posix()] = hashlib.sha256(catalog.read_bytes()).hexdigest()
    cat = X.parse(catalog).getroot()
    ref = next(n for n in cat.findall('t:test-set',ns) if n.get('name')=='prod-ContextItemDecl')
    path = root/ref.get('file')
    hashes[path.relative_to(R).as_posix()] = hashlib.sha256(path.read_bytes()).hexdigest()
    doc = X.parse(path).getroot()
    for case in doc.findall('t:test-case',ns):
        env = case.find('t:environment',ns)
        envref = env.get('ref') if env is not None else None
        resolved = env
        if envref:
            resolved = next((n for n in cat.findall('t:environment',ns) if n.get('name')==envref),None)
            assert resolved is not None, envref
        fields = [n.tag.rsplit('}',1)[-1] for n in resolved] if resolved is not None else []
        resources = sorted(set(fields)&{'source','schema','param','collection'})
        modules = case.findall('t:module',ns)
        expression = case.findtext('t:test',default='',namespaces=ns)
        main = bool(re.search(r'\bdeclare\s+context\s+item\b',expression))
        eligible = suite=='qt3' and main and not resources and not modules
        reason = 'Selected bounded resource-free main declaration' if eligible else ('qt4 XQuery applicability not executed' if suite=='qt4' else 'Module mapping present' if modules else 'Resource-bearing environment' if resources else 'No main-module declaration')
        deps = [{'Type':n.get('type'),'Value':n.get('value'),'Satisfied':n.get('satisfied','true') not in ['false','0']} for n in doc.findall('t:dependency',ns)+case.findall('t:dependency',ns)]
        rows.append(dict(Suite=suite,Name=case.get('name'),SuiteRoot=str(root),SetFile=str(path),Expression=expression,Dependencies=deps,Assertion=X.tostring(case.find('t:result',ns),encoding='unicode'),EnvironmentRef=envref,EnvironmentFields=fields,Resources=resources,ModuleCount=len(modules),MainDeclaration=main,Selected=eligible,Disposition=reason))
assert len(rows)==97 and sum(r['Selected'] for r in rows)==31
write('inventory.json',{'cases':rows,'fixtureHashes':hashes})
write('selected-originals.json',[r for r in rows if r['Selected']])
write('filter-requests.json',rows)

controls = [
    ('baseline-arithmetic','3 + 4','assert-eq','7',None),
    ('baseline-variable','declare variable $x := 3; $x+4','assert-eq','7',None),
    ('baseline-external-focus','.', 'assert-eq','27','27'),
    ('internal','declare context item := 3; .+4','assert-eq','7',None),
    ('typed-internal','declare context item as xs:integer := 3; .','assert-eq','3',None),
    ('external-default','declare context item external := 5; .','assert-eq','5',None),
    ('external-supplied','declare context item as xs:integer external; .','assert-eq','27','27'),
    ('external-overrides-default','declare context item external := 5; .','assert-eq','27','27'),
    ('wrong-internal-type',"declare context item as xs:integer := 'London'; .",'error','XPTY0004',None),
    ('wrong-external-type','declare context item as xs:integer external; .','error','XPTY0004',"'London'"),
    ('empty-internal','declare context item := (); .','error','XPTY0004',None),
    ('plural-internal','declare context item := (1,2); .','error','XPTY0004',None),
    ('duplicate','declare context item := 1; declare context item := 2; .','error','XQST0099',None),
    ('singleton-focus','declare context item := 3; concat(position(), ":",last())','assert-string-value','1:1',None),
    ('forward-variable','declare context item := $x; declare variable $x := 7; .','assert-eq','7',None),
    ('variable-uses-focus','declare variable $x := . + 5; declare context item := 17; $x','assert-eq','22',None),
    ('cycle','declare variable $x := .; declare context item := $x; .','error','XQDY0054',None),
    ('comments','declare (:c:) context (:c:) item := 3; .','assert-eq','3',None),
]
doc = X.Element('{'+NS+'}test-set',name='C24-context-controls')
requests = []
for name,expression,kind,expected,initial in controls:
    case = X.SubElement(doc,'{'+NS+'}test-case',name='C24-'+name)
    if initial is not None:
        env = X.SubElement(case,'{'+NS+'}environment')
        X.SubElement(env,'{'+NS+'}context-item',select=initial)
    X.SubElement(case,'{'+NS+'}test').text = expression
    result = X.SubElement(case,'{'+NS+'}result')
    if kind=='error':
        X.SubElement(result,'{'+NS+'}error',code=expected)
    else:
        X.SubElement(result,'{'+NS+'}'+kind).text = expected
    requests.append(dict(Suite='qt3',Name='C24-'+name,SuiteRoot=str(R/'tests/qt3tests'),SetFile=str(E/'control-fixtures.xml'),Expression=expression,Dependencies=[],ExpectedKind=kind,Expected=expected,InitialContextExpression=initial))
(E/'control-fixtures.xml').write_bytes(X.tostring(doc,encoding='utf-8',xml_declaration=True))
write('control-requests.json',requests)
print('Inventoried 97 originals; selected 31 bounded QT3 originals and 18 independent controls.')
