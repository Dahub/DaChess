function writeTime(time, party) {
    var text = extractText(time);

    if (party.WhitePlayerState === playerStates.canMove) {
        $('#timeWhite').html(text);
    }
    else {
        $('#timeBlack').html(text);
    }
};

function extractText(time) {
    var h = parseInt(time / 3600);
    var m = parseInt((time % 3600) / 60);
    var s = time % 60;
    return twoDigits(h) + ":" + twoDigits(m) + ":" + twoDigits(s);
}

function initTime(whitePlayertime, blackPlayerTime, party) {
    var text = extractText(whitePlayertime);
    $('#timeWhite').html(text);
    text = extractText(blackPlayerTime);
    $('#timeBlack').html(text);
}

function launchCountDown(startTime) {    
    clearTimeout(countDown);
    time = startTime;
    refreshCountDown();
    countDown = setInterval("refreshCountDown()", 1000);
}

function refreshCountDown() {
    if (time <= 0) {
        clearInterval(countDown);
        if (isWhite && party.WhitePlayerState === playerStates.canMove
            || isBlack && party.BlackPlayerState === playerStates.canMove) {
            $.ajax({
                url: '/Party/TimeOver',
                data: { partyName: party.Name, token: playerToken },
                cache: false,
                async: false,
                type: 'GET',
                dataType: "json",
                contentType: 'application/json; charset=utf-8'
            }).done(function (data) {
                $.connection.partyHub.server.aPlayerTimeIsExpired(party.Name);
                $.connection.partyHub.server.newInfo(party.Name, data.ResultText);
            });
        }
    }
    else {
        writeTime(time, party);
        time--;
    }
}

function stopCountDown() {
    clearTimeout(countDown);
}