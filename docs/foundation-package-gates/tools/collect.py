"""Collect selected corrected executions and retain original attempts separately."""
import gzip
import argparse
import hashlib
import json
from pathlib import Path
import re
import shutil
import xml.etree.ElementTree as ET

HERE=Path(__file__).resolve().parents[1];ROOT=HERE.parents[1]
WORK=ROOT/'.guard-tmp/foundation-package-gates-2026-10-30'
NS={'t':'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}


def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()
def save(path,value):path.parent.mkdir(parents=True,exist_ok=True);path.write_bytes((json.dumps(value,indent=2)+'\n').encode())


def main():
    global WORK, HERE
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('--workspace',default=str(WORK));parser.add_argument('--output-root',default=str(HERE));args=parser.parse_args()
    WORK=Path(args.workspace).resolve();assert WORK.is_relative_to((ROOT/'.guard-tmp').resolve())
    HERE=Path(args.output_root).resolve();assert HERE.is_relative_to(ROOT)
    assert not (HERE/'MANIFEST.json').exists(),'Existing dossier is sealed; use a separate output root'
    out=HERE/'evidence';out.mkdir(parents=True,exist_ok=True);m=json.loads((WORK/'runners.json').read_bytes());packages=json.loads((ROOT/'docs/foundation-package-qualification/evidence/packages.json').read_bytes());expected={p['id']:p['assemblySha256'] for p in packages['packages']}
    rows=[];unit_total=0
    for runner in m['runners']:
        name=runner['name'];key=name+('-'+runner['attemptId'] if 'attemptId' in runner else '')
        execution=json.loads((WORK/(key+'-execution.json')).read_bytes());assert execution['exitCode']==0,(name,execution)
        assert sha(Path(runner['directory'])/runner['entryAssembly'])==runner['entrySha256']
        for replacement in runner['replacements']:assert sha(Path(runner['directory'])/(replacement['assembly']+'.dll'))==expected[replacement['assembly']]
        bindings=[];observed=set();source_observed=set();entry_seen=False
        for path in sorted(Path(runner['bindingDirectory']).glob('*.json')):
            report=json.loads(path.read_bytes());assert not report['captureErrors'],(name,report)
            for assembly in report['assemblies']:
                if assembly['context']!='Default':continue
                if assembly['name']==name:
                    assert assembly.get('sha256')==runner['entrySha256'];entry_seen=True
                if assembly['name'] in expected:
                    assert assembly.get('sha256')==expected[assembly['name']],(name,assembly);observed.add(assembly['name'])
                for other in runner['otherProductAssemblies']:
                    if assembly['name']==other['assembly']:
                        assert assembly.get('sha256')==other['sha256'];source_observed.add(assembly['name'])
            dest=out/'bindings'/name/path.name;dest.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(path,dest);bindings.append(dest.relative_to(HERE).as_posix())
        assert entry_seen and observed,(name,'Missing actual test/gate process binding evidence')
        record={'runner':name,'kind':runner['kind'],'attemptId':runner.get('attemptId','initial'),'entrySha256':runner['entrySha256'],'observerSha256':execution['hookSha256'],'packageReplacements':runner['replacements'],'sourceBuiltProducts':runner['otherProductAssemblies'],'observedPackageAssemblies':sorted(observed),'observedSourceProducts':sorted(source_observed),'bindingReports':bindings,'execution':execution}
        shutil.copyfile(WORK/(key+'.txt'),out/(key+'.txt'))
        if runner['kind']=='unit':
            source=WORK/'unit-results'/key/'results.trx';raw=source.read_bytes();tree=ET.fromstring(raw);counts=tree.find('.//t:Counters',NS)
            assert tree.find('.//t:ResultSummary',NS).get('outcome')=='Completed';assert int(counts.get('failed'))==0 and int(counts.get('passed'))==int(counts.get('total'))
            passed=int(counts.get('passed'));unit_total+=passed;dest=out/(key+'.trx.gz');dest.write_bytes(gzip.compress(raw,mtime=0));record.update(passed=passed,failed=0,trx=dest.name,uncompressedTrxSha256=hashlib.sha256(raw).hexdigest())
        else:
            gate='qt3' if name=='Bosak.XPath.Conformance' else 'qt4';text=(WORK/(key+'.txt')).read_text(encoding='utf-8-sig')
            counts=[int(re.search(r'^'+label+r':\s*(\d+)',text,re.M).group(1)) for label in ['Passed','Failed','Skipped']];assert counts==([31397,0,424] if gate=='qt3' else [8941,0,1918])
            current=(WORK/(gate+'-skips.txt')).read_text(encoding='utf-8-sig');base=ROOT/'docs/schema-integration/evidence'/(gate+'-skips.txt');assert current==base.read_text(encoding='utf-8-sig')
            (out/(gate+'-skips.txt')).write_bytes(current.replace('\r\n','\n').encode());record.update(gate=gate,counts=counts,exactSkipNamesReasonsUnchanged=True)
            if gate=='qt4':assert len(re.findall(r'^  Starting:',text,re.M))==230;assert 'passes: 11' in text
        rows.append(record)
    assert unit_total==24225 and len([r for r in rows if r['kind']=='unit'])==10
    for name in ['runtime-layout-correction.json','xslt-binding-correction.json','runners.json']:
        if (WORK/name).exists():shutil.copyfile(WORK/name,out/name)
    original=[]
    for name in ['Bosak.XPath.Runtime.Tests','Bosak.Xslt.Tests']:
        if not next(r for r in m['runners'] if r['name']==name).get('attemptId'):continue
        key='initial-'+name;source=WORK/'unit-results'/name/'results.trx';raw=source.read_bytes();(out/(key+'.trx.gz')).write_bytes(gzip.compress(raw,mtime=0));shutil.copyfile(WORK/(name+'.txt'),out/(key+'.txt'));shutil.copyfile(WORK/(name+'-execution.json'),out/(key+'-execution.json'))
        counts=ET.fromstring(raw).find('.//t:Counters',NS);original.append({'runner':name,'passed':int(counts.get('passed')),'failed':int(counts.get('failed')),'trx':key+'.trx.gz','selectedForAggregate':False})
    for name,source in [('observer-v1-build.txt',ROOT/'.guard-tmp/package-binding-hook-build.txt'),('observer-v2-build.txt',ROOT/'.guard-tmp/package-binding-hook-v2-build.txt')]:shutil.copyfile(source,out/name)
    result={'recordFormatVersion':1,'date':'2026-10-30','status':'Exact seven-package cohort locally qualified with unchanged source-built other products; no publication/freeze','packageFeedSha256':m['packageCohortSha256'],'unitTests':{'passed':unit_total,'failed':0,'projects':10},'runners':rows,'preservedInitialAttempts':original,'scope':'Seven package-derived libraries plus qualified source-built XSLT/XQuery/database products where required; LanguageServer and external consumers excluded','productionSourceChanged':False,'testAssertionsChanged':False,'newSkipAdmissions':False,'packageBytesChanged':False,'actualConsumerQualified':False,'publicationOrFreezeAccepted':False,'documentationChecks':'Pending'}
    save(HERE/'RESULTS.json',result);print('Collected 24,225 passing tests, both exact gates and actual package binding reports.')


if __name__=='__main__':main()
