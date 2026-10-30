"""Independently validate external signature coverage and proposed compatibility boundaries."""
import hashlib
import json
from pathlib import Path
import re

HERE = Path(__file__).resolve().parents[1]
ROOT = HERE.parents[1]


def load(path):
    return json.loads(path.read_bytes())


def validate(data, boundary):
    old = load(ROOT / 'docs/foundation-api-baseline/evidence/public-api.json')
    expected = {hashlib.sha256((a['assembly'] + '\n' + row).encode()).hexdigest(): row for a in old['assemblies'] for row in a['declarations'] if row[0] in 'TMFPE'}
    rows = data['declarations']
    assert len(rows) == len(expected) == 3506
    assert len({r['entryId'] for r in rows}) == len(rows)
    assert {r['entryId'] for r in rows} == set(expected), 'Incomplete public declarations'
    assert boundary['freezeAccepted'] is False
    assert set(boundary['ownerDispositions'].values()) == {'Pending'}
    assert len(boundary['entries']) == len(rows)
    members = {r['entryId']: r for r in boundary['entries']}
    definitions = {d['id']: d for d in boundary['externalDefinitions']}
    assert len(definitions) == len(boundary['externalDefinitions'])
    for row in rows:
        assert row['signature'] == expected[row['entryId']], 'Signature drift'
        observed_schema = {t['definitionType'] for t in row['externalTypes'] if t['isSchemaType']}
        direct_schema = set(re.findall(r'System\.Xml\.Schema\.[A-Za-z0-9_]+', row['signature']))
        assert direct_schema <= observed_schema, 'Missing schema dependency'
        ids = set()
        for typ in row['externalTypes']:
            assert typ['isSchemaType'] == typ['definitionType'].startswith('System.Xml.Schema.')
            reference = hashlib.sha256((typ['definitionType'] + '@' + typ['resolvedAssemblyIdentity']).encode()).hexdigest()
            assert reference in definitions
            definition = definitions[reference]
            assert row['entryId'] in definition['usedByDeclarations']
            assert row['assembly'] in definition['ownerAssemblies']
            ids.add(reference)
        member = members[row['entryId']]
        assert set(member['externalDefinitionIds']) == ids
        assert set(member['schemaDefinitions']) == observed_schema
        if row['assembly'] == 'Bosak.LanguageServer':
            assert member['group'] == 'ApplicationOutsideProposedFoundation'
        elif observed_schema:
            assert member['group'] == 'LegacySchemaCompatibilityReview'
        elif '.Xsd.' in row['declaringType']:
            assert member['group'] == 'LegacyValidatorFacadeReview'
    third_party = [d for d in definitions.values() if not d['resolvedAssembly'].startswith('System.')]
    assert {d['resolvedAssembly'] for d in third_party} == {'MediatR', 'OmniSharp.Extensions.JsonRpc', 'OmniSharp.Extensions.LanguageProtocol'}
    assert all(d['ownerAssemblies'] == ['Bosak.LanguageServer'] and not d['usedWithinProposedTypeClosure'] for d in third_party)
    assert 'Bosak.LanguageServer' not in boundary['declaredProjectGraph']['coreAndApiProjectClosure']
    assert all(p['declaredPackageReferences'] == [] for p in boundary['declaredProjectGraph']['projects'] if p['project'] in boundary['declaredProjectGraph']['coreAndApiProjectClosure'])
    baseline_artifacts = load(ROOT / 'docs/streaming-completion-cleanup/evidence/api-comparison.json')['assemblies']
    hashes = {a['assembly']: a['candidateSha256'].lower() for a in baseline_artifacts}
    assert len(data['observedAssemblies']) == len(hashes) == 11
    assert all(a['sha256'] == hashes[a['assembly']] for a in data['observedAssemblies']), 'Compiled artifact differs from qualified candidate'
    assert sum(bool(r['schemaDefinitions']) for r in boundary['entries']) == boundary['summary']['schemaDeclarations'] == 26
    assert sum(bool(r['schemaDefinitions']) and r['inProposedTypeClosure'] for r in boundary['entries']) == boundary['summary']['schemaDeclarationsWithinProposedTypeClosure'] == 9
    return len(rows), len(definitions)


if __name__ == '__main__':
    print('Verified external signature boundary:', validate(load(HERE / 'evidence/signature-dependencies.json'), load(HERE / 'evidence/boundary-proposal.json')))
