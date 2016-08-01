﻿function setDebugInfo(model) {
    console.log(model);
    var html = '';
    if (model.IsError == true) {
        html = '<br /><strong>Erreur :</strong><br />';
        html += model.ErrorMsg;
        html += '<br />';
        html += model.ErrorDetails;
        html += '<br />';
    } else
    {
        html = model.ResultText;
        html += '<br />';
    }
    $('#DebugZone').prepend(html);
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