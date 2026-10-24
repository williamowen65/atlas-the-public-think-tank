# Atlas.Web component catalog

A source-based reference for every Svelte component in Atlas.Web: **29 reusable application components**, **14 Storybook wrappers**, and **15 route/layout files**. Start with the application components; Storybook controls belong to the demonstration wrappers unless the application contract also lists them.

## How to read the contracts

Props are the arguments passed through component attributes. `?` in TypeScript means optional. A listed default applies when omitted or passed as `undefined`; passing `null` is different. `undefined` in the default column means no explicit initializer is declared, not that the feature necessarily has no fallback. Required means required by the declared TypeScript contract, not that every string is validated here.

`on…` props are ordinary callback functions supplied by the parent. A `Snippet` is a renderable Svelte fragment, not text or an HTML string. `children` is the snippet built from nested component content. `Snippet<[FeedItem]>` receives one feed item when rendered. Some components perform API calls despite not having a `Live` prefix; their descriptions below identify those cases.

These contracts describe the current source, including limitations. Keep this catalog synchronized when changing `$props()` or shared interfaces. It is documentation, not a runtime validator or an automatically enforced API schema.

## Choosing a component

- **Session UI:** SessionControl; see the examples below for disclosure, embedded and registration use.
- **Caller-controlled voting UI:** VotingDial. **A real vote workflow:** LiveVotingDial.
- **A caller-supplied card/list/page:** NodeCard, NodeFeed, NodePage. **Backend-connected compositions:** LiveNodeFeed and LiveNodePage.
- **An editable form with parent-owned submission:** NodeForm. **Session-aware backend creation and private drafts:** LiveNodeForm.
- **A discussion:** CommentsPanel composes SessionControl and LiveComments; LiveComments owns recursive lists and mutations.

## VotingDial versus LiveVotingDial

These are two layers of the same control, not competing visual designs.

| Responsibility                                                  | VotingDial                     | LiveVotingDial                                                                     |
| --------------------------------------------------------------- | ------------------------------ | ---------------------------------------------------------------------------------- |
| Render the dial and accept wheel/drag/touch/keyboard selections | Yes                            | Uses VotingDial internally                                                         |
| Own the rating value and totals                                 | Receives props from its parent | Loads and tracks the confirmed API summary                                         |
| Report a proposed 0–10 selection                                | Calls onChange                 | Receives onChange from its inner VotingDial                                        |
| Confirm, save and undo a rating                                 | Parent must implement this     | Handles confirmation and session-aware API writes                                  |
| Calculate community averages/counts                             | Never                          | Never; refreshes authoritative backend values                                      |
| Sign-in handling                                                | None                           | A 401 write response opens the sign-in prompt; user chooses again after signing in |
| Disable interaction                                             | When its disabled prop is true | Passes disabled when archived, busy saving/refreshing, or loading initial status   |

A plain VotingDial is not necessarily display-only: it can emit interactive selections for a parent or isolated Storybook simulation. It becomes display-only when explicitly disabled. A LiveVotingDial still renders VotingDial; the wrapper supplies the API/session workflow around it.

### Why NodePage has a fallback

NodePage is a presentation component that can render without a live integration. If its optional voting snippet exists, it renders that snippet. LiveNodePage supplies a snippet containing LiveVotingDial, including the node ID and archive state. That replaces the fallback entirely; both controls are not shown together.

Without a snippet, NodePage renders a plain VotingDial with average and totalVotes plus the bare disabled attribute (equivalent to disabled={true}). It can show totals safely without offering a vote that has no persistence handler. This fallback is always disabled, even for an active node. This statement is specific to NodePage: other card presentations can supply onVote to their plain dial.

The snippet prop does not have to contain LiveVotingDial; a different parent can supply another renderer. The API behavior comes from what the parent puts inside the snippet, not from the name voting itself.

## Application component index

| Component                                             | Purpose                                                                                                                                            |
| ----------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------- |
| [ActionButton](#actionbutton)                         | Reusable action button with accessibility attributes and an activation callback.                                                                   |
| [AtlasFooter](#atlasfooter)                           | Shared grouped navigation links and site footer.                                                                                                   |
| [AtlasHeader](#atlasheader)                           | Shared navigation, search, account menu and logout behavior.                                                                                       |
| [Breadcrumb](#breadcrumb)                             | Scrollable ancestry ribbon with older-page loading and alternate-parent paths.                                                                     |
| [CommentsPanel](#commentspanel)                       | Composes session controls and the live threaded discussion for a node.                                                                             |
| [CommunityLinks](#communitylinks)                     | Reads node-community associations and displays links, with names as fallback.                                                                      |
| [ContentBlocks](#contentblocks)                       | Renders an ordered list of supported content blocks.                                                                                               |
| [DiscoverySearch](#discoverysearch)                   | Shared discovery search and filter toolbox; communicates through discoveryQuery state.                                                             |
| [LiveComments](#livecomments)                         | Fetches and edits one level of a threaded discussion, recursively rendering replies.                                                               |
| [LiveNodeFeed](#livenodefeed)                         | Loads discovery pages and composes NodeFeed with live voting/reaction controls.                                                                    |
| [LiveNodeForm](#livenodeform)                         | Session-aware creation, browser-private draft handling, custom types and backend submission.                                                       |
| [LiveNodePage](#livenodepage)                         | Fetches a node, its content and types, then composes voting, tabs, child forms and discussion.                                                     |
| [LiveProfilePage](#liveprofilepage)                   | Loads a public participant profile and their contribution feed.                                                                                    |
| [LiveReactions](#livereactions)                       | Loads reaction choices/scores and handles session-aware reaction voting and undo.                                                                  |
| [LiveVotingDial](#livevotingdial)                     | Connects VotingDial to API summaries, explicit confirmation, undo and sign-in prompts.                                                             |
| [MarkdownContent](#markdowncontent)                   | Renders Markdown through the shared markdown-it policy: formatting, lists, quotes, code and safe links; raw HTML and Markdown images are disabled. |
| [NodeCard](#nodecard)                                 | Selects a card presentation and forwards the common card contract.                                                                                 |
| [NodeCommunities](#nodecommunities)                   | Displays node-community associations and lets the node author change them.                                                                         |
| [NodeFeed](#nodefeed)                                 | Presentational card list with automatic/manual pagination and empty/error states.                                                                  |
| [NodeForm](#nodeform)                                 | Editable creation form and Markdown preview; parent owns persistence.                                                                              |
| [NodePage](#nodepage)                                 | Presentational full-node composition with discussion tabs and caller-supplied snippets.                                                            |
| [SessionControl](#sessioncontrol)                     | Reusable session check, login, registration and optional sign-out UI.                                                                              |
| [VotingDial](#votingdial)                             | Controlled, presentational importance dial with wheel, drag, touch and keyboard interactions.                                                      |
| [NodeCardContextBelow](#nodecardcontextbelow)         | Common card contract using the context-below presentation.                                                                                         |
| [NodeCardRibbon](#nodecardribbon)                     | Common card contract using the ribbon presentation.                                                                                                |
| [NodeCardRibbonCorner](#nodecardribboncorner)         | Common card contract using the ribbon-corner presentation.                                                                                         |
| [NodeCardRibbonResponsive](#nodecardribbonresponsive) | Common card contract using the responsive ribbon presentation.                                                                                     |
| [NodeCardRibbonSplit](#nodecardribbonsplit)           | Common card contract using the split ribbon presentation.                                                                                          |

## SessionControl examples

A collapsed sign-in prompt that reports session changes:

```svelte
<SessionControl prompt="Sign in to comment" onSession={(id) => (participantId = id)} />
```

An embedded sign-in form that disappears once signed in, as used around creation:

```svelte
<SessionControl
  expanded
  hideWhenSignedIn
  focusOnMount
  prompt="Sign in to create a node"
  onSession={(id) => (participantId = id)}
/>
```

Start with registration instead of login:

```svelte
<SessionControl expanded startRegister />
```

Omit `hideWhenSignedIn` to retain the signed-in status and Sign out button. `expanded` only controls disclosure visibility; it does not sign anyone in. `startRegister` seeds mode once. `onSession` runs for initial checks and subsequent changes, not only after clicking Sign in. A failed session check can also report null. Registration needs the backend email-confirmation workflow; a created account is not automatically a confirmed, authenticated session. SessionControl listens for the custom browser event `atlas-session-changed`; parents must not treat its UI state as backend authorization.

## Application component contracts

### ActionButton

Reusable action button with accessibility attributes and an activation callback. [Source](../src/lib/components/ActionButton.svelte).

| Prop         | Type         | Required | Default when omitted | Purpose                                                                 |
| ------------ | ------------ | -------- | -------------------- | ----------------------------------------------------------------------- |
| `label`      | `string`     | Yes      | —                    | Accessible control name; also visible button text for ActionButton.     |
| `disabled`   | `boolean`    | No       | `false`              | Prevents interaction with this control.                                 |
| `expanded`   | `boolean`    | No       | `undefined`          | Sets aria-expanded to describe whether the controlled region is open.   |
| `controls`   | `string`     | No       | `undefined`          | HTML ID of the region controlled by the button; supplies aria-controls. |
| `onActivate` | `() => void` | No       | `undefined`          | Called when the enabled button is activated by pointer or keyboard.     |

### AtlasFooter

Shared grouped navigation links and site footer. [Source](../src/lib/components/AtlasFooter.svelte).

| Prop      | Type     | Required | Default when omitted                                   | Purpose                        |
| --------- | -------- | -------- | ------------------------------------------------------ | ------------------------------ |
| `tagline` | `string` | No       | `'A place to connect questions, evidence, and ideas.'` | Supporting text in the footer. |

### AtlasHeader

Shared navigation, search, account menu and logout behavior. [Source](../src/lib/components/AtlasHeader.svelte).

| Prop            | Type                                                                       | Required | Default when omitted | Purpose                                                                                         |
| --------------- | -------------------------------------------------------------------------- | -------- | -------------------- | ----------------------------------------------------------------------------------------------- |
| `active`        | `'discover' \| 'communities' \| 'create' \| 'login' \| 'register' \| null` | No       | `null`               | Highlighted navigation item; null leaves all items unselected.                                  |
| `authenticated` | `boolean`                                                                  | No       | `false`              | Selects the signed-in header menu versus login/register links; does not authenticate by itself. |
| `displayName`   | `string`                                                                   | No       | `'Atlas User'`       | Name used to derive the account-avatar initial.                                                 |

### Breadcrumb

Scrollable ancestry ribbon with older-page loading and alternate-parent paths. [Source](../src/lib/components/Breadcrumb.svelte).

| Prop         | Type                   | Required | Default when omitted  | Purpose                                                                                      |
| ------------ | ---------------------- | -------- | --------------------- | -------------------------------------------------------------------------------------------- |
| `items`      | `BreadcrumbItem[]`     | Yes      | —                     | Ancestor labels in oldest-to-newest order; IDs enable navigation when a handler is supplied. |
| `onNavigate` | `(id: string) => void` | No       | `undefined`           | Receives the selected node ID when an ancestor is activated. The parent owns navigation.     |
| `hasMore`    | `boolean`              | No       | `false`               | Whether older ancestors remain available to load.                                            |
| `loadOlder`  | `LoadOlderAncestors`   | No       | `undefined`           | Async ancestor loader; receives the oldest displayed ID and returns a BreadcrumbPage.        |
| `originId`   | `string`               | No       | `undefined`           | Current node ID used to look up alternative parents for the final breadcrumb segment.        |
| `apiBase`    | `string`               | No       | `'/atlas-public-api'` | Base URL for this component’s API reads. This prop alone does not configure a proxy.         |

### CommentsPanel

Composes session controls and the live threaded discussion for a node. [Source](../src/lib/components/CommentsPanel.svelte).

| Prop          | Type         | Required | Default when omitted   | Purpose                                                                                                     |
| ------------- | ------------ | -------- | ---------------------- | ----------------------------------------------------------------------------------------------------------- |
| `nodeId`      | `string`     | Yes      | —                      | ID of the node this component reads, displays, or modifies.                                                 |
| `apiBase`     | `string`     | No       | `'/atlas-public-api'`  | Base URL for this component’s API reads. This prop alone does not configure a proxy.                        |
| `sessionBase` | `string`     | No       | `'/atlas-session-api'` | Session-aware API base for authentication and permitted operations, including CSRF-protected writes.        |
| `onAdded`     | `() => void` | No       | `undefined`            | Called after a comment or reply is successfully added; lets the parent refresh its displayed comment count. |

### CommunityLinks

Reads node-community associations and displays links, with names as fallback. [Source](../src/lib/components/CommunityLinks.svelte).

| Prop     | Type       | Required | Default when omitted | Purpose                                                                            |
| -------- | ---------- | -------- | -------------------- | ---------------------------------------------------------------------------------- |
| `nodeId` | `string`   | Yes      | —                    | ID of the node this component reads, displays, or modifies.                        |
| `names`  | `string[]` | Yes      | —                    | Fallback community display names while linked community records are being fetched. |

### ContentBlocks

Renders an ordered list of supported content blocks. [Source](../src/lib/components/ContentBlocks.svelte).

| Prop     | Type             | Required | Default when omitted | Purpose                                                   |
| -------- | ---------------- | -------- | -------------------- | --------------------------------------------------------- |
| `blocks` | `ContentBlock[]` | Yes      | —                    | Ordered content blocks to render; see ContentBlock below. |

### DiscoverySearch

Shared discovery search and filter toolbox; communicates through discoveryQuery state. [Source](../src/lib/components/DiscoverySearch.svelte).

| Prop               | Type                          | Required | Default when omitted | Purpose                                                                                     |
| ------------------ | ----------------------------- | -------- | -------------------- | ------------------------------------------------------------------------------------------- |
| `onExpandedChange` | `(expanded: boolean) => void` | No       | `undefined`          | Reports whether the search filter toolbox is expanded, so the header can adjust its layout. |

### LiveComments

Fetches and edits one level of a threaded discussion, recursively rendering replies. [Source](../src/lib/components/LiveComments.svelte).

| Prop            | Type             | Required | Default when omitted   | Purpose                                                                                                                               |
| --------------- | ---------------- | -------- | ---------------------- | ------------------------------------------------------------------------------------------------------------------------------------- |
| `nodeId`        | `string`         | Yes      | —                      | ID of the node this component reads, displays, or modifies.                                                                           |
| `parentId`      | `string`         | No       | `undefined`            | Comment ID whose direct replies this list loads; omit to load root comments for the node.                                             |
| `apiBase`       | `string`         | No       | `'/atlas-public-api'`  | Base URL for this component’s API reads. This prop alone does not configure a proxy.                                                  |
| `sessionBase`   | `string`         | No       | `'/atlas-session-api'` | Session-aware API base for authentication and permitted operations, including CSRF-protected writes.                                  |
| `participantId` | `string \| null` | No       | `null`                 | Current signed-in participant ID, supplied by the parent. Enables composing and author controls; the backend still authorizes writes. |
| `showComposer`  | `boolean`        | No       | `true`                 | Allows the comment/reply editor to appear when a participant is signed in.                                                            |
| `depth`         | `number`         | No       | `0`                    | Reply nesting depth used to limit increasing indentation.                                                                             |
| `onAdded`       | `() => void`     | No       | `undefined`            | Called after a comment or reply is successfully added; lets the parent refresh its displayed comment count.                           |

### LiveNodeFeed

Loads discovery pages and composes NodeFeed with live voting/reaction controls. [Source](../src/lib/components/LiveNodeFeed.svelte).

| Prop            | Type                                    | Required | Default when omitted  | Purpose                                                                                                                                              |
| --------------- | --------------------------------------- | -------- | --------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------- |
| `presentation`  | `NodeCardPresentation`                  | No       | `'ribbon-corner'`     | Selects a NodeCardPresentation layout; it does not change the underlying node data.                                                                  |
| `apiBase`       | `string`                                | No       | `'/atlas-public-api'` | Base URL for this component’s API reads. This prop alone does not configure a proxy.                                                                 |
| `parentId`      | `string`                                | No       | `undefined`           | Restrict discovery to children of this parent node. When omitted, filters.parentId can still supply the scope; otherwise the parent filter is unset. |
| `typeId`        | `string`                                | No       | `undefined`           | Node type ID used to restrict the feed.                                                                                                              |
| `authorId`      | `string`                                | No       | `undefined`           | Participant ID used to restrict the feed to that author.                                                                                             |
| `search`        | `string`                                | No       | `''`                  | Free-text discovery search, separate from structured filters.                                                                                        |
| `filters`       | `DiscoveryFilters`                      | No       | `{}`                  | Additional DiscoveryFilters for the feed request; see the data-shape reference.                                                                      |
| `emptyTypeName` | `string`                                | No       | `undefined`           | Singular contribution type name used in the empty-feed invitation.                                                                                   |
| `onCreateFirst` | `() => void`                            | No       | `undefined`           | Called by the empty-feed invitation to open creation for that contribution type.                                                                     |
| `onOpenNode`    | `(id: string, typeId?: string) => void` | No       | `undefined`           | Receives a node ID to open. Where the signature includes typeId, it selects the destination contribution tab.                                        |
| `onOpenProfile` | `(id: string) => void`                  | No       | `undefined`           | Receives the author/participant ID to open in a profile page.                                                                                        |

Explicit parentId/typeId/authorId override the corresponding fields in filters. The feed loads on mount; callers key/remount it when filters change. The root page demonstrates that pattern. Without onOpenNode, it navigates to the app’s node URL.

### LiveNodeForm

Session-aware creation, browser-private draft handling, custom types and backend submission. [Source](../src/lib/components/LiveNodeForm.svelte).

| Prop              | Type                                      | Required | Default when omitted   | Purpose                                                                                                                                            |
| ----------------- | ----------------------------------------- | -------- | ---------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------- |
| `draftId`         | `string`                                  | No       | `undefined`            | ID of an existing browser-private draft to restore for the signed-in participant.                                                                  |
| `parentId`        | `string`                                  | No       | `undefined`            | Parent node ID for child-node creation; omit for root creation.                                                                                    |
| `parentTitle`     | `string`                                  | No       | `undefined`            | Display title for the parent breadcrumb; parentId is the actual association.                                                                       |
| `initialTypeId`   | `string`                                  | No       | `undefined`            | Initial node type selection when creating a draft.                                                                                                 |
| `breadcrumb`      | `BreadcrumbItem[]`                        | No       | `[]`                   | Parent ancestry used above a creation form, before the immediate parent is appended.                                                               |
| `focusOnMount`    | `boolean`                                 | No       | `false`                | Requests initial focus/scroll placement when the relevant form is rendered in the browser.                                                         |
| `apiBase`         | `string`                                  | No       | `'/atlas-public-api'`  | Base URL for this component’s API reads. This prop alone does not configure a proxy.                                                               |
| `sessionBase`     | `string`                                  | No       | `'/atlas-session-api'` | Session-aware API base for authentication and permitted operations, including CSRF-protected writes.                                               |
| `onCreated`       | `(id: string) => void`                    | No       | `undefined`            | Receives the ID returned by the backend after successful node creation.                                                                            |
| `onDraftChange`   | `(draft: NodeDraft \| null) => void`      | No       | `undefined`            | Receives the current draft or null according to the callback contract; lets the parent track child-form state.                                     |
| `onRegisterClose` | `(close: () => Promise<boolean>) => void` | No       | `undefined`            | Registers an async draft-save function with the parent. Calling that function returns whether saving succeeded; it does not itself close the form. |

### LiveNodePage

Fetches a node, its content and types, then composes voting, tabs, child forms and discussion. [Source](../src/lib/components/LiveNodePage.svelte).

| Prop            | Type                                    | Required | Default when omitted  | Purpose                                                                                                       |
| --------------- | --------------------------------------- | -------- | --------------------- | ------------------------------------------------------------------------------------------------------------- |
| `nodeId`        | `string`                                | No       | `''`                  | ID of the node this component reads, displays, or modifies.                                                   |
| `initialTypeId` | `string`                                | No       | `''`                  | Initial discussion tab ID (a contribution type ID or comments), not a node-creation type.                     |
| `apiBase`       | `string`                                | No       | `'/atlas-public-api'` | Base URL for this component’s API reads. This prop alone does not configure a proxy.                          |
| `onOpenNode`    | `(id: string, typeId?: string) => void` | No       | `undefined`           | Receives a node ID to open. Where the signature includes typeId, it selects the destination contribution tab. |
| `onOpenProfile` | `(id: string) => void`                  | No       | `undefined`           | Receives the author/participant ID to open in a profile page.                                                 |

An empty nodeId is supported by the declaration and leaves discovery to select a displayable result; supply an ID for deterministic navigation. Comments and voting use their own session-aware defaults.

### LiveProfilePage

Loads a public participant profile and their contribution feed. [Source](../src/lib/components/LiveProfilePage.svelte).

| Prop            | Type                   | Required | Default when omitted  | Purpose                                                                                                       |
| --------------- | ---------------------- | -------- | --------------------- | ------------------------------------------------------------------------------------------------------------- |
| `participantId` | `string`               | No       | `''`                  | Participant whose public profile and authored contributions are displayed.                                    |
| `apiBase`       | `string`               | No       | `'/atlas-public-api'` | Base URL for this component’s API reads. This prop alone does not configure a proxy.                          |
| `onOpenNode`    | `(id: string) => void` | No       | `undefined`           | Receives a node ID to open. Where the signature includes typeId, it selects the destination contribution tab. |
| `onOpenProfile` | `(id: string) => void` | No       | `undefined`           | Receives the author/participant ID to open in a profile page.                                                 |

### LiveReactions

Loads reaction choices/scores and handles session-aware reaction voting and undo. [Source](../src/lib/components/LiveReactions.svelte).

| Prop       | Type      | Required | Default when omitted   | Purpose                                                                                                |
| ---------- | --------- | -------- | ---------------------- | ------------------------------------------------------------------------------------------------------ |
| `nodeId`   | `string`  | Yes      | —                      | ID of the node this component reads, displays, or modifies.                                            |
| `archived` | `boolean` | No       | `false`                | Marks the node as archived and disables applicable voting/editing controls.                            |
| `apiBase`  | `string`  | No       | `'/atlas-session-api'` | Session-aware endpoint base for both reaction reads and writes, despite the generic apiBase prop name. |

### LiveVotingDial

Connects VotingDial to API summaries, explicit confirmation, undo and sign-in prompts. [Source](../src/lib/components/LiveVotingDial.svelte).

| Prop                | Type             | Required | Default when omitted   | Purpose                                                                                              |
| ------------------- | ---------------- | -------- | ---------------------- | ---------------------------------------------------------------------------------------------------- |
| `nodeId`            | `string`         | Yes      | —                      | ID of the node this component reads, displays, or modifies.                                          |
| `initialAverage`    | `number \| null` | No       | `null`                 | Initial average shown while the live voting summary is loading; the API summary is authoritative.    |
| `initialTotalVotes` | `number`         | No       | `0`                    | Initial count shown while the live voting summary is loading; refreshed from the API.                |
| `archived`          | `boolean`        | No       | `false`                | Marks the node as archived and disables applicable voting/editing controls.                          |
| `sessionBase`       | `string`         | No       | `'/atlas-session-api'` | Session-aware API base for authentication and permitted operations, including CSRF-protected writes. |

### MarkdownContent

Renders Markdown through the shared markdown-it policy: formatting, lists, quotes, code and safe links; raw HTML and Markdown images are disabled. [Source](../src/lib/components/MarkdownContent.svelte).

| Prop   | Type     | Required | Default when omitted | Purpose                                                                                    |
| ------ | -------- | -------- | -------------------- | ------------------------------------------------------------------------------------------ |
| `text` | `string` | Yes      | —                    | Markdown block source rendered by the shared policy; short descriptions remain plain text. |

### NodeCard

Selects a card presentation and forwards the common card contract. [Source](../src/lib/components/NodeCard.svelte).

| Prop                 | Type                                   | Required | Default when omitted | Purpose                                                                                          |
| -------------------- | -------------------------------------- | -------- | -------------------- | ------------------------------------------------------------------------------------------------ |
| `node`               | `NodeCardData`                         | Yes      | —                    | Presentation data for one node; see NodeCardData below.                                          |
| `voting`             | `Snippet`                              | No       | `undefined`          | Optional Svelte snippet that replaces the built-in voting dial presentation.                     |
| `reactionControls`   | `Snippet`                              | No       | `undefined`          | Optional Svelte snippet that replaces the static reaction summary with custom/live controls.     |
| `hasMoreAncestors`   | `boolean`                              | No       | `undefined`          | Whether additional older ancestors can be loaded for the node’s breadcrumb.                      |
| `loadOlderAncestors` | `LoadOlderAncestors`                   | No       | `undefined`          | Async ancestor loader forwarded to Breadcrumb; see LoadOlderAncestors below.                     |
| `apiBase`            | `string`                               | No       | `undefined`          | Base URL for this component’s API reads. This prop alone does not configure a proxy.             |
| `value`              | `number \| null`                       | No       | `undefined`          | Current controlled rating, 0–10. null means unrated; zero is a real rating.                      |
| `average`            | `number \| null`                       | No       | `undefined`          | Community average supplied by the parent; null means no average is available.                    |
| `totalVotes`         | `number`                               | No       | `undefined`          | Community vote count supplied by the parent.                                                     |
| `onVote`             | `(value: number) => void`              | No       | `undefined`          | Reports a selected rating to the parent. The presentation component does not save it to the API. |
| `onNavigate`         | `(id: string) => void`                 | No       | `undefined`          | Receives the selected node ID when an ancestor is activated. The parent owns navigation.         |
| `onOpenProfile`      | `(id: string) => void`                 | No       | `undefined`          | Receives the author/participant ID to open in a profile page.                                    |
| `onReadMore`         | `(id: string) => void`                 | No       | `undefined`          | Receives the node ID when Read more is activated; absence disables that action.                  |
| `onChildType`        | `(id: string, typeId: string) => void` | No       | `undefined`          | Receives node ID and contribution type ID when a contribution-summary button is activated.       |
| `onComments`         | `(id: string) => void`                 | No       | `undefined`          | Receives the node ID when its comment-count button is activated.                                 |
| `presentation`       | `NodeCardPresentation`                 | No       | `'ribbon'`           | Selects a NodeCardPresentation layout; it does not change the underlying node data.              |

The wrapper initializes only `presentation`; all other props are forwarded. The selected presentation supplies defaults such as `value = null`, `average = null`, and `totalVotes = 0`. Presentation-specific defaults are listed below. Card presentations still use CommunityLinks for community lookup; use supplied callbacks/snippets to integrate navigation and live controls.

### NodeCommunities

Displays node-community associations and lets the node author change them. [Source](../src/lib/components/NodeCommunities.svelte).

| Prop       | Type      | Required | Default when omitted | Purpose                                                                                                                         |
| ---------- | --------- | -------- | -------------------- | ------------------------------------------------------------------------------------------------------------------------------- |
| `nodeId`   | `string`  | Yes      | —                    | ID of the node this component reads, displays, or modifies.                                                                     |
| `authorId` | `string`  | No       | `undefined`          | Node author ID used with the current session to show association-editing controls; backend authorization remains authoritative. |
| `archived` | `boolean` | No       | `false`              | Marks the node as archived and disables applicable voting/editing controls.                                                     |

### NodeFeed

Presentational card list with automatic/manual pagination and empty/error states. [Source](../src/lib/components/NodeFeed.svelte).

| Prop                 | Type                                   | Required | Default when omitted | Purpose                                                                                    |
| -------------------- | -------------------------------------- | -------- | -------------------- | ------------------------------------------------------------------------------------------ |
| `renderVoting`       | `Snippet<[FeedItem]>`                  | No       | `undefined`          | Svelte snippet receiving a FeedItem, used to supply voting controls for each card.         |
| `renderReactions`    | `Snippet<[FeedItem]>`                  | No       | `undefined`          | Svelte snippet receiving a FeedItem, used to supply reaction controls for each card.       |
| `items`              | `FeedItem[]`                           | Yes      | —                    | Loaded FeedItem records to display; the parent owns fetching and accumulation.             |
| `loading`            | `boolean`                              | No       | `false`              | Whether a page request is in progress; shows loading feedback and restricts paging.        |
| `hasMore`            | `boolean`                              | No       | `false`              | Whether more feed pages remain. Enables automatic loading and the Load more button.        |
| `error`              | `string`                               | No       | `''`                 | Error text for feed paging; a nonempty value exposes retry feedback.                       |
| `presentation`       | `NodeCardPresentation`                 | No       | `'ribbon-corner'`    | Selects a NodeCardPresentation layout; it does not change the underlying node data.        |
| `onLoadMore`         | `() => void`                           | No       | `undefined`          | Requests the next page or retries the failed page; parent owns fetching and page state.    |
| `onNavigate`         | `(id: string) => void`                 | No       | `undefined`          | Receives the selected node ID when an ancestor is activated. The parent owns navigation.   |
| `onOpenProfile`      | `(id: string) => void`                 | No       | `undefined`          | Receives the author/participant ID to open in a profile page.                              |
| `onReadMore`         | `(id: string) => void`                 | No       | `undefined`          | Receives the node ID when Read more is activated; absence disables that action.            |
| `loadOlderAncestors` | `LoadOlderAncestors`                   | No       | `undefined`          | Async ancestor loader forwarded to Breadcrumb; see LoadOlderAncestors below.               |
| `apiBase`            | `string`                               | No       | `undefined`          | Forwarded to card breadcrumb readers; NodeFeed itself does not fetch discovery pages.      |
| `onChildType`        | `(id: string, typeId: string) => void` | No       | `undefined`          | Receives node ID and contribution type ID when a contribution-summary button is activated. |
| `onComments`         | `(id: string) => void`                 | No       | `undefined`          | Receives the node ID when its comment-count button is activated.                           |
| `emptyTypeName`      | `string`                               | No       | `undefined`          | Singular contribution type name used in the empty-feed invitation.                         |
| `onCreateFirst`      | `() => void`                           | No       | `undefined`          | Called by the empty-feed invitation to open creation for that contribution type.           |

### NodeForm

Editable creation form and Markdown preview; parent owns persistence. [Source](../src/lib/components/NodeForm.svelte).

| Prop            | Type                         | Required | Default when omitted | Purpose                                                                                                                  |
| --------------- | ---------------------------- | -------- | -------------------- | ------------------------------------------------------------------------------------------------------------------------ |
| `types`         | `NodeType[]`                 | Yes      | —                    | Available node types and their singular/plural display labels.                                                           |
| `initialDraft`  | `NodeDraft`                  | No       | `undefined`          | Seed values for editable fields. Captured once when the form is created; later prop changes do not overwrite user edits. |
| `parentId`      | `string`                     | No       | `undefined`          | Parent node ID for child-node creation; omit for root creation.                                                          |
| `parentTitle`   | `string`                     | No       | `undefined`          | Display title for the parent breadcrumb; parentId is the actual association.                                             |
| `initialTypeId` | `string`                     | No       | `''`                 | Initial node type selection when creating a draft.                                                                       |
| `breadcrumb`    | `BreadcrumbItem[]`           | No       | `[]`                 | Parent ancestry used above a creation form, before the immediate parent is appended.                                     |
| `focusOnMount`  | `boolean`                    | No       | `false`              | Requests initial focus/scroll placement when the relevant form is rendered in the browser.                               |
| `onPreview`     | `(draft: NodeDraft) => void` | No       | `undefined`          | Receives the current editable NodeDraft as its fields change; does not save to the API.                                  |
| `onSubmit`      | `(draft: NodeDraft) => void` | Yes      | —                    | Receives the draft when the form submits; the parent owns persistence.                                                   |
| `submitting`    | `boolean`                    | No       | `false`              | Shows the creation-in-progress label and disables the submit button.                                                     |

### NodePage

Presentational full-node composition with discussion tabs and caller-supplied snippets. [Source](../src/lib/components/NodePage.svelte).

| Prop                     | Type                           | Required | Default when omitted | Purpose                                                                                                                                                                                    |
| ------------------------ | ------------------------------ | -------- | -------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `voting`                 | `Snippet`                      | No       | `undefined`          | Optional Svelte snippet that replaces the built-in voting dial presentation.                                                                                                               |
| `reactions`              | `Snippet`                      | No       | `undefined`          | Optional Svelte snippet for the live/custom reactions region.                                                                                                                              |
| `scrollToDiscussionTabs` | `boolean`                      | No       | `false`              | Initially focuses the selected discussion tab and scrolls toward a 20px top offset within natural page limits. Adds no extra panel height. Alignment correction stops on user interaction. |
| `node`                   | `NodeCardData`                 | Yes      | —                    | Presentation data for one node; see NodeCardData below.                                                                                                                                    |
| `blocks`                 | `ContentBlock[]`               | Yes      | —                    | Ordered content blocks to render; see ContentBlock below.                                                                                                                                  |
| `types`                  | `NodeType[]`                   | Yes      | —                    | Available node types and their singular/plural display labels.                                                                                                                             |
| `active`                 | `string`                       | Yes      | —                    | Selected discussion-tab ID for NodePage; header components instead use a navigation destination.                                                                                           |
| `draftTab`               | `{ id: string; name: string }` | No       | `undefined`          | Temporary contribution-tab ID and label for an in-progress child draft.                                                                                                                    |
| `onTabChange`            | `(id: string) => void`         | Yes      | —                    | Receives the requested discussion-tab ID; the parent controls active.                                                                                                                      |
| `onAddChild`             | `(id?: string) => void`        | Yes      | —                    | Requests a child form for the supplied type ID, or lets the parent choose when omitted.                                                                                                    |
| `onNavigate`             | `(id: string) => void`         | No       | `undefined`          | Receives the selected node ID when an ancestor is activated. The parent owns navigation.                                                                                                   |
| `onOpenProfile`          | `(id: string) => void`         | No       | `undefined`          | Receives the author/participant ID to open in a profile page.                                                                                                                              |
| `hasMoreAncestors`       | `boolean`                      | No       | `false`              | Whether additional older ancestors can be loaded for the node’s breadcrumb.                                                                                                                |
| `loadOlderAncestors`     | `LoadOlderAncestors`           | No       | `undefined`          | Async ancestor loader forwarded to Breadcrumb; see LoadOlderAncestors below.                                                                                                               |
| `apiBase`                | `string`                       | No       | `undefined`          | Forwarded to Breadcrumb for alternate-parent lookups; the parent supplies node/content data.                                                                                               |
| `average`                | `number \| null`               | No       | `null`               | Community average supplied by the parent; null means no average is available.                                                                                                              |
| `totalVotes`             | `number`                       | No       | `0`                  | Community vote count supplied by the parent.                                                                                                                                               |
| `children`               | `Snippet`                      | Yes      | —                    | Svelte snippet containing the current tab’s content, such as comments, child forms and feeds.                                                                                              |

Without a voting snippet, its built-in dial is disabled. Without a reactions snippet, node.reactions supplies the static summary. The parent owns tab state and supplies tab content through children.

### SessionControl

Reusable session check, login, registration and optional sign-out UI. [Source](../src/lib/components/SessionControl.svelte).

| Prop               | Type                           | Required | Default when omitted   | Purpose                                                                                                                                        |
| ------------------ | ------------------------------ | -------- | ---------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------- |
| `sessionBase`      | `string`                       | No       | `'/atlas-session-api'` | Session-aware API base for authentication and permitted operations, including CSRF-protected writes.                                           |
| `prompt`           | `string`                       | No       | `'Sign in to Atlas'`   | Text of the signed-out sign-in disclosure summary.                                                                                             |
| `expanded`         | `boolean`                      | No       | `false`                | Opens the signed-out disclosure so the login/register form is visible.                                                                         |
| `startRegister`    | `boolean`                      | No       | `false`                | Initial registration versus login mode. Captured once; the visitor can switch modes afterward.                                                 |
| `focusOnMount`     | `boolean`                      | No       | `false`                | Requests initial focus/scroll placement when the relevant form is rendered in the browser.                                                     |
| `hideWhenSignedIn` | `boolean`                      | No       | `false`                | Hides this entire session helper once signed in, including its Sign out button.                                                                |
| `onSession`        | `(id: string \| null) => void` | No       | `undefined`            | Reports the participant ID after session checks or sign-in; null indicates no usable session, including failed checks. May run more than once. |

### VotingDial

Controlled, presentational importance dial with wheel, drag, touch and keyboard interactions. [Source](../src/lib/components/VotingDial.svelte).

| Prop         | Type                      | Required | Default when omitted | Purpose                                                                                                          |
| ------------ | ------------------------- | -------- | -------------------- | ---------------------------------------------------------------------------------------------------------------- |
| `label`      | `string`                  | No       | `'Node importance'`  | Accessible control name; also visible button text for ActionButton.                                              |
| `value`      | `number \| null`          | No       | `null`               | Current controlled rating, 0–10. null means unrated; zero is a real rating.                                      |
| `average`    | `number \| null`          | No       | `null`               | Community average supplied by the parent; null means no average is available.                                    |
| `totalVotes` | `number`                  | No       | `0`                  | Community vote count supplied by the parent.                                                                     |
| `hasVoted`   | `boolean`                 | No       | `undefined`          | Controls voted styling; when omitted, defaults to whether value is non-null.                                     |
| `compact`    | `boolean`                 | No       | `false`              | Selects the compact dial layout.                                                                                 |
| `disabled`   | `boolean`                 | No       | `false`              | Prevents interaction with this control.                                                                          |
| `onChange`   | `(value: number) => void` | No       | `undefined`          | Receives the selected 0–10 rating. This control is presentational; the caller handles acceptance or persistence. |

### NodeCardContextBelow

Common card contract using the context-below presentation. [Source](../src/lib/components/node-card-versions/NodeCardContextBelow.svelte).

| Prop                 | Type                                   | Required | Default when omitted | Purpose                                                                                            |
| -------------------- | -------------------------------------- | -------- | -------------------- | -------------------------------------------------------------------------------------------------- |
| `node`               | `NodeCardData`                         | Yes      | —                    | Presentation data for one node; see NodeCardData below.                                            |
| `voting`             | `Snippet`                              | No       | `undefined`          | Optional Svelte snippet that replaces the built-in voting dial presentation.                       |
| `reactionControls`   | `Snippet`                              | No       | `undefined`          | Optional Svelte snippet that replaces the static reaction summary with custom/live controls.       |
| `hasMoreAncestors`   | `boolean`                              | No       | `undefined`          | Accepted by the shared card type but unused in this presentation; its ancestry is plain text.      |
| `loadOlderAncestors` | `LoadOlderAncestors`                   | No       | `undefined`          | Accepted by the shared card type but unused in this presentation; it has no ancestor pager.        |
| `apiBase`            | `string`                               | No       | `undefined`          | Accepted by the shared card type but not used by this presentation.                                |
| `value`              | `number \| null`                       | No       | `null`               | Current controlled rating, 0–10. null means unrated; zero is a real rating.                        |
| `average`            | `number \| null`                       | No       | `null`               | Community average supplied by the parent; null means no average is available.                      |
| `totalVotes`         | `number`                               | No       | `0`                  | Community vote count supplied by the parent.                                                       |
| `onVote`             | `(value: number) => void`              | No       | `undefined`          | Reports a selected rating to the parent. The presentation component does not save it to the API.   |
| `onNavigate`         | `(id: string) => void`                 | No       | `undefined`          | Receives a related node ID when its related-node link is activated; ancestry is displayed as text. |
| `onOpenProfile`      | `(id: string) => void`                 | No       | `undefined`          | Receives the author/participant ID to open in a profile page.                                      |
| `onReadMore`         | `(id: string) => void`                 | No       | `undefined`          | Receives the node ID when Read more is activated; absence disables that action.                    |
| `onChildType`        | `(id: string, typeId: string) => void` | No       | `undefined`          | Receives node ID and contribution type ID when a contribution-summary button is activated.         |
| `onComments`         | `(id: string) => void`                 | No       | `undefined`          | Receives the node ID when its comment-count button is activated.                                   |

### NodeCardRibbon

Common card contract using the ribbon presentation. [Source](../src/lib/components/node-card-versions/NodeCardRibbon.svelte).

| Prop                 | Type                                   | Required | Default when omitted | Purpose                                                                                          |
| -------------------- | -------------------------------------- | -------- | -------------------- | ------------------------------------------------------------------------------------------------ |
| `node`               | `NodeCardData`                         | Yes      | —                    | Presentation data for one node; see NodeCardData below.                                          |
| `voting`             | `Snippet`                              | No       | `undefined`          | Optional Svelte snippet that replaces the built-in voting dial presentation.                     |
| `reactionControls`   | `Snippet`                              | No       | `undefined`          | Optional Svelte snippet that replaces the static reaction summary with custom/live controls.     |
| `hasMoreAncestors`   | `boolean`                              | No       | `false`              | Whether additional older ancestors can be loaded for the node’s breadcrumb.                      |
| `loadOlderAncestors` | `LoadOlderAncestors`                   | No       | `undefined`          | Async ancestor loader forwarded to Breadcrumb; see LoadOlderAncestors below.                     |
| `apiBase`            | `string`                               | No       | `undefined`          | Base URL for this component’s API reads. This prop alone does not configure a proxy.             |
| `value`              | `number \| null`                       | No       | `null`               | Current controlled rating, 0–10. null means unrated; zero is a real rating.                      |
| `average`            | `number \| null`                       | No       | `null`               | Community average supplied by the parent; null means no average is available.                    |
| `totalVotes`         | `number`                               | No       | `0`                  | Community vote count supplied by the parent.                                                     |
| `onVote`             | `(value: number) => void`              | No       | `undefined`          | Reports a selected rating to the parent. The presentation component does not save it to the API. |
| `onNavigate`         | `(id: string) => void`                 | No       | `undefined`          | Receives the selected node ID when an ancestor is activated. The parent owns navigation.         |
| `onOpenProfile`      | `(id: string) => void`                 | No       | `undefined`          | Receives the author/participant ID to open in a profile page.                                    |
| `onReadMore`         | `(id: string) => void`                 | No       | `undefined`          | Receives the node ID when Read more is activated; absence disables that action.                  |
| `onChildType`        | `(id: string, typeId: string) => void` | No       | `undefined`          | Receives node ID and contribution type ID when a contribution-summary button is activated.       |
| `onComments`         | `(id: string) => void`                 | No       | `undefined`          | Receives the node ID when its comment-count button is activated.                                 |

### NodeCardRibbonCorner

Common card contract using the ribbon-corner presentation. [Source](../src/lib/components/node-card-versions/NodeCardRibbonCorner.svelte).

| Prop                 | Type                                   | Required | Default when omitted | Purpose                                                                                          |
| -------------------- | -------------------------------------- | -------- | -------------------- | ------------------------------------------------------------------------------------------------ |
| `node`               | `NodeCardData`                         | Yes      | —                    | Presentation data for one node; see NodeCardData below.                                          |
| `voting`             | `Snippet`                              | No       | `undefined`          | Optional Svelte snippet that replaces the built-in voting dial presentation.                     |
| `reactionControls`   | `Snippet`                              | No       | `undefined`          | Optional Svelte snippet that replaces the static reaction summary with custom/live controls.     |
| `hasMoreAncestors`   | `boolean`                              | No       | `false`              | Whether additional older ancestors can be loaded for the node’s breadcrumb.                      |
| `loadOlderAncestors` | `LoadOlderAncestors`                   | No       | `undefined`          | Async ancestor loader forwarded to Breadcrumb; see LoadOlderAncestors below.                     |
| `apiBase`            | `string`                               | No       | `undefined`          | Base URL for this component’s API reads. This prop alone does not configure a proxy.             |
| `value`              | `number \| null`                       | No       | `null`               | Current controlled rating, 0–10. null means unrated; zero is a real rating.                      |
| `average`            | `number \| null`                       | No       | `null`               | Community average supplied by the parent; null means no average is available.                    |
| `totalVotes`         | `number`                               | No       | `0`                  | Community vote count supplied by the parent.                                                     |
| `onVote`             | `(value: number) => void`              | No       | `undefined`          | Reports a selected rating to the parent. The presentation component does not save it to the API. |
| `onNavigate`         | `(id: string) => void`                 | No       | `undefined`          | Receives the selected node ID when an ancestor is activated. The parent owns navigation.         |
| `onOpenProfile`      | `(id: string) => void`                 | No       | `undefined`          | Receives the author/participant ID to open in a profile page.                                    |
| `onReadMore`         | `(id: string) => void`                 | No       | `undefined`          | Receives the node ID when Read more is activated; absence disables that action.                  |
| `onChildType`        | `(id: string, typeId: string) => void` | No       | `undefined`          | Receives node ID and contribution type ID when a contribution-summary button is activated.       |
| `onComments`         | `(id: string) => void`                 | No       | `undefined`          | Receives the node ID when its comment-count button is activated.                                 |

### NodeCardRibbonResponsive

Common card contract using the responsive ribbon presentation. [Source](../src/lib/components/node-card-versions/NodeCardRibbonResponsive.svelte).

| Prop                 | Type                                   | Required | Default when omitted | Purpose                                                                                          |
| -------------------- | -------------------------------------- | -------- | -------------------- | ------------------------------------------------------------------------------------------------ |
| `node`               | `NodeCardData`                         | Yes      | —                    | Presentation data for one node; see NodeCardData below.                                          |
| `voting`             | `Snippet`                              | No       | `undefined`          | Optional Svelte snippet that replaces the built-in voting dial presentation.                     |
| `reactionControls`   | `Snippet`                              | No       | `undefined`          | Optional Svelte snippet that replaces the static reaction summary with custom/live controls.     |
| `hasMoreAncestors`   | `boolean`                              | No       | `false`              | Whether additional older ancestors can be loaded for the node’s breadcrumb.                      |
| `loadOlderAncestors` | `LoadOlderAncestors`                   | No       | `undefined`          | Async ancestor loader forwarded to Breadcrumb; see LoadOlderAncestors below.                     |
| `apiBase`            | `string`                               | No       | `undefined`          | Base URL for this component’s API reads. This prop alone does not configure a proxy.             |
| `value`              | `number \| null`                       | No       | `null`               | Current controlled rating, 0–10. null means unrated; zero is a real rating.                      |
| `average`            | `number \| null`                       | No       | `null`               | Community average supplied by the parent; null means no average is available.                    |
| `totalVotes`         | `number`                               | No       | `0`                  | Community vote count supplied by the parent.                                                     |
| `onVote`             | `(value: number) => void`              | No       | `undefined`          | Reports a selected rating to the parent. The presentation component does not save it to the API. |
| `onNavigate`         | `(id: string) => void`                 | No       | `undefined`          | Receives the selected node ID when an ancestor is activated. The parent owns navigation.         |
| `onOpenProfile`      | `(id: string) => void`                 | No       | `undefined`          | Receives the author/participant ID to open in a profile page.                                    |
| `onReadMore`         | `(id: string) => void`                 | No       | `undefined`          | Receives the node ID when Read more is activated; absence disables that action.                  |
| `onChildType`        | `(id: string, typeId: string) => void` | No       | `undefined`          | Receives node ID and contribution type ID when a contribution-summary button is activated.       |
| `onComments`         | `(id: string) => void`                 | No       | `undefined`          | Receives the node ID when its comment-count button is activated.                                 |

### NodeCardRibbonSplit

Common card contract using the split ribbon presentation. [Source](../src/lib/components/node-card-versions/NodeCardRibbonSplit.svelte).

| Prop                 | Type                                   | Required | Default when omitted | Purpose                                                                                          |
| -------------------- | -------------------------------------- | -------- | -------------------- | ------------------------------------------------------------------------------------------------ |
| `node`               | `NodeCardData`                         | Yes      | —                    | Presentation data for one node; see NodeCardData below.                                          |
| `voting`             | `Snippet`                              | No       | `undefined`          | Optional Svelte snippet that replaces the built-in voting dial presentation.                     |
| `reactionControls`   | `Snippet`                              | No       | `undefined`          | Optional Svelte snippet that replaces the static reaction summary with custom/live controls.     |
| `hasMoreAncestors`   | `boolean`                              | No       | `false`              | Whether additional older ancestors can be loaded for the node’s breadcrumb.                      |
| `loadOlderAncestors` | `LoadOlderAncestors`                   | No       | `undefined`          | Async ancestor loader forwarded to Breadcrumb; see LoadOlderAncestors below.                     |
| `apiBase`            | `string`                               | No       | `undefined`          | Base URL for this component’s API reads. This prop alone does not configure a proxy.             |
| `value`              | `number \| null`                       | No       | `null`               | Current controlled rating, 0–10. null means unrated; zero is a real rating.                      |
| `average`            | `number \| null`                       | No       | `null`               | Community average supplied by the parent; null means no average is available.                    |
| `totalVotes`         | `number`                               | No       | `0`                  | Community vote count supplied by the parent.                                                     |
| `onVote`             | `(value: number) => void`              | No       | `undefined`          | Reports a selected rating to the parent. The presentation component does not save it to the API. |
| `onNavigate`         | `(id: string) => void`                 | No       | `undefined`          | Receives the selected node ID when an ancestor is activated. The parent owns navigation.         |
| `onOpenProfile`      | `(id: string) => void`                 | No       | `undefined`          | Receives the author/participant ID to open in a profile page.                                    |
| `onReadMore`         | `(id: string) => void`                 | No       | `undefined`          | Receives the node ID when Read more is activated; absence disables that action.                  |
| `onChildType`        | `(id: string, typeId: string) => void` | No       | `undefined`          | Receives node ID and contribution type ID when a contribution-summary button is activated.       |
| `onComments`         | `(id: string) => void`                 | No       | `undefined`          | Receives the node ID when its comment-count button is activated.                                 |

## Shared data shapes

These are presentation contracts, not interchangeable backend DTOs. Field names and types below follow the source; callbacks and fetch adapters translate where needed.

| Type                   | Fields and meaning                                                                                                                                                                                                                                                                                                                                                                                                                                                                       | Definition                                                                                             |
| ---------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------ |
| `NodeCardPresentation` | `context-below`, `ribbon`, `ribbon-responsive`, `ribbon-split`, `ribbon-corner`: visual layouts for the same card contract.                                                                                                                                                                                                                                                                                                                                                              | [types.ts](../src/lib/components/node-card-versions/types.ts)                                          |
| `NodeCardData`         | `id`, `title`, `typeName`, `authorName`, optional `authorId`; `createdAt` timestamp; `description` display text; `status` is Active/Archived. `communities` and `requestedTypes` are label arrays. `breadcrumb` is BreadcrumbItem[]. `related` contains id/title/typeName. Optional `childTypes` contains typeId/label/count. `reactions` contains label/count. Optional `reactionSummary` and `subNodeSummary` are display summaries. `commentCount` is the displayed discussion count. | [types.ts](../src/lib/components/node-card-versions/types.ts)                                          |
| `NodeDraft`            | Editable `title`, `shortDescription`, `markdown`; `typeId` is string or null; `requestedTypeIds` is a string array; optional `parentId`; optional `newType` contains name/description and optional autoPluralize. Null typeId is used with a newly defined type.                                                                                                                                                                                                                         | [NodeForm](../src/lib/components/NodeForm.svelte)                                                      |
| `NodeType`             | `id`, `name`; optional pluralName, autoPluralize, description and isArchived. Used for type selection and labels.                                                                                                                                                                                                                                                                                                                                                                        | [nodePage.ts](../src/lib/api/nodePage.ts)                                                              |
| `ContentBlock`         | `id`, `kind`, `content`. Content can include markdown, url, altText, caption, title, description, pollId or chartId. The renderer determines supported behavior; supplying IDs does not implement interactive polls/charts.                                                                                                                                                                                                                                                              | [nodePage.ts](../src/lib/api/nodePage.ts), [ContentBlocks](../src/lib/components/ContentBlocks.svelte) |
| `BreadcrumbItem`       | Optional `id` plus `title`. Oldest ancestor first; an item without an ID is display-only.                                                                                                                                                                                                                                                                                                                                                                                                | [Breadcrumb](../src/lib/components/Breadcrumb.svelte)                                                  |
| `BreadcrumbPage`       | `items` plus `hasMore`. A loader returns this shape for an older ancestor page.                                                                                                                                                                                                                                                                                                                                                                                                          | [Breadcrumb](../src/lib/components/Breadcrumb.svelte)                                                  |
| `LoadOlderAncestors`   | `(oldestId?: string) => Promise<BreadcrumbPage>`. The caller owns lookup/cancellation.                                                                                                                                                                                                                                                                                                                                                                                                   | [Breadcrumb](../src/lib/components/Breadcrumb.svelte)                                                  |
| `FeedItem`             | `node: NodeCardData`, `average: number or null`, `totalVotes: number`; optional reactionError and hasMoreAncestors.                                                                                                                                                                                                                                                                                                                                                                      | [NodeFeed](../src/lib/components/NodeFeed.svelte)                                                      |
| `DiscoveryFilters`     | Optional parentId, typeId, authorId, communityId; minVotes/maxVotes and minAverage/maxAverage; from/through date strings; archived boolean (include archived results).                                                                                                                                                                                                                                                                                                                   | [discovery.ts](../src/lib/api/discovery.ts)                                                            |

## Storybook wrappers

These files adapt production components to Storybook navigation, fixture controls, or live previews. They are not additional application APIs. Every declared wrapper prop is listed; wrappers without props are noted explicitly.

### ActionButtonExample

[Source](../storybook/ActionButton/ActionButtonExample.svelte).

| Prop       | Type      | Required | Default when omitted | Purpose                                                             |
| ---------- | --------- | -------- | -------------------- | ------------------------------------------------------------------- |
| `label`    | `string`  | No       | `'Try action'`       | Accessible control name; also visible button text for ActionButton. |
| `disabled` | `boolean` | No       | `false`              | Prevents interaction with this control.                             |

### BreadcrumbExample

[Source](../storybook/Breadcrumb/BreadcrumbExample.svelte).

| Prop    | Type     | Required | Default when omitted | Purpose                                                                        |
| ------- | -------- | -------- | -------------------- | ------------------------------------------------------------------------------ |
| `depth` | `number` | No       | `8`                  | Number of generated ancestor labels in the Storybook breadcrumb demonstration. |

### MultipleParentsExample

[Source](../storybook/Breadcrumb/MultipleParentsExample.svelte).

No explicit props are declared. This wrapper supplies its own composition.

### VotingComparison

[Source](../storybook/Compositions/VotingComparison.svelte).

No explicit props are declared. This wrapper supplies its own composition.

### FeedExample

[Source](../storybook/Feed/FeedExample.svelte).

| Prop           | Type                   | Required | Default when omitted | Purpose                                                                             |
| -------------- | ---------------------- | -------- | -------------------- | ----------------------------------------------------------------------------------- |
| `presentation` | `NodeCardPresentation` | No       | `'ribbon-corner'`    | Selects a NodeCardPresentation layout; it does not change the underlying node data. |

### FooterExample

[Source](../storybook/Footer/FooterExample.svelte).

| Prop      | Type     | Required | Default when omitted                                   | Purpose                        |
| --------- | -------- | -------- | ------------------------------------------------------ | ------------------------------ |
| `tagline` | `string` | No       | `'A place to connect questions, evidence, and ideas.'` | Supporting text in the footer. |

### HeaderExample

[Source](../storybook/Header/HeaderExample.svelte).

| Prop            | Type                                           | Required | Default when omitted | Purpose                                                                                          |
| --------------- | ---------------------------------------------- | -------- | -------------------- | ------------------------------------------------------------------------------------------------ |
| `active`        | `ComponentProps<typeof AtlasHeader>['active']` | No       | `'discover'`         | Selected discussion-tab ID for NodePage; header components instead use a navigation destination. |
| `authenticated` | `boolean`                                      | No       | `false`              | Selects the signed-in header menu versus login/register links; does not authenticate by itself.  |
| `displayName`   | `string`                                       | No       | `'Atlas User'`       | Name used to derive the account-avatar initial.                                                  |

### LiveMyProfile

Session-aware current-participant profile, editing and community display. [Source](../storybook/MyProfile/LiveMyProfile.svelte).

| Prop            | Type                   | Required | Default when omitted   | Purpose                                                                                                       |
| --------------- | ---------------------- | -------- | ---------------------- | ------------------------------------------------------------------------------------------------------------- |
| `apiBase`       | `string`               | No       | `'/atlas-public-api'`  | Base URL for this component’s API reads. This prop alone does not configure a proxy.                          |
| `sessionBase`   | `string`               | No       | `'/atlas-session-api'` | Session-aware API base for authentication and permitted operations, including CSRF-protected writes.          |
| `onOpenNode`    | `(id: string) => void` | No       | `undefined`            | Receives a node ID to open. Where the signature includes typeId, it selects the destination contribution tab. |
| `onOpenProfile` | `(id: string) => void` | No       | `undefined`            | Receives the author/participant ID to open in a profile page.                                                 |

The profile read/write bases are configurable, but the community helper currently uses its own default endpoints. This is not a universal endpoint override for every nested helper.

### MyProfileExample

[Source](../storybook/MyProfile/MyProfileExample.svelte).

No explicit props are declared. This wrapper supplies its own composition.

### LiveNodePreview

[Source](../storybook/NodeCard/LiveNodePreview.svelte).

| Prop           | Type                   | Required | Default when omitted | Purpose                                                                             |
| -------------- | ---------------------- | -------- | -------------------- | ----------------------------------------------------------------------------------- |
| `presentation` | `NodeCardPresentation` | No       | `'ribbon'`           | Selects a NodeCardPresentation layout; it does not change the underlying node data. |

### NodeFormExample

[Source](../storybook/NodeForm/NodeFormExample.svelte).

| Prop            | Type     | Required | Default when omitted | Purpose                                                                      |
| --------------- | -------- | -------- | -------------------- | ---------------------------------------------------------------------------- |
| `parentId`      | `string` | No       | `undefined`          | Parent node ID for child-node creation; omit for root creation.              |
| `parentTitle`   | `string` | No       | `undefined`          | Display title for the parent breadcrumb; parentId is the actual association. |
| `initialTypeId` | `string` | No       | `undefined`          | Initial node type selection when creating a draft.                           |

### NodePageExample

[Source](../storybook/NodePage/NodePageExample.svelte).

| Prop            | Type     | Required | Default when omitted | Purpose                                                                                                                        |
| --------------- | -------- | -------- | -------------------- | ------------------------------------------------------------------------------------------------------------------------------ |
| `nodeId`        | `string` | No       | `''`                 | ID of the node this component reads, displays, or modifies.                                                                    |
| `initialTypeId` | `string` | No       | `''`                 | Discussion tab to select when the node story opens: a contribution type ID or comments. This is not a new-node type selection. |

### ProfileExample

[Source](../storybook/Profile/ProfileExample.svelte).

| Prop            | Type     | Required | Default when omitted | Purpose                                                                    |
| --------------- | -------- | -------- | -------------------- | -------------------------------------------------------------------------- |
| `participantId` | `string` | No       | `''`                 | Participant whose public profile and authored contributions are displayed. |

### StylePlayground

[Source](../storybook/StylesAndLayout/StylePlayground.svelte).

| Prop                | Type         | Required | Default when omitted | Purpose                                                                                               |
| ------------------- | ------------ | -------- | -------------------- | ----------------------------------------------------------------------------------------------------- |
| `fontFamily`        | `string`     | No       | `undefined`          | CSS font-family stack for workbench experiments.                                                      |
| `fontSize`          | `number`     | No       | `16`                 | Base font size in pixels.                                                                             |
| `lineHeight`        | `number`     | No       | `1.6`                | Unitless text line-height multiplier.                                                                 |
| `headingScale`      | `number`     | No       | `1.5`                | Heading size multiplier for the typography specimen and shared tokens.                                |
| `paragraphGap`      | `number`     | No       | `16`                 | Paragraph spacing in pixels.                                                                          |
| `headingGap`        | `number`     | No       | `12`                 | Heading spacing in pixels.                                                                            |
| `space`             | `number`     | No       | `undefined`          | Shared spacing override in pixels; zero follows Storybook Density.                                    |
| `cardPadding`       | `number`     | No       | `undefined`          | Standard card padding in pixels.                                                                      |
| `cardPaddingNarrow` | `number`     | No       | `undefined`          | Narrow-screen card padding in pixels.                                                                 |
| `dialTextGap`       | `number`     | No       | `undefined`          | Gap between the voting dial and adjacent text, in pixels.                                             |
| `radius`            | `number`     | No       | `undefined`          | Shared corner radius in pixels.                                                                       |
| `background`        | `string`     | No       | `undefined`          | Page background CSS color; empty uses the selected theme.                                             |
| `surface`           | `string`     | No       | `undefined`          | Panel surface CSS color; empty uses the selected theme.                                               |
| `text`              | `string`     | No       | `undefined`          | Primary text CSS color; empty uses the selected theme.                                                |
| `muted`             | `string`     | No       | `undefined`          | Secondary text CSS color; empty uses the selected theme.                                              |
| `accent`            | `string`     | No       | `undefined`          | Accent CSS color; empty uses the selected theme.                                                      |
| `onAccent`          | `string`     | No       | `undefined`          | Text color over accent surfaces; empty uses the selected theme.                                       |
| `border`            | `string`     | No       | `undefined`          | Border CSS color; empty uses the selected theme.                                                      |
| `focus`             | `string`     | No       | `undefined`          | Keyboard-focus CSS color; empty uses the selected theme.                                              |
| `onReset`           | `() => void` | No       | `undefined`          | Called when Reset styles to code defaults is activated; the parent provides the reset implementation. |

### VotingDialExample

[Source](../storybook/VotingDial/VotingDialExample.svelte).

| Prop           | Type             | Required | Default when omitted | Purpose                                                                                                               |
| -------------- | ---------------- | -------- | -------------------- | --------------------------------------------------------------------------------------------------------------------- |
| `initialValue` | `number \| null` | No       | `null`               | Initial simulated dial value in the Storybook example; null starts unrated.                                           |
| `average`      | `number \| null` | No       | `7.4`                | Community average supplied by the parent; null means no average is available.                                         |
| `totalVotes`   | `number`         | No       | `21`                 | Community vote count supplied by the parent.                                                                          |
| `compact`      | `boolean`        | No       | `false`              | Selects the compact dial layout.                                                                                      |
| `disabled`     | `boolean`        | No       | `false`              | Prevents interaction with this control.                                                                               |
| `label`        | `string`         | No       | `'Node importance'`  | Accessible control name; also visible button text for ActionButton.                                                   |
| `voteOutcome`  | `VoteOutcome`    | No       | `'accepted'`         | Simulated acceptance outcome for the standalone dial story: accepted, anonymous, or failed. No backend vote is saved. |

StylePlayground directly reads fontSize, lineHeight, headingScale, paragraphGap and headingGap. Its declared Partial<DesignSettings> also accepts the remaining style fields; Storybook’s global decorator applies those through applyDesign, rather than this component reading them. See [designSettings.ts](../storybook/StylesAndLayout/designSettings.ts) for the workbench-level defaults and token mapping. Passing those extra fields to the component alone does not apply global styles.

## Route and layout wrappers

SvelteKit selects these by directory/file convention. They are not normally imported and configured as reusable components. URL query/path parameters are router inputs, not component props; see the source links for each route’s current behavior.

| File                                                                                    | Explicit props                                                                   |
| --------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------- |
| [src/routes/+layout.svelte](../src/routes/+layout.svelte)                               | `children: Snippet`, required; current page/nested layout content.               |
| [src/routes/+page.svelte](../src/routes/+page.svelte)                                   | None declared; this page owns its composition and reads routing/state as needed. |
| [src/routes/about/+page.svelte](../src/routes/about/+page.svelte)                       | None declared; this page owns its composition and reads routing/state as needed. |
| [src/routes/communities/+page.svelte](../src/routes/communities/+page.svelte)           | None declared; this page owns its composition and reads routing/state as needed. |
| [src/routes/communities/[id]/+page.svelte](../src/routes/communities/[id]/+page.svelte) | None declared; this page owns its composition and reads routing/state as needed. |
| [src/routes/components/+page.svelte](../src/routes/components/+page.svelte)             | None declared; this page owns its composition and reads routing/state as needed. |
| [src/routes/login/+page.svelte](../src/routes/login/+page.svelte)                       | None declared; this page owns its composition and reads routing/state as needed. |
| [src/routes/me/+page.svelte](../src/routes/me/+page.svelte)                             | None declared; this page owns its composition and reads routing/state as needed. |
| [src/routes/my-drafts/+page.svelte](../src/routes/my-drafts/+page.svelte)               | None declared; this page owns its composition and reads routing/state as needed. |
| [src/routes/my-settings/+page.svelte](../src/routes/my-settings/+page.svelte)           | None declared; this page owns its composition and reads routing/state as needed. |
| [src/routes/new/+page.svelte](../src/routes/new/+page.svelte)                           | None declared; this page owns its composition and reads routing/state as needed. |
| [src/routes/nodes/+page.svelte](../src/routes/nodes/+page.svelte)                       | None declared; this page owns its composition and reads routing/state as needed. |
| [src/routes/profiles/+page.svelte](../src/routes/profiles/+page.svelte)                 | None declared; this page owns its composition and reads routing/state as needed. |
| [src/routes/profiles/me/+page.svelte](../src/routes/profiles/me/+page.svelte)           | None declared; this page owns its composition and reads routing/state as needed. |
| [src/routes/register/+page.svelte](../src/routes/register/+page.svelte)                 | None declared; this page owns its composition and reads routing/state as needed. |

Related: [Svelte reading guide](SVELTE-READING-GUIDE.md), [SvelteKit orientation](SVELTEKIT.md), [Storybook workbench](STORYBOOK-WORKBENCH.md).
