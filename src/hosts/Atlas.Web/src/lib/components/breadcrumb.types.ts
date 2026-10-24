// Shared breadcrumb data contracts for components and plain TypeScript API helpers.

/** Oldest ancestor first; omit id when the source supplies display text only. */
export interface BreadcrumbItem {
  /** Node ID used for ancestor navigation; omit for a display-only breadcrumb item. */
  id?: string;

  /** Visible ancestor title. */
  title: string;
}
/** Alternative parent routes for a node; each route is oldest ancestor first. */
export interface BreadcrumbRoute {
  /** Stable identifier used to select this alternative route. */
  id: string;

  /** Human-readable label for this alternative ancestry route. */
  label: string;

  /** Ancestor items ordered from oldest to newest. */
  items: BreadcrumbItem[];

  /** Optional index identifying where this route diverges from the displayed ancestry. */
  divergenceIndex?: number;
}

/** One page of older ancestors, ordered for display, plus continuation availability. */
export interface BreadcrumbPage {
  /** Ancestor items ordered from oldest to newest. */
  items: BreadcrumbItem[];

  /** Whether additional older ancestors remain available to load. */
  hasMore: boolean;
}

/** Async ancestor loader: receives the oldest displayed node ID, if available, and returns older items plus hasMore. */
export type LoadOlderAncestors = (oldestId?: string) => Promise<BreadcrumbPage>;
