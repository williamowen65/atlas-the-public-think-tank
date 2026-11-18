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
    <button class="report-trigger" type="button" aria-expanded={open} onclick={() => { open = !open; message = ''; }}>Report node</button>
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
  .report-control {
    display: flex;
    flex-direction: column;
    align-items: flex-end;
    gap: 0.5rem;
    margin-block: 0.5rem;
  }
  .report-trigger {
    padding: 0.25rem 0;
    border: 0;
    background: transparent;
    color: var(--atlas-danger);
    font: inherit;
    font-size: var(--atlas-node-meta-size);
    line-height: var(--atlas-node-meta-line-height);
    cursor: pointer;
  }
  .report-trigger:focus-visible {
    outline: 2px solid var(--atlas-focus);
    outline-offset: 2px;
  }
  form {
    display: grid;
    gap: 0.375rem;
    inline-size: min(100%, 28rem);
    padding: 1rem;
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    background: var(--atlas-bg);
  }
  label {
    font-weight: 600;
    overflow-wrap: anywhere;
  }
  input + label { margin-block-start: 0.5rem; }
  input, textarea {
    inline-size: 100%;
    min-inline-size: 0;
    min-block-size: 2.75rem;
    padding: 0.55rem 0.7rem;
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    background: var(--atlas-surface);
    color: var(--atlas-text);
    font: inherit;
  }
  input::placeholder { color: var(--atlas-muted); }
  textarea { resize: vertical; }
  .actions {
    display: flex;
    flex-wrap: wrap;
    gap: 0.5rem;
    margin-block-start: 0.5rem;
  }
  .actions button {
    min-block-size: 2.25rem;
    padding: 0.25rem 0.65rem;
    line-height: 1.25;
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    background: var(--atlas-surface);
    color: var(--atlas-text);
    font: inherit;
    font-weight: 600;
    cursor: pointer;
  }
  .actions button[type='submit'] {
    border-color: var(--atlas-accent);
    background: var(--atlas-accent);
    color: var(--atlas-on-accent);
  }
  .actions button:disabled {
    opacity: 0.55;
    cursor: default;
  }
  @media (max-width: 560px) {
    form { inline-size: 100%; }
  }
</style>
