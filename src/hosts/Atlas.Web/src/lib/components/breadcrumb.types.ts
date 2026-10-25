// Shared breadcrumb data contracts for components and plain TypeScript API helpers.

/** Oldest ancestor first; omit id when the source supplies display text only. */
export interface BreadcrumbItem {
  /** Node ID used for ancestor navigation; omit for a display-only breadcrumb item. */
  id?: string;

  /** Visible ancestor title. */
  title: string;
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
