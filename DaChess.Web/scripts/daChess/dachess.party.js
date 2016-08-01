function addPromoteValidation(partyName)
{
    $('#validPromote').click(function () {
        $.ajax({
            url: '/Party/MakePromote',
            data: { name: partyName, piece: $('#promotedChoise:checked').val(), token: $('#token').val() },
            cache: false,
            type: 'GET',
            dataType: "json",
            contentType: 'application/json; charset=utf-8'
        }).done(function (data) {
            $.connection.partyHub.server.newMove(partyName);
            if (data.IsError === true) {
                setMsg(data.ErrorMessage);
            }
            else {
                $.connection.partyHub.server.newInfo(partyName, data.InfoMessage);
            }
        });
    });
}

function initPartyInfo(partyName) {
    var p = { name: partyName };
    $.ajax({
        url: '/Party/Get',
        data: p,
        cache: false,
        type: 'GET',
        dataType: "json",
        contentType: 'application/json; charset=utf-8'
    }).done(function (data) {
        party = data;
        myBoard = JSON.parse(party.Board);
        refreshCanvas(myBoard.board);
        // on tente de voir si on a des données de cookies
        $.ajax({
            url: "/Player/GetPlayerPartyName",
            type: "get",
            success: function (dataPartyName) {
                if (dataPartyName === p.name) {
                    $.ajax({
                        url: "/Player/GetPlayerToken",
                        type: "get",
                        success: function (dataToken) {
                            $('#token').val(dataToken);

                            // on récupère la couleur via l'API
                            $.ajax({
                                url: '/Player/GetPlayerInfo',
                                data: { partyName: party.Name, token: dataToken },
                                cache: false,
                                type: 'GET',
                                dataType: "json",
                                contentType: 'application/json; charset=utf-8'
                            }).done(function (data) {
                                if (data.IsWhite === true)
                                    ImWhitePlayer(dataToken);
                                if (data.IsBlack === true)
                                    ImBlackPlayer(dataToken);
                            });
                        }
                    });
                }
                else {
                    if (false === isEmpty(party.WhiteToken)) {
                        $('#whiteBtn').prop('disabled', true);
                        $('#whiteBtn').removeClass('btn-success');
                    }
                    else {
                        $('#whiteBtn').prop('disabled', false);
                        $('#whiteBtn').addClass('btn-success');
                    }

                    if (false === isEmpty(party.BlackToken)) {
                        $('#blackBtn').prop('disabled', true);
                        $('#blackBtn').removeClass('btn-success');
                    }
                    else {
                        $('#blackBtn').prop('disabled', false);
                        $('#blackBtn').addClass('btn-success');
                    }
                }
            }
        });

        RefreshPartyState();
    });
}


function playerTurn(myParty, isWhite, isBlack) {
    if (myParty.IsReady === false) {
        return false;
    }
    else if (myParty.WhiteTurn === true && isBlack === true) {
        return false;
    }
    else if (myParty.WhiteTurn === false && isWhite === true) {
        return false;
    }
    else if (isBlack === false && isWhite === false) {
        return false;
    }
    else {
        return true;
    }
}