<script lang="ts">
  import { onMount } from 'svelte';
  import { commentRequest, CommentRequestError } from '../api/comments';
  let { sessionBase = '/atlas-comments-api', prompt = 'Sign in to Atlas', expanded = false, focusOnMount = false, onSession }: { sessionBase?: string; prompt?: string; expanded?: boolean; focusOnMount?: boolean; onSession?: (id: string | null) => void } = $props();
  let participantId = $state<string | null>(null);
  let checking = $state(true); let busy = $state(false); let error = $state('');
  let email = $state(''); let password = $state('');
  let generation = 0;
  const instance = $props.id();
  const controller = new AbortController();
  async function refresh() {
    error = '';
    const current = ++generation;
    try {
      const session = await commentRequest<{ participantId: string | null }>(sessionBase, 'auth/session', 'GET', undefined, AbortSignal.any([controller.signal, AbortSignal.timeout(10000)]));
      if (!controller.signal.aborted && current === generation) { participantId = session.participantId; onSession?.(participantId); }
    } catch (cause) {
      if (!controller.signal.aborted && current === generation) {
        participantId = null; onSession?.(null);
        if (!(cause instanceof CommentRequestError) || cause.status !== 401) error = 'Sign-in is unavailable. Start the local API and retry.';
      }
    } finally { if (current === generation) checking = false; }
  }
  onMount(() => {
    void refresh();
    const changed = () => void refresh();
    window.addEventListener('atlas-session-changed', changed);
    return () => { controller.abort(); window.removeEventListener('atlas-session-changed', changed); };
  });
  function reveal(element: HTMLElement) {
    if (!focusOnMount) return;
    const frame = requestAnimationFrame(() => { element.scrollIntoView({ block: 'start' }); element.querySelector('input')?.focus({ preventScroll: true }); });
    return { destroy: () => cancelAnimationFrame(frame) };
  }
  async function signIn() {
    if (busy) return; busy = true; error = '';
    try {
      await commentRequest(sessionBase, 'auth/login', 'POST', { email, password }, AbortSignal.any([controller.signal, AbortSignal.timeout(10000)]));
      const session = await commentRequest<{ participantId: string | null }>(sessionBase, 'auth/session', 'GET', undefined, AbortSignal.any([controller.signal, AbortSignal.timeout(10000)]));
      participantId = session.participantId;
      if (!participantId) throw new Error('Sign-in was not completed.');
      onSession?.(participantId);
      window.dispatchEvent(new Event('atlas-session-changed'));
    } catch { if (!controller.signal.aborted) error = 'Sign-in was not completed. Check your credentials and account confirmation, then try again.'; }
    finally { password = ''; busy = false; }
  }
  async function signOut() {
    if (busy) return; busy = true; error = '';
    try { await commentRequest(sessionBase, 'auth/logout', 'POST', undefined, AbortSignal.any([controller.signal, AbortSignal.timeout(10000)])); participantId = null; onSession?.(null); window.dispatchEvent(new Event('atlas-session-changed')); }
    catch { if (!controller.signal.aborted) error = 'Could not sign out. Please retry.'; }
    finally { busy = false; }
  }
</script>
<div class="session-control">
  {#if checking}<p role="status">Checking sign-in…</p>
  {:else if participantId}<div class="session"><span>Signed in</span><button onclick={() => void signOut()} disabled={busy}>Sign out</button></div>
  {:else}<details class="sign-in" open={expanded} use:reveal><summary>{prompt}</summary><form aria-label="Sign in to Atlas" onsubmit={event => { event.preventDefault(); void signIn(); }}>
    <label for={`${instance}-email`}>Email</label><input id={`${instance}-email`} type="email" bind:value={email} autocomplete="username" required maxlength="256" />
    <label for={`${instance}-password`}>Password</label><input id={`${instance}-password`} type="password" bind:value={password} autocomplete="current-password" required maxlength="1024" />
    <button disabled={busy}>{busy ? 'Signing in…' : 'Sign in'}</button>
  </form></details>{/if}
  {#if error}<p role="alert">{error}</p><button onclick={() => void refresh()} disabled={busy}>Retry sign-in connection</button>{/if}
</div>
<style lang="scss">
  .session{display:flex;align-items:center;justify-content:space-between;gap:1rem;}.sign-in{margin-bottom:1rem;}summary{cursor:pointer;color:var(--atlas-accent);}form{display:grid;gap:.5rem;margin-top:.75rem;max-width:24rem;}input,button{font:inherit;}input{min-width:0;padding:.6rem;border:1px solid var(--atlas-border);border-radius:var(--atlas-radius);background:var(--atlas-surface);color:var(--atlas-text);}button{padding:.5rem .75rem;cursor:pointer;}button:disabled{cursor:default;}
</style>
