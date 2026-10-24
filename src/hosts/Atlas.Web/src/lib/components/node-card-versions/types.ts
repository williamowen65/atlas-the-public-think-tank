import type { Snippet } from 'svelte';
import type { BreadcrumbItem, LoadOlderAncestors } from '../breadcrumb.types';
/** Presentation data: adapt API projections to this model at the application boundary. */
export interface NodeCardData {
  /** Stable node ID used for navigation and API lookups. */
  id: string;

  /** Visible node title. */
  title: string;

  /** Display name of the node’s type. */
  typeName: string;

  /** Display name of the node’s author. */
  authorName: string;

  /** Participant ID for author-profile navigation; omitted when unavailable. */
  authorId?: string;

  /** Creation timestamp formatted by the presentation component. */
  createdAt: string;

  /** Short introduction displayed by the card, rather than the full content blocks. */
  description: string;

  /** Community display names associated with the node; these are not community IDs. */
  communities: string[];

  /** Ancestor items in oldest-to-newest order for the breadcrumb. */
  breadcrumb: BreadcrumbItem[];

  /** Related nodes with IDs and display labels for navigation. */
  related: {
    /** Stable node ID used for navigation and API lookups. */
    id: string;
    /** Visible node title. */
    title: string;
    /** Display name of the node’s type. */
    typeName: string;
  }[];

  /** Display labels for contribution types requested by the author. */
  requestedTypes: string[];

  /** Structured contribution type IDs, labels and counts used for child-feed buttons. */
  childTypes?: {
    /** Catalog type ID passed to the contribution-navigation callback. */
    typeId: string;
    /** Visible contribution or reaction label. */
    label: string;
    /** Backend-supplied child-node or reaction-vote count for this entry. */
    count: number;
  }[];

  /** Structured reaction labels and counts displayed when custom reaction controls are absent. */
  reactions: {
    /** Visible contribution or reaction label. */
    label: string;
    /** Backend-supplied child-node or reaction-vote count for this entry. */
    count: number;
  }[];

  /** Optional formatted reaction summary used as a text fallback. */
  reactionSummary?: string;

  /** Optional formatted contribution summary used when structured child-type data is unavailable. */
  subNodeSummary?: string;

  /** Number of comments shown by the card’s discussion action. */
  commentCount: number;

  /** Lifecycle state used to display archived status and disable archived-node voting. */
  status: 'Active' | 'Archived';
}

/** Supported node-card layouts; selects presentation without changing node data. */
export type NodeCardPresentation =
  'context-below' | 'ribbon' | 'ribbon-responsive' | 'ribbon-split' | 'ribbon-corner';

/** Shared input contract forwarded to each node-card presentation. */
export interface NodeCardProps {
  /** Presentation data for one node; follow the NodeCardData type definition for its fields. */
  node: NodeCardData;
  /**
   * Renderable snippet that replaces the card’s built-in VotingDial. Supply LiveVotingDial here
   * for API-backed voting; otherwise the built-in dial reports selections through onVote.
   */
  voting?: Snippet;
  /**
   * Renderable snippet replacing the static reaction summary with custom/live controls, such as
   * LiveReactions.
   */
  reactionControls?: Snippet;
  /** Whether additional older ancestors can be loaded for the node’s breadcrumb. */
  hasMoreAncestors?: boolean;
  /**
   * Async callback receiving the oldest visible ancestor ID and returning BreadcrumbPage: older
   * items and hasMore. Used by ribbon variants; context-below has no ancestor pager.
   */
  loadOlderAncestors?: LoadOlderAncestors;
  /**
   * Public read base forwarded to breadcrumb lookups by ribbon variants. Context-below accepts but
   * does not use it; this is not the vote-save endpoint.
   */
  apiBase?: string;
  /** Current controlled rating, 0–10. null means unrated; zero is a real rating. */
  value?: number | null;
  /** Community average supplied by the parent; null means no average is available. */
  average?: number | null;
  /** Community vote count supplied by the parent. */
  totalVotes?: number;
  /**
   * Callback: Reports a selected rating to the parent. The presentation component does not save it
   * to the API.
   */
  onVote?: (value: number) => void;
  /**
   * Callback receiving a node ID from ancestor or related-node navigation. The parent decides how
   * to open it in the app or Storybook.
   */
  onNavigate?: (id: string) => void;
  /** Callback: Receives the author/participant ID to open in a profile page. */
  onOpenProfile?: (id: string) => void;
  /** Callback: Receives the node ID when Read more is activated; absence disables that action. */
  onReadMore?: (id: string) => void;
  /**
   * Callback: Receives node ID and contribution type ID when a contribution-summary button is
   * activated.
   */
  onChildType?: (id: string, typeId: string) => void;
  /** Callback: Receives the node ID when its comment-count button is activated. */
  onComments?: (id: string) => void;
}
