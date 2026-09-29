document.addEventListener("DOMContentLoaded", () => {
    userOpensGeneralFeedbackForm();
    userSendsAccessibilityFeedback()
    userSendsGeneralFeedback()
})

function userOpensGeneralFeedbackForm() {
    document.addEventListener("click", (e) => {
        if (e.target.closest(".open-general-feedback-form")) {
            const modal = document.querySelector("#general-feedback-modal");
            if (modal) {
                // Bootstrap 5 modal
                const bsModal = new bootstrap.Modal(modal);
                bsModal.show();
            }
        }
    })
}




function userSendsAccessibilityFeedback() {

    handleInBoundEmailAJAX({
        formSelector: "#accessibility-feedback-form",
    })
}


function userSendsGeneralFeedback() {
    handleInBoundEmailAJAX({
        formSelector: "#general-feedback-form",
    })
}




/**
 * These forms contain the InBoundEmailModel for predictable form fields
 * The view is _FeedbackForm.cshtml
 */
function handleInBoundEmailAJAX({
    formSelector,
}) {

    const form = document.querySelector(formSelector);
    const messageEl = form.querySelector("#Message");
    const messageType = form.getAttribute("data-message-type")
    const formSubmitButton = form.querySelector("button[type='submit']")
    const submitBtnText = formSubmitButton.innerText
    const feedbackModal = form.closest(".modal")
    const thankYouMessage = feedbackModal.querySelector(".thank-you-message")

    form.addEventListener("submit", (e) => {
        e.preventDefault();
        formSubmitButton.disabled = true
        const spinner = document.createElement("span");
        spinner.className = "spinner-border spinner-border-sm pagination-spinner";
        spinner.setAttribute("role", "status");
        spinner.setAttribute("aria-hidden", "true");
        formSubmitButton.innerText = ""
        formSubmitButton.insertAdjacentElement("beforeend", spinner)
        

        // check for min text
        const feedback = messageEl.value.trim();
        if (!feedback) {
            alert("requires more data");
            return;
        }


        const formData = new FormData(form); // <-- Should have ImageAttachemnts
        formData.set("Message", feedback);
        formData.set("MessageType", messageType);

        fetch("/feedback", {
            method: "POST",
            // Do NOT set Content-Type header; browser will set it for FormData
            body: formData
        })
            .then(res => res.json())
            .then(res => {
                if (res.success == false) {
                    const errorContainer = document.querySelector(`#${messageType}-feedback-error`)
                    errorContainer.innerText = res.message
                    formSubmitButton.innerHTML = submitBtnText
                }
                if (res.success == true) {
                    form.reset() // Empty all fields

                    thankYouMessage.classList.remove("d-none")
                    form.classList.add('d-none')
                    formSubmitButton.innerHTML = submitBtnText
                    formSubmitButton.disabled = false;


                    setTimeout(() => {
                        // Close the modal using Bootstrap's modal API
                        const modalInstance = window.bootstrap.Modal.getOrCreateInstance(feedbackModal);
                        modalInstance.hide();
                        setTimeout(() => {
                            // Time for the modal to hide
                            thankYouMessage.classList.add("d-none")
                            form.classList.remove('d-none')
                        }, 1000)
                    }, 3000)

                }
            })
            .catch(err => {
                console.log({ err });
                formSubmitButton.innerHTML = submitBtnText
            });
    });

}
