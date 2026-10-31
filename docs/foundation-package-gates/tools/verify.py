"""Verify full test/gate receipts and the exact package/source-built binding mix."""
import gzip
import hashlib
import json
from pathlib import Path
import re
import xml.etree.ElementTree as ET

HERE=Path(__file__).resolve().parents[1];ROOT=HERE.parents[1]
NS={'t':'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}


def load(path):return json.loads(path.read_bytes())


def validate(results):
    packages=load(ROOT/'docs/foundation-package-qualification/evidence/packages.json')
    expected={p['id']:p['assemblySha256'] for p in packages['packages']}
    qualified={a['assembly']:a['candidateSha256'].lower() for a in load(ROOT/'docs/streaming-completion-cleanup/evidence/api-comparison.json')['assemblies']}
    assert results['packageFeedSha256']==packages['feedArchive']['sha256']
    assert results['publicationOrFreezeAccepted'] is False and results['packageBytesChanged'] is False
    rows=results['runners'];units=[r for r in rows if r['kind']=='unit'];gates=[r for r in rows if r['kind']=='gate']
    assert len(rows)==12 and len(units)==10 and len(gates)==2
    assert len({r['runner'] for r in rows})==12
    total=0;all_packages=set();observer_hashes={o['assemblySha256'] for o in results['observers']}
    for row in rows:
        assert row['execution']['exitCode']==0
        assert row['observerSha256'] in observer_hashes
        for other in row['sourceBuiltProducts']:
            assert other['assembly'] in {'Bosak.Xslt','Bosak.XQuery','Bosak.XPath.Providers.Database'}
            assert other['sha256']==qualified[other['assembly']]
        observed=set();entry=False
        for filename in row['bindingReports']:
            report=load(HERE/filename);assert not report['captureErrors']
            for assembly in report['assemblies']:
                if assembly['context']!='Default':continue
                name=assembly['name']
                if name==row['runner']:
                    assert assembly.get('sha256')==row['entrySha256'];entry=True
                if name in expected:
                    assert assembly.get('sha256')==expected[name], 'Wrong loaded package hash'
                    assert assembly['informationalVersion']=='1.34.2+'+packages['sourceRevision']
                    observed.add(name)
                if name in {'Bosak.Xslt','Bosak.XQuery','Bosak.XPath.Providers.Database'}:
                    assert assembly.get('sha256')==qualified[name]
        assert entry and observed, 'Missing actual runner/package binding proof'
        assert observed==set(row['observedPackageAssemblies'])
        all_packages|=observed
        if row['kind']=='unit':
            raw=gzip.decompress((HERE/'evidence'/row['trx']).read_bytes())
            assert hashlib.sha256(raw).hexdigest()==row['uncompressedTrxSha256']
            tree=ET.fromstring(raw);counts=tree.find('.//t:Counters',NS)
            assert tree.find('.//t:ResultSummary',NS).get('outcome')=='Completed'
            assert int(counts.get('failed'))==row['failed']==0
            assert int(counts.get('passed'))==int(counts.get('total'))==row['passed']
            total+=row['passed']
        else:
            assert observed==set(expected), 'Gate did not observe all seven package assemblies'
            desired=[31397,0,424] if row['gate']=='qt3' else [8941,0,1918]
            assert row['counts']==desired
            key=row['runner']+('-'+row['attemptId'] if row['attemptId']!='initial' else '')
            text=(HERE/'evidence'/(key+'.txt')).read_text(encoding='utf-8-sig')
            assert [int(re.search(r'^'+label+r':\s*(\d+)',text,re.M).group(1)) for label in ['Passed','Failed','Skipped']]==desired
            gate=row['gate'];saved=(HERE/'evidence'/(gate+'-skips.txt')).read_text(encoding='utf-8-sig')
            assert saved==(ROOT/'docs/schema-integration/evidence'/(gate+'-skips.txt')).read_text(encoding='utf-8-sig')
            if gate=='qt4':assert len(re.findall(r'^  Starting:',text,re.M))==230 and 'passes: 11' in text
    assert total==results['unitTests']['passed']==24225
    assert all_packages==set(expected)
    original={r['runner']:r for r in results['preservedInitialAttempts']}
    assert (original['Bosak.XPath.Runtime.Tests']['passed'],original['Bosak.XPath.Runtime.Tests']['failed'])==(218,51)
    failed=ET.fromstring(gzip.decompress((HERE/'evidence'/original['Bosak.XPath.Runtime.Tests']['trx']).read_bytes()))
    for case in failed.findall('.//t:UnitTestResult',NS):
        if case.get('outcome')=='Failed':
            message=case.find('t:Output/t:ErrorInfo/t:Message',NS).text
            assert 'DirectoryNotFoundException' in message and 'qt3tests' in message
    assert original['Bosak.Xslt.Tests']['passed']==8196 and original['Bosak.Xslt.Tests']['failed']==0
    return total,len(gates),len(all_packages)


if __name__=='__main__':
    result=validate(load(HERE/'RESULTS.json'))
    manifest=HERE/'MANIFEST.json'
    if manifest.exists():
        for entry in load(manifest)['files']:assert hashlib.sha256((HERE/entry['path']).read_bytes()).hexdigest()==entry['sha256'],entry['path']
    print('Verified unit tests/gates/package binding union:',result,'; other products remain explicitly source-built.')
