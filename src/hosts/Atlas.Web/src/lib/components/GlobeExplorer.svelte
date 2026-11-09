<script lang="ts">
  import { onMount, type Snippet } from 'svelte';
  import { afterNavigate } from '$app/navigation';
  import { apiRequest } from '$lib/api/apiRequest';
  import { parseSnapshot, type GlobeSnapshot } from '$lib/globe/presence';
  import { createGlobeRotation } from '$lib/globe/rotation.svelte';
  import { pickLocation } from '$lib/globe/pickLocation';
  import VisitorGlobe from './VisitorGlobe.svelte';

  let { navigation }: { navigation?: Snippet<[Snippet]> } = $props();
  let mobile = $state(false);
  let dialog: HTMLDialogElement;
  let expanded = $state(false);
  let mode = $state<'live' | 'historical'>('live');
  let snapshot = $state<GlobeSnapshot>();
  let status = $state<'loading' | 'ready' | 'failed'>('loading');
  let canShare = $state(false);
  let consent = $state(false);
  let pinColor = $state('#b73539');
  let hasLocation = $state(false);
  let infoOpen = $state(false);
  let globeTop = $state(90);
  let globeBottom = $state(16);
  let globeSize = $state(360);
  let manual = $state(false);
  let chosenLatitude = $state<number | undefined>();
  let chosenLongitude = $state<number | undefined>();
  const picked = $derived(
    chosenLatitude !== undefined &&
      chosenLongitude !== undefined &&
      Number.isInteger(chosenLatitude) &&
      Number.isInteger(chosenLongitude) &&
      Math.abs(chosenLatitude) <= 90 &&
      Math.abs(chosenLongitude) <= 180
      ? { latitude: chosenLatitude, longitude: chosenLongitude }
      : null,
  );

  const rotation = createGlobeRotation(() => !(consent && manual));
  const summary = $derived(mode === 'live' ? snapshot?.live : snapshot?.historical);
  const previewPins = $derived([
    ...(status === 'ready' ? (summary?.pins ?? []) : []),
    ...(consent && manual && picked ? [{ ...picked, visitors: 1, color: pinColor }] : []),
  ]);
  let sharing = $state(false);
  let message = $state('');
  let alive = false;
  let request: AbortController | undefined;
  let generation = 0;
  let shareGeneration = 0;
  let pollTimer: ReturnType<typeof setInterval> | undefined;
  let requestNumber = 0;
  let loading = false;
  let opener: HTMLElement | null = null;

  export function open() {
    if (dialog.open) {
      closeGlobe();
      return;
    }
    placeDialog();
    opener = document.activeElement instanceof HTMLElement ? document.activeElement : null;
    // Non-modal dialog: the surrounding site stays interactive.
    dialog.show();
    expanded = true;
    void refresh();
  }
  function closed() {
    if (dialog.open) return;
    shareGeneration++;
    expanded = false;
    consent = false;
    manual = false;
    chosenLatitude = undefined;
    chosenLongitude = undefined;
    infoOpen = false;
  }
  function closeGlobe(restoreFocus = true) {
    if (!dialog.open) return;
    dialog.close();
    closed();
    if (restoreFocus && opener?.isConnected) opener.focus({ preventScroll: true });
  }
  function outsideClick(event: MouseEvent) {
    if (!expanded || !(event.target instanceof Node) || dialog.contains(event.target)) return;
    // Launchers toggle themselves; let header links and buttons receive this same click.
    if (
      event.target instanceof Element &&
      event.target.closest('[aria-label="Open Atlas visitor globe"]')
    )
      return;
    closeGlobe(false);
  }
  function escape(event: KeyboardEvent) {
    if (!expanded || event.key !== 'Escape') return;
    event.preventDefault();
    event.stopPropagation();
    closeGlobe();
  }
  function apply(value: unknown) {
    snapshot = parseSnapshot(value);
    hasLocation = snapshot.hasLocation;
    if (!consent && snapshot.pinColor) pinColor = snapshot.pinColor;
    canShare = snapshot.canShare;
    status = 'ready';
  }
  async function refresh(force = false) {
    if (!alive || document.hidden || sharing || (loading && !force)) return;
    const ticket = ++requestNumber;
    request?.abort();
    request = new AbortController();
    loading = true;
    if (!snapshot) status = 'loading';
    try {
      const signal = AbortSignal.any([request.signal, AbortSignal.timeout(12000)]);
      let view = await apiRequest<GlobeSnapshot>(
        '/atlas-session-api',
        canShare && hasLocation ? 'globe/activity' : 'globe/snapshot',
        canShare && hasLocation ? 'POST' : 'GET',
        undefined,
        signal,
      );
      if (!alive || ticket !== requestNumber) return;
      // Initial/session refresh learns the account state before recording any activity.
      if (!(canShare && hasLocation) && view.canShare && view.hasLocation)
        view = await apiRequest<GlobeSnapshot>(
          '/atlas-session-api',
          'globe/activity',
          'POST',
          undefined,
          signal,
        );
      if (alive && ticket === requestNumber) apply(view);
    } catch {
      if (alive && ticket === requestNumber) status = 'failed';
    } finally {
      if (ticket === requestNumber) loading = false;
    }
  }
  function cancelRead() {
    requestNumber++;
    request?.abort();
    loading = false;
  }
  afterNavigate(({ from, to }) => {
    if (!from || !alive) return;
    if (to?.url.pathname === '/login') closeGlobe(false);
    void refresh();
  });
  function placeDialog() {
    const height = window.innerHeight;
    const header = document.querySelector('.atlas-header')?.getBoundingClientRect();
    const navigation = document.querySelector('.bottom-navigation')?.getBoundingClientRect();
    globeTop = Math.max(16, (header?.bottom ?? 64) + 12);
    globeBottom =
      navigation && navigation.height > 0 ? Math.max(16, height - navigation.top + 12) : 16;
    globeSize = Math.max(
      90,
      Math.min(400, window.innerWidth - 64, height - globeTop - globeBottom - 174),
    );
  }
  onMount(() => {
    alive = true;
    const mobileQuery = matchMedia('(max-width: 560px)');
    const updateMobile = () => (mobile = mobileQuery.matches);
    updateMobile();
    mobileQuery.addEventListener('change', updateMobile);
    placeDialog();
    window.addEventListener('resize', placeDialog);
    window.addEventListener('scroll', placeDialog, { passive: true });
    document.addEventListener('click', outsideClick, true);
    document.addEventListener('keydown', escape, true);
    void refresh();
    const restart = () => {
      generation++;
      cancelRead();
      message = '';
      consent = false;
      canShare = false;
      hasLocation = false;
      void refresh(true);
    };
    const visible = () => {
      if (!document.hidden) void refresh();
    };
    document.addEventListener('visibilitychange', visible);
    pollTimer = setInterval(() => {
      void refresh();
    }, 60000);
    window.addEventListener('atlas-session-changed', restart);
    return () => {
      mobileQuery.removeEventListener('change', updateMobile);
      alive = false;
      generation++;
      request?.abort();
      clearInterval(pollTimer);
      document.removeEventListener('visibilitychange', visible);
      window.removeEventListener('atlas-session-changed', restart);
      window.removeEventListener('resize', placeDialog);
      window.removeEventListener('scroll', placeDialog);
      document.removeEventListener('click', outsideClick, true);
      document.removeEventListener('keydown', escape, true);
    };
  });
  function chooseSpot(event: MouseEvent) {
    if (!manual || !consent || sharing) return;
    const rect = (event.currentTarget as HTMLElement).getBoundingClientRect();
    const point = pickLocation(
      event.detail === 0 ? rect.width / 2 : event.clientX - rect.left,
      event.detail === 0 ? rect.height / 2 : event.clientY - rect.top,
      rect.width,
      rotation.angle,
    );
    if (point) {
      chosenLatitude = point.latitude;
      chosenLongitude = point.longitude;
    }
  }
  async function savePicked() {
    if (!picked || !canShare || !consent || sharing) return;
    cancelRead();
    sharing = true;
    const current = generation;
    const ticket = ++shareGeneration;
    try {
      const updated = await apiRequest<GlobeSnapshot>(
        '/atlas-session-api',
        'globe/location',
        'PUT',
        { ...picked, color: pinColor },
        AbortSignal.timeout(12000),
      );
      if (alive && current === generation && ticket === shareGeneration && expanded && consent) {
        apply(updated);
        consent = false;
        manual = false;
        chosenLatitude = undefined;
        chosenLongitude = undefined;
        message = 'Your chosen approximate location is shared on the Atlas globe.';
      }
    } catch {
      if (alive && current === generation && ticket === shareGeneration)
        message = 'Your pin could not be saved. Please try again.';
    } finally {
      sharing = false;
    }
  }
  async function share() {
    if (!canShare) {
      message = 'Sign in to place a pin.';
      return;
    }
    cancelRead();
    if (!navigator.geolocation) {
      message = 'Location sharing is unavailable in this browser.';
      return;
    }
    sharing = true;
    message = 'Waiting for your browser’s location permission for the Atlas globe…';
    const current = generation;
    const sharingTicket = ++shareGeneration;
    try {
      const position = await new Promise<GeolocationPosition>((resolve, reject) => {
        navigator.geolocation.getCurrentPosition(resolve, reject, {
          enableHighAccuracy: false,
          timeout: 15000,
          maximumAge: 300000,
        });
      });
      if (
        !alive ||
        current !== generation ||
        sharingTicket !== shareGeneration ||
        !expanded ||
        !consent
      )
        return;
      // Exact coordinates stay in this browser; only whole-degree positions are sent.
      const updated = await apiRequest<GlobeSnapshot>(
        '/atlas-session-api',
        'globe/location',
        'PUT',
        {
          longitude: Math.round(position.coords.longitude),
          latitude: Math.round(position.coords.latitude),
          color: pinColor,
        },
        AbortSignal.timeout(12000),
      );
      if (alive && current === generation) {
        consent = false;
        apply(updated);
        message =
          'Your approximate location is shared on the Atlas globe. You can remove it at any time.';
      }
    } catch (error) {
      if (alive && current === generation)
        message =
          typeof error === 'object' && error !== null && 'code' in error
            ? 'Location was not shared. Permission may be denied, or your location could not be found. You can continue without a pin.'
            : 'Your location could not be saved. Please try again.';
    } finally {
      sharing = false;
    }
  }
  async function removeLocation() {
    cancelRead();
    sharing = true;
    const current = generation;
    try {
      const updated = await apiRequest<GlobeSnapshot>(
        '/atlas-session-api',
        'globe/location',
        'PUT',
        { longitude: null, latitude: null },
        AbortSignal.timeout(12000),
      );
      if (alive && current === generation) {
        consent = false;
        apply(updated);
        message = 'Your location was removed from both Live and Historical.';
      }
    } catch {
      if (alive && current === generation)
        message = 'Your location could not be removed. Please retry.';
    } finally {
      sharing = false;
    }
  }
</script>

{#snippet launcher(inNavigation: boolean)}
  <button
    class="mini-launcher"
    class:in-navigation={inNavigation}
    type="button"
    aria-label="Open Atlas visitor globe"
    aria-haspopup="dialog"
    onclick={open}
  >
    <VisitorGlobe
      mini
      angle={rotation.angle}
      pins={status === 'ready' ? snapshot?.live.pins : []}
    />
  </button>
{/snippet}

{#snippet mobileLauncher()}
  {#if mobile}{@render launcher(true)}{/if}
{/snippet}

{@render navigation?.(mobileLauncher)}
{#if !mobile || !navigation}{@render launcher(false)}{/if}

{#if expanded}<div class="body-shade" aria-hidden="true"></div>{/if}
<dialog
  bind:this={dialog}
  class="explorer"
  class:editing={consent && manual}
  aria-label="Visitor globe"
  aria-modal="false"
  onclose={closed}
  style:top={`${globeTop}px`}
  style:bottom={`${globeBottom}px`}
  style:--globe-size={`${consent && manual ? Math.min(globeSize, 240) : globeSize}px`}
  style:max-height={consent && manual ? `calc(100dvh - ${globeTop + globeBottom}px)` : 'none'}
>
  <div class="toolbar">
    <div class="view-controls" role="group" aria-label="Globe view">
      <button type="button" aria-pressed={mode === 'live'} onclick={() => (mode = 'live')}
        >Live</button
      >
      <button
        type="button"
        aria-pressed={mode === 'historical'}
        onclick={() => (mode = 'historical')}>Historical</button
      >
    </div>
    <button
      class="close"
      type="button"
      aria-label="Close visitor globe"
      onclick={() => closeGlobe()}>✕</button
    >
  </div>
  <div class="world">
    {#if expanded}<VisitorGlobe angle={rotation.angle} pins={previewPins} />{/if}
    {#if consent && manual}<button
        class="pick-surface"
        type="button"
        aria-label="Choose a location on the globe"
        onclick={chooseSpot}
        disabled={sharing}
      ></button>{/if}
  </div>
  <div class="globe-caption">
    <span class="metric"
      ><strong>{status === 'ready' ? summary?.visitors : '—'}</strong>
      {summary?.visitors === 1 ? 'account' : 'accounts'}
      {mode === 'live' ? 'recently active' : 'with saved pins'}</span
    >
    <span class="connection" role="status"
      >{status === 'ready'
        ? 'Updated ' +
          new Date(snapshot!.capturedAt).toLocaleTimeString([], {
            hour: '2-digit',
            minute: '2-digit',
          })
        : status === 'loading'
          ? 'Loading…'
          : 'Update unavailable'}</span
    >
  </div>
  <div class="globe-actions">
    {#if !canShare}
      <a class="sign-in" href="/login">Sign in to place a pin</a>
    {:else if hasLocation}
      <span class="shared-state">Location shared</span>
      <button
        type="button"
        disabled={sharing || status !== 'ready'}
        onclick={() => {
          consent = true;
          rotation.angle = ((((rotation.angle + 180) % 360) + 360) % 360) - 180;
          manual = true;
          chosenLatitude = undefined;
          chosenLongitude = undefined;
          infoOpen = false;
          message = '';
        }}>Edit my pin</button
      >
      <button type="button" disabled={sharing || status !== 'ready'} onclick={removeLocation}
        >Remove my shared location</button
      >
    {:else}
      <button
        type="button"
        disabled={sharing || status !== 'ready'}
        onclick={() => {
          consent = true;
          manual = false;
          chosenLatitude = undefined;
          chosenLongitude = undefined;
          infoOpen = false;
          message = '';
        }}>Share my location on this globe</button
      >
    {/if}
    {#if status === 'failed'}<button type="button" onclick={() => void refresh()}>Retry now</button
      >{/if}
    <button
      class="info-toggle"
      type="button"
      aria-label="About this globe"
      aria-expanded={infoOpen}
      onclick={() => {
        infoOpen = !infoOpen;
        consent = false;
      }}>ⓘ</button
    >
  </div>
  <p class="feedback" role="status">{message}</p>
  {#if consent}
    <section class="disclosure consent" class:manual aria-label="Share location on this globe">
      <strong>Put a pin on this globe?</strong>
      <div class="pin-choice">
        <label>Pin color <input type="color" bind:value={pinColor} disabled={sharing} /></label>
        <span class="pin-preview" role="img" aria-label={`Your pin preview: ${pinColor}`}>
          <span class="preview-head" style:background={pinColor}></span><span class="preview-shaft"
          ></span>
        </span>
      </div>
      {#if manual}
        <p>
          Choose an approximate spot on the globe. No browser location permission is needed. This
          replaces your saved pin only when you save.
        </p>
        <label class="turn-globe"
          >Turn globe <input
            type="range"
            min="-180"
            max="180"
            step="1"
            bind:value={rotation.angle}
            disabled={sharing}
          /></label
        >
        <div class="coordinate-fields">
          <label
            >Latitude <input
              type="number"
              min="-90"
              max="90"
              step="1"
              bind:value={chosenLatitude}
              disabled={sharing}
            /></label
          >
          <label
            >Longitude <input
              type="number"
              min="-180"
              max="180"
              step="1"
              bind:value={chosenLongitude}
              disabled={sharing}
            /></label
          >
        </div>
        <button
          class="primary"
          type="button"
          disabled={!picked || sharing || status !== 'ready'}
          onclick={savePicked}>{sharing ? 'Saving…' : 'Save this pin'}</button
        >
        <button
          type="button"
          disabled={sharing}
          onclick={() => {
            manual = false;
            chosenLatitude = undefined;
            chosenLongitude = undefined;
          }}>Use browser location instead</button
        >
      {:else}
        <p>
          The next step opens your browser’s permission prompt. Only an approximate location is
          shared, without your name. It appears in Live while recently active and stays in
          Historical until you remove it.
        </p>
        <button
          class="primary"
          type="button"
          disabled={sharing || status !== 'ready'}
          onclick={share}>{sharing ? 'Sharing…' : 'Continue: share on this globe'}</button
        >
        <button
          type="button"
          disabled={sharing}
          onclick={() => {
            rotation.angle = ((((rotation.angle + 180) % 360) + 360) % 360) - 180;
            manual = true;
            chosenLatitude = undefined;
            chosenLongitude = undefined;
          }}>Choose a spot on the globe instead</button
        >
      {/if}
      <button type="button" disabled={sharing} onclick={() => (consent = false)}>Cancel</button>
    </section>
  {/if}
  {#if infoOpen}
    <section class="disclosure information" aria-label="Globe information">
      <div class="info-heading">
        <strong>About this globe</strong><button
          type="button"
          aria-label="Close globe information"
          onclick={() => (infoOpen = false)}>✕</button
        >
      </div>
      <p>
        Historical shows accounts that chose to save a pin. Live shows those accounts active within
        the last {snapshot?.onlineWindowMinutes ?? 5} minutes. Each account counts once, across tabs and
        devices. Anonymous visitors are not included.
      </p>
      <p>
        The map refreshes about once a minute while this page is visible. Leaving or signing out can
        take up to five minutes to drop out of Live. Pins stay in Historical until removed. Only
        approximate locations are shown, without account names.
      </p>
      <details class="locations">
        <summary
          >Shared locations ({status === 'ready'
            ? (summary?.pins.reduce((total, pin) => total + pin.visitors, 0) ?? 0)
            : 'unavailable'} accounts)</summary
        >
        {#if status !== 'ready'}<p>The latest snapshot is unavailable. Try refreshing.</p>
        {:else if !summary?.pins.length}<p>No locations shared in this view yet.</p>
        {:else}<ul>
            {#each summary.pins as pin}<li>
                Near {pin.latitude}° latitude, {pin.longitude}° longitude: {pin.visitors}
                {pin.visitors === 1 ? 'account' : 'accounts'}
              </li>{/each}
          </ul>{/if}
      </details>
      <p class="credit">
        Map data: <a
          href="https://www.naturalearthdata.com/about/terms-of-use/"
          target="_blank"
          rel="noreferrer">Natural Earth</a
        > (public domain).
      </p>
    </section>
  {/if}
</dialog>

<style>
  .pick-surface {
    position: absolute;
    inset: 0;
    width: 100%;
    height: 100%;
    padding: 0;
    border: 0;
    background: transparent;
    cursor: crosshair;
    border-radius: 50%;
  }
  .pick-surface:focus-visible {
    outline: 2px solid var(--atlas-accent);
  }
  .explorer.editing {
    overflow: auto;
  }
  .editing .toolbar {
    position: sticky;
    top: 0;
    z-index: 3;
    background: var(--atlas-surface);
  }
  .editing .globe-caption,
  .editing .globe-actions {
    display: none;
  }
  .disclosure.manual {
    max-height: none;
    overflow: visible;
    position: static;
    margin: 0.75rem;
  }
  .turn-globe {
    display: flex;
    align-items: center;
    gap: 0.5rem;
  }
  .coordinate-fields {
    display: flex;
    flex-wrap: wrap;
    gap: 0.75rem;
    margin: 0.75rem 0;
  }
  .coordinate-fields label {
    display: flex;
    gap: 0.5rem;
    align-items: center;
  }
  .coordinate-fields input {
    width: 5rem;
  }

  .pin-choice {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 1.5rem;
    margin: 0.75rem 0;
  }
  .pin-choice label {
    display: flex;
    align-items: center;
    gap: 0.75rem;
  }
  .pin-choice input {
    width: 3rem;
    height: 2.5rem;
    padding: 0.15rem;
  }
  .pin-preview {
    display: flex;
    align-items: center;
    flex-direction: column;
    transform: rotate(15deg);
  }
  .preview-head {
    width: 1.1rem;
    height: 1.1rem;
    border-radius: 50%;
    box-shadow: inset -2px -2px 3px #0003;
  }
  .preview-shaft {
    width: 2px;
    height: 1.5rem;
    background: #485453;
  }

  .mini-launcher {
    position: fixed;
    bottom: 1.25rem;
    right: 1.25rem;
    z-index: 30;
    width: 4.25rem;
    height: 4.25rem;
    padding: 0.1rem;
    border: 1px solid var(--atlas-border);
    border-radius: 50%;
    background: var(--atlas-surface);
    cursor: pointer;
    box-shadow: 0 3px 10px #172b3822;
  }
  .mini-launcher.in-navigation {
    position: relative;
    inset: auto;
    width: 2.75rem;
    height: 2.75rem;
    padding: 0;
    border: 0;
    background: transparent;
    box-shadow: none;
  }
  .mini-launcher.in-navigation :global(.globe-art) {
    /* The Earth disc fills 80% of the canvas. Leave room for raised pins. */
    position: absolute;
    top: 50%;
    left: 50%;
    width: 125%;
    transform: translate(-50%, -50%);
    pointer-events: none;
  }
  .body-shade {
    position: fixed;
    inset: 0;
    width: 100vw;
    height: 100dvh;
    z-index: 36;
    background: #172b3817;
    pointer-events: none;
  }
  .explorer {
    position: fixed;
    z-index: 40;
    width: min(32rem, calc(100vw - 2rem));
    height: fit-content;
    max-width: none;
    max-height: none;
    margin: auto;
    padding: 0.75rem;
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    color: var(--atlas-text);
    background: var(--atlas-surface);
    box-shadow: 0 8px 30px #172b3830;
    overflow: visible;
  }
  .toolbar {
    display: flex;
    justify-content: space-between;
    align-items: center;
    gap: 0.5rem;
  }
  button {
    color: var(--atlas-text);
    font: inherit;
    font-size: 0.85rem;
    cursor: pointer;
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    background: var(--atlas-surface);
    padding: 0.4rem 0.65rem;
  }
  button:disabled {
    opacity: 0.55;
    cursor: default;
  }
  .view-controls {
    display: flex;
    gap: 0.15rem;
    padding: 0.15rem;
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
  }
  .view-controls button {
    border-color: transparent;
    padding: 0.3rem 0.65rem;
  }
  .view-controls button[aria-pressed='true'],
  .primary {
    color: var(--atlas-on-accent);
    background: var(--atlas-accent);
  }
  .close {
    border-color: transparent;
    font-size: 1.1rem;
  }
  .world {
    position: relative;
    width: min(100%, var(--globe-size));
    margin: 0.25rem auto;
    aspect-ratio: 1;
  }
  .globe-caption {
    display: flex;
    justify-content: center;
    align-items: center;
    flex-wrap: wrap;
    gap: 0.3rem 0.8rem;
    font-size: 0.85rem;
  }
  .connection {
    font-size: 0.75rem;
    color: var(--atlas-muted);
  }
  .globe-actions {
    display: flex;
    justify-content: center;
    align-items: center;
    flex-wrap: wrap;
    gap: 0.4rem;
    margin-top: 0.6rem;
  }
  .sign-in {
    font-size: 0.8rem;
    padding: 0.4rem;
  }
  .globe-actions > button {
    font-size: 0.8rem;
  }
  .shared-state {
    font-size: 0.8rem;
    color: var(--atlas-muted);
  }
  .info-toggle {
    border-color: transparent;
    font-size: 1.1rem !important;
    padding: 0.25rem;
  }
  .feedback {
    margin: 0.4rem 0 0;
    font-size: 0.8rem;
    text-align: center;
    color: var(--atlas-muted);
    line-height: 1.35;
  }
  .feedback:empty {
    display: none;
  }
  .disclosure {
    position: absolute;
    z-index: 1;
    inset: auto 0.75rem 0.75rem;
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    background: var(--atlas-surface);
    box-shadow: 0 4px 20px #172b382d;
    padding: 1rem;
    font-size: 0.85rem;
    line-height: 1.45;
    max-height: calc(100% - 3.5rem);
    overflow: auto;
  }
  .disclosure p {
    margin: 0.65rem 0;
  }
  .info-heading {
    display: flex;
    justify-content: space-between;
    align-items: center;
  }
  .info-heading button {
    padding: 0.2rem 0.45rem;
    border-color: transparent;
  }
  .locations ul {
    max-height: 7rem;
    overflow: auto;
    padding-left: 1.2rem;
  }
  .credit {
    font-size: 0.75rem;
    color: var(--atlas-muted);
  }
  @media (max-width: 560px) {
    .mini-launcher:not(.in-navigation) {
      display: none;
    }
    .explorer {
      width: calc(100vw - 1.5rem);
    }
    .globe-actions {
      gap: 0.25rem;
    }
  }
</style>
