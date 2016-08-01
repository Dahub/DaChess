function loadImages(sources, callback) {
    var loadedImages = 0;
    var numImages = 0;

    for (var s in sources) {
        numImages++;
    }
    for (var src in sources) {
        images[src] = new Image();
        images[src].onload = function () {
            if (++loadedImages >= numImages) {
                callback(images);
            }
        };
        images[src].src = sources[src];
    }
}

function refreshCanvas(myParty, board, myImages, size, caseNumber) {
    var c = $("#canvas");
    var ctx = c[0].getContext('2d');
    var caseSize = size / caseNumber;
    var piecePadding = caseSize / 10;
    var pieceSize = caseSize * 0.8;

    // on efface tout
    ctx.clearRect(0, 0, size, size);

    // remplissage avec couleur de fond
    ctx.fillStyle = "#ffff99";
    ctx.fillRect(0, 0, size, size);

    // cases en fond du plateau
    ctx.fillStyle = "#cc6600";
    var startCase = 1;
    for (posY = 0; posY < caseNumber; posY = posY + 1) {
        for (posX = startCase; posX < caseNumber; posX = posX + 2) {
            ctx.fillRect(posX * caseSize, posY * caseSize, caseSize, caseSize);
        }
        if (startCase === 1) {
            startCase = 0;
        }
        else {
            startCase = 1;
        }
    }

    // les pièces
    for (var i = 0; i < board.length; i++) {
        if (isEmpty(board[i].piece) === false) {
            var xPos = ((letterToNumber(board[i].col) - 1) * caseSize) + piecePadding;
            var yPos = ((caseNumber - board[i].line) * caseSize) + piecePadding;
            //   var myImage = document.querySelector('#' + board[i].piece + '');
            var myImage = images[board[i].piece];
            ctx.drawImage(myImage, xPos, yPos, pieceSize, pieceSize);

            // on colore en rouge la case du roi en échec
            // 7 est l'état "CHECK" du joueur
            if ((board[i].piece === 'w_king' && party.WhitePlayerState === 7)
                || (board[i].piece === 'b_king' && party.BlackPlayerState === 7)) {
                ctx.fillStyle = 'rgba(255, 102, 102, 0.5)';
                ctx.fillRect(xPos - piecePadding, yPos - piecePadding, caseSize, caseSize);
            }
        }
    }

    // on colore la dernière case ou une pièce à bougé
    if (isEmpty(party.LastCase) === false) {
        var xPosLast = ((letterToNumber(party.LastCase.charAt(0)) - 1) * caseSize) + piecePadding;
        var yPosLast = ((caseNumber - party.LastCase.charAt(1)) * caseSize) + piecePadding;
        ctx.fillStyle = 'rgba(102, 255, 51, 0.5)';
        ctx.fillRect(xPosLast - piecePadding, yPosLast - piecePadding, caseSize, caseSize);
    }

    // on grise le canvas si la partie n'est pas prête
    // 2 étant l'état "RUNNING" de la partie
    if (party.PartyState !== partyStates.running) {
        ctx.fillStyle = 'rgba(184, 184, 184, 0.4)';
        ctx.fillRect(0, 0, size, size);
    }
}

function getMousePos(canvas, evt) {
    if (playerTurn(party, isWhite, isBlack) === true) {
        if (moveStep === 2) {
            resetCanvasMove(myBoard.board);
        }
        moveStep++;

        var rect = canvas.getBoundingClientRect();
        var xcord = evt.clientX - rect.left
        var ycord = evt.clientY - rect.top

        // y => chiffres
        var caseSize = size / caseNumber;
        ycord = caseNumber - Math.trunc(ycord / caseSize);
        xcord = Math.trunc(xcord / caseSize);
        var result = String.fromCharCode(97 + xcord) + ycord.toString();

        if (moveText === result + ' ') // clic sur la même case, on reprends à 0
        {
            resetCanvasMove(myBoard);
        }
        else if (isLegalCase(result, myBoard) === true) {
            var c = $("#canvas");
            var ctx = c[0].getContext('2d');

            ctx.fillStyle = 'rgba(23, 105, 138, 0.5)';
            ctx.fillRect(xcord * caseSize, size - ycord * caseSize, caseSize, caseSize);

            moveText += result;
            moveText += ' ';
            if (moveStep === 2) {
                $.ajax({
                    url: '/Party/MakeMove',
                    data: { partyName: party.Name, move: moveText, token: playerToken },
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
                        $.connection.partyHub.server.newInfo('@Model', data.ResultText);
                    }
                    $.connection.partyHub.server.newMove('@Model');
                });
            }
        }
        else {
            moveStep--;
        }
    }
}

function resetCanvasMove(board) {
    refreshCanvas(party, board, images, size, caseNumber);
    moveStep = 0;
    moveText = '';
}

function playerTurn(myParty, isWhite, isBlack) {
    if (myParty.PartyState !== partyStates.running) {
        return false;
    }
    else if (myParty.WhitePlayerState === playerStates.canMove && isBlack === true) {
        return false;
    }
    else if (myParty.BlackPlayerState === playerStates.canMove && isWhite === true) {
        return false;
    }
    else if (isBlack === false && isWhite === false) {
        return false;
    }
    else {
        return true;
    }
}

// on vérifie qu'on a bien sélectionné une case valide
// par exemple : une pièce de la couleur du joueur pour la case de départ
function isLegalCase(myCase, board) {
    var col = myCase.charAt(0);
    var line = myCase.charAt(1);
    var caseFind = false;

    for (var i = 0; i < board.length; i++) {
        if (board[i].col === col && board[i].line === line) {
            caseFind = true;
            if (moveStep === 1 && isEmpty(board[i].piece))
                return false;
            if (moveStep === 1) {
                if (isBlack === true && board[i].piece.charAt(0) === 'w')
                    return false;
                if (isWhite === true && board[i].piece.charAt(0) === 'b')
                    return false;
            }
            else if (moveStep === 2) { // le jouer clique sur une case d'une pièce qui lui appartient, on reprends le déplacement à 0 avec cette case comme début
                if (isBlack === true && board[i].piece.charAt(0) === 'b'
                    || isWhite === true && board[i].piece.charAt(0) === 'w') {
                    refreshCanvas(party, board, images, size, caseNumber);
                    moveText = '';
                    moveStep = 1;
                    return true;
                }
            }
        }
    }
    if (moveStep === 1 && caseFind === false)
        return false;

    return true;
}