# Content block composition

Related requirements: [CON-003](../requirements/REQUIREMENTS.md#con-003), [CON-004](../requirements/REQUIREMENTS.md#con-004), [CON-005](../requirements/REQUIREMENTS.md#con-005), [CON-006](../requirements/REQUIREMENTS.md#con-006), [CON-007](../requirements/REQUIREMENTS.md#con-007)

## Ownership

Content owns documents, block identities, ordered composition, block payloads, and presentation metadata. Graph stores only DescriptionId. Media, polls, and charts remain external resources referenced by stable identifiers.

## Composition lifecycle

1. Create a concrete block from user-facing content; Content assigns its permanent, randomly generated GUID BlockId. The console never asks the user to supply an identity.
2. Save the block to blocks.json.
3. Add the BlockId to the document at the intended position and advance Document.UpdatedAt.
4. Save the document's ordered BlockIds to documents.json.
5. Edit the block through its type-specific behavior without changing BlockId.
6. Move a block by changing only its position in the document and advancing Document.UpdatedAt.
7. Remove a block reference and advance Document.UpdatedAt. Detached-block deletion or retention is deliberately deferred.

## Implemented block types

| Kind | Owned data | Validation |
|---|---|---|
| markdown | Markdown source | maximum length |
| image | URL, alt text, caption | URL and alt text required |
| video | URL, caption | URL required |
| link-preview | URL, title, description | absolute HTTP(S) URL and title required |
| poll-reference | poll ID | non-empty identifier |
| chart-reference | chart ID, title | non-empty identifier |

Markdown provides headings and inline formatting; there is no Header block. Specialized chart payloads remain deferred until their contracts are known.

## Persistence boundary

documents.json stores document identity, timestamps, and ordered BlockIds. blocks.json stores discriminated concrete block records. Each JSON record contains only its shared metadata and the fields belonging to its concrete block kind; unrelated and empty optional properties are omitted. This is a one-way migration from plain-text documents, not a compatibility layer.
