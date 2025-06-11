
/*

This file manages the client-side logic for pagination




*/


function setPaginationButtonListener(paginationButtonElement) {
    if (paginationButtonElement) {
        paginationButtonElement.addEventListener("click", (e => {

            const nextPageNumber = paginationButtonElement.getAttribute("data-next-page")

            if (nextPageNumber) {
                // Call api/posts for next page of data

                fetch(`/issue/getPaginatedIssues?currentPage=${nextPageNumber}`)
                    .then(response => response.text())
                    .then(data => {

                        // Add issues to dom
                        const domTarget = document.querySelector('#main-content')
                        domTarget.insertAdjacentHTML("beforeend", data)

                        paginationButtonElement.setAttribute("data-next-page", Number(nextPageNumber) + 1)


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