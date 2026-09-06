document.addEventListener("DOMContentLoaded", () => {
    const searchBarId = "#header-search-bar";
    const searchBarInputElement = document.querySelector(searchBarId);
    const searchResultsContainer = document.querySelector("#header-search-bar-results");
    const backdrop = document.querySelector(".backdrop-search-results")
    const headerSearchBarResults = document.querySelector("#header-search-bar-results")
    const searchBarIconForSmallScreen = document.querySelector(".search-bar-icon-trigger");
    const headerEl = document.querySelector("header")
    const searchBarForm = document.querySelector('.search-bar-form')
    const searchBarGridElement = document.querySelector(".search-bar-grid-element")

    searchBarIconForSmallScreen.addEventListener("click", () => {
        // show search
        // hide website logo and header controls (Add a class to the header, "small-screen-search-bar")
        enableSmallScreenSearch()
        searchBarInputElement.focus();
    })

    function enableSmallScreenSearch() {
        headerEl.classList.add("small-screen-search-bar")
        searchBarIconForSmallScreen.classList.add("d-none")
        searchBarForm.classList.remove("d-none")
        searchBarForm.classList.remove("me-3")
        searchBarGridElement.style.gridColumn = "1/-1";
    }

    function revertSmallScreenSearch() {
        headerEl.classList.remove("small-screen-search-bar");
        searchBarIconForSmallScreen.classList.remove("d-none");
        searchBarForm.classList.add("d-none");
        searchBarForm.classList.add("me-3");
        searchBarGridElement.style.gridColumn = ""; // Reset to default
    }
    



    backdrop.addEventListener("click", () => {
        searchBarInputElement.value = ""
        searchResultsContainer.innerHTML = ""
        backdrop.classList.add('d-none')
        revertSmallScreenSearch();
    })

    var sendSearchRequestThrottle = throttle(sendSearchRequest, 0);
    searchBarInputElement.addEventListener("keyup", (e) => {
        const searchString = e.target.value;
        if (searchString.length == 0) {
            searchResultsContainer.innerHTML = ""
            backdrop.classList.add('d-none')
            revertSmallScreenSearch()
            return
        }
        sendSearchRequestThrottle(e);
    });


   

    searchBarInputElement.addEventListener("change", (e) => {
        const searchString = e.target.value;
        if (searchString.length === 0) {
            searchResultsContainer.innerHTML = "";
            backdrop.classList.add('d-none')
            revertSmallScreenSearch()
        }
    });

    searchBarInputElement.addEventListener("blur", (e) => {
        const searchString = e.target.value;
        if (searchString.length === 0) {
            searchResultsContainer.innerHTML = "";
            backdrop.classList.add('d-none')
            revertSmallScreenSearch()
        }
    })



    function sendSearchRequest(e) {
        const searchString = e.target.value;

        // Double check that the search hasn't been cleared
        if (searchBarInputElement.value.trim() == "") {
            return
        }
      

        fetch("/search", {
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