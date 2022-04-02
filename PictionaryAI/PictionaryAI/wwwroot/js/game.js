(async function() {
    const prompt = document.getElementById("prompt");
    const timer = document.getElementById("timer");
    const playerList = document.getElementById("player-list");
    let players = [];

    const conn = new signalR.HubConnectionBuilder().withUrl("/pictionaryHub").build();

    /**
     * Update the list of players in the sidebar.
     */
    conn.on("playerListChange", function(updatedList) {
        players = updatedList;
        playerList.innerHTML = "";

        for (const player of players) {
            playerList.innerHTML += `<li><span>${player.name}</span><span>${player.score}</span></li>`;
        }
    });

    /**
     * Launch the countdown.
     */
    conn.on("doCountdown", function(duration) {
        console.log(`Do countdown ${duration}`);
    });

    /**
     * Start a new round with a particular prompt.
     */
    conn.on("newRound", function(prompt, promptIndex, roundLength) {
        prompt.innerHTML = prompt;
        console.log(`New round: ${prompt}, ${promptIndex}, ${roundLength}`);
    });


    /**
     * Indicate that a player scored.
     */
    conn.on("playerScored", function(playerId) {
        console.log(`Player scored: ${playerId}`);
    });

    /**
     * End the round.
     */
    conn.on("endRound", function() {
        console.log("End round");
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
            document.getElementById("game-not-found").style.visibility = "visible";
        }
    }

    /**
     * Host a new game and let the host start the game.
     */
    async function hostGame() {
        const response = await fetch(`/api/game/host?id=${connId}`, { method: "POST" });
        code = await response.text();
        document.getElementById("code").innerHTML = code;
        
        // Make start game button visible
        const startButton = document.getElementById("start-button")
        startButton.style.display = "block";
        startButton.addEventListener("click", function(_) {
            fetch(`/api/game/startgame?id=${connId}`, { method: "POST" });
        });
    }

    /**
     * Ask the user to enter a name.
     */
    document.getElementById("set-name-button").addEventListener("click", async function(_) {
        document.getElementById("enter-name").style.visibility = "hidden";
        const name = document.getElementById("name-input").value;

        if (name) {
            await fetch(`/api/game/setname/${name}?id=${connId}`, { method: "POST" });
        }
    });
})();

(function() {
    const canvas = document.getElementById("canvas");

    /** @type {CanvasRenderingContext2D} */
    const context = canvas.getContext("2d");
    const bounds = canvas.getBoundingClientRect();

    context.lineCap = "round";
    context.strokeStyle = "black";
    context.lineWidth = 20;
    context.fillStyle = "rgba(0, 0, 0, 0)";
    context.fillRect(0, 0, canvas.width, canvas.height);

    let mouseX = 0;
    let mouseY = 0;
    let isDrawing = false;

    /**
     * Start drawing when the user clicks.
     */
    canvas.addEventListener("mousedown", function(event) {
        setMousePos(event);
        isDrawing = true;

        context.beginPath();
        context.moveTo(mouseX, mouseY);
    });
    
    /**
     * Draw a line to the mouse's new position.
     */
    canvas.addEventListener("mousemove", function(event) {
        setMousePos(event);

        if (isDrawing) {
            context.lineTo(mouseX, mouseY);
            context.stroke();
        }
    });

    /**
     * Stop drawing the line when the user releases the mouse anywhere on the page.
     */
    document.body.addEventListener("mouseup", function(event) {
        setMousePos(event);
        isDrawing = false;
    });

    /**
     * Set mouseX and mouseY to the current mouse position.
     * 
     * @param {MouseEvent} event 
     */
    function setMousePos(event) {
        mouseX = event.clientX - bounds.left;
        mouseY = event.clientY - bounds.top;
    }

    /**
     * Extract a 28 x 28 image from the canvas.
     * 
     * @returns {Uint8ClampedArray}
     */
    function extractImage() {
        const result = document.createElement("canvas");

        result.width = 28;
        result.height = 28;

        /** @type {CanvasRenderingContext2D} */
        const resultContext = result.getContext("2d");
        resultContext.drawImage(canvas, 0, 0, 28, 28);

        // Only return the alpha channel
        const image = resultContext.getImageData(0, 0, 28, 28).data;
        return image.filter((_, index) => (index - 3) % 4 == 0);
    }
})();
