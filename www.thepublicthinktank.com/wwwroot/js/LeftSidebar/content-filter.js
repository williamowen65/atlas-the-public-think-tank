function initContentTypeListener() {
    const contentTypeFilters = document.querySelector("#content-filter").elements["contentTypeFilter"];
    const debouncedFilterTrigger = getDebouncedFilterTrigger();

    if (contentTypeFilters) {
        // Check if it's a collection (radio group) or a single element
        if (contentTypeFilters.length) {
            // It's a collection, attach listener to each radio button
            for (let i = 0; i < contentTypeFilters.length; i++) {
                contentTypeFilters[i].addEventListener('change', function () {
                    debouncedFilterTrigger();
                });
            }
        } else {
            // It's a single element
            contentTypeFilters.addEventListener('change', function () {
                debouncedFilterTrigger();
            });
        }
    } else {
        console.log("Could not find contentTypeFilter elements");
    }
}


// A pair of inputs for min and max must not over lap
// Initial min is 0
// Inital max is "no max"
// If min is 100, max must be no lower than 100
function initializeMinMaxNumberInput() {
    const debouncedFilterTrigger = getDebouncedFilterTrigger()

    const totalVotesControl = document.querySelector("#total-votes-filter");
    const minInput = totalVotesControl.querySelector("#fromInput-total-votes-filter");
    const maxInput = totalVotesControl.querySelector("#toInput-total-votes-filter");

    if (!totalVotesControl || !minInput || !maxInput) {
        console.log("Could not find total votes filter inputs");
        return;
    }


    // Function to validate and adjust min/max values
    function validateInputs() {
        const minValue = parseInt(minInput.value) || 0;
        const maxValue = maxInput.value ? (parseInt(maxInput.value) || 0) : null;

        // Ensure min value is not negative
        if (minValue < 0) {
            minInput.value = 0;
        }

        // Ensure max value is either null ("no max") or >= min value
        if (maxValue !== null) {
            if (maxValue < minValue) {
                maxInput.value = minValue;
            }
        }
    }

    // Add event listeners
    minInput.addEventListener('input', function () {
        validateInputs();
        debouncedFilterTrigger()
    });

    minInput.addEventListener('change', function () {
        validateInputs();
        debouncedFilterTrigger()
    });

    maxInput.addEventListener('input', function () {
        validateInputs();
        debouncedFilterTrigger()
    });

    maxInput.addEventListener('change', function () {
        validateInputs();
        debouncedFilterTrigger()
    });

    // Run initial validation
    validateInputs();
}


/**
 * MCDatepicker docs: https://mcdatepicker.netlify.app/
 * https://mcdatepicker.netlify.app/docs/configuration
 */
function initializeDatePickers() {

    const debouncedFilterTrigger = getDebouncedFilterTrigger()

    const today = new Date()

    const configBase = {
        bodyType: 'modal',
        closeOnBlur: true,
        bodyType: 'inline',
        maxDate: today
    };

    let fromDatePicker = createFromDatePicker();
    let toDatePicker = createToDatePicker();

    function createFromDatePicker(maxDate = null) {
        const config = maxDate ? { ...configBase, maxDate } : configBase;
        const datepickerConfig = {
            el: '#datepicker-from',
            ...config
        }
        const picker = MCDatepicker.create(datepickerConfig);   
        addFromDateListeners(picker);
        const element = document.querySelector("#datepicker-from")
        // Adding instance accessor to the element
        element.datepicker = picker;
        element.datepickerConfig = datepickerConfig
        return picker;
    }

    function createToDatePicker(minDate = null) {
        const config = minDate ? { ...configBase, minDate } : configBase;
        const datepickerConfig = {
            el: '#datepicker-to',
            ...config
        }
        const picker = MCDatepicker.create(datepickerConfig);
        addToDateListeners(picker);
        const element = document.querySelector("#datepicker-to")
        // Adding instance accessor to the element
        element.datepicker = picker;
        element.datepickerConfig = datepickerConfig
        return picker;
    }

    function addFromDateListeners(picker) {
        picker.onSelect((date) => {
            const nextDay = new Date(date);
            nextDay.setDate(nextDay.getDate() + 1);
            const currentToDate = toDatePicker.getFullDate()
            toDatePicker.destroy();
            toDatePicker = createToDatePicker(nextDay);
            if (currentToDate) {
                toDatePicker.setFullDate(currentToDate)
            }
            debouncedFilterTrigger()
        });

        picker.onClear(() => {
            toDatePicker.destroy();
            toDatePicker = createToDatePicker();
            debouncedFilterTrigger()
        });
    }

    function addToDateListeners(picker) {
        picker.onSelect((date) => {
            const previousDay = new Date(date);
            previousDay.setDate(previousDay.getDate() - 1);
            const currentFromDate = fromDatePicker.getFullDate()
            fromDatePicker.destroy();
            fromDatePicker = createFromDatePicker(previousDay);
            if (currentFromDate) {
                fromDatePicker.setFullDate(currentFromDate)
            }
            debouncedFilterTrigger()
        });

        picker.onClear(() => {
            fromDatePicker.destroy();
            fromDatePicker = createFromDatePicker();
            debouncedFilterTrigger()
        });
    }
}


function loadContentFilterFromCookie() {
    try {
        // Get stored filter values from cookie
        const storedFilter = getCookie('contentFilter');
        if (!storedFilter) {
            console.log('No stored filter found');
            return;
        }

        const filterValues = JSON.parse(storedFilter);
        console.log('Loading stored filter values:', filterValues);

        if (filterValues.contentType) {
            // set value of contentTypeFilter radio
            const contentTypeFilters = document.querySelector("#content-filter").elements["contentTypeFilter"];

            // Check if it's a collection (radio group) or a single element
            if (contentTypeFilters && contentTypeFilters.length) {
                for (let i = 0; i < contentTypeFilters.length; i++) {
                    if (contentTypeFilters[i].value === filterValues.contentType) {
                        contentTypeFilters[i].checked = true;
                        break;
                    }
                }
            } 
        }

        // Apply average vote range values
        if (filterValues.avgVoteRange) {
            const minAvgVoteInput = document.querySelector("#fromInput-average-score-filter");
            const maxAvgVoteInput = document.querySelector("#toInput-average-score-filter");
            const minAvgVoteSlider = document.querySelector("#fromSlider-average-score-filter");
            const maxAvgVoteSlider = document.querySelector("#toSlider-average-score-filter");

            if (minAvgVoteInput && typeof filterValues.avgVoteRange.min === 'number') {
                minAvgVoteInput.value = filterValues.avgVoteRange.min;
                if (minAvgVoteSlider) minAvgVoteSlider.value = filterValues.avgVoteRange.min;
            }

            if (maxAvgVoteInput && typeof filterValues.avgVoteRange.max === 'number') {
                maxAvgVoteInput.value = filterValues.avgVoteRange.max;
                if (maxAvgVoteSlider) maxAvgVoteSlider.value = filterValues.avgVoteRange.max;
            }
        }

        console.log("ON load filter values", {filterValues})

        // Apply total vote count values
        if (filterValues.totalVoteCount) {
            const minTotalVoteInput = document.querySelector("#fromInput-total-votes-filter");
            const maxTotalVoteInput = document.querySelector("#toInput-total-votes-filter");

            if (minTotalVoteInput && typeof filterValues.totalVoteCount.min === 'number') {
                minTotalVoteInput.value = filterValues.totalVoteCount.min;
            }

            if (maxTotalVoteInput) {
                if (typeof filterValues.totalVoteCount.max === 'number' && filterValues.totalVoteCount.max > 0) {
                    maxTotalVoteInput.value = filterValues.totalVoteCount.max;
                } else {
                    maxTotalVoteInput.value = ''; // No max
                }
            }
        }


        
        // Apply date range values
        if (filterValues.dateRange) {
            const fromDateInput = document.querySelector("#datepicker-from");
            const toDateInput = document.querySelector("#datepicker-to");

            // Useful to know: This does not cause the onSelect event to fire

            if (fromDateInput && filterValues.dateRange.from) {
                const fromDate = new Date(filterValues.dateRange.from)
                fromDateInput.datepicker.setFullDate(fromDate)
                // set the min date on the to date input
                const minDate = new Date(fromDate)
                minDate.setDate(fromDate.getDate() + 1)
                toDateInput.datepicker.prevLimitDate = minDate
            }

            if (toDateInput && filterValues.dateRange.to) {
                const toDate = new Date(filterValues.dateRange.to)
                toDateInput.datepicker.setFullDate(toDate)
                // Set the max date on from datepicker
                const maxDate = new Date(toDate)
                maxDate.setDate(toDate.getDate() - 1)
                fromDateInput.datepicker.nextLimitDate = maxDate
            }
        }
    }
    catch (error) {
        console.error('Error loading filter from cookie:', error);
    }
}



function getDebouncedFilterTrigger() {
    return debounce(filterTrigger, 800)
}


// Ajax call for filtered data
// The filter is persisted via cookies
function filterTrigger() {
    // Get all filter values
    const filterValues = {
        contentType : getContentType(),
        avgVoteRange: {
            min: getMinAvgVote(),
            max: getMaxAvgVote()
        },
        totalVoteCount: {
            min: getMinTotalVote(),
            max: getMaxTotalVote()
        },
        dateRange: {
            from: getFromDate(),
            to: getToDate()
        },
        tags: getTags()
    }

    console.log("Filter trigger", { filterValues })

    // Persist filter values in cookie
    try {
        const filterJson = JSON.stringify(filterValues);
        setCookie('contentFilter', filterJson);
    } catch (error) {
        console.error('Error saving filter to cookie:', error);
    }


    try {
        fetchFilteredContent()
        .then(repopulatePageContents)
    }
    catch (error) {
        console.error('Error fetching filtered content or setting content in DOM:', error);
    }

    function getContentType() {
        const radioInputs = document.querySelector("#content-filter").elements["contentTypeFilter"]
        const existingFilterCookie  = getCookie("filterCookie")
        if (radioInputs) {
            return radioInputs.value
        } else if (existingFilterCookie) {
            return existingCookieContentType;
        } else {
            return 'both'
        }
        
    }

    // averageVote Range
    // Min and Max
    // returns Number
    function getMinAvgVote() {
        const input = document.querySelector("#fromInput-average-score-filter")
        return Number(input.value);
    }
    function getMaxAvgVote() {
        const input = document.querySelector("#toInput-average-score-filter")
        return Number(input.value);
    }

    // Total Vote count
    // min and max
    function getMinTotalVote() {
        const input = document.querySelector("#fromInput-total-votes-filter")
        return Number(input.value);
    }
    function getMaxTotalVote() {
        const input = document.querySelector("#toInput-total-votes-filter")
        return input.value ? Number(input.value) : null;
    }

    // Date Range
    // min and max
    function getFromDate() {
        const input = document.querySelector("#datepicker-from")
        return input.value ? new Date(input.value) : null;
    }
    function getToDate() {
        const input = document.querySelector("#datepicker-to")
        return input.value ? new Date(input.value) : null;
    }

    // Tags
    function getTags() {
        return ["tag1"]
    }
}

/**
 * Fetch content when the filter is updated
 * Filter Params are sent via Cookies
 * 
 * Pipes return value into repopulatePageContents
 */
async function fetchFilteredContent() {

    // Send a new get request. home/getPaginatedContent
    return fetch("/home/getPaginatedContent?currentPage=1")
        .then(res => res.json())
        .then(res => {
            return res;
        });
}

function repopulatePageContents(updatedFilterContent) {
    const contentFeed = document.querySelector("#main-content");

    if (!contentFeed) {
        console.error("Could not find main content element with ID 'main-content'");
        return;
    }

    if (!updatedFilterContent) {
        console.error("Received empty content from server");
        return;
    }

    // Empty the content feed
    contentFeed.innerHTML = '';

    // Populate it with new content
    contentFeed.innerHTML = updatedFilterContent.html;


    const fetchPaginatedContentButton = document.querySelector("#fetchPaginatedContent")

    // Removes a possible disabled setting on this button
    fetchPaginatedContentButton.removeAttribute("disabled")
    fetchPaginatedContentButton.setAttribute("data-url", "/home/getPaginatedContent?currentPage=2")

    const fetchPaginatedContentButtonText = fetchPaginatedContentButton.querySelector(".button-text")

    fetchPaginatedContentButtonText.innerText = "Fetch more posts";

    const nextCurrentCount = updatedFilterContent.pagination.pageSize * (updatedFilterContent.pagination.currentPage);
    if (nextCurrentCount >= updatedFilterContent.pagination.totalCount) {
        const paginationContentType = fetchPaginatedContentButton.getAttribute("data-content-type")
        // Disable the button
        fetchPaginatedContentButton.disabled = true;
        fetchPaginatedContentButtonText.innerText = `No more ${paginationContentType}`;
    }

    //// Update the Total Count on the pagination button
    //// The "total count" should reflect
    // Format == "(NumberOfVisiblePost/TotalNumberOfPost)"
    const fullPaginatedCountEl = document.querySelector(".fullPaginatedCount")
    fullPaginatedCountEl.innerText = `(${Math.min(updatedFilterContent.pagination.pageSize, updatedFilterContent.pagination.totalCount)}/${updatedFilterContent.pagination.totalCount})`;

    // Scroll to top
    //window.scrollTo({
    //    top: 0,
    //    behavior: 'smooth'
    //})

}
