"""Verify retained publication receipts; --local-artifacts also checks public nupkgs."""
from pathlib import Path
import hashlib,json,subprocess,sys,zipfile,xml.etree.ElementTree as E
root=Path(__file__).resolve().parents[3];evidence=Path(__file__).resolve().parent
proof=json.loads((root/'docs/RELEASE-1.26.1-PUBLICATION.json').read_text(encoding='utf-8-sig'))
revision='65417cdb3bff184ca9ff1f624582f55ca4681db9'
assert proof['revision']==revision and proof['status']=='PUBLISHED_AND_INDEPENDENTLY_VERIFIED'
def sha(data):return hashlib.sha256(data).hexdigest()
for name,wanted in proof['evidenceHashes'].items():
 path=evidence/name;assert path.resolve().is_relative_to(evidence.resolve())
 assert sha(path.read_bytes())==wanted,name
 if '--staged' in sys.argv:
  staged=subprocess.check_output(['git','cat-file','blob',':'+path.relative_to(root).as_posix()],cwd=root)
  assert sha(staged)==wanted,name
assert subprocess.check_output(['git','rev-parse','v1.26.1^{}'],cwd=root,text=True).strip()==revision
workflow=proof['workflow'];assert workflow['solutionTestsPassed']==14536 and workflow['uploadAcknowledgments']==10 and workflow['releaseRunCountForTag']==1
assert not proof['retagged'] and not proof['duplicateDispatchPerformed']
signatures=proof['signatures'];downloads=json.loads((evidence/'public-downloads.json').read_text(encoding='utf-8-sig'));assert len(downloads)==len(signatures)==10
for row in downloads:
 assert row['available'] and row['indexListed'] and row['repositorySignaturePresent'] and row['approvedGitAssetBytesMatch'] and row['revision']==revision
 sig=next(x for x in signatures if x['package']==row['id']+'.1.26.1.nupkg')
 assert sig['exitCode']==0 and sig['archiveSha256']==row['sha256']
 if '--local-artifacts' in sys.argv:assert sha(Path(row['archive']).read_bytes())==row['sha256']
consumer=proof['publicConsumerProvenance'];assert consumer['publicFocusedTests']==3913 and consumer['broadChecks']==153 and consumer['nugetOrgOnly']
for kind,expected in [('broad-public',10),('focused-public',9)]:
 assert len(consumer[kind]['assemblyMetadata'])==len(consumer[kind]['packages'])==expected
 for row in consumer[kind]['assemblyMetadata']:
  assert row['version']=='1.26.1.0' and row['informationalVersion']=='1.26.1+'+revision
  assert row['sha256']==next(x for x in downloads if x['id']==row['id'])['assemblySha256']
 for row in consumer[kind]['packages']:assert row['source']=='https://api.nuget.org/v3/index.json' and row['archiveCacheOutputMatch']
with zipfile.ZipFile(evidence/'public-consumer-receipts.zip') as archive:
 trx=[n for n in archive.namelist() if n.endswith('.trx')];assert len(trx)==1
 xml=E.fromstring(archive.read(trx[0]));c=xml.find('.//{*}Counters');assert c.get('total')==c.get('passed')=='3913' and c.get('failed')==c.get('notExecuted')=='0'
 names=[n.get('testName','') for n in xml.findall('.//{*}UnitTestResult')]
 for name,count in consumer['addedPublicCases'].items():assert sum(name+'.' in n for n in names)==count
 with zipfile.ZipFile(evidence/'public-consumer-source.zip') as source:
  candidate=json.loads((root/'docs/RELEASE-1.26.1-CANDIDATE.json').read_text(encoding='utf-8-sig'))
  for row in candidate['packageOnlyConsumers']['sourceFilesCopiedByteIdentically']:assert sha(source.read('focused/'+row['file']))==row['sha256']
assert not subprocess.check_output(['git','diff','f82d715003d0c7fc5410660cb74ec52807db5606','--name-only','--','src','tests','.github','assets','.crossnote','AGENTS.md','docs/RELEASE-1.26.1-CANDIDATE.json','docs/release-evidence/1.26.1'],cwd=root,text=True).strip()
print('Verified publication evidence, approved tag, single release, ten signatures, 19 identities and 3913 public tests; original candidate evidence unchanged.')
