"""Retain exact-case XDM compatibility results and the next source qualification recipe."""
from pathlib import Path
import hashlib,json,xml.etree.ElementTree as ET,zipfile

P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
plan=read(P/'PLAN.json');owner=Path(plan['ownerRoot'])
old=read(owner/'docs/foundation-requirements/native-lifetime/results.json')
baseline_receipt=next(row for row in old['selectedWindows'] if row['counts']['passed']=='47')
historic=owner/baseline_receipt['path'];assert sha(historic)==baseline_receipt['sha256']
case_names=lambda path:sorted(x.attrib['testName'] for x in ET.parse(path).findall('.//{*}UnitTestResult'))
historical_cases=case_names(historic);results={}
for lane in ['baseline','preview']:
    files=list((E/'trx'/lane).glob('*.trx'));assert len(files)==1
    file=files[0];tree=ET.parse(file);counts=tree.find('.//{*}Counters').attrib
    assert counts['passed']=='47' and counts['failed']==counts['notExecuted']=='0'
    names=case_names(file);assert names==historical_cases
    results[lane]=dict(passed=47,failed=0,caseNames=names,trx=file.relative_to(P).as_posix(),sha256=sha(file))
(E/'RESULTS.json').write_text(json.dumps(dict(lanes=results,historicalTrx=dict(path=str(historic),sha256=sha(historic)),
    sameTestAssembly=True,countsOverlapNotAdditive=True,ownerAccepted=False,sourceLaneRun=False),indent=2)+'\n')
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip','x',zipfile.ZIP_DEFLATED) as z:
    for lane in ['baseline','preview']:
        log=R/f'docs/foundation-r2-closure/evidence/execution/xmldb-package-{lane}.log';receipt=log.with_suffix('.log.json');row=read(receipt)
        assert row['exitCode']==0 and row['failure'] is None and row['logSha256']==sha(log)
        z.write(log,log.name);z.write(receipt,receipt.name)
with zipfile.ZipFile(E/'TRX-RESULTS.zip','x',zipfile.ZIP_DEFLATED) as z:
    for file in (E/'trx').rglob('*.trx'):z.write(file,file.relative_to(P).as_posix())
recipe=dict(ownerHead=plan['ownerHead'],previewVersion=plan['previewVersion'],status='PreparedNotExecuted',
    productionAdapterImplemented=False,requirementsWaived=0,
    steps=[
        dict(id='S1',work='Archive exact clean owner revision into a new isolated workspace; include src, tests/TestData, test projects, sample project and build configuration. Hash-bind original-format fixtures; preserve live checkout and original histories.'),
        dict(id='S2',work='Record source-build/version/tool inputs and external dependency provenance. In the isolated recipe only, update selected Bosak PackageReference versions to the exact preview cohort; preserve original owner pins/projects as evidence. Resolve MinVer identity explicitly before build; do not accidentally inherit parent Core repository versioning.'),
        dict(id='S3',work='Build and test all source projects under owner analysis/doc requirements. Compare exact 3572 Storage,863 Integration,47 XDM case names with retained source catalogs; audit process helper safety before launching full integration/crash lanes. Record source/default-overload migration errors separately.'),
        dict(id='S4',work='Prepare a separately adopted ordinary input/lifetime adapter over actual snapshot ownership. Use provider-private exact membership before foreign getters; do not expose a generic callback factory or fabricate Schema authority. Existing private helpers require a reviewed production placement or a clearly labeled friend-test-only diagnostic.'),
        dict(id='S5',work='Qualify same/distinct/duplicate/foreign inputs, cancellation/close/reentry, deferred consumption through XSLT serialization and original work/cleanup failure graphs. Keep raw work alive until actual completion; caller timeout/kill is not confirmed native cleanup.'),
        dict(id='S6',work='Choose actual worker/currentness routing with owners, then run representative PF-04/05/shared scenarios with explicit baselines/budgets. Source/binary compatibility is not R4/C45/freeze or product adoption.')])
(P/'QUALIFICATION-RECIPE.json').write_text(json.dumps(recipe,indent=2)+'\n',encoding='utf-8')
print('Retained baseline47/preview47 exact-case compatibility and a six-step source/integration qualification recipe; source/adapter acceptance not claimed.')
