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
/** Browser-memory cache only: never share a user's antiforgery token across SSR requests. */
const csrfTokens = new Map<string, Promise<string>>();
let sessionGeneration = 0;

/** Discard client tokens, including preparations still in flight; this is not server revocation. */
export function clearCsrfTokens() {
  sessionGeneration++;
  csrfTokens.clear();
}

if (typeof window !== 'undefined') {
  window.addEventListener('atlas-session-changed', clearCsrfTokens);
  // Another tab may have changed the browser's shared session cookie while we were away.
  window.addEventListener('focus', clearCsrfTokens);
}

/** Share preparation without allowing one cancelled caller to cancel another caller's token. */
async function csrfToken(base: string, signal?: AbortSignal): Promise<string> {
  signal?.throwIfAborted();
  const generation = sessionGeneration;
  const cached = typeof window !== 'undefined' ? csrfTokens.get(base) : undefined;
  const pending =
    cached ??
    apiRequest<{ token: string }>(
      base,
      'auth/csrf',
      'GET',
      undefined,
      AbortSignal.timeout(10000),
    ).then(({ token }) => {
      if (typeof token !== 'string' || !token) throw new Error('Could not prepare the request.');
      return token;
    });
  if (!cached && typeof window !== 'undefined') csrfTokens.set(base, pending);
  let onAbort: (() => void) | undefined;
  try {
    const token = await (signal
      ? Promise.race([
          pending,
          new Promise<never>((_, reject) => {
            onAbort = () => reject(signal.reason);
            signal.addEventListener('abort', onAbort, { once: true });
            if (signal.aborted) onAbort();
          }),
        ])
      : pending);
    signal?.throwIfAborted();
    if (generation !== sessionGeneration)
      throw new Error('Your session changed. Please try the action again.');
    return token;
  } catch (error) {
    // A cancelled waiter leaves shared preparation available for other callers.
    if (!signal?.aborted && csrfTokens.get(base) === pending) csrfTokens.delete(base);
    throw error;
  } finally {
    if (onAbort) signal?.removeEventListener('abort', onAbort);
    // Evict failed preparations even if all callers cancelled their own wait.
    void pending.catch(() => {
      if (csrfTokens.get(base) === pending) csrfTokens.delete(base);
    });
  }
}

/**
 * Shared JSON transport for session-aware UI requests (and some public reads).
 * @param base Same-origin API prefix; public/session routing is selected by the caller.
 * @param path Relative endpoint without a leading slash.
 * @param method HTTP method; unsafe requests reuse a token for the current identity.
 * @param body Optional JSON payload; omitted sends no request body.
 * @param signal Cancels this caller’s token wait and main request, not shared preparation.
 * @returns Parsed JSON, or undefined for HTTP 204. T does not validate JSON at runtime.
 */
export async function apiRequest<T>(
  base: string,
  path: string,
  method = 'GET',
  body?: unknown,
  signal?: AbortSignal,
): Promise<T> {
  method = method.toUpperCase();
  const unsafe = !['GET', 'HEAD', 'OPTIONS', 'TRACE'].includes(method);
  const identityTransition = unsafe && /^auth\/(login|logout|delete-account)$/.test(path);
  const headers: Record<string, string> = {};
  if (unsafe) headers['X-CSRF-TOKEN'] = await csrfToken(base, signal);
  if (body !== undefined) headers['Content-Type'] = 'application/json';
  let response: Response;
  try {
    response = await fetch(`${base}/${path}`, {
      method,
      credentials: 'same-origin',
      headers,
      body: body === undefined ? undefined : JSON.stringify(body),
      signal,
    });
  } finally {
    // Even a lost response may follow a committed login/logout on the server.
    if (identityTransition) clearCsrfTokens();
  }
  // Do not replay a write. A later explicit action can prepare a new token.
  // 400 includes CSRF rejection but also validation; clearing conservatively is harmless.
  if ([400, 401, 403].includes(response.status)) clearCsrfTokens();
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
