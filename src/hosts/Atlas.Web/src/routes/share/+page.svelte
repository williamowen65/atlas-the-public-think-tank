<script lang="ts">
  import { page } from '$app/state';

  const designs = [
    {
      title: 'Explore Atlas',
      description: 'Browse public questions, ideas, and proposals in Discover.',
      path: '/',
      image: '/share-images/discover.png',
      alt: 'The Atlas Discover feed',
    },
    {
      title: 'See how ideas connect',
      description: 'Follow a question through an issue to a possible solution.',
      path: '/share?design=connections',
      image: '/atlas-share-connections.png',
      alt: 'A question about summer heat connects to an issue about shade and a solution about planting trees.',
    },
    {
      title: 'Put your pin on the map',
      description: 'See where people are joining Atlas and add your own pin.',
      path: '/?globe=open',
      image: '/share-images/globe.png',
      alt: 'The Atlas globe with community pins and an invitation to add yours',
    },
    {
      id: 'globe-gif',
      title: 'Put your pin on the map · GIF',
      description: 'Share the globe making a full turn.',
      path: '/?globe=open&motion=gif',
      image: '/share-images/globe.gif',
      alt: 'The Atlas globe and community pins rotating through a full turn',
    },
  ];

  const selected = $derived(page.url.searchParams.get('design') === 'connections' ? designs[1] : undefined);
  let globeView = $state('');
  let message = $state('');
  let manualLink = $state('');
  let gifMessage = $state('');
  let copyingGif = $state(false);

  async function copyGlobeGif(imagePath: string) {
    if (copyingGif) return;
    gifMessage = '';
    copyingGif = true;
    try {
      if (!navigator.clipboard?.write || typeof ClipboardItem === 'undefined') {
        shareFeedback = 'This browser cannot copy the GIF. You can still copy the Atlas link.';
        return;
      }

      // Start the clipboard write during the click, before the network request completes.
      const gif = fetch(imagePath).then(async (response) => {
        const image = await response.blob();
        if (!response.ok || image.type !== 'image/gif' || !image.size) {
          throw new Error('The animated globe image is unavailable.');
        }
        return image;
      });
      const nativeGif = ClipboardItem.supports?.('image/gif');
      if (nativeGif) {
        await navigator.clipboard.write([new ClipboardItem({ 'image/gif': gif })]);
      } else {
        // Rich clipboard HTML can carry the animated bytes when native GIF is unavailable.
        const html = gif.then((image) => new Promise<Blob>((resolve, reject) => {
          const reader = new FileReader();
          reader.onerror = () => reject(reader.error);
          reader.onload = () => {
            const fragment = document.createElement('div');
            const animation = document.createElement('img');
            animation.src = String(reader.result);
            animation.alt = 'Animated Atlas globe';
            fragment.append(animation);
            resolve(new Blob([fragment.innerHTML], { type: 'text/html' }));
          };
          reader.readAsDataURL(image);
        }));
        await Promise.all([navigator.clipboard.write([new ClipboardItem({ 'text/html': html })]), html]);
      }
      shareFeedback = 'GIF copied. Paste into your Facebook post and check that the GIF plays.';
    } catch {
      shareFeedback = 'Could not copy the GIF. You can still copy the Atlas link.';
    } finally {
      copyingGif = false;
    }
  }

  async function copyLink(path: string) {
    const url = new URL(path, window.location.origin).href;
    message = '';
    manualLink = '';
    try {
      await navigator.clipboard.writeText(url);
      message = 'Share link copied.';
    } catch {
      manualLink = url;
      message = 'Copy the link below.';
    }
  }
</script>

<svelte:head><title>{selected?.title ?? 'Share Atlas'} · Atlas</title></svelte:head>

<div class="share-page">
  {#if selected}
    <h1>{selected.title}</h1>
    <img class="feature" src={selected.image} alt={selected.alt} width="1200" height="630" />
    <p>Pick a question in Discover and open its contributions to see how people build on it.</p>
    <a class="action" href="/">Explore Discover</a>
    <a href="/share">See other ways to share</a>
  {:else}
    <h1>Share Atlas</h1>
    <p>Choose the preview you want people to see when you share the link.</p>
    <div class="designs">
      {#each designs as design (design.path)}
        {@const view = design.path === '/?globe=open' ? globeView : ''}
        {@const link = view ? design.path + '&view=' + view : design.path}
        <article id={design.id}>
          {#key view}
          <img src={view ? design.image + '?view=' + view : design.image} alt={design.alt} width="1200" height="630"
            onerror={(event) => {
              if (event.currentTarget instanceof HTMLImageElement && !event.currentTarget.src.endsWith('/atlas-share.png')) event.currentTarget.src = '/atlas-share.png';
            }} />
          {/key}
          <div class="details">
            <h2>{design.title}</h2>
            <p>{design.description}</p>
            {#if design.path === '/?globe=open'}
              <label class="globe-view">Globe view
                <select bind:value={globeView}>
                  <option value="">Default</option>
                  <option value="0">0°</option>
                  <option value="90">90° east</option>
                  <option value="180">180°</option>
                  <option value="270">90° west</option>
                </select>
              </label>
            {/if}
            <div class="card-actions">
              <button class="action" type="button" onclick={() => copyLink(link)} aria-label={'Copy link: ' + design.title}>Copy link</button>
              {#if design.id === 'globe-gif'}
                <button class="action" type="button" disabled={copyingGif} onclick={() => copyGlobeGif(design.image)}>
                  {copyingGif ? 'Copying GIF…' : 'Copy GIF'}
                </button>
              {/if}
            </div>
            {#if design.id === 'globe-gif'}
              <p role="status">{gifMessage}</p>
            {/if}
          </div>
        </article>
      {/each}
    </div>
    <p role="status">{message}</p>
    {#if manualLink}
      <label>Share link <input readonly value={manualLink} onfocus={(event) => event.currentTarget.select()} /></label>
    {/if}
  {/if}
</div>

<style>
  .share-page { max-width: 66rem; margin: 2rem auto; padding-inline: 1rem; }
  .share-page > p, .details p { color: var(--atlas-muted); }
  .designs { display: grid; grid-template-columns: repeat(auto-fit, minmax(min(100%, 17rem), 1fr)); gap: 1rem; }
  article { display: grid; grid-template-rows: auto 1fr; border: 1px solid var(--atlas-border); border-radius: var(--atlas-radius); overflow: hidden; background: var(--atlas-surface); }
  img { display: block; width: 100%; height: auto; }
  .feature { max-width: 48rem; border-radius: var(--atlas-radius); }
  .details { display: flex; flex-direction: column; align-items: start; padding: 1rem; }
  h2 { font-size: 1.15rem; margin: 0; }
  .details p { margin: .5rem 0 1rem; }
  .action { display: inline-block; font: inherit; margin: auto .75rem 0 0; padding: .55rem .85rem; border: 1px solid var(--atlas-accent); border-radius: var(--atlas-radius); color: var(--atlas-on-accent); background: var(--atlas-accent); text-decoration: none; cursor: pointer; }
  #globe-gif { scroll-margin-top: 6rem; }
  .card-actions { display: flex; flex-wrap: wrap; gap: .5rem; margin-top: auto; }
  .card-actions .action { margin: 0; }
  .globe-view { display: flex; gap: .5rem; align-items: center; margin-block-end: .75rem; }
  select { font: inherit; }
  input { display: block; width: min(100%, 36rem); font: inherit; padding: .5rem; }
</style>
