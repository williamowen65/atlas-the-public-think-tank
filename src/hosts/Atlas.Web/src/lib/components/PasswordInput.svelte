<!-- Shared password field: the reveal control appears only when there is a value. -->
<script lang="ts">
  let { id, value = $bindable(''), autocomplete = 'current-password', required = true,
    minlength, describedBy }: { id: string; value?: string; autocomplete?: 'current-password' | 'new-password';
    required?: boolean; minlength?: number; describedBy?: string } = $props();
  let revealed = $state(false);
  $effect(() => { if (!value) revealed = false; });
</script>
<div class="password-field">
  <input {id} type={revealed ? 'text' : 'password'} bind:value {autocomplete} {required}
    {minlength} maxlength="1024" aria-describedby={describedBy} />
  {#if value}
    <button type="button" class="reveal" aria-label={revealed ? 'Hide password' : 'Show password'}
      aria-pressed={revealed} aria-controls={id} onclick={() => { revealed = !revealed; }}>
      <svg viewBox="0 0 24 24" width="22" height="22" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
        <path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7S2 12 2 12Z" />
        <circle cx="12" cy="12" r="3" />
        {#if revealed}<path d="m3 3 18 18" />{/if}
      </svg>
    </button>
  {/if}
</div>
<style>
  .password-field { position: relative; min-width: 0; width: 100%; }
  input { box-sizing: border-box; width: 100%; min-width: 0; font: inherit; padding: .6rem 3.25rem .6rem .6rem; border: 1px solid var(--atlas-border); border-radius: var(--atlas-radius); background: var(--atlas-surface); color: var(--atlas-text); }
  /* Edge exposes a native eye; use one consistent, keyboard-accessible control. */
  input::-ms-reveal, input::-ms-clear { display: none; }
  .reveal { position: absolute; inset-inline-end: .2rem; top: 50%; transform: translateY(-50%); display: grid; place-items: center; width: 2.75rem; height: 2.75rem; padding: 0; border: 0; border-radius: var(--atlas-radius); background: transparent; color: var(--atlas-text); cursor: pointer; }
  .reveal:focus-visible { outline: 2px solid var(--atlas-accent); outline-offset: -3px; }
</style>
