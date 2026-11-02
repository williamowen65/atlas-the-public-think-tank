<!-- Member-only node reporting. Server validates actor, node eligibility and quotas. -->
<script lang="ts">
  import { apiRequest, ApiRequestError } from '$lib/api/apiRequest';
  let { nodeId }: { nodeId: string } = $props();
  let open = $state(false);
  let reason = $state('');
  let explanation = $state('');
  let saving = $state(false);
  let message = $state('');
  let success = $state(false);
  async function submit(event: SubmitEvent) {
    event.preventDefault();
    if (!reason.trim() || saving) return;
    saving = true;
    message = '';
    try {
      await apiRequest('/atlas-session-api', 'moderation/nodes/' + encodeURIComponent(nodeId) + '/reports',
        'POST', { reason: reason.trim(), explanation: explanation.trim() || null });
      success = true;
      open = false;
      message = 'Report submitted for moderator review.';
    } catch (error) {
      message = error instanceof ApiRequestError && error.status === 401
        ? 'Please sign in to report this node.'
        : error instanceof Error ? error.message : 'Unable to submit report.';
    } finally { saving = false; }
  }
</script>

<div class="report-control">
  {#if success}<p role="status">{message}</p>
  {:else}
    <button type="button" aria-expanded={open} onclick={() => { open = !open; message = ''; }}>Report node</button>
    {#if open}
      <form onsubmit={submit}>
        <label for={'report-reason-' + nodeId}>Reason for reporting</label>
        <input id={'report-reason-' + nodeId} bind:value={reason} maxlength="100" required placeholder="Brief reason" />
        <label for={'report-details-' + nodeId}>Additional details (optional)</label>
        <textarea id={'report-details-' + nodeId} bind:value={explanation} maxlength="2000" rows="3"></textarea>
        <div class="actions">
          <button type="submit" disabled={saving || !reason.trim()}>{saving ? 'Submitting…' : 'Submit report'}</button>
          <button type="button" onclick={() => open = false}>Cancel</button>
        </div>
      </form>
    {/if}
    {#if message}<p role="alert">{message}</p>{/if}
  {/if}
</div>
<style>
  .report-control { margin: 1rem 0; }
  form { max-width: 28rem; padding: 1rem; border: 1px solid var(--atlas-muted); border-radius: .5rem; }
  label { display: block; margin: .6rem 0 .25rem; }
  input, textarea { width: 100%; }
  .actions { display: flex; gap: .5rem; margin-top: .75rem; }
</style>
