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
            if ((board[i].piece === 'w_king' && party.WhitePlayerState == 7)
                || (board[i].piece === 'b_king' && party.BlackPlayerState == 7)) {
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
    if (party.PartyState != 2) {
        ctx.fillStyle = 'rgba(184, 184, 184, 0.4)';
        ctx.fillRect(0, 0, size, size);
    }
}