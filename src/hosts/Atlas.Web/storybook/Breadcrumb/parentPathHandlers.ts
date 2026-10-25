// MSW's http helpers match requests; HttpResponse builds mock responses without an API server.
// These handlers belong to Storybook, not the production component or development proxy.
import { http, HttpResponse } from 'msw/http';
import type { BreadcrumbItem } from '../../src/lib/components/breadcrumb.types';

/** Workbench-only path data used to mock the production parent/ancestor HTTP contract. */
export interface ExampleRoute {
  /** Fixture path identifier. */
  id: string;
  /** Human-readable path label shown by the example. */
  label: string;
  /** Full fixture path, oldest ancestor first. */
  items: BreadcrumbItem[];
}

/**
 * Simulate the same parent/ancestor response shapes that Breadcrumb uses in the app.
 * Match only /storybook-breadcrumb-api: real /atlas-public-api requests are not mocked.
 * @param getRoutes Read the currently selected fixture scenario for each request.
 * @param onSelection Update the example’s status label after choosing a parent path.
 */
export function parentPathHandlers(
  getRoutes: () => ExampleRoute[],
  onSelection: (label: string) => void,
) {
  return [
    // List distinct immediate parents across the scenario’s paths, like the real endpoint.
    http.get('*/storybook-breadcrumb-api/nodes/:id/parents', ({ params }) => {
      const parents = new Map<string, BreadcrumbItem>();
      for (const route of getRoutes()) {
        const index = route.items.findIndex((item) => item.id === params.id);
        if (index > 0) {
          const parent = route.items[index - 1];
          if (parent.id) parents.set(parent.id, parent);
        }
        if (params.id === 'example-origin') {
          const parent = route.items.at(-1)!;
          if (parent.id) parents.set(parent.id, parent);
        }
      }
      return HttpResponse.json([...parents.values()]);
    }),
    // A selected firstParent chooses the ancestry prefix; Breadcrumb retains the later nodes.
    http.get('*/storybook-breadcrumb-api/nodes/:id/ancestors', ({ params, request }) => {
      const parentId = new URL(request.url).searchParams.get('firstParent');
      const route = getRoutes().find((candidate) => {
        const index = candidate.items.findIndex((item) => item.id === params.id);
        return index > 0 && candidate.items[index - 1].id === parentId;
      });
      if (!route) return HttpResponse.json({ items: [], hasMore: false });
      onSelection(route.label);
      const index = route.items.findIndex((item) => item.id === params.id);
      // Real endpoint order is nearest-first; Breadcrumb reverses it for display.
      return HttpResponse.json({ items: route.items.slice(0, index).toReversed(), hasMore: false });
    }),
  ];
}
