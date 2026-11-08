const assert = require('node:assert/strict');
const check = require('./source-run.cjs');
const baseline = { conclusion:'success', event:'workflow_dispatch', path:'.github/workflows/release.yml', head_repository:{full_name:'Owner/Repo'}, head_sha:'a'.repeat(40) };
const env = { SOURCE_RUN_ID:'42', EXPECTED_MANIFEST_SHA256:'b'.repeat(64), EXPECTED_WORKFLOW:'.github/workflows/release.yml' };
(async()=>{
  let output;
  const invoke = (run, inputs=env) => check({github:{rest:{actions:{getWorkflowRun:async()=>({data:run})}}},context:{repo:{owner:'Owner',repo:'Repo'}},core:{setOutput:(_k,v)=>{output=v;}},env:inputs});
  await invoke(baseline); assert.equal(output,baseline.head_sha);
  for(const change of [{conclusion:'failure'},{event:'pull_request'},{path:'.github/workflows/other.yml'},{head_repository:{full_name:'Fork/Repo'}},{head_sha:'wrong'}]) await assert.rejects(invoke({...baseline,...change}));
  for(const change of [{SOURCE_RUN_ID:''},{SOURCE_RUN_ID:'1;bad'},{EXPECTED_MANIFEST_SHA256:''},{EXPECTED_MANIFEST_SHA256:'wrong'}]) await assert.rejects(invoke(baseline,{...env,...change}));
  console.log('Preparation-run controls passed: valid source plus nine refusal cases.');
})().catch(error=>{console.error(error);process.exitCode=1;});
