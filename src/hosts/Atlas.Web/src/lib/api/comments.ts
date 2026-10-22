export class CommentRequestError extends Error { constructor(message: string, public status: number) { super(message); } }
export async function commentRequest<T>(base: string, path: string, method = 'GET', body?: unknown, signal?: AbortSignal): Promise<T> {
  const headers: Record<string, string> = {};
  if (method !== 'GET') {
    const csrf = await commentRequest<{ token: string }>(base, 'auth/csrf', 'GET', undefined, signal);
    headers['X-CSRF-TOKEN'] = csrf.token;
    if (body !== undefined) headers['Content-Type'] = 'application/json';
  }
  const response = await fetch(`${base}/${path}`, { method, credentials: 'same-origin', headers, body: body === undefined ? undefined : JSON.stringify(body), signal });
  if (!response.ok) throw new CommentRequestError(response.status === 401 ? 'Please sign in to continue.' : response.status === 403 ? 'This action is not permitted. Your session may have expired.' : response.status === 429 ? 'Too many requests. Please wait before trying again.' : response.status === 400 ? 'The request was not accepted. Check your text or sign in again.' : 'The request could not be completed. Please retry.', response.status);
  return response.status === 204 ? undefined as T : response.json();
}
