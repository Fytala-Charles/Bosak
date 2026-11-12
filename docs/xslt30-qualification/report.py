"""Reconcile unchanged-runner output with every original catalog case and its skip policy."""
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

NS = {'t': 'http://www.w3.org/2012/10/xslt-test-catalog'}
ROOT = Path(__file__).resolve().parents[2]


def spec_supported(value):
    for token in value.split():
        if not token.upper().startswith('XSLT'):
            continue
        number = token[4:].removesuffix('+')
        if not number.isdigit() or (30 >= int(number) if token.endswith('+') else 30 == int(number)):
            return True
    return False


def explain_silent(case, unsupported, file):
    deps = case.find('t:dependencies', NS)
    if deps is not None:
        for dep in deps.findall('t:spec', NS):
            if not spec_supported(dep.get('value', '')):
                return 'Catalog spec=' + dep.get('value', '') + ' is outside the retained XSLT 3.0 catalog profile'
        for dep in deps.findall('t:unicode-version', NS):
            if dep.get('value', '') != '9.0':
                return 'Requires Unicode ' + dep.get('value', '') + '; this runner admits Unicode 9.0'
        for dep in deps.findall('t:enable_assertions', NS):
            if dep.get('satisfied', 'true') == 'false':
                return 'Requires assertions disabled; this runner always enables xsl:assert'
        for kind in ['feature', 'year_component_values']:
            for dep in deps.findall('t:' + kind, NS):
                value = dep.get('value', '')
                supported = value.casefold() not in unsupported
                negative = dep.get('satisfied', 'true') == 'false'
                if supported == negative:
                    return (f'Requires absence of admitted {kind}={value}' if negative
                            else f'Requires {kind}={value}, excluded by this runner profile')
        for dep in deps.findall('t:maximum_number_of_decimal_digits', NS):
            value = dep.get('value', '')
            if value.isdigit() and int(value) > 28:
                return f'Requires {value} decimal digits; retained legacy precision limit is 28'
        for dep in deps.findall('t:ignore_doc_failure', NS):
            if dep.get('satisfied', 'true') == 'true':
                return 'Requires ignoring document-load failure; the retained runner does not support this policy'
        for dep in deps.findall('t:package_version_resolution', NS):
            value = dep.get('value', '')
            supported = value in {'highest_version', 'lowest_version', 'unspecified'}
            negative = dep.get('satisfied', 'true') == 'false'
            if supported == negative:
                return f'Package-version-resolution requirement does not match the runner: value={value}, satisfied={not negative}'
    if case.find('t:test', NS) is None:
        return 'Adapter has no test element to execute in this catalog case'
    if case.find('t:result', NS) is None:
        return 'Adapter has no result/assertion element in this catalog case'
    return 'Adapter returned Skip without a printed reason; inspect the pinned case and skip-return sites: ' + file


def set_reason(name, original):
    reasons = {
        'error': 'Existing full-sweep exclusion: error set; policy cites incomplete static-validator coverage',
        'import-schema': 'Existing full-sweep exclusion: schema import requires schema-aware mode; this run is basic',
        'regex-syntax-xslt20': 'Existing full-sweep exclusion: exact XSLT 2.0 regex expectations differ from this profile',
        'unicode-90': 'Existing full-sweep throughput exclusion: this Unicode set is slow; not evidence that every case is unsupported',
    }
    return reasons.get(name, original)


def named_skip_notes(source):
    body = source.split('HashSet<string> SkipTests', 1)[1].split('\n    };', 1)[0]
    notes, comment, had_names = {}, [], False
    for line in body.splitlines():
        stripped = line.strip()
        if stripped.startswith('//'):
            if had_names:
                comment, had_names = [], False
            comment.append(stripped[2:].strip())
            continue
        code = line.split('//', 1)[0]
        for name in re.findall(r'"([^"\n]+)"', code):
            notes[name] = ' '.join(comment)
            had_names = True
    return notes


def parse(plan, text, complete):
    sets = {s['name']: s for s in plan['sets']}
    if len(sets) != len(plan['sets']):
        raise ValueError('Duplicate test-set name')
    rows, current, pending = {}, None, None

    def record(name, status, reason, line, provenance):
        key = (current, name)
        if current not in sets or name not in sets[current]['cases'] or key in rows:
            raise ValueError(f'Unknown/duplicate outcome: {key}')
        rows[key] = dict(test_set=current, test=name, status=status, reason=reason,
                         evidence_line=line, reason_source=provenance, catalog_file=sets[current]['file'])

    def finish_pending(status='Skipped'):
        nonlocal pending
        if pending:
            name, line = pending
            record(name, status, 'Silent skip returned by unchanged runner' if status == 'Skipped'
                   else 'Runner ended before this case emitted an outcome', line, 'DerivedFromControlFlow')
            pending = None

    for line_number, line in enumerate(text.splitlines(), 1):
        start = re.fullmatch(r'  Starting: (.+) \.\.\.', line)
        if start:
            finish_pending()
            current = start[1]
            continue
        whole = re.fullmatch(r'  SKIP ([^:]+): (.*\(.*\d+ tests\))', line)
        if whole and whole[1] in sets:
            finish_pending()
            current = whole[1]
            for name in sets[current]['cases']:
                record(name, 'Skipped', whole[2], line_number, 'RunnerSetSkip')
            continue
        event = re.match(r'^  (RUN |PASS|FAIL|SKIP) (\S+)(?::\s*(.*))?$', line)
        if event:
            kind, name, message = event[1].strip(), event[2], event[3] or ''
            if kind == 'RUN':
                finish_pending()
                pending = (name, line_number)
            else:
                if pending and pending[0] != name:
                    finish_pending()
                record(name, {'PASS': 'Passed', 'FAIL': 'Failed', 'SKIP': 'Skipped'}[kind],
                       message or 'Original assertion accepted', line_number, 'RunnerCaseLine')
                pending = None
            continue
        done = re.fullmatch(r'  Done: (.+) \((\d+) tests, (\d+) passed, (\d+) failed, (\d+) skipped\)', line)
        if done:
            finish_pending()
            current = done[1]
            counts = collections.Counter(r['status'] for (s, _), r in rows.items() if s == current)
            if [len(sets[current]['cases']), counts['Passed'], counts['Failed'], counts['Skipped']] != [int(done[i]) for i in range(2, 6)]:
                raise ValueError(f'Per-set outcome mismatch: {current}')
    finish_pending('Skipped' if complete else 'Incomplete')
    for name, item in sets.items():
        for case in item['cases']:
            if (name, case) not in rows:
                rows[(name, case)] = dict(test_set=name, test=case, status='NotRun',
                    reason='No outcome observed before the attempt ended', evidence_line='',
                    reason_source='CoverageReconciliation', catalog_file=item['file'])
    if complete and any(r['status'] in {'NotRun', 'Incomplete'} for r in rows.values()):
        raise ValueError('Complete runner summary lacks per-case coverage')
    return list(rows.values())


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--packet', type=Path, required=True)
    args = parser.parse_args()
    packet = args.packet.resolve()
    plan = json.loads((packet / 'PLAN.json').read_text(encoding='utf-8'))
    receipt = json.loads((packet / 'RUN.json').read_text(encoding='utf-8'))
    text = (packet / 'stdout.log').read_text(encoding='utf-8-sig', errors='replace')
    rows = parse(plan, text, receipt['completeCatalogCounts'])
    source = subprocess.check_output(['git', '-C', str(ROOT), 'show', plan['sourceCommit'] + ':tests/Bosak.Xslt.Conformance/Program.cs'], text=True, encoding='utf-8-sig')
    body = source.split('HashSet<string> SkipFeatures', 1)[1].split('{', 1)[1].split('}', 1)[0]
    body = '\n'.join(line.split('//', 1)[0] for line in body.splitlines())
    unsupported = {s.casefold() for s in re.findall(r'"([^"\n]+)"', body)}
    notes = named_skip_notes(source)
    cases = {}
    for item in plan['sets']:
        path = packet / 'corpus' / item['file']
        if hashlib.sha256(path.read_bytes()).hexdigest() != item['sha256']:
            raise ValueError('Original test-set definitions changed during run: ' + item['file'])
        for case in ET.parse(path).getroot().findall('t:test-case', NS):
            cases[(item['name'], case.get('name'))] = case
    for row in rows:
        if row['reason_source'] == 'DerivedFromControlFlow' and row['status'] == 'Skipped':
            row['reason'] = explain_silent(cases[(row['test_set'], row['test'])], unsupported, row['catalog_file'])
            row['reason_source'] = 'DerivedFromPinnedPolicyAndCatalog'
        elif row['reason_source'] == 'RunnerSetSkip':
            row['recorded_reason'] = row['reason']
            row['reason'] = set_reason(row['test_set'], row['reason'])
        elif row['status'] == 'Skipped' and row['reason'] == 'Known harness skip':
            if not notes.get(row['test']):
                raise ValueError('Named skip has no documented policy note: ' + row['test'])
            row['recorded_reason'] = row['reason']
            row['reason'] = 'Retained named skip; historical policy note (not revalidated): ' + notes[row['test']]
            row['reason_source'] = 'DerivedFromPinnedNamedSkipComment'
        row.setdefault('recorded_reason', row['reason'] if row['reason_source'] == 'RunnerCaseLine' else '')
    fields = ['test_set', 'test', 'status', 'reason', 'recorded_reason', 'reason_source', 'evidence_line', 'catalog_file']
    for name, selected in [('outcomes.csv', rows), ('failures.csv', [r for r in rows if r['status'] == 'Failed']),
                           ('skips.csv', [r for r in rows if r['status'] == 'Skipped'])]:
        stream = io.StringIO(newline='')
        writer = csv.DictWriter(stream, fieldnames=fields, lineterminator='\n')
        writer.writeheader()
        writer.writerows(selected)
        (packet / name).write_text(stream.getvalue(), encoding='utf-8', newline='\n')
    counts = collections.Counter(r['status'] for r in rows)
    if receipt['completeCatalogCounts']:
        for name in ['Passed', 'Failed', 'Skipped']:
            if counts[name] != receipt['counts'][name]:
                raise ValueError('Aggregate outcome mismatch: ' + name)
    report = dict(sourceCommit=plan['sourceCommit'], corpusCommit=plan['corpusCommit'],
        counts=dict(counts), totalCases=len(rows), completeCatalogCounts=receipt['completeCatalogCounts'],
        unresolvedSkipReasons=[dict(test_set=r['test_set'], test=r['test']) for r in rows
                              if r['reason'].startswith('Adapter returned Skip without a printed reason')],
        skipReasons=dict(collections.Counter(r['reason'] for r in rows if r['status'] == 'Skipped')),
        failureSets=dict(collections.Counter(r['test_set'] for r in rows if r['status'] == 'Failed')))
    (packet / 'REPORT.json').write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
    print(json.dumps({k: v for k, v in report.items() if k not in ['skipReasons', 'unresolvedSkipReasons']}, indent=2))
    print('Unresolved skip reasons:', len(report['unresolvedSkipReasons']))


if __name__ == '__main__':
    main()
