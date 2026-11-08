"""Inventory inherited capability declarations across both pinned catalogs."""
from pathlib import Path
import xml.etree.ElementTree as X,collections,re,json,hashlib
E=Path(__file__).resolve().parent;R=E.parents[3];ns={'t':'http://www.w3.org/2010/09/qt-fots-catalog'}
pin=json.loads((E/'INVENTORY-PIN.json').read_text());source=(R/'tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs').read_text(encoding='utf-8');source=source[source.index('private static readonly HashSet<string> GatedSets'):];gated=set(re.findall(r'"([^"]+)"',source[:source.index('};')]))
result={};groups=[]
for suite,dirname in [('qt3','qt3tests'),('qt4','qt4tests')]:
 root=R/'tests'/dirname;counts=collections.Counter();examples=collections.defaultdict(list);total=normal=0
 for ref in X.parse(root/'catalog.xml').getroot().findall('t:test-set',ns):
  p=root/ref.get('file');assert hashlib.sha256(p.read_bytes()).hexdigest()==pin['fixtureHashes'][p.relative_to(R).as_posix()];doc=X.parse(p).getroot();isgate=suite=='qt3' or ref.get('name') in gated
  for case in doc.findall('t:test-case',ns):
   total+=1;normal+=isgate
   for d in doc.findall('t:dependency',ns)+case.findall('t:dependency',ns):
    key=(d.get('type'),d.get('value'),d.get('satisfied','true') not in ['false','0'],isgate);counts[key]+=1
    if len(examples[key])<3:examples[key].append(dict(name=case.get('name'),setFile=p.relative_to(R).as_posix()))
 for key,count in sorted(counts.items()):groups.append(dict(suite=suite,type=key[0],value=key[1],satisfied=key[2],gated=key[3],mentions=count,examples=examples[key]))
 result[suite]=dict(allCases=total,normalGateCases=normal,types=sorted({k[0] for k in counts}),featureValues=sorted({k[1] for k in counts if k[0]=='feature'}))
obj=dict(catalogs=result,gatedSetCount=len(gated),groups=groups,countsAreOverlappingMentions=True)
(E/'inventory.json').write_text(json.dumps(obj,indent=2,ensure_ascii=True)+'\n',encoding='utf-8');print(json.dumps({s:dict(allCases=v['allCases'],normalGateCases=v['normalGateCases'],featureNames=len(v['featureValues'])) for s,v in result.items()}))
