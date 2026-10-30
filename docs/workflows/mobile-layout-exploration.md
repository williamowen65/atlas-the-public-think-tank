# Mobile layout exploration

This branch compares Atlas with the supplied Reddit mobile screenshots. It is an experiment for phone review, not a requirement to copy Reddit.

## Review the alternatives

At phone widths (560px and below), the application feed uses the mobile experiment. In Storybook, open **Compositions / Feed / Mobile Experiment**. Compare it with **Playground** using the ribbon-corner presentation at the same viewport width. Desktop keeps the existing card arrangement; full node pages keep their detailed metadata and dial.

The mobile experiment uses half-rem outer gutters, a smaller node-type label above the title, a shorter byline without the By/Community prefixes, and full-width introduction text. Community links remain in the byline, away from the ancestor ribbon. Long community names still wrap.

The rating summary shows the authoritative average, the existing people icon, compact vote count and a labeled Rate action. Tapping expands the same LiveVotingDial inline. Arrows adjust a preview, Enter requests review, and touch/wheel gestures retain settling and explicit confirmation. Closing the disclosure cancels an unconfirmed preview. The summary never calculates averages or casts votes. Zero remains a valid rating. Full details retain the full dial for comparison.

Reactions and contribution counts remain separate rows. This pass does not collapse requested contribution types or introduce unexplained aggregate counts.

## Navigation and search

The mobile header retains the Atlas tagline and offers a wide Search Atlas entry that opens the existing input/results/filter controls in the header. The entry is a disclosure button, not a second search implementation.

Discover, Create and You appear in bottom navigation. Signed-out You opens login. The navigation follows header scroll visibility: hidden when reading downward, restored upward or near the top. Search, an open navigation menu and keyboard focus keep navigation visible. Bottom safe-area clearance keeps content reachable. Desktop navigation remains as before.

The menu experiments with a left community drawer. It reads actual joined communities on opening, links directly to their pages, and keeps a link to browse all communities. It does not fabricate favorites, recent history, notifications or an inbox.

## Joining from cards

Experimental feed cards show Join beside linked active communities when membership is known. Already joined communities say Joined. Unknown membership disables Join rather than guessing; fallback community names without IDs cannot be joined until the public association lookup succeeds.

Joining uses the existing PUT communities/{id}/membership through the shared session/CSRF transport. A 401 sends the visitor to login with a return URL; joining is never submitted automatically after login. Failure offers another explicit attempt. A membership-change event refreshes other mounted card controls; the switcher re-reads membership on opening. Node page membership management remains available through the community page.

## Validation and open decisions

Browser coverage is in tests/mobile-layout.spec.ts: opening/closing rating without a mutation, explicit Enter review, bottom-navigation scroll visibility, and CSRF-protected membership writes. Test fixtures do not replace a real Identity/membership smoke check.

Review on a phone: content width, title hierarchy, long community names, Join visibility, expanded dial height, thumb reach, drawer dismissal, search placement, keyboard focus and bottom browser controls. Try both light and dark themes, zoom and reduced motion.

The compact rating currently expands inline. A bottom sheet is an alternative if inline expansion feels disruptive. The wide search disclosure is an initial comparison; a permanently editable header input could be a later alternative.
