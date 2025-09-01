
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
});

function initListenerOnEditButton(button) {
    button.addEventListener("click", fetchRelatedEditForm)
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