<script lang="ts">
  import { onMount } from 'svelte';
  import LiveComments from './LiveComments.svelte';
  import { commentRequest, CommentRequestError } from '../api/comments';
  let { nodeId, apiBase = '/atlas-preview-api', sessionBase = '/atlas-comments-api' }: { nodeId: string; apiBase?: string; sessionBase?: string } = $props();
  let participantId = $state<string | null>(null);
  let checking = $state(true); let busy = $state(false); let error = $state('');
  let email = $state(''); let password = $state('');
  const instance = $props.id();
  const controller = new AbortController();
  onMount(() => {
    void commentRequest<{ participantId: string | null }>(sessionBase, 'auth/session', 'GET', undefined, AbortSignal.any([controller.signal, AbortSignal.timeout(10000)]))
      .then(session => participantId = session.participantId).catch(cause => { if (!controller.signal.aborted && (!(cause instanceof CommentRequestError) || cause.status !== 401)) error = 'Sign-in is unavailable. Start the local API and retry by refreshing this page.'; }).finally(() => checking = false);
    return () => controller.abort();
  });
  async function signIn() {
    if (busy) return; busy = true; error = '';
    try {
      await commentRequest(sessionBase, 'auth/login', 'POST', { email, password }, AbortSignal.any([controller.signal, AbortSignal.timeout(10000)]));
      const session = await commentRequest<{ participantId: string | null }>(sessionBase, 'auth/session', 'GET', undefined, AbortSignal.any([controller.signal, AbortSignal.timeout(10000)]));
      participantId = session.participantId;
      if (!participantId) throw new Error('Sign-in was not completed.');
    } catch { if (!controller.signal.aborted) error = 'Sign-in was not completed. Check your credentials and account confirmation, then try again.'; }
    finally { password = ''; busy = false; }
  }
  async function signOut() {
    if (busy) return; busy = true; error = '';
    try { await commentRequest(sessionBase, 'auth/logout', 'POST', undefined, AbortSignal.any([controller.signal, AbortSignal.timeout(10000)])); participantId = null; }
    catch { if (!controller.signal.aborted) error = 'Could not sign out. Please retry.'; }
    finally { busy = false; }
  }
</script>
<section class="discussion" aria-label="Comment discussion">
  <header><h2>Comments</h2><p>Join the discussion or build on someone’s contribution.</p></header>
  {#if checking}<p role="status">Checking sign-in…</p>
  {:else if participantId}<div class="session"><span>Signed in</span><button onclick={() => void signOut()} disabled={busy}>Sign out</button></div>
  {:else}<details class="sign-in"><summary>Sign in to comment</summary><form aria-label="Comment sign-in" onsubmit={event => { event.preventDefault(); void signIn(); }}>
    <label for={`${instance}-email`}>Email</label><input id={`${instance}-email`} type="email" bind:value={email} autocomplete="username" required maxlength="256" />
    <label for={`${instance}-password`}>Password</label><input id={`${instance}-password`} type="password" bind:value={password} autocomplete="current-password" required maxlength="1024" />
    <button disabled={busy}>{busy ? 'Signing in…' : 'Sign in'}</button>
  </form></details>{/if}
  {#if error}<p role="alert">{error}</p>{/if}
  <LiveComments {nodeId} {apiBase} {sessionBase} {participantId} />
</section>
<style lang="scss">
  h2{margin:0;}header p{color:var(--atlas-muted);margin:.4rem 0 1rem;}.session{display:flex;align-items:center;justify-content:space-between;gap:1rem;}.sign-in{margin-bottom:1rem;}summary{cursor:pointer;color:var(--atlas-accent);}form{display:grid;gap:.5rem;margin-top:.75rem;max-width:24rem;}input,button{font:inherit;}input{min-width:0;padding:.6rem;border:1px solid var(--atlas-border);border-radius:var(--atlas-radius);background:var(--atlas-surface);color:var(--atlas-text);}button{padding:.5rem .75rem;cursor:pointer;}button:disabled{cursor:default;}
</style>
