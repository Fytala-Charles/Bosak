"""Verify the authorized completed scopes and the remaining automatic-review restriction."""
from pathlib import Path
import hashlib,json,re,subprocess,xml.etree.ElementTree as ET,zipfile

P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
p=read(P/'PLAN.json');auth=read(P/'AUTHORIZATION.json');restriction=read(P/'INTEGRATION-RESTRICTION.json');s=read(E/'RESULTS.json')
prior=R/'docs/foundation-xmldb-source-candidate';old=read(prior/'PLAN.json');old_result=read(prior/'evidence/RESULTS.json')
assert sha(prior/'PLAN.json')==p['priorSourcePlanSha256'] and sha(prior/'MANIFEST.json')==p['priorSourceManifestSha256']
assert sha(P/'AUTHORIZATION.json')==p['authorizationSha256'] and auth['explicitUserAuthorizationReceived']
assert not auth['machineWideErrorSettingsAllowed'] and auth['unexpectedManagedFailuresMustBeCaught']
assert restriction['explicitUserAuthorizationReceived'] and not restriction['fullIntegrationLaunched']
assert not restriction['rejectedMethodRetriedOrBypassed'] and not restriction['machineWideErrorSettingsChanged']
owner=Path(p['ownerRoot']);source=Path(p['source']);work=Path(p['work'])
assert subprocess.check_output(['git','--no-optional-locks','-C',str(owner),'rev-parse','HEAD'],text=True).strip()==p['ownerHead']
assert subprocess.check_output(['git','--no-optional-locks','-C',str(owner),'status','--porcelain'],text=True).strip()==''
for name,h in old['sourceFiles'].items():assert sha(source/name)==old_result['dependencyLockChanges'].get(name,h),name
guard=read(E/'GUARDED-INTEGRATION.json');assert guard['diagnosticGuardOnly'] and guard['expectedKillBehaviorUnchanged'] and guard['originalOutputsRestored']
original=source/'tests/Bosak.XmlDb.IntegrationTests/CrashProbe.cs';derived=work/'GuardedCrashProbe.cs'
assert sha(original)==guard['originalSourceSha256'] and sha(derived)==guard['derivedSourceSha256']
text=derived.read_text(encoding='utf-8');assert 'try { return MainCore(args); }' in text and 'catch (Exception error) { Console.Error.WriteLine(error); return 1; }' in text
history='Isolated diagnostic entry catches managed failures; expected process kills unchanged.'
assert text.count(history)==1
without_history=''.join(line for line in text.splitlines(keepends=True) if history not in line)
wrapper='''    private static int Main(string[] args)
    {
        try { return MainCore(args); }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }

    private static int MainCore(string[] args)'''
assert without_history.replace(wrapper,'    private static int Main(string[] args)',1)==original.read_text(encoding='utf-8-sig')
assert sha(work/'GuardedIntegration.targets')==guard['overlaySha256']
integration=source/'tests/Bosak.XmlDb.IntegrationTests/bin/Release/net10.0'
for name,h in p['originalIntegrationFiles'].items():assert sha(integration/name)==sha(work/'integration-original'/name)==h
packages=read(R/'docs/foundation-preview-packages/evidence/PACKAGES.json')
assert sha(R/'docs/foundation-preview-packages/MANIFEST.json')==p['packageManifestSha256']
assert s['storagePassed']==3572 and s['xdmReusedPassed']==47 and s['integrationInProcessPassed']==69
assert s['distinctCompletedCases']==3688 and s['integrationCasesNotQualified']==794
assert not s['fullSourceQualificationComplete'] and s['automaticReviewRejectedFullIntegration']
assert not any(s[k] for k in ['fullIntegrationLaunched','sourceProductionBodiesChanged','ownerAccepted','apiFrozen','publicationAuthorized'])
assert sha(prior/'evidence/RESULTS.json')==s['xdmReceiptSha256']
with zipfile.ZipFile(E/'TRX-RESULTS.zip') as z:
    for row in s['rows']:
        trx=P/row['trx'];assert sha(trx)==row['trxSha256']==hashlib.sha256(z.read(row['trx'])).hexdigest()
        tree=ET.parse(trx);counts=tree.find('.//{*}Counters').attrib
        assert int(counts['passed'])==row['passed'] and counts['failed']==counts['notExecuted']=='0'
        names=sorted(x.attrib['testName'] for x in tree.findall('.//{*}UnitTestResult'));assert names==row['caseNames']
        hist=owner/row['historicalTrx']['path'];assert sha(hist)==row['historicalTrx']['sha256']
        baseline=sorted(x.attrib['testName'] for x in ET.parse(hist).findall('.//{*}UnitTestResult'))
        if row['lane']=='storage':assert names==baseline
        else:assert set(names)<=set(baseline)
        layout=row['execution']['layout'];folder=Path(layout['path'])
        for name,h in layout['files'].items():assert sha(folder/name)==h,(row['lane'],name)
        seen={}
        for file in (E/'bindings'/row['lane']).glob('*.json'):
            report=read(file);assert not report['captureErrors']
            for load in report['assemblies']:
                if load['name'] in packages['loadedEngines']:
                    assert load['sha256']==packages['loadedEngines'][load['name']] and load['context']=='Default' and load['collectible']=='False'
                    seen[load['name']]=load['sha256']
        assert seen==row['loadedPackageEngines']
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
    for name in z.namelist():
        if name.endswith('.log.json'):
            row=json.loads(z.read(name));assert row['exitCode']==0 and row['failure'] is None
            assert hashlib.sha256(z.read(name[:-5])).hexdigest()==row['logSha256']
    build=z.read('xmldb-full-guard-build.log').decode('utf-8-sig');assert '0 Warning(s)' in build and '0 Error(s)' in build
api=read(E/'API-COMPARISON.json');assert api['publicProtectedRowsUnchanged'] and api['friendAssembliesUnchanged']
assert api['publicTypes']==16 and api['declarationRows']==462 and len(api['privateDebugAttributeDeltas'])==13
before=read(E/'api-baseline.json');after=read(E/'api-candidate.json')
marker='[System.Diagnostics.DebuggerBrowsableAttribute((System.Diagnostics.DebuggerBrowsableState)0)];'
for a,b in zip(before['assemblies'],after['assemblies'],strict=True):
    assert a['assembly']==b['assembly'] and a['friendAssemblies']==b['friendAssemblies']
    assert sorted(row.replace(marker,'') if row.startswith('L ') else row for row in a['declarations'])==sorted(b['declarations'])
for lane,files in api['layouts'].items():
    root=work/('api-'+lane)
    for name,h in files.items():assert sha(root/name)==h
for file in [P/'README.md',R/'docs/CHECKPOINT-1.81.1.md']:
    text=re.sub(r'`[^`]*`','',file.read_text(encoding='utf-8'))
    for link in re.findall(r'\[[^\[\]\n]*\]\(([^)\n]+)\)',text):
        name=link.strip('<>').split('#')[0]
        if name and not re.match(r'[A-Za-z][A-Za-z0-9+.-]*:',name):assert (file.parent/name).exists()
if (P/'MANIFEST.json').exists():
    files={f.relative_to(P).as_posix():sha(f) for f in P.rglob('*') if f.is_file() and f.name!='MANIFEST.json' and '__pycache__' not in f.parts}
    assert read(P/'MANIFEST.json')==files
print('Verified full Storage3572 + reused XDM47 + approved in-process Integration69; 794 Integration cases remain unqualified because automatic review rejected the process probes despite explicit user approval.')
