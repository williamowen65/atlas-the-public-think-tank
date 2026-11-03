/** Shared moderator display preference; API authorization remains mandatory. */
export const moderatorVisibility = $state({ includeHidden: true, canModerate: false });

export function setModeratorVisibility(allowed: boolean): void {
  moderatorVisibility.canModerate = allowed;
  if (!allowed) moderatorVisibility.includeHidden = false;
}

export function setIncludeHidden(value: boolean): void {
  if (moderatorVisibility.canModerate) moderatorVisibility.includeHidden = value;
}
