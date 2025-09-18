document.addEventListener("DOMContentLoaded", () => {
    const searchBarId = "#header-search-bar";
    const searchBar = document.querySelector(searchBarId);
    const searchResultsContainer = document.querySelector("#header-search-bar-results");
    const backdrop = document.querySelector(".backdrop-search-results")
    const headerSearchBarResults = document.querySelector("#header-search-bar-results")

    backdrop.addEventListener("click", () => {
        searchBar.value = ""
        searchResultsContainer.innerHTML = ""
        backdrop.classList.add('d-none')
    })

    var sendSearchRequestThrottle = throttle(sendSearchRequest, 3000);
    searchBar.addEventListener("keyup", (e) => {
        const searchString = e.target.value;
        if (searchString.length == 0) {
            searchResultsContainer.innerHTML = ""
            backdrop.classList.add('d-none')
            return
        }
        sendSearchRequestThrottle(e);
    });

   

    searchBar.addEventListener("change", (e) => {
        const searchString = e.target.value;
        if (searchString.length === 0) {
            searchResultsContainer.innerHTML = "";
            backdrop.classList.add('d-none')
        }
    });



    function sendSearchRequest(e) {
        const searchString = e.target.value;

        // Double check that the search hasn't been cleared
        if (searchBar.value.trim() == "") {
            return
        }
      

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
            populateSearchResults(data.html)
        })
        .catch(error => {
            console.error("Error:", error);
        });
    }

    function populateSearchResults(htmlSearchResultCsHTML) {
     
        searchResultsContainer.innerHTML = htmlSearchResultCsHTML;
        
        backdrop.classList.remove('d-none')
        headerSearchBarResults.scroll(0,0)



      
    }
});