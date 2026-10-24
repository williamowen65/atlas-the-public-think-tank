/** Editable node-creation values shared by the form and device-local draft storage. */

export interface NodeDraft {
  /** Editable title of the node being created. */
  title: string;

  /** Short introduction displayed on the node card and above full content. */
  shortDescription: string;

  /** Markdown source for the node’s main content block. */
  markdown: string;

  /** Selected catalog type ID, or null when no existing type is selected. */
  typeId: string | null;

  /** Catalog type IDs the author requests as contributions to this node. */
  requestedTypeIds: string[];

  /** Parent node ID for a child draft; omitted for a root draft. */
  parentId?: string;

  /** Optional custom type to create before saving the node; used instead of an existing type ID. */
  newType?: {
    /** Name entered for the new custom node type. */
    name: string;
    /** Description entered for the new custom node type. */
    description: string;
    /** Whether the new type should use automatic pluralization; omitted uses the creation flow’s default. */
    autoPluralize?: boolean;
  };
}
