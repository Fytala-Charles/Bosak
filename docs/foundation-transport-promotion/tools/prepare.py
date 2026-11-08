"""Prepare a source-only promotion patch; never modify the live product source."""
from pathlib import Path
import subprocess,json,hashlib,difflib,re
ROOT=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1]
sha=lambda b:hashlib.sha256(b).hexdigest()
revision=subprocess.check_output(['git','rev-parse','f2831379'],cwd=ROOT,text=True).strip()
old=lambda n:subprocess.check_output(['git','show',revision+':'+n],cwd=ROOT)
files={};qualified={}
items=[('src/Bosak.XPath.Core/Xdm/XdmValue.cs','2.11','Private scalar transport and evidence-neutral CLR equality (1.36.0)'),('src/Bosak.XPath.Core/Xdm/SchemaEvidenceKernel.cs','0.2','Bind private scalar transport and retain stack-owned cached-read leases (1.36.0)'),('src/Bosak.XPath.Runtime/Vm/SchemaQuerySession.cs','0.2','Compose issued-value reads and explicit conversion transport (1.36.0)')]
def significant(s):return '\n'.join(line for line in s.splitlines() if line.strip() and not line.lstrip().startswith('//'))
for name,version,note in items:
 candidate=ROOT/'docs/foundation-transport-atomic-cost/candidate'/(Path(name).name+'.txt');text=candidate.read_text(encoding='utf-8')
 while text.startswith('// Generated'):text=text.split('\n',1)[1]
 before=text
 using=text.find('\nusing ');namespace=text.find('\nnamespace ');start=min(n for n in [using,namespace] if n>=0)
 header=text[:start];body=text[start:];at=header.rfind('//                      |==================|=======|================|')
 assert at>=0
 row=f'//                      | Charles Korthout | {version:<5} | 31-10-2026     | {note:<88} |\n'
 text=header[:at]+row+header[at:]+body
 if name.endswith('/XdmValue.cs'):
  text=re.sub(r'^// PURPOSE .*$', '// PURPOSE              : Represents any value in the XQuery Data Model.',text,count=1,flags=re.M)
  text=re.sub(r'^// SPECIAL NOTES .*$', '// SPECIAL NOTES        : Foundation types for the XQuery Data Model; used by all higher layers.',text,count=1,flags=re.M)
 text=text.replace('Diagnostic marker only: no schema authority or new normative XPath behavior.','Private transport storage; authority is validated by the evidence kernel.')
 text=text.replace('Diagnostic atomic marker attachment, not an authority factory.','Internal atomic marker attachment; admission belongs to the evidence kernel.')
 text=text.replace('The diagnostic marker.','The private marker.').replace('The marked diagnostic value.','The value carrying private transport state.').replace('Reads only a diagnostic marker.','Reads raw internal transport state without admitting it.').replace('The diagnostic value.','The private transported value.')
 text=text.replace('Generated isolated candidate; no host frame or public transport factory.','Private scalar transport; no public attachment factory.')
 text=text.replace('Immutable admitted boundary evidence; no attachment to XdmValue.','Immutable admitted evidence bound to private scalar transport.')
 text=text.replace('    /// <inheritdoc/>\n    public override bool Equals(object? other)', '    /// <summary>Compares ordinary value fields independently of private evidence.</summary>\n    /// <param name="other">The value to compare.</param>\n    /// <returns>Whether the ordinary value fields compare equal.</returns>\n    public override bool Equals(object? other)')
 text=text.replace('    /// <inheritdoc/>\n    public override int GetHashCode()', '    /// <summary>Hashes ordinary value fields independently of private evidence.</summary>\n    /// <returns>The ordinary value hash code.</returns>\n    public override int GetHashCode()')
 assert significant(text)==significant(before),name
 files[name]=text.encode('utf-8');qualified[name]={'qualifiedSourceSha256':sha(candidate.read_bytes()),'significantLinesSha256':sha(significant(text).encode('utf-8'))}
props=old('src/Directory.Build.props').decode('utf-8-sig');assert props.count('<Version>1.35.0</Version>')==1
files['src/Directory.Build.props']=props.replace('<Version>1.35.0</Version>','<Version>1.36.0</Version>').encode('utf-8')
template=old('tests/Bosak.XPath.Core.Tests/SchemaEvidenceKernelTests.cs').decode('utf-8-sig').split('using ',1)[0]
for layer,description in [('Core','Verifies representation binding and synchronous ownership in private scalar transport.'),('Runtime','Checks direct issued-value transport through the private compiled session.')]:
 header=re.sub(r'^// PURPOSE .*$', '// PURPOSE              : '+description,template,count=1,flags=re.M).replace('Creation: private foundation admission (1.35.0)','Creation: durable private transport regressions (1.36.0)')
 body=(P/'tools'/(layer+'Regressions.body.txt')).read_text(encoding='utf-8')
 files[f'tests/Bosak.XPath.{layer}.Tests/SchemaValueTransportTests.cs']=(header+body).encode('utf-8')
tracked=set(subprocess.check_output(['git','ls-tree','-r','--name-only',revision],cwd=ROOT,text=True).splitlines());entries={};patch=[]
for name,data in files.items():
 previous=old(name) if name in tracked else b''
 path=P/'files'/(name+'.txt');path.parent.mkdir(parents=True,exist_ok=True);path.write_bytes(data)
 entries[name]={'file':path.relative_to(P).as_posix(),'beforeSha256':sha(previous) if name in tracked else None,'afterSha256':sha(data)}
 patch.extend(difflib.unified_diff(previous.decode('utf-8-sig').splitlines(True),data.decode('utf-8').splitlines(True),fromfile='a/'+name if name in tracked else '/dev/null',tofile='b/'+name))
(P/'PRODUCTION.patch').write_bytes(''.join(patch).encode('utf-8'))
(P/'PATCH.json').write_text(json.dumps({'baseRevision':revision,'proposedVersion':'1.36.0','liveVersion':'1.35.0','livePatchApplied':False,'qualifiedBodies':qualified,'files':entries,'patchSha256':sha((P/'PRODUCTION.patch').read_bytes())},indent=2)+'\n',encoding='utf-8')
print('Prepared six-file production patch; three implementation bodies unchanged; live source untouched.')
