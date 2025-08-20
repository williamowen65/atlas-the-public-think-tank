document.addEventListener("DOMContentLoaded", () => {
    initListenersForCreateIssue()
})


function initListenersForCreateIssue() {

    // Get form and button elements
    const form = document.querySelector(".border-1.border.card");
    const draftIssueButton = document.querySelector("#create-issue-draft");
    const publishIssueButton = document.querySelector("#publish-issue");

    // Prevent default form submission and handle both buttons
    form.addEventListener("submit", (e) => {
        e.preventDefault(); // Prevent standard form submission
    });

    // Handler for Draft button
    draftIssueButton.addEventListener("click", (e) => {
        submitForm(e, ContentStatus.Draft);
    });

    // Handler for Publish button
    publishIssueButton.addEventListener("click", (e) => {
        submitForm(e, ContentStatus.Published);
    });

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


