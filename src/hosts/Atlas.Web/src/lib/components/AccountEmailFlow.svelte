<!-- Account confirmation is submitted explicitly so mail-link scanners cannot confirm accounts. -->
<script lang="ts">
  import { onMount } from 'svelte';
  import PasswordInput from './PasswordInput.svelte';
  import { apiRequest } from '../api/apiRequest';
  let { mode }: { mode: 'confirm' | 'reset' | 'forgot' } = $props();
  let email = $state('');
  let password = $state('');
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
    busy = true;
    error = '';
    try {
      const path = mode === 'confirm' ? 'confirm-email' : mode === 'reset' ? 'reset-password' : 'forgot-password';
      const body = mode === 'confirm' ? { userId, token } : mode === 'reset' ? { email, token, newPassword: password } : { email };
      await apiRequest('/atlas-session-api', `auth/${path}`, 'POST', body, AbortSignal.timeout(30000));
      done = true;
      token = '';
    } catch {
      error = mode === 'forgot' ? 'Could not request an email. Please try again.' : 'Could not complete this request. The link may have expired. Request another email and try again; for password reset, also check the password requirements.';
    } finally { password = ''; busy = false; }
  }
</script>
<svelte:head><title>{title} · Atlas</title><meta name="referrer" content="no-referrer" /></svelte:head>
<section class="auth-page">
  <h1>{title}</h1>
  {#if done}
    <p role="status">{mode === 'forgot' ? 'If this address has an eligible account, a password reset email has been sent. Check your inbox and spam folder.' : mode === 'confirm' ? 'Your email is confirmed. You can now sign in.' : 'Your password has been reset. You can now sign in.'}</p>
  {:else if ready && mode !== 'forgot' && (!token || (mode === 'confirm' && !userId))}
    <p role="alert">This link is incomplete. Request another email.</p>
  {:else}
    <form onsubmit={submit}>
      {#if mode !== 'confirm'}
        <label>Email <input type="email" bind:value={email} required autocomplete="email" maxlength="256" /></label>
      {/if}
      {#if mode === 'reset'}
        <p>Use at least 12 characters, including uppercase and lowercase letters, a number, and a symbol.</p>
        <label for="reset-new-password">New password</label>
        <PasswordInput id="reset-new-password" bind:value={password} autocomplete="new-password" minlength={12} />
      {/if}
      <button type="submit" disabled={!ready || busy}>{busy ? 'Please wait…' : mode === 'confirm' ? 'Confirm email' : mode === 'reset' ? 'Reset password' : 'Send reset email'}</button>
    </form>
  {/if}
  {#if error}<p role="alert">{error}</p>{/if}
  <p><a href="/login">Sign in or resend confirmation</a></p>
  {#if mode === 'reset'}<p><a href="/forgot-password">Request another reset email</a></p>{/if}
</section>
<style>
  form { display: grid; gap: 1rem; max-width: 28rem; }
  label { display: grid; gap: .4rem; }
  input, button { padding: .7rem; font: inherit; }
</style>
