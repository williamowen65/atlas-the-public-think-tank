// This file is only loaded via playwright in WebPhotographer.cs
// It is not meant to be on the public site. Although, for testing purposes of the methods, it could be added.

// The purpose of these methods is to prep content for a photoshoot
// Ex: When photoing the body of the site, the site dimensions should be not scrollable, cutoff.
// Ex: the content cards should have a share friendly look to them



//document.addEventListener("DOMContentLoaded", () => {
//    PrepContentCard();
//})


/**
 * This will be ran on the issue or solution read pages.
 * 
 * 
 */
function PrepContentCard() {
    const selector = ".card.issue-card, .card.solution-card"

    const element = document.querySelector(selector)

    const contentEl = element.querySelector(".issue-content, .solution-content, .text-collapsible-target")

    contentEl.classList.add("truncate-multiline")

    // For content cards
    // set dimensions 600 × 315 pixel

    //element.style.height = "315px"

    // Wrap the card in a div
    const wrapper = document.createElement("div");
    wrapper.className = "photographer-card-wrapper";
    element.parentNode.insertBefore(wrapper, element);
    wrapper.appendChild(element);
    wrapper.style.width = "670px"

    const statIcons = element.querySelector(".issue-card-stat-icons")
    const wbr = statIcons.querySelector("wbr");
    if (wbr) {
        // Solution cards don't have the wbr
        const br = document.createElement("br");
        wbr.replaceWith(br);
    }

    const isIssue = element.classList.contains("issue-card")
    const text = isIssue ? "Does this issue matter to you?" : "Is this a good solution?"

    element.insertAdjacentHTML("beforebegin", `
        <div class='text-center '>
            <h1>${text}</h1>
            <h4 class="position-relative d-flex">

            <div class="flex-grow-1 arrow-to-dial"
                style="
                    border: 1px solid white;
                    margin-left: 55px;
                    border-bottom: none;
                    border-right: none;
                    transform: translate(-5px, 50%)
                "
            ></div>

            Cast your vote

            <div class="flex-grow-1"
                style="margin-right: 55px;"
            ></div>
           
            </h4>
             
        </div>
    `)



    
}
window.PrepContentCard = PrepContentCard;






/**
 * This will be ran against the home page screen
 */
function PrepHomePage() {
    // Apply to body
    document.body.style.height = "100vh";
    document.body.style.overflow = "hidden";

    // Apply to html
    document.documentElement.style.height = "100vh";
    document.documentElement.style.overflow = "hidden";

    // Set filter view
    const filterToggle = document.querySelector(".toggle-option[for=filter]")
    filterToggle.click();
}

window.PrepHomePage = PrepHomePage;