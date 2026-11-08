"""Parse release YAML and check tool pins and prepare/publish isolation."""
from pathlib import Path
import json, re, yaml
ROOT = Path(__file__).resolve().parents[2]
config = json.loads((ROOT / '.github/release-config.json').read_text())
for file, prefix in [('release.yml', 'nuget'), ('vscode-publish.yml', 'vscode')]:
    path = ROOT / '.github/workflows' / file
    workflow = yaml.load(path.read_text(encoding='utf-8'), Loader=yaml.BaseLoader)
    inputs = workflow['on']['workflow_dispatch']['inputs']
    assert inputs['publish']['type'] == 'boolean' and inputs['publish']['default'] == 'false'
    assert 'source_run_id' in inputs and 'expected_manifest_sha256' in inputs
    prepare, publish = workflow['jobs']['prepare'], workflow['jobs']['publish']
    assert prepare['if'] == "github.event_name != 'workflow_dispatch' || !inputs.publish"
    assert publish['if'] == "github.event_name == 'workflow_dispatch' && inputs.publish"
    assert publish['permissions']['actions'] == 'read'
    assert workflow['defaults']['run']['shell'] == 'bash'
    steps = publish['steps']; text = '\n'.join(x.get('run', '') for x in steps)
    assert 'dotnet build' not in text and 'dotnet pack' not in text and 'dotnet publish' not in text
    assert not re.search(r'vsce@\S+\s+package\b', text)
    assert ' verify ' + prefix + ' ' in text
    source = next(x for x in steps if x.get('id') == 'source')
    assert source['env']['EXPECTED_WORKFLOW'] == '.github/workflows/' + file
    download = next(x for x in steps if x.get('uses', '').startswith('actions/download-artifact@'))
    assert download['with']['run-id'] == '${{ inputs.source_run_id }}'
    assert download['with']['name'] == prefix + '-${{ steps.source.outputs.sha }}'
    assert download['with']['path'] == '${{ runner.temp }}/release-input'
    for step in prepare['steps'] + publish['steps']:
        if step.get('uses', '').startswith('actions/setup-dotnet@'): assert step['with']['dotnet-version'] == config['dotnetSdk']
        if step.get('uses', '').startswith('actions/setup-node@'): assert step['with']['node-version'] == config['nodeVersion']
    if prefix == 'vscode':
        assert '--packagePath' in text
        for step in prepare['steps'] + publish['steps']:
            if 'npx' in step.get('run', ''): assert '@vscode/vsce@' + config['vsceVersion'] in step['run']
        assert publish['steps'][-1]['env']['VSCE_PAT'] == '${{ secrets.VCSE_PAT }}'
    else:
        assert publish['permissions']['id-token'] == 'write'
        assert 'check-gates nuget' in '\n'.join(x.get('run', '') for x in prepare['steps'])
ci = yaml.load((ROOT / '.github/workflows/ci.yml').read_text(encoding='utf-8'), Loader=yaml.BaseLoader)
controls = ci['jobs']['release-recipes']['steps']
assert any(x.get('uses', '').startswith('actions/setup-node@') and x['with']['node-version'] == config['nodeVersion'] for x in controls)
assert 'PyYAML==6.0.3' in '\n'.join(x.get('run', '') for x in controls)
print('Release YAML parsed; tool pins, frozen gates, prior-run provenance, default preparation and exact-artifact publish paths verified.')
