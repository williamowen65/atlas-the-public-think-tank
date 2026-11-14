const anchors = [
  { x: .50, y: .09, angle: 0, name: 'Top' },
  { x: .91, y: .48, angle: 90, name: 'Right' },
  { x: .48, y: .91, angle: 180, name: 'Bottom' },
  { x: .09, y: .52, angle: 270, name: 'Left' },
] as const;

export type BlobTangent = { angle: number; incoming: number; outgoing: number };
export type BlobSide = 'incoming' | 'outgoing';

function bounded(value: unknown, min: number, max: number, fallback: number) {
  return typeof value === 'number' && Number.isFinite(value) ? Math.max(min, Math.min(max, value)) : fallback;
}

export function blobCurve(tangents?: BlobTangent[], previousRadii?: number[]) {
  return anchors.map((anchor, index) => {
    const tangent = tangents?.[index];
    const previousLength = bounded(previousRadii?.[index * 2], 30, 70, 50) / 250;
    const value = {
      angle: bounded(tangent?.angle, -20, 20, [8, -7, 9, -6][index]),
      incoming: bounded(tangent?.incoming, .09, .25, previousLength),
      outgoing: bounded(tangent?.outgoing, .09, .25, previousLength),
    };
    const radians = (anchor.angle + value.angle) * Math.PI / 180;
    const point = (distance: number) => ({
      x: anchor.x + Math.cos(radians) * distance,
      y: anchor.y + Math.sin(radians) * distance,
    });
    return { ...anchor, value, incoming: point(-value.incoming), outgoing: point(value.outgoing) };
  });
}

export function blobPath(nodes: ReturnType<typeof blobCurve>) {
  const point = ({ x, y }: { x: number; y: number }) => `${x.toFixed(5)} ${y.toFixed(5)}`;
  return `M ${point(nodes[0])} ${nodes.map((node, index) => {
    const next = nodes[(index + 1) % nodes.length];
    return `C ${point(node.outgoing)} ${point(next.incoming)} ${point(next)}`;
  }).join(' ')} Z`;
}
