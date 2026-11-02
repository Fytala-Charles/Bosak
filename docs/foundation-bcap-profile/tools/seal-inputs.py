"""Retain isolated source/binary archives and precise cohort status."""
from pathlib import Path
import hashlib,json,zipfile,subprocess
R=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';W=R/'.guard-tmp/bcap-profile'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
if any((W/n).exists() for n in ['qualified-source.zip','qualified-binaries.zip']):
 raise RuntimeError('Qualified archives already exist; preserve them and use a new candidate for a new cohort')
production={n:sha(R/n) for n in subprocess.check_output(['git','ls-files','src'],text=True).splitlines() if (R/n).is_file()}
assert subprocess.run(['git','diff','--quiet','HEAD','--','src']).returncode==0
source={p.relative_to(W).as_posix():sha(p) for p in (W/'src').rglob('*') if p.is_file() and not {'bin','obj'}&set(p.relative_to(W).parts)}
with zipfile.ZipFile(W/'qualified-source.zip','w',zipfile.ZIP_DEFLATED) as z:
 for n in source:z.write(W/n,n)
 for n in ['license.md','NOTICE']:
  if (R/n).exists():z.write(R/n,n)
with zipfile.ZipFile(W/'qualified-binaries.zip','w',zipfile.ZIP_DEFLATED) as z:
 for p in (W/'Host/bin/Release/net10.0').iterdir():
  if p.is_file():z.write(p,'probe/'+p.name)
 for p in (W/'src').glob('*/bin/Release/net10.0/Bosak.*.dll'):
  if p.parent.parent.parent.parent.name==p.stem:z.write(p,'engines/'+p.name)
 for n in ['license.md','NOTICE']:
  if (R/n).exists():z.write(R/n,n)
archives={p.name:{'path':str(p),'sha256':sha(p)} for p in [W/'qualified-source.zip',W/'qualified-binaries.zip']}
(E/'INPUTS.json').write_text(json.dumps({'sourceRevision':subprocess.check_output(['git','rev-parse','HEAD'],text=True).strip(),'productionChanged':False,'productionSources':production,'candidateSources':source,'archives':archives,'probeSourceSha256':sha(P/'tools/Probe.cs.txt'),'candidateProducerSourceSha256':sha(P/'tools/SchemaCaptureCandidate.cs.txt'),'setupReflectionOnly':True,'newFriendGrants':0,'nativeConsumersTested':False,'fullSolutionTestsRun':True,'frozenGatesRun':True,'reviewDeclarationManifestSha256':sha(R/'docs/foundation-bcap-outcomes/MANIFEST.json')},indent=2)+'\n')
rows=[]
for id in ['C01','C02','C03','C04','C05','C06','C07','C08','C09','C10']:
 rows.append({'id':id,'status':'PartiallyExercised' if id=='C10' else 'ExercisedBoundedSequentialProfile','ownerAcceptance':False})
(E/'STATUS.json').write_text(json.dumps({'date':'2026-11-02','p03':'InProgress','cohortChecks':83,'includesPrevious70':True,'repetitions':2,'unitTestsPassed':24390,'qt3':[31397,0,424],'qt4Gated':[8941,0,1918],'shippingPublicTypes':270,'shippingDeclarationRows':5770,'reviewProposedTypes':47,'bcap01Closed':False,'publicCandidateImplemented':False,'scope':'Unchanged isolated outcome source; invocation/profile and full candidate build/unit/conformance qualification, not production promotion','cases':rows,'remaining':['Exact revised contract disposition by Braid and impacted owners','Cold/workload allocation and cancellation latency, native performance budgets','Actual Schema/XmlDb consumer and worker handoff qualification','Concurrent/reentrant gates and arbitrary resource authority remain unsupported','Public 47-type declarations are throwing proposals, not a shipping SDK']},indent=2)+'\n')
print('Retained source/binary archives, all source pins and honest partial cohort status.')
