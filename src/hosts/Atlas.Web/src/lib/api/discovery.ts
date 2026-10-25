import type { BreadcrumbPage } from '../components/breadcrumb.types';
import type { NodeCardData } from '../components/node-card-versions/types';

/** Optional discovery constraints translated into API query parameters. */
export interface DiscoveryFilters {
  /** Relative voting influence in text search, from 0 through 100 percent. */
  votingWeight?: number;
  /**
   * Restrict discovery to children of this parent node ID; omitted leaves parent scope
   * unset.
   */
  parentId?: string;

  /** Global search scope; browsing feeds remain node-only. */
  scope?: 'all' | 'nodes' | 'comments';

  /** Node-type ID used for filtering or identifying a contribution category. */
  typeId?: string;

  /** Participant ID used for author filtering or profile navigation. */
  authorId?: string;

  /** Community ID used to restrict discovery results. */
  communityId?: string;

  /** Inclusive minimum vote count; omitted leaves the lower bound unset. */
  minVotes?: number;

  /** Inclusive maximum vote count; omitted leaves the upper bound unset. */
  maxVotes?: number;

  /** Inclusive minimum average importance rating. */
  minAverage?: number;

  /** Inclusive maximum average importance rating. */
  maxAverage?: number;

  /** Creation-date lower bound forwarded to the API for date parsing and validation. */
  from?: string;

  /** Creation-date upper bound forwarded to the API for date parsing and validation. */
  through?: string;

  /**

   * Include archived nodes alongside active nodes when true; does not select archived nodes

   * exclusively.

   */
  archived?: boolean;
}

/** Public discovery projection with display summaries and moderation visibility. */
interface DiscoveryCard {
  /** Structured contribution categories and counts used for child-feed links. */
  childTypes?: {
    /** Node-type ID used for filtering or identifying a contribution category. */
    typeId: string;
    /** Display label supplied by the API. */
    label: string;
    /** Number of children in this contribution category. */
    count: number;
  }[];

  /** Stable node ID used for navigation and detail requests. */
  nodeId: string;

  /** Display title supplied by the API. */
  title: string;

  /** Display name of the node type. */
  typeName: string;

  /** Display name of the author. */
  authorName: string;

  /** Participant ID used for author filtering or profile navigation. */
  authorId?: string | null;

  /** Backend lifecycle status, interpreted by the caller for presentation. */
  status: string;

  /** Structured ancestor segment; avoids parsing formatted breadcrumb text. */
  breadcrumbSegment?: {
    /** Records in this page or segment, in endpoint order. */
    items: {
      /** Stable node ID used for navigation and detail requests. */
      nodeId: string;
      /** Display label supplied by the API. */
      label: string;
    }[];

    /** Node ID marking ancestor continuation, or null if none remains. */
    continuationNodeId: string | null;

    /** Whether another ancestor route exists in the graph. */
    hasAlternativeRoutes: boolean;

    /** Whether traversal encountered a cycle in the graph. */
    cycleDetected: boolean;
  };

  /** API creation timestamp. */
  createdAt: string;

  /** Description supplied for this record. */
  description: string;

  /** Short introduction shown before full node content. */
  shortDescription: string;

  /** Formatted ancestor fallback when a structured segment is unavailable. */
  breadcrumb: string;

  /** Formatted child summary for display, not structured navigation data. */
  subNodes: string;

  /** Formatted reaction summary; fetch structured totals separately. */
  reactions: string;

  /** Comma-separated community names for display; contains no community IDs. */
  communities: string;

  /** Comment total used by cards and discussion tabs. */
  commentCount: number;

  /** Moderation flag; hidden projections are omitted by discoveryCard. */
  isHidden: boolean;
}

/** One discovery result with aggregate voting data and an optional card. */
interface DiscoveryItem {
  /** Present only for a comment hit in global search. */
  comment?: { id: string; nodeTitle: string; authorId: string; authorName: string; createdAt: string; excerpt: string; score: number } | null;
  /** Stable node ID used for navigation and detail requests. */
  nodeId: string;

  /** Number of votes included in the aggregate. */
  voteCount: number;

  /** Backend-calculated importance average, or null when unavailable. */
  averageVote: number | null;

  /** Public card projection, or null if unavailable. */
  card: DiscoveryCard | null;
}

/** Numbered discovery results with total count and continuation information. */
export interface DiscoveryPage {
  /** Records in this page or segment, in endpoint order. */
  items: DiscoveryItem[];

  /** Total matching records reported by the API. */
  totalCount: number;

  /** One-based discovery page number. */
  page: number;

  /** Maximum number of records in a discovery page. */
  pageSize: number;

  /** Whether another discovery page can be requested. */
  hasNextPage: boolean;
}

/** Known safe service failure; components can show it without exposing backend diagnostics. */
export class SearchUnavailableError extends Error {
  constructor() {
    super('Text search is temporarily unavailable. Please try again later.');
    this.name = 'SearchUnavailableError';
  }
}

/** Anonymous read only. The caller supplies an API base and owns request cancellation. */
export async function readDiscovery(
  base: string,
  page: number,
  search: string,
  signal: AbortSignal,
  nodeId?: string,
  filters?: DiscoveryFilters,
  mixedSearch = false,
): Promise<DiscoveryPage> {
  // Map the UI filter names to the backend contract. Omitted bounds are not sent.
  const query = new URLSearchParams({ Page: String(page) });
  // Exact-node lookup includes archived records so direct navigation can show their status.
  if (nodeId) {
    query.set('NodeId', nodeId);
    query.set('IncludeArchived', 'true');
  }
  if (filters?.parentId) query.set('ParentNodeId', filters.parentId);
  if (filters?.typeId) query.set('NodeTypeId', filters.typeId);
  if (filters?.authorId) query.set('AuthorParticipantId', filters.authorId);
  if (filters?.communityId) query.set('CommunityId', filters.communityId);
  if (filters?.minVotes !== undefined) query.set('MinimumVoteCount', String(filters.minVotes));
  if (filters?.maxVotes !== undefined) query.set('MaximumVoteCount', String(filters.maxVotes));
  if (filters?.minAverage !== undefined)
    query.set('MinimumAverageVote', String(filters.minAverage));
  if (filters?.maxAverage !== undefined)
    query.set('MaximumAverageVote', String(filters.maxAverage));
  if (filters?.from) query.set('CreatedFrom', filters.from);
  if (filters?.through) query.set('CreatedThrough', filters.through);
  if (filters?.archived) query.set('IncludeArchived', 'true');
  if (mixedSearch && filters?.votingWeight !== undefined) query.set('VotingWeight', String(filters.votingWeight));
  if (mixedSearch && filters?.scope) query.set('scope', filters.scope);
  if (search.trim()) query.set('SearchText', search.trim());
  const response = await fetch(`${base}/${mixedSearch ? 'search' : 'nodes'}?${query}`, {
    method: 'GET',
    credentials: 'omit',
    signal,
  });
  if (!response.ok) {
    if (response.status === 503) {
      const problem = await response.json().catch(() => null);
      if (problem?.code === 'search_unavailable') throw new SearchUnavailableError();
    }
    throw new Error(`Atlas API returned HTTP ${response.status}.`);
  }
  // Check the minimum page envelope; this is not exhaustive validation of each card.
  const result = (await response.json()) as DiscoveryPage;
  if (
    !Array.isArray(result.items) ||
    !Number.isInteger(result.page) ||
    !Number.isInteger(result.totalCount)
  ) {
    throw new Error('Atlas API returned an unexpected Discovery response.');
  }
  return result;
}

/**

 * Discovery strings remain display text; do not invent structured links or reaction counts.

 */
export function discoveryCard(item: DiscoveryItem): NodeCardData | null {
  const card = item.card;
  if (!card || card.isHidden) return null;
  // Reverse nearest-first ancestors for oldest-first display. Formatted summaries
  // remain text; structured links/counts come only from structured API fields.
  return {
    id: item.nodeId,
    title: card.title,
    typeName: card.typeName,
    authorName: card.authorName,
    authorId: card.authorId ?? undefined,
    status: card.status === 'Archived' ? 'Archived' : 'Active',
    createdAt: card.createdAt,
    description: card.shortDescription ?? '',
    communities:
      card.communities && card.communities.trim().toLowerCase() !== 'none'
        ? card.communities
            .split(',')
            .map((name) => name.trim())
            .filter(Boolean)
        : [],
    breadcrumb: card.breadcrumbSegment
      ? [...card.breadcrumbSegment.items]
          .reverse()
          .map((item) => ({ id: item.nodeId, title: item.label }))
      : card.breadcrumb
        ? [{ title: card.breadcrumb }]
        : [],
    related: [],
    requestedTypes: [],
    reactions: [],
    childTypes: card.childTypes ?? [],
    reactionSummary: card.reactions,
    subNodeSummary: card.subNodes,
    commentCount: card.commentCount,
  };
}

/** The existing endpoint returns nearest-first; presentation pages are oldest-first. */
export async function readOlderAncestors(
  id: string,
  signal: AbortSignal,
  base = '/atlas-public-api',
): Promise<BreadcrumbPage> {
  const response = await fetch(`${base}/nodes/${encodeURIComponent(id)}/ancestors`, {
    credentials: 'omit',
    signal,
  });
  if (!response.ok) throw new Error(`Could not load older ancestors (HTTP ${response.status}).`);
  const page = (await response.json()) as {
    /** Records in this page or segment, in endpoint order. */
    items: {
      /** Stable identifier of this record. */
      id: string;
      /** Display title supplied by the API. */
      title: string;
    }[];

    /** Whether more ancestor records can be requested. */
    hasMore: boolean;
  };
  if (!Array.isArray(page.items) || typeof page.hasMore !== 'boolean')
    throw new Error('Unexpected ancestor response.');
  return { items: [...page.items].reverse(), hasMore: page.hasMore };
}

/** Read individual public reaction totals rather than parsing the formatted summary. */
export async function readReactions(
  id: string,
  signal: AbortSignal,
  base = '/atlas-public-api',
): Promise<NodeCardData['reactions']> {
  const response = await fetch(`${base}/nodes/${encodeURIComponent(id)}/reactions`, {
    credentials: 'omit',
    signal,
  });
  if (!response.ok) throw new Error('Reaction totals could not be loaded.');
  const choices = (await response.json()) as {
    /** Node-reaction association ID, or null when this reaction is not associated. */
    associationId: string | null;

    /** Reaction display text. */
    text: string;

    /** Emoji representing the reaction. */
    emoji: string;

    /** Backend-reported vote total for this reaction. */
    totalVotes: number;
  }[];
  if (!Array.isArray(choices)) throw new Error('Unexpected reaction response.');
  return choices
    .filter((choice) => choice.associationId && choice.totalVotes > 0)
    .map((choice) => ({
      label: `${choice.emoji} ${choice.text}`.trim(),
      count: choice.totalVotes,
    }));
}
