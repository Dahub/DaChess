﻿function reloadParty(partyName) {
    party = getParty(partyName);
    myBoard = extractBoard(party);
    myHistory = extractHistory(party);
    defineBtnAddPlayerState(party);
    refreshCanvas(party, myBoard, images, size, caseNumber);
    refreshInfoDiv(party);
    refreshTurnDiv(party);
    refreshHistoryDiv(myHistory);
    refreshPartyState(party);
    checkPat(party);
    checkCheckMat(party);
    checkPromote(party);
}

function getParty(partyName) {
    var myParty;
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

function extractBoard(myParty) {
    if (isEmpty(myParty.Board) === false)
        return JSON.parse(myParty.Board).board;
    else
        return '';
}

function extractHistory(myParty) {
    if (isEmpty(myParty.History) === false)
        return JSON.parse(myParty.History);
    else
        return '';
}

function loadCookie(partyName) {
    // on récupère la couleur via l'API
    $.ajax({
        url: '/Player/GetPlayerInfosFromCookies',
        data: { partyName: partyName },
        cache: false,
        async: false,
        type: 'GET',
        dataType: "json",
        contentType: 'application/json; charset=utf-8'
    }).done(function (data) {
        setDebugInfo(data);
        if (isEmpty(data.Token) === false) {
            playerToken = data.Token;
            if (data.PlayerColor === colors.black) {
                isBlack = true;
            }
            else if (data.PlayerColor === colors.white) {
                isWhite = true;
            }
        }
    });
}

function addPlayer(partyName, color) {
    var model = {
        PartyName: partyName,
        PlayerColor: color
    };
    console.log(model);
    $.ajax({
        url: '/Party/AddPlayerToParty',
        data: JSON.stringify(model),
        cache: false,
        type: 'POST',
        dataType: "json",
        contentType: 'application/json; charset=utf-8'
    }).done(function (data) {
        setDebugInfo(data);
        playerToken = data.Token;
        party = getParty(partyName);

        // ajout du cookies du joueur
        $.ajax({
            url: "/Player/SetPlayerCookies",
            type: "get",
            data: { partyName: party.Name, token: playerToken }
        });

        // on envoie l'info aux autres joueurs
        if (color === colors.white) {
            isWhite = true;
            $.connection.partyHub.server.sendAddWhitePlayer(partyName);
        }
        else {
            isBlack = true;
            $.connection.partyHub.server.sendAddBlackPlayer(partyName);
        }
    });
}

function defineBtnAddPlayerState(myParty) {
    if (myParty.PartyState === partyStates.running) {
        $('#btnRow').hide();
    }
    else {
        if (myParty.WhitePlayerState === playerStates.undefined) {
            $('#whiteBtn').prop('disabled', false);
            $('#whiteBtn').addClass('btn-success');
        }
        else if (isWhite === true) {
            hideAllBtn();
        }
        else {
            $('#whiteBtn').prop('disabled', true);
            $('#whiteBtn').removeClass('btn-success');
            $('#whiteBtn').html('Joueur connecté');
        }
        if (myParty.BlackPlayerState === playerStates.undefined) {
            $('#blackBtn').prop('disabled', false);
            $('#blackBtn').addClass('btn-success');
        }
        else if (isBlack === true) {
            hideAllBtn();
        }
        else {
            $('#blackBtn').prop('disabled', true);
            $('#blackBtn').removeClass('btn-success');
            $('#blackBtn').html('Joueur connecté');
        }
    }
}

function hideAllBtn() {
    $('#whiteBtn').prop('disabled', true);
    $('#whiteBtn').addClass('hidden');
    $('#whiteBtn').removeClass('btn-success');
    $('#blackBtn').prop('disabled', true);
    $('#blackBtn').addClass('hidden');
    $('#blackBtn').removeClass('btn-success');
}

function refreshPartyState(party) {
    // on affiche les bouton d'abandon ou de demande nulle si la partie est prête
    if (party.PartyState === partyStates.running && (isBlack == true || isWhite == true)) {
        $('#btnDiv').show();
    }
    else {
        $('#btnDiv').hide();
    }
}

function refreshTurnDiv(party) {
    if (party.WhitePlayerState === playerStates.canMove || party.WhitePlayerState === playerStates.check || party.WhitePlayerState === playerStates.canPromote) {
        if (isWhite === true) {
            $('#turnDiv').html('A votre tour de jouer');
        }
        else {
            $('#turnDiv').html('Au joueur blanc de jouer');
        }
    }
    else if (party.BlackPlayerState === playerStates.canMove || party.BlackPlayerState === playerStates.check || party.BlackPlayerState === playerStates.canPromote) {
        if (isBlack === true) {
            $('#turnDiv').html('A votre tour de jouer');
        }
        else {
            $('#turnDiv').html('Au joueur noir de jouer');
        }
    }
    else {
        $('#turnDiv').html('Bienvenue');
    }
}

function refreshInfoDiv(party) {
    if (isWhite === true) {
        $('#displayname').val('Blancs');
        $('#colorDiv').html('Vous jouez les blancs');
        $('#whiteIcon').removeClass('hidden');
    }
    if (isBlack === true) {
        $('#displayname').val('Noirs');
        $('#colorDiv').html('Vous jouez les noirs');
        $('#blackIcon').removeClass('hidden');
    }

    if (party.PartyState === partyStates.waitPlayers) {
        if (isWhite === true) {
            setInfo('Attente du joueur noir');
        }
        else if (isBlack === true) {
            setInfo('Attente du joueur blanc');
        }
        else {
            setInfo('En attente de joueurs');
        }
    }
    else if (party.PartyState === partyStates.running) {
        setInfo('Partie prête');
    }
    else if (party.PartyState === partyStates.drawn) {
        setInfo('Partie nulle');
    }
    else if (party.PartyState === partyStates.overWhiteWin) {
        setInfo('Victoire des blancs');
    }
    else if (party.PartyState === partyStates.overBlackWin) {
        setInfo('Victoire des noirs');
    }
}

function setInfo(msg) {
    $('#infoDiv').html(msg);
}

function setMsg(msg) {
    if (isEmpty(msg)) {
        $('#msgDivParent').hide();
    } else {
        $('#msgDivParent').show();
    }
    $('#msgDiv').html(msg);
}

function refreshHistoryDiv(myHistory) {
    $('#moves').empty();
    if (myHistory) {
        jQuery.each(myHistory.Moves, function (i, val) {
            $('#moves').prepend('<li><strong>' + htmlEncode(i)
               + '</strong>: ' + htmlEncode(val) + '</li>');
        });
    }
}

function checkPat(party) {
    if (party.PartyState === partyStates.drawn && partyOver == false) {
        if (isWhite === true && party.BlackPlayerState === playerStates.pat
           || isBlack === true && party.WhitePlayerState === playerStates.pat) {
            $('#patText').html('<h3>Le roi adverse est Pat</h3>')
            partyOver = true;
            $('#patModal').modal('show');
        }
        else if (isBlack === true && party.BlackPlayerState === playerStates.pat
           || isWhite === true && party.WhitePlayerState === playerStates.pat) {
            $('#patText').html('<h3>Vous êtes Pat</h3>')
            partyOver = true;
            $('#patModal').modal('show');
        }
    }
}

function checkCheckMat(party) {
    if ((party.WhitePlayerState === playerStates.checkMat || party.BlackPlayerState === playerStates.checkMat) && partyOver === false) {
        if (isWhite === true && party.BlackPlayerState === playerStates.checkMat
            || isBlack == true && party.WhitePlayerState === playerStates.checkMat) {
            $('#matText').html('<h3>Vous avez gagné</h3>')
        }
        else {
            $('#matText').html('<h3>Vous avez perdu :(</h3>')
        }
        partyOver = true;
        $('#matModal').modal('show');
    }
}

function checkPromote(party) {
    if (party.WhitePlayerState === playerStates.canPromote && isWhite === true) {
        $('#promoteModal').modal('show');
    }
    else if (party.BlackPlayerState === playerStates.canPromote && isBlack === true) {
        $('#promoteModal').modal('show');
    }
}

function validPromote(partyName) {
    $.ajax({
        url: '/Party/MakePromote',
        data: { partyName: partyName, piece: $('#promotedChoise:checked').val(), token: playerToken },
        cache: false,
        type: 'GET',
        dataType: "json",
        contentType: 'application/json; charset=utf-8'
    }).done(function (data) {
        setDebugInfo(data);
        if (data.IsError === true) {
            setMsg(data.ErrorMsg);
        }
        else {
            $.connection.partyHub.server.newInfo(party.Name, data.ResultText);
        }
        $.connection.partyHub.server.newMove(party.Name);
    });
}

function resign(event, partyName) {
    event.preventDefault();
    $('#confirm').modal({ backdrop: 'static', keyboard: false })
        .one('click', '#confirmBtn', function (e) {
            // abandon du joueur
            $.ajax({
                url: '/Party/Resign',
                data: { partyName: partyName, token: playerToken },
                cache: false,
                async: false,
                type: 'GET',
                dataType: "json",
                contentType: 'application/json; charset=utf-8'
            }).done(function (data) {
                $.connection.partyHub.server.aPlayerResign(partyName);
                $.connection.partyHub.server.newInfo(partyName, data.ResultText);
            });
        });
}

function askForDrawn(partyName){
    $.connection.partyHub.server.askForDrawn(partyName);
}

function respondToDrawnAsk(partyName) {
    if (isBlack == true || isWhite == true) {
        $('#confirmDrawnModal').modal({ backdrop: 'static', keyboard: false })
            .one('click', '#confirmDrawnBtn', function (e) {
                $.ajax({
                    url: '/Party/Drawn',
                    data: { partyName: partyName, token: playerToken },
                    cache: false,
                    type: 'GET',
                    dataType: "json",
                    contentType: 'application/json; charset=utf-8'
                }).done(function (data) {
                    $.connection.partyHub.server.newInfo(partyName, data.ResultText);
                    $.connection.partyHub.server.aPlayerAcceptDrawn(partyName);
                });
            }).one('click', '#refuseDrawn', function (e) {
                var msgDrawn = '';
                if (isBlack == true)
                    msgDrawn = 'Les noirs refusent la nulle';
                else if (isWhite == true)
                    msgDrawn = 'les Blancs refusent la nulle';
                $.connection.partyHub.server.newInfo(partyName, msgDrawn);
            });
    }
}