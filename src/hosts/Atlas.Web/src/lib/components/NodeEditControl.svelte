<script lang="ts">
  import { onMount, tick } from 'svelte';
  import { editorSession } from '../api/editorSession';
  import { apiRequest, ApiRequestError } from '../api/apiRequest';
  import { readJson, type NodeDetail, type ContentBlock } from '../api/nodePage';
  import { authorBlock, blockRequest, derivedExcerpt, validateAuthorBlocks, type AuthorBlock } from '../api/contentAuthoring';
  import NodeAuthoringCard from './NodeAuthoringCard.svelte';
  import type { NodeCardData } from './node-card-versions/types';

  let { node, apiBase = '/atlas-public-api', onSaved, corner = false, average, totalVotes }: {
    corner?: boolean;
    node: NodeCardData;
    average?: number | null;
    totalVotes?: number;
    apiBase?: string;
    onSaved?: (update: { title: string; shortDescription: string; blocks: ContentBlock[] }) => void;
  } = $props();

  const instance = $props.id();
  const nodeId = $derived(node.id);
  const authorId = $derived(node.authorId);
  let editorRevision = $state(0);
  let dialog = $state<HTMLDialogElement>();
  let opened = $state(false);
  let busy = $state(false);
  let loaded = $state(false);
  let error = $state('');
  let notice = $state('');
  let title = $state('');
  const preview = $derived({ ...node, title: title.trim() || 'Untitled node', isHidden: false });
  let originalTitle = $state('');
  let blocks = $state<AuthorBlock[]>([]);
  let original = $state<ContentBlock[]>([]);
  let savedSnapshot = $state('');
  let draftAvailable = $state(false);
  let controller: AbortController | undefined;
  let owner = $state<string | null>(null);
  const permitted = $derived(!!authorId && $editorSession?.toLowerCase() === authorId.toLowerCase());
  const snapshot = $derived(JSON.stringify({ title, blocks }));
  const dirty = $derived(snapshot !== savedSnapshot);
  const draftKey = () => `atlas-node-revision:${owner}:${nodeId}`;
  const signal = () => AbortSignal.any([controller!.signal, AbortSignal.timeout(15000)]);
  const path = () => `nodes/${nodeId}`;
  const write = <T,>(suffix: string, method: string, body?: unknown) =>
    apiRequest<T>('/atlas-session-api', `${path()}${suffix}`, method, body, signal());

  function setSaved(nextTitle: string, nextBlocks: ContentBlock[]) {
    title = originalTitle = nextTitle;
    original = nextBlocks;
    blocks = nextBlocks.map(authorBlock);
    savedSnapshot = JSON.stringify({ title, blocks });
    editorRevision += 1;
  }

  async function open() {
    owner = $editorSession;
    opened = true;
    loaded = false;
    busy = true;
    error = '';
    notice = '';
    controller?.abort();
    controller = new AbortController();
    await tick();
    dialog?.showModal();
    try {
      const [detail, content] = await Promise.all([
        readJson<NodeDetail>(apiBase, path(), signal()),
        readJson<ContentBlock[]>(apiBase, `${path()}/description`, signal()),
      ]);
      if (controller.signal.aborted) return;
      if (detail.authorId?.toLowerCase() !== owner?.toLowerCase())
        throw new Error('Only the node owner can edit this node.');
      setSaved(detail.title, content);
      loaded = true;
      try { draftAvailable = !!localStorage.getItem(draftKey()); }
      catch { draftAvailable = false; }
    } catch (cause) {
      if (!controller.signal.aborted) error = cause instanceof Error ? cause.message : 'Could not load node.';
    } finally { busy = false; }
  }

  function close() {
    if (busy || (dirty && !window.confirm('Discard unsaved edits? Changes already saved remain public.'))) return;
    controller?.abort();
    dialog?.close();
    opened = false;
    loaded = false;
    blocks = [];
  }

  $effect(() => {
    if (opened && (!permitted || $editorSession !== owner)) {
      controller?.abort();
      dialog?.close();
      opened = false;
      loaded = false;
      blocks = [];
      title = '';
    }
  });
  onMount(() => () => controller?.abort());

  async function save() {
    if (!permitted || busy || !loaded) return;
    if (!title.trim()) { error = 'Enter a node title.'; return; }
    const validation = validateAuthorBlocks(blocks);
    if (validation) { error = validation; return; }
    const desired = blocks.map((block) => ({ ...block }));
    const nextTitle = title.trim();
    busy = true;
    error = '';
    notice = '';
    try {
      if (nextTitle !== originalTitle)
        await write('/title', 'PUT', { text: nextTitle });

      // Do not discard unrecognized blocks. They are reorderable but retain their original payload.
      const remainingIds = new Set(desired.map((block) => block.id).filter(Boolean));
      for (const block of desired) {
        if (!block.id) continue;
        const old = original.find((entry) => entry.id === block.id);
        if (!old || (block.kind !== 'markdown' && block.kind !== 'image')) continue;
        const oldFields = authorBlock(old);
        if (oldFields.text !== block.text || oldFields.url !== block.url || oldFields.caption !== block.caption)
          await write(`/description/blocks/${block.id}`, 'PUT', blockRequest(block));
      }
      for (const block of desired) {
        if (block.id) continue;
        const saved = await write<ContentBlock>('/description/blocks', 'POST', blockRequest(block));
        block.id = saved.id;
      }
      for (const old of original) {
        if (!remainingIds.has(old.id))
          await write(`/description/blocks/${old.id}`, 'DELETE');
      }
      // Sequential moves preserve the author's order and each existing BlockId.
      for (let index = 0; index < desired.length; index++)
        await write(`/description/blocks/${desired[index].id}/position`, 'PUT', { index });

      // Compatibility only: the API/SQL projection still requires a short description.
      // It is derived, never a second author-maintained text field.
      const excerpt = derivedExcerpt(desired, nextTitle);
      await write('/short-description', 'PUT', { text: excerpt });
      const persisted = await readJson<ContentBlock[]>(apiBase, `${path()}/description`, signal());
      setSaved(nextTitle, persisted);
      onSaved?.({ title: nextTitle, shortDescription: excerpt, blocks: persisted });
      notice = 'Changes published. Block order and identities are preserved.';
      try { localStorage.removeItem(draftKey()); draftAvailable = false; } catch { /* private storage optional */ }
    } catch (cause) {
      error = `${cause instanceof Error ? cause.message : 'Save failed.'} Some changes may already be public. Your editor remains open; reload before retrying if necessary.`;
      if (cause instanceof ApiRequestError && cause.status === 401)
        window.dispatchEvent(new Event('atlas-session-changed'));
    } finally { busy = false; }
  }

  function saveDraft() {
    try {
      localStorage.setItem(draftKey(), JSON.stringify({ title, blocks }));
      draftAvailable = true;
      notice = 'Private revision saved in this browser; public content is unchanged.';
    } catch { error = 'Could not save a private revision on this device.'; }
  }
  function restoreDraft() {
    try {
      const draft = JSON.parse(localStorage.getItem(draftKey()) ?? 'null');
      if (typeof draft?.title !== 'string' || !Array.isArray(draft.blocks)) throw new Error();
      // Restore only blocks owned by the current document plus unsaved ones.
      const known = new Set(original.map((block) => block.id));
      const restored = draft.blocks.filter((block: AuthorBlock) =>
        typeof block.key === 'string' && typeof block.kind === 'string' &&
        (!block.id || known.has(block.id)));
      if (!restored.length) throw new Error();
      for (const old of original) {
        if (old.kind !== 'markdown' && old.kind !== 'image' && !restored.some((block: AuthorBlock) => block.id === old.id))
          restored.push(authorBlock(old));
      }
      blocks = restored;
      editorRevision += 1;
      title = draft.title;
      notice = 'Private revision restored. Review before publishing.';
    } catch { error = 'Could not restore this revision.'; }
  }
  function deleteDraft() {
    try { localStorage.removeItem(draftKey()); draftAvailable = false; notice = 'Private revision deleted.'; }
    catch { error = 'Could not delete this private revision.'; }
  }
</script>

{#if permitted}
  <button class="edit-node" class:corner type="button" aria-label="Edit your node" title="Edit your node" onclick={() => void open()}>
    <span aria-hidden="true">✎</span>{#if !corner} Edit{/if}
  </button>
{/if}
{#if opened}
  <dialog bind:this={dialog} aria-label="Edit node" oncancel={(event) => { event.preventDefault(); close(); }}>
    <h2>Edit your node</h2>
    {#if busy}<p role="status">Saving or loading…</p>{/if}
    {#if error}<p role="alert">{error}</p>{/if}
    {#if notice}<p role="status">{notice}</p>{/if}
    {#if loaded}
      <fieldset disabled={busy || !permitted}>
        {#key editorRevision}
          <NodeAuthoringCard node={preview} {blocks} fields={titleField} disabled={busy}
            {average} {totalVotes} onChange={(next) => blocks = next} />
        {/key}
        <div class="actions">
          <button class="primary" type="button" onclick={() => void save()} disabled={!dirty || busy}>Publish changes</button>
          <button type="button" onclick={saveDraft}>Save private revision</button>
          {#if draftAvailable}
            <button type="button" onclick={restoreDraft}>Restore private revision</button>
            <button type="button" onclick={deleteDraft}>Delete private revision</button>
          {/if}
        </div>
      </fieldset>
    {:else if !busy}<button type="button" onclick={() => void open()}>Retry loading editor</button>{/if}
    <button type="button" disabled={busy} onclick={close}>Close editor</button>
  </dialog>
{/if}

{#snippet titleField()}
  <label for={`${instance}-title`}>Title</label>
  <div class="title-row">
    <strong>{node.typeName}:</strong>
    <input id={`${instance}-title`} bind:value={title} maxlength="450" />
  </div>
{/snippet}

<style>
  button, input { font: inherit; }
  button { cursor: pointer; padding: .4rem .7rem; margin: .25rem; }
  .edit-node { color: var(--atlas-accent); background: var(--atlas-surface); border: 1px solid var(--atlas-border); border-radius: var(--atlas-radius); font-size: .85rem; }
  .edit-node.corner { position: absolute; top: 0; right: 0; z-index: 2; display: grid; place-items: center; width: 2.75rem; height: 2.75rem; margin: 0; padding: 0; border: 0; border-left: 1px solid var(--atlas-border); border-bottom: 1px solid var(--atlas-border); border-radius: 0 var(--atlas-radius) 0 var(--atlas-radius); font-size: 1.3rem; }
  .edit-node:focus-visible { outline: 3px solid var(--atlas-focus); outline-offset: -4px; }
  dialog { box-sizing: border-box; width: min(48rem, 96vw); max-height: 94dvh; overflow-y: auto; padding: clamp(.75rem, 3vw, 1.5rem); color: var(--atlas-text); background: var(--atlas-surface); border: 1px solid var(--atlas-border); border-radius: var(--atlas-radius); }
  dialog::backdrop { background: rgb(0 0 0 / .55); }
  .title-row { display: flex; flex-wrap: wrap; align-items: baseline; gap: .4rem; }
  .title-row strong { color: var(--atlas-accent); }
  fieldset { border: 0; padding: 0; min-width: 0; }
  label { display: block; margin: .8rem 0 .3rem; }
  input { flex: 1 1 14rem; min-width: 0; box-sizing: border-box; font-size: 1.3rem; font-weight: 600; padding: .65rem; background: var(--atlas-surface); border: 1px solid var(--atlas-border); border-radius: var(--atlas-radius); color: var(--atlas-text); }
  .actions { display: flex; gap: .5rem; flex-wrap: wrap; margin: 1rem 0; }
  .primary { background: var(--atlas-accent); color: white; border-radius: var(--atlas-radius); }
  [role='alert'] { color: #b3261e; }
</style>
