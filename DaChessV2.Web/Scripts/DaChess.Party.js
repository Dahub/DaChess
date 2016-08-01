function GetParty(partyName) {
    var myParty
    $.ajax({
        url: '/Party/GetParty',
        data: { partyName: partyName },
        cache: false,
        type: 'GET',
        dataType: "json",
        async: false,
        contentType: 'application/json; charset=utf-8'
    }).done(function (data) {
        setDebugInfo(data);
        myParty = data;
    });
    return myParty;
}

function ExtractBoard(myParty) {
    if (isEmpty(myParty.Board) === false)
        return JSON.parse(myParty.Board).board;
    else
        return '';
}

function ExtractHistory(myParty) {
    if (isEmpty(myParty.History) === false)
        return JSON.parse(myParty.History)
    else
        return '';
}