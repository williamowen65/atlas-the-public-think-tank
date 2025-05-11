 // Debounce function to limit how often the vote is saved
    function debounce(func, wait) {
        let timeout;
        return function() {
            const context = this;
            const args = arguments;
            clearTimeout(timeout);
            timeout = setTimeout(() => {
                func.apply(context, args);
            }, wait);
        };
}

function dismissTopbarAlert(alertId) {
    const alertElement = document.getElementById(alertId);
    if (alertElement) {
        // Fade out the alert
        alertElement.style.transition = 'opacity 0.5s';
        alertElement.style.opacity = '0';

        setTimeout(() => {
            alertElement.remove();

            // Hide container if no alerts left
            const container = document.getElementById('alert-container');
            if (container && container.querySelectorAll('.alert').length === 0) {
                container.style.display = 'none';
            }
        }, 500);
    }
}

