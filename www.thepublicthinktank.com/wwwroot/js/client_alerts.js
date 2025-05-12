
const AlertTargets = {
    TopBar: "TopBar",
    CardFooter: "CardFooter"
}

// Alert target handlers
const alertHandlers = {
    [AlertTargets.TopBar]: {
        getTargetElement: () => document.getElementById('header-alert-target'),
        getDismissFunction: () => (id) => dismissAlert(id),
        getErrorMessage: () => 'Topbar alert target not found'
    },
    [AlertTargets.CardFooter]: {
        getTargetElement: (cardId) => document.getElementById(`card-${cardId}-footer-alert`),
        getDismissFunction: (cardId) => (id) => dismissAlert(id),
        getErrorMessage: (cardId) => `Alert target for card ID ${cardId} not found`
    }
};

// Main alert function
function showAlert({
    target = AlertTargets.TopBar,
    type,
    message,
    cardId = null,
    dismissible = true,
    timeout = 5000
}) {
    // Validate required parameters
    if (!type || !message) {
        console.error('Type and message are required for showAlert');
        return;
    }
    
    // Check if the target is CardFooter and cardId is provided
    if (target === AlertTargets.CardFooter && !cardId) {
        console.error('CardId is required for CardFooter alerts');
        return;
    }
    
    // Check if the alert type is valid
    const validTypes = ['success', 'info', 'warning', 'error', "plaintext"];
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
            const handler = alertHandlers[target];
            const targetElement = handler.getTargetElement(cardId);
            
            if (targetElement) {
                targetElement.innerHTML = alertHtml;
                targetElement.classList.add('alert-visible');

                const timeoutEl = targetElement.querySelector("*[data-timeout]");
                const customTimeout = timeoutEl ? parseInt(timeoutEl.getAttribute("data-timeout"), 10) || 0 : 0;                if (customTimeout > 0) {
                    const dismissFunc = handler.getDismissFunction(cardId);
                    const alertElement = targetElement.querySelector(".alert");
                    if (alertElement && alertElement.id) {
                        setTimeout(() => {
                            dismissFunc(alertElement.id);
                        }, customTimeout);
                    }
                }
            } else {
                console.error(handler.getErrorMessage(cardId));
            }
        })
        .catch(error => {
            console.error('Error loading alert:', error);
        });
}

// Convenience functions to match your existing API
function client_TopBar_Alert(options) {
    return showAlert({
        target: AlertTargets.TopBar,
        ...options
    });
}

function client_CardFooter_Alert(options) {
    return showAlert({
        target: AlertTargets.CardFooter,
        ...options
    });
}

function dismissAlert(alertId) {
    const alertElement = document.getElementById(alertId);

    console.log({ alertElement })
    if (alertElement) {
        // Get alert Parent
        const alertElParent = alertElement.closest(".alert-container")
        // Fade out the alert
        alertElParent.style.transition = 'opacity 0.5s';
        alertElParent.style.opacity = '0';

        setTimeout(() => {
            alertElParent.remove();

            // Hide container if no alerts left
            const container = document.getElementById('alert-container');
            if (container && container.querySelectorAll('.alert').length === 0) {
                container.style.display = 'none';
            }
        }, 500);
    }
}

function dismissCardFooterAlert(alertId) {
    const alert = document.getElementById(alertId);
    if (alert) {
        alert.classList.add('alert-dismissing');
        setTimeout(() => {
            alert.innerHTML = '';
            alert.classList.remove('alert-visible', 'alert-dismissing');
        }, 300); // Match this to your CSS transition duration
    }
}