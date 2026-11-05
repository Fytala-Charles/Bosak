"""Read-only checks of the source-pinned owner-review handoff; no consumer execution."""
from pathlib import Path
import json,hashlib,re,textwrap,xml.etree.ElementTree as ET,subprocess,sys
P=Path(__file__).resolve().parents[1];ROOT=P.parents[1];sha=lambda b:hashlib.sha256(b).hexdigest()
for n,h in json.loads((P/'MANIFEST.json').read_text()).items():assert sha((P/n).read_bytes())==h,n
inputs=json.loads((P/'INPUTS.json').read_text());assert not inputs['externalCheckoutsModified'] and not inputs['messagesSent']
assert len(inputs['localDocuments'])==7 and len(inputs['onlineIssue'])==2
for row in inputs['localDocuments']+inputs['onlineIssue']:
 assert sha((P/row['snapshot']).read_bytes())==row['sha256']
 if '--check-current' in sys.argv and 'source' in row:assert sha(Path(row['source']).read_bytes())==row['sha256']
for row in inputs['localDocuments']:
 if row['snapshot']!='inputs/xmldb-prior-return.json':assert row['committedDocumentAvailable'] and row['documentMatchesCommitIgnoringLineEndings']
issue=json.loads((P/'inputs/issue-468.json').read_text());comments=json.loads((P/'inputs/issue-468-comments.json').read_text())
assert issue['number']==468 and len(comments)==2 and comments[-1]['id']==6532702411
c=json.loads((P/'CANDIDATE.json').read_text());assert c['sourceRevision']=='0126d492a8930001e0b947a0e27ae732af5a0653'
if '--check-current' in sys.argv:
 proposal=json.loads((ROOT/'docs/foundation-transport-promotion/PATCH.json').read_text())
 for n,v in proposal['files'].items():assert sha(subprocess.check_output(['git','show',c['sourceRevision']+':'+n],cwd=ROOT))==v['afterSha256']
assert sha((ROOT/c['sourceArchivePath']).read_bytes())==c['sourceArchiveSha256']
assert sha((ROOT/c['qualifiedResultsPath']).read_bytes())==c['qualifiedResultsSha256']
assert sha((ROOT/c['publicReflectionSnapshot']['path']).read_bytes())==c['publicReflectionSnapshot']['sha256']
layout=json.loads((ROOT/'docs/foundation-transport136/evidence/LAYOUT.json').read_text());assert c['sevenAssemblies']==layout['seven'] and len(c['sevenAssemblies'])==7 and c['engineAssemblyCount']==10
assert c['publicReflectionSnapshot']['exportedTypes']==78 and c['publicReflectionSnapshot']['declaredMembers']==1201
meta=json.loads((ROOT/c['publicReflectionSnapshot']['path']).read_text());qualified=json.loads((ROOT/c['qualifiedResultsPath']).read_text())
assert len(meta['surface'])==78 and sum(len(t['members']) for t in meta['surface'])==1201 and {a['name']+'.dll':a['sha256'] for a in meta['assemblies']}==c['sevenAssemblies']
assert qualified['version']=='1.36.0' and qualified['unitTests']['passed']==24267 and not qualified['c45Accepted']
assert not c['published'] and c['candidateFeed'] is None and not c['publicProviderSdkAvailable'] and not c['c45Accepted'] and not c['apiFreezeAccepted']
m=json.loads((P/'MATRIX.json').read_text());rows=m['rows'];expected={f'N{i}' for i in range(1,9)}|{f'FXR-{i:02}' for i in range(1,16)}|{f'FXG-{i:02}' for i in range(1,7)}|{f'BF140-{i:02}' for i in range(1,6)}|{f'PF-{i:02}' for i in range(1,10)}
assert len(rows)==len({r['id'] for r in rows})==43 and {r['id'] for r in rows}==expected and all(not r['accepted'] and r['requestedReturn'] for r in rows)
assert not m['allAccepted'] and m['real136ConsumerRuns']=='NotRun'
xml=json.loads((P/'inputs/xmldb-matrix.json').read_text());assert len(xml['matrix'])==15 and len(xml['openItems'])==6 and xml['candidateVersion']=='1.34.2' and xml['validation']['uniqueManifestEntries']==176
assert not xml['requiredContractGapsResolved'] and not xml['freezeAccepted']
for original in xml['matrix']:
 r=next(r for r in rows if r['id']==original['id']);assert r['requiredContract']==original['requiredContract'] and r['title']==original['title']
old=json.loads((P/'inputs/xmldb-prior-return.json').read_text());assert old['candidateSourceRevision']=='c88eea4308c0e49a81e7bfe2f481319f2e4d208c'
template=json.loads((P/'OWNER-RETURN.template.json').read_text());assert template['status']=='NotReturned' and template['requiredGaps'] is None and template['disposition'] is None and template['accountableOwner'] is None and not template['apiFreezeAccepted']
fixture=P/'fixtures';e=json.loads((fixture/'EXPECTED.json').read_text());source=ROOT/e['sourceProbe'];assert sha(source.read_bytes())==e['sourceProbeSha256']
text=source.read_text(encoding='utf-8');schema=textwrap.dedent(re.search(r'const string schema="""\n(.*?)\n """;',text,re.S).group(1))+'\n';instance=re.search(r'var callerXml=XDocument.Parse\("([^"\n]+)"\)',text).group(1)+'\n'
assert (fixture/'invoice.xsd').read_text(encoding='utf-8')==schema and (fixture/'invoice.xml').read_text(encoding='utf-8')==instance
assert sha((fixture/'invoice.xsd').read_bytes())==e['schemaSha256'] and sha((fixture/'invoice.xml').read_bytes())==e['instanceSha256']
assert ET.fromstring(schema).tag=='{http://www.w3.org/2001/XMLSchema}schema' and [v.text.strip() for v in ET.fromstring(instance).findall('amount')]==['10.00','5']
assert not e['newExecutionInHandoff'] and e['operations'][0]['values']==['10','5'] and e['operations'][1]['error']=='FOTY0012'
result=json.loads((P/'RESULTS.json').read_text());assert result['reviewRows']==43 and result['newOwnerAcceptances']==0 and result['consumerRuns']==result['runtimeTestsInThisStep']=='NotRun'
assert all(not result[k] for k in ['coreSourceChanged','ownerCheckoutsChanged','messagesSent','published','c45Accepted','apiFreezeAccepted','completeNamedManifestRefreshed'])
print('PASS: source1.36/qualified hashes,7 local owner snapshots, issue468 receipt,43 unique open review rows, unaccepted return template and public bounded fixture provenance; no consumer/owner acceptance inferred.')
