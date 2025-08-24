
const documentObserver = new MutationObserver(mutations => {
    mutations.forEach(mutation => {
        mutation.addedNodes.forEach(node => {

            documentObserver.events.forEach(e => e(node))

        });
    });
});


documentObserver.observe(document, { childList: true, subtree: true });

documentObserver.events = [];
documentObserver.registerEvent = function (eventLogic) {

    documentObserver.events.push(eventLogic)
}