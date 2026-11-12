export const imageWidths = [
  { value: 'quarter', label: 'Quarter width', css: '25%' },
  { value: 'third', label: 'Third width', css: '33.333%' },
  { value: 'half', label: 'Half width', css: '50%' },
] as const;

export const imagePlacements = ['left', 'center', 'right'] as const;
export type ImageWidth = (typeof imageWidths)[number]['value'];
export type ImagePlacement = (typeof imagePlacements)[number];
export type ImageLayout = { width: ImageWidth; placement: ImagePlacement };

export const defaultImageLayout: ImageLayout = { width: 'half', placement: 'left' };

export function imageLayout(width: unknown, placement: unknown): ImageLayout {
  return {
    width: imageWidths.find((entry) => entry.value === width)?.value ?? 'half',
    placement: imagePlacements.find((entry) => entry === placement) ?? 'left',
  };
}

export function imageLayoutStyle(layout: ImageLayout): string {
  return '--atlas-image-width:' + imageWidths.find((entry) => entry.value === layout.width)!.css;
}

// Store Atlas layout as a reserved Markdown image-title prefix. A regular
// image title can follow it, and ordinary Markdown stays valid.
export function readImageTitle(value: string): ImageLayout & { title: string } {
  const match = /^atlas-image:(quarter|third|half):(left|center|right)(?: (.*))?$/s.exec(value);
  return match ? { ...imageLayout(match[1], match[2]), title: match[3] ?? '' }
    : { ...defaultImageLayout, title: value };
}

export function writeImageTitle(layout: ImageLayout, title: string): string {
  if (layout.width === 'half' && layout.placement === 'left') return title;
  return `atlas-image:${layout.width}:${layout.placement}${title ? ' ' + title : ''}`;
}
