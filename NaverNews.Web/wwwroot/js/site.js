// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function getCookie(name) {
    var value = "; " + document.cookie;
    var parts = value.split("; " + name + "=");
    if (parts.length === 2) {
        return parts.pop().split(";").shift();
    }
}

function setOffsetCookie() {
    // Get the local time offset in minutes
    var offsetMinutes = new Date().getTimezoneOffset() * -1;

    // Check if a cookie with the same value already exists
    var existingOffset = getCookie("offset");
    if (existingOffset !== undefined && parseInt(existingOffset) === offsetMinutes) {
        console.log("Offset cookie already exists with the same value:", existingOffset);
        return;
    }

    // Create a date object for one year from now
    var expirationDate = new Date();
    expirationDate.setFullYear(expirationDate.getFullYear() + 1);

    // Create the cookie string
    var cookieValue = "offset=" + offsetMinutes + "; expires=" + expirationDate.toUTCString() + "; path=/";

    // Set the cookie
    document.cookie = cookieValue;

    console.log("Offset cookie set:", cookieValue);
}

// Call the function to set the offset cookie
setOffsetCookie();
