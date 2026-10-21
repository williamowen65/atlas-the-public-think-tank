<script lang="ts" module>
  export interface NodeDraft { title: string; shortDescription: string; markdown: string; typeId: string | null; requestedTypeIds: string[]; parentId?: string; newType?: { name: string; description: string } }
</script>
<script lang="ts">
  import type { NodeType } from '../api/nodePage';
  let { types, parentId, parentTitle, initialTypeId = '', onPreview, onSubmit, submitting = false }: {
    types: NodeType[]; parentId?: string; parentTitle?: string; initialTypeId?: string;
    onPreview?: (draft: NodeDraft) => void; onSubmit?: (draft: NodeDraft) => void; submitting?: boolean;
  } = $props();
  let title = $state(''); let shortDescription = $state(''); let markdown = $state('');
  let typeId = $state(''); let requestedTypeIds = $state<string[]>([]);
  let validation = $state('');
  let typeName = $state(''); let typeDescription = $state('');
  const instance = $props.id();
  $effect(() => { if (!typeId && types.length) typeId = initialTypeId || types[0].id; });
  function draft(): NodeDraft { return { title: title.trim(), shortDescription: shortDescription.trim(), markdown, typeId: typeId === 'new' ? null : typeId, requestedTypeIds, parentId, ...(typeId === 'new' ? { newType: { name: typeName.trim(), description: typeDescription.trim() } } : {}) }; }
</script>
<form class="node-form atlas-panel" aria-label={parentId ? 'Create child node' : 'Create root node'} onsubmit={event => { event.preventDefault(); const value = draft(); if (!value.title || !value.shortDescription || !value.markdown.trim() || (value.newType && !value.newType.name)) { validation = 'Enter a title, short description, content, and a type name where needed.'; return; } validation = ''; onPreview?.(value); }}>
  <h2>{parentId ? 'Add a child node' : 'Create a root node'}</h2>
  {#if parentId}<p>Connected to: <strong>{parentTitle ?? parentId}</strong></p>{/if}
  <label for={`${instance}-title`}>Title</label><input id={`${instance}-title`} bind:value={title} required maxlength="450" />
  <label for={`${instance}-short`}>Short description</label><textarea id={`${instance}-short`} bind:value={shortDescription} required maxlength="280" rows="3" aria-describedby={`${instance}-remaining`}></textarea>
  <small id={`${instance}-remaining`}>{shortDescription.length}/280 characters</small>
  <label for={`${instance}-type`}>Node type</label><select id={`${instance}-type`} bind:value={typeId} required>{#each types as type}<option value={type.id}>{type.name}</option>{/each}<option value="new">Define a new node type…</option></select>
  {#if typeId === 'new'}<fieldset><legend>New node type</legend><label for={`${instance}-type-name`}>Type name</label><input id={`${instance}-type-name`} bind:value={typeName} required maxlength="100" /><label for={`${instance}-type-description`}>Type description</label><textarea id={`${instance}-type-description`} bind:value={typeDescription} maxlength="500"></textarea></fieldset>{/if}
  <label for={`${instance}-content`}>Content (Markdown)</label><textarea id={`${instance}-content`} bind:value={markdown} required maxlength="32000" rows="8"></textarea>
  <fieldset><legend>Invite these response types (optional)</legend>{#each types as type}<label class="choice"><input type="checkbox" bind:group={requestedTypeIds} value={type.id} /> {type.name}</label>{/each}</fieldset>
  {#if validation}<p role="alert">{validation}</p>{/if}
  <div class="actions"><button type="submit" disabled={submitting}>Preview draft</button><button type="button" disabled={!onSubmit || submitting} onclick={event => { const form = event.currentTarget.form!; if (form.reportValidity()) onSubmit?.(draft()); }}>{submitting ? 'Creating…' : 'Create node'}</button></div>
  {#if !onSubmit}<p class="note">You can preview this draft. Sign-in and saving are not connected yet; nothing is posted.</p>{/if}
</form>
<style lang="scss">
  .node-form{max-width:48rem;margin-inline:auto;display:grid;gap:.6rem;}h2{margin:0 0 .5rem;}input,textarea,select,button{font:inherit;max-width:100%;}input:not([type=checkbox]),textarea,select{width:100%;padding:.6rem;border:1px solid var(--atlas-border);border-radius:var(--atlas-radius);background:var(--atlas-surface);color:var(--atlas-text);}fieldset{min-width:0;border:1px solid var(--atlas-border);padding:.75rem;}.choice{display:inline-flex;align-items:center;margin:.3rem .7rem .3rem 0;}.actions{display:flex;flex-wrap:wrap;gap:.75rem;}button{padding:.65rem 1rem;cursor:pointer;}button:disabled{cursor:default;}.note,small{color:var(--atlas-muted);font-size:.85rem;}
</style>
