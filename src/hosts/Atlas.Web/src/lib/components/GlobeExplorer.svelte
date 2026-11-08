<script lang="ts">
  import { onMount } from 'svelte';
  import { apiRequest } from '$lib/api/apiRequest';
  import { parseSnapshot, type GlobeSnapshot } from '$lib/globe/presence';
  import VisitorGlobe from './VisitorGlobe.svelte';

  let dialog: HTMLDialogElement;
  let expanded = $state(false);
  let mode = $state<'live' | 'historical'>('live');
  let snapshot = $state<GlobeSnapshot>();
  let status = $state<'connecting' | 'connected' | 'offline'>('connecting');
  let consent = $state(false);
  let sharing = $state(false);
  let message = $state('');
  let alive = false;
  let source: EventSource | undefined;
  let request: AbortController | undefined;
  let generation = 0;
  let shareGeneration = 0;
  let reconnectTimer: ReturnType<typeof setTimeout> | undefined;
  let scrollBefore = '';
  const summary = $derived(mode === 'live' ? snapshot?.live : snapshot?.historical);

  export function open() {
    if (!dialog.open) {
      scrollBefore = document.body.style.overflow;
      document.body.style.overflow = 'hidden';
      dialog.showModal();
      expanded = true;
    }
  }
  function closed() {
    shareGeneration++;
    expanded = false;
    consent = false;
    document.body.style.overflow = scrollBefore;
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
        apiRequest('/atlas-session-api', 'globe/visit', 'POST', undefined, signal);
      // Serialize first-cookie creation across tabs of the same anonymous browser.
      if (navigator.locks) await navigator.locks.request('atlas-globe-visit', { signal }, visit);
      else await visit();
      if (!alive || current !== generation) return;
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
  onMount(() => {
    alive = true;
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
      if (expanded) document.body.style.overflow = scrollBefore;
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
  <span class="mini-art"
    ><VisitorGlobe
      mini
      pins={status === 'connected' ? snapshot?.live.pins : []}
      animate={!expanded}
    /></span
  >
  <span
    ><strong>Atlas, together</strong><span class="mini-caption"
      >{status === 'connected'
        ? `${snapshot?.live.visitors} here now`
        : status === 'connecting'
          ? 'Connecting…'
          : 'Reconnecting…'}</span
    ></span
  >
</button>

<dialog bind:this={dialog} class="explorer" aria-labelledby="globe-title" onclose={closed}>
  <div class="explorer-shell">
    <header>
      <div>
        <p class="eyebrow">A world of perspectives</p>
        <h2 id="globe-title">Atlas, together</h2>
      </div>
      <button
        class="close"
        type="button"
        aria-label="Close visitor globe"
        onclick={() => dialog.close()}>✕</button
      >
    </header>
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
    <div class="explorer-content">
      <div class="world">
        <VisitorGlobe pins={status === 'connected' ? summary?.pins : []} animate={expanded} />
      </div>
      <section class="details" aria-label="Visitor details">
        <p class="metric">
          <strong>{status === 'connected' ? summary?.visitors : '—'}</strong><span
            >unique {summary?.visitors === 1 ? 'visitor' : 'visitors'}<br />{mode === 'live'
              ? 'here now'
              : 'since history began'}</span
          >
        </p>
        {#if mode === 'historical' && summary?.since}<p class="muted">
            Since {new Date(summary.since).toLocaleDateString()}
          </p>{/if}
        <p>
          {mode === 'live'
            ? 'Visitors exploring Atlas right now.'
            : 'Visitors recorded since this globe’s history began.'}
        </p>
        <details class="count-explanation">
          <summary>How visitors are counted</summary>
          <p class="muted">
            Counts use accounts when signed in, otherwise anonymous browsers. Multiple tabs count
            once. Pins only appear when visitors choose to share.
          </p>
        </details>
        <div class="connection" role="status">
          {#if status === 'offline'}Live connection unavailable. Reconnecting… <button
              type="button"
              onclick={() => void connect()}>Retry now</button
            >
          {:else if status === 'connecting'}Connecting to the visitor globe…
          {:else}<span class="live-dot" aria-hidden="true"></span> Connected{/if}
        </div>
        <div class="share-panel">
          <h3>Your place on the globe</h3>
          <p>
            Sharing is optional. An approximate pin appears in Live while you are here and stays in
            Historical until you remove it. Your name and exact location are never shown.
          </p>
          {#if consent}
            <p class="consent-copy">
              Allow location access for the Atlas globe? The next step opens your browser’s
              permission prompt. We send only a whole-degree location, which can be tens of miles
              from your actual position.
            </p>
            <button
              type="button"
              class="primary"
              disabled={sharing || status !== 'connected'}
              onclick={share}>{sharing ? 'Sharing…' : 'Continue: share on this globe'}</button
            >
            <button type="button" disabled={sharing} onclick={() => (consent = false)}
              >Cancel</button
            >
          {:else}
            <button
              type="button"
              class="primary"
              disabled={sharing || status !== 'connected'}
              onclick={() => {
                consent = true;
                message = '';
              }}>Share my location on this globe</button
            >
          {/if}
          <button
            type="button"
            class="remove"
            disabled={sharing || status !== 'connected'}
            onclick={removeLocation}>Remove my shared location</button
          >
          <p role="status">{message}</p>
        </div>
      </section>
    </div>
    <details class="locations">
      <summary
        >Shared locations ({status === 'connected'
          ? (summary?.pins.reduce((total, pin) => total + pin.visitors, 0) ?? 0)
          : 'unavailable'} visitors)</summary
      >
      {#if status !== 'connected'}<p>
          Waiting for the connection. Locations will appear when it returns.
        </p>
      {:else if !summary?.pins.length}<p>
          No locations shared in this view yet. You are welcome here with or without a pin.
        </p>
      {:else}<ul>
          {#each summary.pins as pin}<li>
              Near {pin.latitude}° latitude, {pin.longitude}° longitude: {pin.visitors}
              {pin.visitors === 1 ? 'visitor' : 'visitors'}
            </li>{/each}
        </ul>{/if}
    </details>
    <footer>
      Atlas visitor globe · <a
        href="https://www.naturalearthdata.com/about/terms-of-use/"
        target="_blank"
        rel="noreferrer">Map: Natural Earth</a
      >
    </footer>
  </div>
</dialog>

<style>
  .mini-launcher {
    position: fixed;
    bottom: 1.25rem;
    right: 1.25rem;
    z-index: 30;
    display: flex;
    align-items: center;
    gap: 0.55rem;
    padding: 0.35rem 1rem 0.35rem 0.35rem;
    border: 1px solid var(--atlas-border);
    border-radius: 4rem;
    background: var(--atlas-surface);
    color: var(--atlas-text);
    box-shadow: 0 4px 20px #0002;
    cursor: pointer;
    text-align: left;
    font: inherit;
  }
  .mini-art {
    width: 4rem;
    height: 4rem;
  }
  .mini-caption {
    display: block;
    font-size: 0.8rem;
    color: var(--atlas-muted);
  }
  .explorer {
    width: calc(100vw - 2rem);
    height: calc(100dvh - 2rem);
    max-width: 100rem;
    max-height: none;
    padding: 0;
    border: 1px solid #36566b;
    border-radius: 1.5rem;
    background: #0a1b2b;
    color: #edf5f4;
    box-sizing: border-box;
  }
  .explorer::backdrop {
    background: #03101bd9;
    backdrop-filter: blur(5px);
  }
  .explorer-shell {
    padding: clamp(1rem, 2vw, 2rem);
  }
  header {
    position: sticky;
    top: 0;
    z-index: 1;
    background: #0a1b2b;
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: 1rem;
  }
  h2 {
    font-size: clamp(1.8rem, 4vw, 3rem);
    margin: 0;
    letter-spacing: -0.04em;
  }
  .eyebrow {
    color: #9bcabb;
    font-size: 0.75rem;
    text-transform: uppercase;
    letter-spacing: 0.15em;
    margin: 0 0 0.5rem;
  }
  button {
    font: inherit;
    cursor: pointer;
    border-radius: 0.6rem;
    border: 1px solid #587485;
    color: #edf5f4;
    padding: 0.65rem 0.9rem;
    background: transparent;
  }
  button:disabled {
    opacity: 0.55;
    cursor: default;
  }
  button:focus-visible,
  a:focus-visible,
  summary:focus-visible {
    outline: 3px solid #ffd481;
    outline-offset: 3px;
  }
  .close {
    font-size: 1.25rem;
    padding: 0.4rem 0.75rem;
  }
  .view-controls {
    display: flex;
    gap: 0.25rem;
    margin: 1rem 0 0.5rem;
    padding: 0.25rem;
    background: #142d40;
    width: max-content;
    border-radius: 0.8rem;
  }
  .view-controls button {
    border-color: transparent;
    min-width: 7rem;
  }
  .view-controls button[aria-pressed='true'] {
    background: #cce9df;
    color: #092d2a;
  }
  .explorer-content {
    display: grid;
    grid-template-columns: minmax(0, 1.6fr) minmax(18rem, 1fr);
    gap: clamp(1rem, 4vw, 4rem);
    align-items: center;
  }
  .world {
    width: min(100%, calc(100dvh - 16rem));
    justify-self: center;
  }
  .details {
    max-width: 32rem;
  }
  .metric {
    display: flex;
    align-items: center;
    gap: 1rem;
    margin: 0.5rem 0;
  }
  .metric strong {
    font-size: clamp(3rem, 6vw, 5rem);
    font-weight: 500;
    line-height: 1;
  }
  .metric span,
  .muted {
    color: #b1c7d0;
    font-size: 0.9rem;
  }
  p {
    line-height: 1.6;
  }
  .connection {
    font-size: 0.85rem;
    margin: 1rem 0;
    color: #d7e7eb;
  }
  .live-dot {
    display: inline-block;
    width: 0.5rem;
    height: 0.5rem;
    background: #9bdfb6;
    border-radius: 50%;
    margin-right: 0.25rem;
  }
  .share-panel {
    background: #122b3b;
    padding: 1.2rem;
    border: 1px solid #345464;
    border-radius: 1rem;
  }
  .share-panel h3 {
    margin: 0;
    font-size: 1.1rem;
  }
  .share-panel p {
    font-size: 0.85rem;
    color: #c9dbdf;
  }
  .share-panel p:empty {
    display: none;
  }
  .share-panel button {
    margin-top: 0.5rem;
  }
  .primary {
    background: #cce9df;
    color: #092d2a;
    border-color: #cce9df;
  }
  .remove {
    display: block;
    font-size: 0.85rem;
    border-color: transparent;
    text-decoration: underline;
    padding-inline: 0;
  }
  .locations {
    margin-top: 1.5rem;
    border-top: 1px solid #345464;
    padding-top: 1rem;
    font-size: 0.9rem;
  }
  .locations ul {
    max-height: 12rem;
    overflow: auto;
  }
  footer {
    margin-top: 1.25rem;
    color: #b1c7d0;
    font-size: 0.75rem;
  }
  footer a {
    color: inherit;
  }
  @media (max-width: 560px) {
    .mini-launcher {
      display: none;
    }
  }
  @media (max-width: 700px) {
    .explorer {
      width: 100vw;
      height: 100dvh;
      border-radius: 0;
      max-width: none;
      margin: 0;
    }
    .explorer-shell {
      padding: max(1rem, env(safe-area-inset-top)) 1rem max(1rem, env(safe-area-inset-bottom));
    }
    .explorer-content {
      grid-template-columns: 1fr;
      gap: 0.5rem;
    }
    .world {
      width: min(100%, 45vh);
    }
    .details {
      max-width: none;
    }
  }
</style>
