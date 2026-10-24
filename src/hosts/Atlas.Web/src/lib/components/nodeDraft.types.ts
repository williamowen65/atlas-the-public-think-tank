/** Editable node-creation values shared by the form and device-local draft storage. */

export interface NodeDraft {
  title: string;
  shortDescription: string;
  markdown: string;
  typeId: string | null;
  requestedTypeIds: string[];
  parentId?: string;
  newType?: { name: string; description: string; autoPluralize?: boolean };
}
