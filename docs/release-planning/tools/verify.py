"""Verify the finite release plan and immutable read-only review inputs."""
from pathlib import Path
import hashlib,json,subprocess
P=Path(__file__).resolve().parents[1];R=P.parents[1];sha=lambda b:hashlib.sha256(b).hexdigest()
i=json.loads((P/'INPUTS.json').read_text());g=json.loads((P/'GATES.json').read_text())
assert not i['ownerAcceptance'] and i['productionVersion']=='1.42.0' and g['sourceVersion']=='1.51.0'
assert g['targetReleaseVersion'] is None and g['releaseDate'] is None and not g['apiFrozen'] and not g['publicationAuthorized']
assert [x['id'] for x in g['gates']]==['R'+str(n) for n in range(8)]
assert [x['status'] for x in g['gates']]==['Qualified','Audited','ClosedWithScope','InProgress','Pending','Pending','Pending','Pending']
closure=json.loads((R/'docs/foundation-r2-closure/CLOSURE.json').read_text());assert closure['r2Closed'] and closure['status']=='ClosedWithScope' and not closure['runtimeQualification'] and not closure['apiFrozen']
assert g['gates'][2]['recordedAcceptedReturns']==7 and not g['gates'][2]['openRevisionRequests']
subprocess.run(['python',str(R/'docs/foundation-r2-closure/tools/verify.py')],check=True,capture_output=True)
seen=set()
for row in g['gates']:
 assert set(row['dependsOn'])<=seen and row['owner'] and row['exitEvidence'];seen.add(row['id'])
for repo in i['repositories']:
 assert repo['observedStatus']=='clean'
 for name,digest in repo['files'].items():assert sha(subprocess.check_output(['git','show',repo['revision']+':'+name],cwd=repo['root']))==digest,(repo['name'],name)
current=json.loads((R/'docs/foundation-exact-division/evidence/SOURCE-AND-BINARY-PINS.json').read_text())
for name,digest in i['coreFiles'].items():assert sha((R/name).read_bytes())==current['sources'].get(name,digest),name
old=subprocess.check_output(['git','show',i['originalRoadmapRevision']+':ROADMAP.md'],cwd=R)
assert sha(old)==i['originalRoadmapSha256']==sha((P/'inputs/ROADMAP-before-consolidation.txt').read_bytes())
source=json.loads((R/'docs/foundation-xslt-source-membership/evidence/SOURCE.json').read_text())
current=json.loads((R/'docs/foundation-exact-division/evidence/SOURCE-AND-BINARY-PINS.json').read_text())
for name,digest in source['files'].items():assert sha((R/name).read_bytes())==current['sources'].get(name,digest),name
subprocess.run(['python',str(R/'docs/foundation-exact-division/tools/verify.py')],check=True,capture_output=True)
roadmap=(R/'ROADMAP.md').read_text(encoding='utf-8');assert len(roadmap.splitlines())<90 and 'Immediate foundation work' not in roadmap and 'release-planning/README.md' in roadmap
if (P/'MANIFEST.json').exists():
 m=json.loads((P/'MANIFEST.json').read_text());assert set(m)=={p.relative_to(P).as_posix() for p in P.rglob('*') if p.is_file() and p.name!='MANIFEST.json'}
 for name,digest in m.items():assert sha((P/name).read_bytes())==digest,name
print('Verified R0-R7 gate dependencies/statuses, observed owner pins, archived roadmap and qualified exact-division source overlay. No native/freeze acceptance claimed.')
