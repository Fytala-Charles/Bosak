"""Structural screening against current pins; never an owner acceptance verifier."""
from pathlib import Path
import argparse,hashlib,importlib.util,json

P=Path(__file__).resolve().parents[1];R=P.parents[1]
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def check_current(data):
    inputs=json.loads((P/'INPUTS.json').read_text())
    module_path=R/'docs/foundation-provider-qualification/tools/receipt_preflight.py'
    assert sha(module_path)==inputs['files'][module_path.relative_to(R).as_posix()]
    spec=importlib.util.spec_from_file_location('historical_receipt_preflight',module_path)
    module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
    pin_path=R/'docs/foundation-captured-identity-production/evidence/SOURCE-AND-BINARY-PINS.json'
    api_path=R/'docs/foundation-captured-identity-production/evidence/public-api-complete.json'
    for path in [pin_path,api_path]:assert sha(path)==inputs['files'][path.relative_to(R).as_posix()]
    return module.check(data,P,json.loads(pin_path.read_text()),sha(pin_path),sha(api_path))
def main():
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('receipt',type=Path);args=parser.parse_args()
    try:result=check_current(json.loads(args.receipt.read_text(encoding='utf-8')))
    except (OSError,ValueError,TypeError,KeyError,AssertionError):
        result=dict(structureReady=False,errors=['Unreadable or mismatched receipt/context'],qualificationAccepted=False,ownerAcceptanceVerified=False)
    print(json.dumps(result,indent=2));return 0 if result['structureReady'] else 2
if __name__=='__main__':raise SystemExit(main())
