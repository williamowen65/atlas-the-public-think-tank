/**
 * Controls the "from" (minimum) value when user types in the number input
 * Example: User types "50" in the Min input field
 * @param {HTMLElement} fromSlider - The left slider element
 * @param {HTMLElement} fromInput - The Min number input field  
 * @param {HTMLElement} toInput - The Max number input field
 * @param {HTMLElement} controlSlider - The slider to apply visual styling to
 */
function controlFromInput(fromSlider, fromInput, toInput, controlSlider) {
    // Get the current values from both input fields as numbers
    // Example: from = 50, to = 60
    const [from, to] = getParsed(fromInput, toInput);

    // Update the visual appearance of the slider track
    fillSlider(fromInput, toInput, '#C6C6C6', '#25daa5', controlSlider);

    // Prevent invalid range: if min value > max value, set min = max
    // Example: If user enters 50 but max is 30, set min to 30
    if (from > to) {
        fromSlider.value = to;    // Update slider position to match max
        fromInput.value = to;     // Update input field to show corrected value
    } else {
        // Valid range: just sync the slider with the input value
        fromSlider.value = from;
    }
}

/**
 * Controls the "to" (maximum) value when user types in the number input
 * Example: User types "80" in the Max input field
 * @param {HTMLElement} toSlider - The right slider element
 * @param {HTMLElement} fromInput - The Min number input field
 * @param {HTMLElement} toInput - The Max number input field
 * @param {HTMLElement} controlSlider - The slider to apply visual styling to
 */
function controlToInput(toSlider, fromInput, toInput, controlSlider) {
    // Get current values from both input fields as numbers
    // Example: from = 20, to = 80
    const [from, to] = getParsed(fromInput, toInput);

    // Update the visual track styling
    fillSlider(fromInput, toInput, '#C6C6C6', '#25daa5', controlSlider);

    // Handle z-index for slider accessibility (when sliders overlap)
    setToggleAccessible(toInput);

    // Ensure valid range: max value must be >= min value
    if (from <= to) {
        // Valid range: sync slider with input value
        toSlider.value = to;
        toInput.value = to;
    } else {
        // Invalid range: if max < min, set max = min
        // Example: If user enters 15 but min is 20, set max to 20
        toInput.value = from;
    }
}

/**
 * Controls the "from" (minimum) slider when user drags it
 * Example: User drags left slider to position 25
 * @param {HTMLElement} fromSlider - The left slider element
 * @param {HTMLElement} toSlider - The right slider element  
 * @param {HTMLElement} fromInput - The Min number input field
 */
function controlFromSlider(fromSlider, toSlider, fromInput) {
    // Get current values from both sliders as numbers
    // Example: from = 25, to = 80
    const [from, to] = getParsed(fromSlider, toSlider);

    // Update the visual styling of the slider track
    fillSlider(fromSlider, toSlider, '#C6C6C6', '#25daa5', toSlider);

    // Prevent overlap: if left slider goes past right slider, clamp it
    if (from > to) {
        fromSlider.value = to;      // Move left slider back to right slider position
        fromInput.value = to;       // Update input to reflect change
    } else {
        // Valid position: sync the number input with slider value
        fromInput.value = from;
    }
}

/**
 * Controls the "to" (maximum) slider when user drags it
 * Example: User drags right slider to position 75
 * @param {HTMLElement} fromSlider - The left slider element
 * @param {HTMLElement} toSlider - The right slider element
 * @param {HTMLElement} toInput - The Max number input field
 */
function controlToSlider(fromSlider, toSlider, toInput) {
    // Get current values from both sliders as numbers
    // Example: from = 20, to = 75
    const [from, to] = getParsed(fromSlider, toSlider);

    // Update the visual styling of the slider track
    fillSlider(fromSlider, toSlider, '#C6C6C6', '#25daa5', toSlider);

    // Handle z-index for slider accessibility (prevents visual glitches when sliders overlap)
    setToggleAccessible(toSlider);

    // Ensure valid range: max slider must be >= min slider
    if (from <= to) {
        // Valid range: sync the input field with slider value
        toSlider.value = to;
        toInput.value = to;
    } else {
        // Invalid range: if max slider < min slider, set max = min
        // Example: If user drags max slider to 15 but min is at 20, set max to 20
        toInput.value = from;
        toSlider.value = from;
    }
}

/**
 * Utility function to parse string values from form elements into numbers
 * Example: If inputs contain "5.55" and "9.65", returns [5.55, 9.65]
 * @param {HTMLElement} currentFrom - Element containing the "from" value
 * @param {HTMLElement} currentTo - Element containing the "to" value
 * @returns {number[]} Array with [fromValue, toValue] as floating point numbers
 */
function getParsed(currentFrom, currentTo) {
    const from = parseFloat(currentFrom.value);  // Convert string to floating point number
    const to = parseFloat(currentTo.value);      // Convert string to floating point number
    return [from, to];
}

/**
 * Creates a visual gradient effect on the slider track to show the selected range
 * Example: If range is 0-100 and selection is 25-75, it will show:
 * - Gray from 0-25
 * - Green from 25-75  
 * - Gray from 75-100
 * @param {HTMLElement} from - The "from" element (min value source)
 * @param {HTMLElement} to - The "to" element (max value source)
 * @param {string} sliderColor - Color for unselected parts (default: '#C6C6C6')
 * @param {string} rangeColor - Color for selected range (default: '#25daa5')
 * @param {HTMLElement} controlSlider - The slider element to apply styling to
 */
function fillSlider(from, to, sliderColor, rangeColor, controlSlider) {
    // Calculate the total range of the slider (max - min)
    // Example: If slider goes 0-100, rangeDistance = 100
    const rangeDistance = to.max - to.min;

    // Calculate position of "from" value as distance from minimum
    // Example: If from=25 and min=0, fromPosition = 25
    const fromPosition = from.value - to.min;

    // Calculate position of "to" value as distance from minimum  
    // Example: If to=75 and min=0, toPosition = 75
    const toPosition = to.value - to.min;

    // Create a CSS linear gradient with 5 color stops:
    // 1. Gray from 0% to fromPosition%
    // 2. Green from fromPosition% to toPosition%
    // 3. Gray from toPosition% to 100%
    controlSlider.style.background = `linear-gradient(
      to right,
      ${sliderColor} 0%,
      ${sliderColor} ${(fromPosition) / (rangeDistance) * 100}%,
      ${rangeColor} ${((fromPosition) / (rangeDistance)) * 100}%,
      ${rangeColor} ${(toPosition) / (rangeDistance) * 100}%, 
      ${sliderColor} ${(toPosition) / (rangeDistance) * 100}%, 
      ${sliderColor} 100%)`;
}

/**
 * Manages z-index stacking when sliders overlap (accessibility helper)
 * When the max slider is at 0, it brings it to the front so users can interact with it
 * Example: If right slider is at position 0, make it more accessible to click
 * @param {HTMLElement} currentTarget - The element that triggered this function
 */
function setToggleAccessible(currentTarget) {

    const parentId = currentTarget.closest('.range_container').getAttribute('id')

    const toSlider = document.querySelector('#toSlider-' + parentId);
    if (Number(currentTarget.value) <= 0) {
        toSlider.style.zIndex = 2;  // Bring to front when at minimum value
    } else {
        toSlider.style.zIndex = 0;  // Reset z-index for normal layering
    }
}


function initializeDualSliderInput(fromSlider, toSlider, fromInput, toInput) {
    const debouncedFilterTrigger = getDebouncedFilterTrigger()

    // Set up initial visual styling for the slider track
    fillSlider(fromSlider, toSlider, '#C6C6C6', '#25daa5', toSlider);

    // Set up initial z-index accessibility for overlapping sliders
    setToggleAccessible(toSlider);

    // Event listeners: When user interacts with sliders or inputs, call appropriate control functions
    fromSlider.oninput = () => {
        controlFromSlider(fromSlider, toSlider, fromInput);
        debouncedFilterTrigger();
    };
    toSlider.oninput = () => {
        controlToSlider(fromSlider, toSlider, toInput);
        debouncedFilterTrigger();
    };
    fromInput.oninput = () => {
        controlFromInput(fromSlider, fromInput, toInput, toSlider);
        debouncedFilterTrigger();
    };
    toInput.oninput = () => {
        controlToInput(toSlider, fromInput, toInput, toSlider);
        debouncedFilterTrigger();
    };
}


// This method is called after loading sidebar content
function initializeDualSlider() {

    const averageScoreControl = document.querySelector("#average-score-filter")
    const rootFormControls = [averageScoreControl]

    rootFormControls.forEach(c => {
        const controlId = c.getAttribute('id')
        const fromSlider = c.querySelector('#fromSlider' + "-" + controlId);
        const toSlider = c.querySelector('#toSlider' + "-" + controlId);
        const fromInput = c.querySelector('#fromInput' + "-" + controlId);
        const toInput = c.querySelector('#toInput' + "-" + controlId);


        if (fromSlider && toSlider && fromInput && toInput) {
            console.log("Initializing dual slider inputs");
            initializeDualSliderInput(fromSlider, toSlider, fromInput, toInput);
        } else {
            console.log("Could not find all dual slider elements");
        }
    })
}
