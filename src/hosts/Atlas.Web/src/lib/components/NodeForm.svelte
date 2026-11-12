<!--
  @component
  NodeForm: Editable creation form and Markdown preview; parent owns persistence.
  Seeds editable fields once and emits drafts through callbacks; it does not persist data itself.

  **Props**
  - `types` (`NodeType[]`; required): Available node types and their singular/plural display labels.
  - `initialDraft` (`NodeDraft`; optional): Seed values for editable fields. Captured once when the
    form is created; later prop changes do not overwrite user edits.
  - `parentId` (`string`; optional): Parent node ID for child-node creation; omit for root creation.
  - `parentTitle` (`string`; optional): Display title for the parent breadcrumb; parentId is the
    actual association.
  - `initialTypeId` (`string`; optional; default `''`): Initial node type selection when creating a
    draft.
  - `breadcrumb` (`BreadcrumbItem[]`; optional; default `[]`): Parent ancestry used above a creation
    form, before the immediate parent is appended.
  - `focusOnMount` (`boolean`; optional; default `false`): Requests initial focus/scroll placement
    when the relevant form is rendered in the browser.
  - `onPreview` (`(draft: NodeDraft) => void`; optional): Callback: Receives the current editable
    NodeDraft as its fields change; does not save to the API.
  - `onSubmit` (`(draft: NodeDraft) => void`; required): Receives the draft when the form submits; the parent owns persistence.
  - `submitting` (`boolean`; optional; default `false`): Shows the creation-in-progress label and
    disables the submit button.
-->
<script lang="ts" module>
  export type { NodeDraft } from './nodeDraft.types';
</script>

<script lang="ts">
  import type { NodeDraft } from './nodeDraft.types';
  import { untrack } from 'svelte';
  import type { BreadcrumbItem } from './Breadcrumb.svelte';
  import NodeAuthoringCard from './NodeAuthoringCard.svelte';
  import type { NodeCardData } from './node-card-versions/types';
  import { derivedExcerpt, newAuthorBlock, validateAuthorBlocks, type AuthorBlock } from '../api/contentAuthoring';
  import type { NodeType } from '../api/nodePage';
  let {
    initialDraft,
    communities = [],
    previewReactions = [],
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
    communities?: { id: string; name: string }[];
    previewReactions?: NodeCardData['reactions'];
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
     * Receives the draft when the form submits; the parent owns persistence.
     */
    onSubmit: (draft: NodeDraft) => void;
    /** Shows the creation-in-progress label and disables the submit button. */
    submitting?: boolean;
  } = $props();
  // Seed editable fields once; later prop updates must not overwrite user edits.
  const startingDraft = untrack(() => initialDraft);
  /**
   * Working node title seeded from the initial draft.
   */
  let title = $state(startingDraft?.title ?? '');
  /**
   * Ordered editor blocks are the node body; no separately authored introduction.
   */
  let blocks = $state<AuthorBlock[]>(
    startingDraft?.blocks?.length
      ? startingDraft.blocks
      : [newAuthorBlock('markdown', startingDraft?.markdown ?? '')],
  );
  let blockError = $state('');
  /**
   * Selected catalog type ID, or new for custom-type creation.
   */
  let typeId = $state(startingDraft?.newType ? 'new' : (startingDraft?.typeId ?? ''));
  /**
   * Selected catalog IDs requested as contributions to this node.
   */
  let requestedTypeIds = $state<string[]>(startingDraft?.requestedTypeIds ?? []);
  /**
   * Current contribution-type picker value, reset after adding a choice.
   */
  let requestedType = $state('');
  let communityIds = $state<string[]>(startingDraft?.communityIds ?? []);
  /**
   * Parent ancestry plus the immediate parent, used by the child-draft breadcrumb.
   */
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
  /**
   * Working name for a new custom node type.
   */
  let typeName = $state(startingDraft?.newType?.name ?? '');
  /**
   * Working description for a new custom node type.
   */
  let typeDescription = $state(startingDraft?.newType?.description ?? '');
  /**
   * Whether the new custom type should use automatic pluralization.
   */
  let autoPluralize = $state(startingDraft?.newType?.autoPluralize ?? true);
  const instance = $props.id();
  $effect(() => {
    if (!typeId && types.length) typeId = initialTypeId || types[0].id;
  });
  const newTypeOption = 'Define a new node type…';
  const selectedTypeName = $derived(types.find((type) => type.id === typeId)?.name);
  const selectedTypeLabel = $derived(typeId === 'new' ? newTypeOption : selectedTypeName ?? '');
  const createdAt = new Date().toISOString();
  const preview = $derived<NodeCardData>({
    id: instance,
    title: title.trim() || 'Untitled node',
    typeName: typeId === 'new'
      ? typeName.trim() || 'New node type'
      : selectedTypeName ?? 'Node',
    authorName: 'You',
    createdAt,
    description: '',
    breadcrumb: path,
    communities: communityIds.map((id) => communities.find((community) => community.id === id)?.name ?? 'Unavailable community'),
    related: [],
    requestedTypes: [],
    childTypes: types.filter((type) => requestedTypeIds.includes(type.id)).map((type) => ({
      typeId: type.id,
      label: `0 ${type.pluralName ?? type.name}`,
      count: 0,
    })),
    reactions: previewReactions,
    commentCount: 0,
    status: 'Active',
  });
  // Build a normalized snapshot for preview/submission without mutating the caller’s initial draft.
  function draft(): NodeDraft {
    return {
      title: title.trim(),
      shortDescription: derivedExcerpt(blocks, title),
      markdown: blocks.find((block) => block.kind === 'markdown')?.text ?? derivedExcerpt(blocks, title),
      blocks,
      typeId: typeId === 'new' ? null : typeId,
      requestedTypeIds,
      communityIds,
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
  Layout: Optional parent context leads into type/title and sequential block editing;
  preview and contribution requests precede the submit actions.
-->

<form
  class="node-form"
  aria-label={parentId ? 'Create child node' : 'Create root node'}
  onsubmit={(event) => {
    event.preventDefault();
    blockError = validateAuthorBlocks(blocks) ?? '';
    if (blockError) return;
    onSubmit(draft());
  }}
>
  <NodeAuthoringCard node={preview} {blocks} fields={titleFields} actions={creationActions}
    disabled={submitting} onChange={(updated) => { blocks = updated; blockError = ''; }} />
  {#if blockError}<p role="alert">{blockError}</p>{/if}
</form>

{#snippet titleFields()}
    <div class="title-row">
      <div class="type-field">
        <span aria-hidden="true">{selectedTypeLabel}</span>
        <select
          aria-label="Node type"
          use:focusAndScrollOnMount
          id={`${instance}-type`}
          bind:value={typeId}
          required
          >{#each types as type}<option value={type.id}>{type.name}</option>{/each}<option
            value="new">{newTypeOption}</option
          ></select
        >
      </div>
      <div class="title-field">
        <input
          aria-label="Title"
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
{/snippet}

{#snippet creationActions()}
  <footer>
    {#if communities.length || communityIds.length}
      <div class="contributions">
        <label for={`${instance}-community`}>Communities (optional)</label>
        <select id={`${instance}-community`} aria-describedby={`${instance}-community-help`}
          onchange={(event) => {
            const id = event.currentTarget.value;
            if (id && !communityIds.includes(id)) communityIds = [...communityIds, id];
            event.currentTarget.value = '';
          }}>
          <option value="">Add a community…</option>
          {#each communities as community (community.id)}
            <option value={community.id} disabled={communityIds.includes(community.id)}>{community.name}</option>
          {/each}
        </select>
        <small id={`${instance}-community-help`}>Link to any number of communities, or leave this blank.</small>
        {#if communityIds.length}
          <ul aria-label="Linked communities">
            {#each communityIds as id (id)}
              {@const name = communities.find((community) => community.id === id)?.name ?? 'Unavailable community'}
              <li>{name}<button type="button" aria-label={`Remove ${name}`}
                onclick={() => communityIds = communityIds.filter((entry) => entry !== id)}>×</button></li>
            {/each}
          </ul>
        {/if}
      </div>
    {/if}
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
    <button type="submit" disabled={submitting}>{submitting ? 'Creating…' : 'Create node'}</button>
  </div>
{/snippet}

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
  footer,
  .actions {
    padding-inline: var(--atlas-card-padding, 1.25rem);
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
    position: relative;
    flex: 0 1 auto;
    min-width: 0;
    max-width: 100%;
  }
  /* Size the closed native select from its current label. The options stay
     in the native popup, which can size independently of the control. */
  .type-field span {
    display: block;
    visibility: hidden;
    white-space: nowrap;
    overflow: hidden;
    padding: 0.6rem 2rem 0.6rem 0.6rem;
    border: 1px solid transparent;
  }
  .type-field select {
    position: absolute;
    inset: 0;
    height: 100%;
    text-overflow: ellipsis;
    color: var(--atlas-accent);
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
  :focus-visible {
    outline: 2px solid var(--atlas-focus);
    outline-offset: 2px;
  }
  @media (max-width: 380px) {
    footer,
    .actions {
      padding-inline: var(--atlas-card-padding-narrow, 0.75rem);
    }
  }
</style>
