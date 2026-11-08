from pathlib import Path
import json, hashlib, xml.etree.ElementTree as ET, zipfile, tarfile
root = Path(__file__).resolve().parents[1]
evidence = root / "evidence"
ns = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}
def digest(data): return hashlib.sha256(data).hexdigest()
def load(path): return json.loads(path.read_text(encoding="utf-8"))
def text_manifest(path): return {name.removeprefix("./"): value for value, name in (line.split("  ", 1) for line in path.read_text(encoding="utf-8").splitlines())}
manifest = load(root / "MANIFEST.json")
for item in manifest["files"]:
    path = root / item["path"]
    assert path.resolve().is_relative_to(root)
    assert path.is_file() and digest(path.read_bytes()) == item["sha256"], item["path"]
selection = load(evidence / "selection.json")
expected_candidate = {p["id"]: p["assemblySha256"] for p in selection["packages"]}
assert len(expected_candidate) == 7
windows_before = load(evidence / "frozen-layouts-before.json")
expected_baseline = {name: windows_before["control"]["probe"][name + ".dll"] for name in expected_candidate}
roles = ["source-baseline", "source-candidate", "binary-control", "binary-replacement"]
all_ids = None
all_observations = None
for platform, base in [("windows", evidence), ("linux", evidence / "linux"), ("linux-replay", evidence / "linux-replay")]:
    for role in roles:
        x = ET.parse(base / (role + "-tests/results.trx"))
        counters = x.find(".//t:Counters", ns).attrib
        assert int(counters["passed"]) == int(counters["total"]) == 27 and int(counters["failed"]) == int(counters["notExecuted"]) == 0
        ids = {row.get("testId") for row in x.findall(".//t:UnitTestResult", ns)}
        if all_ids is None: all_ids = ids
        assert ids == all_ids
        observations = load(base / (role + "-observations.json"))["observations"]
        if all_observations is None: all_observations = observations
        assert observations == all_observations and len(observations) == 37
        for kind in ["tests", "probe"]:
            if platform == "windows":
                command = load(base / (role + "-" + kind + "-command.json"))["command"]
                expected_root = command[2 if kind == "tests" else 1].replace("\\", "/").rsplit("/", 1)[0].lower()
            else:
                suffix = "tests/Bosak.XmlDb.Xdm.Tests/bin/Release/net10.0" if kind == "tests" else "qualification/Probe/bin/Release/net10.0"
                expected_root = "/work/" + (role.removeprefix("source-") + "/source/" + suffix if role.startswith("source-") else "frozen/" + role.removeprefix("binary-") + "/" + kind)
            found = {}
            for path in (base / (role + "-" + kind + "-bindings")).glob("*.json"):
                report = load(path)
                assert not report["captureErrors"]
                for row in report["assemblies"]:
                    if not row["name"].startswith("Bosak."): continue
                    parent = row["path"].replace("\\", "/").rsplit("/", 1)[0]
                    if platform == "windows": parent = parent.lower()
                    assert parent == expected_root and row["context"] == "Default" and row["collectible"] == "False", (platform, role, kind, row)
                    if row["name"] in expected_candidate:
                        expected = expected_candidate if role in ["source-candidate", "binary-replacement"] else expected_baseline
                        assert row["sha256"] == expected[row["name"]]
                        found[row["name"]] = row["sha256"]
            assert found.keys() == expected_candidate.keys()
        if platform != "windows":
            assert (base / "store-before.txt").read_bytes() == (base / (role + "-store-after.txt")).read_bytes()
            assert (base / "store-dirs-before.txt").read_bytes() == (base / (role + "-store-dirs-after.txt")).read_bytes()
        else:
            assert load(base / "existing-store-before.json") == load(base / (role + "-store-after.json"))
for platform, base in [("windows", evidence), ("linux", evidence / "linux"), ("linux-replay", evidence / "linux-replay")]:
    for kind in ["tests", "probe"]:
        if platform == "windows":
            before = windows_before
            after = load(evidence / "frozen-layouts-after.json")
            control, replacement = before["control"][kind], before["replacement"][kind]
            assert before == after
        else:
            control = text_manifest(base / ("layout-control-" + kind + "-before.txt"))
            replacement = text_manifest(base / ("layout-replacement-" + kind + "-before.txt"))
            assert control == text_manifest(base / ("layout-control-" + kind + "-after.txt"))
            assert replacement == text_manifest(base / ("layout-replacement-" + kind + "-after.txt"))
        assert control.keys() == replacement.keys()
        assert {name for name in control if control[name] != replacement[name]} == {name + ".dll" for name in expected_candidate}
for mode in ["control", "replacement"]:
    with zipfile.ZipFile(evidence / ("windows-frozen-" + mode + ".zip")) as archive:
        stored = {name: digest(archive.read(name)) for name in archive.namelist()}
    for kind in ["tests", "probe"]:
        assert {name.removeprefix(kind + "/"): value for name, value in stored.items() if name.startswith(kind + "/")} == windows_before[mode][kind]
    with tarfile.open(evidence / "linux-replay" / ("frozen-" + mode + ".tar.gz"), "r:gz") as archive:
        stored = {member.name.removeprefix("./"): digest(archive.extractfile(member).read()) for member in archive.getmembers() if member.isfile()}
    for kind in ["tests", "probe"]:
        initial = text_manifest(evidence / "linux" / ("layout-" + mode + "-" + kind + "-before.txt"))
        replay = text_manifest(evidence / "linux-replay" / ("layout-" + mode + "-" + kind + "-before.txt"))
        assert initial == replay
        assert {name.removeprefix(kind + "/"): value for name, value in stored.items() if name.startswith(kind + "/")} == initial
results = load(root / "RESULTS.json")
assert results["coreOwnerDecision"] is results["xmlDbFinalOwnerSignoff"] is results["schemaOwnerDecision"] is None
assert not any(results[key] for key in ["namedStableManifestAccepted", "candidateApiFrozen", "productAdoptionAuthorized", "publicationAuthorized"])
print("PASS: all artifact hashes, primary/replay counts and IDs, 37 observations, actual seven-DLL default/layout bindings, unchanged frozen/store inventories, replay archives and pending owner flags.")
