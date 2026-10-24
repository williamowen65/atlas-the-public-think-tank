<!--
  SessionControl: Reusable session check, login, registration and optional sign-out UI.
  Reports participant identity through onSession; hideWhenSignedIn controls the signed-in helper display.
-->
<script lang="ts">
  import { onMount, untrack } from 'svelte';
  import { commentRequest, CommentRequestError } from '../api/comments';
  let {
    sessionBase = '/atlas-session-api',
    prompt = 'Sign in to Atlas',
    expanded = false,
    startRegister = false,
    focusOnMount = false,
    hideWhenSignedIn = false,
    onSession,
  }: {
    /**
     * Base URL for session/CSRF reads and login, registration and logout writes. Defaults to
     * /atlas-session-api; this prop chooses request URLs, not proxy configuration.
     */
    sessionBase?: string;
    /** Text of the signed-out sign-in disclosure summary. */
    prompt?: string;
    /** Opens the signed-out disclosure so the login/register form is visible. */
    expanded?: boolean;
    /**
     * Initial registration versus login mode. Captured once; the visitor can switch modes
     * afterward.
     */
    startRegister?: boolean;
    /**
     * Requests initial focus/scroll placement when the relevant form is rendered in the browser.
     */
    focusOnMount?: boolean;
    /** Hides this entire session helper once signed in, including its Sign out button. */
    hideWhenSignedIn?: boolean;
    /**
     * Callback: Reports the participant ID after session checks or sign-in; null indicates no
     * usable session, including failed checks. May run more than once.
     */
    onSession?: (id: string | null) => void;
  } = $props();
  let participantId = $state<string | null>(null);
  let checking = $state(true);
  let busy = $state(false);
  let error = $state('');
  let email = $state('');
  let password = $state('');
  let displayName = $state('');
  let registering = $state(untrack(() => startRegister));
  let registered = $state(false);
  let generation = 0;
  const instance = $props.id();
  const controller = new AbortController();
  // Read the current session; generation checks prevent older responses from replacing a newer check.
  async function refresh() {
    error = '';
    const current = ++generation;
    try {
      const session = await commentRequest<{ participantId: string | null }>(
        sessionBase,
        'auth/session',
        'GET',
        undefined,
        AbortSignal.any([controller.signal, AbortSignal.timeout(10000)]),
      );
      if (!controller.signal.aborted && current === generation) {
        participantId = session.participantId;
        onSession?.(participantId);
      }
    } catch (cause) {
      if (!controller.signal.aborted && current === generation) {
        participantId = null;
        onSession?.(null);
        if (!(cause instanceof CommentRequestError) || cause.status !== 401)
          error = 'Sign-in is unavailable. Start the local API and retry.';
      }
    } finally {
      if (current === generation) checking = false;
    }
  }
  // Check once when mounted, then listen for Atlas sign-in/out events until the component is removed.
  onMount(() => {
    void refresh();
    const changed = () => void refresh();
    window.addEventListener('atlas-session-changed', changed);
    return () => {
      controller.abort();
      window.removeEventListener('atlas-session-changed', changed);
    };
  });
  function reveal(element: HTMLElement) {
    if (!focusOnMount) return;
    const frame = requestAnimationFrame(() => {
      element.scrollIntoView({ block: 'start' });
      element.querySelector('input')?.focus({ preventScroll: true });
    });
    return { destroy: () => cancelAnimationFrame(frame) };
  }
  // Login first, verify the resulting session, then notify the parent and other session-aware components.
  async function signIn() {
    if (busy) return;
    busy = true;
    error = '';
    try {
      await commentRequest(
        sessionBase,
        'auth/login',
        'POST',
        { email, password },
        AbortSignal.any([controller.signal, AbortSignal.timeout(10000)]),
      );
      const session = await commentRequest<{ participantId: string | null }>(
        sessionBase,
        'auth/session',
        'GET',
        undefined,
        AbortSignal.any([controller.signal, AbortSignal.timeout(10000)]),
      );
      participantId = session.participantId;
      if (!participantId) throw new Error('Sign-in was not completed.');
      onSession?.(participantId);
      window.dispatchEvent(new Event('atlas-session-changed'));
    } catch {
      if (!controller.signal.aborted)
        error =
          'Sign-in was not completed. Check your credentials and account confirmation, then try again.';
    } finally {
      password = '';
      busy = false;
    }
  }
  // Registration creates an account but does not bypass the email-confirmation/sign-in workflow.
  async function register() {
    if (busy) return;
    busy = true;
    error = '';
    try {
      await commentRequest(
        sessionBase,
        'auth/register',
        'POST',
        { email, password, displayName },
        AbortSignal.any([controller.signal, AbortSignal.timeout(10000)]),
      );
      registered = true;
      registering = false;
    } catch (cause) {
      error =
        cause instanceof CommentRequestError && cause.status === 503
          ? 'Registration requires local email delivery to be configured.'
          : 'Registration was not completed. Check your information and try again.';
    } finally {
      password = '';
      busy = false;
    }
  }
  // Clear local identity only after the server accepts logout; failures remain retryable.
  async function signOut() {
    if (busy) return;
    busy = true;
    error = '';
    try {
      await commentRequest(
        sessionBase,
        'auth/logout',
        'POST',
        undefined,
        AbortSignal.any([controller.signal, AbortSignal.timeout(10000)]),
      );
      participantId = null;
      onSession?.(null);
      window.dispatchEvent(new Event('atlas-session-changed'));
    } catch {
      if (!controller.signal.aborted) error = 'Could not sign out. Please retry.';
    } finally {
      busy = false;
    }
  }
</script>

<div class="session-control">
  {#if checking}<p role="status">Checking sign-in…</p>
  {:else if participantId && hideWhenSignedIn}<!-- Authentication helper is hidden once signed in; use the header account menu to sign out. -->
  {:else if participantId}<div class="session">
      <span>Signed in</span><button onclick={() => void signOut()} disabled={busy}>Sign out</button>
    </div>
  {:else}<details class="sign-in" open={expanded} use:reveal>
      <summary>{prompt}</summary>{#if registered}<p role="status">
          Account created. Confirm your email through the development email workflow, then sign in.
        </p>{/if}
      <form
        aria-label={registering ? 'Create an Atlas account' : 'Sign in to Atlas'}
        onsubmit={(event) => {
          event.preventDefault();
          if (registering) void register();
          else void signIn();
        }}
      >
        {#if registering}<label for={`${instance}-name`}>Display name</label><input
            id={`${instance}-name`}
            bind:value={displayName}
            required
            maxlength="100"
            autocomplete="nickname"
          />{/if}
        <label for={`${instance}-email`}>Email</label><input
          id={`${instance}-email`}
          type="email"
          bind:value={email}
          autocomplete="username"
          required
          maxlength="256"
        />
        <label for={`${instance}-password`}>Password</label><input
          id={`${instance}-password`}
          type="password"
          bind:value={password}
          autocomplete={registering ? 'new-password' : 'current-password'}
          required
          maxlength="1024"
        />
        <button disabled={busy}
          >{busy ? 'Please wait…' : registering ? 'Create account' : 'Sign in'}</button
        >
      </form>
      <button
        type="button"
        class="switch-mode"
        onclick={() => {
          registering = !registering;
          error = '';
        }}
        >{registering
          ? 'Already have an account? Sign in'
          : 'New to Atlas? Create an account'}</button
      >
    </details>{/if}
  {#if error}<p role="alert">{error}</p>
    <button onclick={() => void refresh()} disabled={busy}>Retry sign-in connection</button>{/if}
</div>

<style lang="scss">
  .session {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 1rem;
  }
  .sign-in {
    margin-bottom: 1rem;
  }
  summary {
    cursor: pointer;
    color: var(--atlas-accent);
  }
  form {
    display: grid;
    gap: 0.5rem;
    margin-top: 0.75rem;
    max-width: 24rem;
  }
  input,
  button {
    font: inherit;
  }
  input {
    min-width: 0;
    padding: 0.6rem;
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    background: var(--atlas-surface);
    color: var(--atlas-text);
  }
  button {
    padding: 0.5rem 0.75rem;
    cursor: pointer;
  }
  button:disabled {
    cursor: default;
  }
</style>
