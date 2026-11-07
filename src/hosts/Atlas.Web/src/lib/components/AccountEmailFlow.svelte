<!-- Account confirmation is submitted explicitly so mail-link scanners cannot confirm accounts. -->
<script lang="ts">
  import { onMount } from 'svelte';
  import PasswordInput from './PasswordInput.svelte';
  import ActionButton from './ActionButton.svelte';
  import { apiRequest, ApiRequestError } from '../api/apiRequest';
  let { mode }: { mode: 'confirm' | 'reset' | 'forgot' } = $props();
  let email = $state('');
  let password = $state('');
  let confirmation = $state('');
  const requirements = $derived([
    { label: 'At least 12 characters', met: password.length >= 12 },
    { label: 'An uppercase letter', met: /[A-Z]/.test(password) },
    { label: 'A lowercase letter', met: /[a-z]/.test(password) },
    { label: 'A number', met: /[0-9]/.test(password) },
    { label: 'A symbol', met: /[^a-zA-Z0-9]/.test(password) },
  ]);
  let token = $state('');
  let userId = $state('');
  let ready = $state(false);
  let busy = $state(false);
  let done = $state(false);
  let error = $state('');
  const title = $derived(mode === 'confirm' ? 'Confirm your email' : mode === 'reset' ? 'Reset your password' : 'Forgot your password?');
  onMount(() => {
    const params = new URLSearchParams(window.location.hash.slice(1));
    token = params.get('token') ?? '';
    userId = params.get('userId') ?? '';
    email = params.get('email') ?? '';
    if (window.location.hash) window.history.replaceState(null, '', window.location.pathname);
    ready = true;
  });
  async function submit(event: SubmitEvent) {
    event.preventDefault();
    if (busy) return;
    error = '';
    if (mode === 'reset' && !requirements.every(item => item.met)) {
      error = 'Your new password must meet all the requirements below.';
      return;
    }
    if (mode === 'reset' && password !== confirmation) {
      error = 'The passwords do not match. Enter the same password in both fields.';
      return;
    }
    busy = true;
    try {
      const path = mode === 'confirm' ? 'confirm-email' : mode === 'reset' ? 'reset-password' : 'forgot-password';
      const body = mode === 'confirm' ? { userId, token } : mode === 'reset' ? { email, token, newPassword: password } : { email };
      await apiRequest('/atlas-session-api', `auth/${path}`, 'POST', body, AbortSignal.timeout(60000));
      done = true;
      token = '';
    } catch (cause) {
      if (cause instanceof ApiRequestError && cause.status === 429) {
        error = cause.message;
      } else if (cause instanceof ApiRequestError && cause.status === 400 && mode !== 'forgot') {
        error = mode === 'reset'
          ? 'This reset request was not accepted. Reopen the email link and try again, or request a fresh reset email. Your new password must meet all requirements.'
          : 'This confirmation request was not accepted. Reopen the email link and try again, or request a fresh confirmation email.';
      } else {
        error = mode === 'forgot'
          ? 'We could not confirm that the reset email was sent. Check your inbox before requesting another, or try again shortly.'
          : 'We could not confirm the result. The service may be temporarily unavailable. Try signing in first; otherwise reopen the email link and retry shortly.';
      }
    } finally { password = ''; confirmation = ''; busy = false; }
  }
</script>
<svelte:head><title>{title} · Atlas</title><meta name="referrer" content="no-referrer" /></svelte:head>
<section class="auth-page">
  <h1>{title}</h1>
  <div class="auth-content">
  {#if mode === 'forgot' && !done}<p>Enter the email address you used to register. We’ll email you a link to choose a new password.</p>{/if}
  {#if done}
    <p role="status">{mode === 'forgot' ? 'If this address has an eligible account, a password reset email has been sent. Check your inbox and spam folder.' : mode === 'confirm' ? 'Your email is confirmed. You can now sign in.' : 'Your password has been reset. You can now sign in.'}</p>
  {:else if ready && mode !== 'forgot' && (!token || (mode === 'confirm' && !userId))}
    <p role="alert">This link is incomplete. Request another email.</p>
  {:else}
    <form onsubmit={submit} aria-label={title} aria-busy={busy}>
      {#if mode !== 'confirm'}
        <label>Email <input type="email" bind:value={email} required autocomplete="email" maxlength="256" /></label>
      {/if}
      {#if mode === 'reset'}
        <label for="reset-new-password">New password</label>
        <PasswordInput id="reset-new-password" bind:value={password} autocomplete="new-password" minlength={12} describedBy="reset-password-requirements" />
        <ul id="reset-password-requirements" class="requirements" aria-label="Password requirements">
          {#each requirements as item}<li><input type="checkbox" disabled checked={item.met} aria-hidden="true" /><span>{item.label}<span class="sr-only"> — {item.met ? 'met' : 'not yet met'}</span></span></li>{/each}
        </ul>
        <label for="reset-confirm-password">Confirm new password</label>
        <PasswordInput id="reset-confirm-password" bind:value={confirmation} autocomplete="new-password" minlength={12} />
      {/if}
      <ActionButton type="submit" disabled={!ready || busy} label={busy ? mode === 'forgot' ? 'Sending reset email…' : 'Please wait…' : mode === 'confirm' ? 'Confirm email' : mode === 'reset' ? 'Reset password' : 'Send reset email'} />
    </form>
  {/if}
  {#if error}<p role="alert">{error}</p>{/if}
  {#if done && mode !== 'forgot'}<p><a href="/login">Sign in</a></p>{/if}
  {#if done && mode === 'forgot'}<button type="button" class="secondary" onclick={() => { done = false; error = ''; }}>Request another reset email</button>{/if}
  {#if !done && mode === 'confirm'}<p><a href="/register">Resend confirmation instructions</a></p>{/if}
  {#if mode === 'reset'}<p><a href="/forgot-password">Request another reset email</a></p>{/if}
  </div>
</section>
<style>
  .auth-content { --atlas-action-width: 100%; max-width: 24rem; margin-bottom: 1rem; }
  form { display: grid; gap: .5rem; margin-top: .75rem; }
  label { display: grid; gap: .5rem; }
  input { box-sizing: border-box; width: 100%; min-width: 0; padding: .6rem; font: inherit; border: 1px solid var(--atlas-border); border-radius: var(--atlas-radius); background: var(--atlas-surface); color: var(--atlas-text); }
  .secondary { width: 100%; padding: .65rem 1rem; font: inherit; border: 1px solid var(--atlas-border); border-radius: var(--atlas-radius); background: var(--atlas-surface); color: var(--atlas-text); cursor: pointer; }
  .requirements { list-style: none; padding: 0; margin: .5rem 0 1rem; }
  .requirements li { display: flex; align-items: center; gap: .5rem; }
  .requirements input { width: 1rem; height: 1rem; margin: 0; opacity: 1; }
  .sr-only { position: absolute; width: 1px; height: 1px; overflow: hidden; clip-path: inset(50%); }
</style>
