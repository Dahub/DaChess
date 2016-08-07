function listenWindowsResize() {
    var c = $("#canvas");
    var context = c[0].getContext('2d');
    // resize the canvas to fill browser window dynamically
    window.addEventListener('resize', resizeCanvas, false);

    function resizeCanvas() {
        size = $('#canvasDiv').width();
        canvas.width = size;
        canvas.height = size;

        refreshCanvas(party, myBoard, images, size, caseNumber);
    }
    resizeCanvas();
};

function refreshCanvas(myParty, board, myImages, size, caseNumber) {
    if (typeof board !== 'undefined') {
        var brightColor = "#ffff99";
        var darkColor = '#cc6600';

        var c = $("#canvas");
        var ctx = c[0].getContext('2d');
        var caseSize = size / caseNumber;
        var piecePadding = caseSize / 10;
        var pieceSize = caseSize * 0.8;

        // on efface tout
        ctx.clearRect(0, 0, size, size);

        // remplissage avec couleur de fond
        ctx.fillStyle = brightColor;
        ctx.fillRect(0, 0, size, size);

        // cases en fond du plateau
        ctx.fillStyle = darkColor;
        var startCase = 1;
        if (flip === true) {
            startCase = 0;
        }
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
                var yPos = 0;
                if (flip === false) {
                    yPos = ((caseNumber - board[i].line) * caseSize) + piecePadding;
                }
                else {
                    yPos = size - caseSize - ((caseNumber - board[i].line) * caseSize) + piecePadding;
                }

                //   var myImage = document.querySelector('#' + board[i].piece + '');
                var myImage = images[board[i].piece];
                ctx.drawImage(myImage, xPos, yPos, pieceSize, pieceSize);

                // on colore en rouge la case du roi en échec
                if ((board[i].piece === 'w_king' && party.WhitePlayerState === playerStates.check)
                    || (board[i].piece === 'b_king' && party.BlackPlayerState === playerStates.check)) {
                    ctx.fillStyle = 'rgba(255, 102, 102, 0.5)';
                    ctx.fillRect(xPos - piecePadding, yPos - piecePadding, caseSize, caseSize);
                }
            }
        }

        // on colore la dernière case ou une pièce à bougé
        if (isEmpty(party.LastCase) === false) {
            var xPosLast = ((letterToNumber(party.LastCase.charAt(0)) - 1) * caseSize) + piecePadding;
            var yPosLast = 0;
            if (flip === false) {
                yPosLast = ((caseNumber - party.LastCase.charAt(1)) * caseSize) + piecePadding;
            }
            else {
                yPosLast = size - caseSize - ((caseNumber - party.LastCase.charAt(1)) * caseSize) + piecePadding;
            }
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
}


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

function getMousePos(canvas, evt) {
    if (playerTurn(party, isWhite, isBlack) === true) {
        if (moveStep === 2) {
            resetCanvasMove(myBoard);
        }
        moveStep++;

        var rect = canvas.getBoundingClientRect();
        var xcord = evt.clientX - rect.left
        var ycord = evt.clientY - rect.top;

        // y => chiffres
        var caseSize = size / caseNumber;
        if (flip === false) {
            ycord = caseNumber - Math.trunc(ycord / caseSize);
        }
        else {
            ycord = Math.trunc(ycord / caseSize) + 1;
        }
        console.log(ycord);
        xcord = Math.trunc(xcord / caseSize);

        var result = String.fromCharCode(97 + xcord) + ycord.toString();
        var myPiece = getPieceAtCase(result, myBoard);

        if (moveModel.firstCase === result) { // clic sur la même case, on reprends à 0
            resetCanvasMove(myBoard);
        }
        else if (isLegalCase(myBoard, myPiece) === true) {
            var c = $("#canvas");
            var ctx = c[0].getContext('2d');

            ctx.fillStyle = 'rgba(23, 105, 138, 0.5)';
            if (flip === false) {
                ctx.fillRect(xcord * caseSize, size - ycord * caseSize, caseSize, caseSize);
            }
            else {
                ctx.fillRect(xcord * caseSize, ycord * caseSize - caseSize, caseSize, caseSize);
            }
            if (isEmpty(moveModel.firstCase) === true) { // premier coup
                moveModel.firstCase = result;
            }
            else {
                moveModel.secondCase = result;
            }
            if (moveStep === 2) {
                // vérification qu'on a une promotion
                if (isWhite === true && ycord === 8 || isBlack === true && ycord === 1) {
                    // on récupère la pièce
                    var toTestPiece = getPieceAtCase(moveModel.firstCase, myBoard);
                    if (toTestPiece === 'b_pawn' || toTestPiece === 'w_pawn') {
                        $('#promoteModal').modal('show');
                    }
                }
                else {
                    console.log(moveModel);
                    SendMove();
                }
            }
        }
        else {
            moveStep--;
        }
    }
}

function SendMove() {
    var model = {
        FirstCase: moveModel.firstCase,
        SecondCase: moveModel.secondCase,
        Promote: moveModel.promote,
        PromotePiece: moveModel.promotePiece,
        PartyName: party.Name,
        Token: playerToken
    };

    $.ajax({
        url: '/Party/MakeMove',
        data: JSON.stringify(model),
        cache: false,
        async: false,
        type: 'POST',
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

function resetCanvasMove(board) {
    refreshCanvas(party, board, images, size, caseNumber);
    moveStep = 0;
    moveModel.firstCase = '';
    moveModel.secondCase = '';
    moveModel.promote = false;
    moveModel.promotePiece = '';
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
function isLegalCase(board, myPiece) {
    if (moveStep === 1 && isEmpty(myPiece))
        return false;
    if (moveStep === 1) {
        if (isBlack === true && myPiece.charAt(0) === 'w')
            return false;
        if (isWhite === true && myPiece.charAt(0) === 'b')
            return false;
    } else if (moveStep === 2 && isEmpty(myPiece) === false) { // le jouer clique sur une case d'une pièce qui lui appartient, on reprends le déplacement à 0 avec cette case comme début
        if (isBlack === true && myPiece.charAt(0) === 'b'
            || isWhite === true && myPiece.charAt(0) === 'w') {
            refreshCanvas(party, board, images, size, caseNumber);
            moveModel.firstCase = '';
            moveModel.secondCase = '';
            moveModel.promote = false;
            moveModel.promotePiece = '';
            moveStep = 1;
            return true;
        }
    }

    return true;
}

function getPieceAtCase(myCase, board) {
    var piece;
    var col = myCase.charAt(0);
    var line = myCase.charAt(1);
    for (var i = 0; i < board.length; i++) {
        if (board[i].col === col && board[i].line === line) {
            piece = board[i].piece;
            break;
        }
    }
    return piece;
}