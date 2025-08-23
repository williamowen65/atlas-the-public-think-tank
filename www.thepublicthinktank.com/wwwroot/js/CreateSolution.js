document.addEventListener("DOMContentLoaded", () => {

    /*
    This JS file is used on both CreateIssuePage and CreateSolutionPage
    The logic below is meant for the CreateSolutionPage
    where the .create-solution-card is loaded by default
    */
    const createSolutionForm = document.querySelector(".create-solution-card")
    if (createSolutionForm) {
        initCreateSolutionFormSubmissionListeners(createSolutionForm)
    }
})



function initCreateSolutionFormSubmissionListeners(form) {

    // Get form and button elements
    // Note: these buttons have idenifiers as classes, not ids b/c there may be multiple solution
    const draftSolutionButton = document.querySelector(".create-solution-draft");
    const publishSolutionButton = document.querySelector(".publish-solution");

    // Prevent default form submission and handle both buttons
    form.addEventListener("submit", (e) => {
        e.preventDefault(); // Prevent standard form submission
    });

    // Handler for Draft button
    draftSolutionButton.addEventListener("click", (e) => {
        submitSolutionCreateForm(e, ContentStatus.Draft);
    });

    // Handler for Publish button
    publishSolutionButton.addEventListener("click", (e) => {
        submitSolutionCreateForm(e, ContentStatus.Published);
    });

    function submitSolutionCreateForm(e, contentStatus) {


        const form = e.target.closest('form')
        const errorContainers = Array.from(form.querySelectorAll(".text-danger"))
        errorContainers.forEach(container => {
            container.innerText = ""
        })

        // Get form data
        const formData = new FormData(form);

        // Add the content status to the form data
        formData.append("contentStatus", contentStatus);

        // Manually add the ParentIssueID from the disabled select element
        const parentIssueSelect = form.querySelector('#ParentIssueID');
        if (parentIssueSelect && parentIssueSelect.disabled && parentIssueSelect.value) {
            formData.append("ParentIssueID", parentIssueSelect.value);
        }



        // Get anti-forgery token
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;


        // Send POST request via fetch
        fetch("/create-solution", {
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
                    console.log("create solution", { data })
                    // Handle successful response
                    // Create a temporary container to parse the HTML string into DOM nodes
                    const tempContainer = document.createElement('div');
                    tempContainer.innerHTML = data.content;

                    // Create a document fragment to hold all content
                    const newContent = document.createDocumentFragment();
                    // Move all children from the temporary container to the fragment
                    while (tempContainer.firstChild) {
                        newContent.appendChild(tempContainer.firstChild);
                    }

                    // Replace the form with all the new content
                    form.parentNode.replaceChild(newContent, form);
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
