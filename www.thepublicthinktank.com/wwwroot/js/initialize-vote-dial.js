function initializeVoteDial(forumId) {
    console.log("Vote dial initialized for forum ID:", forumId);
    const containerId = `vote-toggle-container-${forumId}`;
    const container = document.getElementById(containerId);
    const dialId = `vote-dial-${forumId}`;

    // Find the checked radio button
    const checkedRadio = document.querySelector(`input[name="${dialId}"]:checked`);

    if (!container) return;

    // Store all option elements and radios
    const options = Array.from(container.querySelectorAll('.toggle-option'));
    const radios = Array.from(document.querySelectorAll(`input[name="${dialId}"]`));

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
    
    // Function to save vote to server with debouncing
    const saveVote = debounce(function(voteValue) {
        console.log(`Saving vote ${voteValue} for forum ${forumId}`);
        
        const formData = new FormData();
        formData.append('ForumID', forumId);
        formData.append('VoteValue', voteValue);
        
        fetch('/Forum/Vote', {
            method: 'POST',
            body: formData,
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        })
            .then(async (response) => {
                if (!response.ok) {
                    throw new Error("Vote not casted");
                }
                return await response.json();
            })
        .then(data => {
            console.log('Vote saved successfully:', data);
            // Optionally update the UI based on response

              // Add the user-voted class
            container.classList.add('user-voted');

            // update the vote average and the count
            const averageElement = document.querySelector(`#vote-average-${forumId}`);
            const countElement = document.querySelector(`#vote-count-${forumId}`);

            if (averageElement && data.average !== undefined) {
                averageElement.textContent = Number.isInteger(data.average) ? data.average.toString() : data.average.toFixed(1);
            }

            if (countElement && data.count !== undefined) {
                countElement.textContent = data.count;
            }

        })
        .catch(error => {
            console.error('Error saving vote:', error);
        });
    }, 800); // 800ms debounce delay

    // Set up intersection observer to detect which option is most visible
    let isScrolling = false;
    let scrollTimeout;

    const observer = new IntersectionObserver((entries) => {
        if (!isScrolling) return;

        let maxVisibility = 0;
        let mostVisibleEntry = null;

        entries.forEach(entry => {
            const visibleRatio = entry.intersectionRatio;
            if (visibleRatio > maxVisibility) {
                maxVisibility = visibleRatio;
                mostVisibleEntry = entry;
            }
        });

        if (mostVisibleEntry && maxVisibility > 0.99) {
            const label = mostVisibleEntry.target;
            const radioId = label.getAttribute('for');
            const radio = document.getElementById(radioId);

            const value = parseInt(radio.value);
            if (radio && !radio.checked && value >= 0 && value <= 10) {
                radio.checked = true;
                const event = new Event('change');
                radio.dispatchEvent(event);
            }
        }
    }, {
        root: container,
        threshold: [0, 0.25, 0.5, 0.75, 1],
        rootMargin: '-10% 0px -10% 0px'
    });

    options.forEach(option => {
        observer.observe(option);
    });

    if (checkedRadio) {
        const radioId = checkedRadio.id;
        const label = document.querySelector(`label[for="${radioId}"]`);

        if (label) {
            const labelTop = label.offsetTop;
            const containerHeight = container.clientHeight;
            const labelHeight = label.clientHeight;

            container.scrollTop = labelTop - (containerHeight / 2) + (labelHeight / 2);
        }
    }

    container.addEventListener('scroll', () => {
        isScrolling = true;
        clearTimeout(scrollTimeout);
        scrollTimeout = setTimeout(() => {
            isScrolling = false;
            const checkedRadio = document.querySelector(`input[name="${dialId}"]:checked`);
            if (checkedRadio) {
                const label = document.querySelector(`label[for="${checkedRadio.id}"]`);
                if (label) {
                    const labelTop = label.offsetTop;
                    const containerHeight = container.clientHeight;
                    const labelHeight = label.clientHeight;

                    container.scrollTo({
                        top: labelTop - (containerHeight / 2) + (labelHeight / 2),
                        behavior: 'smooth'
                    });
                }
            }
        }, 150);
    });

    radios.forEach(radio => {
        radio.addEventListener('change', function () {
            if (this.checked) {
                const value = parseInt(this.value);
                
                // Only save valid vote values (0-10)
                if (value >= 0 && value <= 10) {
                    saveVote(value);
                }
                
                const label = document.querySelector(`label[for="${this.id}"]`);
                if (label) {
                    const labelTop = label.offsetTop;
                    const containerHeight = container.clientHeight;
                    const labelHeight = label.clientHeight;

                    isScrolling = false;

                    container.scrollTo({
                        top: labelTop - (containerHeight / 2) + (labelHeight / 2),
                        behavior: 'smooth'
                    });
                }
            }
        });
    });
}