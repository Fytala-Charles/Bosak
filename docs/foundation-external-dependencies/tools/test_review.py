"""Test dependency review boundaries using in-memory mutations only."""
import unittest
from verify import HERE, load, validate


class ExternalBoundaryTests(unittest.TestCase):
    def setUp(self):
        self.data = load(HERE / 'evidence/signature-dependencies.json')
        self.boundary = load(HERE / 'evidence/boundary-proposal.json')

    def test_complete_candidate_coverage(self):
        self.assertEqual(validate(self.data, self.boundary), (3506, 176))

    def test_missing_public_declaration_refuses(self):
        self.data['declarations'].pop()
        with self.assertRaises(AssertionError):
            validate(self.data, self.boundary)

    def test_erased_schema_dependency_refuses(self):
        row = next(r for r in self.data['declarations'] if r['signature'].startswith('P Bosak.XPath.Api.CompileOptions.SchemaSet:'))
        row['externalTypes'] = [t for t in row['externalTypes'] if not t['isSchemaType']]
        with self.assertRaisesRegex(AssertionError, 'Missing schema dependency'):
            validate(self.data, self.boundary)

    def test_legacy_surface_cannot_be_silently_reclassified(self):
        entry = next(r for r in self.boundary['entries'] if r['schemaDefinitions'])
        entry['group'] = 'FoundationCandidateWithPlatformContracts'
        with self.assertRaises(AssertionError):
            validate(self.data, self.boundary)

    def test_false_freeze_refuses(self):
        self.boundary['freezeAccepted'] = True
        with self.assertRaises(AssertionError):
            validate(self.data, self.boundary)

    def test_application_packages_do_not_enter_foundation(self):
        self.boundary['declaredProjectGraph']['coreAndApiProjectClosure'].append('Bosak.LanguageServer')
        with self.assertRaises(AssertionError):
            validate(self.data, self.boundary)

    def test_init_only_modifier_dependency_is_captured(self):
        row = next(r for r in self.data['declarations'] if r['signature'].startswith('M Bosak.XPath.Api.CompileOptions.set_SchemaSet:'))
        self.assertIn('System.Runtime.CompilerServices.IsExternalInit', {t['definitionType'] for t in row['externalTypes']})

    def test_non_schema_validator_signatures_are_not_neutral_acceptance(self):
        rows = [r for r in self.data['declarations'] if r['declaringType'] == 'Bosak.XPath.Api.Xsd.IXsdValidator']
        self.assertTrue(rows)
        proposed = {r['entryId']: r for r in self.boundary['entries']}
        self.assertTrue(all(not any(t['isSchemaType'] for t in row['externalTypes']) for row in rows))
        self.assertTrue(all(proposed[row['entryId']]['group'] == 'LegacyValidatorFacadeReview' for row in rows))


if __name__ == '__main__':
    unittest.main()
