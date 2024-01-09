// calendar.js

if (typeof months === 'undefined') {
    var months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
}
var currentIndex = null;

//Setup everything when loaded.
$(function () {
    getLongMonthNamesSetting(function () {
        nextWeek(0);
    });
});

function getLongMonthNamesSetting(callback) {
    // Function to get the value of LongMonthNames from the XML file
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
                if (data.longMonthNamesValue) {
                    // Replace abbreviated month names with full month names
                    months = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
                } else {
                    months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
                }
                // Call the provided callback function to continue the initialization
                if (typeof callback === 'function') {
                    callback();
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

function handleCellClick(index, selectedDay) {
    // Handle the cell click event by finding the difference in indices and progressing the week

    // Calculate the difference in indices
    let dayDifference = index - currentIndex;
    nextWeek(dayDifference); // Progress week by difference
}

function nextWeek(number) {
    //Progress week by number. ex. Back 5 days.
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
    // Access month, day year in html and update them to c# values
    document.getElementById("editableMonth").innerText = months[month - 1];
    document.getElementById("editableDay").innerText = day;
    document.getElementById("editableDay").innerText += ",";  // Add comma to it
    document.getElementById("editableYear").innerText = year;

    // Update the table with new day value
    updateTable(days);
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
        url: UpdateServerUrl,
        data: JSON.stringify({
            MonthAJAX: months.indexOf(document.getElementById("editableMonth").innerText) + 1,
            DayAJAX: parseInt(document.getElementById("editableDay").innerText.slice(0, -1)),
            YearAJAX: parseInt(document.getElementById("editableYear").innerText),
        }),
        contentType: 'application/json',
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
    //console.log(response);
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

        // If day is in a different month, then make it grey;
        if (Math.abs(day - document.getElementById("editableDay").innerText.slice(0, -1)) >= 10 ) {
            cell.addClass('grey-cell');
        }


        // Append the cell to the row
        newRow.append(cell);
    });


    // Append the new row to the table body
    tableBody.append(newRow);

    updateTrackers();
}

function updateTrackers() {
    $.ajax({
        type: "POST",
        url: LoadTrackersUrl,
        data: JSON.stringify({
            MonthAJAX: months.indexOf(document.getElementById("editableMonth").innerText) + 1,
            DayAJAX: parseInt(document.getElementById("editableDay").innerText.slice(0, -1)),
            YearAJAX: parseInt(document.getElementById("editableYear").innerText),
        }),
        contentType: 'application/json',
        headers: {
            RequestVerificationToken:
                $('input:hidden[name="__RequestVerificationToken"]').val()
        },
        success: function (data) {
            if (data.success) {
                // Use data.trackers and data.trackersData to dynamically generate HTML
                if (data.trackers && Object.keys(data.trackers).length > 0 && data.trackersData) {
                    // Clear existing content
                    $('#trackersContainer').empty();

                    // Loop through trackers and generate HTML for Tracker Text
                    var trackerTextContainer = $('<div class="text-column">');
                    Object.keys(data.trackers).forEach(function (trackerId) {
                        var tracker = data.trackers[trackerId];

                        var trackerTextDiv = $('<div>');
                        var trackerTextIdInput = $('<input type="hidden" name="TrackersValues[' + trackerId + '].Id" class="tracker-id" value="' + tracker.id + '" />');
                        var trackerTextParagraph = $('<p class="text-column">' + tracker.name + '</p>');

                        // Append elements to the text container
                        trackerTextDiv.append(trackerTextIdInput);
                        trackerTextDiv.append(trackerTextParagraph);
                        trackerTextContainer.append(trackerTextDiv);
                    });

                    // Append Tracker Text container to main container
                    $('#trackersContainer').append(trackerTextContainer);

                    // Loop through trackers and generate HTML for Tracker Components
                    var trackerComponentsContainer = $('<div class="components-column">');
                    Object.keys(data.trackers).forEach(function (trackerId) {
                        var tracker = data.trackers[trackerId];
                        var trackerData = data.trackersData[trackerId];

                        var trackerComponentsDiv = $('<div class="components-container">');

                        // Additional logic to handle different tracker types and their values
                        switch (tracker.type) {
                            case "Slider":
                                var initialSliderValue = (trackerData && trackerData.sliderValue !== null) ? trackerData.sliderValue : "";
                                var sliderInput = $('<input id="' + tracker.name + '" name="TrackersValues[' + trackerId + '].SliderValue" type="range" min="0" max="10" class="tracker-component" value="' + initialSliderValue + '">');
                                var sliderOutput = $('<output style="width: 30px;" id="' + tracker.name + ' Output">' + initialSliderValue + '</output>');

                                if (!initialSliderValue) {
                                    sliderInput.addClass("grayout");
                                }

                                trackerComponentsDiv.append(sliderInput);
                                trackerComponentsDiv.append(sliderOutput);

                                // Attach input event listener for the slider
                                sliderInput.on("input", function () {
                                    $(this).removeClass("grayout");

                                    var trackerName = $(this).attr("id"); // Get the trackerName from the slider's ID
                                    var outputId = tracker.name + " Output";

                                    // Update the value displayed next to the slider dynamically
                                    document.getElementById(trackerName + " Output").innerHTML = $(this).val();
                                });
                                break;

                            case "Checkbox":
                                // Check if checkboxValue is true, otherwise default to false
                                var isChecked = (trackerData && trackerData.checkboxValue === true);
                                var checkboxInput = $('<input type="checkbox" id="' + tracker.name + '" name="TrackersValues[' + trackerId + '].CheckboxValue">');

                                // Set the 'checked' attribute based on the boolean value
                                if (isChecked) {
                                    checkboxInput.attr('checked', 'checked');
                                }

                                trackerComponentsDiv.append(checkboxInput);
                                break;

                            case "Text":
                                // Check if textValue is null, display default text
                                var textValue = (trackerData && trackerData.textValue !== null) ? trackerData.textValue : "";
                                var defaultTextValue = (tracker && tracker.defaultText !== null) ? tracker.defaultText : "";

                                var textInput = $('<input type="text" id="' + tracker.name + '" name="TrackersValues[' + trackerId + '].TextValue" placeholder="' + defaultTextValue + '" class="tracker-textbox" value="' + textValue + '">');
                                trackerComponentsDiv.append(textInput);
                                break;

                            case "Dropdown":
                                // Check if dropdownValue is null, display default text
                                var dropdownValue = (trackerData && trackerData.dropdownValue !== null) ? trackerData.dropdownValue : "";
                                var dropdownInput = $('<select id="' + tracker.name + '" name="TrackersValues[' + trackerId + '].DropdownValue" class="tracker-dropdown"></select>');

                                // Add options to the dropdown based on tracker.DropdownOptions
                                if (tracker && tracker.dropdownOptions && tracker.dropdownOptions.length > 0) {
                                    // Loop through each option in tracker.DropdownOptions and add it to the dropdown
                                    for (var i = 0; i < tracker.dropdownOptions.length; i++) {
                                        var option = $('<option value="' + tracker.dropdownOptions[i] + '">' + tracker.dropdownOptions[i] + '</option>');

                                        // Set the selected attribute based on the value
                                        if (tracker.dropdownOptions[i] === dropdownValue) {
                                            option.prop('selected', true);
                                        }

                                        dropdownInput.append(option);
                                    }
                                } else {
                                    // If no options are available, you can add a default option or leave it empty
                                    dropdownInput.append('<option value="" selected>Add in Settings</option>');
                                }

                                // Append the dropdown input to the trackerComponentsDiv
                                trackerComponentsDiv.append(dropdownInput);
                                break;

                            default:
                                trackerComponentsDiv.append($('<p>Tracker type undefined.</p>'));
                                break;
                        }

                        // Append Tracker Components div to the components container
                        trackerComponentsContainer.append(trackerComponentsDiv);
                    });

                    // Append Tracker Components container to main container
                    $('#trackersContainer').append(trackerComponentsContainer);
                } else {
                    // Handle case where no trackers are found
                    $('#trackersContainer').html('<p>No trackers found.</p>');
                }
            } else {
                console.error("Update failed.");
            }
        },
        error: function (error) {
            console.error("Error updating server", error);
        }
    });
}







