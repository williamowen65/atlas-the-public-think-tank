function client_TopBar_Alert({
    type, 
    message,
    dismissible = true,
    timeout = 5000
}) {
    // both type and message are required
    if (!type || !message) {
        console.error('Type and message are required for client_TopBar_Alert');
        return;
    }
    // Check if the alert type is valid
    const validTypes = ['success', 'info', 'warning', 'error'];
    if (!validTypes.includes(type)) {
        console.error('Invalid alert type:', type);
        return;
    }

    // Use Base64 encoding to safely transmit the message
    const encodedMessage = btoa(encodeURIComponent(message));

    // Use the query parameter for type but headers for other parameters
    fetch(`/Shared/_Alert?type=${type}`, {
        method: 'GET',
        headers: {
            'X-Requested-With': 'XMLHttpRequest',
            'X-Alert-Message': encodedMessage,
            'X-Alert-Dismissible': dismissible.toString(),
            'X-Alert-Timeout': timeout.toString()
        }
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Failed to load alert content');
            }
            return response.text();
        })
        .then(alertHtml => {
            // Find the target element and set the HTML content
            const alertTarget = document.getElementById('header-alert-target');
            if (alertTarget) {
                alertTarget.innerHTML = alertHtml;

                // Optional: Add a class to animate the alert appearing
                alertTarget.classList.add('alert-visible');

                const timeoutEl = alertTarget.querySelector("*[data-timeout]")
                const customTimeout = timeoutEl ? parseInt(timeoutEl.getAttribute("data-timeout"), 10) || 0 : 0;

                if (customTimeout > 0) {
                    setTimeout(() => {
                        dismissTopbarAlert('topbar-alert');
                    }, customTimeout);
                }
            }
        })
        .catch(error => {
            console.error('Error loading alert:', error);
        });
}