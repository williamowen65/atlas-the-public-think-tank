import type { Snippet } from 'svelte';
import type { BreadcrumbItem, LoadOlderAncestors } from '../breadcrumb.types';
/** Presentation data: adapt API projections to this model at the application boundary. */
export interface NodeCardData {
  id: string;
  title: string;
  typeName: string;
  authorName: string;
  authorId?: string;
  createdAt: string;
  description: string;
  communities: string[];
  breadcrumb: BreadcrumbItem[];
  related: { id: string; title: string; typeName: string }[];
  requestedTypes: string[];
  childTypes?: { typeId: string; label: string; count: number }[];
  reactions: { label: string; count: number }[];
  reactionSummary?: string;
  subNodeSummary?: string;
  commentCount: number;
  status: 'Active' | 'Archived';
}

export type NodeCardPresentation =
  'context-below' | 'ribbon' | 'ribbon-responsive' | 'ribbon-split' | 'ribbon-corner';
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
