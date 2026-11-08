import { geoEquirectangular, geoGraticule10, geoPath } from 'd3-geo';
import type { CanvasGlobeOptions } from './canvasRenderer';
import type { PreparedPin } from './pinGeometry';
export type WebGLGlobeOptions = Omit<CanvasGlobeOptions, 'projection' | 'pins'> & {
  pins: PreparedPin[];
};

const camera = `
vec3 camera(vec3 p) {
  float c = cos(angle), s = sin(angle);
  float forward = p.x*c-p.y*s;
  return vec3(p.x*s+p.y*c, p.z*0.9510565163-forward*0.3090169944,
    forward*0.9510565163+p.z*0.3090169944);
}`;
const quad = `const vec2 corners[6] = vec2[6](vec2(-1,-1),vec2(1,-1),vec2(-1,1),vec2(-1,1),vec2(1,-1),vec2(1,1));`;
const earthVertex = `#version 300 es
precision highp float;
precision highp int;
out vec2 position;
${quad}
void main() { position=corners[gl_VertexID]*220.0; gl_Position=vec4(corners[gl_VertexID],0,1); }`;
const earthFragment = `#version 300 es
precision highp float;
precision highp int;
in vec2 position;
uniform float angle;
uniform sampler2D mapTexture;
uniform vec3 ocean, edge, outline;
out vec4 color;
void main() {
  float radius=length(position);
  if(radius>176.0) discard;
  float depth=sqrt(max(0.0,176.0*176.0-dot(position,position)));
  float forward=depth*0.9510565163-position.y*0.3090169944;
  float north=position.y*0.9510565163+depth*0.3090169944;
  float x=forward*cos(angle)+position.x*sin(angle);
  float y=-forward*sin(angle)+position.x*cos(angle);
  vec2 uv=vec2(atan(y,x)/6.28318530718+0.5,asin(clamp(north/176.0,-1.0,1.0))/3.14159265359+0.5);
  vec4 map=texture(mapTexture,uv);
  vec3 background=mix(ocean,edge,radius/176.0);
  vec3 surface=mix(background,map.rgb,map.a);
  surface=mix(surface,outline,smoothstep(175.2,176.0,radius));
  color=vec4(surface,1.0-smoothstep(176.0-fwidth(radius),176.0,radius));
  gl_FragDepth=0.5-depth/500.0;
}`;
const pinVertex = `#version 300 es
precision highp float;
precision highp int;
layout(location=0) in vec3 base;
layout(location=1) in vec3 head;
layout(location=2) in vec3 headColor;
uniform float angle, radius, shaftWidth;
uniform int kind;
out vec2 local;
out vec3 tint;
${camera}
${quad}
void main() {
  vec2 corner=corners[gl_VertexID];
  vec3 a=camera(base), b=camera(head);
  vec3 p=b;
  if(kind==0) {
    float t=corner.y*0.5+0.5;
    p=mix(a,b,t);
    vec2 direction=b.xy-a.xy;
    vec2 perpendicular=vec2(-direction.y,direction.x)/max(length(direction),0.00001);
    p.xy+=perpendicular*corner.x*shaftWidth*0.5;
  } else { p.xy+=corner*radius; }
  local=corner; tint=headColor;
  gl_Position=vec4(p.xy/220.0,-p.z/250.0,1);
}`;
const pinFragment = `#version 300 es
precision highp float;
precision highp int;
in vec2 local;
in vec3 tint;
uniform int kind, roundHead;
uniform vec3 shaft;
out vec4 color;
void main() {
  if(kind==0) { color=vec4(shaft,1); return; }
  float distance=length(local);
  if(distance>1.0) discard;
  vec3 fill=tint;
  if(roundHead==1 && length((local-vec2(-0.3,0.35))/vec2(0.4,0.23))<1.0) fill=mix(fill,vec3(1),0.3);
  color=vec4(fill,1.0-smoothstep(1.0-fwidth(distance),1.0,distance));
}`;

function rgb(value: string): [number, number, number] {
  // Browser CSS color controls may emit hex, rgb() or named colors.
  const context = document.createElement('canvas').getContext('2d')!;
  context.fillStyle = value;
  context.fillRect(0, 0, 1, 1);
  const pixel = context.getImageData(0, 0, 1, 1).data;
  return [pixel[0] / 255, pixel[1] / 255, pixel[2] / 255];
}

/** Upload stationary geometry once; rotation changes only a GPU camera uniform. */
export function createWebGLGlobe(canvas: HTMLCanvasElement) {
  const gl = canvas.getContext('webgl2', { alpha: true, antialias: true });
  if (!gl) throw new Error('WebGL 2 is unavailable');
  const programs: WebGLProgram[] = [];
  const buffers: WebGLBuffer[] = [];
  const shaders: WebGLShader[] = [];
  const vaos: WebGLVertexArrayObject[] = [];
  const texture = gl.createTexture()!;
  function dispose() {
    buffers.forEach((buffer) => gl!.deleteBuffer(buffer));
    vaos.forEach((vao) => gl!.deleteVertexArray(vao));
    programs.forEach((program) => gl!.deleteProgram(program));
    shaders.forEach((shader) => gl!.deleteShader(shader));
    gl!.deleteTexture(texture);
  }
  function program(vertex: string, fragment: string) {
    const result = gl!.createProgram()!;
    programs.push(result);
    for (const [type, source] of [
      [gl!.VERTEX_SHADER, vertex],
      [gl!.FRAGMENT_SHADER, fragment],
    ] as const) {
      const shader = gl!.createShader(type)!;
      shaders.push(shader);
      gl!.shaderSource(shader, source);
      gl!.compileShader(shader);
      if (!gl!.getShaderParameter(shader, gl!.COMPILE_STATUS))
        throw new Error(gl!.getShaderInfoLog(shader) || 'Shader compilation failed');
      gl!.attachShader(result, shader);
    }
    gl!.linkProgram(result);
    if (!gl!.getProgramParameter(result, gl!.LINK_STATUS))
      throw new Error(gl!.getProgramInfoLog(result) || 'Shader link failed');
    return result;
  }
  try {
    const earth = program(earthVertex, earthFragment);
    const pins = program(pinVertex, pinFragment);
    const earthVao = gl.createVertexArray()!;
    vaos.push(earthVao);
    const pinVao = gl.createVertexArray()!;
    vaos.push(pinVao);
    const buffer = gl.createBuffer()!;
    buffers.push(buffer);
    gl.bindVertexArray(pinVao);
    gl.bindBuffer(gl.ARRAY_BUFFER, buffer);
    for (let attribute = 0; attribute < 3; attribute++) {
      gl.enableVertexAttribArray(attribute);
      gl.vertexAttribPointer(attribute, 3, gl.FLOAT, false, 36, attribute * 12);
      gl.vertexAttribDivisor(attribute, 1);
    }
    const locations = new Map<WebGLProgram, Map<string, WebGLUniformLocation | null>>();
    const location = (p: WebGLProgram, name: string) => {
      if (!locations.has(p)) locations.set(p, new Map());
      const cache = locations.get(p)!;
      if (!cache.has(name)) cache.set(name, gl.getUniformLocation(p, name));
      return cache.get(name)!;
    };
    let count = 0;
    let current: WebGLGlobeOptions;
    return {
      setMap(o: WebGLGlobeOptions) {
        const image = document.createElement('canvas');
        image.width = 2048;
        image.height = 1024;
        const ctx = image.getContext('2d')!;
        const path = geoPath(
          geoEquirectangular()
            .translate([1024, 512])
            .scale(1024 / Math.PI),
          ctx,
        );
        if (o.showGrid) {
          ctx.beginPath();
          path(geoGraticule10());
          ctx.strokeStyle = o.gridColor;
          ctx.globalAlpha = 0.14;
          ctx.lineWidth = 1.2;
          ctx.stroke();
          ctx.globalAlpha = 1;
        }
        if (o.land) {
          ctx.beginPath();
          path(o.land);
          ctx.fillStyle = o.landColor;
          ctx.fill();
          ctx.strokeStyle = o.outlineColor;
          ctx.lineWidth = 0.7;
          ctx.stroke();
        }
        gl.bindTexture(gl.TEXTURE_2D, texture);
        gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, true);
        gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, image);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.REPEAT);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
      },
      setPins(o: WebGLGlobeOptions) {
        const singleColor = rgb(o.pinHeadColor);
        const colors = new Map<string, number[]>();
        const data = new Float32Array(o.pins.length * 9);
        o.pins.forEach((pin, index) => {
          let color = singleColor;
          if (o.variedHeads) {
            if (!colors.has(pin.color)) colors.set(pin.color, rgb(pin.color));
            color = colors.get(pin.color) as [number, number, number];
          }
          data.set([...pin.base, ...pin.head, ...color], index * 9);
        });
        gl.bindBuffer(gl.ARRAY_BUFFER, buffer);
        gl.bufferData(gl.ARRAY_BUFFER, data, gl.STATIC_DRAW);
        count = o.pins.length;
      },
      setStyle(o: WebGLGlobeOptions) {
        current = o;
        gl.useProgram(earth);
        gl.uniform3fv(location(earth, 'ocean'), rgb(o.oceanColor));
        gl.uniform3fv(location(earth, 'edge'), rgb(o.oceanEdgeColor));
        gl.uniform3fv(location(earth, 'outline'), rgb(o.outlineColor));
        gl.useProgram(pins);
        gl.uniform3fv(location(pins, 'shaft'), rgb(o.pinColor));
        gl.uniform1f(location(pins, 'radius'), o.pinHeadRadius);
        gl.uniform1f(location(pins, 'shaftWidth'), o.mini ? 1.2 : 1.5);
        gl.uniform1i(location(pins, 'roundHead'), o.pinStyle === 'round' ? 1 : 0);
      },
      draw(angle: number) {
        gl.viewport(0, 0, canvas.width, canvas.height);
        gl.clearColor(0, 0, 0, 0);
        gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);
        gl.enable(gl.DEPTH_TEST);
        gl.depthFunc(gl.LEQUAL);
        gl.enable(gl.BLEND);
        gl.blendFunc(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA);
        gl.useProgram(earth);
        gl.bindVertexArray(earthVao);
        gl.activeTexture(gl.TEXTURE0);
        gl.bindTexture(gl.TEXTURE_2D, texture);
        gl.uniform1i(location(earth, 'mapTexture'), 0);
        gl.uniform1f(location(earth, 'angle'), (angle * Math.PI) / 180);
        gl.drawArrays(gl.TRIANGLES, 0, 6);
        gl.useProgram(pins);
        gl.bindVertexArray(pinVao);
        gl.uniform1f(location(pins, 'angle'), (angle * Math.PI) / 180);
        gl.uniform1i(location(pins, 'kind'), 0);
        gl.drawArraysInstanced(gl.TRIANGLES, 0, 6, count);
        if (current.pinStyle !== 'stem') {
          gl.uniform1i(location(pins, 'kind'), 1);
          gl.drawArraysInstanced(gl.TRIANGLES, 0, 6, count);
        }
      },
      dispose,
    };
  } catch (error) {
    dispose();
    throw error;
  }
}
