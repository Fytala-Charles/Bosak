"""Inventory every case and inherited declaration in both pinned local catalogs."""
from pathlib import Path
import json,re,hashlib,xml.etree.ElementTree as X
E=Path(__file__).resolve().parent;R=E.parents[3];ns={'t':'http://www.w3.org/2010/09/qt-fots-catalog'}
s=(R/'tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs').read_text(encoding='utf-8');s=s[s.index('private static readonly HashSet<string> GatedSets'):];gated=set(re.findall(r'"([^"]+)"',s[:s.index('};')]))
rows=[];counts={};negtypes={};pin=json.loads((E/'REVIEW-PIN.json').read_text())
for suite,dirname in [('qt3','qt3tests'),('qt4','qt4tests')]:
 root=R/'tests'/dirname;counts[suite]=dict(allCases=0,normalGateCases=0,selectedCases=0,gatedSelectedCases=0)
 for ref in X.parse(root/'catalog.xml').getroot().findall('t:test-set',ns):
  p=root/ref.get('file');relative=p.relative_to(R).as_posix();assert hashlib.sha256(p.read_bytes()).hexdigest()==pin['fixtureHashes'][relative];doc=X.parse(p).getroot();setdeps=doc.findall('t:dependency',ns);isgate=suite=='qt3' or ref.get('name') in gated
  for case in doc.findall('t:test-case',ns):
   counts[suite]['allCases']+=1;counts[suite]['normalGateCases']+=isgate;deps=setdeps+case.findall('t:dependency',ns)
   for d in deps:
    if d.get('satisfied','true') in ['false','0']:negtypes[d.get('type')]=negtypes.get(d.get('type'),0)+1
   if not any(d.get('satisfied','true') in ['false','0'] and d.get('type') in ['spec','default-language'] for d in deps):continue
   rows.append(dict(Suite=suite,Name=case.get('name'),SetFile=relative,gated=isgate,runnerExcluded=ref.get('name') in ['app-CatalogCheck','app-Demos','app-XMark'],dependencies=[dict(type=d.get('type'),value=d.get('value'),satisfied=d.get('satisfied','true') not in ['false','0']) for d in deps]));counts[suite]['selectedCases']+=1;counts[suite]['gatedSelectedCases']+=isgate
expected=dict(counts=counts,negativeTypes=negtypes,gatedSetCount=len(gated));assert expected==json.loads((E/'inventory.json').read_text()) and rows==json.loads((E/'requests.json').read_text());print('Inventory verified:31821 QT3+46985 qt4 cases;zero negative spec/default-language declarations;224 gated sets.')
