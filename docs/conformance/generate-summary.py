"""Derive documentation from sealed release receipts; never execute or alter a gate."""
import argparse
import collections
import csv
import hashlib
import io
import json
from pathlib import Path
import re
import subprocess
import xml.etree.ElementTree as ET
import zipfile

ROOT = Path(__file__).resolve().parents[2]
OUT = Path(__file__).resolve().parent
SOURCE = '7249d0e01628c47f92b89a3380bc69cf58d70045'
PACKET = ROOT / 'docs/release-linux-collection-hosted'
ARCHIVE = PACKET / 'nuget-42479094503.zip'
NS = {'q': 'http://www.w3.org/2010/09/qt-fots-catalog'}
TRX = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}


def git(*args, cwd=ROOT):
    return subprocess.check_output(['git', '-C', str(cwd), *args]).decode('utf-8-sig').strip()


def parse_skips(text):
    rows, reason, expected, actual = [], None, 0, 0
    for line in text.splitlines():
        if not line.strip():
            continue
        match = re.fullmatch(r'=== (\d+) skipped: (.+) ===', line)
        if match:
            if reason is not None and expected != actual:
                raise ValueError('Skip group count mismatch')
            expected, reason, actual = int(match[1]), match[2], 0
        else:
            if reason is None:
                raise ValueError('Skip without a reason')
            rows.append((line, reason))
            actual += 1
    if reason is not None and expected != actual:
        raise ValueError('Skip group count mismatch')
    if len({name for name, _ in rows}) != len(rows):
        raise ValueError('Duplicate skipped test name')
    return rows


def source_file(path):
    return git('show', f'{SOURCE}:{path}')


def feature_names(source, field):
    body = source.split(field, 1)[1].split('{', 1)[1].split('}', 1)[0]
    return {s.casefold() for s in re.findall(r'"([^"\n]+)"', body)}


def explain(dependencies, suite, source):
    """Editorial explanations of explicit profile mismatches, not a replacement filter."""
    unsupported = feature_names(source, 'UnsupportedFeatures')
    admitted = feature_names(source, 'FrozenAdmittedFeatures')
    specs = {'XP20+', 'XP30+', 'XP31+'}
    specs |= {'XP40+'} if suite == 'qt4' else {'XQ10+', 'XQ30+', 'XQ31', 'XQ31+'}
    explanations = []
    for dep in dependencies:
        kind, value, positive = dep['type'], dep['value'], dep['satisfied']
        label = f'{kind}={value}'
        why = None
        if kind == 'feature':
            if value.casefold() not in admitted | unsupported:
                why = f'Unreviewed feature dependency ({label}); neither polarity is admitted'
            elif positive and value.casefold() in unsupported:
                why = f'Requires unsupported feature ({label}) in this harness profile'
            elif not positive and value.casefold() not in unsupported:
                why = f'Requires absence of a feature admitted by this profile ({label})'
        elif kind == 'spec':
            compatible = bool(set(value.split()) & specs)
            if positive and not compatible:
                why = (f'Requires an excluded language/version profile ({label}); '
                       + ('the 4.0 harness executes XPath, not XQuery' if suite == 'qt4'
                          else 'the frozen routes target XPath/XQuery 3.1-compatible versions'))
            elif not positive and compatible:
                why = f'Requires exclusion of an admitted language/version profile ({label})'
        elif (kind, value) in {('language', 'xib'), ('calendar', 'CB'),
                               ('format-integer-sequence', '\uFBF4')}:
            if positive:
                why = f'Requires unsupported formatting option ({label}); only fallback is available'
        elif kind == 'default-language':
            if (value == 'en') != positive:
                why = f'Default-language requirement does not match the English-default profile ({label}, satisfied={positive})'
        elif kind == 'unicode-version':
            why = f'No specific Unicode version is guaranteed by this .NET-backed profile ({label})'
        elif not positive:
            why = f'Negated dependency is not admitted by this harness profile ({label})'
        elif kind == 'xml-version' and '1.1' not in value.split():
            why = f'Requires an XML-version profile excluded by the frozen XML 1.1 admission policy ({label})'
        elif kind == 'xsd-version' and value != '1.1':
            why = f'Requires an XSD-version profile excluded by the frozen 1.1 admission policy ({label}); this policy is not full XSD support'
        if why and why not in explanations:
            explanations.append(why)
    if not explanations:
        raise ValueError(f'Unexplained {suite} profile refusal: {dependencies}')
    return '; '.join(explanations)


def catalog_cases(suite, wanted, corpus_root):
    root = corpus_root / f'{suite}tests'
    expected = git('rev-parse', f'{SOURCE}:tests/{suite}tests')
    if git('rev-parse', 'HEAD', cwd=root) != expected or git('status', '--porcelain', cwd=root):
        raise ValueError(f'{suite} corpus must match its clean producer gitlink')
    cases = {}
    for ref in ET.parse(root / 'catalog.xml').getroot().findall('q:test-set', NS):
        path = root / ref.attrib['file']
        tree = ET.parse(path).getroot()
        inherited = tree.findall('q:dependency', NS)
        for case in tree.findall('q:test-case', NS):
            name = case.get('name')
            if name not in wanted:
                continue
            if name in cases:
                raise ValueError(f'Duplicate catalog identity: {name}')
            dependencies = [dict(type=d.get('type', ''), value=d.get('value', ''),
                                 satisfied=d.get('satisfied', 'true') == 'true')
                            for d in inherited + case.findall('q:dependency', NS)]
            catalog_path = f'tests/{suite}tests/{ref.attrib["file"]}'
            cases[name] = (ref.get('name'), catalog_path, dependencies)
    if set(cases) != wanted:
        raise ValueError(f'Unresolved catalog test names: {wanted - set(cases)}')
    return cases, expected


def build(corpus_root=None):
    corpus_root = (corpus_root or ROOT / 'tests').resolve()
    manifest = json.loads((PACKET / 'MANIFEST.json').read_text(encoding='utf-8-sig'))
    for name in ['nuget-42479094503.zip', '42479094503-run.json']:
        data = (PACKET / name).read_bytes()
        if hashlib.sha256(data).hexdigest() != manifest[name]['sha256'] or len(data) != manifest[name]['bytes']:
            raise ValueError(f'Sealed input changed: {name}')
    run = json.loads((PACKET / '42479094503-run.json').read_text(encoding='utf-8-sig'))
    if run['head_sha'] != SOURCE or run['conclusion'] != 'success':
        raise ValueError('Producer identity/result mismatch')
    result = dict(sourceCommit=SOURCE, run=run['html_url'], date='2026-11-08',
                  platform='Ubuntu 24.04.5, runner image 20261101.360.1',
                  sdk='10.0.301', configuration='Release', framework='net10.0',
                  archiveSha256=hashlib.sha256(ARCHIVE.read_bytes()).hexdigest(),
                  suites={}, unitProjects=[])
    files = {}
    with zipfile.ZipFile(ARCHIVE) as archive:
        for suite, counts in [('qt3', [31397, 0, 424]), ('qt4', [8941, 0, 1918])]:
            raw = archive.read(f'qualification/gates/{suite}-skips.txt')
            rows = parse_skips(raw.decode('utf-8-sig'))
            text = archive.read(f'qualification/gates/{suite}.txt').decode('utf-8-sig')
            observed = [int(re.search(rf'^{key}:\s+(\d+)', text, re.M)[1])
                        for key in ['Passed', 'Failed', 'Skipped']]
            if observed != counts or len(rows) != counts[2]:
                raise ValueError(f'{suite} gate/skip counts mismatch')
            cases, corpus = catalog_cases(suite, {name for name, _ in rows}, corpus_root)
            filter_path = ('tests/Bosak.XPath.Conformance/DependencyFilter.cs' if suite == 'qt3'
                           else 'tests/Bosak.XPath.Conformance40/DependencyFilter40.cs')
            filter_source = source_file(filter_path)
            csv_text = io.StringIO(newline='')
            writer = csv.writer(csv_text, lineterminator='\n')
            writer.writerow(['test_set', 'test', 'recorded_reason', 'explanation', 'catalog_file', 'dependencies'])
            for name, reason in sorted(rows):
                test_set, catalog, dependencies = cases[name]
                explanation = explain(dependencies, suite, filter_source) if reason == 'Unsupported dependency' else reason
                writer.writerow([test_set, name, reason, explanation, catalog,
                                 json.dumps(dependencies, ensure_ascii=False, separators=(',', ':'))])
            files[f'{suite}-skips.csv'] = csv_text.getvalue().encode('utf-8')
            files[f'evidence/{suite}-skips.txt'] = raw
            files[f'evidence/{suite}.txt'] = archive.read(f'qualification/gates/{suite}.txt')
            result['suites'][suite] = dict(passed=counts[0], failed=counts[1], skipped=counts[2],
                corpusCommit=corpus, skipSha256=hashlib.sha256(raw).hexdigest(),
                recordedReasonCounts=dict(collections.Counter(reason for _, reason in rows)),
                filterSource=filter_path)
            if suite == 'qt4':
                selected_sets = len(re.findall(r'^  Done: ', text, re.M))
                if selected_sets != 230:
                    raise ValueError('Selected qt4 test-set count mismatch')
                result['suites'][suite]['gateOnly'] = True
                result['suites'][suite]['selectedSets'] = selected_sets
                result['suites'][suite]['experimentalPassesIncludedInPassed'] = int(
                    re.search(r'Experimental-level .* passes: (\d+)', text)[1])
        for name in archive.namelist():
            if name.endswith('.trx'):
                tree = ET.fromstring(archive.read(name))
                counters = tree.find('t:ResultSummary/t:Counters', TRX).attrib
                method = tree.find('t:TestDefinitions/t:UnitTest/t:TestMethod', TRX)
                result['unitProjects'].append(dict(project=method.get('codeBase').split('/')[-1],
                    passed=int(counters['passed']), failed=int(counters['failed']),
                    skipped=int(counters['notExecuted']), receipt=name))
        result['unitProjects'].sort(key=lambda row: row['project'])
    if (len(result['unitProjects']) != 10
            or sum(row['passed'] for row in result['unitProjects']) != 26345
            or any(row['failed'] or row['skipped'] for row in result['unitProjects'])):
        raise ValueError('Solution aggregate mismatch')
    files['summary.json'] = (json.dumps(result, ensure_ascii=False, indent=2) + '\n').encode('utf-8')
    return files


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check', action='store_true', help='Verify existing generated files without writing')
    parser.add_argument('--corpus-root', type=Path, default=ROOT / 'tests',
                        help='Directory containing clean, pinned qt3tests and qt4tests checkouts')
    args = parser.parse_args()
    files = build(args.corpus_root)
    for name, content in files.items():
        path = OUT / name
        if args.check:
            if not path.is_file() or path.read_bytes() != content:
                raise SystemExit(f'Stale or modified generated document: {path}')
        else:
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_bytes(content)
    print(f'{"Verified" if args.check else "Generated"} {len(files)} files; 424 QT3 and 1,918 qt4 skips explained.')


if __name__ == '__main__':
    main()
