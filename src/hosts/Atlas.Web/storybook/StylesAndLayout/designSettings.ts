/** Storybook-only experiments. Empty colors inherit the selected Theme. */
export const designDefaults = {
  /** CSS font-family stack for workbench experiments. */
  fontFamily: 'system-ui, sans-serif',
  /** Base font size in pixels. */
  fontSize: 16,
  /** Unitless text line-height multiplier. */
  lineHeight: 1.6,
  /** Heading size multiplier for the typography specimen and shared tokens. */
  headingScale: 1.5,
  /** Paragraph spacing in pixels. */
  paragraphGap: 16,
  /** Heading spacing in pixels. */
  headingGap: 12,
  /** Shared spacing override in pixels; zero follows Storybook Density. */
  space: 0,
  /** Standard card padding in pixels. */
  cardPadding: 20,
  /** Narrow-screen card padding in pixels. */
  cardPaddingNarrow: 12,
  /** Gap between the voting dial and adjacent text, in pixels. */
  dialTextGap: 16,
  /** Shared corner radius in pixels. */
  radius: 8,
  /** Page background CSS color; empty uses the selected theme. */
  background: '',
  /** Panel surface CSS color; empty uses the selected theme. */
  surface: '',
  /** Primary text CSS color; empty uses the selected theme. */
  text: '',
  /** Secondary text CSS color; empty uses the selected theme. */
  muted: '',
  /** Accent CSS color; empty uses the selected theme. */
  accent: '',
  /** Text color over accent surfaces; empty uses the selected theme. */
  onAccent: '',
  /** Border CSS color; empty uses the selected theme. */
  border: '',
  /** Keyboard-focus CSS color; empty uses the selected theme. */
  focus: '',
};
export type DesignSettings = typeof designDefaults;
export const designTokens: Record<keyof DesignSettings, string> = {
  fontFamily: '--atlas-font-family',
  fontSize: '--atlas-font-size',
  lineHeight: '--atlas-line-height',
  headingScale: '--atlas-heading-scale',
  paragraphGap: '--atlas-paragraph-gap',
  headingGap: '--atlas-heading-gap',
  space: '--atlas-space',
  cardPadding: '--atlas-card-padding',
  cardPaddingNarrow: '--atlas-card-padding-narrow',
  dialTextGap: '--atlas-dial-text-gap',
  radius: '--atlas-radius',
  background: '--atlas-bg',
  surface: '--atlas-surface',
  text: '--atlas-text',
  muted: '--atlas-muted',
  accent: '--atlas-accent',
  onAccent: '--atlas-on-accent',
  border: '--atlas-border',
  focus: '--atlas-focus',
};
export function applyDesign(settings: Partial<DesignSettings>) {
  for (const [key, token] of Object.entries(designTokens)) {
    const value = settings[key as keyof DesignSettings];
    if (value === undefined || value === '' || (key === 'space' && value === 0))
      document.documentElement.style.removeProperty(token);
    else
      document.documentElement.style.setProperty(
        token,
        typeof value === 'number' && !['lineHeight', 'headingScale'].includes(key)
          ? `${value}px`
          : String(value),
      );
  }
}
