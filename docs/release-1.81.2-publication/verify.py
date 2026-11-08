from pathlib import Path
import hashlib
import io
import json
import zipfile

root = Path(__file__).resolve().parent
for name, entry in json.loads((root / "MANIFEST.json").read_text(encoding="utf-8")).items():
    path = (root / name).resolve()
    assert path.is_relative_to(root), name
    raw = path.read_bytes()
    assert len(raw) == entry["bytes"] and hashlib.sha256(raw).hexdigest() == entry["sha256"], name
nuget = json.loads((root / "NUGET-SERVICE-VERIFICATION.json").read_text(encoding="utf-8"))
assert len(nuget) == 10
with zipfile.ZipFile(root / "NUGET-DELIVERED.zip") as delivered, zipfile.ZipFile(root.parent / "release-linux-collection-hosted/nuget-42479094503.zip") as approved:
    for row in nuget:
        assert row["downloaded"] and row["versionListed"] and row["payloadEntriesMatchApprovedArtifact"]
        name = row["id"] + ".1.81.2.nupkg"
        payload = delivered.read(name)
        assert hashlib.sha256(payload).hexdigest() == row["sha256"]
        with zipfile.ZipFile(io.BytesIO(payload)) as published, zipfile.ZipFile(io.BytesIO(approved.read("nuget/" + name))) as source:
            assert len(published.namelist()) == len(set(published.namelist()))
            assert set(published.namelist()) - set(source.namelist()) <= {".signature.p7s"}
            for entry in source.namelist():
                assert source.read(entry) == published.read(entry), entry
market = json.loads((root / "MARKETPLACE-SERVICE-VERIFICATION.json").read_text(encoding="utf-8"))
assert market["listed"] and market["byteIdentical"]
with zipfile.ZipFile(root / "MARKETPLACE-DELIVERED.zip") as delivered, zipfile.ZipFile(root.parent / "release-linux-collection-hosted/vscode-42479096917.zip") as approved:
    payload = delivered.read("marketplace-vscode-bosak-0.2.6.vsix")
    assert hashlib.sha256(payload).hexdigest() == market["downloadedSha256"]
    assert payload == approved.read("vscode/vscode-bosak-0.2.6.vsix")
for run_id in [42510683197, 42510685414]:
    run = json.loads((root / (str(run_id) + "-run.json")).read_text(encoding="utf-8-sig"))
    assert run["conclusion"] == "success"
    assert run["head_sha"] == "7249d0e01628c47f92b89a3380bc69cf58d70045"
    jobs = json.loads((root / (str(run_id) + "-jobs.json")).read_text(encoding="utf-8-sig"))["jobs"]
    assert any(j["name"] == "publish" and j["conclusion"] == "success" for j in jobs)
assert json.loads((root / "PUBLIC-CONSUMER-RESULTS.json").read_text(encoding="utf-8"))["passed"] == 7
approval = json.loads((root / "APPROVAL.json").read_text(encoding="utf-8"))
assert approval["publicationAuthorized"] and approval["numericalBudgetsAccepted"] is False
print("Verified ten publicly delivered package payloads, exact Marketplace VSIX, publisher runs, fresh public consumer7 and explicit budget deferral.")
