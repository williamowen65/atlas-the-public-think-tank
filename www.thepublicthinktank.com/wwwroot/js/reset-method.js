/**
 * Adds a reset method to the container that resets the dial to the default position
 */
function createDialResetMethod(container, forumId, observer, dialId, options, state) {
    container.reset = function (silent = false) {
        console.log(`Resetting vote dial for forum ${forumId}`);

        // Disable the observer temporarily
        const observerState = observer.takeRecords();
        observer.disconnect();

        // Flag to prevent vote events during reset
        state.isResetting = true;

        // Find the default radio (value 5)
        const defaultRadio = document.querySelector(`input[name="${dialId}"][value="5"]`);

        if (defaultRadio) {
            // Set checked state without firing events
            defaultRadio.checked = true;

            // Get the label for scrolling
            const label = document.querySelector(`label[for="${defaultRadio.id}"]`);
            if (label) {
                // Calculate position
                const labelTop = label.offsetTop;
                const containerHeight = container.clientHeight;
                const labelHeight = label.clientHeight;
                
                let containerToReset = container;
                
                if (silent) {
                    // Create a new container to replace the old one
                    const newContainer = container.cloneNode(true);
                    container.parentNode.replaceChild(newContainer, container);
                    containerToReset = newContainer;
                    
                    // We need to recreate all event handlers and methods on the new container
                    setupScrollEvents(containerToReset, dialId, state);
                    
                    // Recreate the reset method on the new container (recursive but will only happen once)
                    createDialResetMethod(containerToReset, forumId, observer, dialId, options, state);
                }
                
                // Set scroll position with or without animation
                containerToReset.scrollTo({
                    top: labelTop - (containerHeight / 2) + (labelHeight / 2),
                    behavior: 'smooth'
                });
                
                // Remove user-voted class if present
                containerToReset.classList.remove('user-voted');
                
                // Update the container reference for the rest of the function
                container = containerToReset;
            }
        } else {
            // If we couldn't find the default radio, just remove the user-voted class
            container.classList.remove('user-voted');
        }
        
        // Re-enable the observer after the reset is complete
        setTimeout(() => {
            options.forEach(option => {
                observer.observe(option);
            });
            state.isResetting = false;
            
            // Re-enable scrolling functionality by simulating a small scroll
            // This will trigger the scroll event and set isScrolling to true
            if (!silent) {
                const currentScrollTop = container.scrollTop;
                container.scrollTop = currentScrollTop + 1;
                setTimeout(() => {
                    container.scrollTop = currentScrollTop;
                }, 10);
            }
        }, 500); // Delay to ensure scrolling is complete
    };
}
