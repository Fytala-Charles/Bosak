"""Validate member coverage, signatures and conservative Bosak type reachability."""
import collections
import hashlib
import json
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[3]
HERE = Path(__file__).resolve().parents[1]
BASE = ROOT / 'docs/foundation-api-baseline/evidence'


def load(path):
    return json.loads(path.read_bytes())


def validate(manifest, closure, cases):
    baseline = load(BASE / 'public-api.json')
    assemblies = {a['assembly']: a['declarations'] for a in baseline['assemblies']}
    expected = {(a, i) for a, rows in assemblies.items() for i in range(len(rows))}
    entries = manifest['entries']
    assert len(entries) == len(expected), 'Entry count mismatch'
    assert {(e['assembly'], e['baselineRow']) for e in entries} == expected, 'Missing or duplicate baseline row'
    assert len({e['id'] for e in entries}) == len(entries), 'Duplicate entry ID'
    assert manifest['freezeAccepted'] is False
    assert set(manifest['ownerDispositions'].values()) == {'Pending'}
    for name, digest in manifest['inputHashes'].items():
        assert hashlib.sha256((ROOT / name).read_bytes()).hexdigest() == digest, 'Changed input: ' + name
    known = {r[2:].split(':', 1)[0] for rows in assemblies.values() for r in rows if r.startswith('T ')}
    assert not any('<' in name for name in known), 'Generic type definitions require parser extension'
    case_ids = {c['id'] for c in cases['cases']}
    assert len(case_ids) == len(cases['cases']) == 20
    assert all(c['status'] == 'NotRun' and c['resultReceipt'] is None for c in cases['cases'])
    assert set(manifest['globalCaseIds']) <= case_ids
    edges = collections.defaultdict(list)
    slots = collections.Counter()
    groups = collections.Counter()
    kinds = collections.Counter()
    for entry in entries:
        row = assemblies[entry['assembly']][entry['baselineRow']]
        assert entry['id'] == hashlib.sha256((entry['assembly'] + '\n' + row).encode()).hexdigest(), 'Signature ID mismatch'
        symbol = row[2:].split(':', 1)[0]
        assert entry['type'] in known and (symbol == entry['type'] or symbol.startswith(entry['type'] + '.')), 'Wrong declaring type'
        assert entry['kind'] == row[0]
        assert entry['evidenceOnly'] == (row[0] in ['A', 'L'])
        assert set(entry['caseIds']) <= case_ids and 'FC-19' in entry['caseIds']
        assert entry['reviewGroup'] in {'OutsideProposedFoundation', 'DraftMemberReview', 'SchemaContractReview', 'FoundationCandidate', 'RuntimeDependencyReview', 'ProviderContractReview', 'CompatibilityWatch'}
        # Token scanning is independent of the compiler's per-type regex matching.
        tokens = set(re.findall(r'Bosak\.[A-Za-z0-9_.+`]+', row.split(':', 1)[1])) if not entry['evidenceOnly'] else set()
        dependencies = (tokens & known) - {entry['type']}
        assert set(entry['signatureTypes']) == dependencies, 'Signature dependency mismatch'
        for dependency in dependencies:
            edges[(entry['type'], dependency)].append(entry['id'])
        type_row = next(r for r in assemblies[entry['assembly']] if r.startswith('T ' + entry['type'] + ':'))
        interface = 'ClassSemanticsMask' in type_row.split(':', 2)[1]
        expected_slot = ('Required' if 'Abstract' in row.split(':', 2)[1] else 'DefaultImplementation') if interface and row[0] == 'M' else None
        assert entry['interfaceSlot'] == expected_slot, 'Interface slot mismatch'
        if expected_slot:
            slots[expected_slot] += 1
        groups[entry['reviewGroup']] += 1
        kinds[entry['kind']] += 1
    assert {(e['from'], e['to']) for e in closure['edges']} == set(edges), 'Closure edge mismatch'
    for edge in closure['edges']:
        ids = edges[(edge['from'], edge['to'])]
        assert edge['witnessEntryId'] in ids and edge['declarationCount'] == len(ids)
    reached = set(closure['seeds'])
    assert reached <= known
    while True:
        expanded = reached | {b for a, b in edges if a in reached}
        if expanded == reached:
            break
        reached = expanded
    assert set(closure['reachableTypes']) == reached, 'Incomplete transitive closure'
    assert {t['type'] for t in closure['additionalReviewTypes']} == reached - set(closure['seeds'])
    assert manifest['summary']['entries'] == len(entries)
    assert manifest['summary']['types'] == len(known)
    assert manifest['summary']['interfaceSlots'] == dict(slots)
    assert manifest['summary']['reviewGroups'] == dict(groups)
    assert manifest['summary']['declarationKinds'] == dict(kinds)
    assert manifest['summary']['behavioralCasesExecuted'] == 0
    return len(entries), len(reached)


def main():
    count, reached = validate(load(HERE / 'evidence/member-manifest.json'), load(HERE / 'evidence/signature-closure.json'), load(HERE / 'compatibility-cases.json'))
    print(f'Verified {count} entries, {reached} reachable types and 20 planned compatibility cases; freeze pending.')


if __name__ == '__main__':
    main()
