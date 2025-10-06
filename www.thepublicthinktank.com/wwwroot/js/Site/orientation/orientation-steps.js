function getOrientationSteps(orientationElements, orientationStatus) {

    const isMobile = () => window.innerWidth < 768;


    const {
        context,
        modalPosition,
        arrowPosition
    } = orientationElements

    const {
        orientationContainer,
        orientationModal,
        backdrop,
        targetFocus,
        tooltipArrow,
        leftSideBarToggle,
        widthAlignmentEnum,
        buttons
    } = context

    const steps = [
        async () => ({
            headerText: "Orientation: Intro",
            content: await fetchOrientationStep("Intro-To-Orientation"),
            customLogic: animateIntroInspirationLink
        }),
        async () => ({
            headerText: "Orientation: Issue & Solution Cards",
            tooltipMode: true,
            content: await fetchOrientationStep("Content-Card-Orientation"),
            element: '.issue-card:nth-child(2), .solution-card:nth-child(2)',
            position: modalPosition.top,
            arrow: arrowPosition.downMiddle,
        }),
        async () => ({
            headerText: "This is the vote dial",
            tooltipMode: true,
            element: '.dial',
            content: await fetchOrientationStep("Vote-Component-Orientation"),
            position: modalPosition.right,
            arrow: arrowPosition.leftMiddle,
            widthAlignment: widthAlignmentEnum.right 
        }),
        async () => ({
            headerText: "The average of all votes",
            tooltipMode: true,
            element: '.vote-average',
            content: await fetchOrientationStep("Vote-Average-Orientation"),
            position: modalPosition.rightAdjusted,
            arrow: arrowPosition.leftMiddle,
            widthAlignment: widthAlignmentEnum.right
        }),
        async () => ({
            headerText: "The vote dial",
            tooltipMode: true,
            element: '.vote-dial-toggle',
            content: await fetchOrientationStep("Vote-Dial-Orientation"),
            position: modalPosition.rightAdjusted,
            arrow: arrowPosition.leftMiddle,
            widthAlignment: widthAlignmentEnum.right
        }),
        async () => ({
            headerText: "The vote count",
            tooltipMode: true,
            element: '.vote-count',
            content: await fetchOrientationStep("Vote-TotalVotes-Orientation"),
            position: modalPosition.rightAdjusted,
            arrow: arrowPosition.leftMiddle,
            widthAlignment: widthAlignmentEnum.right
        }),
        async () => ({
            headerText: "This is a scope ribbon",
            tooltipMode: true,
            element: ".ribbon",
            content: await fetchOrientationStep("Scope-Ribbon-Orientation"),
            position: {
                ...modalPosition.left,
                transform: "translate(-105%, -74%)"
            },
            arrow: {
                ...arrowPosition.rightMiddle,
                top: "66%"
            },
            widthAlignment: widthAlignmentEnum.left,
            desiredWidth: 600
        }),
        async () => ({
            headerText: "Quick Links / Info Counts",
            tooltipMode: true,
            element: ".issue-card-stat-icons, .solution-card-stat-icons",
            content: await fetchOrientationStep("Content-Stat-Icons-Orientation"),
            position: {
                ...modalPosition.left,
                transform: "translate(-102%, -55%)"
            },
            arrow: arrowPosition.rightMiddle,
            widthAlignment: widthAlignmentEnum.left,
            desiredWidth: 600
        }),
        async () => ({
            headerText: "This is a breadcrumb",
            tooltipMode: true,
            element: "nav[aria-label=breadcrumb]",
            content: await fetchOrientationStep("Breadcrumb-Orientation"),
            position: modalPosition.top,
            arrow: arrowPosition.downMiddle
        }),
        async () => ({
            headerText: "Left Side bar",
            tooltipMode: true,
            element: ".left-sidebar-toggle",
            content: "Open and close the side bar",
            position: {
                ...modalPosition.right,
                transform: "translate(15%, -50%)",
                top: "12px"
            },
            arrow: arrowPosition.leftMiddle,
            widthAlignment: widthAlignmentEnum.right,
        }),
        async () => ({
            headerText: "Sidebar Views",
            tooltipMode: true,
            element: ".view-mode-toggle",
            content: "Toggle between sidebar views",
            position: isMobile() ? {
                ...modalPosition.bottom,
                transform: "translate(-50%, 69%)",
                top: "-20px"
            } : {
                ...modalPosition.right,
                transform: "translate(45%, -50%)",
                top: "12px"
            },
            arrow: isMobile() ? arrowPosition.topMiddle : arrowPosition.leftMiddle,
            widthAlignment: isMobile() ? null : widthAlignmentEnum.right,
            desiredWidth: 300,
            preConditionLogic: () => {
                confirmSideBarOpen()
            },
            customLogic: () => {
                setListenerOnSideBarToggles()
            }
        }),
        async () => ({
            headerText: "Content Filter / Sort",
            tooltipMode: isMobile() ? false : true,
            element: "#left-sidebar-container",
            content: await fetchOrientationStep("Content-Filter-Orientation"),
            position: isMobile() ? null : {
                ...modalPosition.right,
                transform: "translate(105%, -50%)",
                top: "22%"
            },
            arrow: isMobile() ? null : arrowPosition.leftMiddle,
            widthAlignment: isMobile()  ? null : widthAlignmentEnum.right,
            preConditionLogic: () => {
                confirmSideBarOpen()
                confirmContentFilterSelected()
            },
            customLogic: () => {
            }
        }),
        async () => ({
            //headerText: "Creating Issues and Solutions",
            headerText: "There are 2 general ways to create new issues and/or solutions",
            content: await fetchOrientationStep("Create-Issues-And-Solutions-Orientation"),
            customLogic: setListenersForCreateIssueAndSolutionStep,
            /**
             * 2 sub workflows triggered from modal
             * customLogic is used to set in motion these workflows
             */
            userClicksShowHow_CreateButtonWorkflow: [
                async () => ({
                    headerText: "The Create button",
                    element: '#createDropdown',
                    tooltipMode: true,
                    content: await fetchOrientationStep("Create-Content-Header-Button-Orientation"),
                    position: {
                        ...modalPosition.bottom,
                        transform: "translate(-86%, 12%)"
                    },
                    desiredWidth: 300,
                    arrow: {
                        ...arrowPosition.topMiddle,
                        left: "100%",
                        transform: "translateX(-252%)"
                    },
                    customLogic: (context) => {
                        setCreateButtonOrientationListener()
                    }
                }),
                async () => ({
                    headerText: "Navigate to a create page",
                    tooltipMode: true,
                    element: ".create-content-dropdown",
                    content: await fetchOrientationStep("Choose-New-Issue-Or-Solution-Orientation"),
                    position: {
                        ...modalPosition.bottom,
                        transform: "translate(-52%, 44%)"
                    },
                    arrow: arrowPosition.topMiddle,
                    isDeadEndStep: true,
                    preConditionLogic: () => {
                        confirmCreateDropdownOpen()
                    },
                    customLogic: navigateToACreatePageLogic
                }),
                async () => ({
                    headerText: "This is a create page",
                    content: await fetchOrientationStep("Create-Page-Navigated-To-Orientation"),
                    isDeadEndStep: true,
                    elements: ['#createDropdown', ".parent-fieldset"], // <-- not valid. Was attempting to be able to have two "windows" in the backdrop, but was too complicated
                    customLogic: createContentPageLogic

                })
            ],
            userClicksShowHow_CreateViaContentPageWorkflow: [
                async () => ({
                    headerText: "Creating content via visiting a contents page",
                    element: ".view-btn",
                    elements: [".breadcrumb-ribbon-custom"],
                    tooltipMode: true,
                    position: {
                        ...modalPosition.bottom,
                        transform: "translate(-50%, 30%)"
                    },
                    arrow: arrowPosition.topMiddle,
                    content: "Learn more by clicking the view button of content, or by clicking a breadcrumb navigation item",
                    //isDeadEndStep: true,
                    desiredWidth: 300,
                    preConditionLogic: () => {
                        confirmViewBtn()
                    },
                    customLogic: () => {
                        setListenersOnViewContentPageLinks()
                    }
                }),
                async () => ({
                    headerText: "The tab bar offers a way to navigate sub content",
                    content: await fetchOrientationStep("Sub-Content-Tab-Bar-Orientation"),
                    element: ".sub-content-tab-bar",
                    tooltipMode: true,
                    position: modalPosition.top,
                    arrow: arrowPosition.downMiddle,
                    preConditionLogic: (context) => {
                        confirmIsContentPage(context)
                    },
                    customLogic: () => {
                        setListenerOnVisitCreatePageViaContentPage()
                    }
                }),
                //async () => ({
                //    headerText: "Here is an example Action button",
                //    content: "Give it a click",
                //    element: "#dynamic-actions .dropdown-item",
                //    tooltipMode: true,
                //    position: modalPosition.bottom,
                //    arrow: arrowPosition.topMiddle,
                //    preConditionLogic: () => {
                //        debugger
                //        // The sub-issue tab selected, and dropdown actions opened.
                //        const subIssueTab = document.querySelector(".sub-content-tab-bar .nav-tabs .nav-link#sub-issues-tab")
                //        subIssueTab.click()
                //        const dropdown = document.querySelector('#actionDropdown')
                //        if (!dropdown.classList.contains("show")) {
                //             dropdown.click()
                //        }


                //    }
                //}),
                async () => ({
                    headerText: "You navigated here via a content page",
                    content: await fetchOrientationStep("Create-Issue-Or-Solution-Via-ContentPageVisit"),
                    preConditionLogic: (context) => {
                          confirmIsCreatePageWithParentContentSelected(context)
                    },
                    tooltipMode: true,
                    position: isMobile() ? modalPosition.top : modalPosition.left,
                    arrow: isMobile() ? arrowPosition.downMiddle : arrowPosition.rightMiddle,
                    element: ".parent-fieldset",
                    desiredWidth: 300,
                    isDeadEndStep: true

                })
            ]
        }),
        async () => ({
            headerText: "Thank you for completing the orientation!",
            content: await fetchOrientationStep("End-Of-Orientation")
        })
    ]


    function animateIntroInspirationLink() {
        // Initialize angle variable
        let angle = 0;
        let speed = 1.5;


        // Function to animate the gradient
        function animateGradient() {
            // Increment angle and keep it between 0-360 (361 to be inclusive)
            angle = (angle + speed) % 361;
            const border = document.querySelector("#orientation-modal .fancy-border")

            // Update the CSS variable
            if (border) {
                border.style.setProperty('--gradient-angle', `-${angle}deg`);
            }
            // Request the next animation frame
            requestAnimationFrame(animateGradient);
        }

        // Start the animation
        animateGradient();
    }

    function confirmIsContentPage(context) {
        const isOnContentPage = location.pathname.match(new RegExp(/(\/issue|\/solution)/))
        if (!isOnContentPage) {
            document.querySelector(".view-btn").click()
        }
    }

    function confirmIsCreatePageWithParentContentSelected() {

        const isOnCreatePageWithParentContentSelected = location.href.match(new RegExp(/(\/create-issue|\/create-solution)\?parent(Issue|Solution)ID/))
        if (!isOnCreatePageWithParentContentSelected) {
            // Go back to root of workflow
            orientationStatus.workflowName = null
            orientationStatus.workflowStep = 0;
            orientationStatus.save()
            moveOrientationToStep(
                orientationStatus.step,
                context,
            );
        }
    }

    function setListenerOnVisitCreatePageViaContentPage() {

        const moveForwardOrientationStep = (e) => {
            e.preventDefault();
            orientationStatus.workflowStep = 2;
            orientationStatus.save()

            /// NO NEED TO MOVE TO STEP. It will happen on navigation
            //moveOrientationToStep(
            //    orientationStatus.step,
            //    context,
            //);
            const link = e.target
            // If you want to continue navigation after your logic:
            window.location.href = link.href;
        }

        const actionBtn = document.querySelector("#dynamic-actions .dropdown-item")
        actionBtn.addEventListener("click", moveForwardOrientationStep)

        //There is only ever one button in the dropdown

        const subContentTabs = document.querySelectorAll(".sub-content-tab-bar .nav-link")
        subContentTabs.forEach(tab => {

            tab.addEventListener('click', () => {
                const actionBtn = document.querySelector("#dynamic-actions .dropdown-item")
                // make sure to add listener on action button
                if (actionBtn) {
                    actionBtn.addEventListener("click", moveForwardOrientationStep)
                }
            })
        })
      
    }

    function setListenersOnViewContentPageLinks() {
        const viewContentLinks = document.querySelectorAll(".breadcrumb-ribbon-custom a, a.view-btn")
        const callback = (event) => {
            event.preventDefault(); // Stop default navigation
            // Your custom logic here
            // For example, update orientation, show a modal, etc.
            orientationStatus.workflowStep = 1;
            orientationStatus.save()

            const link = event.target.closest("a")
            // If you want to continue navigation after your logic:
            window.location.href = link.href;
        }
        viewContentLinks.forEach(link => {
            link.addEventListener("click", callback);
        });
    }
    function confirmViewBtn() {
        const viewButtonOnPage = document.querySelector(".view-btn")
        if (!viewButtonOnPage) {
            window.location.href = "/"
            // And propbably should confirm content filter is not set
        }
    }
    function createContentPageLogic() {
        // confirm is on
        const pathNames = ['/create-issue', '/create-solution']
        if (!pathNames.includes(location.pathname)) {
            window.location.href = "/create-issue"
        }

   
    }

    function navigateToACreatePageLogic() {

        // Set listener on the links to also update the orientation step.
        const createNavLinks = document.querySelectorAll(".create-content-dropdown a")
        createNavLinks.forEach(link => {
            link.addEventListener("click", function (event) {
                event.preventDefault(); // Stop default navigation
                // Your custom logic here
                // For example, update orientation, show a modal, etc.
                orientationStatus.workflowStep = 2;
                orientationStatus.save()

                // If you want to continue navigation after your logic:
                window.location.href = link.href;
            });
        });

    }
    function setListenersForCreateIssueAndSolutionStep(context) {
        const createButtonWorkflowBtn = document.querySelector("#create-button-custom-orientation-step")

        const createButtonClick = () => {
            orientationStatus.workflowName = "userClicksShowHow_CreateButtonWorkflow"
            orientationStatus.workflowStep = 0;
            orientationStatus.save()
            moveOrientationToStep(
                orientationStatus.step,
                context,
            );
            // after running the event, remove it
            createButtonWorkflowBtn.removeEventListener("click", createButtonClick)
        }

        createButtonWorkflowBtn.addEventListener("click", createButtonClick)

        const createViaContentPageWorkflowBtn = document.querySelector("#view-content-to-create-custom-orientation-step")
        const createViaView = () => {
            orientationStatus.workflowName = "userClicksShowHow_CreateViaContentPageWorkflow"
            orientationStatus.workflowStep = 0;
            orientationStatus.save()
            moveOrientationToStep(
                orientationStatus.step,
                context,
            );

            // after running the event, remove it
            createViaContentPageWorkflowBtn.removeEventListener("click", createViaView)
        }
        createViaContentPageWorkflowBtn.addEventListener("click", createViaView)
    }

    function setCreateButtonOrientationListener() {
        // If user clicks create, go to next step.
        const createBtn = document.querySelector("#createDropdown")

        const createBtnClick = () => {
            orientationStatus.workflowStep = 1; // This is the 2nd step in this workflow (Navigate to a create page)
            orientationStatus.save()
            moveOrientationToStep(
                orientationStatus.step,
                context,
            );

            // after running the event, remove it
            createBtn.removeEventListener("click", createBtnClick)
        }

        createBtn.addEventListener("click", createBtnClick)
        context.buttons.workflowBackOrientationButton.addEventListener("click", () => {
            createBtn.removeEventListener("click", createBtnClick)
        })

    }

    function confirmCreateDropdownOpen() {
        const createBtnNotShown = document.querySelector("#createDropdown:not(.show)")
        if (createBtnNotShown) {
            createBtnNotShown.click()
            setTimeout(() => {
                moveOrientationToStep(
                    orientationStatus.step,
                    context,
                );
            }, 200)
        }
    }

    function confirmContentFilterSelected() {
        const filterToggle = document.querySelector(".toggle-option[for=filter]")
        filterToggle.click();
    }

    function setListenerOnSideBarToggles() {
        const inputs = document.querySelectorAll(".view-mode-toggle input[name=view-mode]")
        const update = () => {
            backdrop._scrollListener_backgroundWindow_wTimeout()
        }
        inputs.forEach(input => {

            input.removeEventListener("click", update)
            input.addEventListener("click", update)
        })

        // on next: remove this listener
        buttons.stepForwardOrientationButton.addEventListener("click", () => {
            inputs.forEach(input => {
                input.removeEventListener("click", update)
            })
        })

    }

    function confirmSideBarOpen() {
        // Make sure the sidebar is open
        // This is slightly tricky because of how the sidebar works.
        // When the screen is larger sidebarState(open||closed) is in localstorage
        // But in mobile view the screen always collapses to hidden no matter the localStorage state.
        // But that state is useful for if the user expands their browser window.
        if (isMobile()) {
            // make sure #left-sidebar-container has class sidebar-open
            const sidebar = document.querySelector("#left-sidebar-container")
            if (!sidebar.classList.contains("sidebar-open")) {
                sidebar.classList.add("sidebar-open")
                setTimeout(() => {
                    moveOrientationToStep(context.orientationStatus.step, context);
                }, 500)
            }
        } else {
            const closedSideBarBtn = document.querySelector(".left-sidebar-toggle.sidebar-closed")
            if (closedSideBarBtn) {
                closedSideBarBtn.click();
                setTimeout(() => {
                    moveOrientationToStep(context.orientationStatus.step, context);
                }, 500)
            }
        }
    }



    /**
     * These steps are onses triggered by 
     * user interactions with the website.
     * 
     */
    const customSteps = {
        userVotedDuringOrientation: async () => ({
            headerText: "You just casted a vote during orientation",
            tooltipMode: true,
            element: '#confirmVoteModal .modal-content',
            content: await fetchOrientationStep("Vote-Cast-During-Orientation"),
            position: modalPosition.top,
            arrow: arrowPosition.downMiddle,
            isDeadEndStep: true
        }),
        userOpensContentVoteModal: async () => ({
            element: "#userVoteModal .modal-content",
            hideModal: true,
            isDeadEndStep: true,
            customLogic: () => {
                const modal = document.querySelector("#userVoteModal")
                const footer = modal.querySelector('.modal-footer')
                footer.insertAdjacentHTML("afterbegin", `<p>Good job testing that button<p>`)
                const closeBtn = footer.querySelector(".btn")
                closeBtn.innerText = "Go Back to Orientation"

                // Add listener for modal close event
                modal.addEventListener('hidden.bs.modal', function () {
                    // Code to run when modal is closed
                    moveOrientationToStep(context.orientationStatus.step, context);
                });
            }
        }),

    }


    return { steps, customSteps }

}