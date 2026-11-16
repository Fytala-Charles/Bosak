from pathlib import Path
import json,hashlib,zipfile,subprocess
D=Path(__file__).resolve().parent;W=D.parents[1];R=W.parents[1];B=R/'.guard-tmp/release-1.81.17-20261116';P=B/'publication'
read=lambda p:json.loads(p.read_bytes())
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
for name,h in read(D/'MANIFEST.json').items():assert sha(D/name)==h,name
assert sha(D/'RELEASE-MANIFEST.json')=='2ad34e65ade407a914dac93b551caf703a9cfc298ecb82921bd750de3dade2ca'
host=read(D/'HOSTED-COMPLETED.json');assert host['run']['conclusion']=='success' and host['run']['head_sha']=='6a7618e9dcf70eb8a67d5d1789ff7b787133b52d'
push=read(D/'PUSH-SUMMARY.json');assert push['pushedMessages']==10 and push['duplicateMessages']==0
s=read(D/'SERVICE-VERIFICATION.json');assert s['allTenVerified'] and len(s['rows'])==10
for row in s['rows']:
 assert row['verified'] and row['registrationListed'] and row['versionListed']
 name=row['package']+'.1.81.17.nupkg';original=B/'packages'/name;delivered=P/'delivered'/name
 assert sha(original)==row['approvedUnsignedSha256'] and sha(delivered)==row['sha256']
 with zipfile.ZipFile(original) as a,zipfile.ZipFile(delivered) as b:
  assert len(a.namelist())==len(set(a.namelist())) and len(b.namelist())==len(set(b.namelist()))
  assert set(b.namelist())-set(a.namelist())=={'.signature.p7s'} and not set(a.namelist())-set(b.namelist())
  for n in a.namelist():assert a.read(n)==b.read(n)
assert read(D/'SIGNATURE-VERIFICATION.json')['allPackageSignaturesVerified']
c=read(D/'CONSUMER-RESULTS.json');assert c['success'] and c['result']['passed']==53 and c['result']['failed']==0 and len(c['result']['assemblies'])==10 and c['loadedDllsMatchReviewedPackages'] and c['exactPackageCacheVerified']
changed=subprocess.check_output(['git','diff','6a7618e9','--name-only','--','src','tests','tools','.github','global.json','Directory.Build.props'],cwd=W,text=True);assert not changed
print('Offline publication evidence verified: ten packages, signatures, 53 consumer checks; production source unchanged.')
