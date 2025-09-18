document.addEventListener("DOMContentLoaded", () => {
    const searchBarId = "#header-search-bar";
    const searchBar = document.querySelector(searchBarId);

    var sendSearchRequestDebounced = throttle(sendSearchRequest, 3000);
    searchBar.addEventListener("keyup", (e) => {
        const searchString = e.target.value;
        if (searchString.length > 4) {
            sendSearchRequestDebounced(e);
        }
    });



    function sendSearchRequest(e) {
        const searchString = e.target.value;
        fetch("https://localhost:5501/search", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ searchString: searchString })
        })
        .then(response => response.json())
        .then(data => {
            console.log("Search results:", data);
            populateSearchResults(data)
        })
        .catch(error => {
            console.error("Error:", error);
        });
    }

    function populateSearchResults(data) {
        const searchResultsContainer = document.querySelector("#header-search-bar-results");
        searchResultsContainer.innerHTML = "";

        if (!Array.isArray(data) || data.length === 0) {
            searchResultsContainer.innerHTML = "<div class='search-result-empty'>No results found.</div>";
            return;
        }

        data.forEach(item => {
            // Create a result option element
            const option = document.createElement("div");
            option.className = "search-result-option";
            option.dataset.id = item.id;
            option.innerHTML = `
            <strong>${item.title}</strong>
            <div class="search-result-type">${item.type}</div>
            <div class="search-result-content">${item.content.substring(0, 120)}...</div>
        `;
            // Optionally, add a click handler
            option.addEventListener("click", () => {
                // Handle selection (e.g., navigate, fill input, etc.)
                document.querySelector("#header-search-bar").value = item.title;
                searchResultsContainer.innerHTML = "";
            });

            searchResultsContainer.appendChild(option);
        });
    }
});