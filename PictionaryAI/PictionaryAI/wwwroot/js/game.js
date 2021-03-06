(async function() {
    const conn = new signalR.HubConnectionBuilder().withUrl("/pictionaryHub").build();

    let players = [];
    const playerList = document.getElementById("player-list");
    const playerTemplate = document.getElementById("player-template");
    let hasEndedGame = false;

    function sortPlayers() {
        players = players.sort(function (a, b) {
            if (a.score < b.score) return 1;
            if (a.score > b.score) return -1;
            if (a.name > b.name) return 1;
            if (a.name < b.name) return -1;
            return 0;
        });
    }

    function clearGuessedPlayers() {
        for (const player of players) {
            let playerEle = document.querySelector(`.player[player-id='${player.id}']`);
            playerEle.removeAttribute("guessed");
            playerEle.querySelector(".player-increase").classList.remove("showing");
        }
    }

    /**
     * Display error message when the server closes.
     */
    conn.onclose(function () {
        hasEndedGame = true;
        document.getElementById("connection-lost").classList.add("active");
    });

    /**
     * Update the list of players in the sidebar.
     */
    conn.on("playerListChange", function(updatedList) {
        // Sort players by score (or alphabetically if they have the same score)
        players = updatedList;
        sortPlayers();
        playerList.innerHTML = "";

        for (const player of players) {
            let clone = playerTemplate.cloneNode(true);
            clone.id = "";
            clone.setAttribute("player-id", player.id);
            clone.style.display = "";
            clone.querySelector(".player-name").innerHTML = player.name;
            clone.querySelector(".player-score").innerHTML = player.score;
            if (player.hasGuessed) {
                clone.setAttribute("guessed", "");
            }
            playerList.appendChild(clone);
        }
    });

    const startButton = document.getElementById("start-button");
    const timer = document.getElementById("timer");
    const countdownBackground = document.getElementById("canvas-overlay");
    const countdown = document.getElementById("canvas-overlay-text");
    const clearButton = document.getElementById("canvas-clear-button");
    const guessContainer = document.getElementById("guess-container");
    const speechBubble = document.getElementById("speech-bubble");

    /**
     * Launch the countdown.
     */
    conn.on("doCountdown", function(duration) {
        new Audio("/sound/countdown.wav").play();
        console.log(`Do countdown ${duration}`);

        startButton.style.display = "none";
        timer.style.display = "inline";

        let number = Math.floor(duration / 1000);
        countdownBackground.style.visibility = "visible";
        clearButton.style.visibility = "hidden";
        countdown.innerHTML = number
        
        const interval = setInterval(function() {
            number--;
            countdown.innerHTML = number;

            if (number == 0) {
                countdownBackground.style.visibility = "hidden";
                clearButton.style.visibility = "visible";
                clearInterval(interval);
            }
        }, 1000);
    });

    const prompt = document.getElementById("prompt");
    let currentPromptName;
    let currentPromptIndex;
    let interval = null;
    let guessInterval;

    /**
     * Start a new round with a particular prompt.
     */
    conn.on("newRound", function(currentRound, totalRounds, promptName, promptIndex, duration) {
        console.log(`New round: ${promptName}, ${promptIndex}, ${duration}`);
        isRunning = true;

        document.getElementById("rounds").innerHTML = `${currentRound}/${totalRounds}`;
        document.getElementById("rounds").style.display = "inline";

        setTimeout(function() {
            guessInterval = setInterval(predict, 1000);
        }, 2000);

        currentPromptName = promptName;
        currentPromptIndex = promptIndex;

        document.getElementById("mascot-happy").classList.remove("active");
        document.getElementById("mascot-confused").classList.add("active");

        countdownBackground.style.visibility = "hidden";
        clearButton.style.visibility = "visible";
        let number = Math.floor(duration / 1000);
        timer.style.display = "inline";
        timer.innerHTML = number;
        prompt.innerHTML = promptName;
        guessContainer.style.visibility = "hidden";
        speechBubble.innerHTML = "";
        hasDrawn = false;
        context.clearRect(0, 0, canvas.width, canvas.height);

        clearGuessedPlayers();

        interval = setInterval(function() {
            number--;
            timer.innerHTML = number;

            if (number == 0) {
                clearInterval(interval);
            }
        }, 1000);
    });

    /**
     * Indicate that a player scored.
     */
    conn.on("playerScored", function(playerId, newScore, changeInScore) {
        console.log(`Player scored: ${playerId}, ${newScore}, ${changeInScore}`);
        let player = players.find(p => p.id == playerId);
        if (!hasEndedGame && player) {
            player.score = newScore;
            player.hasGuessed = true;
            sortPlayers();
            let playerIndex = players.findIndex(p => p.id == playerId);
            let playerEle = document.querySelector(`.player[player-id='${player.id}']`);
            if (playerIndex == 0) {
                playerList.insertAdjacentElement("afterbegin", playerEle);
            }
            else {
                playerList.children.item(playerIndex - 1).insertAdjacentElement("afterend", playerEle);
            }
            playerEle.querySelector(".player-score").innerHTML = newScore;
            playerEle.setAttribute("guessed", "");
            let increaseBalloon = playerEle.querySelector(".player-increase");
            increaseBalloon.innerHTML = `+${changeInScore}`;
            increaseBalloon.classList.add("showing");
        }
    });

    /**
     * End the round.
     */
    conn.on("endRound", function() {
        new Audio("/sound/finish.wav").play();
        console.log("End round");
        isRunning = false;

        guessContainer.style.visibility = "hidden";
        speechBubble.innerHTML = "";
        timer.innerHTML = 0;
        clearInterval(interval);
        clearInterval(guessInterval);

        countdownBackground.style.visibility = "visible";
        clearButton.style.visibility = "hidden";
        countdown.innerHTML = "Round Over";
    });

    conn.on("showResults", function(players) {
        console.log("results");
        players = players.sort(function(a, b) {
            if (a.score < b.score) return 1;
            if (a.score > b.score) return -1;
            if (a.name > b.name) return 1;
            if (a.name < b.name) return -1;
            return 0;
        });

        hasEndedGame = true;
        clearGuessedPlayers();

        document.getElementById("results").classList.add("active");
        document.getElementById("first-place").innerHTML = players[0].name;

        if (players.length > 1) {
            document.getElementById("second-place").innerHTML = players[1].name;
        }

        if (players.length > 2) {
            document.getElementById("third-place").innerHTML = players[2].name;
        }
    });

    await conn.start();
    const connId = conn.connection.connectionId;

    /**
     * Get the game code and decide on joining a game or creating a new game.
     */
    let { code } = new Proxy(new URLSearchParams(window.location.search), {
        get: (searchParams, prop) => searchParams.get(prop)
    });
    if (code != null) {
        joinGame(code);
    } else {
        hostGame();
    }

    /**
     * Copy the game code to the clipboard.
     */
    document.getElementById("copy-code-button").addEventListener("click", function(_) {
        navigator.clipboard.writeText(code);
    });

    /**
     * Join an active game using the code.
     * 
     * @param {string} code 
     */
    async function joinGame(code) {
        document.getElementById("code").value = code;
        const response = await fetch(`/api/game/join/${code}?id=${connId}`, { method: "POST" });
        if (!response.ok) {
            document.getElementById("game-not-found").classList.add("active");
        }
    }

    /**
     * Host a new game and let the host start the game.
     */
    async function hostGame() {
        const response = await fetch(`/api/game/host?id=${connId}`, { method: "POST" });
        code = await response.text();
        document.getElementById("code").value = code;
        history.pushState(null, '', `${location.origin}${location.pathname}?code=${code}`);
        
        // Make start game button visible
        startButton.style.display = "block";
        startButton.addEventListener("click", function(_) {
            fetch(`/api/game/startgame?id=${connId}`, { method: "POST" });
        });
    }

    /**
     * Ask the user to enter a name.
     */
    document.getElementById("set-name-button").addEventListener("click", async function(_) {
        document.getElementById("enter-name").classList.remove("active");
        const name = document.getElementById("name-input").value;

        if (name) {
            await fetch(`/api/game/setname/${name}?id=${connId}`, { method: "POST" });
        }
    });

    let model = await tf.loadLayersModel('model/model.json');
    // Process useless data to prevent freeze when the first guess is made
    model.predict(tf.randomNormal([1, 28, 28, 1])).dataSync();

    function processImage(image) {
        return tf.tidy(() => {
            let tensor = tf.browser.fromPixels(image, numChannels=1);
            return tensor.expandDims(0);
        });
    }

    function predict() {
        // Only make prediction if the player has drawn on their canvas
        if (!hasDrawn) {
            guessContainer.style.visibility = "hidden";
            speechBubble.innerHTML = "";
            return;
        }

        const result = document.createElement("canvas");

        result.width = 28;
        result.height = 28;

        /**
         * @type {CanvasRenderingContext2D}
         */
        const resultContext = result.getContext("2d");
        resultContext.filter = "invert(1)";

        const width = maxCoordX - minCoordX;
        const height = maxCoordY - minCoordY;
        const pixelSize = Math.max(width, height) / 26;
        const sourceSize = Math.max(width, height) + 2 * pixelSize;
        const centreX = (minCoordX + maxCoordX) / 2;
        const centreY = (minCoordY + maxCoordY) / 2;

        resultContext.drawImage(
            canvas,
            centreX - 0.5 * sourceSize,
            centreY - 0.5 * sourceSize,
            sourceSize,
            sourceSize,
            0, 0, 28, 28
        );

        // Only return the alpha channel
        const image = resultContext.getImageData(0, 0, 28, 28);
        const prediction = model.predict(processImage(image)).dataSync();

        let prediction_sorted = structuredClone(prediction);
        prediction_sorted.sort();
        prediction_sorted.reverse();
        console.log(prediction_sorted.indexOf(prediction[currentPromptIndex]));

        if (prediction_sorted.indexOf(prediction[currentPromptIndex]) < 5) {
            // Don't allow guesses after the round has finished
            if (isRunning) {
                isRunning = false;
                conn.invoke("drawingGuessed");
                countdownBackground.style.visibility = "visible";
                clearButton.style.visibility = "hidden";
                countdown.innerHTML = "Correct!";
                guessContainer.style.visibility = "visible";
                speechBubble.innerHTML = prompts[currentPromptIndex];
                new Audio("/sound/win.mp3").play();
                document.getElementById("mascot-confused").classList.remove("active");
                document.getElementById("mascot-happy").classList.add("active");
                clearInterval(guessInterval);
            }
        } else {
            const guessIndex = prediction.indexOf(Math.max(...prediction));
            guessContainer.style.visibility = "visible";
            speechBubble.innerHTML = prompts[guessIndex];
        }
    }
})();