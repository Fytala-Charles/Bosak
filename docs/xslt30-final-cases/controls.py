"""Bounded runner controls: independent golden inputs, missing/stale outputs and logical URIs."""
import argparse
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import subprocess
import xml.etree.ElementTree as ET


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output', type=Path, required=True)
    parser.add_argument('--assembly', type=Path, required=True)
    args = parser.parse_args()
    root = args.output.resolve()
    root.mkdir(parents=True, exist_ok=False)
    spec = importlib.util.spec_from_file_location('bounded', Path(__file__).parents[1] / 'xslt30-qualification/run.py')
    bounded = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(bounded)
    ns = 'http://www.w3.org/2012/10/xslt-test-catalog'
    ET.register_namespace('', ns)
    catalog = ET.Element('{%s}catalog' % ns)
    ET.SubElement(catalog, '{%s}test-set' % ns, name='output-isolation-controls', file='controls.xml')
    ET.ElementTree(catalog).write(root / 'catalog.xml', encoding='utf-8', xml_declaration=True)
    suite = ET.Element('{%s}test-set' % ns, name='output-isolation-controls')
    (root / 'golden.xml').write_text('<expected/>', encoding='utf-8')
    (root / 'escape.xml').write_text('<expected/>', encoding='utf-8')
    logical = (root / 'logical.xml').as_uri()
    cases = [
        ('matching', '<xsl:result-document href="golden.xml"><expected/></xsl:result-document>', '<assert-result-document uri="golden.xml"><assert-xml file="golden.xml"/></assert-result-document>', True),
        ('mismatch', '<xsl:result-document href="golden.xml"><wrong/></xsl:result-document>', '<assert-result-document uri="golden.xml"><assert-xml file="golden.xml"/></assert-result-document>', False),
        ('missing', '<main/>', '<assert-result-document uri="golden.xml"><assert-xml file="golden.xml"/></assert-result-document>', False),
        ('stale-producer', '<xsl:result-document href="stale.xml"><expected/></xsl:result-document>', '<assert-result-document uri="stale.xml"><assert-xml file="golden.xml"/></assert-result-document>', True),
        ('stale-consumer', '<main/>', '<assert-result-document uri="stale.xml"><assert-xml file="golden.xml"/></assert-result-document>', False),
        ('absolute', f'<xsl:result-document href="{(root / "escape.xml").as_uri()}"><expected/></xsl:result-document>', f'<assert-result-document uri="{(root / "escape.xml").as_uri()}"><assert-xml file="golden.xml"/></assert-result-document>', True),
        ('logical-uri', '<main uri="{current-output-uri()}"/><xsl:result-document href="logical.xml"><uri><xsl:value-of select="current-output-uri()"/></uri></xsl:result-document>', f'<all-of><assert-xml>&lt;main uri="{(root / "principal.xml").as_uri()}"/&gt;</assert-xml><assert-result-document uri="logical.xml"><assert-xml>&lt;uri&gt;{logical}&lt;/uri&gt;</assert-xml></assert-result-document></all-of>', True),
        ('text', '<xsl:result-document href="text.txt" method="text">hello</xsl:result-document>', '<assert-result-document uri="text.txt"><assert-string-value>hello</assert-string-value></assert-result-document>', True),
        ('https', '<xsl:result-document href="https://example.invalid/result.xml"><expected/></xsl:result-document>', '<assert-result-document uri="https://example.invalid/result.xml"><assert-xml file="golden.xml"/></assert-result-document>', True),
        ('duplicate', '<xsl:result-document href="duplicate.xml"><one/></xsl:result-document><xsl:result-document href="duplicate.xml"><two/></xsl:result-document>', '<error code="XTDE1490"/>', True),
        ('catch-serialization', '<xsl:try><xsl:result-document href="error.xml" method="xml" build-tree="no"><xsl:sequence select="map{\'a\':1}"/></xsl:result-document><unexpected/><xsl:catch errors="err:SENR0001"><caught/></xsl:catch></xsl:try>', '<assert-xml>&lt;caught/&gt;</assert-xml>', True),
    ]
    cases.extend([
        ('relative-match', '<xsl:result-document href=""><xsl:result-document href="golden.xml"><expected/></xsl:result-document></xsl:result-document>', '<assert-result-document uri="golden.xml"><assert-xml file="golden.xml"/></assert-result-document>', True),
        ('relative-mismatch', '<xsl:result-document href=""><xsl:result-document href="golden.xml"><wrong/></xsl:result-document></xsl:result-document>', '<assert-result-document uri="golden.xml"><assert-xml file="golden.xml"/></assert-result-document>', False),
        ('relative-missing', '<xsl:result-document href=""><main/></xsl:result-document>', '<assert-result-document uri="golden.xml"><assert-xml file="golden.xml"/></assert-result-document>', False),
    ])
    for name, content, assertion, _ in cases:
        case = ET.SubElement(suite, '{%s}test-case' % ns, name=name)
        ET.SubElement(case, '{%s}description' % ns).text = 'Local output-isolation control; not an upstream contribution.'
        test = ET.SubElement(case, '{%s}test' % ns)
        ET.SubElement(test, '{%s}stylesheet' % ns, file=name + '.xsl')
        if not name.startswith('relative-'):
            ET.SubElement(test, '{%s}output' % ns, file='principal.xml')
        case.append(ET.fromstring(f'<result xmlns="{ns}">{assertion}</result>'))
        (root / (name + '.xsl')).write_text('<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:err="http://www.w3.org/2005/xqt-errors" exclude-result-prefixes="err" version="3.0"><xsl:template name="xsl:initial-template">' + content + '</xsl:template></xsl:stylesheet>', encoding='utf-8')
    ET.ElementTree(suite).write(root / 'controls.xml', encoding='utf-8', xml_declaration=True)
    def inventory():
        return {p.name: hashlib.sha256(p.read_bytes()).hexdigest() for p in root.iterdir()
                if p.suffix in {'.xml', '.xsl', '.txt'} and p.name != 'last_test.txt'}
    before = inventory()
    command = ['dotnet', str(args.assembly.resolve()), '--processor-profile', 'legacy', str(root / 'catalog.xml')]
    job = bounded.BoundedJob(8 * 1024**3)
    with (root / 'stdout.log').open('wb') as out, (root / 'stderr.log').open('wb') as err:
        with bounded.process_error_mode():
            process = subprocess.Popen(command, cwd=root, stdout=out, stderr=err, creationflags=subprocess.CREATE_NO_WINDOW)
        try:
            try:
                job.assign(process)
            except Exception:
                process.kill()
                process.wait(timeout=10)
                raise
            process.wait(timeout=120)
        finally:
            job.close()
            process.wait(timeout=15)
    output = (root / 'stdout.log').read_text(encoding='utf-8-sig')
    outcomes = {name: bool(re.search(r'^  PASS ' + re.escape(name) + r'\s*$', output, re.M)) for name, _, _, _ in cases}
    expected = {name: passed for name, _, _, passed in cases}
    wrong_results_refused = all(re.search(r'^  FAIL ' + re.escape(name) + r': Result mismatch\s*$', output, re.M)
                                for name, passed in expected.items() if not passed)
    after = inventory()
    receipt = dict(command=command, runnerExit=process.returncode, expected=expected, actual=outcomes,
                   before=before, after=after, unchangedInputs=before == after,
                   wrongResultsRefused=bool(wrong_results_refused),
                   success=outcomes == expected and bool(wrong_results_refused) and before == after
                   and 'Selected-case result: FAIL' in output and 'Pass rate:' not in output
                   and bool(re.search(r'^Failed:\s+5\s*$', output, re.M)))
    (root / 'CONTROLS.json').write_text(json.dumps(receipt, indent=2) + '\n', encoding='utf-8')
    print(json.dumps({k: receipt[k] for k in ['actual', 'unchangedInputs', 'success']}, indent=2))
    return 0 if receipt['success'] else 1


if __name__ == '__main__':
    raise SystemExit(main())
