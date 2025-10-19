// This file is only loaded via playwright in WebPhotographer.cs
// It is not meant to be on the public site. Although, for testing purposes of the methods, it could be added.

// The purpose of these methods is to prep content for a photoshoot
// Ex: When photoing the body of the site, the site dimensions should be not scrollable, cutoff.
// Ex: the content cards should have a share friendly look to them

//document.addEventListener("DOMContentLoaded", () => {
//    PrepHomePage()
//})
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