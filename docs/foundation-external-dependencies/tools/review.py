"""Build/check a proposed external-dependency boundary without changing production source."""
import argparse
import hashlib
import json
from pathlib import Path
import xml.etree.ElementTree as ET

HERE = Path(__file__).resolve().parents[1]
ROOT = HERE.parents[1]


def load(path):
    return json.loads(path.read_bytes())


def proposal():
    data = load(HERE / 'evidence/signature-dependencies.json')
    closure = set(load(ROOT / 'docs/foundation-api-contract-draft/evidence/signature-closure.json')['reachableTypes'])
    previous = {e['id']: e for e in load(ROOT / 'docs/foundation-api-contract-draft/evidence/member-manifest.json')['entries']}
    definitions = {}
    entries = []
    for row in data['declarations']:
        refs = set()
        schema = set()
        for typ in row['externalTypes']:
            key = typ['definitionType'] + '@' + typ['resolvedAssemblyIdentity']
            reference = hashlib.sha256(key.encode()).hexdigest()
            refs.add(reference)
            if typ['isSchemaType']:
                schema.add(typ['definitionType'])
            definition = definitions.setdefault(reference, {'id': reference, 'type': typ['definitionType'], 'resolvedAssembly': typ['resolvedAssembly'], 'resolvedAssemblyIdentity': typ['resolvedAssemblyIdentity'], 'isSchemaType': typ['isSchemaType'], 'ownerAssemblies': set(), 'usedByDeclarations': set(), 'usedWithinProposedTypeClosure': False})
            definition['ownerAssemblies'].add(row['assembly'])
            definition['usedByDeclarations'].add(row['entryId'])
            definition['usedWithinProposedTypeClosure'] |= row['declaringType'] in closure
        if row['assembly'] == 'Bosak.LanguageServer':
            group = 'ApplicationOutsideProposedFoundation'
        elif schema:
            group = 'LegacySchemaCompatibilityReview'
        elif '.Xsd.' in row['declaringType']:
            group = 'LegacyValidatorFacadeReview'
        elif row['declaringType'].endswith('.XdmElementAssessment'):
            group = 'BoundedAssessmentBridgeReview'
        elif previous[row['entryId']]['reviewGroup'] == 'DraftMemberReview':
            group = 'DraftMemberReview'
        elif row['declaringType'] in closure:
            group = 'FoundationCandidateWithPlatformContracts'
        elif row['assembly'] == 'Bosak.XPath.Providers':
            group = 'XmlProviderAdapterReview'
        else:
            group = 'ExistingCompatibilityWatch'
        entries.append({'entryId': row['entryId'], 'group': group, 'inProposedTypeClosure': row['declaringType'] in closure, 'externalDefinitionIds': sorted(refs), 'schemaDefinitions': sorted(schema)})
    for definition in definitions.values():
        definition['ownerAssemblies'] = sorted(definition['ownerAssemblies'])
        definition['usedByDeclarations'] = sorted(definition['usedByDeclarations'])
    projects = []
    for path in sorted((ROOT / 'src').glob('*/*.csproj')):
        tree = ET.parse(path)
        projects.append({'project': path.stem, 'path': path.relative_to(ROOT).as_posix(),
                         'sha256': hashlib.sha256(path.read_bytes()).hexdigest(),
                         'declaredProjectReferences': sorted(Path(r.get('Include').replace('\\', '/')).stem for r in tree.findall('.//ProjectReference')),
                         'declaredPackageReferences': [{'id': p.get('Include'), 'version': p.get('Version')} for p in tree.findall('.//PackageReference')]})
    refs_by_project = {p['project']: p['declaredProjectReferences'] for p in projects}
    reached_projects = {'Bosak.XPath.Core', 'Bosak.XPath.Api'}
    while True:
        expanded = reached_projects | {target for source in reached_projects for target in refs_by_project[source]}
        if expanded == reached_projects:
            break
        reached_projects = expanded
    groups = {g: sum(e['group'] == g for e in entries) for g in sorted({e['group'] for e in entries})}
    return {'formatVersion': 1, 'status': 'Proposed review boundary; no removal, deprecation, API acceptance or freeze', 'freezeAccepted': False,
            'ownerDispositions': {'Core': 'Pending', 'Schema': 'Pending', 'XmlDb': 'Pending'},
            'inputSha256': hashlib.sha256((HERE / 'evidence/signature-dependencies.json').read_bytes()).hexdigest(),
            'summary': {'publicDeclarations': len(entries), 'externalDefinitions': len(definitions), 'resolvedAssemblies': len({d['resolvedAssembly'] for d in definitions.values()}), 'schemaDeclarations': sum(bool(e['schemaDefinitions']) for e in entries), 'schemaDeclarationsWithinProposedTypeClosure': sum(bool(e['schemaDefinitions']) and e['inProposedTypeClosure'] for e in entries), 'externalDefinitionsWithinProposedTypeClosure': sum(d['usedWithinProposedTypeClosure'] for d in definitions.values()), 'reviewGroups': groups},
            'externalDefinitions': sorted(definitions.values(), key=lambda d: (d['resolvedAssembly'], d['type'])),
            'entries': entries,
            'declaredProjectGraph': {'scope': 'Declared csproj references only, not evaluated MSBuild/assets or published package graph', 'projects': projects, 'coreAndApiProjectClosure': sorted(reached_projects)}}


def encoded(value):
    return (json.dumps(value, indent=2) + '\n').encode()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check', action='store_true')
    args = parser.parse_args()
    result = proposal()
    path = HERE / 'evidence/boundary-proposal.json'
    if args.check:
        assert path.read_bytes() == encoded(result), 'Boundary proposal drift'
    else:
        path.write_bytes(encoded(result))
    print(json.dumps(result['summary'], indent=2))


if __name__ == '__main__':
    main()
