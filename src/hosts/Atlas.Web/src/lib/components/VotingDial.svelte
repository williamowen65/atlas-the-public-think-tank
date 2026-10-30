<!--
  @component
  VotingDial: Controlled, presentational importance dial with wheel, drag, touch and keyboard interactions.
  Receives value/average/totalVotes from its parent and emits selections through onChange.
  It can be interactive, but never calls the API or calculates community totals.
  Use LiveVotingDial for persistence, confirmation, undo and session handling.
  disabled is a caller-supplied interaction lock, not an authentication check.

  **Props**
  - `label` (`string`; optional; default `'Node importance'`): Accessible name for the rating
    spinbutton, such as Node importance. It does not label a save button.
  - `value` (`number | null`; optional; default `null`): Current controlled rating, 0–10. null means
    unrated; zero is a real rating.
  - `average` (`number | null`; optional; default `null`): Community average supplied by the parent;
    null means no average is available.
  - `totalVotes` (`number`; optional; default `0`): Community vote count supplied by the parent.
  - `hasVoted` (`boolean`; optional): Controls voted styling; when omitted, defaults to whether
    value is non-null.
  - `compact` (`boolean`; optional; default `false`): Selects the compact dial layout.
  - `disabled` (`boolean`; optional; default `false`): Prevents interaction with this control.
  - `onCommit` (`(value: number) => void`; optional): Requests confirmation after pointer/wheel settling or Enter.
  - `onChange` (`(value: number) => void`; optional): Callback: Receives the selected 0–10 rating.
    This control is presentational; the caller handles acceptance or persistence.
-->
<script lang="ts">
  import { formatVoteCount } from '../format/formatVoteCount';
  interface Props {
    /**
     * Accessible name for the rating spinbutton, such as Node importance. It does not label a save
     * button.
     */
    label?: string;
    /** Current controlled rating, 0–10. null means unrated; zero is a real rating. */
    value?: number | null;
    /** Community average supplied by the parent; null means no average is available. */
    average?: number | null;
    /** Community vote count supplied by the parent. */
    totalVotes?: number;
    /** Controls voted styling; when omitted, defaults to whether value is non-null. */
    hasVoted?: boolean;
    /** Selects the compact dial layout. */
    compact?: boolean;
    /** Prevents interaction with this control. */
    disabled?: boolean;
    /**
     * Callback: Receives the selected 0–10 rating. This control is presentational; the caller
     * handles acceptance or persistence.
     */
    onChange?: (value: number) => void;
    /** Request review after pointer/wheel settling or Enter; never save during adjustment. */
    onCommit?: (value: number) => void;
    /** Restore caller-owned preview state if a pointer gesture is cancelled. */
    onCancel?: () => void;
  }

  let {
    label = 'Node importance',
    value = null,
    average = null,
    totalVotes = 0,
    hasVoted,
    compact = false,
    disabled = false,
    onChange,
    onCommit,
    onCancel,
  }: Props = $props();

  /**
   * Whether to show the voted treatment, using hasVoted when supplied or the selected
   * value otherwise.
   */
  const marked = $derived(hasVoted ?? value !== null);
  const ratings = Array.from({ length: 11 }, (_, index) => 10 - index);
  let reel: HTMLDivElement;
  /**
   * Whether pointer dragging is active on the rating reel.
   */
  let dragging = $state(false);
  /**
   * Vertical pixel offset applied to the noncompact selection lens during interaction.
   */
  let lensPull = $state(0);
  /**
   * Whether the lens is in its pulled interaction state.
   */
  let pulling = $state(false);
  let drag: { id: number; y: number; top: number; moved: boolean } | null = null;
  let lastEmitted: number | null = null;
  let syncTarget: number | null = null;
  let lastLayout: boolean | undefined;
  let lastDisabled: boolean | undefined;
  let gestureStart: number | null = null;
  let velocity = 0;
  let lastPointerY = 0;
  let lastPointerTime = 0;
  let coasting = $state(false);
  let motionFrame = 0;
  let reviewFrame = 0;
  let reviewVersion = 0;
  let lastMotion = 0;
  let touchGesture = false;
  let wheelRemainder = 0;
  let wheelTime = 0;
  let wheelPullDistance = 0;
  let wheelRelease: ReturnType<typeof setTimeout> | undefined;

  $effect(() => () => {
    clearTimeout(wheelRelease);
    stopMotion();
  });

  /** Cancel queued review and animation when the visitor starts a new adjustment. */
  function stopMotion() {
    reviewVersion++;
    cancelAnimationFrame(reviewFrame);
    cancelAnimationFrame(motionFrame);
    coasting = false;
  }

  /**
   * Review only after motion has finished and a further 450ms quiet interval.
   * New input invalidates this request; this never writes a vote to the API.
   */
  function reviewWhenSettled() {
    const version = ++reviewVersion;
    cancelAnimationFrame(reviewFrame);
    lastMotion = performance.now();
    const check = (now: number) => {
      if (version !== reviewVersion || disabled) return;
      if (syncTarget !== null && Math.abs(reel.scrollTop - syncTarget) < 1) syncTarget = null;
      if (drag || coasting || wheelRelease !== undefined || syncTarget !== null)
        lastMotion = now;
      if (now - lastMotion >= 450) {
        onCommit?.(Math.max(0, Math.min(10, 10 - Math.round(reel.scrollTop / rowHeight()))));
        return;
      }
      reviewFrame = requestAnimationFrame(check);
    };
    reviewFrame = requestAnimationFrame(check);
  }

  /**
   * Ease a released touch swipe toward its projected whole-number destination.
   * @param speed Recent reel velocity in pixels per millisecond; pauses remove momentum.
   */
  function coast(speed: number) {
    const start = reel.scrollTop;
    const maximum = 10 * rowHeight();
    const projected = Math.max(0, Math.min(maximum, start + speed * 180));
    const target = Math.round(projected / rowHeight()) * rowHeight();
    const duration = Math.min(850, 350 + Math.abs(target - start) * 1.2);
    const began = performance.now();
    syncTarget = null;
    coasting = true;
    const step = (now: number) => {
      const progress = Math.min(1, (now - began) / duration);
      reel.scrollTop = start + (target - start) * (1 - Math.pow(1 - progress, 3));
      select(10 - Math.round(reel.scrollTop / rowHeight()));
      if (progress < 1) motionFrame = requestAnimationFrame(step);
      else {
        coasting = false;
        reel.scrollTop = target;
        select(10 - Math.round(target / rowHeight()));
        reviewWhenSettled();
      }
    };
    motionFrame = requestAnimationFrame(step);
  }

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
      stopMotion();
      clearTimeout(wheelRelease);
      wheelRelease = undefined;
      wheelPullDistance = 0;
      lensPull = 0;
      pulling = false;
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

  // Clamp all gesture inputs to whole ratings and emit changes to the parent rather than saving here.
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
    reel.scrollTo({
      top: syncTarget,
      behavior: matchMedia('(prefers-reduced-motion: reduce)').matches ? 'instant' : 'smooth',
    });
    select(next);
  }

  // Map keyboard navigation to the same selection path used by pointer and wheel input.
  function handleKey(event: KeyboardEvent) {
    if (disabled) return;
    stopMotion();
    clearTimeout(wheelRelease);
    wheelRelease = undefined;
    if (event.key === 'Enter') {
      event.preventDefault();
      if (!event.repeat && value !== null) onCommit?.(value);
      return;
    }
    if (['ArrowUp', 'ArrowRight', 'ArrowDown', 'ArrowLeft', 'Home', 'End'].includes(event.key)) {
      event.preventDefault();
      if (event.key === 'Home') moveTo(0);
      else if (event.key === 'End') moveTo(10);
      else
        moveTo(
          value === null
            ? 5
            : value + (event.key === 'ArrowUp' || event.key === 'ArrowRight' ? 1 : -1),
        );
    }
  }

  // Standard lens follows the pull; compact keeps a fixed selection window.
  function pullLens(distance: number) {
    lensPull = compact ? 0 : Math.max(-16, Math.min(16, distance * 0.3));
  }

  function handleScroll() {
    if (disabled) return;
    lastMotion = performance.now();
    if (syncTarget !== null) {
      if (Math.abs(reel.scrollTop - syncTarget) < 1) syncTarget = null;
      return;
    }
    select(10 - Math.round(reel.scrollTop / rowHeight()));
  }

  /**
   * Begin a captured pointer gesture for mouse, pen or touch.
   * Captured touch permits controlled momentum after release without premature review.
   * @param event Pointer press on the reel; only the primary button starts adjustment.
   */
  function pointerDown(event: PointerEvent) {
    if (disabled) return;
    stopMotion();
    clearTimeout(wheelRelease);
    wheelRelease = undefined;
    wheelPullDistance = 0;
    syncTarget = null;
    if (event.button !== 0) return;
    pulling = true;
    gestureStart = value;
    touchGesture = event.pointerType === 'touch';
    velocity = 0;
    lastPointerY = event.clientY;
    lastPointerTime = event.timeStamp;
    reel.focus({ preventScroll: true });
    drag = { id: event.pointerId, y: event.clientY, top: reel.scrollTop, moved: false };
    dragging = true;
    reel.setPointerCapture(event.pointerId);
  }

  function pointerMove(event: PointerEvent) {
    if (disabled) return;
    if (!drag || drag.id !== event.pointerId) return;
    const elapsed = event.timeStamp - lastPointerTime;
    if (elapsed > 0) velocity = Math.max(-3, Math.min(3, (lastPointerY - event.clientY) / elapsed));
    lastPointerY = event.clientY;
    lastPointerTime = event.timeStamp;
    pullLens(event.clientY - drag.y);
    const distance = drag.y - event.clientY;
    if (Math.abs(distance) > 3) drag.moved = true;
    reel.scrollTop = drag.top + distance;
  }

  function pointerEnd(event: PointerEvent) {
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
    const offset =
      previous.moved || event.type !== 'pointerup'
        ? 0
        : event.clientY - reel.getBoundingClientRect().top - reel.clientHeight / 2;
    if (event.type !== 'pointerup') {
      // Cancellation/lost capture never requests confirmation.
      lastEmitted = gestureStart;
      if (onCancel) onCancel();
      else onChange?.(gestureStart ?? 5);
      return;
    }
    const next = Math.max(0, Math.min(10, 10 - Math.round((reel.scrollTop + offset) / rowHeight())));
    if (touchGesture && previous.moved && !matchMedia('(prefers-reduced-motion: reduce)').matches) {
      coast(event.timeStamp - lastPointerTime < 100 ? velocity : 0);
    } else {
      moveTo(next);
      reviewWhenSettled();
    }
  }

  function scrollEnd() {
    if (drag || wheelRelease !== undefined) return;
    lensPull = 0;
    pulling = false;
  }

  // Accumulate wheel movement into rating steps while the lens provides temporary visual feedback.
  function handleWheel(event: WheelEvent) {
    if (disabled || event.ctrlKey || Math.abs(event.deltaX) > Math.abs(event.deltaY)) return;
    stopMotion();
    event.preventDefault();
    // Normalize line/page wheels and accumulate small trackpad deltas.
    const distance =
      event.deltaY * (event.deltaMode === 1 ? 16 : event.deltaMode === 2 ? reel.clientHeight : 1);
    if (!distance) return;
    if (event.timeStamp - wheelTime > 180) {
      wheelRemainder = 0;
      wheelPullDistance = 0;
    }
    wheelTime = event.timeStamp;
    if (Math.sign(distance) !== Math.sign(wheelPullDistance)) wheelPullDistance = 0;
    wheelPullDistance += distance;
    pulling = true;
    // Positive wheel deltas move the reel upward, matching an upward pointer pull.
    pullLens(-wheelPullDistance);
    clearTimeout(wheelRelease);
    wheelRelease = setTimeout(() => {
      wheelRelease = undefined;
      wheelPullDistance = 0;
      scrollEnd();
    }, 220);
    wheelRemainder += distance;
    const steps = Math.trunc(wheelRemainder / rowHeight());
    if (steps) {
      wheelRemainder -= steps * rowHeight();
      moveTo((value ?? 5) - steps);
    }
    reviewWhenSettled();
  }
</script>

<!--
  Layout: Community average above the selector, vote count below. A decorative lens overlays the
  scrollable rating reel; the reel is the accessible input.
-->

<section class:compact class:voted={marked} class="voting-dial" aria-label={label}>
  <strong
    class="average"
    aria-label={average === null ? 'No average rating' : `Average rating: ${average.toFixed(1)}`}
    >{average === null ? '—' : average.toFixed(1)}</strong
  >
  <div class="selector" class:unrated={!marked} class:disabled>
    <!--
      The lens is visual only; the reel below owns focus, input events and the accessible rating
      value.
    -->
    <div
      class="selection-window"
      class:pulling
      style:transform={`translateY(${compact ? 0 : lensPull}px)`}
      aria-hidden="true"
    ></div>
    <div
      bind:this={reel}
      class="reel"
      class:dragging
      class:coasting
      role="spinbutton"
      tabindex={disabled ? -1 : 0}
      aria-label={label}
      aria-valuemin={0}
      aria-valuemax={10}
      aria-valuenow={value === null ? undefined : value}
      aria-valuetext={value === null
        ? 'Unrated. Scroll, drag or use arrow keys to choose a rating.'
        : `${value} out of 10`}
      title="Swipe or scroll, then pause to review a rating. With keyboard arrows, press Enter when ready."
      aria-disabled={disabled}
      onkeydown={handleKey}
      onscroll={handleScroll}
      onscrollend={scrollEnd}
      onpointerdown={pointerDown}
      onpointermove={pointerMove}
      onpointerup={pointerEnd}
      onpointercancel={pointerEnd}
      onlostpointercapture={pointerEnd}
      onwheel={handleWheel}
    >
      {#each ratings as rating}
        <span class="rating" class:selected={rating === (value ?? 5)} aria-hidden="true"
          >{rating}</span
        >
      {/each}
    </div>
  </div>
  <!--
    Expose one accessible vote-count label; hide the decorative people icon and duplicate
    visible count from assistive technology.
  -->
  <span class="vote-count" role="img" aria-label={`${totalVotes} ${totalVotes === 1 ? 'vote' : 'votes'}`} title={totalVotes.toLocaleString('en-US')}>
    <svg viewBox="0 0 56 44" aria-hidden="true">
      <circle cx="28" cy="7" r="6" /><circle cx="10" cy="12" r="5" /><circle
        cx="46"
        cy="12"
        r="5"
      />
      <!-- Full side bodies overlap behind the central figure. -->
      <path d="M1 41V26a9 9 0 0 1 18 0v15ZM37 41V26a9 9 0 0 1 18 0v15Z" />
      <path d="M12 44V30a16 16 0 0 1 32 0v14Z" />
    </svg>
    <span class="count" aria-hidden="true">{formatVoteCount(totalVotes)}</span>
  </span>
</section>

<style lang="scss">
  .voting-dial {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 0.4rem;
    width: max-content;
    padding: 0.4rem;
    color: var(--atlas-text);
  }

  .average {
    display: grid;
    place-items: center;
    min-width: 3.5rem;
    padding: 0.2rem 0.4rem;
    border: 2px solid var(--atlas-border);
    border-radius: 0.25rem;
    font-size: 1.15rem;
    font-variant-numeric: tabular-nums;
  }

  .selector {
    --row-height: 2.75rem;
    position: relative;
    width: 3.5rem;
    border: 2px solid var(--atlas-accent);
    border-radius: 0.5rem;
    background: linear-gradient(180deg, color-mix(in srgb, var(--atlas-accent) 35%, var(--atlas-surface)), var(--atlas-surface) 38%, var(--atlas-surface) 62%, color-mix(in srgb, var(--atlas-accent) 35%, var(--atlas-surface)));
    box-shadow: inset 0 0 12px color-mix(in srgb, var(--atlas-accent) 30%, transparent);
  }

  // Repaint the frame above the lens and reel so the lens reads as inset.
  // Inherit the actual border to preserve voted/unrated colors and rounded corners.
  // The overlay is decorative: pointer gestures still reach the reel beneath it.
  .selector::after {
    content: '';
    position: absolute;
    inset: -2px;
    z-index: 2;
    border: inherit;
    border-radius: inherit;
    pointer-events: none;
  }

  .unrated {
    border-color: var(--atlas-border);
    background: linear-gradient(180deg, color-mix(in srgb, #a58552 28%, var(--atlas-surface)), var(--atlas-surface) 40%, var(--atlas-surface) 60%, color-mix(in srgb, #a58552 28%, var(--atlas-surface)));
    box-shadow: inset 0 0 12px #a5855240;

    .selection-window {
      background: linear-gradient(135deg, #77634c, #514537 50%, #77634c);
      box-shadow: inset 0 1px 2px #ffffff50, 0 1px 3px #0005;
    }

    .selected {
      color: #fff;
    }
  }

  .selection-window {
    position: absolute;
    inset: var(--row-height) -2px auto;
    height: var(--row-height);
    background: linear-gradient(135deg, color-mix(in srgb, var(--atlas-accent) 75%, var(--atlas-surface)), var(--atlas-accent) 48%, color-mix(in srgb, var(--atlas-accent) 80%, var(--atlas-text)));
    pointer-events: none;
    border-radius: 0.2rem;
    box-shadow: inset 0 1px 2px #ffffff60, 0 1px 3px #0005;
    transition: transform 180ms ease-out;
  }

  .selection-window.pulling {
    transition: none;
  }

  .reel {
    position: relative;
    height: calc(var(--row-height) * 3);
    padding-block: var(--row-height);
    overflow-y: auto;
    scrollbar-width: none;
    scroll-snap-type: y mandatory;
    overscroll-behavior-y: contain;
    touch-action: none;
    cursor: grab;
    user-select: none;
    border-radius: 0.35rem;

    &::-webkit-scrollbar {
      display: none;
    }

    &.coasting {
      scroll-snap-type: none;
    }

    &.dragging {
      cursor: grabbing;
      scroll-snap-type: none;
    }

    &:focus-visible {
      outline: 3px solid var(--atlas-focus);
      outline-offset: 4px;
    }
  }

  .rating {
    display: grid;
    place-items: center;
    height: var(--row-height);
    scroll-snap-align: center;
    font-size: 1.3rem;
    font-weight: 600;
    font-variant-numeric: tabular-nums;
    color: var(--atlas-text);
  }

  .selected {
    color: var(--atlas-on-accent);
  }

  .vote-count {
    position: relative;
    display: grid;
    place-items: center;
    min-width: 3.5rem;
    height: 2.75rem;
  }

  svg {
    position: absolute;
    width: 100%;
    height: 100%;
    fill: var(--atlas-accent);
  }

  .count {
    position: relative;
    margin-top: 1rem;
    padding: 0 0.3rem;
    color: var(--atlas-on-accent);
    background: var(--atlas-accent);
    border-radius: 0.15rem;
    font-size: 0.85rem;
    font-weight: 700;
    font-variant-numeric: tabular-nums;
  }

  .disabled {
    opacity: 0.55;

    .reel {
      overflow: hidden;
      cursor: default;
      touch-action: auto;
    }
  }

  @media (prefers-reduced-motion: reduce) {
    .selection-window {
      transition: none;
    }
  }

  .compact {
    flex-direction: row;

    .reel {
      height: var(--row-height);
      padding-block: 0;
    }

    .selection-window {
      top: 0;
    }
  }
</style>
