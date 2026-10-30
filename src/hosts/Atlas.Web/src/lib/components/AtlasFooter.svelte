<!--
  @component
  AtlasFooter: Shared grouped navigation links and site footer.
  Links use normal application URLs; presentation text can be customized by the parent.

  **Props**
  - `tagline` (`string`; optional; default `'A place to connect questions, evidence, and ideas.'`):
    Supporting text in the footer.
-->
<script lang="ts">
  let {
    tagline = 'A place to connect questions, evidence, and ideas.',
  }: {
    /** Supporting text in the footer. */
    tagline?: string;
  } = $props();
  const year = new Date().getFullYear();
  // Keep link groups as display data; links use normal application navigation.
  const groups = [
    {
      title: 'Explore',
      links: [
        { text: 'Discover', href: '/' },
        { text: 'Create a contribution', href: '/new' },
        { text: 'How Atlas works', href: '/about#how-it-works' },
        { text: 'Guides', href: '/guides' },
      ],
    },
    {
      title: 'About',
      links: [
        { text: 'About Atlas', href: '/about' },
        { text: 'Our mission', href: '/about#mission' },
        { text: 'GitHub', href: 'https://github.com/williamowen65/atlas-the-public-think-tank' },
      ],
    },
    {
      title: 'Get involved',
      links: [
        { text: 'Give feedback', href: '/feedback' },
        { text: 'Become a tester', href: '/testers' },
        { text: 'Support Atlas', href: '/support' },
        { text: 'Donate', href: '/support#donate' },
      ],
    },
    {
      title: 'Help & legal',
      links: [
        { text: 'Help & support', href: '/support' },
        { text: 'Privacy policy', href: '/privacy' },
        { text: 'Terms of use', href: '/terms' },
      ],
    },
  ];
</script>

<!-- Layout: Grouped navigation sits above the site identity, tagline and copyright row. -->

<footer class="atlas-footer">
  <div class="footer-links">
    {#each groups as group}
      <nav aria-label={group.title}>
        <strong>{group.title}</strong>
        {#each group.links as link}<a href={link.href}>{link.text}</a>{/each}
      </nav>
    {/each}
  </div>
  <div class="footer-bottom">
    <div>
      <strong>Atlas · The Public Think Tank</strong>
      <p>{tagline}</p>
    </div>
    <span>© {year} Atlas</span>
  </div>
</footer>

<style lang="scss">
  .atlas-footer {
    position: relative;
    padding: 2rem 0;
    margin-top: 2rem;
    border-top: 0;
    font-size: 0.9rem;
    color: var(--atlas-muted);
  }
  .atlas-footer::before {
    content: '';
    position: absolute;
    top: 0;
    left: 50%;
    width: 100vw;
    transform: translateX(-50%);
    border-top: 1px solid var(--atlas-border);
    pointer-events: none;
  }
  .footer-links {
    display: grid;
    grid-template-columns: repeat(4, minmax(0, 1fr));
    gap: 1.5rem 2rem;
    margin: 0 auto 2rem;
    padding-inline: clamp(1rem, 3vw, 3rem);
    width: 100%;
    max-width: 80rem;
    box-sizing: border-box;
  }
  nav {
    display: flex;
    flex-direction: column;
    align-items: center;
    text-align: center;
    justify-self: center;
    width: max-content;
    max-width: 100%;
    gap: 0.6rem;
    strong {
      color: var(--atlas-text);
      margin-bottom: 0.3rem;
    }
    a {
      color: var(--atlas-muted);
      text-decoration: none;
    }
    a:hover {
      text-decoration: underline;
    }
  }
  .footer-bottom {
    border-top: 1px solid var(--atlas-border);
    padding-top: 1.5rem;
    display: flex;
    flex-direction: column;
    align-items: center;
    text-align: center;
    gap: 0.75rem;
    p {
      margin: 0.4rem 0;
      max-width: 40rem;
    }
  }
  @media (max-width: 700px) {
    .footer-links {
      // A wider first column balances its longer links and shifts the second column right.
      grid-template-columns: minmax(0, 1.2fr) minmax(0, 1fr);
    }
    nav {
      // Fill each mobile column so stacked sections share the same center line.
      justify-self: stretch;
      width: 100%;
      min-width: 0;
      overflow-wrap: anywhere;
    }
  }
  @media (max-width: 360px) {
    .footer-links {
      // Keep both columns on small phones while leaving room for link text to wrap.
      gap: 1.5rem 1rem;
      padding-inline: 0.5rem;
    }
    nav {
      min-width: 0;
      overflow-wrap: anywhere;
    }
  }
</style>
