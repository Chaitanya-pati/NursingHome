
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