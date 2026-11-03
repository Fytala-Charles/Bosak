"""Require a passed TRX result for every W1-A acceptance prefix; never accept zero tests."""
from pathlib import Path
import argparse,json,sys,xml.etree.ElementTree as ET,hashlib
P=Path(__file__).resolve().parents[1]
def main():
 a=argparse.ArgumentParser(description=__doc__);a.add_argument('--trx',required=True,type=Path);a.add_argument('--validate-only',action='store_true');v=a.parse_args();data=json.loads((P/'ACCEPTANCE.json').read_text())
 results=[r.attrib for r in ET.parse(v.trx).getroot().iter() if r.tag.endswith('UnitTestResult')]
 required=[c for c in data['cases'] if c['requiredFor']=='W1-A neutral admission']+[data['additionalDrainControl']]
 if any(r.get('outcome')!='Passed' for r in results):raise ValueError('TRX contains non-passing results')
 records=[]
 for c in required:
  matches=[r for r in results if c['plannedTestNamePrefix'] in r.get('testName','')]
  if not matches:raise ValueError('Missing executed acceptance case '+c['id'])
  dst=P/c['receipt']
  if not v.validate_only and dst.exists():raise ValueError('Refuse receipt overwrite '+str(dst))
  records.append((dst,{'id':c['id'],'status':'Passed','trxSha256':hashlib.sha256(v.trx.read_bytes()).hexdigest(),'results':matches}))
 if not v.validate_only:
  for dst,record in records:dst.parent.mkdir(parents=True,exist_ok=True);dst.write_text(json.dumps(record,indent=2)+'\n')
 print('All 18 W1-A acceptance prefixes passed; downstream obligations remain unqualified.')
if __name__=='__main__':
 try:main()
 except (ValueError,OSError,ET.ParseError) as e:print(str(e),file=sys.stderr);sys.exit(2)
