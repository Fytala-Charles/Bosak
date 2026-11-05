"""Read-only capture of current public owner documents and issue evidence."""
from pathlib import Path
import subprocess,json,hashlib,datetime,sys
ROOT=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];I=P/'inputs';I.mkdir(parents=True,exist_ok=True)
sha=lambda b:hashlib.sha256(b).hexdigest()
sources=[('schema-needs.md',ROOT/'.worktrees/Bosak.Schema.Assessment','docs/S009-04-FOUNDATION-REQUIREMENTS.md'),('schema-facts.md',ROOT/'.worktrees/Bosak.Schema.Assessment','docs/S009-02-PROVIDER-FACT-MAPPING.md'),('schema-fixtures.json',ROOT/'.worktrees/Bosak.Schema.Assessment','docs/S009-02-FIXTURE-IDENTITIES.json'),('xmldb-needs.md',Path('D:/Development/Bosal.XmlDb'),'docs/foundation-requirements/README.md'),('xmldb-matrix.json',Path('D:/Development/Bosal.XmlDb'),'docs/foundation-requirements/fx009-01-matrix.json'),('braid-work.md',ROOT/'.worktrees/Bosak.Braid.ReadOnly','docs/SHARED-FOUNDATION-WORK-PACKAGES.md'),('xmldb-prior-return.json',ROOT,'docs/foundation-owner-status/xmldb-fx009-02-characterization-2026-10-30/OWNER-RETURN.json')]
records=[]
for name,repo,relative in sources:
 data=(repo/relative).read_bytes();(I/name).write_bytes(data)
 committed=subprocess.run(['git','show','HEAD:'+relative],cwd=repo,capture_output=True)
 records.append({'snapshot':'inputs/'+name,'source':str(repo/relative),'repositoryRevision':subprocess.check_output(['git','rev-parse','HEAD'],cwd=repo,text=True).strip(),'branch':subprocess.check_output(['git','branch','--show-current'],cwd=repo,text=True).strip(),'sha256':sha(data),'committedDocumentAvailable':committed.returncode==0,'documentMatchesCommitIgnoringLineEndings':committed.returncode==0 and committed.stdout.replace(b'\r\n',b'\n')==data.replace(b'\r\n',b'\n')})
online=[]
if '--online' in sys.argv:
 issue=json.loads(subprocess.check_output(['gh','api','repos/Fytala-Charles/Bosak/issues/468'],cwd=ROOT))
 issue={k:issue[k] for k in ['number','title','state','updated_at','body','html_url']}
 pages=json.loads(subprocess.check_output(['gh','api','repos/Fytala-Charles/Bosak/issues/468/comments','--paginate','--slurp'],cwd=ROOT))
 comments=[{**{k:c[k] for k in ['id','created_at','updated_at','html_url','body']},'author':c['user']['login']} for page in pages for c in page]
 for name,value in [('issue-468.json',issue),('issue-468-comments.json',comments)]:
  data=(json.dumps(value,indent=2)+'\n').encode('utf-8');(I/name).write_bytes(data);online.append({'snapshot':'inputs/'+name,'sha256':sha(data)})
record={'capturedUtc':datetime.datetime.now(datetime.timezone.utc).isoformat(),'localDocuments':records,'onlineIssue':online,'externalCheckoutsModified':False,'messagesSent':False}
(P/'INPUTS.json').write_text(json.dumps(record,indent=2)+'\n',encoding='utf-8')
print('Captured',len(records),'local owner documents and',len(online),'issue receipts; no checkout edits or messages.')
