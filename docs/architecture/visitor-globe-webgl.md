# Learning WebGL through the Atlas globe

This guide follows the code in this branch. Start with the Storybook playground, then read the implementation a piece at a time. You do not need to learn a game engine first.

## What WebGL does here

The browser still displays a rectangular `<canvas>`. WebGL supplies its pixels using small programs called **shaders**, usually executed on the GPU. JavaScript prepares the data, sends it to WebGL, and asks it to draw. The GPU can run the same calculations across many vertices or pixels in parallel.

| Renderer | What our code asks the browser to do during rotation |
| --- | --- |
| SVG | Update the coordinates of individual shaft, head and highlight elements, plus projected map paths. |
| Canvas 2D | Project the map and pins in JavaScript, then issue drawing commands to one bitmap. |
| WebGL 2 | Reuse uploaded positions and a map image; change the camera angle and draw batches on the GPU. |

WebGL is the sole globe renderer in both the application and Storybook; the earlier SVG and Canvas 2D implementations were removed after the visual comparison. The table above explains the approaches we explored.

D3 still has a role. It produces the geographic map image that WebGL samples. We use native WebGL 2, with no Three.js or scene-graph library. The view keeps the same orthographic presentation: distant parts do not shrink as they would with a perspective camera.

## A reading order

Paths below are relative to this guide. The frontend root is `src/hosts/Atlas.Web`.

| Read | Responsibility |
| --- | --- |
| [VisitorGlobe.svelte](../../src/hosts/Atlas.Web/src/lib/components/VisitorGlobe.svelte) | Loads local map data, prepares pin geometry, selects the renderer. |
| [pinGeometry.ts](../../src/hosts/Atlas.Web/src/lib/globe/pinGeometry.ts) | Converts latitude/longitude into fixed 3D positions and a repeatable outward lean. |
| [WebGLGlobe.svelte](../../src/hosts/Atlas.Web/src/lib/components/WebGLGlobe.svelte) | Connects Svelte props, canvas size, context lifecycle and drawing. |
| [webglRenderer.ts](../../src/hosts/Atlas.Web/src/lib/globe/webglRenderer.ts) | Creates shaders, uploads data and issues GPU drawing commands. |
| [VisitorGlobeExample.svelte](../../src/hosts/Atlas.Web/storybook/VisitorGlobe/VisitorGlobeExample.svelte) | Supplies demo settings and the shared animation clock. |
| [globe-webgl.spec.ts](../../src/hosts/Atlas.Web/tests/globe-webgl.spec.ts) | Verifies uploads, drawing, cleanup, unavailable contexts and restoration. |

## The vocabulary in our code

| Term | Meaning here |
| --- | --- |
| Vertex | A corner of a triangle. Two triangles make a rectangular drawing surface. |
| Vertex shader | Positions those corners in the view, using pin data and the current camera angle. |
| Fragment shader | Calculates the color and depth for a candidate pixel covered by a triangle. |
| Program | A linked vertex shader and fragment shader. We have an Earth program and a pin program. |
| Attribute | Input associated with vertices or instances: our pin base, head and color. |
| Uniform | A value shared by a draw call, such as camera angle, head radius or shaft color. |
| Buffer | GPU storage containing the numeric pin records uploaded by JavaScript. |
| VAO | A vertex array object. It remembers how to interpret that buffer as attributes. |
| Texture | An image the fragment shader can sample. Ours contains land and grid lines. |
| Instance | Another copy of a shape, positioned using a different pin record. |
| Depth buffer | Stored distances that let WebGL reject surfaces hidden by nearer surfaces. |

The shader strings use GLSL, a different language from TypeScript. `vec2`, `vec3` and `vec4` hold two, three and four numbers. `mix(a, b, t)` interpolates between values. `uniform` declares shared inputs, while `in` and `out` connect stages. `gl_Position` is the vertex shader's output position; our fragment shaders write their output color and, for Earth, an explicit depth.

## Follow one pin from coordinates to pixels

`preparePin()` first turns longitude and latitude into a point on a sphere:

```text
x = cos(latitude) * cos(longitude)
y = cos(latitude) * sin(longitude)
z = sin(latitude)
```

Angles must be in radians. This unit vector points outward from Earth's center. Multiplying it by the globe radius, 176, gives the shaft's base.

The head is farther along an outward direction. A small contribution from the local east/north directions creates lean. Its bearing comes from a deterministic function of the location, so it stays attached to the same place during rotation and snapshot refreshes. A saved pin supplies its user-selected hex color. Synthetic Storybook fixtures choose palette colors independently of location and lean; missing colors retain a deterministic palette fallback for standalone renderer fixtures. The API supplies red for older persisted pins without a saved color.

`setPins()` packs each pin into nine floating-point values: three for its base, three for its head, and three for its RGB color. A `Float32Array` holds these records. Each record occupies 36 bytes; 10,000 records occupy about 360,000 bytes before other graphics resources. `bufferData(..., STATIC_DRAW)` uploads them. The usage hint describes our intent to reuse the data; it is not a prohibition on future updates.

`vertexAttribPointer()` describes each field's size and offset. `vertexAttribDivisor(..., 1)` tells WebGL to advance that field once per **pin instance**, rather than once per triangle corner.

The pin vertex shader's `camera()` rotates the 3D positions around Earth, then applies the same 18-degree tilt used by D3. Screen X/Y determine placement; the remaining coordinate supplies depth. Dividing by 220 maps our 440-unit view into WebGL's visible horizontal and vertical range of -1 to 1.

For a shaft, the shader forms a thin rectangle between the projected base and head. For a head, it forms a square centered on the projected head; the fragment shader discards pixels outside a circle. Round heads add a small elliptical highlight. These are **camera-facing shapes positioned in 3D**, not detailed spherical pin meshes. This preserves the existing illustration style.

## Why two pin draw calls can draw thousands of pins

The `corners` shader constant describes six vertices: two triangles forming one rectangle. `gl_VertexID` selects a corner without needing a separate corner buffer.

`drawArraysInstanced(TRIANGLES, 0, 6, count)` repeats that rectangle for every pin record. One call draws all shafts; a second draws all heads. Stem-only mode skips the head call. Earth uses one more draw call.

The number of calls stays small as the pin count grows. The GPU still processes more triangles and pixels for more pins; instancing does not make that work free. Dense overlapping heads can remain expensive and visually cluttered.

## How the Earth appears round without a sphere mesh

`setMap()` uses D3's equirectangular projection to draw land and grid lines into a 2048 × 1024 image. The ocean is transparent in this image. `texImage2D()` uploads it as a texture. This happens when the map or its relevant styles change, rather than on every camera update.

The Earth vertex shader covers the view with two triangles. The fragment shader treats the center of that view as a sphere. Pixels outside radius 176 are discarded. For a pixel inside the disc, it computes the sphere's front depth:

```text
depth = sqrt(radius * radius - screenX * screenX - screenY * screenY)
```

The shader reverses the camera rotation to find that surface point's longitude and latitude, converts them to texture coordinates, then samples the map image. The transparent ocean areas receive our center-to-edge ocean colors. A narrow edge band supplies the outline. `fwidth()` helps soften pixel boundaries; it estimates how quickly a value changes across neighboring fragments.

This method is an analytic sphere drawn by a shader. It retains real surface depth without a tessellated globe mesh. Texture resolution still affects coastline sharpness, so it may look different from vector SVG when enlarged. Increasing texture resolution trades additional memory and upload work for map detail; it does not reduce pin cost.

## How pins disappear behind Earth

The Earth fragment shader writes its calculated front-surface depth into `gl_FragDepth`. The pin shader supplies each shaft/head's depth through its projected position.

`DEPTH_TEST` and `depthFunc(LEQUAL)` allow a pixel to draw when it is at least as near as the pixel already stored at that location. A shaft behind Earth's surface fails that test. A raised tip beyond Earth's visible disc has no Earth pixel in front of it, so it can still appear around the horizon.

The earlier SVG/Canvas experiment used back/earth/front drawing order. The current implementation handles overlap with the depth buffer.

Head sprites use the depth of their centers across their circular faces. They are an approximation, so their silhouettes are not identical to physical spherical knobs intersecting Earth.

## What updates, and when?

| Trigger | Work |
| --- | --- |
| Pin locations, length or lean change | Prepare fixed positions and upload pin records. |
| Head palette/single color changes | Upload updated color records. |
| Land data, grid or map styling changes | Rebuild and upload the D3 texture as needed. |
| Other appearance settings change | Update shader uniforms; some prop changes can also refresh setup data. |
| Rotation tick | Update angle uniforms, clear the drawing buffers and issue drawing calls. No pin-buffer or map-texture upload. |
| Canvas resize or display density change | Resize the backing buffer and draw at the new resolution. |

Svelte effects connect these updates to the renderer. The pin geometry is separate from the angle, so changing the angle does not recreate every geographic pin record. Uniform locations are cached rather than looked up repeatedly during rotation.

The Storybook readout counts delivered rotation updates per second. It does not measure completed GPU frames. A busy GPU can lag behind submitted commands, so use browser performance tools and visual inspection alongside that number. API polling remains independent of the animation clock.

## Lifecycle, compatibility and limitations

`getContext('webgl2')` can fail even in a browser that implements WebGL 2. Hardware, drivers and browser policy matter. This feature does not explicitly require a discrete graphics card; compatible integrated graphics can run WebGL. Actual smoothness needs testing on target devices.

The component listens for `webglcontextlost` and stops drawing when that happens. Calling `preventDefault()` allows restoration; when `webglcontextrestored` arrives, it rebuilds programs, buffers and the texture. Unmounting the globe calls `dispose()`, which deletes allocated graphics resources. The expanded globe is mounted only while its dialog is open, so closing it releases those resources. Its angle comes from the shared clock, keeping it synchronized with the miniature when reopened. Initialization errors are reported in the console and explained in the expanded view. The existing account counts and location text remain available; there is no alternate drawing backend. Both the miniature and expanded view use WebGL.

We provide one accessible description for the picture. There are no individual pin focus targets or hover controls in this renderer. A separate textual breakdown could be added later.

The tests run real shaders using Chromium's software WebGL implementation. They check that rotation does not re-upload geometry/texture data, confirm unavailable-state handling and recovery, and exercise a 10,000-pin fixture. They do not establish hardware FPS or a production capacity limit.

## Try it in Storybook

From `src/hosts/Atlas.Web`:

```sh
npm run storybook
```

Open **Components → VisitorGlobe → Playground**. There is no renderer selector: WebGL is the sole implementation. Keep size, distribution, speed and target FPS fixed while trying 100, 1,000, 5,000 and 10,000 pins. Compare round heads with stems only, and test the miniature separately. The application targets 60 updates per second; Storybook can lower the target for experimentation.

## References

- [MDN: WebGL overview and browser compatibility](https://developer.mozilla.org/en-US/docs/Web/API/WebGL_API)
- [MDN: Vertex and fragment shaders in a first WebGL drawing](https://developer.mozilla.org/en-US/docs/Web/API/WebGL_API/Tutorial/Adding_2D_content_to_a_WebGL_context)
- [MDN: drawArraysInstanced](https://developer.mozilla.org/en-US/docs/Web/API/WebGL2RenderingContext/drawArraysInstanced)
- [MDN: depthFunc](https://developer.mozilla.org/en-US/docs/Web/API/WebGLRenderingContext/depthFunc)
- [MDN: webglcontextlost](https://developer.mozilla.org/en-US/docs/Web/API/HTMLCanvasElement/webglcontextlost_event)
- [D3: Geographic paths and Canvas contexts](https://d3js.org/d3-geo/path)
