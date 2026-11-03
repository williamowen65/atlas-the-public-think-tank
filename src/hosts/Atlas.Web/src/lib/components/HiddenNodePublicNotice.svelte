<!--
  Public-only moderation explanation. Never accepts or renders hidden original
  content, reporter details, or private moderator decision rationale.
-->
<script lang="ts">
  let { reason = 'Other', author = false }: { reason?: string | null; author?: boolean } = $props();

  const reasons: Record<string, { label: string; explanation: string }> = {
    Spam: {
      label: 'Spam',
      explanation: 'Content that appears primarily promotional, repetitive, or intended to disrupt discussion rather than contribute to it.',
    },
    Harassment: {
      label: 'Harassment',
      explanation: 'Content that targets people with harassment, intimidation, or personal attacks rather than addressing their ideas.',
    },
    UnsafeContent: {
      label: 'Unsafe content',
      explanation: 'Content that may put people at risk or encourage harmful behavior.',
    },
    OffTopic: {
      label: 'Off topic',
      explanation: 'Content that does not meaningfully relate to the discussion or the contribution it was posted under.',
    },
    Other: {
      label: 'Other moderation concern',
      explanation: 'A moderator identified a concern that does not fit one of the standard categories.',
    },
  };
  const selected = $derived(reasons[reason ?? 'Other'] ?? reasons.Other);
</script>

<section class="moderation-notice" aria-labelledby="hidden-heading">
  <div class="notice-heading">
    <span class="eyebrow">Content moderation</span>
    <h1 id="hidden-heading">This contribution is hidden</h1>
    <p>A moderator has temporarily removed this contribution from public view. The original content is not displayed publicly, but its author may revise it and request another review.</p>
  </div>

  <div class="reason" aria-label="Public moderation reason">
    <h2>Why was it hidden?</h2>
    <p><strong>Reason: {selected.label}</strong></p>
    <p>{selected.explanation}</p>
    <p class="fine-print">This is the public reason category, not the moderator's private case notes or a full account of the decision.</p>
  </div>

  <div class="guidance">
    <h2>What kinds of contributions may be hidden?</h2>
    <p>Atlas is intended for constructive discussion, including disagreement and criticism of ideas. Moderation may be used for:</p>
    <ul>
      <li><strong>Spam:</strong> promotional, repetitive, or disruptive submissions.</li>
      <li><strong>Harassment:</strong> personal attacks, threats, or intimidation.</li>
      <li><strong>Unsafe content:</strong> material that could put people at risk.</li>
      <li><strong>Off-topic content:</strong> contributions that do not belong in the discussion where they were posted.</li>
      <li><strong>Other concerns:</strong> cases that require a moderator's judgment outside these categories.</li>
    </ul>
    <p><strong>Disagreement alone is not a reason to hide a contribution.</strong> Challenging a proposal, asking difficult questions, and presenting counterarguments are part of constructive participation.</p>
  </div>

  <div class="guidance">
    <h2>What happens next?</h2>
    <ol>
      <li><strong>The author can sign in</strong> to inspect the original contribution and its moderation notice.</li>
      <li><strong>The author can revise the contribution</strong> to address the concern, such as removing personal attacks or clarifying how it relates to the discussion.</li>
      <li><strong>After saving a revision, the author can request moderator review.</strong> The contribution remains hidden until a moderator approves its restoration.</li>
    </ol>
    {#if author}<p><strong>You're viewing this as the author.</strong> Your private moderation feedback and revision controls appear below. They are not visible to other visitors.</p>{:else}<p>Visitors cannot edit or request review on the author's behalf. If you are the author, sign in with the account that created this contribution, then return to this page.</p>{/if}
  </div>
</section>

<style>
  .moderation-notice { max-width: 48rem; margin: 1rem auto 3rem; padding: clamp(1rem, 3vw, 2rem); color: var(--atlas-text); }
  .notice-heading { padding-bottom: 1.25rem; border-bottom: 1px solid var(--atlas-border); }
  .eyebrow { display: block; color: var(--atlas-muted); font-size: .8rem; font-weight: 700; letter-spacing: .06em; text-transform: uppercase; margin-bottom: .5rem; }
  h1 { font-size: clamp(1.65rem, 4vw, 2.25rem); line-height: 1.2; margin: 0 0 .75rem; }
  h2 { font-size: 1.15rem; margin: 0 0 .65rem; }
  p { line-height: 1.6; margin: .5rem 0 .85rem; }
  .reason, .guidance { margin-top: 1.5rem; }
  .reason { background: var(--atlas-surface); border: 1px solid var(--atlas-border); border-radius: var(--atlas-radius); padding: 1rem 1.25rem; }
  .fine-print { font-size: .85rem; color: var(--atlas-muted); }
  ul, ol { padding-left: 1.5rem; line-height: 1.65; }
  li { margin-bottom: .55rem; }
</style>
