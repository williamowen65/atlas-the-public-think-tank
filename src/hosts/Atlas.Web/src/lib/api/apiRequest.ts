/** Optional problem-response fields used to explain throttling and node-creation quotas. */
/** Optional problem fields used to explain API throttling and node-creation quotas. */
type ApiProblem = {
  /** Display title supplied by the API. */
  title?: string;
  /** Quota category reported by the API, such as node_creation. */
  quotaType?: string;
  /** Maximum operations allowed in the reported window. */
  limit?: number;
  /** Quota window duration in seconds. */
  windowSeconds?: number;
  /** Timestamp when another creation slot becomes available, when supplied. */
  nextSlotAt?: string;
};
/**
 * Prefer the node-creation quota details; otherwise use numeric Retry-After seconds or a
 * generic retry message.
 */
function rateLimitMessage(problem: ApiProblem, retryAfter: string | null): string {
  if (problem.quotaType === 'node_creation' && problem.limit && problem.windowSeconds) {
    const hours = problem.windowSeconds / 3600;
    const windowLabel = Number.isInteger(hours)
      ? `${hours} hour${hours === 1 ? '' : 's'}`
      : `${Math.ceil(problem.windowSeconds / 60)} minutes`;
    const next = problem.nextSlotAt ? Date.parse(problem.nextSlotAt) : NaN;
    const wait = Number.isFinite(next)
      ? ` You can create another in about ${Math.max(1, Math.ceil((next - Date.now()) / 60000))} minutes (at ${new Date(next).toLocaleString()}).`
      : '';
    return `You've reached the limit of ${problem.limit} node creations in a rolling ${windowLabel}. Root and child nodes both count.${wait}`;
  }
  if (retryAfter && /^\d+$/.test(retryAfter))
    return `Too many requests. Please try again in about ${Math.max(1, Math.ceil(Number(retryAfter) / 60))} minutes.`;
  return 'Too many requests. Please wait before trying again.';
}
/**
 * Request failure retaining its HTTP status so components can handle sign-in and permission
 * failures.
 */
export class ApiRequestError extends Error {
  constructor(
    message: string,
    public status: number,
  ) {
    super(message);
  }
}
/**
 * Shared JSON transport for session-aware UI requests (and some public reads).
 * @param base Same-origin API prefix; public/session routing is selected by the caller.
 * @param path Relative endpoint without a leading slash.
 * @param method HTTP method; non-GET requests first obtain an antiforgery token.
 * @param body Optional JSON payload; omitted sends no request body.
 * @param signal Optional cancellation signal passed to both token and main requests.
 * @returns Parsed JSON, or undefined for HTTP 204. T does not validate JSON at runtime.
 */
export async function apiRequest<T>(
  base: string,
  path: string,
  method = 'GET',
  body?: unknown,
  signal?: AbortSignal,
): Promise<T> {
  const headers: Record<string, string> = {};
  // Obtain a fresh token using the same routing prefix and session as the mutation.
  if (method !== 'GET') {
    const csrf = await apiRequest<{
      /** Antiforgery token obtained before a state-changing request. */
      token: string;
    }>(base, 'auth/csrf', 'GET', undefined, signal);
    headers['X-CSRF-TOKEN'] = csrf.token;
    if (body !== undefined) headers['Content-Type'] = 'application/json';
  }
  const response = await fetch(`${base}/${path}`, {
    method,
    credentials: 'same-origin',
    headers,
    body: body === undefined ? undefined : JSON.stringify(body),
    signal,
  });
  // Keep user-facing failures consistent while preserving the status for callers.
  if (!response.ok) {
    let problem: ApiProblem = {};
    if (response.status === 429) {
      try {
        problem = (await response.json()) as ApiProblem;
      } catch {
        /* Non-JSON throttling response. */
      }
    }
    throw new ApiRequestError(
      response.status === 401
        ? 'Please sign in to continue.'
        : response.status === 403
          ? 'This action is not permitted. Your session may have expired.'
          : response.status === 429
            ? rateLimitMessage(problem, response.headers.get('Retry-After'))
            : response.status === 400
              ? 'The request was not accepted. Check your text or sign in again.'
              : 'The request could not be completed. Please retry.',
      response.status,
    );
  }
  return response.status === 204 ? (undefined as T) : response.json();
}
