import type { Snippet } from 'svelte';
import type { BreadcrumbItem, LoadOlderAncestors } from '../Breadcrumb.svelte';
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

export type NodeCardPresentation = 'context-below' | 'ribbon' | 'ribbon-responsive' | 'ribbon-split' | 'ribbon-corner';
export interface NodeCardProps {
  node: NodeCardData;
  voting?: Snippet;
  reactionControls?: Snippet;
  hasMoreAncestors?: boolean;
  loadOlderAncestors?: LoadOlderAncestors;
  apiBase?: string;
  value?: number | null;
  average?: number | null;
  totalVotes?: number;
  onVote?: (value: number) => void;
  onNavigate?: (id: string) => void;
  onOpenProfile?: (id: string) => void;
  onReadMore?: (id: string) => void;
  onChildType?: (id: string, typeId: string) => void;
  onComments?: (id: string) => void;
}
