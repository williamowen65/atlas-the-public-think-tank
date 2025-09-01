
/*

This file manages the client-side logic for pagination




The pagination setup also requires .NET entities to contain paginated content.

*/


function setPaginationButtonListener(paginationButtonElement) {
    if (paginationButtonElement) {
        paginationButtonElement.addEventListener("click", (e => {


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
                    });

            } else {

                   console.error("Next page number not found for pagination")
            }

        }));
    }
}