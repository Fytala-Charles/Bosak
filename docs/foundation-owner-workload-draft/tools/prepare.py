"""Draft owner workload choices and evidence-derived diagnostic guardrails, without acceptance."""
from pathlib import Path
import hashlib,json,math

P=Path(__file__).resolve().parents[1];R=P.parents[1]
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
inputs=['docs/foundation-owner-repo-review/REPOSITORIES.json',
    'docs/foundation-owner-gap-review/GAPS.json','docs/foundation-package-workload/PROTOCOL.json',
    'docs/foundation-package-workload/evidence/SUMMARY.json','docs/foundation-package-workload/CANDIDATE-PINS.json']
gaps=read(R/inputs[1]);summary=read(R/inputs[3])
choices={
 'PF-01':('Ordinary untyped scalar/axis/map/array queries; first-item and full enumeration separately.',
          'Pin current shipped and candidate capabilities and consumer fixtures; isolate compile/cold/warm execution.'),
 'PF-02':('Schema compile/reuse and supported/foreign authority lookup over owner-selected schemas.',
          'Owner selects actual schema/component sizes and supported/refused profile; compile and lookup budgets separate.'),
 'PF-03':('Start with measured one/eight-child native decimal subset; separately add actual list/union/nil/no-value and preserve/strip/foreign controls.',
          'Decimal subset is real; broader routes need actual supported native carriers and owner qualification before measurement.'),
 'PF-04':('Persistent readers over two immutable store versions; same logical query, cold/warm and first/full results.',
          'Owner supplies original-format store snapshots, expected order/namespaces and read-count/byte observations.'),
 'PF-05':('Partial consumption, early break, close, cancellation, nested/reentrant access and work/cleanup faults on actual snapshot leases.',
          'Record exact deterministic ownership/release events; forced-GC observations cannot replace the release contract.'),
 'PF-06':('Actual XSLT worker preview over ordinary, supported assessed and persistent routes; distinct/shared roots and source-free invocation.',
          'Select typed/persistent worker routing explicitly; byte transport is not live authority. Pin acquisition through IPC/output phase boundaries.'),
 'PF-07':('Repeated unchanged run followed by changed schema/profile/source/store snapshot, with timeout/kill/reopen controls.',
          'Pin worker configuration, run identity and publication currentness checks; no route is approved by a matching digest alone.'),
 'PF-08':('Owner-selected size/depth/width ladder and bounded refusal, cancellation/close/reentry schedules.',
          'Keep current operational input/frame/deadline limits separate from measured latency, allocation and retained-resource budgets.'),
 'PF-09':('Measure callback/dispatch/conversion counts and immutable metadata reuse on selected real workloads.',
          'Do not propose a new batch/capability/scope API until the actual owner route demonstrates the need.'),
}
rows=[]
for original in gaps['scenarios']:
    key=original['id'];workload,prerequisite=choices[key]
    row=dict(id=key,owners=original['owners'],boundary=original['boundary'],metrics=original['metrics'],
        proposedWorkload=workload,requiredInputs=prerequisite,status='DraftForOwnerReview',
        ownerAccepted=False,baseline=None,baselineAccepted=False,ownerBudget=None,
        relativeRegressionTolerance=None,draftBudgets=[])
    if key=='PF-03':
        for measured in summary['workloads']:
            latency=max(measured['repetitionMediansMs']);allocation=max(measured['allocatedBytesPerCycle'])
            row['draftBudgets'].append(dict(fixture=measured['id'],scope='Measured native decimal subset only',
                latency=dict(unit='milliseconds/cycle',statistic='Maximum of three per-session medians over five measured cycles each',
                    observed=latency,headroomMultiplier=2,roundUpQuantum=25,proposedCeiling=math.ceil(2*latency/25)*25),
                allocation=dict(unit='bytes/cycle',statistic='Maximum of three session-average allocations on the measuring thread',
                    observed=allocation,headroomMultiplier=2,roundUpQuantum=65536,proposedCeiling=math.ceil(2*allocation/65536)*65536),
                accepted=False,classification='Provisional diagnostic guardrail; not owner SLA, statistical tolerance or regression acceptance'))
    rows.append(row)
proposal=dict(date='2026-11-07',status='DraftForOwnerReview',previewVersion=summary['previewVersion'],
    candidateCommit=gaps['candidateCommit'],inputs={n:sha(R/n) for n in inputs},scenarios=rows,
    guardrailRationale='Review starting point only: two times largest observed session statistic, rounded upward to 25 ms or 64 KiB. Headroom is a discretionary proposal, not a measured variance bound or product requirement.',
    operationalLimits=dict(owner='Braid',documentBytes=8*1024*1024,frameBytes=64*1024*1024,postLaunchDeadlineSeconds=30,
        scope='Historical product controls from the owner reply, not accepted candidate PF budgets; no bounds on whole-engine/native memory or between-checkpoint time inferred.'),
    ownerAccepted=False,budgetsAccepted=False,apiFrozen=False,publicationAuthorized=False,requirementsWaived=0)
(P/'PROPOSAL.json').write_text(json.dumps(proposal,indent=2)+'\n',encoding='utf-8')
print('Drafted nine workload mappings; two measured decimal fixture guardrails only. No acceptance slots filled.')
