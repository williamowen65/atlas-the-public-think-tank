export interface ActionButtonProps {
  label: string;
  disabled?: boolean;
  onActivate?: () => void;
}

/** Render a native keyboard-accessible button; the caller owns its action. */
export function createActionButton({
  label,
  disabled = false,
  onActivate,
}: ActionButtonProps): HTMLButtonElement {
  const button = document.createElement('button');
  button.type = 'button';
  button.className = 'atlas-action';
  button.textContent = label;
  button.disabled = disabled;

  // Native disabled behavior also prevents activation through keyboard input.
  button.addEventListener('click', () => onActivate?.());

  return button;
}
