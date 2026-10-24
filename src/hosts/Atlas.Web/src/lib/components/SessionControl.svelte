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
  /**
   * Participant identity reported by the server, or null when no usable session is known.
   * This is UI state, not the authentication cookie.
   */
  let participantId = $state<string | null>(null);
  /**
   * Whether the initial session check is still pending; controls the loading message. Later
   * refresh calls do not reset this flag.
   */
  let checking = $state(true);
  /**
   * Whether a login, registration or logout operation is underway; prevents duplicate
   * submissions and disables action buttons.
   */
  let busy = $state(false);
  /**
   * User-facing failure message; empty text hides the error and retry controls.
   */
  let error = $state('');
  /**
   * Email entered in the login or registration form; retained when switching modes.
   */
  let email = $state('');
  /**
   * Password input held temporarily in memory and cleared after each login or registration
   * attempt.
   */
  let password = $state('');
  /**
   * Display name submitted during registration; not used for login.
   */
  let displayName = $state('');
  /**
   * Whether the form shows registration fields. untrack reads startRegister once without
   * making it a reactive dependency.
   */
  let registering = $state(untrack(() => startRegister));
  /**
   * Whether registration succeeded, so the form can show the email-confirmation
   * instructions.
   */
  let registered = $state(false);
  /**
   * Sequence number for refresh calls, not a login/logout count. Each refresh increments it
   * so older checks cannot overwrite a newer check’s result.
   */
  let generation = 0;
  /**
   * Stable component-instance ID used to connect each input with its label without
   * collisions between multiple session controls.
   */
  const instance = $props.id();
  /**
   * Component-lifetime request cancellation controller. AbortController and AbortSignal are
   * built-in Web APIs supplied by the browser, so no import is needed. Its signal is passed
   * to requests; cleanup aborts unfinished work when this component is removed.
   */
  const controller = new AbortController();
  /**
   * Read the server’s current participant identity and report it through onSession.
   * Each call gets a sequence number; only the newest refresh may publish its result.
   * Cancellation stops unfinished client requests; the sequence check separately handles
   * overlapping refreshes that complete in the wrong order.
   */
  async function refresh() {
    error = '';
    // Capture this check’s number. A later refresh changes generation, making this one stale.
    const current = ++generation;
    try {
      const session = await commentRequest<{ participantId: string | null }>(
        sessionBase,
        'auth/session',
        'GET',
        undefined,
        // Cancel if the component is removed OR this request takes longer than ten seconds.
        // AbortSignal.any combines signals; timeout creates a signal that aborts after 10000 ms.
        AbortSignal.any([controller.signal, AbortSignal.timeout(10000)]),
      );
      // Publish only while the component is active and no newer refresh has started.
      if (!controller.signal.aborted && current === generation) {
        participantId = session.participantId;
        onSession?.(participantId);
      }
    } catch (cause) {
      // Ignore results from a removed component or an outdated check. For the newest
      // failed check, report no usable identity; 401 is the normal signed-out response.
      if (!controller.signal.aborted && current === generation) {
        participantId = null;
        onSession?.(null);
        // Connection failures, timeouts and other HTTP errors need a visible retry message.
        if (!(cause instanceof CommentRequestError) || cause.status !== 401)
          error = 'Sign-in is unavailable. Start the local API and retry.';
      }
    } finally {
      // An older request must not dismiss the initial loading message for a newer check.
      if (current === generation) checking = false;
    }
  }
  // Run in the browser when this component mounts. The returned function runs when
  // this component is destroyed—such as leaving the page or conditionally removing it.
  onMount(() => {
    // Call our refresh function above. void discards its Promise, not the request itself.
    void refresh();
    /** Recheck server identity when this tab announces a successful sign-in or sign-out. */
    const changed = () => void refresh();
    window.addEventListener('atlas-session-changed', changed);
    return () => {
      // Stop pending requests and remove the listener so a destroyed control does no more work.
      // Aborting does not undo a login or other operation already processed by the server.
      controller.abort();
      window.removeEventListener('atlas-session-changed', changed);
    };
  });
  /**
   * Svelte action attached with use:reveal when the details element enters the DOM.
   * If requested, scroll it into view and focus its first input on the next animation frame.
   * This does not open/close the disclosure or run on every arrow click: details/summary
   * supplies native toggling, and expanded supplies the open attribute.
   * @param element The mounted details element containing the session form.
   * @returns An action cleanup that cancels the scheduled frame, when one was requested.
   */
  function reveal(element: HTMLElement) {
    if (!focusOnMount) return;
    const frame = requestAnimationFrame(() => {
      element.scrollIntoView({ block: 'start' });
      element.querySelector('input')?.focus({ preventScroll: true });
    });
    return { destroy: () => cancelAnimationFrame(frame) };
  }
  /**
   * Submit credentials, then verify the resulting identity through auth/session.
   * The server issues the HttpOnly authentication cookie; the browser handles it.
   * Notify the parent and session-aware controls in this tab after verification.
   * Always clear the password and release the submission lock, even on failure.
   */
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
      // A successful login response must be followed by a usable participant identity.
      participantId = session.participantId;
      if (!participantId) throw new Error('Sign-in was not completed.');
      onSession?.(participantId);
      window.dispatchEvent(new Event('atlas-session-changed'));
    } catch {
      // Do not show a login error after this component has been removed.
      if (!controller.signal.aborted)
        error =
          'Sign-in was not completed. Check your credentials and account confirmation, then try again.';
    } finally {
      // Discard the secret on both success and failure; allow another submission.
      password = '';
      busy = false;
    }
  }
  /**
   * Create an account, then return the form to login mode with confirmation instructions.
   * Registration does not sign the participant in. Explain unavailable mail delivery
   * separately, and clear the password and submission lock after every attempt.
   */
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
      // Show confirmation instructions rather than treating registration as a login.
      registered = true;
      registering = false;
    } catch (cause) {
      // Mail delivery being unavailable needs different guidance from rejected form data.
      error =
        cause instanceof CommentRequestError && cause.status === 503
          ? 'Registration requires local email delivery to be configured.'
          : 'Registration was not completed. Check your information and try again.';
    } finally {
      // Discard the secret on both success and failure; allow another submission.
      password = '';
      busy = false;
    }
  }
  /**
   * Ask the server to clear its authentication cookie before clearing local identity.
   * Notify the parent and this tab’s other controls only after successful logout.
   * Keep the current identity on failure so the participant can retry.
   */
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
      // The server accepted logout; now synchronize this control and other listeners.
      participantId = null;
      onSession?.(null);
      window.dispatchEvent(new Event('atlas-session-changed'));
    } catch {
      // Preserve local identity on failure, and offer retry only while still mounted.
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
