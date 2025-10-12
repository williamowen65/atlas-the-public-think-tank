const orientationLocalStorageKey = "orientation";

class OrientationStatus {


    constructor(step = 0, workflowName = null, workflowStep = null) {
        this._step = step;
        this._workflowName = workflowName;
        this._workflowStep = workflowStep;
    }

    get step() {
        return this._step;
    }

    set step(value) {
        // Ensure value is a number (or null)
        if (value !== null && typeof value !== 'number') {
            throw new TypeError('Step must be a number or null');
        }

        // Validate lower bound
        if (value !== null && value < 0) {
            console.warn('Step cannot be less than 0. Setting to 0.');
            this._step = 0;
            return;
        }

        // Validate upper bound (if maxStep is set)
        //if (this._maxStep !== null && value > this._maxStep) {
        //    console.warn(`Step cannot be more than ${this._maxStep}. Setting to ${this._maxStep}.`);
        //    this._step = this._maxStep;
        //    return;
        //}

        this._step = value;

        // By default, when working with workflows, you need to be explicit. Most steps won't have alt workflows.
        this._workflowName = null;
        this._workflowStep = 0;
    }

    get workflowStep() {
        return this._workflowStep;
    }

    set workflowStep(value) {
        // Ensure value is a number (or null)
        if (value !== null && typeof value !== 'number') {
            throw new TypeError('workflowStep must be a number or null');
        }

        // Validate lower bound
        if (value !== null && value < 0) {
            console.warn('workflowStep cannot be less than 0. Setting to 0.');
            this._workflowStep = 0;
            return;
        }

        this._workflowStep = value;
    }

    get workflowName() {
        return this._workflowName;
    }

    set workflowName(value) {
        // Ensure value is a number (or null)
        if (value !== null && typeof value !== 'string') {
            throw new TypeError('workflowStep must be a string or null');
        }

        this._workflowName = value;
    }



    

    static load() {
        const data = localStorage.getItem(orientationLocalStorageKey);
        if (!data) return 0;
        try {
            const obj = JSON.parse(data);
            return obj
        } catch {
            return 0
        }
    }

    save() {
        localStorage.setItem(orientationLocalStorageKey, JSON.stringify(this));
    }

    static clear() {
        localStorage.removeItem(orientationLocalStorageKey);
        
    }
}
async function moveOrientationToStep(
    step,
    context
){
    const {
        orientationModal,
        orientationContainer,
        tooltipArrow,
        backdrop,
        steps,
        targetFocus,
        leftSideBarToggle,
        widthAlignmentEnum
    } = context;

    let stepEntry = typeof step == 'function' ? await step() : await steps[step]()

    let isWorkflow = false

    if (context.orientationStatus.workflowName != null) {
        isWorkflow = true;
        let workflowStep
        try {
            // Use the workflow steps instead
            workflowStep = await stepEntry[context.orientationStatus.workflowName][context.orientationStatus.workflowStep]()
        } catch (err) {
            console.log({
                orientationStatus: context.orientationStatus,
                stepEntry
            })
            throw err;
        }
        stepEntry = workflowStep
    }

    const cardHeader = orientationModal.querySelector(".card-header")
    cardHeader.innerText = stepEntry.headerText
    const orientationContent = orientationModal.querySelector(".orientation-content")
    orientationContent.innerHTML = stepEntry.content
    if (!stepEntry.isDeadEndStep) {
        // This adds the normal flow buttons. Start/Mid/End
        orientationContainer.setAttribute("data-step", step === 0 ? "start" : step < steps.length - 1 ? "mid" : "end")
    } 

    if (isWorkflow) {
        // This will add back buttons for the specific workflow (Back & next, but for a workflow)
        orientationContainer.setAttribute("data-step", "workflow")
    }

    if (stepEntry.isDeadEndStep) {
        // Adding this attribute adds the "Go back to orientation" button instead of the other buttons
        orientationContainer.setAttribute("data-step", "isDeadEndStep")
    }

    if (stepEntry.preConditionLogic) {
        stepEntry.preConditionLogic(context)
    }

    if (stepEntry.hideModal == true) {
        orientationModal.classList.add('d-none')
    } else {
        orientationModal.classList.remove('d-none')
    }

    handleBackdropWindow()
    handleBackdropWindowForMultipleElements()

    handleTooltipMode()


    scrollToModal()

    if (stepEntry.customLogic) {
        /**
         * This was used to put info in a modal.
         * A work around because the z-index couldn't get in front of modal
         * So decided to just show some custom orientation step with in the modal
         */
        stepEntry.customLogic(context)
    }

    if (stepEntry.accessibility) {
        stepEntry.accessibility()
    }


    function scrollToModal() {
        const modal = orientationModal.getBoundingClientRect()
        if (modal.top < 0) {
            window.scrollTo(0, window.scrollY + modal.top - window.innerHeight * .2)
        }
        if (modal.top > window.screen.height - modal.height) {
            window.scrollTo(0, window.scrollY + modal.top - window.innerHeight * .2)
        }

    }


    function handleTooltipMode() {

        orientationModal.style.minWidth = ""
        orientationModal.style.width = ""

        if (stepEntry.tooltipMode == true) {

            if (!stepEntry.element) {
                throw new Error("orientation tooltip element info missing for tooltip mode.");
            }
            if (!stepEntry.position) {
                throw new Error("orientation tooltip postion info missing for tooltip mode");
            }
            if (!stepEntry.arrow) {
                throw new Error("orientation tooltip arrow info missing for tooltip mode");
            }
            const targetElement = document.querySelector(stepEntry.element);

            // Set modal to tooltip mode
            orientationModal.classList.add("tooltip-mode");

            // Use orientation-target-focus from the DOM

            // Show and style the targetFocus element to highlight the target
            const rect = targetElement.getBoundingClientRect();
            targetFocus.style.position = "fixed";
            targetFocus.style.top = `${rect.top}px`;
            targetFocus.style.left = `${rect.left}px`;
            targetFocus.style.width = `${rect.width}px`;
            targetFocus.style.height = `${rect.height}px`;
            targetFocus.style.zIndex = "9999";
            targetFocus.style.pointerEvents = "none";
            targetFocus.style.display = "block";

            // Add orientation-modal to the orientation-tooltip-overlay
            targetFocus.appendChild(orientationModal)


            orientationModal.style.top = ""
            orientationModal.style.bottom = ""
            orientationModal.style.left = ""
            orientationModal.style.right = ""
            orientationModal.style.transform = ""

            Object.entries(stepEntry.position).forEach(([key, value]) => {
                orientationModal.style[key] = value;
            });

            
            stepEntry.arrow && Object.entries(stepEntry.arrow).forEach(([key, value]) => {
                tooltipArrow.style[key] = value;
            });

            callUpdateToolTipWidthTwice()


            // Listener function to update targetFocus position/size
            function updateTargetFocus() {
                const rect = targetElement.getBoundingClientRect();
                targetFocus.style.top = `${rect.top}px`;
                targetFocus.style.left = `${rect.left}px`;
                targetFocus.style.width = `${rect.width}px`;
                targetFocus.style.height = `${rect.height}px`;
                updateToolTipWidth()
            }

            /*
            The initial calc is based on no widht settings
            The second calc is based on calculated settings and provides the correct result
            This id due to a position absolute, where the relative parent element has very little width to begin with
            */
            function callUpdateToolTipWidthTwice() {
                updateToolTipWidth()
                updateToolTipWidth()
            }


            /**
             * The width of elements naturally want to be the width of the target focus element.
             * but this is unreliable.
             * 
             * Any element with this issue can provide widthAlignment (left or right.)
             * widthAlignment: widthAlignmentEnum.right
             */
            function updateToolTipWidth() {

                const modalRect = orientationModal.getBoundingClientRect();
                let maxAvailableWidth = window.innerWidth; // default to full window

                if (stepEntry.widthAlignment) {
                    if (stepEntry.widthAlignment === widthAlignmentEnum.right) {
                        // Available space to the right
                        maxAvailableWidth = window.innerWidth - modalRect.left;
                    }

                    if (stepEntry.widthAlignment === widthAlignmentEnum.left) {
                        // Available space to the left
                        maxAvailableWidth = modalRect.right;
                    }

                    // Apply minWidth (visual constraint)
                    orientationModal.style.minWidth = `${Math.min(maxAvailableWidth, 300)}px`;
                }

                if (stepEntry.desiredWidth) {
                    // Cap desired width so it doesn't overflow
                    const cappedWidth = Math.min(stepEntry.desiredWidth, maxAvailableWidth);
                    orientationModal.style.width = `${cappedWidth}px`;
                }
            }


            // Clean up previous listeners if they exist
            if (targetFocus._scrollListener_tooltipFocus) {
                window.removeEventListener('scroll', targetFocus._scrollListener_tooltipFocus);
                window.removeEventListener('resize', targetFocus._scrollListener_tooltipFocus);
            }

            // Store the listener reference for cleanup
            targetFocus._scrollListener_tooltipFocus = updateTargetFocus;

            // Add scroll and resize listeners to keep targetFocus aligned
            window.addEventListener('scroll', targetFocus._scrollListener_tooltipFocus, { passive: true });
            window.addEventListener('resize', targetFocus._scrollListener_tooltipFocus, { passive: true });

        } else {
            orientationModal.classList.remove("tooltip-mode");
            orientationModal.style.top = ""
            orientationModal.style.bottom = ""
            orientationModal.style.left = ""
            orientationModal.style.right = ""
            orientationModal.style.transform = ""
            orientationContainer.appendChild(orientationModal)


            tooltipArrow.style.top = ""
            tooltipArrow.style.bottom = ""
            tooltipArrow.style.left = ""
            tooltipArrow.style.right = ""
            tooltipArrow.style.translate = ""
            tooltipArrow.style.border = ""
            tooltipArrow.style.borderTop = ""
            tooltipArrow.style.borderLeft = ""
            tooltipArrow.style.borderRight = ""
            tooltipArrow.style.borderBottom = ""

            // Hide and clean up targetFocus if target not found
            const targetFocus = backdrop.querySelector("#orientation-target-focus");
            if (targetFocus) {
                targetFocus.style.display = "none";
                if (targetFocus._scrollListener_tooltipFocus) {
                    window.removeEventListener('scroll', targetFocus._scrollListener_tooltipFocus);
                    window.removeEventListener('resize', targetFocus._scrollListener_tooltipFocus);
                    targetFocus._scrollListener_tooltipFocus = null;
                }
            }
        }
    }


    /**
     * Method for controlling a window in the backdrop
     * 
     * /// TODO: Update this to handle multiple elements
     * 
     */
    function handleBackdropWindow() {
        const targetElement = document.querySelector(stepEntry.element)
        if (targetElement) {


            function updateBackgroundWindow() {
                const elBounding = targetElement.getBoundingClientRect();
                // Update target focus highlight
                targetFocus.style.position = "fixed";  // Changed from absolute
                targetFocus.style.top = `${elBounding.top}px`;  // Removed window.scrollY
                targetFocus.style.left = `${elBounding.left}px`;  // Removed window.scrollX
                targetFocus.style.width = `${elBounding.width}px`;
                targetFocus.style.height = `${elBounding.height}px`;
                targetFocus.style.display = "block";

                // Make the backdrop have a "hole" where the target is
                // These coordinates are already viewport-relative from getBoundingClientRect
                backdrop.style.clipPath = `polygon(
                    0% 0%, 
                    0% 100%, 
                    ${elBounding.left}px 100%, 
                    ${elBounding.left}px ${elBounding.top}px, 
                    ${elBounding.right}px ${elBounding.top}px, 
                    ${elBounding.right}px ${elBounding.bottom}px, 
                    ${elBounding.left}px ${elBounding.bottom}px, 
                    ${elBounding.left}px 100%, 
                    100% 100%, 
                    100% 0%
                )`;
            }

            //// Initial position update
            updateBackgroundWindow();

            //// Clean up previous listeners if they exist
            if (backdrop._scrollListener_backgroundWindow) {
                window.removeEventListener('scroll', backdrop._scrollListener_backgroundWindow);
                window.removeEventListener('resize', backdrop._scrollListener_backgroundWindow);
                leftSideBarToggle.removeEventListener("click", backdrop._scrollListener_backgroundWindow_wTimeout)
            }

            // Store the listener reference for cleanup
            backdrop._scrollListener_backgroundWindow = updateBackgroundWindow;
            backdrop._scrollListener_backgroundWindow_wTimeout = () => {
                setTimeout(updateBackgroundWindow, 500)
            };

            // Add scroll and resize listeners to keep everything aligned
            window.addEventListener('scroll', backdrop._scrollListener_backgroundWindow, { passive: true });
            window.addEventListener('resize', backdrop._scrollListener_backgroundWindow, { passive: true });
            leftSideBarToggle.addEventListener("click", backdrop._scrollListener_backgroundWindow_wTimeout, { passive: true })
        }

        else {
            backdrop.style.clipPath = null;
            //// Clean up scroll/resize listeners
            if (backdrop._scrollListener_backgroundWindow) {
                window.removeEventListener('scroll', backdrop._scrollListener_backgroundWindow);
                window.removeEventListener('resize', backdrop._scrollListener_backgroundWindow);
                leftSideBarToggle.removeEventListener("click", backdrop._scrollListener_backgroundWindow_wTimeout)
                backdrop._scrollListener_backgroundWindow = null;
                backdrop._scrollListener_backgroundWindow_wTimeout = null;
            }     
        }
    }
    function handleBackdropWindowForMultipleElements() {
        if (!stepEntry.elements) return
        if (!Array.isArray(stepEntry.elements)) throw new Error("stepEntry.elements should be an array")
        const query = stepEntry.elements.join(", ")
        const targetElements = Array.from(document.querySelectorAll(query))
        if (targetElements) {

            function updateBackgroundWindow() {
                const elBoundings = targetElements.map(e => e.getBoundingClientRect());

                    const partialPolygons = elBoundings.map(elBounding => {
                        return `
                         ${elBounding.left}px 100%,
                        ${elBounding.left}px ${elBounding.top}px,
                        ${elBounding.right}px ${elBounding.top}px,
                        ${elBounding.right}px ${elBounding.bottom}px,
                        ${elBounding.left}px ${elBounding.bottom}px,
                        ${elBounding.left}px 100%,
                        `
                    })

                // Make the backdrop have a "hole" where the target is
                // These coordinates are already viewport-relative from getBoundingClientRect
                backdrop.style.clipPath = `polygon(
                    0% 0%, 
                    0% 100%,
                    ${partialPolygons.join(" ")}
                    100% 100%, 
                    100% 0%
                )`;
            }
          
            //// Initial position update
            updateBackgroundWindow();

            //// Clean up previous listeners if they exist
            if (backdrop._scrollListener_backgroundWindow_multipleElements) {
                window.removeEventListener('scroll', backdrop._scrollListener_backgroundWindow_multipleElements);
                window.removeEventListener('resize', backdrop._scrollListener_backgroundWindow_multipleElements);
            }

            // Store the listener reference for cleanup
            backdrop._scrollListener_backgroundWindow_multipleElements = updateBackgroundWindow;
            

            // Add scroll and resize listeners to keep everything aligned
            window.addEventListener('scroll', backdrop._scrollListener_backgroundWindow_multipleElements, { passive: true });
            window.addEventListener('resize', backdrop._scrollListener_backgroundWindow_multipleElements, { passive: true });
            
        }

        else {
            backdrop.style.clipPath = null;
            //// Clean up scroll/resize listeners
            if (backdrop._scrollListener_backgroundWindow_multipleElements) {
                window.removeEventListener('scroll', backdrop._scrollListener_backgroundWindow_multipleElements);
                window.removeEventListener('resize', backdrop._scrollListener_backgroundWindow_multipleElements);
                
                backdrop._scrollListener_backgroundWindow_multipleElements = null;
                
            }     
        }
    }



}


function getOrientationElements() {
    const orientationContainer = document.querySelector("#orientation-container")
    const orientationModal = orientationContainer.querySelector("#orientation-modal")
    const backdrop = orientationContainer.querySelector("#orientation-backdrop")
    const targetFocus = orientationContainer.querySelector("#orientation-target-focus")
    const tooltipArrow = orientationModal.querySelector("#orientationTooltipArrow")
    const leftSideBarToggle = document.querySelector(".left-sidebar-toggle")

    /**
     * this enum is useful for tooltip widths on mobile.
     * Makeing sure the tooltip doesn't go off the screen.
     * The tooltip is an absolute element that doesn't obey bounds of window on its own.
     */
    const widthAlignmentEnum = {
        right: 'right',
        left: 'left'
    }

    const context = {
        orientationContainer,
        orientationModal,
        backdrop,
        targetFocus,
        tooltipArrow,
        leftSideBarToggle,
        widthAlignmentEnum,
        buttons: {
            skipOrientationButton: orientationModal.querySelector("#skip-orientation-btn"),
            startOrientationButton: orientationModal.querySelector("#start-orientation-btn"),
            /* step-back-orientation button is in the mid and end btns. */
            stepBackOrientationButtons: orientationModal.querySelectorAll(".step-back-orientation-btn"),
            stepForwardOrientationButton: orientationModal.querySelector("#step-forward-orientation-btn"),
            finishOrientationButton: orientationModal.querySelector("#finish-orientation-btn"),
            goBackToOrientationButton: orientationModal.querySelector("#go-back-to-orientation-btn"),
            workflowBackOrientationButton: orientationModal.querySelector("#workflow-back-orientation-btn"),
            workflowForwardOrientationButton: orientationModal.querySelector("#workflow-forward-orientation-btn"),
        }
    }



    const arrowPosition = {
        leftMiddle: {
            // arrow pointing left
            border: "10px solid transparent",
            borderRight: "10px solid #fff",
            top: "50%",
            left: "0%",
            transform: "translate(-20px,-25%)",
            width: "10px"
        },
        rightMiddle: {
            // arrow pointing left
            border: "10px solid transparent",
            borderLeft: "10px solid #fff",
            top: "50%",
            right: "0%",
            left: "",
            transform: "translate(20px,-25%)",
            width: "10px"
        },
        downMiddle: {
            // arrow pointing down
            border: "10px solid transparent",
            borderTop: "10px solid #fff",
            top: "100%",
            left: "50%",
            transform: "translateX(-100%)",
            width: "10px"
        },
        topMiddle: {
            // arrow pointing down
            border: "10px solid transparent",
            borderBottom: "10px solid #fff",
            bottom: "100%",
            left: "50%",
            transform: "translateX(-100%)",
            width: "10px",
            top: "-20px"
        }
    }

    const modalPosition = {
        right: {
            transform: "translate(110px, -50%)",
            top: "50%",
            right: "0%",
            left: "0%",
        },
        top: {
            transform: "translate(-50%, -107%)",
            top: "0px"
        },
        bottom: {
            transform: "translate(-50%, 107%)",
            top: "0px"
        },
        left: {
            transform: "translate(-113%, -50%)",
            top: "50%",
            right: "0%",
            left: "0%",
        },
        center: {
            transform: "translate(-50%, -50%)",
            top: "50%",
            left: "50%"
        }
    }

    modalPosition.rightAdjusted = {
        ...modalPosition.right,
        transform: "translate(80px, -50%)",
    }

    return {
        context,
        modalPosition,
        arrowPosition
    }
}


document.addEventListener("DOMContentLoaded", async () => {

    const orientationElements = getOrientationElements()
    const {
        context,
    } = orientationElements

    const {
        orientationContainer,
    } = context


    if (typeof getOrientationSteps != "function") {
        throw new Error("Missing getOrientationSteps method")
    }



    // CREATE THE ROOT OrientationStatus Object used for tracking project

    const orientationStatus = new OrientationStatus(OrientationStatus.load()._step, OrientationStatus.load()._workflowName, OrientationStatus.load()._workflowStep);
    context.orientationStatus = orientationStatus;


    const {
        steps,
        customSteps
    } = getOrientationSteps(orientationElements, orientationStatus)

    context.steps = steps
    context.customSteps = customSteps

    setOrientationButtonListeners(context)

    setCustomOrientationWorkflowListeners(context)

    const showStepOnLoad = () => {
        // Show the orientation modal
        orientationContainer.classList.remove("d-none")

        // Move to the current step if in-progress
        moveOrientationToStep(orientationStatus.step, context)
    }

    if (orientationStatus.step == 0) {
        setTimeout(() => {
            showStepOnLoad()
        }, 3000)
    } else {
        showStepOnLoad()
    }



})


async function fetchOrientationStep(step) {
    return await fetch(`/orientation?step=${step}`)
        .then(response => {
            if (!response.ok) {
                throw new Error("Network response was not ok");
            }
            return response.json();
        })
        .then(data => {
            if (data.todoOrientation) {
                return data.html
            }
        })
        .catch(error => {
            console.error("Error loading orientation modal:", error);
        });
}

/**
 * These are for actions the user might take during orientation
 * ex: Voting on content
 *     Clicking to view the vote log
 * @param {any} context
 */
function setCustomOrientationWorkflowListeners(context) {

    if (typeof documentObserver == 'object') {
        documentObserver.registerEvent(watchForUserVoteDuringOrientation)
        documentObserver.registerEvent(watchForUserVoteCountModal)
    } else {
        throw error("documentObserver not defined")
    }

    function watchForUserVoteCountModal(node) {
        if (node.nodeType === 1 && node.id === "userVoteModal") {
            // Move to custom orientation step when #confirmVoteModal appears
            const { customSteps } = context;

            const modal = document.querySelector("#userVoteModal")
            console.log("setting listener on", modal)

            // add event listner on opened modal
            modal.addEventListener('shown.bs.modal', function () {
                // Code to run when modal is fully shown
                
                moveOrientationToStep(
                    customSteps.userOpensContentVoteModal,
                    context,
                );

            });
        }
    }

    function watchForUserVoteDuringOrientation(node) {
        if (node.nodeType === 1 && node.id === "confirmVoteModal") {
            console.log("confirm vote modal loaded")
            // Move to custom orientation step when #confirmVoteModal appears
            const { customSteps } = context;
            const modal = document.querySelector("#confirmVoteModal")
            console.log("setting listener on", modal)
            modal.addEventListener('shown.bs.modal', function () {
                moveOrientationToStep(
                    customSteps.userVotedDuringOrientation,
                    context,
                );

                // On click of cancel or confirm, go back to last step
                const stepBackOrientationBtn = document.querySelector("#go-back-to-orientation-btn")
                const cancelVoteBtn = document.querySelector("#confirmVoteModal .close-modal")
                const confirmVoteBtn = document.querySelector("#confirmVoteModalConfirmBtn")

                const cancelBtnHandler = () => {
                    moveOrientationToStep(
                        context.orientationStatus.step,
                        context,
                    );
                }
                cancelVoteBtn.addEventListener("click", cancelBtnHandler)

                const stepBackOrientationBtnHandler = () => {
                    cancelVoteBtn.removeEventListener("click", cancelBtnHandler);
                    cancelVoteBtn.click();
                    stepBackOrientationBtn.removeEventListener("click", stepBackOrientationBtnHandler);
                };
                stepBackOrientationBtn.removeEventListener("click", stepBackOrientationBtnHandler)
                stepBackOrientationBtn.addEventListener("click", stepBackOrientationBtnHandler);

                confirmVoteBtn.addEventListener("click", () => {
                    moveOrientationToStep(
                        context.orientationStatus.step,
                        context,
                    );
                })
            })

            modal.addEventListener('hidden.bs.modal', function () {
                // Code to run when modal is closed
                moveOrientationToStep(context.orientationStatus.step, context);
            });
        }
    }
}

function setOrientationButtonListeners(context) {

    const {
        orientationStatus,
        orientationContainer,
        orientationModal,
        buttons: {
            skipOrientationButton,
            startOrientationButton,
            finishOrientationButton,
            stepForwardOrientationButton,
            stepBackOrientationButtons,
            goBackToOrientationButton,
            workflowBackOrientationButton,
            workflowForwardOrientationButton
        },
        steps
    } = context;

    skipOrientationButton.addEventListener("click", skipOrientation)
    startOrientationButton.addEventListener("click", startOrientation)
    finishOrientationButton.addEventListener("click", completeOrientation)
    stepForwardOrientationButton.addEventListener("click", goToNextStepInOrientation)
    stepBackOrientationButtons.forEach(button => {
        button.addEventListener("click", goBackAStepInOrientation);
    });
    goBackToOrientationButton.addEventListener("click", () => {
        orientationStatus.workflowName = null;
        orientationStatus.workflowStep = 0;
        orientationStatus.save()

        moveOrientationToStep(orientationStatus.step, context)
    })
    workflowBackOrientationButton.addEventListener("click", () => {
        const workflowStep = orientationStatus.workflowStep;
        if (workflowStep == 0) {
            orientationStatus.workflowName = null;
        } else {
            orientationStatus.workflowStep -= 1;
        }
        orientationStatus.save()
        moveOrientationToStep(orientationStatus.step, context)
    })
    workflowForwardOrientationButton.addEventListener("click", async () => {
        const step = orientationStatus.step;
        const workflowName = orientationStatus.workflowName;
        const workflowStep = orientationStatus.workflowStep;
        const currentMainStep = await context.steps[step]();
        const workflowSteps = currentMainStep[workflowName];
        const workflowLength = workflowSteps.length - 1;
        const isAtEndOfEndOfWorkflow = workflowStep == workflowLength;
        if (isAtEndOfEndOfWorkflow) {
            orientationStatus.workflowName = null;
            orientationStatus.workflowStep = 0;
        } else {
            orientationStatus.workflowStep += 1;
        }
        orientationStatus.save()
        moveOrientationToStep(orientationStatus.step, context)
    })


    async function goToNextStepInOrientation() {
        orientationStatus.step += 1;
        orientationStatus.save()

        // show the first step
        moveOrientationToStep(orientationStatus.step, context)
    }

    async function goBackAStepInOrientation() {
        orientationStatus.step -= 1;
        orientationStatus.save()

        // show the first step
        moveOrientationToStep(orientationStatus.step, context)
    }

    async function skipOrientation() {
        if (confirm("Do you want to skip the orientation")) {
            completeOrientation();
        }
    }

    /**
     * Sets a timestamp for completing the orientation even if skipped
     */
    async function completeOrientation() {
        
        fetch("/orientation?complete=true")
            .then(response => {
                if (!response.ok) {
                    throw new Error("Network response was not ok");
                }
            })
            .then(() => {
                // Remove the dom element related to the orientation
                orientationContainer.remove();
                OrientationStatus.clear();

            })
            .catch(err => {
                console.error("Failed to skip orientation:", err);
            });
    }

    /**
     * The orientation state is controlled of a local storage item
     * which is removed at the end of the orientation.
     * 
     * The localstoarge item is useful for the orientation spanning multiple pages.
     */
    async function startOrientation() {

        // update local storage
        orientationStatus.step += 1;
        orientationStatus.save();

        const isOnHomePage = location.pathname === "/";
        if (isOnHomePage) {
            // show the first step
            moveOrientationToStep(orientationStatus.step, context)
        } else {
            location.href = "/"
        }
    }

   
}

// Utility function to clone a node and copy its event listeners
function cloneNodeWithEvents(node) {
    // Clone the node deeply
    const clone = node.cloneNode(true);

    // Copy event listeners if any are attached via dataset (custom solution)
    // This requires that event listeners are registered and tracked manually
    // Example: node._eventListeners = [{ type: 'click', listener: fn }]
    if (node._eventListeners) {
        clone._eventListeners = [];
        node._eventListeners.forEach(({ type, listener, options }) => {
            clone.addEventListener(type, listener, options);
            clone._eventListeners.push({ type, listener, options });
        });
    }

    // Recursively copy event listeners for child nodes
    const origChildren = node.children;
    const cloneChildren = clone.children;
    for (let i = 0; i < origChildren.length; i++) {
        cloneNodeWithEvents(origChildren[i], cloneChildren[i]);
    }

    return clone;
}

// Usage example replacing:
// const clone = orientationModal.cloneNode(true)
//const clone = cloneNodeWithEvents(orientationModal);