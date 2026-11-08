<script lang="ts">
  import { onMount } from 'svelte';
  import { apiRequest } from '$lib/api/apiRequest';
  import { parseSnapshot, type GlobeSnapshot } from '$lib/globe/presence';
  import { createGlobeRotation } from '$lib/globe/rotation.svelte';
  import VisitorGlobe from './VisitorGlobe.svelte';

  let dialog: HTMLDialogElement;
  let expanded = $state(false);
  let mode = $state<'live' | 'historical'>('live');
  let snapshot = $state<GlobeSnapshot>();
  let status = $state<'connecting' | 'connected' | 'offline'>('connecting');
  let consent = $state(false);
  let hasLocation = $state(false);
  let infoOpen = $state(false);
  let globeTop = $state(90);
  let globeBottom = $state(16);
  let globeSize = $state(360);
  const rotation = createGlobeRotation();
  let sharing = $state(false);
  let message = $state('');
  let alive = false;
  let source: EventSource | undefined;
  let request: AbortController | undefined;
  let generation = 0;
  let shareGeneration = 0;
  let reconnectTimer: ReturnType<typeof setTimeout> | undefined;
  let opener: HTMLElement | null = null;
  const summary = $derived(mode === 'live' ? snapshot?.live : snapshot?.historical);

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
  }
  function closed() {
    if (dialog.open) return;
    shareGeneration++;
    expanded = false;
    consent = false;
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
  async function connect() {
    const current = ++generation;
    clearTimeout(reconnectTimer);
    source?.close();
    request?.abort();
    request = new AbortController();
    status = 'connecting';
    try {
      const signal = AbortSignal.any([request.signal, AbortSignal.timeout(12000)]);
      const visit = () =>
        apiRequest<{ hasLocation: boolean }>(
          '/atlas-session-api',
          'globe/visit',
          'POST',
          undefined,
          signal,
        );
      // Serialize first-cookie creation across tabs of the same anonymous browser.
      const visitor = navigator.locks
        ? await navigator.locks.request('atlas-globe-visit', { signal }, visit)
        : await visit();
      if (!alive || current !== generation) return;
      hasLocation = visitor.hasLocation === true;
      const events = new EventSource('/atlas-session-api/globe/events');
      source = events;
      events.onmessage = (event) => {
        if (!alive || current !== generation) return;
        try {
          snapshot = parseSnapshot(event.data);
          status = 'connected';
        } catch {
          disconnected();
        }
      };
      events.onerror = disconnected;
      function disconnected() {
        if (!alive || current !== generation) return;
        events.close();
        status = 'offline';
        reconnectTimer = setTimeout(() => {
          void connect();
        }, 5000);
      }
    } catch {
      if (alive && current === generation) {
        status = 'offline';
        reconnectTimer = setTimeout(() => {
          void connect();
        }, 5000);
      }
    }
  }
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
    placeDialog();
    window.addEventListener('resize', placeDialog);
    window.addEventListener('scroll', placeDialog, { passive: true });
    document.addEventListener('click', outsideClick, true);
    document.addEventListener('keydown', escape, true);
    void connect();
    const restart = () => {
      message = '';
      consent = false;
      void connect();
    };
    window.addEventListener('atlas-session-changed', restart);
    return () => {
      alive = false;
      generation++;
      request?.abort();
      source?.close();
      clearTimeout(reconnectTimer);
      window.removeEventListener('atlas-session-changed', restart);
      window.removeEventListener('resize', placeDialog);
      window.removeEventListener('scroll', placeDialog);
      document.removeEventListener('click', outsideClick, true);
      document.removeEventListener('keydown', escape, true);
    };
  });
  async function share() {
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
      await apiRequest(
        '/atlas-session-api',
        'globe/location',
        'PUT',
        {
          longitude: Math.round(position.coords.longitude),
          latitude: Math.round(position.coords.latitude),
        },
        AbortSignal.timeout(12000),
      );
      if (alive && current === generation) {
        consent = false;
        hasLocation = true;
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
    sharing = true;
    const current = generation;
    try {
      await apiRequest(
        '/atlas-session-api',
        'globe/location',
        'PUT',
        { longitude: null, latitude: null },
        AbortSignal.timeout(12000),
      );
      if (alive && current === generation) {
        consent = false;
        hasLocation = false;
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

<button
  class="mini-launcher"
  type="button"
  aria-label="Open Atlas visitor globe"
  aria-haspopup="dialog"
  onclick={open}
>
  <VisitorGlobe
    mini
    angle={rotation.angle}
    pins={status === 'connected' ? snapshot?.live.pins : []}
  />
</button>

{#if expanded}<div class="body-shade" aria-hidden="true"></div>{/if}
<dialog
  bind:this={dialog}
  class="explorer"
  aria-label="Visitor globe"
  aria-modal="false"
  onclose={closed}
  style:top={`${globeTop}px`}
  style:bottom={`${globeBottom}px`}
  style:--globe-size={`${globeSize}px`}
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
    <VisitorGlobe angle={rotation.angle} pins={status === 'connected' ? summary?.pins : []} />
  </div>
  <div class="globe-caption">
    <span class="metric"
      ><strong>{status === 'connected' ? summary?.visitors : '—'}</strong>
      {summary?.visitors === 1 ? 'visitor' : 'visitors'}
      {mode === 'live' ? 'here now' : 'over time'}</span
    >
    <span class="connection" role="status"
      >{status === 'connected'
        ? 'Connected'
        : status === 'connecting'
          ? 'Connecting…'
          : 'Reconnecting…'}</span
    >
  </div>
  <div class="globe-actions">
    {#if hasLocation}
      <span class="shared-state">Location shared</span>
      <button type="button" disabled={sharing || status !== 'connected'} onclick={removeLocation}
        >Remove my shared location</button
      >
    {:else}
      <button
        type="button"
        disabled={sharing || status !== 'connected'}
        onclick={() => {
          consent = true;
          infoOpen = false;
          message = '';
        }}>Share my location on this globe</button
      >
    {/if}
    {#if status === 'offline'}<button type="button" onclick={() => void connect()}>Retry now</button
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
    <section class="disclosure consent" aria-label="Share location on this globe">
      <strong>Put a pin on this globe?</strong>
      <p>
        The next step opens your browser’s permission prompt. Only an approximate location is
        shared, without your name. It appears in Live while you are here and stays in Historical
        until you remove it.
      </p>
      <button
        class="primary"
        type="button"
        disabled={sharing || status !== 'connected'}
        onclick={share}>{sharing ? 'Sharing…' : 'Continue: share on this globe'}</button
      >
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
        Live shows visitors connected now. Historical counts visitors recorded over time{#if snapshot?.historical.since},
          since {new Date(snapshot.historical.since).toLocaleDateString()}{/if}. Accounts count once
        across browsers; anonymous visitors count by browser. Multiple tabs count once.
      </p>
      <p>
        Pins are optional, approximate locations. Connection status does not mean your location is
        shared.
      </p>
      <details class="locations">
        <summary
          >Shared locations ({status === 'connected'
            ? (summary?.pins.reduce((total, pin) => total + pin.visitors, 0) ?? 0)
            : 'unavailable'} visitors)</summary
        >
        {#if status !== 'connected'}<p>Waiting for the connection.</p>
        {:else if !summary?.pins.length}<p>No locations shared in this view yet.</p>
        {:else}<ul>
            {#each summary.pins as pin}<li>
                Near {pin.latitude}° latitude, {pin.longitude}° longitude: {pin.visitors}
                {pin.visitors === 1 ? 'visitor' : 'visitors'}
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
    .mini-launcher {
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
