
/**
 * Detects DOM elements related to editing content that are present onload (before the mutation observer can detect them)
 */
document.addEventListener("DOMContentLoaded", () => {
    Array.from(document.querySelectorAll("form.issue-editor, form.solution-editor"))
        .forEach((form) => {
            initListenersForContentCreateForm(form)
        });

   
});




function initListenersForContentCreateForm(form) {
    // Prevent default form submission and handle both buttons
    form.addEventListener("submit", (e) => {
        e.preventDefault(); // Prevent standard form submission
    });
     const contentType = form.getAttribute("data-content-type")


    const draftButton = document.querySelector(`.create-${contentType}-draft`);
    const publishButton = document.querySelector(`.publish-${contentType}`);

    // Handler for Draft button
    draftButton.addEventListener("click", (e) => {
        submitContentCreateForm(e, ContentStatus.Draft);
    });

    // Handler for Publish button
    publishButton.addEventListener("click", (e) => {
        submitContentCreateForm(e, ContentStatus.Published);
    });

    function submitContentCreateForm(e, contentStatus) {

        // Clear all error messages
        const errorContainers = Array.from(form.querySelectorAll(".text-danger"))
        errorContainers.forEach(container => {
            container.innerText = ""
        })

       

        // Get form data
        const formData = new FormData(form);

        // Add the content status to the form data
        formData.append("contentStatus", contentStatus);

        // This JavaScript file is for creating issues or solutions
        // Issues could have ParentIssue, or ParentSolution, or Neither
        // Solution must have a ParentSolution

        // Manually add the ParentIssueID from the disabled select element
        const parentIssueSelect = form.querySelector('#ParentIssueID');
        if (parentIssueSelect && parentIssueSelect.disabled && parentIssueSelect.value) {
            formData.append("ParentIssueID", parentIssueSelect.value);
        }

        if (contentType == 'solution' && !parentIssueSelect.value) {
            // TODO: This can be passed to the server for the proper error handling.
            console.warn("When creating a new solution, a parent issue is required... This is a frontend warning before submission -- error handling provided by backend")
        }

        // Manually add the ParentIssueID from the disabled select element
        const parentSolutionSelect = form.querySelector('#ParentSolutionID');
        if (parentSolutionSelect && parentSolutionSelect.disabled && parentSolutionSelect.value) {
            formData.append("ParentSolutionID", parentSolutionSelect.value);
        }


   

        // Get anti-forgery token
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

        let url;
        if (contentType == 'issue') url = `/create-issue`
        if (contentType == 'solution') url = `/create-solution`


        /*
            NOTE
                if trying to log formData 
                    Object.fromEntries(formData.entries()) will not show a complete object
                    For some reason this does not print the List of items in the formData
                if trying to view the List elements of Scope, like Scope.Scales
                    formData.getAll("Scope.Scales")

                or 

                const obj = {};
                for (const [key, value] of formData.entries()) {
                    if (obj[key]) {
                        if (!Array.isArray(obj[key])) obj[key] = [obj[key]];
                        obj[key].push(value);
                    } else {
                        obj[key] = value;
                    }
                }
            
        */

        // Send POST request via fetch
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
                    console.log("create " + contentType, { data })

                    //const issueContainerEl = document.querySelector("#issue")
                    //issueContainerEl.innerHTML = data.content

                    // Handle successful response
                     window.location.href = `/${contentType}/${data.contentId}`;
                } else {
                    // Handle validation errors or other failures
                    // alert("Failed to create issue. Please check your form inputs and try again.");
                    console.error("Error details:", data);
                    data.errors.forEach(error => {
                        const fieldElement = form.querySelector(`*[name="${error[0]}"]`).closest(".form-element")
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

    // ParentIssue Select2 Search
    //const parentIssueSelectEl = document.querySelector("select#ParentIssueID")
    
    


    // Parent Solution Select2 Search

    

}


function parentIssueSearchMethod() {
    alert("Searching for parent issue")
}