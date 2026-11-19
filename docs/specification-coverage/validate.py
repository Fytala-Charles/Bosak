"""Validate a scoped coverage inventory without launching an engine or claiming conformance."""
# Copyright 2026 Fytala. SPDX-License-Identifier: Apache-2.0
import argparse
import hashlib
import json
from pathlib import Path
import subprocess
import xml.etree.ElementTree as ET


def require(value, message):
    if not value:
        raise ValueError(message)


def check(matrix, catalog):
    require(matrix['inventoryExhaustive'] is False, 'Initial inventory is not an exhaustive requirement set')
    require(matrix['normativeDenominator'] is None and matrix['conformancePercentage'] is None,
            'No unreviewed denominator or completion percentage')
    ids = [row['id'] for row in matrix['rows']]
    require(len(ids) == len(set(ids)) == 24, 'Expected 24 distinct reviewed feature areas')
    cases = {row['caseId'] for row in catalog['cases']}
    require(len(cases) == len(catalog['cases']) == catalog['count'] == matrix['testCatalogCount'] == 365,
            'Historical case catalog changed')
    for row in matrix['rows']:
        require(row['schemaNative'] in ['bounded', 'unsupported', 'not-qualified'], 'Unknown native status')
        require(row['legacyRoute'] in ['partial-helper', 'descriptor-only', 'not-qualified'], 'Unknown legacy status')
        require(row['evidenceLevel'] in ['refusal-only','bounded-runtime','catalog-needs-case-audit',
                'legacy-helper-only','descriptor-only','unassessed'], 'Unknown evidence level')
        require(row['specification'] in matrix['specificationSources'] and row['sections'], 'Missing specification reference')
        require(row['fullyConformant'] is False and not row['normativeTestCaseIds'],
                'Normative qualification requires a separately reviewed case mapping')
        require(set(row['recordedCatalogReferences']) <= cases, 'Unknown historical test ID')
        if row['evidenceLevel'] in ['refusal-only','descriptor-only']:
            require(row['schemaNative'] != 'bounded', 'Refusal/metadata is not positive native support')
    return len(ids), len(cases)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--core-root', required=True, type=Path)
    parser.add_argument('--schema-root', required=True, type=Path)
    parser.add_argument('--matrix', type=Path, help='Explicit review candidate; skips peer byte equality only')
    args = parser.parse_args()
    relative = Path('docs/specification-coverage')
    roots = {'core': args.core_root, 'schema': args.schema_root}
    for name in ['PROGRAM.md','MATRIX.json','CATALOG.json','validate.py']:
        if args.matrix is not None and name == 'MATRIX.json':
            continue
        require((args.core_root / relative / name).read_bytes() == (args.schema_root / relative / name).read_bytes(),
                'Shared peer content drift: ' + name)
    matrix = json.loads((args.matrix or args.schema_root / relative / 'MATRIX.json').read_text(encoding='utf-8-sig'))
    catalog = json.loads((args.schema_root / relative / 'CATALOG.json').read_text(encoding='utf-8-sig'))
    areas, cases = check(matrix, catalog)
    receipts = {}
    for repository, root in roots.items():
        baseline = json.loads((root / relative / 'BASELINE.json').read_text(encoding='utf-8-sig'))
        for item in baseline['sourceInputs']:
            raw = subprocess.check_output(['git', '-C', str(roots[item['repository']]), 'show', item['revision'] + ':' + item['path']])
            require(hashlib.sha256(raw).hexdigest() == item['gitBlobSha256'], 'Immutable source input drift')
        require(baseline['baselines'] == matrix['baselines'], 'Baseline identities disagree')
    for record in catalog['cases']:
        key = record['receipt']
        if key not in receipts:
            raw = subprocess.check_output(['git','-C',str(args.schema_root),'show',record['revision'] + ':' + key])
            tree = ET.fromstring(raw)
            receipts[key] = {r.attrib['testName'] for r in tree.findall('.//{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}UnitTestResult') if r.attrib['outcome'] == 'Passed'}
        require(record['caseId'] in receipts[key], 'Case is not passed in its immutable receipt')
    require(set.union(*receipts.values()) == {r['caseId'] for r in catalog['cases']}, 'Catalog omits or invents historical cases')
    print(json.dumps({'areas': areas, 'historicalCaseIds': cases, 'freshRuntimeTests': False,
                      'normativeCoveragePercentage': None, 'peerAndSourceIdentityVerified': True}))


if __name__ == '__main__':
    try:
        main()
    except (ValueError, KeyError, OSError, subprocess.CalledProcessError, ET.ParseError) as error:
        raise SystemExit('Coverage inventory refused: ' + str(error))
