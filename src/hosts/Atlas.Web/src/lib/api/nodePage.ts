/** Catalog entry used by search filters, creation choices and labels. */
export interface NodeType {
  /** Stable identifier of this record. */
  id: string;

  /** Human-readable display name. */
  name: string;

  /** Plural display label supplied by the catalog. */
  pluralName?: string;

  /** Whether automatic pluralization is enabled for this type. */
  autoPluralize?: boolean;

  /** Description supplied for this record. */
  description?: string;

  /** Whether this type is archived; readTypes excludes archived entries. */
  isArchived?: boolean;
}

/** Ordered content block; kind determines which content fields apply. */
export interface ContentBlock {
  /** Stable identifier of this record. */
  id: string;

  /** Block discriminator that determines which payload fields the renderer uses. */
  kind: string;

  /** Payload for this block kind; unrelated optional fields may be absent. */
  content: {
    /** Markdown source rendered as formatted text. */
    markdown?: string;

    /** Image or link URL; callers must apply their URL-safety policy. */
    url?: string;

    /** Accessible alternative text for an image. */
    altText?: string;

    /** Optional image caption. */
    caption?: string;

    /** Display title supplied by the API. */
    title?: string;

    /** Description supplied for this record. */
    description?: string;

    /** Referenced poll ID; does not contain poll data. */
    pollId?: string;

    /** Referenced chart ID; does not contain chart data. */
    chartId?: string;
  };
}

/** Node details loaded separately from discovery summaries and content blocks. */
export interface NodeDetail {
  /** Stable identifier of this record. */
  id: string;

  /** Participant ID used for author filtering or profile navigation. */
  authorId: string;

  /** Display title supplied by the API. */
  title: string;

  /** Short introduction shown before full node content. */
  shortDescription: string;

  /** Type IDs the author requests as contributions to this node. */
  requestedTypeIds: string[];

  /** Backend lifecycle status, interpreted by the caller for presentation. */
  status: string;

  /** Whether moderation marks this node hidden. */
  hidden?: boolean;
}

/** Comment or reply; removed comments may remain as placeholders. */
export interface Comment {
  /** Stable identifier of this record. */
  id: string;

  /** Participant ID used for author filtering or profile navigation. */
  authorId: string;

  /** Plain-text comment body, or null for a removed placeholder. */
  body: string | null;

  /** Backend lifecycle status, interpreted by the caller for presentation. */
  status: string;

  /** API creation timestamp. */
  createdAt: string;

  /** Most recent update timestamp, when provided. */
  updatedAt?: string;

  /** Parent comment ID for a reply; omitted or null denotes a root comment. */
  parentId?: string | null;
}

/** Keyset-paginated comments with a cursor for the next request. */
export interface CommentPage {
  /** Records in this page or segment, in endpoint order. */
  items: Comment[];

  /** Next-page keyset cursor, or null at the end of the list. */
  next: {
    /** Timestamp cursor value; use together with afterId. */
    afterAt: string;
    /** ID cursor tie-breaker; use together with afterAt. */
    afterId: string;
  } | null;
}
/**
 * Fetch anonymous JSON from base/path. The caller owns cancellation; T is a compile-time
 * shape, not runtime validation.
 */
export async function readJson<T>(base: string, path: string, signal: AbortSignal): Promise<T> {
  const response = await fetch(`${base}/${path}`, { credentials: 'omit', signal });
  if (!response.ok) throw new Error(`Request failed (${response.status})`);
  return response.json();
}
/** Load the public node-type catalog and omit archived entries from selectable choices. */
export async function readTypes(base: string, signal: AbortSignal) {
  const types = await readJson<NodeType[]>(base, 'catalog/node-types', signal);
  return types.filter((type) => !type.isArchived);
}
/** Return a normalized absolute HTTP(S) URL, or undefined for invalid/unsupported URLs. */
export function publicUrl(value?: string): string | undefined {
  if (!value) return;
  try {
    const url = new URL(value);
    return ['http:', 'https:'].includes(url.protocol) ? url.href : undefined;
  } catch {
    return;
  }
}
