
/*

This file manages the client-side logic for pagination




*/


function setPaginationButtonListener(paginationButtonElement) {
    if (paginationButtonElement) {
        paginationButtonElement.addEventListener("click", (e => {

            const paginationUrl = paginationButtonElement.getAttribute("data-url")

            if (paginationUrl) {
                // Call api/posts for next page of data

                fetch(paginationUrl)
                    .then(response => response.text())
                    .then(data => {

                        // Add issues to dom
                        const domTarget = document.querySelector('#main-content')
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