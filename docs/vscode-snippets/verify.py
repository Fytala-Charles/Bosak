"""Verify retained snippet qualification without launching the engine or publishing."""
from pathlib import Path
import hashlib,json,zipfile,xml.etree.ElementTree as ET,subprocess
D=Path(__file__).resolve().parent;W=D.parents[1]
sha=lambda b:hashlib.sha256(b).hexdigest()
read=lambda p:json.loads(p.read_bytes())
for f,h in read(D/'MANIFEST.json').items():assert sha((D/f).read_bytes())==h,f
v=read(D/'VALIDATION.json');assert v['success'] and not v['engineTouched'] and not v['published']
for f,h in v['inputSha256'].items():assert sha((W/f).read_bytes())==h,f
ns={'t':'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
with zipfile.ZipFile(D/'EVIDENCE.zip') as z:
 assert len(z.namelist())==len(set(z.namelist()))
 total=0
 for row in v['solutionTests']['projects']:
  b=z.read(row['receipt'].replace('\\','/'));assert sha(b)==row['sha256'];c=ET.fromstring(b).find('.//t:Counters',ns);assert c.get('failed')=='0';total+=int(c.get('passed'))
 assert total==26488
 r=ET.fromstring(z.read(v['languageServer']['receipt'].replace('\\','/')));c=r.find('.//t:Counters',ns);assert c.get('failed')=='0' and c.get('passed')=='111'
 assert len([x for x in r.findall('.//t:UnitTestResult',ns) if 'SnippetExpansionTests.' in x.get('testName','')])==21
 files=z.read('VSIX-FILE-LIST.txt').decode('utf-8-sig')
 for language in ['xslt','xquery','xpath']:assert 'snippets/'+language+'.json' in files
assert not subprocess.check_output(['git','diff',v['sourceBase'],'--name-only','--','src'],cwd=W,text=True)
print('Snippet evidence verified: exact inputs, 26,488 solution + 111 language-server tests, three packaged JSON files; engine source unchanged.')
