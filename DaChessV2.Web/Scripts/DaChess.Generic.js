function setDebugInfo(model) {
    console.log(model);
}

function letterToNumber(letter) {
    return letter.charCodeAt(0) - 96;
}

function isEmpty(str) {
    return (!str || 0 === str.length);
}

function htmlEncode(value) {
    var encodedValue = $('<div />').text(value).html();
    return encodedValue;
}

function twoDigits(n) {
    return n > 9 ? "" + n : "0" + n;
}

function showEndModal(title, body) {
    $('#endModalTitle').html(title);
    $('#endModalBody').html(body);
    $('#endModal').modal('show');
}