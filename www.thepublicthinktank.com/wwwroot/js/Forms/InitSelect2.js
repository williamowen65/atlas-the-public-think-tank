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
                node.removeChild(placeholderOption);

                // If select2 is initialized with a placeholder, it will show it in the UI
                $(node).select2({
                    width: '100%',
                    placeholder: placeholderOption.text,
                    //allowClear: true
                });
                return;
            }
        }

        $(node).select2({ width: '100%' });
    }
})