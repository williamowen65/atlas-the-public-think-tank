// Shared breadcrumb data contracts for components and plain TypeScript API helpers.

/** Oldest ancestor first; omit id when the source supplies display text only. */
export interface BreadcrumbItem {
  id?: string;
  title: string;
}
/** Alternative parent routes for a node; each route is oldest ancestor first. */
export interface BreadcrumbRoute {
  id: string;
  label: string;
  items: BreadcrumbItem[];
  divergenceIndex?: number;
}
export interface BreadcrumbPage {
  items: BreadcrumbItem[];
  hasMore: boolean;
}
export type LoadOlderAncestors = (oldestId?: string) => Promise<BreadcrumbPage>;
