type ApiProblem = { title?: string; quotaType?: string; limit?: number; windowSeconds?: number; nextSlotAt?: string };
function rateLimitMessage(problem: ApiProblem, retryAfter: string | null): string {
  if (problem.quotaType === 'node_creation' && problem.limit && problem.windowSeconds) {
    const hours = problem.windowSeconds / 3600;
    const windowLabel = Number.isInteger(hours) ? `${hours} hour${hours === 1 ? '' : 's'}` : `${Math.ceil(problem.windowSeconds / 60)} minutes`;
    const next = problem.nextSlotAt ? Date.parse(problem.nextSlotAt) : NaN;
    const wait = Number.isFinite(next) ? ` You can create another in about ${Math.max(1, Math.ceil((next - Date.now()) / 60000))} minutes (at ${new Date(next).toLocaleString()}).` : '';
    return `You've reached the limit of ${problem.limit} node creations in a rolling ${windowLabel}. Root and child nodes both count.${wait}`;
  }
  if (retryAfter && /^\d+$/.test(retryAfter)) return `Too many requests. Please try again in about ${Math.max(1, Math.ceil(Number(retryAfter) / 60))} minutes.`;
  return 'Too many requests. Please wait before trying again.';
}
export class CommentRequestError extends Error { constructor(message: string, public status: number) { super(message); } }
export async function commentRequest<T>(base: string, path: string, method = 'GET', body?: unknown, signal?: AbortSignal): Promise<T> {
  const headers: Record<string, string> = {};
  if (method !== 'GET') {
    const csrf = await commentRequest<{ token: string }>(base, 'auth/csrf', 'GET', undefined, signal);
    headers['X-CSRF-TOKEN'] = csrf.token;
    if (body !== undefined) headers['Content-Type'] = 'application/json';
  }
  const response = await fetch(`${base}/${path}`, { method, credentials: 'same-origin', headers, body: body === undefined ? undefined : JSON.stringify(body), signal });
  if (!response.ok) {
    let problem: ApiProblem = {};
    if (response.status === 429) { try { problem = await response.json() as ApiProblem; } catch { /* Non-JSON throttling response. */ } }
    throw new CommentRequestError(response.status === 401 ? 'Please sign in to continue.' : response.status === 403 ? 'This action is not permitted. Your session may have expired.' : response.status === 429 ? rateLimitMessage(problem, response.headers.get('Retry-After')) : response.status === 400 ? 'The request was not accepted. Check your text or sign in again.' : 'The request could not be completed. Please retry.', response.status);
  }
  return response.status === 204 ? undefined as T : response.json();
}
