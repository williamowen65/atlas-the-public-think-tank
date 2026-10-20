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

  // Selection is controlled by the caller; null means unrated, not zero.
  function select(next: number) {
    if (!disabled) onChange?.(Math.max(0, Math.min(10, next)));
  }

  function adjust(delta: number) {
    select(value === null ? 5 : value + delta);
  }

  function handleKey(event: KeyboardEvent) {
    if (disabled) return;
    if (['ArrowUp', 'ArrowRight', 'ArrowDown', 'ArrowLeft', 'Home', 'End'].includes(event.key)) {
      event.preventDefault();
      if (event.key === 'Home') select(0);
      else if (event.key === 'End') select(10);
      else adjust(event.key === 'ArrowUp' || event.key === 'ArrowRight' ? 1 : -1);
    }
  }
</script>

<section class:compact class="voting-dial" aria-label={label}>
  <div class="summary">
    <span class="caption">Average</span>
    <strong class="average">{average === null ? '—' : average.toFixed(1)}</strong>
  </div>

  <div class="selector" class:unrated={value === null}>
    <button
      type="button" aria-label={`Increase ${label}`} {disabled}
      onclick={() => adjust(1)}
    >↑</button>
    <div
      class="reel" role="spinbutton" tabindex={disabled ? -1 : 0}
      aria-label={label} aria-valuemin={0} aria-valuemax={10}
      aria-valuenow={value === null ? undefined : value}
      aria-valuetext={value === null ? 'Unrated. Use arrow keys to choose a rating.' : `${value} out of 10`}
      aria-disabled={disabled} onkeydown={handleKey}
    >
      {#if !compact}<span class="neighbor" aria-hidden="true">{value !== null && value < 10 ? value + 1 : '·'}</span>{/if}
      <strong class="selection">{value ?? '—'}</strong>
      {#if !compact}<span class="neighbor" aria-hidden="true">{value !== null && value > 0 ? value - 1 : '·'}</span>{/if}
    </div>
    <button
      type="button" aria-label={`Decrease ${label}`} {disabled}
      onclick={() => adjust(-1)}
    >↓</button>
  </div>

  <span class="vote-count" aria-label={`${totalVotes} votes`}>
    <svg viewBox="0 0 40 28" width="32" height="24" aria-hidden="true">
      <circle cx="20" cy="6" r="5"/><circle cx="7" cy="10" r="4"/><circle cx="33" cy="10" r="4"/>
      <path d="M11 28V20a9 9 0 0 1 18 0v8ZM0 27v-8a7 7 0 0 1 10-6v14ZM30 27V13a7 7 0 0 1 10 6v8Z"/>
    </svg>
    <span>{totalVotes} votes</span>
  </span>
  <small>{value === null ? 'Unrated' : `Your selection: ${value}`}</small>
</section>

<style>
  .voting-dial { display: flex; flex-direction: column; align-items: center; gap: var(--atlas-space); width: 8rem; padding: var(--atlas-space); border: 1px solid var(--atlas-border); border-radius: var(--atlas-radius); background: var(--atlas-surface); color: var(--atlas-text); }
  .summary { text-align: center; display: grid; gap: .25rem; }
  .caption, small { font-size: .8rem; }
  .average { border: 2px solid var(--atlas-border); padding: .3rem .8rem; font-size: 1.35rem; }
  .selector { display: grid; justify-items: center; gap: .2rem; }
  button { font: inherit; width: 2.75rem; min-height: 2.75rem; color: var(--atlas-text); background: var(--atlas-bg); border: 1px solid var(--atlas-border); border-radius: .3rem; cursor: pointer; }
  button:disabled { opacity: .55; cursor: default; }
  .reel { width: 3.75rem; text-align: center; background: var(--atlas-bg); border: 2px solid var(--atlas-accent); border-radius: .65rem; overflow: hidden; }
  .unrated .reel { border-style: dashed; }
  .neighbor { display: block; padding: .2rem; opacity: .7; }
  .selection { display: block; padding: .4rem; background: var(--atlas-accent); color: var(--atlas-on-accent); font-size: 1.3rem; }
  .vote-count { display: flex; gap: .3rem; align-items: center; font-size: .85rem; }
  svg { fill: currentColor; }
  :focus-visible { outline: 3px solid var(--atlas-focus); outline-offset: 3px; }
  .compact { width: auto; flex-direction: row; flex-wrap: wrap; justify-content: flex-start; }
  .compact .selector { display: flex; align-items: center; }
  .compact .average { font-size: 1rem; padding: .25rem .5rem; }
  .compact .selection { font-size: 1rem; }
  .compact .reel { width: 2.75rem; }
</style>
