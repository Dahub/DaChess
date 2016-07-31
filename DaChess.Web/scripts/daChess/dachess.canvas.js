function refreshCanvas(board) {
    var c = $("#canvas");
    var ctx = c[0].getContext('2d');
    ctx.clearRect(0, 0, size, size);

    ctx.fillStyle = "#ffff99";
    ctx.fillRect(0, 0, size, size);

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

    for (var i = 0; i < board.length; i++) {
        if (isEmpty(board[i].piece) === false) {
            var xPos = ((letterToNumber(board[i].col) - 1) * caseSize) + piecePadding;
            var yPos = ((caseNumber - board[i].line) * caseSize) + piecePadding;
            //   var myImage = document.querySelector('#' + board[i].piece + '');
            var myImage = images[myBoard.board[i].piece];
            ctx.drawImage(myImage, xPos, yPos, pieceSize, pieceSize);
            if ((board[i].piece === 'w_king' && party.WhiteIsCheck && isWhitePlayer === true)
                || (board[i].piece === 'b_king' && party.BlackIsCheck && isBlackPlayer === true)) {
                ctx.fillStyle = 'rgba(255, 102, 102, 0.5)';
                ctx.fillRect(xPos - piecePadding, yPos - piecePadding, caseSize, caseSize);
            }
        }
    }

    // on colore la dernière case ou une pièce à bougé
    if (isEmpty(party.LastMoveCase === false)) {
        var xPosLast = ((letterToNumber(party.LastMoveCase.charAt(0)) - 1) * caseSize) + piecePadding;
        var yPosLast = ((caseNumber - party.LastMoveCase.charAt(1)) * caseSize) + piecePadding;
        ctx.fillStyle = 'rgba(102, 255, 51, 0.5)';
        ctx.fillRect(xPosLast - piecePadding, yPosLast - piecePadding, caseSize, caseSize);
    }

    if (party.IsReady === false) {
        ctx.fillStyle = 'rgba(184, 184, 184, 0.4)';
        ctx.fillRect(0, 0, size, size);
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
                if (isBlackPlayer === true && board[i].piece.charAt(0) === 'w')
                    return false;
                if (isWhitePlayer === true && board[i].piece.charAt(0) === 'b')
                    return false;
            }
            else if (moveStep === 2) {
                if (isBlackPlayer === true && board[i].piece.charAt(0) === 'b')
                    return false;
                if (isWhitePlayer === true && board[i].piece.charAt(0) === 'w')
                    return false;
            }
        }
    }
    if (moveStep === 1 && caseFind === false)
        return false;

    return true;
}

function resetCanvasMove(board) {
    refreshCanvas(board);
    moveStep = 0;
    moveText = '';
}

function loadImages(sources, callback) {
    var loadedImages = 0;
    var numImages = 0;
    // get num of sources
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