<script lang="ts" module>
  export interface NodeDraft { title: string; shortDescription: string; markdown: string; typeId: string | null; requestedTypeIds: string[]; parentId?: string; newType?: { name: string; description: string } }
</script>
<script lang="ts">
  import VotingDial from './VotingDial.svelte';
  import Breadcrumb, { type BreadcrumbItem } from './Breadcrumb.svelte';
  import MarkdownContent from './MarkdownContent.svelte';
  import type { NodeType } from '../api/nodePage';
  let { types, parentId, parentTitle, initialTypeId = '', breadcrumb = [], focusOnMount = false, onPreview, onSubmit, submitting = false }: {
    types: NodeType[]; parentId?: string; parentTitle?: string; initialTypeId?: string; breadcrumb?: BreadcrumbItem[]; focusOnMount?: boolean;
    onPreview?: (draft: NodeDraft) => void; onSubmit?: (draft: NodeDraft) => void; submitting?: boolean;
  } = $props();
  let title = $state(''); let shortDescription = $state(''); let markdown = $state('');
  let typeId = $state(''); let requestedTypeIds = $state<string[]>([]);
  let requestedType = $state('');
  const path = $derived(parentId ? [...breadcrumb, { id: parentId, title: parentTitle ?? parentId }] : breadcrumb);
  function reveal(control: HTMLSelectElement) {
    if (focusOnMount) requestAnimationFrame(() => {
      if (!control.isConnected) return;
      control.focus({ preventScroll: true });
      control.closest('form')?.scrollIntoView({ block: 'start', behavior: matchMedia('(prefers-reduced-motion: reduce)').matches ? 'instant' : 'smooth' });
    });
  }
  let typeName = $state(''); let typeDescription = $state('');
  const instance = $props.id();
  $effect(() => { if (!typeId && types.length) typeId = initialTypeId || types[0].id; });
  function draft(): NodeDraft { return { title: title.trim(), shortDescription: shortDescription.trim(), markdown, typeId: typeId === 'new' ? null : typeId, requestedTypeIds, parentId, ...(typeId === 'new' ? { newType: { name: typeName.trim(), description: typeDescription.trim() } } : {}) }; }
  $effect(() => { onPreview?.(draft()); });
</script>
<form class="node-form atlas-panel" aria-label={parentId ? 'Create child node' : 'Create root node'} onsubmit={event => { event.preventDefault(); onSubmit?.(draft()); }}>
  {#if path.length}<div class="ribbon-context"><Breadcrumb items={path} /></div>{/if}
  <header>
    <p class="note">{parentId ? 'Add a child node' : 'Create a root node'} · Live draft</p>
    <div class="title-row">
      <div class="type-field"><label for={`${instance}-type`}>Node type</label><select use:reveal id={`${instance}-type`} bind:value={typeId} required>{#each types as type}<option value={type.id}>{type.name}</option>{/each}<option value="new">Define a new node type…</option></select></div>
      <div class="title-field"><label for={`${instance}-title`}>Title</label><input id={`${instance}-title`} bind:value={title} placeholder="Give your node a title" required maxlength="450" /></div>
    </div>
    {#if typeId === 'new'}<fieldset><legend>New node type</legend><label for={`${instance}-type-name`}>Type name</label><input id={`${instance}-type-name`} bind:value={typeName} required maxlength="100" /><label for={`${instance}-type-description`}>Type description</label><textarea id={`${instance}-type-description`} bind:value={typeDescription} maxlength="500"></textarea></fieldset>{/if}
  </header>
  <div class="intro">
    <VotingDial label="Node importance" disabled />
    <div class="summary"><label for={`${instance}-short`}>Short description</label><textarea id={`${instance}-short`} bind:value={shortDescription} placeholder="Introduce your idea…" required maxlength="280" rows="5" aria-describedby={`${instance}-remaining`}></textarea><small id={`${instance}-remaining`}>{shortDescription.length}/280 characters</small></div>
  </div>
  <div class="content-editor"><label for={`${instance}-content`}>Content (Markdown)</label><textarea id={`${instance}-content`} bind:value={markdown} placeholder="Develop your node here…" required maxlength="32000" rows="6"></textarea>
    <section aria-label="Content preview"><MarkdownContent text={markdown} /></section>
  </div>
  <footer>
    <span class="note">0 Comments</span>
    <div class="contributions"><label for={`${instance}-requested`}>Requested contributions</label><select id={`${instance}-requested`} bind:value={requestedType} onchange={() => { if (requestedType && !requestedTypeIds.includes(requestedType)) requestedTypeIds = [...requestedTypeIds, requestedType]; requestedType = ''; }}><option value="">Add a contribution type…</option>{#each types as type}<option value={type.id} disabled={requestedTypeIds.includes(type.id)}>{type.name}</option>{/each}</select>
      {#if requestedTypeIds.length}<ul aria-label="Requested contribution types">{#each requestedTypeIds as id}<li>{types.find(type => type.id === id)?.name}<button type="button" aria-label={`Remove ${types.find(type => type.id === id)?.name} contribution`} onclick={() => requestedTypeIds = requestedTypeIds.filter(value => value !== id)}>×</button></li>{/each}</ul>{/if}
    </div>
  </footer>
  <div class="actions"><button type="submit" disabled={!onSubmit || submitting}>{submitting ? 'Creating…' : 'Create node'}</button></div>
  {#if !onSubmit}<p class="note">Live draft only. Sign-in and saving are not connected yet; nothing is posted.</p>{/if}
</form>
<style lang="scss">
  .node-form{max-width:48rem;width:100%;margin-inline:auto;padding:0;overflow-wrap:anywhere;scroll-margin-top:1rem;}
  header,.intro,.content-editor,footer,.actions,.node-form>p{padding-inline:var(--atlas-card-padding,1.25rem);}header{padding-top:.75rem;}label{display:block;font-size:.85rem;margin-bottom:.25rem;}.title-row{display:flex;flex-wrap:wrap;gap:.75rem;align-items:end;}.type-field{flex:1 1 10rem;min-width:0;}.title-field{flex:3 1 16rem;min-width:0;}.title-field input{font-size:1.3rem;font-weight:600;}
  input,textarea,select,button{font:inherit;max-width:100%;}input,textarea,select{box-sizing:border-box;width:100%;padding:.6rem;border:1px solid var(--atlas-border);border-radius:var(--atlas-radius);background:var(--atlas-surface);color:var(--atlas-text);}textarea{resize:vertical;}fieldset{min-width:0;border:1px solid var(--atlas-border);padding:.75rem;margin-top:.75rem;}.intro{display:flex;align-items:start;gap:var(--atlas-dial-text-gap,1rem);margin-block:1rem;}.summary{flex:1;min-width:0;}.content-editor{min-width:0;}section:empty{display:none;}section{margin-block:1rem;}footer{display:flex;flex-wrap:wrap;gap:1rem;align-items:start;border-top:1px solid var(--atlas-border);padding-block:1rem;}.contributions{flex:1;min-width:0;}ul{display:flex;flex-wrap:wrap;list-style:none;padding:0;gap:.5rem;}li{border:1px solid var(--atlas-border);border-radius:var(--atlas-radius);padding:.25rem .5rem;}li button{border:0;background:transparent;color:inherit;margin-left:.5rem;}.actions{padding-bottom:.5rem;}button{padding:.5rem .75rem;cursor:pointer;}button:disabled{cursor:default;}.note,small{color:var(--atlas-muted);font-size:.85rem;}.node-form>p{padding-bottom:1rem;}:focus-visible{outline:2px solid var(--atlas-focus);outline-offset:2px;}@media(max-width:380px){header,.intro,.content-editor,footer,.actions,.node-form>p{padding-inline:var(--atlas-card-padding-narrow,.75rem);}}
</style>
