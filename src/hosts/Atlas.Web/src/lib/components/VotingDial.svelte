<script lang="ts">
  interface Props {
    label?: string;
    value?: number | null;
    average?: number | null;
    totalVotes?: number;
    compact?: boolean;
    disabled?: boolean;
    onChange?: (value: number) => void;
  }

  let {
    label = 'Node importance', value = null, average = null,
    totalVotes = 0, compact = false, disabled = false, onChange,
  }: Props = $props();

  const ratings = Array.from({ length: 11 }, (_, index) => 10 - index);
  let reel: HTMLDivElement;
  let dragging = $state(false);
  let lensPull = $state(0);
  let pulling = $state(false);
  let touchPointer: { id: number; y: number } | null = null;
  let drag: { id: number; y: number; top: number; moved: boolean } | null = null;
  let lastEmitted: number | null = null;
  let syncTarget: number | null = null;
  let lastLayout: boolean | undefined;
  let lastDisabled: boolean | undefined;
  let touchScrollOrigin: number | null = null;
  let wheelRemainder = 0;
  let wheelTime = 0;

  // Read the rendered row size so zoom and font-size changes keep the same threshold.
  function rowHeight() {
    return reel?.querySelector<HTMLElement>('.rating')?.getBoundingClientRect().height ?? 44;
  }

  // Caller-owned value; scroll-driven updates must not restart native touch inertia.
  $effect(() => {
    const next = value;
    const layout = compact;
    const locked = disabled;
    if (!reel) return;
    if (locked) {
      lensPull = 0;
      pulling = false;
      touchPointer = null;
      touchScrollOrigin = null;
    }
    if (locked && drag) {
      if (reel.hasPointerCapture(drag.id)) reel.releasePointerCapture(drag.id);
      drag = null;
      dragging = false;
    }
    if (next !== lastEmitted || layout !== lastLayout || locked !== lastDisabled || next === null) {
      syncTarget = (10 - (next ?? 5)) * rowHeight();
      reel.scrollTop = syncTarget;
    }
    lastEmitted = next;
    lastLayout = layout;
    lastDisabled = locked;
  });

  function select(next: number) {
    if (disabled) return;
    next = Math.max(0, Math.min(10, Math.round(next)));
    if (next === value) return;
    lastEmitted = next;
    onChange?.(next);
  }

  function moveTo(next: number) {
    if (disabled) return;
    next = Math.max(0, Math.min(10, next));
    syncTarget = (10 - next) * rowHeight();
    reel.scrollTo({ top: syncTarget, behavior: matchMedia('(prefers-reduced-motion: reduce)').matches ? 'instant' : 'smooth' });
    select(next);
  }

  function handleKey(event: KeyboardEvent) {
    if (disabled) return;
    if (['ArrowUp', 'ArrowRight', 'ArrowDown', 'ArrowLeft', 'Home', 'End'].includes(event.key)) {
      event.preventDefault();
      if (event.key === 'Home') moveTo(0);
      else if (event.key === 'End') moveTo(10);
      else moveTo(value === null ? 5 : value + (event.key === 'ArrowUp' || event.key === 'ArrowRight' ? 1 : -1));
    }
  }

  function handleScroll() {
    if (disabled) return;
    if (syncTarget !== null) {
      if (Math.abs(reel.scrollTop - syncTarget) < 1) syncTarget = null;
      return;
    }
    if (touchScrollOrigin !== null) {
      lensPull = Math.max(-10, Math.min(10, (touchScrollOrigin - reel.scrollTop) * .18));
    }
    select(10 - Math.round(reel.scrollTop / rowHeight()));
  }

  // Native touch scrolling supplies momentum and CSS snap. Mouse/pen dragging adds
  // the same direct manipulation on desktop without replacing touch physics.
  function pointerDown(event: PointerEvent) {
    if (disabled) return;
    syncTarget = null;
    if (event.button !== 0) return;
    pulling = true;
    if (event.pointerType === 'touch') {
      touchPointer = { id: event.pointerId, y: event.clientY };
      touchScrollOrigin = reel.scrollTop;
    }
    if (event.pointerType === 'touch') return;
    reel.focus({ preventScroll: true });
    drag = { id: event.pointerId, y: event.clientY, top: reel.scrollTop, moved: false };
    dragging = true;
    reel.setPointerCapture(event.pointerId);
  }

  function pointerMove(event: PointerEvent) {
    if (disabled) return;
    if (touchPointer?.id === event.pointerId) {
      lensPull = Math.max(-10, Math.min(10, (event.clientY - touchPointer.y) * .18));
      return;
    }
    if (!drag || drag.id !== event.pointerId) return;
    lensPull = Math.max(-10, Math.min(10, (event.clientY - drag.y) * .18));
    const distance = drag.y - event.clientY;
    if (Math.abs(distance) > 3) drag.moved = true;
    reel.scrollTop = drag.top + distance;
  }

  function pointerEnd(event: PointerEvent) {
    if (touchScrollOrigin !== null && !drag && event.type === 'lostpointercapture') return;
    if (touchPointer?.id === event.pointerId) {
      touchPointer = null;
      // Native panning cancels the pointer stream; keep the lens until scrollend.
      if (event.type === 'pointercancel' || Math.abs(reel.scrollTop - (touchScrollOrigin ?? reel.scrollTop)) > 1) return;
      touchScrollOrigin = null;
    }
    if (!drag || drag.id === event.pointerId) {
      lensPull = 0;
      pulling = false;
    }
    if (!drag || drag.id !== event.pointerId) return;
    const previous = drag;
    drag = null;
    dragging = false;
    if (reel.hasPointerCapture(event.pointerId)) reel.releasePointerCapture(event.pointerId);
    if (disabled) return;
    const offset = previous.moved || event.type !== 'pointerup' ? 0 : event.clientY - reel.getBoundingClientRect().top - reel.clientHeight / 2;
    moveTo(10 - Math.round((reel.scrollTop + offset) / rowHeight()));
  }

  function scrollEnd() {
    if (drag) return;
    touchScrollOrigin = null;
    lensPull = 0;
    pulling = false;
  }

  function handleWheel(event: WheelEvent) {
    if (disabled || event.ctrlKey || Math.abs(event.deltaX) > Math.abs(event.deltaY)) return;
    event.preventDefault();
    // Normalize line/page wheels and accumulate small trackpad deltas.
    if (event.timeStamp - wheelTime > 180) wheelRemainder = 0;
    wheelTime = event.timeStamp;
    wheelRemainder += event.deltaY * (event.deltaMode === 1 ? 16 : event.deltaMode === 2 ? reel.clientHeight : 1);
    const steps = Math.trunc(wheelRemainder / rowHeight());
    if (steps) {
      wheelRemainder -= steps * rowHeight();
      moveTo((value ?? 5) - steps);
    }
  }
</script>

<section class:compact class="voting-dial" aria-label={label}>
  <strong class="average" aria-label={average === null ? 'No average rating' : `Average rating: ${average.toFixed(1)}`}>{average === null ? '—' : average.toFixed(1)}</strong>
  <div class="selector" class:unrated={value === null} class:disabled>
    <div class="selection-window" class:pulling style:transform={`translateY(${lensPull}px)`} aria-hidden="true"></div>
    <div
      bind:this={reel} class="reel" class:dragging role="spinbutton" tabindex={disabled ? -1 : 0}
      aria-label={label} aria-valuemin={0} aria-valuemax={10}
      aria-valuenow={value === null ? undefined : value}
      aria-valuetext={value === null ? 'Unrated. Scroll, drag or use arrow keys to choose a rating.' : `${value} out of 10`}
      aria-disabled={disabled} onkeydown={handleKey} onscroll={handleScroll} onscrollend={scrollEnd}
      onpointerdown={pointerDown} onpointermove={pointerMove} onpointerup={pointerEnd}
      onpointercancel={pointerEnd} onlostpointercapture={pointerEnd} onwheel={handleWheel}
    >
      {#each ratings as rating}
        <span class="rating" class:selected={rating === (value ?? 5)} aria-hidden="true">{rating}</span>
      {/each}
    </div>
  </div>
  <span class="vote-count" role="img" aria-label={`${totalVotes} votes`}>
    <svg viewBox="0 0 56 44" aria-hidden="true">
      <circle cx="28" cy="7" r="6"/><circle cx="10" cy="12" r="5"/><circle cx="46" cy="12" r="5"/>
      <path d="M15 44V27a13 13 0 0 1 26 0v17ZM1 41V25a9 9 0 0 1 12-8v24ZM43 41V17a9 9 0 0 1 12 8v16Z"/>
    </svg>
    <span class="count" aria-hidden="true">{totalVotes.toLocaleString()}</span>
  </span>
</section>

<style>
  .voting-dial { display: flex; flex-direction: column; align-items: center; gap: .4rem; width: max-content; padding: .4rem; color: var(--atlas-text); }
  .average { display: grid; place-items: center; min-width: 3.5rem; padding: .2rem .4rem; border: 2px solid var(--atlas-border); border-radius: .25rem; font-size: 1.15rem; font-variant-numeric: tabular-nums; }
  .selector { --row-height: 2.75rem; position: relative; width: 3.5rem; border: 2px solid var(--atlas-accent); border-radius: .5rem; background: var(--atlas-bg); }
  .unrated { border-color: var(--atlas-border); }
  .selection-window { position: absolute; inset: var(--row-height) 0 auto; height: var(--row-height); background: var(--atlas-accent); pointer-events: none; border-radius: .2rem; box-shadow: inset 0 0 0 2px var(--atlas-accent); transition: transform 180ms ease-out; }
  .selection-window.pulling { transition: none; }
  .unrated .selection-window { background: var(--atlas-border); }
  .reel { position: relative; height: calc(var(--row-height) * 3); padding-block: var(--row-height); overflow-y: auto; scrollbar-width: none; scroll-snap-type: y mandatory; overscroll-behavior-y: contain; touch-action: pan-y; cursor: grab; user-select: none; border-radius: .35rem; }
  .reel::-webkit-scrollbar { display: none; }
  .reel.dragging { cursor: grabbing; scroll-snap-type: none; }
  .rating { display: grid; place-items: center; height: var(--row-height); scroll-snap-align: center; font-size: 1.3rem; font-weight: 600; font-variant-numeric: tabular-nums; color: var(--atlas-text); }
  .selected { color: var(--atlas-on-accent); }
  .unrated .selected { color: var(--atlas-surface); }
  .vote-count { position: relative; display: grid; place-items: center; min-width: 3.5rem; height: 2.75rem; }
  svg { position: absolute; width: 100%; height: 100%; fill: var(--atlas-accent); }
  .count { position: relative; margin-top: 1rem; padding: 0 .3rem; color: var(--atlas-on-accent); background: var(--atlas-accent); border-radius: .15rem; font-size: .85rem; font-weight: 700; font-variant-numeric: tabular-nums; }
  .disabled { opacity: .55; }
  .disabled .reel { overflow: hidden; cursor: default; touch-action: auto; }
  .reel:focus-visible { outline: 3px solid var(--atlas-focus); outline-offset: 4px; }
  @media (prefers-reduced-motion: reduce) { .selection-window { transition: none; } }
  .compact { flex-direction: row; }
  .compact .reel { height: var(--row-height); padding-block: 0; }
  .compact .selection-window { top: 0; }
</style>
