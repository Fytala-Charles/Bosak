"""Archive final isolated sources and compare them with preserved product/prior evidence."""
from pathlib import Path
import json,hashlib,subprocess,shutil,platform,datetime
E=Path(__file__).resolve().parent;R=E.parents[3];D=R/'.guard-tmp/k04-schema-compatibility-prototype'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
old=json.loads((E/'previous-identities.json').read_text())
assert all((R/f).is_file() and sha(R/f)==h for f,h in old.items())
for name in ['Contracts.cs','Model.cs','Program.cs','Contracts.csproj','Prototype.csproj']:shutil.copyfile(D/name,E/(name+'.txt'))
for name in ['c44r2-none-qualified.log','c44r2-none-refined.log','c44r2-prototype-build-qualified.log','c44r2-prototype-build-refined.log']:
 shutil.copyfile(R/'.guard-tmp'/name,E/name)
baseline=json.loads((E/'prototype-none-results.json').read_text());previous=json.loads((R/'docs/conformance/k04-schema-session-prototype/evidence/api-surface.json').read_text())
def normalize(s):return s.replace('C44R2.Proposed.','Proposed.').replace('C44.Proposed.','Proposed.')
a={normalize(t['type']):set(map(normalize,t['members'])) for t in previous['exportedTypes']}
b={normalize(t['type']):set(map(normalize,t['members'])) for t in baseline['exportedTypes']}
delta={'comparison':'Compiled neutral proposals only; namespace-normalized, never a product ABI change','previousTypes':len(a),'revisedTypes':len(b),'addedTypes':sorted(b.keys()-a.keys()),'removedTypes':sorted(a.keys()-b.keys()),'changedTypes':[{'type':n,'addedMembers':sorted(b[n]-a[n]),'removedMembers':sorted(a[n]-b[n])} for n in sorted(a.keys()&b.keys()) if a[n]!=b[n]]}
(E/'api-surface.json').write_text(json.dumps({'kind':'Compiled revised neutral contract; provider and carrier coverage remain bounded','references':baseline['references'],'exportedTypes':baseline['exportedTypes']},indent=2)+'\n',encoding='utf-8')
(E/'api-diff.json').write_text(json.dumps(delta,indent=2)+'\n',encoding='utf-8')
assert [n for n in baseline['references'] if n.startswith('Bosak.')]==['Bosak.XPath.Core']
rawreview=json.loads((R/'docs/compatibility/xdm40-2026-10-29/evidence/observations.json').read_text())
product=[]
for binary in rawreview['Assemblies']:
 p=Path(binary['Path']);assert sha(p)==binary['Sha256'].lower();product.append({'path':str(p),'sha256':sha(p)})
assert subprocess.check_output(['git','diff','--name-only','--','src','tests'],cwd=R,text=True).strip()==''
for binary in baseline['binaries']:assert sha(Path(binary['path']))==binary['sha256'].lower()
receipts=json.loads((E/'prototype-receipts.json').read_text())
assert receipts[0]['passed']==123 and receipts[0]['failed']==0 and receipts[0]['exitCode']==0
assert len(receipts)==13 and all(r['exitCode']==2 and r['failed']>0 and not r['timeout'] for r in receipts[1:])
for r in receipts:
 assert sha(E/r['log'])==r['logSha256']
 result=json.loads((E/r['log']).read_text(encoding='utf-8-sig'))
 assert result['passed']+result['failed']==123 and len(result['tests'])==123
build=json.loads((E/'build-receipt.json').read_text());assert build['exitCode']==0 and not build['timeout']
assert baseline['bclWitness']['sourceErrors']==[] and baseline['bclWitness']['targetErrors'] and baseline['bclWitness']['originalValidity']=='Valid' and baseline['bclWitness']['targetCloneValidity']=='Invalid' and baseline['bclWitness']['unionCompiled']
for filename,text in [('common.xsd',baseline['bclWitness']['commonXsd']),('extra.xsd',baseline['bclWitness']['extraXsd']),('instance.xml',baseline['bclWitness']['instanceXml'])]:
 (E/filename).write_text(text+'\n',encoding='utf-8')
environment={'capturedUtc':datetime.datetime.now(datetime.timezone.utc).isoformat(),'platform':platform.platform(),'python':platform.python_version(),'dotnetInfo':subprocess.check_output(['dotnet','--info'],text=True),'gitHead':subprocess.check_output(['git','rev-parse','HEAD'],cwd=R,text=True).strip()}
(E/'environment.json').write_text(json.dumps(environment,indent=2)+'\n',encoding='utf-8')
(E/'source-and-binary-identities.json').write_text(json.dumps({'finalSources':{f:sha(D/f) for f in ['Contracts.cs','Model.cs','Program.cs','Contracts.csproj','Prototype.csproj']},'executedBinaries':baseline['binaries'],'unchangedProductAssemblies':product,'preservedPriorFiles':len(old),'productionSourceDiff':'empty','candidate':'1.33.1 unpublished','public':'1.26.3'},indent=2)+'\n',encoding='utf-8')
summary={'kind':'Isolated qualification, not production or real-consumer acceptance','controlsPassed':123,'controlsFailed':0,'mutationModes':len(receipts)-1,'mutationFailures':{r['mode']:r['failed'] for r in receipts[1:]},'finalWatchdogProcesses':13,'timeoutCount':0,'controlEvaluations':13*123,'publicProposalTypes':32,'newProposalTypes':len(delta['addedTypes']),'changedProposalTypes':len(delta['changedTypes']),'priorFilesPreserved':len(old),'unchangedProductBinaries':len(product),'separateBclWitness':True,'inheritedSolutionPasses':24203,'inheritedQT3':'31397/0/424','inheritedQT4':'8941/0/1918','consumerBuildOrAbiCertification':False,'productionApproval':'Pending'}
(E/'RESULTS.json').write_text(json.dumps(summary,indent=2)+'\n',encoding='utf-8')
print(json.dumps(summary,indent=2))
print('Changed neutral types:',[t['type'] for t in delta['changedTypes']])
