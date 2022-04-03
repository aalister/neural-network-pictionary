(async function() {
    let model = await tf.loadLayersModel('model/model.json');

    function processImage(image)
    {
        return tf.tidy(()=>{
            let tensor = tf.browser.fromPixels(image, numChannels=1);
            return tensor.expandDims(0);
        });
    }

    document.getElementById("test").addEventListener("click", function(_) {
        predict();
    });

    /**
     * Extract a 28 x 28 image from the canvas.
     * 
     * @returns {Uint8ClampedArray}
     */
    function predict() {
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
        console.log(prediction);

        const prompt = 8;
        let pred_copy = structuredClone(prediction);
        pred_copy.sort();
        pred_copy.reverse();

        console.log("order", pred_copy.indexOf(prediction[prompt]));
        console.log("confidence", prediction[prompt])
    }

    const conn = new signalR.HubConnectionBuilder().withUrl("/pictionaryHub").build();

    let players = [];
    const playerList = document.getElementById("player-list");

    /**
     * Display error message when the server closes.
     */
    conn.onclose(function() {
        document.getElementById("connection-lost").classList.add("active");
    });

    /**
     * Update the list of players in the sidebar.
     */
    conn.on("playerListChange", function(updatedList) {
        // Sort players by score (or alphabetically if they have the same score)
        players = updatedList.sort(function(a, b) {
            if (a.score < b.score) return 1;
            if (a.score > b.score) return -1;
            if (a.name > b.name) return 1;
            if (a.name < b.name) return -1;
            return 0;
        });

        playerList.innerHTML = "";

        for (const player of players) {
            playerList.innerHTML += `<li><span>${player.name}</span><span>${player.score}</span></li>`;
        }
    });

    const startButton = document.getElementById("start-button");
    const timer = document.getElementById("timer");
    const countdownBackground = document.getElementById("canvas-overlay");
    const countdown = document.getElementById("canvas-overlay-text");

    /**
     * Launch the countdown.
     */
    conn.on("doCountdown", function(duration) {
        console.log(`Do countdown ${duration}`);

        startButton.style.display = "none";
        timer.style.display = "inline";

        let number = Math.floor(duration / 1000);
        countdownBackground.style.visibility = "visible";
        countdown.innerHTML = number
        
        const interval = setInterval(function() {
            number--;
            countdown.innerHTML = number;

            if (number == 0) {
                countdownBackground.style.visibility = "hidden";
                clearInterval(interval);
            }
        }, 1000);
    });

    const prompt = document.getElementById("prompt");
    let interval = null;

    /**
     * Start a new round with a particular prompt.
     */
    conn.on("newRound", function(promptName, promptIndex, duration) {
        console.log(`New round: ${promptName}, ${promptIndex}, ${duration}`);
        isRunning = true;

        countdownBackground.style.visibility = "hidden";
        let number = Math.floor(duration / 1000);
        timer.innerHTML = number;
        prompt.innerHTML = promptName;
        context.clearRect(0, 0, canvas.width, canvas.height);

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
    conn.on("playerScored", function(playerId) {
        console.log(`Player scored: ${playerId}`);
        
        const index = players.map(player => player.id).indexOf(playerId);
        const node = playerList.children.item(index);
        node.classList.add("active");

        setTimeout(function() {
            node.classList.remove("active");
        }, 2000);
    });

    /**
     * End the round.
     */
    conn.on("endRound", function() {
        console.log("End round");
        isRunning = false;

        timer.innerHTML = 0;
        clearInterval(interval);

        countdownBackground.style.visibility = "visible";
        countdown.innerHTML = "Round Over";
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
        document.getElementById("code").innerHTML = code;
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
})();