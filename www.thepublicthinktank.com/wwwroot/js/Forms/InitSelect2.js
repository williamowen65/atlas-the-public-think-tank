


document.addEventListener("DOMContentLoaded", () => {

    const select2elements = Array.from(document.querySelectorAll(".my-select2"))
    select2elements.forEach(selectEl => {
        initSelect2(selectEl)
    })


    // Add this to document observer events 
    if (typeof documentObserver == 'object') {
        documentObserver.registerEvent(select2Listener)
    } else {
        throw new Error("documentObserver not defined")
    }

    function select2Listener(node) {
        // If the node itself has the .select2 class, initialize it
        if (node.classList && node.classList.contains('my-select2')) {
            initSelect2(node)
        }
        // Also initialize any child elements with .select2 class
        const childSelect2s = node.querySelectorAll
            ? node.querySelectorAll('.my-select2')
            : [];
        childSelect2s.forEach(el => {
            initSelect2(el)
        });
    }


    function initSelect2(node) {



        if (!(node instanceof Element)) return

        let configSelect2 = {
            width: '100%',
        }
        const hasAjaxCallback = node.hasAttribute("ajax-callback")
        if (hasAjaxCallback) {
            const callbackName = node.getAttribute("ajax-callback");


            // Check if the callback function exists in the global scope
            if (typeof window[callbackName] !== 'function') {
                throw new Error(`Ajax callback function '${callbackName}' not found`);
            }
            
            // Call the callback function to get the AJAX configuration
            const ajaxConfig = window[callbackName](node);
            
            // Apply the AJAX configuration to select2
            configSelect2 = Object.assign(configSelect2, ajaxConfig);
            
        }


        // check if node has attribute multiple
        const isMultiple = node.hasAttribute("multiple");
            //debugger
        if (isMultiple) {
            // Check for a placeholder option
            const placeholderOption = Array.from(node.options).find(
                opt => opt.value === "" && opt.text && !opt.disabled
            );

            if (placeholderOption) {
                // Remove the placeholder option from the select
                // Multi select place holder needs to be defined in options, not html.. 
                node.removeChild(placeholderOption);

                configSelect2.placeholder = placeholderOption.text

                // If select2 is initialized with a placeholder, it will show it in the UI
                $(node).select2(configSelect2);
                return;
            }
        }

        $(node).select2(configSelect2);

        const hasSelect2ListenerCallback = node.hasAttribute("select2-listener-callback")
        if (hasSelect2ListenerCallback) {
            const callbackName = node.getAttribute("select2-listener-callback");

            // Check if the callback function exists in the global scope
            if (typeof window[callbackName] !== 'function') {
                throw new Error(`select2 listener callback function '${callbackName}' not found`);
            }

            window[callbackName](node);
        }

        console.log({ configSelect2 })
    }

})