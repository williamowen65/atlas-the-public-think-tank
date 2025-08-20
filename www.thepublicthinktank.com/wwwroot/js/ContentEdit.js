
function setEditIssueContent(container) {


    const issueId = container.getAttribute("data-content-id")
    const issueCard = document.querySelector(`*[id="${issueId}"]`)

    // Fetch issue editor from server
    // Fetch issue editor from server
    fetch(`/edit-issue?issueId=${issueId}`)
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
            issueCard.replaceWith(editIssueTemplate);
            // Initialize any new editors that were just added
            initEditorsInNode(editIssueTemplate);

        })
        .catch(error => {
            console.error("Error fetching issue editor:", error);
        });
}



function initEditorsInNode(node) {

    const formFields = Array.from(node.querySelectorAll('.form-field'))
    formFields.forEach(field => {

        const fieldName = field.querySelector("textarea").id
        // Listener on 
        const maxLength = 300; // Default max length
        setupFormField(fieldName, maxLength, field.id);
    })

    // Add submission event for edit issue form
    initListenersForEditIssue(node)

}


function initListenersForEditIssue(node) {

    // Get form and button elements
    const form = node
    const draftIssueButton = form.querySelector(".update-issue-draft");
    const publishIssueButton = form.querySelector(".publish-issue");
    const updateIssueButton = form.querySelector(".update-issue");

    // Prevent default form submission and handle both buttons
    form.addEventListener("submit", (e) => {
        e.preventDefault(); // Prevent standard form submission
    });

    if (draftIssueButton) {
        // Handler for Draft button
        draftIssueButton.addEventListener("click", (e) => {
            submitForm(e, ContentStatus.Draft);
        });
    }

    if (publishIssueButton) {
        // Handler for Publish button
        publishIssueButton.addEventListener("click", (e) => {
            submitForm(e, ContentStatus.Published);
        });
    }

    if (updateIssueButton) {
        updateIssueButton.addEventListener("click", (e) => {
            submitForm(e, ContentStatus.Published);
        });
    }

    // Function to handle form submission via fetch
    function submitForm(e, contentStatus) {

        const contentItemEl = e.target.closest('.card')
        const errorContainers = Array.from(contentItemEl.querySelectorAll(".text-danger"))
        errorContainers.forEach(container => {
            container.innerText = ""
        })

        // Get form data
        const formData = new FormData(form);


        // Add the content status to the form data
        formData.append("contentStatus", contentStatus);

        // Get anti-forgery token
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

        // Send POST request via fetch
        fetch("/edit-issue", {
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
                    console.log("edit issue", { data })
                    // replace node with data.content (as a node)
                    // Parse the HTML content into DOM nodes
                    const tempContainer = document.createElement('div');
                    tempContainer.innerHTML = data.content;

                    form.replaceWith(tempContainer.firstChild)

                    // Handle successful response
                    // window.location.href = `/issue/${data.content.issueID}`;
                } else {
                    // Handle validation errors or other failures
                    // alert("Failed to create issue. Please check your form inputs and try again.");
                    console.error("Error details:", data);
                    data.errors.forEach(error => {
                        const fieldElement = contentItemEl.querySelector(`*[name=${error[0]}]`).closest(".form-element")
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




if (typeof documentObserver == 'object') {
    documentObserver.registerEvent(initEditContentButtonObserver)
    //documentObserver.registerEvent(initEditContentObserver)
} else {
    throw error("documentObserver not defined")
}


function initEditContentButtonObserver(node) {

    /*
        When an issue is rendered and the author of the post is logged in
        This observer detects the button and assigned a listener.
    */


    // Check if the node itself is an edit button
    if (node.classList && (node.classList.contains('edit-issue-button') || node.classList.contains('edit-solution-button'))) {
        handleEditButtonAdded(node);
    }
    // Also check if the node contains edit buttons (most common case)
    else if (node.nodeType === 1) { // Element node
        const editButtons = node.querySelectorAll('.edit-issue-button, .edit-solution-button');
        editButtons.forEach(button => {
            handleEditButtonAdded(button);
        });
    }

}



// Extract the button handling logic to a separate function
function handleEditButtonAdded(node) {
    // Initialize card-specific JS here
    if (typeof setEditIssueContent === 'function') {
        try {
            node.addEventListener("click", () => setEditIssueContent(node));
        } catch (initError) {
            console.error("Error in setEditIssueContent:", initError);
        }
    } else {
        console.error("setEditIssueContent function is not defined");
    }
}


// This captures setting the event on authors issue that are loaded with the initial page
document.addEventListener("DOMContentLoaded", () => {
    Array.from(document.querySelectorAll(".edit-issue-button, .edit-solution-button")).forEach((node) => {
        node.addEventListener("click", () => setEditIssueContent(node))
    });
});

