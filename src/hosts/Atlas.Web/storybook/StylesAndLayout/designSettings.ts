/** Storybook-only experiments. Empty colors inherit the selected Theme. */
export const designDefaults = {
  fontFamily: 'system-ui, sans-serif', fontSize: 16, lineHeight: 1.6, headingScale: 1.5,
  paragraphGap: 16, headingGap: 12, space: 0, cardPadding: 20, cardPaddingNarrow: 12,
  dialTextGap: 16, radius: 8, background: '', surface: '', text: '', muted: '', accent: '', onAccent: '', border: '', focus: '',
};
export type DesignSettings = typeof designDefaults;
export const designTokens: Record<keyof DesignSettings, string> = {
  fontFamily: '--atlas-font-family', fontSize: '--atlas-font-size', lineHeight: '--atlas-line-height', headingScale: '--atlas-heading-scale',
  paragraphGap: '--atlas-paragraph-gap', headingGap: '--atlas-heading-gap', space: '--atlas-space', cardPadding: '--atlas-card-padding', cardPaddingNarrow: '--atlas-card-padding-narrow', dialTextGap: '--atlas-dial-text-gap', radius: '--atlas-radius',
  background: '--atlas-bg', surface: '--atlas-surface', text: '--atlas-text', muted: '--atlas-muted', accent: '--atlas-accent', onAccent: '--atlas-on-accent', border: '--atlas-border', focus: '--atlas-focus',
};
export function applyDesign(settings: Partial<DesignSettings>) {
  for (const [key, token] of Object.entries(designTokens)) {
    const value = settings[key as keyof DesignSettings];
    if (value === undefined || value === '' || (key === 'space' && value === 0)) document.documentElement.style.removeProperty(token);
    else document.documentElement.style.setProperty(token, typeof value === 'number' && !['lineHeight', 'headingScale'].includes(key) ? `${value}px` : String(value));
  }
}
