/** Storybook-only proof of a carousel owning reusable typed child-block data.
 * This is intentionally NOT the Atlas persisted ContentBlock schema.
 * Child types are allowlisted while API, security, and nested ownership are researched.
 */
export interface CarouselChild {
  key: string;
  type: 'image' | 'video' | 'poll';
  url: string;
  alt: string;
  caption: string;
  poster?: string;
  question?: string;
  options?: string[];
}
export type CarouselFrame = 'portrait' | 'landscape';
