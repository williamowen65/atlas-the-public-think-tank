import { geoGraticule10, geoPath, type GeoPermissibleObjects, type GeoProjection } from 'd3-geo';
import type { projectPin } from './pinGeometry';

export interface CanvasGlobeOptions {
  projection: GeoProjection;
  land?: GeoPermissibleObjects;
  pins: ReturnType<typeof projectPin>[];
  oceanColor: string;
  oceanEdgeColor: string;
  landColor: string;
  gridColor: string;
  outlineColor: string;
  pinColor: string;
  pinHeadColor: string;
  variedHeads: boolean;
  pinStyle: 'round' | 'flat' | 'stem';
  pinHeadRadius: number;
  mini: boolean;
  showGrid: boolean;
}
const graticule = geoGraticule10();

/** Same camera, geometry and back/earth/front ordering as the SVG renderer. */
export function drawCanvasGlobe(ctx: CanvasRenderingContext2D, options: CanvasGlobeOptions) {
  const o = options;
  const path = geoPath(o.projection, ctx);
  const drawPins = (front: boolean) => {
    ctx.lineWidth = o.mini ? 1.2 : 1.5;
    ctx.lineCap = 'round';
    for (const pin of o.pins) {
      ctx.strokeStyle = o.pinColor;
      ctx.beginPath();
      for (const segment of pin.segments) {
        if (segment.front !== front) continue;
        ctx.moveTo(segment.from[0], segment.from[1]);
        ctx.lineTo(segment.to[0], segment.to[1]);
      }
      ctx.stroke();
      if (o.pinStyle === 'stem' || pin.head[2] >= 0 !== front) continue;
      ctx.fillStyle = o.variedHeads ? pin.color : o.pinHeadColor;
      ctx.beginPath();
      ctx.arc(pin.head[0], pin.head[1], o.pinHeadRadius, 0, Math.PI * 2);
      ctx.fill();
      if (o.pinStyle === 'round') {
        ctx.fillStyle = 'rgba(255,255,255,0.3)';
        ctx.beginPath();
        ctx.ellipse(
          pin.head[0] - o.pinHeadRadius * 0.3,
          pin.head[1] - o.pinHeadRadius * 0.35,
          o.pinHeadRadius * 0.4,
          o.pinHeadRadius * 0.23,
          0,
          0,
          Math.PI * 2,
        );
        ctx.fill();
      }
    }
  };
  drawPins(false);
  const ocean = ctx.createRadialGradient(200, 200, 0, 200, 200, 176);
  ocean.addColorStop(0, o.oceanColor);
  ocean.addColorStop(1, o.oceanEdgeColor);
  ctx.beginPath();
  ctx.arc(200, 200, 176, 0, Math.PI * 2);
  ctx.fillStyle = ocean;
  ctx.fill();
  ctx.strokeStyle = o.outlineColor;
  ctx.lineWidth = 1;
  ctx.stroke();
  if (o.showGrid) {
    ctx.beginPath();
    path(graticule);
    ctx.strokeStyle = o.gridColor;
    ctx.globalAlpha = 0.14;
    ctx.lineWidth = 0.6;
    ctx.stroke();
    ctx.globalAlpha = 1;
  }
  if (o.land) {
    ctx.beginPath();
    path(o.land);
    ctx.fillStyle = o.landColor;
    ctx.fill();
    ctx.strokeStyle = o.outlineColor;
    ctx.lineWidth = 0.35;
    ctx.stroke();
  }
  drawPins(true);
}
