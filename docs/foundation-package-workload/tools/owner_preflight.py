"""Check package-cohort receipt structure; never verify owner identity or acceptance."""
from pathlib import Path
import argparse,hashlib,importlib.util,json

P=Path(__file__).resolve().parents[1];R=P.parents[1]
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def check_current(data):
    context=json.loads((P/'OWNER-CONTEXT.json').read_text())
    module_path=R/'docs/foundation-provider-qualification/tools/receipt_preflight.py'
    assert sha(module_path)==context['checkerSha256']
    pin_path=P/'CANDIDATE-PINS.json';assert sha(pin_path)==context['candidatePinsSha256']
    api_path=R/'docs/foundation-preview-qualification/evidence/public-api.json';assert sha(api_path)==context['apiSha256']
    spec=importlib.util.spec_from_file_location('package_receipt_structure',module_path)
    module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
    return module.check(data,P,json.loads(pin_path.read_text()),context['candidatePinsSha256'],context['apiSha256'])
def main():
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('receipt',type=Path);args=parser.parse_args()
    try:result=check_current(json.loads(args.receipt.read_text(encoding='utf-8')))
    except (OSError,ValueError,TypeError,KeyError,AssertionError):
        result=dict(structureReady=False,errors=['Unreadable or mismatched package receipt/context'],qualificationAccepted=False,ownerAcceptanceVerified=False)
    print(json.dumps(result,indent=2));return 0 if result['structureReady'] else 2
if __name__=='__main__':raise SystemExit(main())
