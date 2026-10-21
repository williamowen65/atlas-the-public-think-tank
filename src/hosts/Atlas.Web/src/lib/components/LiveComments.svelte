<script lang="ts">
  import { onMount } from 'svelte';
  import LiveComments from './LiveComments.svelte';
  import { readJson, type Comment, type CommentPage } from '../api/nodePage';
  let { nodeId, parentId, apiBase = '/atlas-preview-api' }: { nodeId: string; parentId?: string; apiBase?: string } = $props();
  let items = $state<Comment[]>([]); let next = $state<CommentPage['next']>(null); let started = $state(false); let loading = $state(false); let error = $state(''); let open = $state<string[]>([]);
  const controller = new AbortController();
  async function load() {
    if (loading) return; loading = true; error = '';
    const query = new URLSearchParams({ size: '10' }); if (parentId) query.set('parent', parentId); if (next) { query.set('afterAt', next.afterAt); query.set('afterId', next.afterId); }
    try { const page = await readJson<CommentPage>(apiBase, `nodes/${encodeURIComponent(nodeId)}/comments?${query}`, AbortSignal.any([controller.signal, AbortSignal.timeout(10000)])); if (!controller.signal.aborted) { items = [...items, ...page.items.filter(item => !items.some(old => old.id === item.id))]; next = page.next; started = true; } }
    catch { if (!controller.signal.aborted) error = 'Could not load comments.'; } finally { loading = false; }
  }
  onMount(() => { void load(); return () => controller.abort(); });
</script>
<div class="comments" aria-label={parentId ? 'Replies' : 'Comments'} aria-busy={loading}>
  {#each items as comment (comment.id)}<article class="comment"><p class="meta">Participant {comment.authorId.slice(0, 8)} · <time datetime={comment.createdAt}>{new Date(comment.createdAt).toLocaleDateString()}</time></p><p class="body">{comment.body ?? 'Comment removed.'}</p><button onclick={() => open = open.includes(comment.id) ? open.filter(id => id !== comment.id) : [...open, comment.id]} aria-expanded={open.includes(comment.id)}>{open.includes(comment.id) ? 'Hide replies' : 'View replies'}</button>{#if open.includes(comment.id)}<LiveComments {nodeId} parentId={comment.id} {apiBase} />{/if}</article>{/each}
  {#if error}<p role="alert">{error}</p>{/if}
  {#if loading}<p role="status">Loading comments…</p>{:else if started && !items.length}<p>No comments yet.</p>{/if}
  {#if error || next}<button onclick={() => void load()} disabled={loading}>{error ? 'Retry comments' : 'Load more comments'}</button>{/if}
</div>
<style>.comment{border-bottom:1px solid var(--atlas-border);padding:.75rem 0;}.meta{font-size:.85rem;color:var(--atlas-muted);}.body{white-space:pre-wrap;overflow-wrap:anywhere;}.comments :global(.comments){margin-left:.75rem;padding-left:.75rem;border-left:1px solid var(--atlas-border);}button{font:inherit;}</style>
