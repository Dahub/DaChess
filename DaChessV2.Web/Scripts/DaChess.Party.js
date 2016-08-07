function reloadParty(partyName) {
    party = getParty(partyName);
    myBoard = extractBoard(party);
    myHistory = extractHistory(party);
    defineBtnAddPlayerState(party);
    refreshCanvas(party, myBoard, images, size, caseNumber);
    refreshInfoDiv(party);
    refreshTimeDiv(party);
    checkTimeOver(party);
    refreshTurnDiv(party);
    refreshHistoryDiv(myHistory);
    refreshPartyState(party);
    checkPat(party);
    checkCheckMat(party);
    refreshPlayersEndParty(party);
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
    if (myParty.PartyState !== partyStates.waitPlayers) {
        $('#btnChoiseColorDiv').hide();
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
    if (party.PartyState === partyStates.running && (isBlack === true || isWhite === true)) {
        $('#btnDiv').show();
    }
    else {
        $('#btnDiv').hide();
    }

    // si la partie est terminée, on cache les infos de tour du joueur et on affiche celles de retour à l'accueil ou replay
    if (party.PartyState !== partyStates.running && party.PartyState !== partyStates.waitPlayers) {
        $('#playerTurnDiv').hide();
        if (isWhite === true || isBlack === true) {
            $('#endPartyDiv').show();
        }
    }
}

function refreshPlayersEndParty(party) {
    if ((isWhite === true && party.WhitePlayerState === playerStates.askToReplay && party.BlackPlayerState !== playerStates.askToReplay)
        || (isBlack === true && party.BlackPlayerState === playerStates.askToReplay && party.WhitePlayerState !== playerStates.askToReplay)) {
        setMsg('Attente de la réponse');
        $('#replayBtn').hide();
    }
    else if ((isBlack === true && party.WhitePlayerState === playerStates.askToReplay)
        || (isWhite === true && party.BlackPlayerState === playerStates.askToReplay)) {
        $('#replayBtn').hide();
        $('#replayModal').modal({ backdrop: 'static', keyboard: false })
            .one('click', '#confirmReplayBtn', function (e) {
                $.ajax({
                    url: '/Party/Replay',
                    data: { partyName: party.Name, token: playerToken },
                    cache: false,
                    type: 'GET',
                    async: false,
                    dataType: "json",
                    contentType: 'application/json; charset=utf-8'
                }).done(function (data) {
                    setDebugInfo(data);
                    // informer server et autres usr de la redirection vers la nouvelle partie
                    if (data.IsError === true) {
                        setMsg(data.ErrorMsg);
                    }
                    else {
                        $.connection.partyHub.server.newInfo(party.Name, data.ResultText);
                    }
                    // on lance la demande de redirection
                    $.connection.partyHub.server.redirectToNewParty(party.Name, data.Name);
                });
            }).one('click', '#refuseReplayBtn', function (e) {
                var msgDrawn = '';
                if (isBlack === true)
                    msgDrawn = 'Les noirs refusent de rejouer';
                else if (isWhite === true)
                    msgDrawn = 'les Blancs refusent de rejouer';
                $.connection.partyHub.server.newInfo(party.Name, msgDrawn);
            });
    }
}

function refreshTurnDiv(party) {
    if (party.WhitePlayerState === playerStates.canMove || party.WhitePlayerState === playerStates.check) {
        if (isWhite === true) {
            $('#turnDiv').html('A votre tour de jouer');
        }
        else {
            $('#turnDiv').html('Au joueur blanc de jouer');
        }
        $('#blackTurnImg').hide();
        $('#whiteTurnImg').show();
    }
    else if (party.BlackPlayerState === playerStates.canMove || party.BlackPlayerState === playerStates.check) {
        if (isBlack === true) {
            $('#turnDiv').html('A votre tour de jouer');
        }
        else {
            $('#turnDiv').html('Au joueur noir de jouer');
        }
        $('#whiteTurnImg').hide();
        $('#blackTurnImg').show();
    }
    else {
        $('#turnDiv').html('Bienvenue');
    }
    if (party.PartyState == partyStates.running && (isWhite === true || isBlack === true)) {
        $('#partyFunctionDiv').show();
    } else {
        $('#partyFunctionDiv').hide();
    }
}

function refreshTimeDiv(party) {
    if (party.PartyState === partyStates.running && party.PartyCadence !== cadences.noLimits) {
        $('#timeDiv').show();
        var whiteTime = Math.ceil(party.WhitePlayerTimeLeft / 1000);
        var blackTime = Math.ceil(party.BlackPlayerTimeLeft / 1000);
        if (party.WhitePlayerState === playerStates.canMove && isEmpty(party.History) === false) {
            launchCountDown(whiteTime);
        }
        else if (party.BlackPlayerState === playerStates.canMove) {
            launchCountDown(blackTime);
        }
        initTime(whiteTime, blackTime, party);
    } else {
        stopCountDown();
        $('#timeDiv').hide();
    }
}

function refreshInfoDiv(party) {
    if (isWhite === true) {
        $('#displayname').val('Blancs');
        $('#whiteIcon').removeClass('hidden');
    }
    if (isBlack === true) {
        $('#displayname').val('Noirs');
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

function checkTimeOver(party) {
    if (party.WhitePlayerState === playerStates.timeOver) {
        partyOver = true;
        if (isWhite) {
            showEndModal('Temps écoulé', '<h3>Vous avez perdu :-(</h3>');
        }
        else if (isBlack) {
            showEndModal('Temps écoulé', '<h3>Le temps du joueur blanc est écoulé</h3>');
        }
    }
    else if (party.BlackPlayerState === playerStates.timeOver) {
        partyOver = true;
        if (isBlack) {
            showEndModal('Temps écoulé', '<h3>Vous avez perdu :-(</h3>');
        }
        else if (isWhite) {
            showEndModal('Temps écoulé', '<h3>Le temps du joueur noir est écoulé</h3>');
        }
    }
}

function checkPat(party) {
    if (party.PartyState === partyStates.drawn && partyOver === false) {
        if (isWhite === true && party.BlackPlayerState === playerStates.pat
           || isBlack === true && party.WhitePlayerState === playerStates.pat) {
            partyOver = true;
            showEndModal('Pat, partie nulle', '<h3>Le roi adverse est Pat</h3>');
        }
        else if (isBlack === true && party.BlackPlayerState === playerStates.pat
           || isWhite === true && party.WhitePlayerState === playerStates.pat) {
            partyOver = true;
            showEndModal('Pat, partie nulle', '<h3>Vous êtes Pat</h3>');
        }
    }
}

function checkCheckMat(party) {
    if ((party.WhitePlayerState === playerStates.checkMat || party.BlackPlayerState === playerStates.checkMat) && partyOver === false) {
        if (isWhite === true && party.BlackPlayerState === playerStates.checkMat
            || isBlack === true && party.WhitePlayerState === playerStates.checkMat) {
            partyOver = true;
            showEndModal('Echec et mat', '<h3>Vous avez gagné</h3>');
        }
        else if (isBlack === true && party.BlackPlayerState === playerStates.checkMat
            || isWhite === true && party.WhitePlayerState === playerStates.checkMat) {
            partyOver = true;
            showEndModal('Echec et mat', '<h3>Vous avez perdu :-(</h3>');
        }
        partyOver = true;
    }
}

function validPromote(partyName) {
    moveModel.promote = true;
    moveModel.promotePiece = $('#promotedChoise:checked').val();
    SendMove();
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

function askForDrawn(partyName) {
    $.connection.partyHub.server.askForDrawn(partyName);
}

function respondToDrawnAsk(partyName) {
    if (isBlack === true || isWhite === true) {
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
                if (isBlack === true)
                    msgDrawn = 'Les noirs refusent la nulle';
                else if (isWhite === true)
                    msgDrawn = 'les Blancs refusent la nulle';
                $.connection.partyHub.server.newInfo(partyName, msgDrawn);
            });
    }
}

function askForReplay(partyName) {
    // appel synchrone serveur pour stocker que le joueur demande à rejouer
    $.ajax({
        url: '/Party/Replay',
        data: { partyName: partyName, token: playerToken },
        cache: false,
        async: false,
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
        $.connection.partyHub.server.askForReplay(partyName);
    });
}

function redirectToNewParty(newPartyName) {
    if (isWhite === true || isBlack === true) {
        var url = $(location).attr('origin');
        url += '/Party/Play/';
        url += newPartyName;
        window.location = url;
    }
}