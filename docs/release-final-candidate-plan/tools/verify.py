"""Verify the proposed final candidate decisions without accepting or executing them."""
from pathlib import Path
import hashlib,json,re
P=Path(__file__).resolve().parents[1];R=P.parents[1];read=lambda f:json.loads(f.read_text(encoding='utf-8'));sha=lambda f:hashlib.sha256(f.read_bytes()).hexdigest()
s=read(P/'DECISIONS.json');registry=read(P/'REGISTRY-PREFLIGHT.json')
for name,h in s['inputs'].items():assert sha(R/name)==h
assert len(registry['nuget'])==len(s['selectedPreviewPackages'])==9 and not any(x['alreadyListed'] for x in registry['nuget'])
assert not registry['marketplace']['alreadyListed']
assert s['versionsApproved'] and s['databasePackage']['dispositionApproved'] and not s['apiFrozen'] and not s['publicationAuthorized']
assert len(s['scenarios'])==9 and all(not x['ownerWorkloadAccepted'] and not x['baselineAccepted'] and x['ownerBudget'] is None for x in s['scenarios'])
assert all(v['decision'] is None and v['ownerIdentity'] is None and not v['workloadApproved'] and not v['baselineApproved'] and not v['budgetApproved'] for v in s['ownerAcceptance'].values())
f=P/'README.md';text=re.sub(r'`[^`]*`','',f.read_text(encoding='utf-8'))
for target in re.findall(r'\[[^\[\]\n]*\]\(([^)\n]+)\)',text):
 target=target.strip('<>').split('#')[0]
 if target and not re.match(r'[A-Za-z][A-Za-z0-9+.-]*:',target):assert (f.parent/target).exists(),target
assert read(P/'MANIFEST.json')=={f.relative_to(P).as_posix():sha(f) for f in P.rglob('*') if f.is_file() and f!=P/'MANIFEST.json' and '__pycache__' not in f.parts}
print('Verified nine registry observations, selected version/Database scope decisions, pinned inputs and unchanged owner/API acceptance. No build or publication.')
