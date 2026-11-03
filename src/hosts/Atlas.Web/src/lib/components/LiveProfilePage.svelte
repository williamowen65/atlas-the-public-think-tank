<!--
  @component
  LiveProfilePage: Loads a public participant profile and their contribution feed.
  Public data is readable anonymously; edit controls depend on the current session and profile ownership.

  **Props**
  - `participantId` (`string`; optional; default `''`): Participant whose public profile and
    authored contributions are displayed.
  - `apiBase` (`string`; optional; default `'/atlas-public-api'`): Public API base for
    participant/profile-community reads and the authored-node feed; owner edits use the session API.
  - `onOpenNode` (`(id: string) => void`; optional): Callback: Receives a node ID to open. Where the
    signature includes typeId, it selects the destination contribution tab.
  - `onOpenProfile` (`(id: string) => void`; optional): Callback: Receives the author/participant ID
    to open in a profile page.
-->
<script lang="ts">
  import { onMount } from 'svelte';
  import LiveNodeFeed from './LiveNodeFeed.svelte';
  import NodeCard from './NodeCard.svelte';
  import VotingDial from './VotingDial.svelte';
  import { discoveryCard, type DiscoveryPage } from '../api/discovery';
  import type { NodeCardData } from './node-card-versions/types';
  import { apiRequest } from '../api/apiRequest';
  import type { Community } from '../api/communities';
  let {
    participantId = '',
    apiBase = '/atlas-public-api',
    onOpenNode,
    onOpenProfile,
  }: {
    /** Participant whose public profile and authored contributions are displayed. */
    participantId?: string;
    /**
     * Public API base for participant/profile-community reads and the authored-node feed; owner
     * edits use the session API.
     */
    apiBase?: string;
    /**
     * Callback: Receives a node ID to open. Where the signature includes typeId, it selects the
     * destination contribution tab.
     */
    onOpenNode?: (id: string) => void;
    /** Callback: Receives the author/participant ID to open in a profile page. */
    onOpenProfile?: (id: string) => void;
  } = $props();
  type Profile = {
    id: string;
    displayName: string;
    bio: string;
    createdAt?: string;
    deleted?: boolean;
  };
  /**
   * Fetched public participant profile, including deletion state and contribution
   * identity.
   */
  let profile = $state<Profile | null>(null);
  /**
   * Whether data is being loaded; drives loading feedback for this view.
   */
  let loading = $state(true);
  /**
   * User-facing load or request failure; empty text hides the error message.
   */
  let error = $state('');
  /**
   * Signed-in participant ID used to determine whether this profile belongs to the
   * visitor.
   */
  let sessionId = $state<string | null>(null);
  /**
   * Whether the session lookup has completed, including a signed-out result.
   */
  let sessionReady = $state(false);
  type HiddenNode = { id: string; node: NodeCardData; notice: string; rationale: string; decidedAt: string; average: number | null; voteCount: number };
  let hiddenOwned = $state<HiddenNode[]>([]);
  let hiddenLoadError = $state('');
  let hiddenLoading = $state(false);
  let hiddenPage = $state(1);
  let hiddenHasMore = $state(false);
  async function loadHiddenOwned(ownerId: string, pageNumber = 1) {
    if (hiddenLoading) return;
    hiddenLoading = true;
    hiddenLoadError = '';
    try {
      // The server filters hidden nodes before applying Discovery's fixed-size pagination.
      const page = await apiRequest<DiscoveryPage>('/atlas-session-api', `nodes/mine/hidden?Page=${pageNumber}`);
      if (sessionId !== ownerId || participantId !== ownerId) return;
      const details = await Promise.all(page.items.map(async (item): Promise<HiddenNode> => {
        const detail = await apiRequest<{
          reasons: Array<{ publicReason: string; rationale: string | null; decidedAt: string }>;
        }>('/atlas-session-api', `moderation/nodes/${encodeURIComponent(item.nodeId)}/notice`);
        const decision = detail.reasons[0];
        const node = discoveryCard(item, true);
        if (!node) throw new Error('Missing private node card');
        return {
          id: item.nodeId,
          node,
          notice: decision?.publicReason ?? 'Hidden by moderator',
          rationale: decision?.rationale ?? '',
          decidedAt: decision?.decidedAt ?? '',
          average: item.averageVote,
          voteCount: item.voteCount,
        };
      }));
      if (sessionId !== ownerId || participantId !== ownerId) return;
      hiddenOwned = pageNumber === 1 ? details : [...hiddenOwned, ...details];
      hiddenPage = pageNumber;
      hiddenHasMore = page.hasNextPage;
    } catch {
      if (sessionId === ownerId && participantId === ownerId)
        hiddenLoadError = 'Could not load your hidden nodes.';
    } finally {
      hiddenLoading = false;
    }
  }

  /**
   * Whether the profile or community edit form is open.
   */
  let editing = $state(false);
  /**
   * Whether a save is underway; locks the relevant form against duplicate submission.
   */
  let saving = $state(false);
  /**
   * Non-error feedback shown after an action completes.
   */
  let notice = $state('');
  /**
   * Editable public display name, separate from the fetched profile until saved.
   */
  let displayName = $state('');
  /**
   * Editable public biography, separate from the fetched profile until saved.
   */
  let bio = $state('');
  /**
   * Loaded community records displayed in this view.
   */
  let communities = $state<Community[]>([]);
  /**
   * Whether the current session participant matches the displayed profile ID.
   */
  const isOwner = $derived(!!participantId && sessionId === participantId);
  /**
   * Loaded communities owned by the displayed participant.
   */
  const ownedCommunities = $derived(
    communities.filter((c) => c.ownerParticipantId === profile?.id),
  );
  /**
   * Loaded communities joined by the displayed participant but owned by someone else.
   */
  const joinedCommunities = $derived(
    communities.filter((c) => c.isMember && c.ownerParticipantId !== profile?.id),
  );
  // Load the public profile first; unavailable community data should not hide that profile.
  async function loadProfile(id: string, signal: AbortSignal) {
    loading = true;
    error = '';
    profile = null;
    communities = [];
    try {
      const result = await apiRequest<Profile>(
        apiBase,
        'participants/' + encodeURIComponent(id),
        'GET',
        undefined,
        signal,
      );
      if (signal.aborted) return;
      profile = result;
      displayName = result.displayName;
      bio = result.bio ?? '';
      try {
        const associated = await apiRequest<Community[]>(
          apiBase,
          'communities/participant/' + encodeURIComponent(id),
          'GET',
          undefined,
          signal,
        );
        if (!signal.aborted) communities = associated;
      } catch {
        /* A missing community service should not hide the profile. */
      }
    } catch {
      if (!signal.aborted) error = 'Could not load this public profile.';
    } finally {
      if (!signal.aborted) loading = false;
    }
  }
  // Session identity controls owner-edit affordances; it is independent of the public profile being viewed.
  async function checkSession() {
    try {
      const session = await apiRequest<{ participantId: string | null }>(
        '/atlas-session-api',
        'auth/session',
      );
      sessionId = session.participantId;
      if (session.participantId === participantId) {
        hiddenOwned = [];
        hiddenPage = 1;
        void loadHiddenOwned(participantId);
      }
      else hiddenOwned = [];
    } catch {
      sessionId = null;
    } finally {
      sessionReady = true;
    }
  }
  onMount(() => {
    void checkSession();
    const changed = () => void checkSession();
    window.addEventListener('atlas-session-changed', changed);
    return () => window.removeEventListener('atlas-session-changed', changed);
  });
  // Changing the participant cancels the previous lookup and clears stale editing state.
  $effect(() => {
    const id = participantId;
    editing = false;
    notice = '';
    if (!id) {
      profile = null;
      loading = false;
      return;
    }
    const controller = new AbortController();
    void loadProfile(id, controller.signal);
    return () => controller.abort();
  });
  async function save() {
    if (!isOwner || saving || !profile) return;
    saving = true;
    notice = '';
    try {
      await apiRequest('/atlas-session-api', 'participants/me', 'PUT', { displayName, bio });
      profile = { ...profile, displayName, bio };
      editing = false;
      notice = 'Profile saved.';
      window.dispatchEvent(new Event('atlas-session-changed'));
    } catch {
      notice = 'Profile changes could not be saved. Please try again.';
    } finally {
      saving = false;
    }
  }
</script>

<!--
  Layout: Profile identity and biography lead into community links, optional owner editing and
  the participant’s contribution feed.
-->

<section class="profile-page" aria-label="Public participant profile">
  {#if !participantId}<p>Select a participant to view their public profile.</p>{/if}
  {#if loading && !profile}<p role="status">Loading profile…</p>{/if}
  {#if error}<p role="alert">{error}</p>{/if}
  {#if profile}
    <header class="profile-header">
          {#if isOwner && !profile.deleted}<button
              type="button"
              class="edit-profile"
              aria-label={editing ? 'Cancel editing' : 'Edit my profile'}
              title={editing ? 'Cancel editing' : 'Edit my profile'}
              onclick={() => {
                editing = !editing;
                notice = '';
              }}>✎</button
            >{/if}

      <div class="avatar" aria-hidden="true">{profile.displayName.charAt(0).toUpperCase()}</div>
      <div class="profile-details">
        <div class="profile-title">
          <h1>{profile.displayName}</h1>
        </div>
        {#if profile.createdAt}<p class="muted">
            Member since {new Date(profile.createdAt).toLocaleDateString()}
          </p>{/if}
        <p>
          {profile.bio ||
            (profile.deleted ? 'This account has been deleted.' : 'No public bio yet.')}
        </p>
        {#if ownedCommunities.length}<p class="community-line">
            <strong>Community owner of:</strong>
            {#each ownedCommunities as community, i (community.id)}{#if i > 0},
              {/if}<a href={'/communities/' + community.id}>{community.name}</a>{/each}
          </p>{/if}
        {#if joinedCommunities.length}<p class="community-line">
            <strong>Member of:</strong>
            {#each joinedCommunities as community, i (community.id)}{#if i > 0},
              {/if}<a href={'/communities/' + community.id}>{community.name}</a>{/each}
          </p>{/if}
      </div>
    </header>
    {#if editing && isOwner}<form
        onsubmit={(event) => {
          event.preventDefault();
          void save();
        }}
      >
        <label for="profile-display-name">Display name</label><input
          id="profile-display-name"
          bind:value={displayName}
          maxlength="100"
          required
        /><label for="profile-bio">Public biography</label><textarea
          id="profile-bio"
          bind:value={bio}
          maxlength="500"
          rows="5"></textarea><button disabled={saving}
          >{saving ? 'Saving…' : 'Save changes'}</button
        >
      </form>{/if}
    {#if notice}<p role="status">{notice}</p>{/if}
    {#if !profile.deleted}
      {#if isOwner}
        <section aria-label="My hidden nodes">
          <h2>My hidden nodes</h2>
          <p>These entries are private to your account. Open a node to revise it and request moderator review.</p>
          {#if hiddenLoadError}<p role="status">{hiddenLoadError}</p>{/if}
          {#if hiddenOwned.length === 0 && !hiddenLoading}<p>No hidden nodes found.</p>{/if}
          <div class="hidden-feed">
            {#each hiddenOwned as hidden (hidden.id)}
              <article class="hidden-card">
                <p class="hidden-label">Hidden by moderator <span class="hidden-reason">{hidden.notice}</span></p>
                <NodeCard node={hidden.node} presentation="ribbon-corner" average={hidden.average} totalVotes={hidden.voteCount} onReadMore={(id) => onOpenNode?.(id)}>
                  {#snippet voting()}
                    <VotingDial label="Node importance (voting disabled while hidden)" average={hidden.average} totalVotes={hidden.voteCount} disabled />
                  {/snippet}
                </NodeCard>
                <div class="moderation-feedback">
                  <p class="decision-date"><strong>Moderation decision</strong>{#if hidden.decidedAt} <time datetime={hidden.decidedAt}>{new Date(hidden.decidedAt).toLocaleDateString(undefined, { year: 'numeric', month: 'long', day: 'numeric' })}</time>{/if}</p>
                  <p><strong>Moderator explanation</strong></p>
                  <p>{hidden.rationale || 'No explanation was provided.'}</p>
                </div>
              </article>
            {/each}
          </div>
          {#if hiddenHasMore}<button type="button" disabled={hiddenLoading} onclick={() => void loadHiddenOwned(participantId, hiddenPage + 1)}>Load more hidden nodes</button>{/if}
          {#if hiddenLoading}<p role="status">Loading hidden nodes…</p>{/if}
        </section>
      {/if}
      <h2>Published contributions</h2>
      {#key profile.id}<LiveNodeFeed
          authorId={profile.id}
          {apiBase}
          {onOpenNode}
          {onOpenProfile}
        />{/key}
    {/if}
  {/if}
</section>

<style lang="scss">
  .hidden-feed { display: grid; gap: 1rem; margin-bottom: 1rem; }
  .hidden-card { border: 1px solid color-mix(in srgb, #bd8336 45%, var(--atlas-border)); border-radius: 1rem; background: color-mix(in srgb, #bd8336 7%, var(--atlas-surface)); padding: 1.4rem; }
  .hidden-card h3 { margin: .5rem 0; }
  .hidden-label { font-weight: 700; margin: 0 0 1rem; display: flex; align-items: center; flex-wrap: wrap; gap: .65rem; }
  .hidden-reason { display: inline-block; border-radius: 999px; padding: .3rem .7rem; background: color-mix(in srgb, #bd8336 17%, var(--atlas-surface)); font-size: .85rem; }
  .hidden-card p { overflow-wrap: anywhere; }
  .moderation-feedback { margin-top: 1.1rem; padding: 1rem 1.15rem; background: var(--atlas-surface); border-radius: .7rem; border-left: 3px solid #bd8336; }
  .moderation-feedback p { margin: .4rem 0; }
  .decision-date { display: flex; flex-wrap: wrap; gap: .4rem 1rem; justify-content: space-between; }
  .decision-date time { font-size: .9rem; color: var(--atlas-muted); }
  @media (max-width: 480px) { .hidden-card { padding: .85rem; } }
  .profile-page {
    padding-block-start: var(--atlas-space);
    max-width: 70rem;
    margin-inline: auto;
    min-width: 0;
  }
  button {
    font: inherit;
    padding: 0.45rem;
    max-width: 100%;
    cursor: pointer;
  }
  .profile-title {
    padding-inline-end: 2.75rem;
    display: flex;
    align-items: center;
    gap: 0.7rem;
  }
  .edit-profile {
    border: 1px solid var(--atlas-border);
    position: absolute;
    top: 0;
    right: 0;
    display: grid;
    place-items: center;
    width: 2.75rem;
    height: 2.75rem;
    padding: 0;
    border-top: 0;
    border-right: 0;
    border-radius: 0 var(--atlas-radius) 0 var(--atlas-radius);
    background: transparent;
    color: var(--atlas-text);
  }
  .profile-details {
    min-width: 0;
    flex: 1;
  }
  form {
    display: grid;
    gap: 0.65rem;
    max-width: 36rem;
    margin: 1rem 0;
  }
  input,
  textarea {
    font: inherit;
    padding: 0.65rem;
    background: var(--atlas-surface);
    color: var(--atlas-text);
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
  }
  .community-line {
    margin: 0.35rem 0;
    font-size: 0.9rem;
    line-height: 1.5;
    overflow-wrap: anywhere;
  }
  .community-line a {
    color: inherit;
    text-decoration: none;
  }
  .community-line a:hover,
  .community-line a:focus-visible {
    text-decoration: underline;
  }
  .profile-header {
    position: relative;
    display: flex;
    align-items: flex-start;
    gap: 1.25rem;
    padding: 1.5rem;
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    background: var(--atlas-surface);
  }
  .avatar {
    display: grid;
    place-items: center;
    flex: 0 0 4rem;
    height: 4rem;
    border-radius: 50%;
    background: var(--atlas-accent);
    color: var(--atlas-on-accent);
    font-size: 2rem;
    font-weight: 700;
  }
  h1 {
    margin: 0;
  }
  h2 {
    margin: 2rem 0 1rem;
  }
  .muted {
    color: var(--atlas-muted);
    font-size: 0.9rem;
  }
  @media (max-width: 450px) {
    .profile-header {
      padding: 0.8rem;
      gap: 0.75rem;
    }
    .avatar {
      flex-basis: 3rem;
      height: 3rem;
      font-size: 1.4rem;
    }
  }
</style>
