"""Validate retained candidate evidence; add --local-artifacts to check local nupkgs."""
from pathlib import Path
import hashlib,json,re,subprocess,sys,zipfile,xml.etree.ElementTree as E

root=Path(__file__).resolve().parents[3]
evidence=Path(__file__).resolve().parent
proof=json.loads((root/'docs/RELEASE-1.26.3-CANDIDATE.json').read_text(encoding='utf-8-sig'))
revision='08fbdc31ac79baac04cf00f4773ec6d0b2dd247f'
assert proof['sourceRevision']==revision and proof['version']=='1.26.3'
def digest(data):return hashlib.sha256(data).hexdigest()
for name,wanted in proof['evidenceHashes'].items():
    path=evidence/name
    assert path.resolve().is_relative_to(evidence.resolve())
    assert digest(path.read_bytes())==wanted,name
actualTree=subprocess.check_output(['git','rev-parse',revision+'^{tree}'],cwd=root,text=True).strip()
assert actualTree==proof['sourceTree']
names={}
with zipfile.ZipFile(evidence/'test-receipts.zip') as archive:
    assert len(archive.namelist())==12
    for lane,summary,expected in [('full',proof['sourceTests'],21798),('lsp',proof['languageServerTests'],84),('focused-results',proof['packageOnlyConsumers']['focused'],11175)]:
        total=0;names[lane]=[]
        for row in summary['receipts']:
            raw=archive.read(lane+'/'+Path(row['path']).name)
            assert digest(raw)==row['sha256']
            xml=E.fromstring(raw);counter=xml.find('.//{*}Counters')
            assert int(counter.get('passed'))==int(counter.get('total'))==row['passed']
            assert int(counter.get('failed'))==int(counter.get('notExecuted'))==0
            total+=row['passed'];names[lane].extend(n.get('testName','') for n in xml.findall('.//{*}UnitTestResult'))
        assert total==summary['passed']==expected
with zipfile.ZipFile(evidence/'package-consumer-source.zip') as archive:
    for row in proof['packageOnlyConsumers']['sourceFilesCopiedByteIdentically']:
        name=Path(row['file']).stem
        assert digest(archive.read('focused/'+row['file']))==row['sha256']
        assert sum('.'+name+'.' in n for n in names['full'])==row['expectedCases']==sum('.'+name+'.' in n for n in names['focused-results'])
    for row in proof['consumerPlan']['consumerAdaptations']:
        assert digest(archive.read(row['file']))==row['adaptedSha256'],row['file']
    for kind,project in [('broad','Smoke.csproj'),('focused','Req135.PublicConsumer.csproj')]:
        xml=E.fromstring(archive.read(kind+'/'+project))
        assert not xml.findall('.//ProjectReference') and not xml.findall('.//Reference')
assert len(proof['packages'])==10 and len(proof['assemblyIdentities'])==19
for row in proof['packages']:
    assert row['version']=='1.26.3' and row['repositoryCommit']==revision
    if '--local-artifacts' in sys.argv:
        path=Path(proof['artifacts']['packages'])/(row['package']+'.1.26.3.nupkg')
        assert digest(path.read_bytes())==row['archiveSha256']
        with zipfile.ZipFile(path) as archive:
            assert '.signature.p7s' not in archive.namelist()
            assert digest(archive.read('lib/net10.0/'+row['package']+'.dll'))==row['dllSha256']
for row in proof['assemblyIdentities']:
    assert row['version']=='1.26.3.0' and row['informationalVersion']=='1.26.3+'+revision
gates=proof['conformance']['results']
assert [(row['exitCode'],row['counts']['Passed'],row['counts']['Failed'],row['counts']['Skipped']) for row in gates]==[(0,8436,0,1894),(0,31142,0,679),(2,22,2,0)]
for filename in ['RELEASE-1.26.3-CANDIDATE.md','RELEASE-NOTES-1.26.3.md']:
    path=root/'docs'/filename
    for link in re.findall(r'\]\(([^)]+)\)',path.read_text(encoding='utf-8-sig')):
        if '://' not in link:assert (path.parent/link.split('#')[0]).exists(),link
assert not subprocess.check_output(['git','diff',revision,'--name-only','--','src','tests','AGENTS.md','.gitmodules','.github','assets','.crossnote'],cwd=root,text=True).strip()
if '--staged' in sys.argv:
    for name,wanted in proof['evidenceHashes'].items():
        path=(evidence/name).relative_to(root).as_posix()
        raw=subprocess.check_output(['git','cat-file','blob',':'+path],cwd=root)
        assert digest(raw)==wanted,path
print('Verified pinned source/tree, all evidence bytes, 12 raw TRXs, 3061 copied public cases, 19 identities and frozen-gate receipts.')

