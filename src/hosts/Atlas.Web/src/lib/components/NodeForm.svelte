<!--
  NodeForm: Editable creation form and Markdown preview; parent owns persistence.
  Seeds editable fields once and emits drafts through callbacks; it does not persist data itself.
-->
<script lang="ts" module>
  export type { NodeDraft } from './nodeDraft.types';
</script>

<script lang="ts">
  import type { NodeDraft } from './nodeDraft.types';
  import { untrack } from 'svelte';
  import Breadcrumb, { type BreadcrumbItem } from './Breadcrumb.svelte';
  import MarkdownContent from './MarkdownContent.svelte';
  import type { NodeType } from '../api/nodePage';
  let {
    initialDraft,
    types,
    parentId,
    parentTitle,
    initialTypeId = '',
    breadcrumb = [],
    focusOnMount = false,
    onPreview,
    onSubmit,
    submitting = false,
  }: {
    /** Available node types and their singular/plural display labels. */
    types: NodeType[];
    /**
     * Seed values for editable fields. Captured once when the form is created; later prop changes
     * do not overwrite user edits.
     */
    initialDraft?: NodeDraft;
    /** Parent node ID for child-node creation; omit for root creation. */
    parentId?: string;
    /** Display title for the parent breadcrumb; parentId is the actual association. */
    parentTitle?: string;
    /** Initial node type selection when creating a draft. */
    initialTypeId?: string;
    /** Parent ancestry used above a creation form, before the immediate parent is appended. */
    breadcrumb?: BreadcrumbItem[];
    /**
     * Requests initial focus/scroll placement when the relevant form is rendered in the browser.
     */
    focusOnMount?: boolean;
    /**
     * Callback: Receives the current editable NodeDraft as its fields change; does not save to the
     * API.
     */
    onPreview?: (draft: NodeDraft) => void;
    /**
     * Callback: Receives a NodeDraft when the form submits. Without it, the form is preview-only.
     */
    onSubmit?: (draft: NodeDraft) => void;
    /** Shows the creation-in-progress label and disables the submit button. */
    submitting?: boolean;
  } = $props();
  // Seed editable fields once; later prop updates must not overwrite user edits.
  const startingDraft = untrack(() => initialDraft);
  let title = $state(startingDraft?.title ?? '');
  let shortDescription = $state(startingDraft?.shortDescription ?? '');
  let markdown = $state(startingDraft?.markdown ?? '');
  let typeId = $state(startingDraft?.newType ? 'new' : (startingDraft?.typeId ?? ''));
  let requestedTypeIds = $state<string[]>(startingDraft?.requestedTypeIds ?? []);
  let requestedType = $state('');
  const path = $derived(
    parentId ? [...breadcrumb, { id: parentId, title: parentTitle ?? parentId }] : breadcrumb,
  );
  // Focus only after insertion so the select and its containing form can be measured.
  function focusAndScrollOnMount(control: HTMLSelectElement) {
    if (focusOnMount)
      requestAnimationFrame(() => {
        if (!control.isConnected) return;
        control.focus({ preventScroll: true });
        control.closest('form')?.scrollIntoView({
          block: 'start',
          behavior: matchMedia('(prefers-reduced-motion: reduce)').matches ? 'instant' : 'smooth',
        });
      });
  }
  let typeName = $state(startingDraft?.newType?.name ?? '');
  let typeDescription = $state(startingDraft?.newType?.description ?? '');
  let autoPluralize = $state(startingDraft?.newType?.autoPluralize ?? true);
  const instance = $props.id();
  $effect(() => {
    if (!typeId && types.length) typeId = initialTypeId || types[0].id;
  });
  // Build a normalized snapshot for preview/submission without mutating the caller’s initial draft.
  function draft(): NodeDraft {
    return {
      title: title.trim(),
      shortDescription: shortDescription.trim(),
      markdown,
      typeId: typeId === 'new' ? null : typeId,
      requestedTypeIds,
      parentId,
      ...(typeId === 'new'
        ? { newType: { name: typeName.trim(), description: typeDescription.trim(), autoPluralize } }
        : {}),
    };
  }
  $effect(() => {
    onPreview?.(draft());
  });
</script>

<!--
  Layout: Optional parent context leads into type/title, introduction and Markdown editing;
  preview and contribution requests precede the submit actions.
-->

<form
  class="node-form atlas-panel"
  aria-label={parentId ? 'Create child node' : 'Create root node'}
  onsubmit={(event) => {
    event.preventDefault();
    onSubmit?.(draft());
  }}
>
  {#if path.length}<div class="ribbon-context"><Breadcrumb items={path} /></div>{/if}
  <header>
    <p class="note">{parentId ? 'Add a child node' : 'Create a root node'} · Draft</p>
    <div class="title-row">
      <div class="type-field">
        <label for={`${instance}-type`}>Node type</label><select
          use:focusAndScrollOnMount
          id={`${instance}-type`}
          bind:value={typeId}
          required
          >{#each types as type}<option value={type.id}>{type.name}</option>{/each}<option
            value="new">Define a new node type…</option
          ></select
        >
      </div>
      <div class="title-field">
        <label for={`${instance}-title`}>Title</label><input
          id={`${instance}-title`}
          bind:value={title}
          placeholder="Give your node a title"
          required
          maxlength="450"
        />
      </div>
    </div>
    {#if typeId === 'new'}<fieldset>
        <legend>New node type</legend><label for={`${instance}-type-name`}>Type name</label><input
          id={`${instance}-type-name`}
          bind:value={typeName}
          required
          maxlength="100"
        /><label for={`${instance}-type-description`}>Type description</label><textarea
          id={`${instance}-type-description`}
          bind:value={typeDescription}
          maxlength="500"></textarea><label class="plural-choice"
          ><input type="checkbox" bind:checked={autoPluralize} /> Automatically pluralize this type</label
        >
      </fieldset>{/if}
  </header>
  <!-- Groups the short introduction separately from the full Markdown editor and preview. -->
  <div class="intro">
    <div class="summary">
      <label for={`${instance}-short`}>Short description</label><textarea
        id={`${instance}-short`}
        bind:value={shortDescription}
        placeholder="Introduce your idea…"
        required
        maxlength="280"
        rows="5"
        aria-describedby={`${instance}-remaining`}></textarea><small id={`${instance}-remaining`}
        >{shortDescription.length}/280 characters</small
      >
    </div>
  </div>
  <div class="content-editor">
    <label for={`${instance}-content`}>Content (Markdown)</label><textarea
      id={`${instance}-content`}
      bind:value={markdown}
      placeholder="Develop your node here…"
      required
      maxlength="32000"
      rows="6"></textarea>
    <section aria-label="Content preview"><MarkdownContent text={markdown} /></section>
  </div>
  <!--
    Contribution requests describe desired child types; they are separate from this node’s own
    type.
  -->
  <footer>
    <div class="contributions">
      <label for={`${instance}-requested`}>Requested contributions</label><select
        id={`${instance}-requested`}
        bind:value={requestedType}
        onchange={() => {
          if (requestedType && !requestedTypeIds.includes(requestedType))
            requestedTypeIds = [...requestedTypeIds, requestedType];
          requestedType = '';
        }}
        ><option value="">Add a contribution type…</option>{#each types as type}<option
            value={type.id}
            disabled={requestedTypeIds.includes(type.id)}>{type.pluralName ?? type.name}</option
          >{/each}</select
      >
      {#if requestedTypeIds.length}<ul aria-label="Requested contribution types">
          {#each requestedTypeIds as id}<li>
              {types.find((type) => type.id === id)?.pluralName ??
                types.find((type) => type.id === id)?.name}<button
                type="button"
                aria-label={`Remove ${types.find((type) => type.id === id)?.pluralName ?? types.find((type) => type.id === id)?.name} contribution`}
                onclick={() =>
                  (requestedTypeIds = requestedTypeIds.filter((value) => value !== id))}>×</button
              >
            </li>{/each}
        </ul>{/if}
    </div>
  </footer>
  <div class="actions">
    <button type="submit" disabled={!onSubmit || submitting}
      >{submitting ? 'Creating…' : 'Create node'}</button
    >
  </div>
  {#if !onSubmit}<p class="note">Preview only. Saving is unavailable in this view.</p>{/if}
</form>

<style lang="scss">
  .plural-choice {
    display: flex;
    gap: 0.5rem;
    align-items: center;
    margin-top: 0.5rem;
  }
  .plural-choice input {
    width: auto;
  }
  .node-form {
    max-width: 48rem;
    width: 100%;
    margin-inline: auto;
    padding: 0;
    overflow-wrap: anywhere;
    scroll-margin-top: 1rem;
  }
  .ribbon-context {
    min-width: 0;
    padding: 0.75rem 0 0;
    margin-bottom: var(--atlas-card-padding, 1.25rem);
    --atlas-breadcrumb-bottom-space: 0px;
  }
  header,
  .intro,
  .content-editor,
  footer,
  .actions,
  .node-form > p {
    padding-inline: var(--atlas-card-padding, 1.25rem);
  }
  header {
    padding-top: 0.75rem;
  }
  label {
    display: block;
    font-size: 0.85rem;
    margin-bottom: 0.25rem;
  }
  .title-row {
    display: flex;
    flex-wrap: wrap;
    gap: 0.75rem;
    align-items: end;
  }
  .type-field {
    flex: 1 1 10rem;
    min-width: 0;
  }
  .title-field {
    flex: 3 1 16rem;
    min-width: 0;
  }
  .title-field input {
    font-size: 1.3rem;
    font-weight: 600;
  }
  input,
  textarea,
  select,
  button {
    font: inherit;
    max-width: 100%;
  }
  input,
  textarea,
  select {
    box-sizing: border-box;
    width: 100%;
    padding: 0.6rem;
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    background: var(--atlas-surface);
    color: var(--atlas-text);
  }
  textarea {
    resize: vertical;
  }
  fieldset {
    min-width: 0;
    border: 1px solid var(--atlas-border);
    padding: 0.75rem;
    margin-top: 0.75rem;
  }
  .intro {
    display: flex;
    align-items: start;
    gap: var(--atlas-dial-text-gap, 1rem);
    margin-block: 1rem;
  }
  .summary {
    flex: 1;
    min-width: 0;
  }
  .content-editor {
    min-width: 0;
  }
  section:empty {
    display: none;
  }
  section {
    margin-block: 1rem;
  }
  footer {
    display: flex;
    flex-wrap: wrap;
    gap: 1rem;
    align-items: start;
    border-top: 1px solid var(--atlas-border);
    padding-block: 1rem;
  }
  .contributions {
    flex: 1;
    min-width: 0;
  }
  ul {
    display: flex;
    flex-wrap: wrap;
    list-style: none;
    padding: 0;
    gap: 0.5rem;
  }
  li {
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    padding: 0.25rem 0.5rem;
  }
  li button {
    border: 0;
    background: transparent;
    color: inherit;
    margin-left: 0.5rem;
  }
  .actions {
    padding-bottom: 0.5rem;
  }
  button {
    padding: 0.5rem 0.75rem;
    cursor: pointer;
  }
  button:disabled {
    cursor: default;
  }
  .note,
  small {
    color: var(--atlas-muted);
    font-size: 0.85rem;
  }
  .node-form > p {
    padding-bottom: 1rem;
  }
  :focus-visible {
    outline: 2px solid var(--atlas-focus);
    outline-offset: 2px;
  }
  @media (max-width: 380px) {
    header,
    .intro,
    .content-editor,
    footer,
    .actions,
    .node-form > p {
      padding-inline: var(--atlas-card-padding-narrow, 0.75rem);
    }
  }
</style>
