/**
 * JavaScript file for the entire site
 * 
 * Keep this file lean - Distribute logic in files for features
 * 
 * This file should contain only generic logic to be used anywhere in the app.
 */

/**
 * Creates a debounced version of a function
 * @param {Function} func - The function to debounce
 * @param {number} wait - The number of milliseconds to delay
 * @returns {Function} - The debounced function
 */
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


/**
 * Creates a throttled version of a function
 * @param {Function} func - The function to throttle
 * @param {number} limit - The number of milliseconds to wait
 * @returns {Function} - The throttled function
 */
function throttle(func, limit) {
    let lastFunc;
    let lastRan;
    return function () {
        const context = this;
        const args = arguments;
        if (!lastRan) {
            func.apply(context, args);
            lastRan = Date.now();
        } else {
            clearTimeout(lastFunc);
            lastFunc = setTimeout(function () {
                if ((Date.now() - lastRan) >= limit) {
                    func.apply(context, args);
                    lastRan = Date.now();
                }
            }, limit - (Date.now() - lastRan));
        }
    };
}


/**
 * Cookie utility functions
 */
function setCookie(name, value, days = 30, path = '/') {
    // Create a new Date object representing the expiration date
    const expires = new Date();
    // Set the expiration to `days` into the future (default: 30 days)
    expires.setTime(expires.getTime() + days * 24 * 60 * 60 * 1000);

    // Set the cookie string:
    // - name=value: stores the actual data
    // - encodeURIComponent: escapes special characters (e.g. JSON)
    // - expires: defines when the cookie will expire
    // - path: determines when the cookie is sent (default is `/`, meaning all pages)
    document.cookie = `${name}=${encodeURIComponent(value)};expires=${expires.toUTCString()};path=${path};Secure;SameSite=Lax`;
}
function getCookie(name) {
    const nameEQ = `${name}=`;
    const ca = document.cookie.split(';');
    for (let i = 0; i < ca.length; i++) {
        let c = ca[i];
        while (c.charAt(0) === ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) === 0) return decodeURIComponent(c.substring(nameEQ.length, c.length));
    }
    return null;
}



