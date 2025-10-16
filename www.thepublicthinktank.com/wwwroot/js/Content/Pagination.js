/*

This file manages the client-side logic for pagination

The pagination button is getting an update (10/1/2025)
- It will now trigger on scroll. Users won't have to manually click it.

pre-conditions:
- Controller action endpoints for the paginated content.
- Adding the pagination button partial to the UI where it is needed

*/

document.addEventListener("DOMContentLoaded", () => {
    setPaginationIntersectionObserver();
})

function setPaginationButtonListener(paginationButtonElement) {
    if (paginationButtonElement) {
        paginationButtonElement.addEventListener("click", (e => {

            // Disable button, Add Spinner
            paginationButtonElement.disabled = true;

            const spinner = document.createElement("span");
            spinner.className = "spinner-border spinner-border-sm ms-2 pagination-spinner";
            spinner.setAttribute("role", "status");
            spinner.setAttribute("aria-hidden", "true");
            paginationButtonElement.insertAdjacentElement("beforeend", spinner)


            const paginationUrl = paginationButtonElement.getAttribute("data-url")
            const paginationTargetElement = paginationButtonElement.getAttribute("data-target")
            const paginatedCountElement = paginationButtonElement.querySelector(".fullPaginatedCount")
            const paginatedButtonText = paginationButtonElement.querySelector(".button-text")
            const paginationContentType = paginationButtonElement.getAttribute("data-content-type")
            if (!paginationTargetElement) {
                throw new Error("Could not find pagination target element")
            }
            if (!paginatedCountElement) {
                throw new Error("Could not locate a pagination count element")
            }
            if (!paginationContentType) {
                throw new Error("Could not find pagination button content type")
            }
         
            if (!paginatedButtonText) {
                 throw new Error("Could not find pagination button text element")
            }
           

            if (paginationUrl) {
                // Call api/posts for next page of data

                fetch(paginationUrl)
                    .then(response => response.json())
                    .then(data => {

                        console.log("Paginated Response", {
                                data
                        })

                        // Add issues to dom
                        const domTarget = document.querySelector(paginationTargetElement)
                        domTarget.insertAdjacentHTML("beforeend", data.html)

                        // Find the current page number and increment it for the next request
                        const currentPageRegex = /currentPage=(\d+)/;
                        const match = paginationUrl.match(currentPageRegex);

                        if (match && match[1]) {
                            const currentPage = parseInt(match[1]);
                            const nextPage = currentPage + 1;
                            const newUrl = paginationUrl.replace(currentPageRegex, `currentPage=${nextPage}`);
                            paginationButtonElement.setAttribute("data-url", newUrl);
                        } else {
                            console.error("Could not extract page number from URL");
                        }

                        const newCount = Array.from(domTarget.querySelectorAll(".issue-card, .solution-card")).length

                        // During the normal workflow, reenable the button after paginated data is retrieved. 
                        paginationButtonElement.disabled = false;
                        paginationButtonElement.querySelector(".pagination-spinner").remove()

                        // Update the pagination button count
                        paginatedCountElement.innerText = `(${newCount}/${data.pagination.totalCount})`;
                        if (newCount >= data.pagination.totalCount) {
                            // Disable the button
                            paginationButtonElement.disabled = true;
                            paginatedButtonText.innerText = `No more ${paginationContentType}`;
                        }

                    })
                    .catch(error => {
                        console.error('Error fetching issues:', error);
                        paginationButtonElement.querySelector(".pagination-spinner").remove()
                        paginationButtonElement.disabled = false;
                    });

            } else {

                   console.error("Next page number not found for pagination")
            }

        }));
    }
}

/**
 * Watches for the pagination button to enter the bottom of the screen
 * - Enter the screen in general
 * - Plus extra half screen width below
 */
function setPaginationIntersectionObserver() {
    const paginationButtons = document.querySelectorAll(".pagination-button");
    if (!paginationButtons.length) return;

    const rootMargin = `0px 0px ${window.innerHeight / 2}px 0px`;

    const observer = new IntersectionObserver((entries, obs) => {
        entries.forEach(entry => {
            const button = entry.target;
            if (entry.isIntersecting && !button.disabled) {
                button.click();
            }
        });
    }, {
        root: null,
        threshold: 0,
        rootMargin: rootMargin
    });

    paginationButtons.forEach(button => {
        observer.observe(button);
    });
}