
document.addEventListener("DOMContentLoaded", function () {
    const formFields = Array.from(document.querySelectorAll(".form-input"))
    formFields.forEach(formField => {
        setupFormField(formField);
    })
    // Add mutation observer after initial load
    if (typeof documentObserver == 'object') {
        documentObserver.registerEvent(initFormFieldListener)
        //documentObserver.registerEvent(initEditContentObserver)
    } else {
        throw error("documentObserver not defined")
    }
});


/**
 * Form listener set via mutation observer
 * @param {any} node
 */
function initFormFieldListener(node) {

    if (node.nodeType === 1) { // Element node
        // The buttons for submitting edit forms are always nested in the form
        const formInputs = node.querySelectorAll('.form-input');
        formInputs.forEach(formInput => {
            // These buttons render ask the server for the edit form
            setupFormField(formInput)
        });
    }
}




function setupFormField(formField) {
    const propertyName = formField.querySelector("label").getAttribute("for")
    const maxLength = Number(formField.querySelector(".char-counter").getAttribute("data-max-length"))
    const fieldId = formField.getAttribute("id")

    // Target this specific form field instance using the unique ID
    const formFieldContainer = document.getElementById(fieldId);
    if (!formFieldContainer) return;

    const textarea = formFieldContainer.querySelector('textarea');
    const charCounter = formFieldContainer.querySelector('.char-counter');

    if (!textarea || !charCounter) return;

    // Add active class if textarea has value
    if (textarea.value) {
        formFieldContainer.classList.add("active");
    }

    // Focus and blur events
    textarea.addEventListener("focus", () => {
        formFieldContainer.classList.add("active");
    });

    textarea.addEventListener("blur", () => {
        if (!textarea.value) {
            formFieldContainer.classList.remove("active");
        }
    });

    // Character counter
    function updateCharCounter() {
        const currentLength = textarea.value.length;

        // Enforce maximum length
        if (currentLength > maxLength) {
            textarea.value = textarea.value.substring(0, maxLength);
        }

        // Update counter
        charCounter.textContent = `${Math.min(currentLength, maxLength)}/${maxLength}`;

    }

    textarea.addEventListener('input', updateCharCounter);
    updateCharCounter();

    // Auto-growing textarea
    const initialHeight = 54;
    textarea.style.height = initialHeight + 'px';

    function autoResize() {
        textarea.style.height = initialHeight + 'px';
        if (textarea.scrollHeight > initialHeight) {
            textarea.style.height = textarea.scrollHeight + 'px';
        }
    }

    textarea.addEventListener('input', autoResize);
    window.addEventListener('resize', autoResize);

    // Optionally disable Enter key
    if (propertyName.toLowerCase() === 'title') {
        textarea.addEventListener('keydown', function (event) {
            if (event.key === 'Enter' || event.keyCode === 13) {
                event.preventDefault();
            }
        });
    }
}

