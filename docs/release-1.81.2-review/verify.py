from pathlib import Path
import hashlib
import json

base = Path(__file__).resolve().parent
review = json.loads((base / "REVIEW.json").read_text(encoding="utf-8"))
for pin in review["pins"].values():
    path = (base / pin["path"]).resolve()
    assert path.is_relative_to(base.parent.resolve()), path
    assert hashlib.sha256(path.read_bytes()).hexdigest() == pin["sha256"], path
for key in ["ownerAcceptances", "scenarioBudgetAcceptances", "hostedPreparationRuns"]:
    assert all(value is None for value in review[key].values()), key
for key in ["deferredRequirementDisposition", "apiAcceptance", "signingProvenanceDisposition", "publicationApproval"]:
    assert review[key] is None, key
assert review["messagesSent"] is False
print("Exact review inputs verified; owner/budget/API/signing/hosted/publication decisions remain unfilled.")
