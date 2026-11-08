from pathlib import Path
import hashlib
import json
import zipfile

base = Path(__file__).resolve().parent
manifest = json.loads((base / "MANIFEST.json").read_text(encoding="utf-8"))
for name, expected in manifest.items():
    path = (base / name).resolve()
    assert path.is_relative_to(base), name
    raw = path.read_bytes()
    assert len(raw) == expected["bytes"], name
    assert hashlib.sha256(raw).hexdigest() == expected["sha256"], name
results = json.loads((base / "RESULTS.json").read_text(encoding="utf-8"))
for item in results["artifacts"]:
    with zipfile.ZipFile(base / item["archive"]) as archive:
        raw = archive.read(item["manifestPath"])
        assert hashlib.sha256(raw).hexdigest() == item["manifestSha256"]
        cohort = json.loads(raw)
        assert cohort["sourceCommit"] == results["sourceCommit"]
        assert cohort["coreVersion"] == "1.81.2"
        assert cohort["extensionVersion"] == "0.2.6"
        prefix = item["manifestPath"].rsplit("/", 1)[0] + "/"
        payloads = {name for name in archive.namelist() if name.startswith(prefix) and not name.endswith("/")}
        assert payloads == {prefix + item["file"] for item in cohort["artifacts"]} | {item["manifestPath"]}
        assert len(cohort["artifacts"]) == (10 if item["kind"] == "nuget" else 1)
        for artifact in cohort["artifacts"]:
            payload = archive.read(prefix + artifact["file"])
            assert len(payload) == artifact["bytes"]
            assert hashlib.sha256(payload).hexdigest() == artifact["sha256"]
    run = json.loads((base / (str(item["runId"]) + "-run.json")).read_text(encoding="utf-8-sig"))
    assert run["conclusion"] == "success"
    assert run["head_sha"] == results["sourceCommit"]
    assert run["event"] == "workflow_dispatch"
    jobs = json.loads((base / (str(item["runId"]) + "-jobs.json")).read_text(encoding="utf-8-sig"))["jobs"]
    assert any(job["name"] == "publish" and job["conclusion"] == "skipped" for job in jobs)
assert results["solutionPassed"] == 26345
assert results["serverPassed"] == 90
assert results["publicationPerformed"] is False
print("Verified both hosted artifact sets, successful producer identities, retained receipts and skipped publication.")
