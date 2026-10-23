"""Rebuild the bounded REQ-126 coverage inventory; this does not certify conformance."""
from pathlib import Path
import hashlib
import argparse
import json
import re

ROOT = Path(__file__).resolve().parents[2]
CATALOG = ROOT / "docs/xpath40-catalog"

def read(path):
    return path.read_text(encoding="utf-8-sig")

def section(text, number):
    match = re.search(rf"^## {number}\.[^\n]*\n(.*?)(?=^## \d+\.|\Z)", text, re.M | re.S)
    return match.group(1).strip() if match else ""

def linked_files(text, parent):
    files = []
    for target in re.findall(r"\]\(([^)]+)\)", text):
        if target.startswith(("https:", "http:", "#")):
            continue
        path = (parent / target.split("#")[0]).resolve()
        if path.is_file():
            files.append(path.relative_to(ROOT).as_posix())
    return sorted(set(files))

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--expected-entries", type=int)
parser.add_argument("--expected-cases", type=int)
args = parser.parse_args()

cases = json.loads(read(CATALOG / "examples/cases.json"))
case_ids = {row["id"] for row in cases}
assert len(cases) == len(case_ids)
assert args.expected_cases is None or len(cases) == args.expected_cases
case_pattern = re.compile(r"(?<![A-Za-z0-9_-])(?:" + "|".join(re.escape(x) for x in sorted(case_ids, key=len, reverse=True)) + r")(?![A-Za-z0-9_-])")
runner = read(ROOT / "tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs")
gate_block = runner.split("GatedSets =", 1)[1].split("};", 1)[0]
gates = sorted(set(re.findall(r'"([A-Za-z0-9_-]+)"', gate_block)))
index = read(CATALOG / "README.md")
entries = []
for path in sorted(CATALOG.glob("*/*.md")):
    text = read(path)
    if not re.search(r"^## 1\. Name and Classification", text, re.M):
        continue
    name = path.relative_to(CATALOG).as_posix()
    headings = [int(n) for n in re.findall(r"^## (\d+)\.", text, re.M)]
    assert headings == list(range(1, 17)), name
    assert re.search(r"\]\(" + re.escape(name) + r"(?:#.*?)?\)", index), name
    uses = section(text, 15)
    named_cases = sorted(set(case_pattern.findall(uses)))
    mentioned_gates = [gate for gate in gates if re.search(r"(?<![A-Za-z0-9_-])" + re.escape(gate) + r"(?![A-Za-z0-9_-])", uses)]
    entries.append(dict(entry=name, classification=section(text, 1).split("\n\n")[0], numberedFields=16,
        indexed=True, knownUsesSha256=hashlib.sha256(uses.encode("utf-8")).hexdigest(), exactManifestIdsInKnownUses=named_cases,
        mentionedCurrentGatedSets=mentioned_gates,
        knownUseLocalFiles=linked_files(uses, path.parent),
        sampleLocalFiles=linked_files(section(text, 13), path.parent),
        hasLanguageAlignment=bool(section(text, 6)),
        evidenceInterpretation="Literal citations only; set membership and matching IDs do not certify per-contract coverage, drift correctness or lifecycle completeness."))
assert args.expected_entries is None or len(entries) == args.expected_entries
provenance = json.loads(read(CATALOG / "provenance.json"))
missing_pins = [row["entry"] for row in provenance["entries"] if not row.get("adoptedSpecRevision")]
assert len(missing_pins) == 28
summary = dict(entries=len(entries), featureChecks=len(cases), chapterChecks=24, workedChapters=4,
    totalDocumentationChecks=len(cases)+24, currentGatedSetCount=len(gates),
    entriesMentioningCurrentGatedSet=sum(bool(row["mentionedCurrentGatedSets"]) for row in entries),
    entriesWithoutLiteralCurrentGatedSet=sum(not row["mentionedCurrentGatedSets"] for row in entries),
    entriesWithExactManifestIdsInKnownUses=sum(bool(row["exactManifestIdsInKnownUses"]) for row in entries),
    historicalAdoptedSpecPinsMissing=len(missing_pins),
    countPolicy="Chapter counts are the retained delivery totals; final TRX reconciliation separately verifies documentation test execution. Gate mentions are not proof of full coverage.")
matrix = json.loads(read(CATALOG / "GRAMMAR-COVERAGE-MATRIX.json"))
for item in matrix["rows"]:
    for entry in item["entries"]:
        assert (CATALOG / entry).is_file(), entry
    for source in item["source"]:
        assert (ROOT / source).is_file(), source
    for test in item["testEvidence"]:
        text = read(ROOT / test["file"])
        assert not test["method"] or test["method"] + "(" in text, test
assert len(matrix["rows"]) == 18
summary["mappedGrammarClusterRows"] = len(matrix["rows"])
receipt = dict(date="2026-10-23", sourceBase="ff2a9cd7228282acf5d6dbff00bd9d097c891aeb",
    sourceVersion="1.23.7", publishedVersion="1.23.7", scope="Repository evidence reconciliation; owner-approved AC-03 evidence-policy amendment; record and numeric lexer corrections are audited separately; no tier/gate promotion; publication tracked separately",
    summary=summary, entries=entries, currentGatedSets=gates, historicalAdoptedSpecPinsMissing=missing_pins,
    inputHashes={str(path.relative_to(ROOT)): hashlib.sha256(path.read_bytes()).hexdigest() for path in [CATALOG/"examples/cases.json", CATALOG/"provenance.json", ROOT/"tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs"]},
    ac03OwnerDecision=dict(date="2026-10-22", choice="Adopt applicable-evidence policy (recommended)", criterionAmended=True, criterionClosed=False),
    ac03PolicyStatus="Owner approved applicable-evidence policy on 2026-10-22; registry criterion amended. AC-03 remains partial pending entry-by-entry evidence validation.")
(CATALOG / "COVERAGE-RECONCILIATION.json").write_text(json.dumps(receipt, indent=2, ensure_ascii=True)+"\n", encoding="utf-8")
print(json.dumps(summary, indent=2))
