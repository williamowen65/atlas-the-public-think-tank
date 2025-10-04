document.addEventListener("DOMContentLoaded", () => {

    const emailSubscriptionInput = document.querySelector("#IsSubscribed")

    emailSubscriptionInput.addEventListener("change", (e) => {

        e.target.closest("form").submit()
    })
})