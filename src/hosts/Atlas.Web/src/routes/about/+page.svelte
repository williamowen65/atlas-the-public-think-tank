<!--
  @component
  Public-facing introduction to the Atlas hypothesis and mission.

  **Props**
  No explicit component props. This is static editorial content.
-->
<svelte:head>
  <title>About Atlas | A Public Think Tank</title>
  <meta
    name="description"
    content="Discover Atlas, an experiment in organizing questions, evidence, experiences, and ideas into a public think tank."
  />
</svelte:head>

<main class="about-page">
  <header class="hero">
    <p class="eyebrow">About Atlas</p>
    <h1>A Public Think Tank</h1>
    <p class="lead">An open, evolving place for exploring questions, understanding problems, developing ideas, and building on the thinking of others.</p>
    <p>Atlas is an experiment in whether the internet can support a more organized form of collective thinking.</p>
  </header>

  <section aria-labelledby="problem">
    <h2 id="problem">The Problem: A Shortage of Structure</h2>
    <p>People generate enormous amounts of useful thought every day. Problems are identified, solutions are suggested, questions are asked, evidence is shared, and people make connections based on their own knowledge and experience.</p>
    <p>But much of this thinking is scattered.</p>
    <p>A problem may be discussed in one place while a useful solution is suggested somewhere else. Research may exist without reaching the people who would benefit from it. Valuable questions and ideas can disappear when a conversation ends.</p>
    <p class="emphasis">The problem is not a shortage of information. It is a shortage of structure.</p>
  </section>

  <section aria-labelledby="hypothesis">
    <h2 id="hypothesis">The Atlas Hypothesis</h2>
    <p>Atlas is based on the hypothesis that the tools people already use to communicate and share ideas online can be reorganized into something more deliberate.</p>
    <p>Instead of organizing participation mainly around a feed or disconnected posts, Atlas organizes contributions into a persistent, navigable structure.</p>
    <p>A contribution might be a question, a problem, an idea, an observation, evidence, an experience, or a possible solution. Each contribution can become part of a larger picture, connected to other ideas and perspectives.</p>
    <p>The goal is for this structure to grow through participation.</p>
  </section>

  <section aria-labelledby="how-it-works">
    <h2 id="how-it-works">How Atlas Works</h2>
    <div class="features">
      <article>
        <h3>Explore connected ideas</h3>
        <p>Begin with a question or problem and follow connections to related ideas, explanations, evidence, and possible solutions.</p>
      </article>
      <article>
        <h3>Contribute your perspective</h3>
        <p>Ask questions, share knowledge or experiences, challenge existing ideas, and add new branches of thought.</p>
      </article>
      <article>
        <h3>Help identify what matters</h3>
        <p>Rate the importance of contributions to help bring attention to questions, problems, and ideas that deserve consideration.</p>
      </article>
      <article>
        <h3>Build on what came before</h3>
        <p>Contributions remain part of a growing structure that others can discover, question, and improve over time.</p>
      </article>
    </div>
  </section>

  <section aria-labelledby="differences">
    <h2 id="differences">Thinking Together, Across Differences</h2>
    <p>No one person can reasonably understand every part of a complex problem.</p>
    <p>Experts may understand one area deeply while others contribute knowledge from different fields. People without formal expertise may still bring valuable experiences, questions, observations, or connections.</p>
    <p>Atlas is intended to make room for all of these contributions, including competing explanations and disagreements, within a shared structure.</p>
    <p>The goal isn't necessarily agreement. It's a better understanding of the questions, evidence, and ideas involved.</p>
  </section>

  <section aria-labelledby="improvement">
    <h2 id="improvement">An Experiment That Can Improve Itself</h2>
    <p>Atlas itself can be discussed and questioned through the same system.</p>
    <p>Participants can explore how well its voting, moderation, discovery, organization, and other mechanics are working and contribute ideas for improving them.</p>
    <p>The public think tank should be able to evolve alongside the people using it.</p>
  </section>

  <section id="mission" class="mission" aria-labelledby="mission-heading">
    <h2 id="mission-heading">Building on the Thinking of Others</h2>
    <p>Atlas doesn't need to solve the world's problems itself to be valuable. Its role is to provide better infrastructure for the thinking that happens before solutions are found.</p>
    <p>A question asked today might become the starting point for an idea developed years later. Future participants should be able to inherit what others have learned, challenge it, and improve it.</p>
    <p class="emphasis">The thinking shouldn't have to disappear when the original conversation ends.</p>
  </section>
</main>

<style>
  .about-page {
    width: min(100% - 2rem, 860px);
    margin: 0 auto;
    padding: clamp(1.5rem, 4vw, 3.5rem) 0 5rem;
    line-height: 1.7;
  }

  .hero,
  .mission {
    padding: clamp(1.5rem, 4vw, 3rem);
    border-radius: 1rem;
    background: var(--color-surface, #f4f6f8);
    color: var(--color-text, #202b36);
  }

  .eyebrow {
    margin: 0 0 0.5rem;
    font-size: 0.8rem;
    font-weight: 700;
    letter-spacing: 0.1em;
    text-transform: uppercase;
  }

  h1, h2, h3 {
    line-height: 1.25;
  }

  h1 {
    margin: 0 0 1rem;
    font-size: clamp(2rem, 5vw, 3.25rem);
  }

  h2 {
    margin: 0 0 1.25rem;
    font-size: clamp(1.4rem, 3vw, 1.9rem);
  }

  h3 {
    margin: 0 0 0.5rem;
    font-size: 1.1rem;
  }

  p {
    margin: 0 0 1rem;
  }

  p:last-child {
    margin-bottom: 0;
  }

  .lead {
    font-size: clamp(1.1rem, 2.4vw, 1.35rem);
    font-weight: 600;
  }

  section:not(.mission) {
    padding: 2.5rem 0;
    border-bottom: 1px solid var(--color-border, #dce1e5);
  }

  .hero {
    margin-bottom: 0.5rem;
  }

  .features {
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 1rem;
  }

  .features article {
    padding: 1.25rem;
    border: 1px solid var(--color-border, #dce1e5);
    border-radius: 0.75rem;
  }

  .emphasis {
    font-weight: 700;
    font-size: 1.1rem;
  }

  .mission {
    margin-top: 2.5rem;
  }

  @media (max-width: 600px) {
    .features {
      grid-template-columns: 1fr;
    }

    section:not(.mission) {
      padding: 2rem 0;
    }
  }
</style>
