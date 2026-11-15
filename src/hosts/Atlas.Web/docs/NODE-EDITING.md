# Editing an owned node

Sign in as the author and use the pencil on a feed card or the full node page. The editor reloads the current title and content. The API checks ownership and antiforgery on every write.

- **Edit** opens the authoring view. Sectioned content supports layout presets, text, URL-backed images and galleries, and link previews. Use the editor sidebar to change the selected section or block.
- **Preview in feed** shows the draft as a feed card. **Read More** opens the full draft preview. Narrow views stack section blocks in reading order and adapt gallery columns to the available width.
- **Publish changes** sends the title and content changes to the API. The full node renders its Content document, and discovery and search derive text from content blocks.

New nodes need at least 200 characters of body text to publish. The title, image captions, and alternative text do not count. You can save an incomplete node as a private draft.

## Saving and recovery

**Save private revision** stores the draft for this participant and node in the current browser. Use **Restore private revision** after reopening the editor, or **Delete private revision** to remove the browser copy. Private revisions leave the published node visible and are local to the device.

Publishing an edit can involve several independent calls. If a later call fails, earlier changes may already be public. The editor keeps the draft open. Reload the published node to confirm what saved before retrying. Search indexing runs asynchronously.

## Local development

Apply the checked-in sectioned-content and short-description removal migrations before running the API. To initialize and seed an empty local demo database, see the [local demo seed workflow](../../../../docs/workflows/local-demo-seed.md). Restart the web and Storybook dev servers after pulling changes to the session proxy.
