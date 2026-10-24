# Svelte actions: `use:`

An action is a function that adds DOM behavior to an element. Svelte's `use:` directive connects that function to the element’s lifetime. Here, both actions measure layout and observe resizing; neither is a click handler or an API request.

```svelte
<script lang="ts">
  import { centerInCard } from '$lib/actions/centerInCard';
</script>

<!-- Inside an article with the matching card CSS: -->
<span class="read-more" use:centerInCard>...</span>
```

When the element mounts in the browser, Svelte calls `centerInCard(element)`, passing the actual `HTMLSpanElement`. You write the function’s name after `use:`; you do not call it yourself. The directive is Svelte syntax, not an HTML attribute that the browser implements. Actions do not run during server-side rendering.

The function can return lifecycle hooks:

- `destroy()` runs when the element is removed, including conditional removal or navigating away. Our actions use it to disconnect observers and cancel scheduled work.
- `update(value)` is optional. For an action with a parameter, `use:someAction={value}` passes that value as the second function argument, and Svelte calls the returned `update` hook when the parameter changes. Neither action in this folder currently accepts a parameter or needs an update hook.

An action is not automatically called again on every render. These actions react to layout changes through the browser’s `ResizeObserver`. `requestAnimationFrame` schedules work before a repaint; its return value is a cancellation handle, not a clock.

## Actions in this folder

| Action               | Attach to                                       | Purpose and assumptions                                                                                                                                                                                                                      |
| -------------------- | ----------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `centerInCard`       | The `.read-more` wrapper inside an `article`    | Measures the horizontal offset needed to center the wrapper against the whole card. Writes `--card-center-offset`, which the card’s CSS uses. Without an article it does nothing. Used by context-below, ribbon and responsive-ribbon cards. |
| `bottomCornerAction` | The description element containing `.read-more` | Reserves space and writes `--corner-top` for a bottom-right action. Requires an enclosing article with a header and `.voting-dial`, plus matching card CSS. Used by the ribbon-corner card.                                                  |

These functions are specific to the card markup and CSS; attaching one to an unrelated element does not recreate the card layout. Prefer ordinary Svelte markup/CSS for layout that does not need DOM measurements.
