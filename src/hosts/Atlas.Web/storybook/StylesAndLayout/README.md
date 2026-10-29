# Atlas design system in Storybook

The **Styles and Layout / Atlas Design System / Overview** story is a **read-only visual inventory** of existing Atlas styles. It is deliberately not a replacement theme editor. It shows color roles and live CSS values, spacing, shared panels, and examples composed from actual application components. **The typography table reports values computed from the actual rendered node card**, not a separate heading mockup. It separates global root text defaults from locally styled card title, byline, description and details. Other components may use different local rules.

## Source of truth

- **Application styles:** `src/styles.scss` defines the shared light/dark theme variables, base typography, global spacing and panel treatment. The SPA and Storybook both import this stylesheet.
- **Component styles:** The real `src/lib/components/*.svelte` components contain component-specific style rules and token fallbacks. The Overview reuses components so updates to those files appear in both places.
- **Storybook experiments:** `storybook/StylesAndLayout/designSettings.ts` and the existing Typography Playground provide optional global overrides for investigating changes. They **do not** save to the app, and they are not a second source of truth.

## How to use it

1. Open **Styles and Layout / Atlas Design System / Overview** in Storybook.
2. Review the active color values, spacing, typography, action states and composed node card together. Use the Theme and Density toolbar settings and resize the viewport to inspect differences.
3. If something looks wrong, **edit the relevant shared SCSS or component styles**. The dev servers will show the same source changes in Storybook and the SPA.
4. Inspect the actual SPA to verify the whole page, and run the existing Storybook/Playwright tests before merging.
5. Use the separate component stories when you need more detailed states or API-driven workflows.

## Current limits

This is a first-pass reference for *existing* styling, not a finished design standard or a complete catalog of every form/control. Some CSS values are component-level fallbacks instead of root variables; the overview identifies these rather than silently assigning new defaults. The interactive style Controls in the old Typography Playground are retained for backwards compatibility with current tests, but this Overview introduces no new editing knobs. A later PR could simplify those Controls once the new reference proves useful.

Related: `docs/COMPONENT-CATALOG.md` contains application component contracts rather than visual styling guidelines.
