document.addEventListener("click", (e) => {
    const el = e.target;


    // NOTE: This is a link and only works because enableSendBeacon: true
    if (el.classList.contains("view-btn")) {
        const card = el.closest(".card")
        const contentType = card.getAttribute("data-content-type")
        const contentTitle = card.querySelector(".card-title").innerText;

        appInsights.trackEvent({
            name: "Content Item - View Button Click",
            properties: {
                contentType,
                contentTitle
            }
         });
    }

    if (el.closest(".breadcrumb-item-custom")) {
        const breadcrumbItem = el.closest(".breadcrumb-item-custom");
        const breadcrumbText = breadcrumbItem.querySelector(".breadcrumb-title").innerText;
        appInsights.trackEvent({
            name: "Breadcrumb Navigation Click",
            properties: {
                breadcrumbText
            }
        });
    }


    if (el.classList.contains("card-expand-toggle")) {

        // Get card type and title
        const card = el.closest(".card")
        const contentType = card.getAttribute("data-content-type")
        const contentTitle = card.querySelector(".card-title").innerText;

        appInsights.trackEvent({
            name: "Content Item - Expand Button Click", 
            properties: {
                contentType,
                contentTitle
            } 
        });
    }

    if (el.classList.contains("card-minimize-toggle")) {

        // Get card type and title
        const card = el.closest(".card")
        const contentType = card.getAttribute("data-content-type")
        const contentTitle = card.querySelector(".card-title").innerText;

        appInsights.trackEvent({
            name: "Content Item - Minimize Button Click", 
            properties: {
                contentType,
                contentTitle
            } 
        });
    }

    if (el.closest("#fetchPaginatedContent")) {
        const paginationButton = el.closest("#fetchPaginatedContent")

        // Get the pagination numbers
        const paginatedCounts = paginationButton.querySelector(".fullPaginatedCount").innerText

        appInsights.trackEvent({
            name: "Main page Pagination - Button Click", 
            properties: {
                paginatedCounts
            } 
        });
        console.log("pagination event tracked")
    }

  

})



/*
   These events need to happen before the page navigation
*/
class ContentNavigationEvent {

    static ViewButtonClick = (el) => {
        const card = el.closest(".card")
        const contentType = card.getAttribute("data-content-type")
        const contentTitle = card.querySelector(".card-title").innerText;

        appInsights.trackEvent({
            name: "Content Item - View Button Click",
            properties: {
                contentType,
                contentTitle
            }
        });
        console.log("Button click tracked!");
    }

    static BreadcrumbClick = (el) => {
        //if (el.closest(".breadcrumb-item-custom")) {
        //    const breadcrumbItem = el.closest(".breadcrumb-item-custom");
        //    const breadcrumb
        //}
    }

}