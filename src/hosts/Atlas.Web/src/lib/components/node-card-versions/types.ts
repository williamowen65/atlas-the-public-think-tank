import type { BreadcrumbItem, LoadOlderAncestors } from '../Breadcrumb.svelte';
/** Presentation data: adapt API projections to this model at the application boundary. */
export interface NodeCardData {
  id: string;
  title: string;
  typeName: string;
  authorName: string;
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
  hasMoreAncestors?: boolean;
  loadOlderAncestors?: LoadOlderAncestors;
  value?: number | null;
  average?: number | null;
  totalVotes?: number;
  onVote?: (value: number) => void;
  onNavigate?: (id: string) => void;
  onReadMore?: (id: string) => void;
  onChildType?: (id: string, typeId: string) => void;
  onComments?: (id: string) => void;
}
