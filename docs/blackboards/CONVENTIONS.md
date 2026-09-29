# Atlas blackboard conventions

These conventions capture the visual decisions used to make Atlas workflow
blackboards readable, editable, and traceable. They are guidance for creating a
strong first draft; a blackboard remains an exploratory companion to the
authoritative Markdown requirements and implementation.

## Organize one cohesive subject on one canvas

- Keep related workflows in one Excalidraw file when they share concepts and
  state. Divide the canvas into numbered, titled regions rather than forcing
  every workflow into one long path.
- Give each region a clear local reading direction. A reader should be able to
  understand one region without first tracing every arrow elsewhere.
- Use a dedicated folder for each subject even when it initially contains only
  one blackboard. This leaves room for later views without reorganizing the
  documentation tree.
- Separate implemented behavior from deferred integration. Use solid elements
  for the current slice and dashed elements for future or unresolved work.

## Size text naturally, then size the shape

- Place a separate text element over every workflow shape. Do not use a bound
  container label as the only editable text.
- Let the text element use its natural rendered width and height. Do not stretch
  its bounding box to fill the shape; oversized text bounds make alignment and
  later editing harder to judge.
- Center the natural-size text block horizontally and vertically inside its
  shape. Use intentional line breaks when they clarify a phrase, especially in
  diamonds, rather than relying on accidental wrapping.
- Size the surrounding shape for its content with visible padding. Narrow a box
  when its label is short, but preserve consistent sizing among equivalent
  steps when that consistency aids scanning.
- Give dense summaries enough height to wrap cleanly. Do not compress several
  concepts into a shallow strip merely to preserve a nominal row height.

## Treat spacing as part of the workflow

- Place the major steps first, then reserve open corridors for arrows and their
  labels. Avoid using every available gap for another box.
- Spread branches far enough apart that the decision is visible before the
  reader studies its labels.
- Use the available panel width when it reduces crossings or separates a return
  path from the primary path. Compactness is valuable only while the flow stays
  immediately traceable.
- Align related rows, but allow a step to move off the row when that creates a
  cleaner route. Logical grouping is more important than a perfectly rigid
  grid.
- Refine one numbered region at a time, then review the whole canvas at a zoom
  where section titles and primary paths remain distinguishable.

## Build connections that survive editing

- Bind arrows to the shapes at both ends so connections remain intact when a
  step is dragged.
- Put transition text in an Excalidraw label attached to the arrow. Do not use a
  nearby independent text element to imitate an arrow label.
- Route arrows after the final box placement. Keep lines and arrow labels away
  from node text, neighboring shapes, and unrelated paths.
- Attach arrows to shapes, not to their text elements. Group each shape with its
  separate text element so they move together.

## Make traceability unobtrusive

- Put implementation, test, or requirement links on the visible text element,
  not on the background shape.
- Link a step to the most specific useful source. Link explanatory notes to the
  governing requirement or workflow document when no single method owns the
  behavior.
- Keep link styling subordinate to the workflow. A reader should understand the
  board without opening the source.

## Maintain a consistent visual language

- Blue: application or command flow.
- Orange: decisions, permission checks, and governing rules.
- Green: successful domain results or implemented state.
- Purple: lifecycle, persistence, and audit history.
- Cyan: display, composition, and read behavior.
- Red: rejected operations and error outcomes.
- Gray dashed: deferred work, integration points, and explanatory notes.

Color supports meaning but does not replace labels. Use the same meaning for a
color throughout one canvas.

## What the Node Tags refinement demonstrated

The hand refinement of `NodeTags.excalidraw` established several practical
preferences that should be applied to future generated boards:

1. Text bounds should fit the rendered words and be centered within the box.
2. Long flows should use the width of their region instead of clustering boxes
   around the center.
3. Boxes should be narrowed selectively when their content is short, creating
   more useful negative space for routing.
4. Decision branches need visible separation and room for their arrow labels.
5. Dense summary elements should become taller and wrap naturally rather than
   staying artificially shallow.
6. Layout should be refined region by region without breaking existing groups,
   links, arrow bindings, or attached arrow labels.

## First-draft checklist

Before committing a generated blackboard, verify:

- [ ] Each region has a clear title and local reading direction.
- [ ] Text has natural-size bounds and is centered within its shape.
- [ ] Shapes have consistent, intentional padding.
- [ ] Every shape and its text are grouped.
- [ ] Every workflow arrow is bound at both ends.
- [ ] Every transition label is attached to its arrow.
- [ ] Arrow corridors are clear of node text and unrelated paths.
- [ ] Links are attached to text and open the intended source.
- [ ] Implemented and deferred behavior are visually distinct.
- [ ] The board remains readable both by section and as a whole.

