﻿// calendar.js

var months = [
    "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
];

function makeEditable(element, widthValue, number, typeValue) {
    // Create an input element
    var inputElement = document.createElement("input");
    inputElement.type = "text";
    inputElement.value = element.innerText;

    if (typeValue === "day") {
        inputElement.value = inputElement.value.slice(0, -1); // Remove comma from day
    }

    inputElement.pattern = `\\d{${number}}`;
    inputElement.style.width = widthValue;

    // Replace the span with the input element
    element.replaceWith(inputElement);

    // Focus on the input field
    inputElement.focus();

    // Attach event listeners to handle input restrictions and updating
    inputElement.addEventListener("input", function () {

        // Limit the length to # digits 
        if (inputElement.value.length > number) {
            inputElement.value = inputElement.value.substring(0, number);
        }
    });

    inputElement.addEventListener("blur", function () {
        // Perform any additional validation or processing here before updating
        // For simplicity, let's assume any entered value is a valid year

        // Update the displayed value
        var newValue = inputElement.value;

        // Create a span element without the comma initially
        var spanElement = document.createElement("span");
        spanElement.id = `editable${typeValue.charAt(0).toUpperCase() + typeValue.slice(1)}`;
        spanElement.onclick = function () { makeEditable(this, widthValue, number, typeValue); };
        spanElement.style.cursor = "pointer";

        // Check if the value is valid
        if (typeValue === "year") {
            newValue = newValue.replace(/\D/g, '');  // Remove non-numeric characters
            if (newValue === "") {
                newValue = new Date().getFullYear();  // Default to this year
            }
        } else if (typeValue === "day") {
            newValue = newValue.replace(/\D/g, '');  // Remove non-numeric characters

            if (isNaN(newValue) || newValue === "") {
                newValue = new Date().getDate();  // Default to this year

            }

            if (newValue < 1) {
                // Invalid day, set it to default (1)
                newValue = 1;
            }

            var maxDaysInMonth = new Date(parseInt(document.getElementById("editableYear").innerText), months.indexOf(document.getElementById("editableMonth").innerText) + 1, 0).getDate();
            if (newValue > maxDaysInMonth) {
                // Invalid day, set it to default (1)
                newValue = maxDaysInMonth;
            }

            newValue += ",";  // Add the comma to day
        }

        spanElement.innerText = newValue;

        // Replace the input with the span
        inputElement.replaceWith(spanElement);

        updateServer();
    });

    // Handle pressing Enter key
    inputElement.addEventListener("keydown", function (event) {
        if (event.key === "Enter") {
            // Trigger the blur event when Enter is pressed
            inputElement.blur();
        }
    });
}

function updateServer() {
    console.log("updateServer");

    document.getElementById("editableDay").innerText = document.getElementById("editableDay").innerText.slice(0, -1); // Remove comma from day
    var maxDaysInMonth = new Date(parseInt(document.getElementById("editableYear").innerText), months.indexOf(document.getElementById("editableMonth").innerText) + 1, 0).getDate();
    if (parseInt(document.getElementById("editableDay").innerText) > maxDaysInMonth) {
        // Invalid day, set it to default (1)
        console.log("invalid day", document.getElementById("editableDay").innerText, " ", maxDaysInMonth);
        document.getElementById("editableDay").innerText = maxDaysInMonth;
    }
    var day = document.getElementById("editableDay").innerText;
    document.getElementById("editableDay").innerText += ",";  // Add the comma to day

    $.ajax({
        type: "POST",
        url: updateDateUrl,
        data: JSON.stringify({
            MonthAJAX: months.indexOf(document.getElementById("editableMonth").innerText) + 1,
            DayAJAX: day,
            YearAJAX: parseInt(document.getElementById("editableYear").innerText),
        }), contentType: 'application/json',
        headers: {
            RequestVerificationToken:
                $('input:hidden[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            console.log("Update successful", response);
        },
        error: function (error) {
            console.error("Error updating server", error);
        }
    });
}


function cycleMonth(direction) {
    var currentMonth = document.getElementById("editableMonth").innerText;
    var currentIndex = months.indexOf(currentMonth);

    // Cycle through months in the specified direction
    var newMonthIndex = (currentIndex + direction + months.length) % months.length;
    var newMonth = months[newMonthIndex];

    // Update the displayed month
    document.getElementById("editableMonth").innerText = newMonth;

    updateServer();
}