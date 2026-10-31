"""Generate a proposed member review manifest from the immutable F140-01 baseline."""
import argparse
import collections
import hashlib
import json
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[3]
HERE = Path(__file__).resolve().parents[1]
BASE = ROOT / 'docs/foundation-api-baseline/evidence'


def digest(data):
    return hashlib.sha256(data).hexdigest()


def read(path):
    return json.loads(path.read_bytes())


def generate():
    api = read(BASE / 'public-api.json')
    rules = read(HERE / 'scope-rules.json')
    cases = read(HERE / 'compatibility-cases.json')
    known = {}
    type_rows = {}
    for assembly in api['assemblies']:
        for row in assembly['declarations']:
            if row.startswith('T '):
                name = row[2:].split(':', 1)[0]
                assert name not in known, 'Ambiguous assembly ownership: ' + name
                known[name] = assembly['assembly']
                type_rows[name] = row
    names = sorted(known, key=lambda n: (-len(n), n))
    refs = {name: re.compile(r'(?<![\w.+`])' + re.escape(name) + r'(?![\w.+`])') for name in names}
    edges = {}
    records = []
    slots = collections.Counter()
    dispositions = collections.Counter()
    kinds = collections.Counter()
    for assembly in api['assemblies']:
        for index, row in enumerate(assembly['declarations']):
            body = row[2:]
            owner = next((n for n in names if body.startswith(n + ':') or body.startswith(n + '.')), None)
            assert owner, 'Unowned row: ' + row
            symbol = body.split(':', 1)[0]
            kind = row[0]
            label = symbol[len(owner):].lstrip('.')
            full_id = digest((assembly['assembly'] + '\n' + row).encode())
            draft = owner in rules['draftTypes'] or any(token in label for token in rules['draftMemberTokens']) or (owner.endswith('.XdmValueKind') and label == 'JNode')
            schema = any(token in label for token in rules['schemaTokens'])
            if assembly['assembly'] == 'Bosak.LanguageServer':
                disposition = 'OutsideProposedFoundation'
            elif draft:
                disposition = 'DraftMemberReview'
            elif schema or '.Xsd.' in owner or owner.endswith('.XdmElementAssessment'):
                disposition = 'SchemaContractReview'
            elif assembly['assembly'] == 'Bosak.XPath.Core' or owner in rules['queryEntryTypes']:
                disposition = 'FoundationCandidate'
            elif assembly['assembly'] == 'Bosak.XPath.Runtime':
                disposition = 'RuntimeDependencyReview'
            elif assembly['assembly'] == 'Bosak.XPath.Providers':
                disposition = 'ProviderContractReview'
            else:
                disposition = 'CompatibilityWatch'
            evidence_only = kind in ['A', 'L']
            slot = None
            if kind == 'M' and ('Interface' in type_rows[owner].split(':', 2)[1] or 'ClassSemanticsMask' in type_rows[owner].split(':', 2)[1]):
                flags = row.split(':', 2)[1]
                slot = 'Required' if 'Abstract' in flags else 'DefaultImplementation'
                slots[slot] += 1
            linked = {'FC-19'}
            if not evidence_only:
                linked.update(['FC-01', 'FC-02', 'FC-17'])
            if kind == 'F' or 'Optional' in row:
                linked.add('FC-03')
            if kind == 'L' or 'SequentialLayout' in row or owner.endswith(('.XdmValue', '.XdmSequence', '.XdmSequence+Enumerator')):
                linked.add('FC-04')
            if any(token in owner + label for token in rules['nodeTokens']):
                linked.update(['FC-05', 'FC-14'])
            if any(token in owner + label for token in rules['sequenceTokens']):
                linked.update(['FC-07', 'FC-08'])
            if 'SinglePass' in owner + label:
                linked.add('FC-09')
            if any(token in owner + label for token in ['Map', 'Array', 'Function', 'External', 'JNode', 'Record']):
                linked.add('FC-08')
            if 'Compile' in owner or 'EvaluationContext' in owner:
                linked.update(['FC-10', 'FC-16'])
            if 'Resource' in owner + label or 'Resolver' in owner + label or 'Loader' in owner + label:
                linked.update(['FC-11', 'FC-16'])
            if schema or 'TypedValue' in label or 'SchemaTypeName' in label or owner.endswith('.XdmElementAssessment') or '.Xsd.' in owner:
                linked.update(['FC-06', 'FC-12', 'FC-13', 'FC-15'])
            if 'Exception' in owner or 'Error' in owner + label:
                linked.add('FC-15')
            if draft:
                linked.add('FC-20')
            # Public type reachability is deliberately conservative: include mixed/deferred members.
            dependencies = []
            if not evidence_only:
                signature = row.split(':', 1)[1]
                dependencies = sorted(n for n in names if n != owner and refs[n].search(signature))
                for target in dependencies:
                    key = (owner, target)
                    if key not in edges:
                        edges[key] = {'from': owner, 'to': target, 'witnessEntryId': full_id, 'declarationCount': 0}
                    edges[key]['declarationCount'] += 1
            if any(n in rules['draftTypes'] for n in dependencies) and disposition != 'OutsideProposedFoundation':
                disposition = 'DraftMemberReview'
                linked.add('FC-20')
            records.append({'id': full_id, 'assembly': assembly['assembly'], 'baselineRow': index,
                            'type': owner, 'symbol': label or '(type)', 'kind': kind,
                            'reviewGroup': disposition, 'evidenceOnly': evidence_only,
                            'interfaceSlot': slot, 'caseIds': sorted(linked), 'signatureTypes': dependencies})
            dispositions[disposition] += 1
            kinds[kind] += 1
    assert len({r['id'] for r in records}) == len(records), 'Duplicate exact metadata rows'
    seeds = sorted(n for n in names if known[n] == 'Bosak.XPath.Core' or n in rules['queryEntryTypes'])
    adjacency = collections.defaultdict(set)
    for source, target in edges:
        adjacency[source].add(target)
    reachable = set(seeds)
    todo = list(seeds)
    while todo:
        for target in adjacency[todo.pop()]:
            if target not in reachable:
                reachable.add(target)
                todo.append(target)
    manifest = {'formatVersion': 1, 'status': 'Proposed; no owner acceptance or frozen members',
                'freezeAccepted': False, 'globalCaseIds': ['FC-17', 'FC-18', 'FC-19'], 'ownerDispositions': {'Core': 'Pending', 'Schema': 'Pending', 'XmlDb': 'Pending'},
                'baselineSourceRevision': read(BASE / 'baseline.json')['sourceRevision'],
                'inputHashes': {str(p.relative_to(ROOT)).replace('\\', '/'): digest(p.read_bytes()) for p in [BASE / 'public-api.json', HERE / 'scope-rules.json', HERE / 'compatibility-cases.json']},
                'rowIndexConvention': 'Zero-based index in baseline assemblies[].declarations; exact signature and attributes remain in the immutable baseline',
                'summary': {'entries': len(records), 'types': len(names), 'declarationKinds': dict(sorted(kinds.items())),
                            'reviewGroups': dict(sorted(dispositions.items())), 'interfaceSlots': dict(slots),
                            'behavioralCasesExecuted': 0}, 'entries': records}
    closure = {'formatVersion': 1, 'status': 'Conservative observed Bosak public-type reachability; not accepted member-level freeze closure',
               'limitations': ['Includes all declarations of reached types, including deferred/draft members',
                              'Does not prove BCL/external package closure, IL/body dependencies, runtime offsets or behavior',
                              'Field-shape/attribute-only rows are retained in the manifest, not public-signature edges'],
               'seeds': seeds, 'reachableTypes': sorted(reachable),
               'additionalReviewTypes': [{'assembly': known[n], 'type': n} for n in sorted(reachable-set(seeds))],
               'edges': [edges[key] for key in sorted(edges)]}
    used = set(c for r in records for c in r['caseIds'])
    assert used <= {c['id'] for c in cases['cases']}
    return manifest, closure


def encoded(value):
    if 'entries' in value:
        header = json.dumps({k: v for k, v in value.items() if k != 'entries'}, ensure_ascii=False, indent=2)
        rows = ',\n'.join('    ' + json.dumps(e, ensure_ascii=False, separators=(',', ':')) for e in value['entries'])
        return (header[:-2] + ',\n  \"entries\": [\n' + rows + '\n  ]\n}\n').encode('utf-8')
    return (json.dumps(value, ensure_ascii=False, indent=2) + '\n').encode('utf-8')


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check', action='store_true', help='Compare committed drafts without rewriting them')
    options = parser.parse_args()
    manifest, closure = generate()
    for name, value in [('member-manifest.json', manifest), ('signature-closure.json', closure)]:
        path = HERE / 'evidence' / name
        data = encoded(value)
        if options.check:
            assert path.read_bytes() == data, 'Draft drift: ' + name
        else:
            path.write_bytes(data)
    print(json.dumps({'mode': 'check' if options.check else 'generate', **manifest['summary'],
                      'signatureEdges': len(closure['edges']), 'reachableTypes': len(closure['reachableTypes']),
                      'additionalReviewTypes': closure['additionalReviewTypes']}, indent=2))


if __name__ == '__main__':
    main()
