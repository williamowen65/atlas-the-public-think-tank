/** Shared moderator display preference; API authorization remains mandatory. */
export const moderatorVisibility = $state({ includeHidden: true, canModerate: false });

export function setModeratorVisibility(allowed: boolean): void {
  const wasAllowed = moderatorVisibility.canModerate;
  moderatorVisibility.canModerate = allowed;
  if (allowed && !wasAllowed) moderatorVisibility.includeHidden = true;
  if (!allowed) moderatorVisibility.includeHidden = false;
  if (allowed !== wasAllowed && typeof window !== 'undefined')
    window.dispatchEvent(new Event('atlas-moderator-visibility-changed'));
}

export function setIncludeHidden(value: boolean): void {
  if (moderatorVisibility.canModerate && moderatorVisibility.includeHidden !== value) {
    moderatorVisibility.includeHidden = value;
    if (typeof window !== 'undefined') window.dispatchEvent(new Event('atlas-moderator-visibility-changed'));
  }
}
