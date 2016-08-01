function reloadParty(partyName) {
    party = getParty(partyName);

    myBoard = extractBoard(party);
    myHistory = extractHistory(party);

    loadCookie();

    defineBtnAddPlayerState(party);
    refreshCanvas(party, myBoard, images, size, caseNumber);
    refreshInfoDiv(party);
    refreshTurnDiv(party);
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

function loadCookie() {
    // on récupère la couleur via l'API
    $.ajax({
        url: '/Player/GetPlayerInfosFromCookies',
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

function refreshTurnDiv(party) {
    if (party.WhitePlayerState === playerStates.canMove) {
        if (isWhite === true) {
            $('#turnDiv').html('A votre tour de jouer');
        }
        else {
            $('#turnDiv').html('Au joueur blanc de jouer');
        }
    }
    else if (party.BlackPlayerState === playerStates.canMove) {
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