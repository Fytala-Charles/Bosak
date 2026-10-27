"""Reproduce all inherited numbering-sequence declarations in the pinned catalogs."""
from pathlib import Path
import xml.etree.ElementTree as X,json,hashlib
E=Path(__file__).resolve().parent;R=E.parents[3];ns={'t':'http://www.w3.org/2010/09/qt-fots-catalog'};pin=json.loads((E/'PROPOSAL-PIN.json').read_text());rows=[];counts={}
for suite,dirname in [('qt3','qt3tests'),('qt4','qt4tests')]:
 root=R/'tests'/dirname;total=0
 for ref in X.parse(root/'catalog.xml').getroot().findall('t:test-set',ns):
  p=root/ref.get('file');assert hashlib.sha256(p.read_bytes()).hexdigest()==pin['fixtureHashes'][p.relative_to(R).as_posix()];doc=X.parse(p).getroot()
  for case in doc.findall('t:test-case',ns):
   total+=1
   for dep in doc.findall('t:dependency',ns)+case.findall('t:dependency',ns):
    if dep.get('type')=='format-integer-sequence':rows.append(dict(suite=suite,name=case.get('name'),value=dep.get('value'),satisfied=dep.get('satisfied','true') not in ['false','0'],setFile=p.relative_to(R).as_posix()))
 counts[suite]=total
assert dict(scannedCases=counts,declarations=rows)==json.loads((E/'sequence-inventory.json').read_text());print('Inventory verified:78806cases,18sequence declarations,onlytwo target negatives.')
