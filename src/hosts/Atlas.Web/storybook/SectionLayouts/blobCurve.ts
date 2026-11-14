const anchors = [
  { x: .50, y: .09, angle: 0, name: 'Top' },
  { x: .91, y: .48, angle: 90, name: 'Right' },
  { x: .48, y: .91, angle: 180, name: 'Bottom' },
  { x: .09, y: .52, angle: 270, name: 'Left' },
] as const;

type Point = { x: number; y: number };
export type BlobTangent = { origin?: Point & { angle: number }; x?: number; y?: number; angle: number; incoming: number; outgoing: number };
export type BlobSide = 'incoming' | 'outgoing';

export const blobLimits = { anchorTravel: .07, angle: 35, minLength: .06, maxLength: .34, maxNodes: 12 };

export function bounded(value: unknown, min: number, max: number, fallback: number) {
  return typeof value === 'number' && Number.isFinite(value) ? Math.max(min, Math.min(max, value)) : fallback;
}

export function blobCurve(tangents?: BlobTangent[], previousRadii?: number[]) {
  const saved = tangents ?? [];
  const entries: { anchor: Point & { angle: number; name: string }; tangent?: BlobTangent; index: number }[] = [];
  let index = 0;
  for (const tangent of saved.slice(0, blobLimits.maxNodes)) {
    if (tangent?.origin) {
      const { x, y, angle } = tangent.origin;
      const origin = { x: bounded(x, -.2, 1.2, .5), y: bounded(y, -.2, 1.2, .5), angle: bounded(angle, -180, 180, 0) };
      entries.push({ anchor: { ...origin, name: `Added ${entries.length + 1}` }, tangent, index: -1 });
    } else if (index < anchors.length) {
      entries.push({ anchor: anchors[index], tangent, index });
      index++;
    }
  }
  for (; index < anchors.length; index++) entries.push({ anchor: anchors[index], index });
  return entries.slice(0, blobLimits.maxNodes).map(({ anchor, tangent, index }) => {
    const previousLength = bounded(previousRadii?.[index * 2], 30, 70, 50) / 250;
    const value = {
      ...(index < 0 ? { origin: { x: anchor.x, y: anchor.y, angle: anchor.angle } } : {}),
      x: bounded(tangent?.x, anchor.x - blobLimits.anchorTravel, anchor.x + blobLimits.anchorTravel, anchor.x),
      y: bounded(tangent?.y, anchor.y - blobLimits.anchorTravel, anchor.y + blobLimits.anchorTravel, anchor.y),
      angle: bounded(tangent?.angle, -blobLimits.angle, blobLimits.angle, [8, -7, 9, -6][index] ?? 0),
      incoming: bounded(tangent?.incoming, 0, blobLimits.maxLength, previousLength),
      outgoing: bounded(tangent?.outgoing, 0, blobLimits.maxLength, previousLength),
    };
    const radians = (anchor.angle + value.angle) * Math.PI / 180;
    const point = (distance: number) => ({
      x: value.x + Math.cos(radians) * distance,
      y: value.y + Math.sin(radians) * distance,
    });
    return { ...anchor, origin: anchor, x: value.x, y: value.y, value, incoming: point(-value.incoming), outgoing: point(value.outgoing) };
  });
}

export function blobPath(nodes: ReturnType<typeof blobCurve>) {
  const point = ({ x, y }: { x: number; y: number }) => `${x.toFixed(5)} ${y.toFixed(5)}`;
  return `M ${point(nodes[0])} ${nodes.map((node, index) => {
    const next = nodes[(index + 1) % nodes.length];
    return `C ${point(node.outgoing)} ${point(next.incoming)} ${point(next)}`;
  }).join(' ')} Z`;
}

function interpolate(a: Point, b: Point, t: number): Point {
  return { x: a.x + (b.x - a.x) * t, y: a.y + (b.y - a.y) * t };
}

function splitSegment(nodes: ReturnType<typeof blobCurve>, index: number, t: number) {
  const start = nodes[index];
  const end = nodes[(index + 1) % nodes.length];
  const a = interpolate(start, start.outgoing, t);
  const b = interpolate(start.outgoing, end.incoming, t);
  const c = interpolate(end.incoming, end, t);
  const incoming = interpolate(a, b, t);
  const outgoing = interpolate(b, c, t);
  return { incoming, outgoing, point: interpolate(incoming, outgoing, t) };
}

export function insertBlobNode(nodes: ReturnType<typeof blobCurve>, index: number, target = splitSegment(nodes, index, .5).point) {
  if (nodes.length >= blobLimits.maxNodes) return;
  const distance = (a: Point, b: Point) => Math.hypot(a.x - b.x, a.y - b.y);
  const error = (t: number) => distance(splitSegment(nodes, index, t).point, target);
  let best = 0;
  for (let sample = 1; sample <= 40; sample++) {
    if (error(sample / 40) < error(best)) best = sample / 40;
  }
  let left = Math.max(0, best - .025);
  let right = Math.min(1, best + .025);
  for (let step = 0; step < 18; step++) {
    const a = (2 * left + right) / 3;
    const b = (left + 2 * right) / 3;
    if (error(a) < error(b)) right = b;
    else left = a;
  }
  const t = (left + right) / 2;
  const nextIndex = (index + 1) % nodes.length;
  const { point, incoming, outgoing } = splitSegment(nodes, index, t);
  if (distance(point, nodes[index]) < .04 || distance(point, nodes[nextIndex]) < .04) return;
  const angle = Math.atan2(outgoing.y - incoming.y, outgoing.x - incoming.x) * 180 / Math.PI;
  const values: BlobTangent[] = nodes.map(({ value }) => ({ ...value }));
  values[index].outgoing *= t;
  values[nextIndex].incoming *= 1 - t;
  values.splice(index + 1, 0, {
    origin: { ...point, angle }, ...point, angle: 0,
    incoming: distance(incoming, point), outgoing: distance(outgoing, point),
  });
  return values;
}

export function blobSegment(nodes: ReturnType<typeof blobCurve>, index: number) {
  const start = nodes[index];
  const end = nodes[(index + 1) % nodes.length];
  return `M ${start.x} ${start.y} C ${start.outgoing.x} ${start.outgoing.y} ${end.incoming.x} ${end.incoming.y} ${end.x} ${end.y}`;
}
