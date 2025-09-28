
/**
 * 
 * This file is loaded with the layout.cshtml
 * 
 * It is for editing issues and solutions
 * 
 * These content items can be edited from any page of the website
 * 
 */

// This can be added right away b/c no existing edit forms should be on the page
if (typeof documentObserver == 'object') {
    documentObserver.registerEvent(initEditContentButtonObserver)
    //documentObserver.registerEvent(initEditContentObserver)
} else {
    throw error("documentObserver not defined")
}


/**
 * Detects DOM elements related to editing content and assigns listeners
 * This observer detects the button and assigned a listener.
 * @param {DOMElement} node - is passed to method via global MutationObserver
 */

function initEditContentButtonObserver(node) {

    if (node.nodeType === 1) { // Element node
        // The buttons for submitting edit forms are always nested in the form
        const editButtons = node.querySelectorAll('.edit-issue-button, .edit-solution-button');
        editButtons.forEach(button => {
            // These buttons render ask the server for the edit form
            initListenerOnEditButton(button)
        });
        const cancelEditButtons = node.querySelectorAll('.cancel-edit-issue-button, .cancel-edit-solution-button');
        cancelEditButtons.forEach(button => {
            // These buttons render ask the server for the edit form
            initListenerOnCloseEditButton(button)
        });
    }
}

/**
 * Detects DOM elements related to fetch edit forms that are present onload (before the mutation observer can detect them)
 * This would be in the first 3 content items of the feed if any of them are written by active user.
 */
document.addEventListener("DOMContentLoaded", () => {
    Array.from(document.querySelectorAll(".edit-issue-button, .edit-solution-button")).forEach((button) => {
        initListenerOnEditButton(button)
    });
    Array.from(document.querySelectorAll(".cancel-edit-issue-button, .cancel-edit-solution-button")).forEach((button) => {
        initListenerOnCloseEditButton(button)
    });
});

function initListenerOnEditButton(button) {
    button.addEventListener("click", fetchRelatedEditForm)
}
function initListenerOnCloseEditButton(button) {
    button.addEventListener("click", closeEditForm)
}

function closeEditForm(e) {

    const isAuthorAlertCancelButton = Boolean(e.target.closest(".author-content-alert"))
    const isEditFormCancelButton = Boolean(e.target.closest(".issue-editor, .solution-editor"))
    let contentType;
    if (isAuthorAlertCancelButton) contentType = e.target.closest(".cancel-edit-issue-button, .cancel-edit-solution-button").getAttribute("data-content-type")
    if (isEditFormCancelButton) contentType = e.target.closest(".issue-editor, .solution-editor").getAttribute("data-content-type")
    let contentId;
    if (isAuthorAlertCancelButton) contentId = e.target.closest(".cancel-edit-issue-button, .cancel-edit-solution-button").getAttribute("data-content-id")
    if (isEditFormCancelButton) contentId = e.target.closest(".issue-editor, .solution-editor")
        .querySelector(`input[name=${contentType[0].toUpperCase() + contentType.slice(1)}ID]`)
        .getAttribute("value")

    const editor = document.querySelector(`.issue-editor[data-content-id='${contentId}'], .solution-editor[data-content-id='${contentId}']`)


    const url = `/cancel-edit-${contentType}/${contentId}`
     
    console.log("CLose form.... fetch the issue and put it back in its place",
        {
            isAuthorAlertCancelButton,
            isEditFormCancelButton,
            contentType,
            url
        })

    fetch(url)
        .then(response => {
            if (!response.ok) {
                throw new Error(`Server returned ${response.status}: ${response.statusText}`);
            }
            return response.json();
        })
        .then(html => {

            // Toggle class on related contnet author tag to display a cancel button 
            const authorContentAlert = editor.parentNode.querySelector(`.author-content-alert[data-id='${contentId}']`);
            authorContentAlert.classList.remove("edit-mode")

            console.log("cancel edit", {html})

            const tempContainer = document.createElement('div');
            tempContainer.innerHTML = html.content;
            const contentCard = tempContainer.querySelector(".issue-card, .solution-card"); 
            editor.replaceWith(contentCard);

        })
        .catch(error => {
            console.error(`Error fetching ${contentType} editor:`, error);
        });

}

/**
 * Fetches an edit issue form or an edit solution form
 * and renders the form in place of the issue-card or solution-card
 * @param {any} e - Event on button click
 */
function fetchRelatedEditForm(e) {
    const button = e.target.closest("button")
    const contentType = button.getAttribute("data-content-type")
    const contentId = button.getAttribute("data-content-id")
    const contentCard = document.querySelector(`*[id="${contentId}"]`)
    let url;
    if (contentType == 'issue') url = `/edit-issue?issueId=${contentId}`
    if (contentType == 'solution') url = `/edit-solution?solutionId=${contentId}`
    // Fetch issue editor from server


    fetch(url)
        .then(response => {
            if (!response.ok) {
                throw new Error(`Server returned ${response.status}: ${response.statusText}`);
            }
            return response.json();
        })
        .then(html => {

            // Toggle class on related contnet author tag to display a cancel button 
            const authorContentAlert = contentCard.parentNode.querySelector(`.author-content-alert[data-id='${contentId}']`);
            authorContentAlert.classList.add("edit-mode")

            const tempContainer = document.createElement('div');
            tempContainer.innerHTML = html.content;
            const editIssueTemplate = tempContainer.firstElementChild;
            contentCard.replaceWith(editIssueTemplate);

            // Initialize any new editors that were just added
            initListenersOnFormElements(editIssueTemplate);

        })
        .catch(error => {
            console.error(`Error fetching ${contentType} editor:`, error);
        });
}


/**
 * This sets the listeners on the newly created form
 * @param {DOMElement} form
 */
function initListenersOnFormElements(form) {

    const formFields = Array.from(form.querySelectorAll('.form-input'))
    formFields.forEach(field => {
        setupFormField(field);
    })

    // Add submission event for edit form
    initFormSubmissionListeners(form)
}

/**
 * Handle setting the listeners on the submit buttons and POST logic
 * @param {DOMElement} form
 */
function initFormSubmissionListeners(form) {
    form.addEventListener("submit", (e) => {
        e.preventDefault(); // Prevent standard form submission
    });

    const contentType = form.getAttribute("data-content-type")
    if (!contentType) {
        throw new Error("Form missing 'data-content-type'")
    }

    // The create-or-edit forms are dynamic and not all these buttons exist at 
    // the same time

    let draftButton = null;
    let publishButton = null;
    let updateButton = null;

    if (contentType == 'issue') {
        draftButton = form.querySelector(".update-issue-draft");
        publishButton = form.querySelector(".publish-issue");
        updateButton = form.querySelector(".update-issue");
    }
    if (contentType == 'solution') {
        draftButton = form.querySelector(".update-solution-draft");
        publishButton = form.querySelector(".publish-solution");
        updateButton = form.querySelector(".update-solution");
    }

    // ContentStatus is defined in ClientSideEnums.js

    if (draftButton) {
        draftButton.addEventListener("click", (e) => submitForm(e, ContentStatus.Draft))
    }
    if (publishButton) {
        publishButton.addEventListener("click", (e) => submitForm(e, ContentStatus.Published))
    }
    if (updateButton) {
        updateButton.addEventListener("click", (e) => submitForm(e, ContentStatus.Published))
    }


    function submitForm(e, contentStatus) {
        // Clear all errors before submitting
        const errorContainers = Array.from(form.querySelectorAll(".text-danger"))
        errorContainers.forEach(container => {
            container.innerText = ""
        })

        // Get form data
        const formData = new FormData(form);

        // Add the content status to the form data
        formData.append("contentStatus", contentStatus);

        // Add potentially hidden/disabled fields
        // An issue may have an hidden IssueID, or possibly disabled parentIssueID, or possibly disabled parentSolutionID
        // A Solution will have a hidden IssueID, and possibly diasabled ParentIssueID
        const issueIdField = form.querySelector("#IssueID")
        const solutionIdField = form.querySelector("#SolutionID")
        const parentIssueIdField = form.querySelector("#ParentIssueID")
        const parentSolutionIdField = form.querySelector("#ParentSolutionID")

        // Add these fields to formData if they exist, even if hidden or disabled
        if (issueIdField && issueIdField.value) {
            formData.append("IssueID", issueIdField.value);
        }
        if (solutionIdField && solutionIdField.value) {
            formData.append("SolutionID", solutionIdField.value);
        }
        if (parentIssueIdField && parentIssueIdField.value) {
            formData.append("ParentIssueID", parentIssueIdField.value);
        }
        if (parentSolutionIdField && parentSolutionIdField.value) {
            formData.append("ParentSolutionID", parentSolutionIdField.value);
        }




        // Get anti-forgery token
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

        const url = form.getAttribute("data-form-url")
        if (!url) {
            throw new Error("Form missing 'data-form-url'")
        }

        // form data should have the issueID or solutionID depending on contentType


        fetch(url, {
            method: "POST",
            headers: {
                "RequestVerificationToken": token
            },
            body: formData
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error("Network response was not ok");
                }
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    // Handle successful response
                    console.log("edit " + contentType, { data })
                    // replace node with data.content (as a node)
                    // Parse the HTML content into DOM nodes
                    const tempContainer = document.createElement('div');

                    tempContainer.innerHTML = data.content;



                    //// remove the author card alert tab
                    const contentID = tempContainer.querySelector(".card.solution-card, .card.issue-card").getAttribute("id")
                    const authorContentTab = document.querySelector(`.author-content-alert[data-id='${contentID}']`)
                    authorContentTab.remove()
                    // NOTE: A new author content tab will be added with the card

                    // replace form with updated card - use the actual DOM nodes
                    // using the children collection because there might be multiple elements
                    const newContent = document.createDocumentFragment();
                    while (tempContainer.firstChild) {
                        newContent.appendChild(tempContainer.firstChild);
                    }
                    form.replaceWith(newContent);
                } else {
                    // Handle validation errors or other failures
                    // alert("Failed to create issue. Please check your form inputs and try again.");
                    console.error("Error details:", data);
                    data.errors.forEach(error => {
                        const fieldElement = form.querySelector(`*[name=${error[0]}]`).closest(".form-element")
                        const fieldErrorEl = fieldElement.querySelector(".text-danger")
                        fieldErrorEl.innerText = error[1]


                    })

                }
            })
            .catch(error => {
                console.error("Error submitting form:", error);
                alert("An error occurred while submitting the form. Please try again.");
            });

    }

}


/* CREATE OR EDIT METHODS */


const parentContentAjaxConfigBase = (node) => ({
    dataType: "json",
    method: "POST",
    contentType: 'application/json',
    transport: function (params, success, failure) {
        // 🔑 Cancel if there's no user input
        const parsedData = JSON.parse(params.data)
        const select2Instance = $(node).data('select2');
        const dropdown = select2Instance.dropdown.$dropdown[0]
        const hasOptionsAlready = dropdown.querySelector(".select2-results__option--selectable")

        if (parsedData.searchString.trim() === '' && !hasOptionsAlready) {

            // This prevent the "no results" from initially appearing
            dropdown.querySelector(".loading-results").innerText = "Start typing to search"

            return null;  // ← no network call
        }

        // otherwise perform the default Ajax call
        return $.ajax(params).then(success).fail(failure);
    },
    data: function (params) {
        // Making sure a default empty string is passed.
        // Function to build the data payload for the POST request
        return JSON.stringify({ searchString: params.term || "" });
    },
    processResults: function (data) {
        return {
            results: data.map(e => ({
                id: e.id,
                text: e.title
            }))
        };
    }
})

// Global method for select2 ajax
window.getParentIssueSelect2Config = function (node) {
    return {
        language: {
            noResults: function () {
                setTimeout(() => {
                    const select2container = $(node).data("select2").$dropdown[0]
                    const messageNode = select2container.querySelector(".select2-results__message")
                    messageNode.innerHTML = "<div>No matching issues found. <a href='/create-issue'>Create new?</a></div>"
                }, 1)
                return null;   // your custom
            }
        },
        allowClear: true,
        ajax: {
            ...parentContentAjaxConfigBase(node),
            url: "/search?showRanks=true&contentType=issue"
        }
    }
}

// Global method for select2 ajax
window.getParentSolutionSelect2Config = function (node) {
    return {
        language: {
            noResults: function () {
                setTimeout(() => {
                    const select2container = $(node).data("select2").$dropdown[0]
                    const messageNode = select2container.querySelector(".select2-results__message")
                    messageNode.innerHTML = "<div>No matching solution found. <a href='/create-solution'>Create new?</a></div>"
                }, 1)
                return null;   // your custom
            }
        },
        allowClear: true,
        ajax: {
            ...parentContentAjaxConfigBase(node),
            url: "/search?showRanks=true&contentType=solution"
        }
    }
}

window.setParentIssueSelect2Listener = function (node) {
    const select2Instance = $(node).data('select2');
    // updating the classname of the clear button to not use the broken select 2 clear listener (delegated listener)
    // Styles were copied to the select2 css to keep the layout
    const clearButton = select2Instance.$container[0].querySelector(".select2-selection__clear")
    clearButton.classList.remove('select2-selection__clear')
    clearButton.classList.add('select2-selection__clear-custom')
    clearButton.classList.add('d-none')


    const solutionSelectContainer = document.querySelector(".parentSolutionSelectContainer")
    select2Instance.$element.on("select2:select", (e) => {
        if (solutionSelectContainer) {
            solutionSelectContainer.classList.add("d-none")
        }
        clearButton.classList.remove("d-none")
    })


    clearButton.addEventListener("click", (e) => {
        e.stopPropagation();
        solutionSelectContainer.classList.remove("d-none")
        $(node).val(null).trigger("change")
        clearButton.classList.add("d-none")
        $(node).select2('close');
    })
}

window.setParentSolutionSelect2Listener = function (node) {
    const select2Instance = $(node).data('select2');
    // updating the classname of the clear button to not use the broken select 2 clear listener (delegated listener)
    // Styles were copied to the select2 css to keep the layout
    const clearButton = select2Instance.$container[0].querySelector(".select2-selection__clear")
    clearButton.classList.remove('select2-selection__clear')
    clearButton.classList.add('select2-selection__clear-custom')
    clearButton.classList.add('d-none')


    const issueSelectContainer = document.querySelector(".parentIssueSelectContainer")
    select2Instance.$element.on("select2:select", (e) => {
        issueSelectContainer.classList.add("d-none")
        clearButton.classList.remove("d-none")
    })


    clearButton.addEventListener("click", (e) => {
        e.stopPropagation();
        issueSelectContainer.classList.remove("d-none")
        $(node).val(null).trigger("change")
        clearButton.classList.add("d-none")
        $(node).select2('close');
    })
}