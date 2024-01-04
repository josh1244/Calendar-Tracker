// calendar.js

let months = [
    "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
];

let currentIndex = null

//Setup current week at start
window.onload = function () {
    getLongMonthNamesSetting();
    nextWeek(0);
}
function makeEditable(element, widthValue, number, typeValue) {
    // Create an input element
    var inputElement = document.createElement("input");
    inputElement.type = "text"; //Maybe change to number?
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
            if (newValue.length < 4) {
                newValue = 1000;  // Default 1000
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

function cycleMonth(direction) {
    var currentMonth = document.getElementById("editableMonth").innerText;
    var currentIndex = months.indexOf(currentMonth);

    // Increase Year if going from Dec to Jan. Could make this optional with setting
    //if (currentIndex == 11) document.getElementById("editableYear").innerText ++;

    // Cycle through months in the specified direction
    var newMonthIndex = (currentIndex + direction + months.length) % months.length;
    var newMonth = months[newMonthIndex];

    // Update the displayed month
    document.getElementById("editableMonth").innerText = newMonth;


    updateServer();
}

function updateServer() {
    console.log("updateServer");

    // Remove comma from day
    document.getElementById("editableDay").innerText = document.getElementById("editableDay").innerText.slice(0, -1); 

    // Get max numbers of day allowed in that month on that year
    var maxDaysInMonth = new Date(parseInt(document.getElementById("editableYear").innerText), months.indexOf(document.getElementById("editableMonth").innerText) + 1, 0).getDate();

    // Check is day of month is higher than max 
    if (parseInt(document.getElementById("editableDay").innerText) > maxDaysInMonth) {
        // Invalid day, set it to max allowed
        console.log("invalid day", document.getElementById("editableDay").innerText, " ", maxDaysInMonth);
        document.getElementById("editableDay").innerText = maxDaysInMonth;
    }

    // put day to variable before we add comma back to it.
    var day = document.getElementById("editableDay").innerText;

    // Add the comma to day
    document.getElementById("editableDay").innerText += ",";  

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
        success: function (data) {
            if (data.success) {
                // Update the table with the new data
                updateTable(data.days);
            } else {
                console.error("Update failed.");
            }
        },
        error: function (error) {
            console.error("Error updating server", error);
        }
    });
}

function updateTable(response) {
    console.log("UpdateTable function invoked");

    var tableBody = $('#weekTable');
    tableBody.empty(); // Clear existing rows

    // Create a new row for the week
    var newRow = $('<tr></tr>');

    // Iterate through each day in the 'weeks.days' array and add a cell to the row
    $.each(response, function (dayIndex, day) {
        // Create a new cell
        var cell = $('<td></td>');

        // Set the cell content
        cell.text(day);

        // Add a click event handler to the cell
        cell.on("click", function () {
            // Handle the click event
            handleCellClick(dayIndex, day);
        });

        // If day is the current day, then highlight it
        if (day == document.getElementById("editableDay").innerText.slice(0, -1)) {
            cell.addClass('selected-cell');
            currentIndex = dayIndex;
        }

        // If day is in a different month, then make it grey
        console.log(Math.abs(day - document.getElementById("editableDay").innerText.slice(0, -1)));
        if (Math.abs(day - document.getElementById("editableDay").innerText.slice(0, -1)) >= 10 ) {
            cell.addClass('grey-cell');
        }


        // Append the cell to the row
        newRow.append(cell);
    });

    console.log("hello");

    // Append the new row to the table body
    tableBody.append(newRow);
}

function handleCellClick(index, selectedDay) {
    // Handle the cell click event, e.g., update the date
    //console.log("Cell clicked: " + selectedDay);
    //console.log("currentIndex: " + currentIndex);
    //console.log("selectedIndex: " + index);

    // Calculate the difference in indices
    let dayDifference = index - currentIndex;
    nextWeek(dayDifference);
}

function nextWeek(number) {
    console.log("nextWeek function invoked");
    $.ajax({
        type: "POST",
        url: nextWeekUrl,
        data: JSON.stringify({ Days: number }), // Wrap the number in an object
        contentType: "application/json", // Set content type to JSON
        headers: {
            RequestVerificationToken:
                $('input:hidden[name="__RequestVerificationToken"]').val()
        },
        success: function (data) {
            if (data.success) {
                // Send Month, Day, Year from c# to updateDate
                updateDate(data.month, data.day, data.year, data.days);
            } else {
                console.error("Update failed.");
            }
        },
        error: function (error) {
            // Handle errors
            console.error(error);
        }
    });
}

function updateDate(month, day, year, days) {
    console.log("updateDate function invoked");

    //Access month, day year in html and update them to c# values
    document.getElementById("editableMonth").innerText = months[month - 1];
    document.getElementById("editableDay").innerText = day;
    document.getElementById("editableDay").innerText += ",";  // Add comma to it
    document.getElementById("editableYear").innerText = year;

    // Update the table with new day values
    updateTable(days);
}

// Function to get the value of LongMonthNames from the XML file
function getLongMonthNamesSetting() {
    $.ajax({
        type: "POST",
        url: loadSettingsUrl,
        headers: {
            RequestVerificationToken:
                $('input:hidden[name="__RequestVerificationToken"]').val()
        },
        success: function (data) {
            if (data.success) {
                // Send Month, Day, Year from c# to updateDate
                console.log(`Long Month Names is "${longMonthNamesValue}"`);
                if (data.longMonthNamesValue) {
                    // Replace abbreviated month names with full month names
                    months = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
                }
            } else {
                console.error("Update failed.");
            }
        },
        error: function (error) {
            // Handle errors
            console.error(error);
        }
    });
}