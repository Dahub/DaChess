function sendNewMsg(partyName) {
    var partyHub = $.connection.partyHub;
    partyHub.server.sendMessage(partyName, $('#displayname').val(), $('#message').val());
    $('#message').val('').focus();
}

function appendMessage(name, message) {
    if ($("#discussion li").length > 10) {
        $('#discussion li:last').remove();
    }
    $('#discussion').prepend('<li><strong>' + htmlEncode(name)
        + '</strong>: ' + htmlEncode(message) + '</li>');
}

function checkIfSendMsg(event, partyName) {
    if (event.keyCode === 13) {
        sendNewMsg(partyName);
    }
}