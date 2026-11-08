// Verify artifact provenance before downloading from a preparation run.
module.exports = async ({ github, context, core, env = process.env }) => {
  if (!/^[1-9][0-9]*$/.test(env.SOURCE_RUN_ID || '') || !Number.isSafeInteger(Number(env.SOURCE_RUN_ID))) throw new Error('A valid preparation run ID is required');
  if (!/^[0-9a-f]{64}$/i.test(env.EXPECTED_MANIFEST_SHA256 || '')) throw new Error('The reviewed manifest SHA-256 is required');
  const { data: run } = await github.rest.actions.getWorkflowRun({ ...context.repo, run_id: Number(env.SOURCE_RUN_ID) });
  const repository = `${context.repo.owner}/${context.repo.repo}`.toLowerCase();
  if (run.conclusion !== 'success' || !['push', 'workflow_dispatch'].includes(run.event) || run.path.split('@')[0] !== env.EXPECTED_WORKFLOW || run.head_repository?.full_name.toLowerCase() !== repository || !/^[0-9a-f]{40}$/.test(run.head_sha)) throw new Error('Run must be a successful preparation from this repository and workflow');
  core.setOutput('sha', run.head_sha);
};
