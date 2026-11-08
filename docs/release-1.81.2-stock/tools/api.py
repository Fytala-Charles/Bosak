"""Compare the rebuilt eleven-product API, allowing only intended version identities."""
from pathlib import Path
import json,subprocess,hashlib
P=Path(__file__).resolve().parents[1];R=P.parents[1];p=json.loads((P/'PLAN.json').read_text());tool=R/'.guard-tmp/preview-qualification-1811/api-audit/bin/Release/net10.0/Audit.dll';baseline=json.loads((R/'docs/foundation-captured-identity-production/evidence/SOURCE-AND-BINARY-PINS.json').read_text());cmd=['dotnet',str(tool),p['source'],str(P/'evidence/public-api.json'),baseline['engines']['Bosak.Xslt']['path']];out=subprocess.run(cmd,capture_output=True,text=True,timeout=180);(P/'evidence/api-inventory.txt').write_text(out.stdout+out.stderr);assert out.returncode==0,out.stderr
before=json.loads((R/'docs/foundation-preview-qualification/evidence/public-api.json').read_text());after=json.loads((P/'evidence/public-api.json').read_text())
for old,new in zip(before['assemblies'],after['assemblies'],strict=True):
 for key in ['project','assembly','identity','references','friendAssemblies','publicTypes','declarationCount','declarations']:
  expected=json.loads(json.dumps(old[key]).replace('Version=1.81.1.0','Version=1.81.2.0'))
  assert expected==new[key],(old['assembly'],key)
assert len(after['assemblies'])==11 and sum(x['publicTypes'] for x in after['assemblies'])==318 and sum(x['declarationCount'] for x in after['assemblies'])==6465
(P/'evidence/API-COMPARISON.json').write_text(json.dumps({'products':11,'packaged':10,'nonpackaged':['Bosak.LanguageServer'],'publicTypes':318,'declarationRows':6465,'allowedDelta':'Bosak assembly identity/reference version1.81.1.0 to1.81.2.0 only','apiFrozen':False,'toolSha256':hashlib.sha256(tool.read_bytes()).hexdigest()},indent=2)+'\n');print('API318 types/6465 declarations match, allowing only intended assembly-version identities.')
