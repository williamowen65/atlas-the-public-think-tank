document.addEventListener("DOMContentLoaded", () => {

    fetchComponent("components/inspiration-link.html")
        .then(res => renderComponent(res, "#link-container"))

})

function renderComponent(component, target) {
    const el = document.querySelector(target)
    el.insertAdjacentHTML("beforeend", component)


    // Execute any scripts in the inserted HTML
    const scripts = el.querySelectorAll('script')
    scripts.forEach(oldScript => {
        const newScript = document.createElement('script')
        // Copy all attributes from the old script to the new one
        Array.from(oldScript.attributes).forEach(attr => {
            newScript.setAttribute(attr.name, attr.value)
        })
        // Copy the content of the script
        newScript.textContent = oldScript.textContent
        // Replace the old script with the new one to execute it
        oldScript.parentNode.replaceChild(newScript, oldScript)
    })
    
}

async function fetchComponent(path) {
    return fetch(path)
        .then(res => res.text())
        .then(res => {
            return res
        }).catch(e => {
            console.error(e)
        })
}