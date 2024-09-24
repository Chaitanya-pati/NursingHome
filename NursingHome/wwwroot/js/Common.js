
function GetFormatedDateValue(now) {
    var curr_day = getFormattedValue(now.getDate());
    var curr_month = getFormattedValue((now.getMonth() + 1));
    var curr_year = now.getFullYear();
    var curr_hours = now.getHours();
    var curr_minutes = now.getMinutes();
    var startdate = curr_year + '-' + curr_month + '-' + curr_day + "T" + (curr_hours <= 9 ? "0" + curr_hours : curr_hours) + ":" + (curr_minutes <= 9 ? "0" + curr_minutes : curr_minutes);
    return startdate;
}
function getFormattedValue(para) {
    return para < 10 ? "0" + para : para;
}


function GetFormattedStartDate(now) {
    var curr_day = getFormattedStartValue(now.getDate());
    var curr_month = getFormattedStartValue(now.getMonth() + 1); // Months are 0-based in JavaScript
    var curr_year = now.getFullYear();

    // Set hours and minutes to 0 for 12:00 AM
    var curr_hours = 0;
    var curr_minutes = 0;

    // Format hours and minutes to two digits
    var formatted_hours = curr_hours <= 9 ? "0" + curr_hours : curr_hours;
    var formatted_minutes = curr_minutes <= 9 ? "0" + curr_minutes : curr_minutes;

    // Create the formatted date string
    var startdate = curr_year + '-' + curr_month + '-' + curr_day + "T" + formatted_hours + ":" + formatted_minutes;

    return startdate;
}

// Helper function to format date and month values
function getFormattedStartValue(value) {
    return value <= 9 ? "0" + value : value;
}
function formatDate(date) {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0'); // Months are zero-based
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
}

function ConvertToBase64(input) {
    return new Promise((resolve, reject) => {
        if (input.files && input.files[0]) {
            var reader = new FileReader();
            reader.onload = function (e) {
                var base64String = input.files[0].type + "~" + e.target.result.replace(/^data:.+\/(.+);base64,/, "");

                resolve(base64String);
            }
            reader.onerror = function (error) {
                reject(error);
            }
            reader.readAsDataURL(input.files[0]);
        } else {
            reject("No file selected");
        }
    });
}

function msgPopup(icons = 'success' | 'info' | 'error' | 'warning' | 'question', titles) {
    const Toast = Swal.mixin({
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: 3000
    });
    Toast.fire({
        icon: icons,
        title: titles
    });
}
function getTableDataString() {
    const table = document.getElementById('dynamicTable');
    const rows = table.querySelectorAll('tbody tr');
    const rowData = [];
    rows.forEach(row => {
        const cells = row.querySelectorAll('td');
        const cellData = [];

        cells.forEach((cell, index) => {
            let value = "";
            value = cell.children[0].value;
            cellData.push(value);
        });

        rowData.push(cellData.join('|'));
    });



    return rowData.join('~');
}

function GetOtherPeopleDetailsString() {
    const container = document.getElementById('personDetailsContainer');
    const table = container.querySelector('table');
    if (!table) {
        return ''; // Return an empty string if no table exists
    }

    const rows = table.querySelectorAll('tbody tr');
    const rowData = [];

    rows.forEach(row => {
        const cells = row.querySelectorAll('td');
        const cellData = [];

        cells.forEach(cell => {
            const input = cell.querySelector('input');
            const value = input ? input.value : ''; // Get the input value or empty string if input is not found
            cellData.push(value);
        });

        rowData.push(cellData.join('|'));
    });

    return rowData.join('~');
}
function getCurrentDate() {
    // Returns today's date in YYYY-MM-DD format
    const today = new Date();
    const year = today.getFullYear();
    const month = String(today.getMonth() + 1).padStart(2, '0'); // Months are 0-based
    const day = String(today.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
}

function timeToDate(time) {
    if (!time) return null;
    const currentDate = getCurrentDate();
    return new Date(`${currentDate}T${time}`); // Combine date and time
}
function formatDateToTimeString(date) {
    if (!date) return '';
    let hours = date.getHours().toString().padStart(2, '0');
    let minutes = date.getMinutes().toString().padStart(2, '0');
    return `${hours}:${minutes}`;
}
function generatePersonTable(numOfPersons, dataString = '') {
    var container = $('#personDetailsContainer');
    container.empty();

    if (numOfPersons > 1) {
        var table = $('<table class="table table-bordered"><thead><tr><th>Person Name</th><th>Phone Number</th></tr></thead><tbody></tbody></table>');
        var tbody = table.find('tbody');

        // Parse the dataString
        var data = dataString.split('~').map(entry => {
            var [name, phone] = entry.split('|');
            return { name: name || '', phone: phone || '' };
        });

        // Ensure data length matches numOfPersons, fill with empty objects if necessary
        while (data.length < numOfPersons - 1) {
            data.push({ name: '', phone: '' });
        }

        // Generate table rows
        for (var i = 0; i < numOfPersons - 1; i++) {
            var row = $('<tr></tr>');
            var nameInput = $('<input type="text" class="form-control" placeholder="Enter Person" />').attr('name', 'personName' + (i + 1)).attr('placeholder','Enter Person ' +( i+2 )+' Name');
            var phoneInput = $('<input type="text" class="form-control" />').attr('name', 'phoneNumber' + (i + 1)).attr('placeholder', 'Enter Person ' + (i + 2) + ' Phone Number');

            // Populate input fields with data
            if (data[i]) {
                nameInput.val(data[i].name || '');
                phoneInput.val(data[i].phone || '');
            }

            row.append($('<td></td>').append(nameInput));
            row.append($('<td></td>').append(phoneInput));
            tbody.append(row);
        }

        container.append(table);
    }
}

function checkrange(startid,endid) {
    var startDate = $("#" + startid).val();
    var endDate = $("#" + endid).val();


    if (startDate == '' || endDate == '') {
        return false;
    }
    else {
        var startTimestamp = Date.parse(startDate);
        var endTimestamp = Date.parse(endDate);
        if (isNaN(startTimestamp) || isNaN(endTimestamp)) {
            return false;
        }
        var minDate = Date.parse("1/1/1753 12:00:00 AM");
        var maxDate = Date.parse("12/31/9999 11:59:59 PM");

        if (startTimestamp < minDate || endTimestamp > maxDate) {
            return false;
        }
        if (endTimestamp <= startTimestamp) {
            $('#daterangealert').show();
            $('#search').prop('disabled', true);
            return false;
        } else {
            $('#daterangealert').hide();
            $('#search').prop('disabled', false);
            return true;
        }
    }
}

function alertMessage(msg) {
    $.alert({
        title: 'Alert',
        content: msg,
        typeAnimated: true,
        backgroundDismiss: false,
        backgroundDismissAnimation: 'glow',
        boxWidth: '30%',
        useBootstrap: false,
        buttons: {
            Yes: {
                text: 'Ok',
                btnClass: 'btn btn-success',
                action: function () {
                    return true;
                },
            },
        },
    });
}
function downloadFile(base64String, fileType, fileName) {
    const link = document.createElement('a');
    link.href = `data:${fileType};base64,${base64String}`;
    link.download = fileName + fileType.split("/")[1];
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}
function DeleteWarning(methodname,id) {
    $.alert({
        title: 'Alert',
        content: 'Are you sure want to delete ?',
        typeAnimated: true,
        backgroundDismiss: false,
        backgroundDismissAnimation: 'glow',
        boxWidth: '30%',
        useBootstrap: false,
        buttons: {
            Yes: {
                text: 'Yes',
                btnClass: 'btn btn-success',
                action: function () {
                    methodname(id);
                },
            },
            No: {
                text: 'No',
                btnClass: 'btn btn-warning',
                action: function () {
                    return;
                },
            },
        },
    });
}

function validatePhoneNumber(phoneNumber) {
    // Regular expression to check for 10 digits
    var phoneRegex = /^[0-9]{0,10}$/;
    return phoneRegex.test(phoneNumber);

}
// Event listener on keypress for the phone number validation
$(document).ready(function () {
    $('.PhonenumberValidation').on('input', function () {
        var $inputField = $(this);
        var phoneNumber = $inputField.val();

        // Remove any existing warning message
        $inputField.next('.error-message').remove();

        // Check if phone number is valid
        if (!validatePhoneNumber(phoneNumber)) {
            // Append warning message if the format is wrong
            $inputField.after('<small class="error-message" style="color:red;">Please enter a valid phone number (10 digits only)</small>');
        }
    });
    //For Sugnature 
    const canvases = document.getElementsByClassName('signatureContainer');

    // Loop through each canvas element and add event listeners for drawing
    Array.from(canvases).forEach((canvas) => {
        const ctx = canvas.getContext('2d');
        let isDrawing = false;

        // Helper function to get coordinates for touch or mouse events
        function getCoordinates(e) {
            const rect = canvas.getBoundingClientRect(); // Get canvas position
            let x, y;

            if (e.touches && e.touches.length > 0) {
                x = e.touches[0].clientX - rect.left;
                y = e.touches[0].clientY - rect.top;
            } else {
                x = e.clientX - rect.left; // Use clientX instead of offsetX
                y = e.clientY - rect.top;   // Use clientY instead of offsetY
            }

            return { x: x * (canvas.width / rect.width), y: y * (canvas.height / rect.height) };
        }

        // Event listener for starting the drawing (mouse + touch)
        const startDrawing = (e) => {
            isDrawing = true;
            const { x, y } = getCoordinates(e);
            ctx.beginPath();
            ctx.moveTo(x, y);
        };

        // Event listener for drawing as the mouse or touch moves
        const draw = (e) => {
            if (!isDrawing) return;
            const { x, y } = getCoordinates(e);
            ctx.lineTo(x, y);
            ctx.stroke();
            e.preventDefault(); // Prevent scrolling during touch
        };

        // Event listener for stopping the drawing
        const stopDrawing = () => {
            isDrawing = false;
        };

        // Mouse event listeners
        canvas.addEventListener('mousedown', startDrawing);
        canvas.addEventListener('mousemove', draw);
        canvas.addEventListener('mouseup', stopDrawing);
        canvas.addEventListener('mouseout', stopDrawing);

        // Touch event listeners
        canvas.addEventListener('touchstart', startDrawing);
        canvas.addEventListener('touchmove', draw);
        canvas.addEventListener('touchend', stopDrawing);
        canvas.addEventListener('touchcancel', stopDrawing);
    });

});

   
function clearSignature(id) {
    var canvas = document.getElementById(id);
    if (canvas) {
        var ctx = canvas.getContext('2d');
        ctx.clearRect(0, 0, canvas.width, canvas.height);
    }
}