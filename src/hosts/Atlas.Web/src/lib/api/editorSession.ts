import { readable } from 'svelte/store';
import { apiRequest } from './apiRequest';

/** One session read for all visible edit controls; never treat browser storage as identity. */
export const editorSession = readable<string | null>(null, (set) => {
  if (typeof window === 'undefined') return;
  let controller: AbortController | undefined;
  const refresh = () => {
    controller?.abort();
    const current = (controller = new AbortController());
    set(null);
    void apiRequest<{ participantId: string | null }>(
      '/atlas-session-api',
      'auth/session',
      'GET',
      undefined,
      AbortSignal.any([current.signal, AbortSignal.timeout(10000)]),
    )
      .then((session) => {
        if (!current.signal.aborted) set(session.participantId);
      })
      .catch(() => {
        if (!current.signal.aborted) set(null);
      });
  };
  refresh();
  window.addEventListener('atlas-session-changed', refresh);
  return () => {
    controller?.abort();
    window.removeEventListener('atlas-session-changed', refresh);
  };
});
