<script lang="ts">
  import { onMount, tick } from 'svelte';
  import { editorSession } from '../api/editorSession';
  import { apiRequest, ApiRequestError } from '../api/apiRequest';
  import { readJson, type NodeDetail, type ContentBlock } from '../api/nodePage';
  import MarkdownContent from './MarkdownContent.svelte';
  type Fields = { kind: string; text: string };
  type Editable = { block: ContentBlock; fields: Fields; saved: string };
  let {
    nodeId,
    authorId,
    apiBase = '/atlas-public-api',
    onSaved,
    corner = false,
  }: {
    /** Icon-only control positioned in the parent card's upper-right corner. */
    corner?: boolean;
    nodeId: string;
    authorId?: string;
    apiBase?: string;
    onSaved?: (update: { title: string; shortDescription: string; blocks: ContentBlock[] }) => void;
  } = $props();
  let dialog = $state<HTMLDialogElement>();
  let opened = $state(false);
  let busy = $state(false);
  let loaded = $state(false);
  let error = $state('');
  let notice = $state('');
  let title = $state('');
  let intro = $state('');
  let savedTitle = $state('');
  let savedIntro = $state('');
  let entries = $state<Editable[]>([]);
  let draftAvailable = $state(false);
  let controller: AbortController | undefined;
  let owner = $state<string | null>(null);
  const permitted = $derived(
    !!authorId && $editorSession?.toLowerCase() === authorId.toLowerCase(),
  );
  const dirty = $derived(
    title !== savedTitle ||
      intro !== savedIntro ||
      entries.some((e) => JSON.stringify(e.fields) !== e.saved),
  );
  const draftKey = () => `atlas-node-revision:${owner}:${nodeId}`;
  const signal = () => AbortSignal.any([controller!.signal, AbortSignal.timeout(15000)]);
  function editable(block: ContentBlock): Editable {
    const c = block.content;
    const fields = { kind: block.kind, text: c.markdown ?? '' };
    return { block, fields, saved: JSON.stringify(fields) };
  }
  function publish() {
    onSaved?.({
      title: savedTitle,
      shortDescription: savedIntro,
      blocks: entries.map((e) => e.block),
    });
  }
  async function open() {
    owner = $editorSession;
    opened = true;
    loaded = false;
    busy = true;
    error = '';
    notice = '';
    entries = [];
    controller?.abort();
    controller = new AbortController();
    await tick();
    dialog?.showModal();
    try {
      const [detail, blocks] = await Promise.all([
        readJson<NodeDetail>(apiBase, `nodes/${nodeId}`, signal()),
        readJson<ContentBlock[]>(apiBase, `nodes/${nodeId}/description`, signal()),
      ]);
      if (controller.signal.aborted) return;
      if (detail.authorId?.toLowerCase() !== owner?.toLowerCase())
        throw new Error('Only the node owner can edit this node.');
      title = savedTitle = detail.title;
      intro = savedIntro = detail.shortDescription;
      entries = blocks.map(editable);
      loaded = true;
      try {
        draftAvailable = !!localStorage.getItem(draftKey());
      } catch {
        draftAvailable = false;
      }
    } catch (cause) {
      if (!controller.signal.aborted)
        error = cause instanceof Error ? cause.message : 'Could not load node.';
    } finally {
      busy = false;
    }
  }
  function close() {
    if (
      busy ||
      (dirty && !window.confirm('Discard unsaved edits? Changes already saved remain public.'))
    )
      return;
    controller?.abort();
    dialog?.close();
    opened = false;
    loaded = false;
    entries = [];
  }
  $effect(() => {
    if (opened && (!permitted || $editorSession !== owner)) {
      controller?.abort();
      dialog?.close();
      opened = false;
      loaded = false;
      entries = [];
      title = '';
      intro = '';
    }
  });
  onMount(() => () => controller?.abort());
  async function change(operation: () => Promise<void>) {
    if (busy || !permitted || !loaded) return;
    busy = true;
    error = '';
    notice = '';
    try {
      await operation();
      if (!controller!.signal.aborted) {
        publish();
        notice = 'Saved. This change is public.';
      }
    } catch (cause) {
      if (!controller!.signal.aborted) {
        error = `${cause instanceof Error ? cause.message : 'Save failed.'} Unsaved edits have been kept.`;
        if (cause instanceof ApiRequestError && cause.status === 401)
          window.dispatchEvent(new Event('atlas-session-changed'));
      }
    } finally {
      busy = false;
    }
  }
  const path = () => `nodes/${nodeId}`;
  const write = <T,>(suffix: string, method: string, body?: unknown) =>
    apiRequest<T>('/atlas-session-api', `${path()}${suffix}`, method, body, signal());
  function saveText(field: 'title' | 'short-description') {
    const text = field === 'title' ? title : intro;
    void change(async () => {
      await write(`/${field}`, 'PUT', { text });
      if (field === 'title') savedTitle = text;
      else savedIntro = text;
    });
  }
  function saveBlock(entry: Editable) {
    const fields = { ...entry.fields };
    void change(async () => {
      const block = await write<ContentBlock>(
        `/description/blocks/${entry.block.id}`,
        'PUT',
        fields,
      );
      entry.block = block;
      entry.saved = JSON.stringify(fields);
    });
  }
  function saveDraft() {
    try {
      localStorage.setItem(
        draftKey(),
        JSON.stringify({
          title,
          intro,
          entries: entries.map((e) => ({ id: e.block.id, fields: e.fields })),
        }),
      );
      draftAvailable = true;
      notice =
        'Revision draft saved privately in this browser. Public content is unchanged by this draft save.';
    } catch {
      error = 'Could not save the revision draft in this browser.';
    }
  }
  function restoreDraft() {
    try {
      const draft = JSON.parse(localStorage.getItem(draftKey()) ?? 'null');
      if (
        typeof draft?.title !== 'string' ||
        typeof draft?.intro !== 'string' ||
        !Array.isArray(draft.entries)
      )
        throw new Error();
      title = draft.title;
      intro = draft.intro;
      // Restore only editable fields of blocks that still belong to this node; never overwrite IDs/kinds.
      for (const entry of entries) {
        const found = draft.entries.find((e: { id: string }) => e.id === entry.block.id)?.fields;
        if (entry.block.kind === 'markdown' && typeof found?.text === 'string')
          entry.fields = { ...entry.fields, text: found.text };
      }
      notice = 'Draft restored. Review and save each change to publish it.';
    } catch {
      error = 'This revision draft could not be restored.';
    }
  }
  function deleteDraft() {
    try {
      localStorage.removeItem(draftKey());
      draftAvailable = false;
      notice = 'Private revision draft deleted.';
    } catch {
      error = 'Could not delete the private draft.';
    }
  }
</script>

{#if permitted}
  <button
    class="edit-node"
    class:corner
    type="button"
    aria-label="Edit your node"
    title="Edit your node"
    onclick={() => void open()}><span aria-hidden="true">✎</span>{#if !corner} Edit{/if}</button
  >
{/if}
{#if opened}
  <dialog
    bind:this={dialog}
    aria-label="Edit node"
    oncancel={(event) => {
      event.preventDefault();
      close();
    }}
  >
    <h2>Edit your node</h2>
    <p>
      Each Save publishes that change immediately. Save a private revision draft to continue later
      on this browser.
    </p>
    {#if busy}<p role="status">Working…</p>{/if}
    {#if error}<p role="alert">{error}</p>{/if}
    {#if notice}<p role="status">{notice}</p>{/if}
    {#if loaded}
      <fieldset disabled={busy || !permitted}>
        <label>Title<input bind:value={title} maxlength="450" /></label>
        <button
          type="button"
          disabled={title === savedTitle || !title.trim()}
          onclick={() => saveText('title')}>Save title</button
        >
        <label>Short description<textarea bind:value={intro} maxlength="280"></textarea></label>
        <button
          type="button"
          disabled={intro === savedIntro}
          onclick={() => saveText('short-description')}>Save short description</button
        >
        <h3>Content blocks</h3>
        {#each entries as entry, i (entry.block.id)}
          <section aria-label={`Block ${i + 1}: ${entry.fields.kind}`}>
            <h4>Block {i + 1} · {entry.fields.kind}</h4>
            {#if entry.fields.kind === 'markdown'}
              <label
                >Markdown<textarea bind:value={entry.fields.text} maxlength="32000"
                ></textarea></label
              >
              <details>
                <summary>Preview</summary><MarkdownContent text={entry.fields.text} />
              </details>
            {:else}<p>
                This block is preserved. Only Markdown editing is available in this editor.
              </p>{/if}
            {#if entry.fields.kind === 'markdown'}
              <button
                type="button"
                disabled={JSON.stringify(entry.fields) === entry.saved}
                onclick={() => saveBlock(entry)}>Save block {i + 1}</button
              >
            {/if}
          </section>
        {/each}
        <hr />
        <button type="button" onclick={saveDraft}>Save private revision draft</button>
        {#if draftAvailable}<button type="button" onclick={restoreDraft}
            >Restore private revision draft</button
          ><button type="button" onclick={deleteDraft}>Delete private revision draft</button>{/if}
      </fieldset>
    {:else if !busy}<button type="button" onclick={() => void open()}>Retry loading editor</button
      >{/if}
    <button type="button" disabled={busy} onclick={close}>Close editor</button>
  </dialog>
{/if}

<style lang="scss">
  button,
  input,
  textarea {
    font: inherit;
  }
  button {
    cursor: pointer;
    padding: 0.4rem 0.7rem;
    margin: 0.25rem;
  }
  .edit-node {
    color: var(--atlas-accent);
    background: var(--atlas-surface);
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    font-size: 0.85rem;
  }
  .edit-node.corner {
    position: absolute;
    top: 0;
    right: 0;
    z-index: 2;
    display: grid;
    place-items: center;
    width: 2.75rem;
    height: 2.75rem;
    margin: 0;
    padding: 0;
    border: 0;
    border-left: 1px solid var(--atlas-border);
    border-bottom: 1px solid var(--atlas-border);
    border-radius: 0 var(--atlas-radius) 0 var(--atlas-radius);
    font-size: 1.3rem;

    &:hover {
      background: color-mix(in srgb, var(--atlas-accent) 10%, var(--atlas-surface));
    }

    &:focus-visible {
      outline: 3px solid var(--atlas-focus);
      outline-offset: -4px;
    }
  }
  dialog {
    box-sizing: border-box;
    width: min(48rem, 94vw);
    max-height: 90vh;
    overflow-y: auto;
    padding: 1.5rem;
    color: var(--atlas-text);
    background: var(--atlas-surface);
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
  }
  dialog::backdrop {
    background: rgb(0 0 0 / 0.45);
  }
  fieldset {
    border: 0;
    padding: 0;
    min-width: 0;
  }
  label {
    display: grid;
    gap: 0.4rem;
    margin: 0.8rem 0;
  }
  input,
  textarea {
    box-sizing: border-box;
    width: 100%;
    padding: 0.5rem;
  }
  textarea {
    min-height: 6rem;
    resize: vertical;
  }
  section {
    margin: 1rem 0;
    padding: 1rem;
    border: 1px solid var(--atlas-border);
  }
  [role='alert'] {
    color: #b3261e;
  }
</style>
