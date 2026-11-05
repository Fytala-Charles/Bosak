"""Build a self-contained Schema decision inbox from exact already-reviewed public inputs."""
from pathlib import Path
import hashlib,json,subprocess,shutil
R=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];I=P/'inputs';I.mkdir(exist_ok=True)
if (P/'SOURCE-PINS.json').exists():raise RuntimeError('Existing request retained')
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest();N=R/'.worktrees/Bosak.Schema.Assessment';revision='c3624998eca8c83018e121fa84af611f834a0501';pins={}
native=['src/Bosak.Schema/Assessment/AssessedDocument.cs','src/Bosak.Schema/Assessment/XsdDocumentAssessor.cs','src/Bosak.Schema/Assessment/SchemaDocumentAssessment.cs','src/Bosak.Schema/Assessment/AssessmentProfileParser.cs','docs/DOCUMENT-ASSESSMENT.md','docs/S009-02-PROVIDER-FACT-MAPPING.md','docs/S009-04-FOUNDATION-REQUIREMENTS.md','LICENSE.md']
for name in native:
 data=subprocess.check_output(['git','show',revision+':'+name],cwd=N);out=I/'native'/(name+'.txt');out.parent.mkdir(parents=True,exist_ok=True);out.write_bytes(data);pins[out.relative_to(P).as_posix()]={'origin':'public Schema assessment '+revision+':'+name,'sha256':sha(out)}
core=['docs/foundation-bcap-owner-review/PROPOSAL-MANIFEST.json','docs/foundation-bcap-owner-review/inputs/proposal/Provider.cs.txt','docs/foundation-bcap-owner-review/inputs/proposal/QueryFacade.cs.txt','docs/foundation-bcap-owner-review/inputs/proposal/ExactNumeric.cs.txt','docs/foundation-bcap-core-review/returns/BCAP-O2-Core.response.json','docs/foundation-bcap-core-review/returns/BCAP-O4-Core.response.json','docs/foundation-schema-reconciliation/RECONCILIATION.md','docs/foundation-schema-reconciliation/SPEC-BASIS.md','docs/foundation-schema-reconciliation/evidence/native-facts-0.json','docs/foundation-schema-reconciliation/evidence/native-facts-1.json']
core += ['docs/foundation-r2-ledger/DECISIONS.md','docs/foundation-r2-ledger/REQUIREMENTS.md']
for name in core:
 filename=Path(name).name+('.txt' if Path(name).suffix=='.md' else '');out=I/'core'/filename;out.parent.mkdir(exist_ok=True);shutil.copyfile(R/name,out);pins[out.relative_to(P).as_posix()]={'origin':name,'sha256':sha(out)}
for id in ['BCAP-O1-Schema','BCAP-O2-Schema']:
 out=I/'prior-returns'/(id+'.json');out.parent.mkdir(exist_ok=True);source=R/'docs/foundation-bcap-owner-review/returns'/(id+'.response.json');shutil.copyfile(source,out);pins[out.relative_to(P).as_posix()]={'origin':str(source),'sha256':sha(out)}
(P/'SOURCE-PINS.json').write_bytes((json.dumps({'date':'2026-11-02','proposalManifestSha256':sha(R/'docs/foundation-bcap-owner-review/PROPOSAL-MANIFEST.json'),'nativeRevision':revision,'files':pins,'readOnlyEvidence':True,'responseCreatedOnBehalfOfSchema':False},indent=2)+'\n').encode())
print('Prepared native source, declared policy and actual probe evidence in one review inbox; no Schema disposition created.')
