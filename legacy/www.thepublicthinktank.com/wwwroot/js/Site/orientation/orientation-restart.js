
document.addEventListener("DOMContentLoaded", () => {
    setOrientationListeners();
});



/**
 * This exist in sitejs b/c the orientation folder files are only loaded if orientation is a possiblity.
 */
function setOrientationListeners() {
    const restartOrientation = document.querySelector("#restart-orientation");
    if (!restartOrientation) return;

    restartOrientation.addEventListener("click", () => {
        fetch("/orientation?restart=true")
            .then(response => {
                if (!response.ok) {
                    throw new Error("Network response was not ok");
                }
            })
            .then(() => {
                // redirect to home page
                window.location.replace("/");
            })
            .catch(err => {
                console.error("Failed to restart orientation:", err);
            });
    });
}