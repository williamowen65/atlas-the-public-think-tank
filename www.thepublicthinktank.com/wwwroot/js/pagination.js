
/*

This file manages the client-side logic for pagination




*/


function setPaginationButtonListener(paginationButtonElement) {
    if (paginationButtonElement) {
        paginationButtonElement.addEventListener("click", (e => {

            const paginationUrl = paginationButtonElement.getAttribute("data-url")
            const paginationTargetElement = paginationButtonElement.getAttribute("data-target")
            const paginationPageSize = paginationButtonElement.getAttribute("data-page-size")
            const paginationTotalCount = paginationButtonElement.getAttribute("data-total-count")
            const paginatedCountElement = paginationButtonElement.querySelector(".paginatedCount")
            const paginatedButtonText = paginationButtonElement.querySelector(".button-text")

            if (!paginationTargetElement) {
                throw new Error("Could not find pagination target element")
            }
            if (!paginatedCountElement) {
                throw new Error("Could not locate a pagination count element")
            }
            if (!paginationPageSize) {
                throw new Error("Could not find the pagination page size value")
            }
            if (!paginationTotalCount) {
                throw new Error("Could not determine the pagination total count")
            }
            if (!paginatedButtonText) {
                 throw new Error("Could not find pagination button text element")
            }

            if (paginationUrl) {
                // Call api/posts for next page of data

                fetch(paginationUrl)
                    .then(response => response.text())
                    .then(data => {

                        // Add issues to dom
                        const domTarget = document.querySelector(paginationTargetElement)
                        domTarget.insertAdjacentHTML("beforeend", data)

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

                        // Update the pagination button count
                        const currentCount = Number(paginatedCountElement.innerText)
                        const nextCurrentCount = currentCount + Number(paginationPageSize)
                        paginatedCountElement.innerText = Math.min(nextCurrentCount, paginationTotalCount)
                        if (nextCurrentCount >= paginationTotalCount) {
                            // Disable the button
                            paginationButtonElement.disabled = true;
                            paginatedButtonText.innerText = "No more posts"
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