"""Build independent namespace-scope controls; never edit the original catalog or assertions."""
from pathlib import Path
import copy
import json
import xml.etree.ElementTree as X

E = Path(__file__).resolve().parent
R = E.parents[3]
NS = 'http://www.w3.org/2010/09/qt-fots-catalog'
S = '{'+NS+'}'
XS = 'http://www.w3.org/2001/XMLSchema'
resources = E/'controls'
resources.mkdir(exist_ok=True)
original = X.parse(R/'tests/qt3tests/prod/ContextItemDecl.xml').getroot()
metadata = {r['case']:r['result'] for r in json.loads((E/'original-receipts.json').read_text(encoding='utf-8')) if r['mode']=='resources'}
doc = X.Element(S+'test-set',name='C27-namespace-controls')
X.SubElement(doc,S+'dependency',type='spec',value='XQ31+')
requests = []

def request(case, purpose):
    requests.append(dict(Suite='qt3',Name=case.get('name'),SuiteRoot=str(R/'tests/qt3tests'),SetFile=str(E/'control-fixtures.xml'),Purpose=purpose))

for number in ['056','057']:
    name='contextDecl-'+number
    case=copy.deepcopy(next(c for c in original.findall(S+'test-case') if c.get('name')==name))
    case.set('name','C27-original-'+number+'-importer-u-bound')
    env=case.find(S+'environment')
    X.SubElement(env,S+'namespace',prefix='u',uri='http://www.w3.org/XQueryTest/unionListDefined')
    for schema, actual in zip(env.findall(S+'schema'),metadata[name]['Schemas']):schema.set('file',actual['FilePath'])
    for module, actual in zip(case.findall(S+'module'),metadata[name]['Modules']):module.set('file',actual['FilePath'])
    doc.append(case)
    request(case,'Diagnostic importer binding only; unchanged original expression/assertion and original resources')

modules = {
    'builtin':("urn:c27:lib", "module namespace m='urn:c27:lib'; declare context item as xs:integer external; declare variable $m:v := .;"),
    'alias':("urn:c27:lib", f"module namespace m='urn:c27:lib'; declare namespace t='{XS}'; declare context item as t:integer external; declare variable $m:v := .;"),
    'schema':("urn:c27:other", "module namespace m='urn:c27:other'; import schema namespace t='urn:c27:schema'; declare context item as t:integer external; declare variable $m:v := .;"),
    'schema-distinct':("urn:c27:other", "module namespace m='urn:c27:other'; import schema namespace s='urn:c27:schema'; declare context item as s:integer external; declare variable $m:v := .;"),
}
for name, (_,source) in modules.items():(resources/(name+'.xq')).write_text(source,encoding='utf-8')
(resources/'types.xsd').write_text(f'''<xs:schema xmlns:xs="{XS}" targetNamespace="urn:c27:schema"><xs:simpleType name="integer"><xs:restriction base="xs:string"/></xs:simpleType></xs:schema>''',encoding='utf-8')

def synthetic(name, module_names, bindings, initial, error=None, schema=False):
    case=X.SubElement(doc,S+'test-case',name='C27-'+name)
    env=X.SubElement(case,S+'environment')
    for prefix,uri in bindings.items():X.SubElement(env,S+'namespace',prefix=prefix,uri=uri)
    if schema:X.SubElement(env,S+'schema',uri='urn:c27:schema',file=str(resources/'types.xsd'))
    X.SubElement(env,S+'context-item',select=initial)
    for module in module_names:
        uri,_=modules[module]
        X.SubElement(case,S+'module',uri=uri,file=str(resources/(module+'.xq')))
    query="import module namespace m='urn:c27:lib'; "
    if len(module_names)>1:query+="import module namespace n='urn:c27:other'; "
    query+="declare context item external; $m:v eq 4"
    X.SubElement(case,S+'test').text=query
    result=X.SubElement(case,S+'result')
    X.SubElement(result,S+'error',code=error) if error else X.SubElement(result,S+'assert-true')
    request(case,'Independent library declaration namespace/type constraint control')

synthetic('builtin-prefix-baseline',['builtin'],{},'4')
synthetic('library-alias-only',['alias'],{},'4')
synthetic('matching-importer-alias',['alias'],{'t':XS},'4')
synthetic('conflicting-importer-alias',['alias'],{'t':'urn:c27:wrong'},'4')
synthetic('library-alias-wrong-value',['alias'],{},"'London'",'XPTY0004')
synthetic('colliding-lexical-types',['alias','schema'],{'t':XS},'4','XPTY0004',True)
synthetic('distinct-lexical-types',['alias','schema-distinct'],{'t':XS,'s':'urn:c27:schema'},'4','XPTY0004',True)

# Union membership supplies a genuine positive first constraint, so an expected-error
# result cannot pass merely because an earlier alias was rejected incorrectly.
for suffix, members in [('left','xs:decimal xs:string'),('right','xs:date xs:time')]:
    (resources/('union-'+suffix+'.xsd')).write_text(f'<xs:schema xmlns:xs="{XS}" targetNamespace="urn:c27:union-{suffix}"><xs:simpleType name="choice"><xs:union memberTypes="{members}"/></xs:simpleType></xs:schema>',encoding='utf-8')
for prefix, suffix, filename in [('t','left','union-left'),('t','right','union-right'),('r','right','union-distinct')]:
    module='a' if suffix=='left' else 'b'
    (resources/(filename+'.xq')).write_text(f"module namespace {module}='urn:c27:module-{suffix}'; import schema namespace {prefix}='urn:c27:union-{suffix}'; declare context item as {prefix}:choice external; declare variable ${module}:v := .;",encoding='utf-8')
for distinct in [False,True]:
    case=X.SubElement(doc,S+'test-case',name='C27-'+('distinct' if distinct else 'colliding')+'-union-type-contexts')
    env=X.SubElement(case,S+'environment')
    X.SubElement(env,S+'namespace',prefix='t',uri='urn:c27:union-left')
    if distinct:X.SubElement(env,S+'namespace',prefix='r',uri='urn:c27:union-right')
    for side in ['left','right']:X.SubElement(env,S+'schema',uri='urn:c27:union-'+side,file=str(resources/('union-'+side+'.xsd')))
    X.SubElement(env,S+'context-item',select='4')
    X.SubElement(case,S+'module',uri='urn:c27:module-left',file=str(resources/'union-left.xq'))
    X.SubElement(case,S+'module',uri='urn:c27:module-right',file=str(resources/('union-distinct.xq' if distinct else 'union-right.xq')))
    X.SubElement(case,S+'test').text="import module namespace a='urn:c27:module-left'; import module namespace b='urn:c27:module-right'; declare context item external; $a:v eq 4"
    X.SubElement(X.SubElement(case,S+'result'),S+'error',code='XPTY0004')
    request(case,'Both modules constrain the same item: decimal/string accepts integer4, date/time rejects it; distinguish declaration contexts')
case=X.SubElement(doc,S+'test-case',name='C27-union-left-positive')
env=X.SubElement(case,S+'environment')
X.SubElement(env,S+'namespace',prefix='t',uri='urn:c27:union-left')
X.SubElement(env,S+'schema',uri='urn:c27:union-left',file=str(resources/'union-left.xsd'))
X.SubElement(env,S+'context-item',select='4')
X.SubElement(case,S+'module',uri='urn:c27:module-left',file=str(resources/'union-left.xq'))
X.SubElement(case,S+'test').text="import module namespace a='urn:c27:module-left'; declare context item external; $a:v eq 4"
X.SubElement(X.SubElement(case,S+'result'),S+'assert-true')
request(case,'Positive witness that integer4 satisfies the first union constraint independently')
(E/'control-fixtures.xml').write_bytes(X.tostring(doc,encoding='utf-8',xml_declaration=True))
(E/'control-requests.json').write_text(json.dumps(requests,indent=2)+'\n',encoding='utf-8')
assert len(requests)==12
print('Twelve namespace/type controls prepared; original fixtures unchanged.')
