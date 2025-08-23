document.addEventListener("DOMContentLoaded", () => {
    initListenersForCreateIssue()
    initListenersForCreateSolution_OnCreateIssuePage()
})


/**
 * Sets listeners on the initial dom elements
 * 
 * These listeners will be removed after saving the issue content
 */
function initListenersForCreateIssue() {

    // Get form and button elements
    const form = document.querySelector("#issue .card");
    const draftIssueButton = document.querySelector(".create-issue-draft");
    const publishIssueButton = document.querySelector(".publish-issue");

    // Prevent default form submission and handle both buttons
    form.addEventListener("submit", (e) => {
        e.preventDefault(); // Prevent standard form submission
    });

    // Handler for Draft button
    draftIssueButton.addEventListener("click", (e) => {
        submitIssueCreateForm(e, ContentStatus.Draft);
    });

    // Handler for Publish button
    publishIssueButton.addEventListener("click", (e) => {
        submitIssueCreateForm(e, ContentStatus.Published);
    });

    // Function to handle form submission via fetch
    function submitIssueCreateForm(e, contentStatus) {

        const contentItemEl = e.target.closest('.card')
        const errorContainers = Array.from(contentItemEl.querySelectorAll(".text-danger"))
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

        // Manually add the ParentIssueID from the disabled select element
        const parentSolutionSelect = form.querySelector('#ParentSolutionID');
        if (parentSolutionSelect && parentSolutionSelect.disabled && parentSolutionSelect.value) {
            formData.append("ParentSolutionID", parentSolutionSelect.value);
        }

        // Get anti-forgery token
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

        // Send POST request via fetch
        fetch("/create-issue", {
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
                    console.log("create issue", { data })
                    const issueContainerEl = document.querySelector("#issue")
                    issueContainerEl.innerHTML = data.content

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




function initListenersForCreateSolution_OnCreateIssuePage() {
    const addSolutionButton = document.querySelector("#add-solution")

    addSolutionButton.addEventListener("click", () => {

        // try to get AuthenticatorAssertionResponse newly created issueID
        const issueCard = document.querySelector(".issue-card")
        const issueAuthorAlert = document.querySelector(`#issue .author-content-alert`)
        // Using the presense of dom elements to make sure the issue has been properly created
        if (!issueCard && !issueAuthorAlert) {
            alert("You must create the issue as a draft or publish before creating a solution")
            return
        }
        const issueId = issueCard?.getAttribute("id") || issueAuthorAlert?.getAttribute("data-id")

        fetch(`/create-solution-form?issueId=${issueId}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error(`Server returned ${response.status}: ${response.statusText}`);
                }
                return response.json()
            })
            .then(html => {
                const targetContainer = document.querySelector(".issue-solutions")
                const tempContainer = document.createElement('div');
                // convert the text html to dom nodes
                tempContainer.innerHTML = html.content;
                const createSolutionTemplate = tempContainer.firstElementChild;
                targetContainer.appendChild(createSolutionTemplate)
                initListenersOnSolutionForm(createSolutionTemplate)

            })
            .catch(error => {
                console.error("Error fetching solution create form:", error)
            })
    })




    function initListenersOnSolutionForm(createSolutionForm) {

        const targetContainer = document.querySelector(".issue-solutions")

        // Find the solution count element and update it
        const solutionCount = targetContainer.querySelectorAll('.create-solution-card, .solution-card').length;
        const solutionCountElement = createSolutionForm.querySelector('.solution-count');
        if (solutionCountElement) {
            solutionCountElement.textContent = '#' + solutionCount;
        }


        //// Initialize form fields
        //// Find all form fields in the newly added solution
        const newFormInputs = targetContainer.querySelectorAll(`.card:nth-child(${solutionCount}) .form-field`);
        newFormInputs.forEach(field => {
            const textarea = field.querySelector('textarea');
            const maxLength = Number(field.querySelector(".char-counter").getAttribute("data-max-length"))
            setupFormField(textarea.id, maxLength, field.id);
        });


        // Add form submission listeners

        initCreateSolutionFormSubmissionListeners(createSolutionForm)

    }


}