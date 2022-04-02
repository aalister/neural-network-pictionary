(async function() {
    const canvas = document.getElementById("canvas");

    /**
     * @type {CanvasRenderingContext2D}
     */
    const context = canvas.getContext("2d");
    let bounds;

    /**
     * Update the canvas when it resizes.
     */
    function resize() {
        bounds = canvas.getBoundingClientRect();

        canvas.width = bounds.width;
        canvas.height = bounds.height;

        context.lineWidth = Math.ceil(canvas.width / 28);
    }
    new ResizeObserver(resize).observe(canvas);
    resize();

    context.lineCap = "round";
    context.strokeStyle = "black";
    context.fillStyle = "rgba(0, 0, 0, 0)";
    context.clearRect(0, 0, canvas.width, canvas.height);

    let mouseX = 0;
    let mouseY = 0;
    let isDrawing = false;
    let inGame = false;

    /**
     * Start drawing when the user clicks.
     */
    canvas.addEventListener("mousedown", function(event) {
        setMousePos(event);
        isDrawing = true && inGame;

        context.beginPath();
        context.moveTo(mouseX, mouseY);
    });
    
    /**
     * Draw a line to the mouse's new position.
     */
    canvas.addEventListener("mousemove", function(event) {
        setMousePos(event);

        if (isDrawing && inGame) {
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

        /**
         * @type {CanvasRenderingContext2D}
         */
        const resultContext = result.getContext("2d");
        resultContext.drawImage(canvas, 0, 0, 28, 28);

        // Only return the alpha channel
        const image = resultContext.getImageData(0, 0, 28, 28).data;
        return image.filter((_, index) => (index - 3) % 4 == 0);
    }

    const conn = new signalR.HubConnectionBuilder().withUrl("/pictionaryHub").build();

    let players = [];
    const playerList = document.getElementById("player-list");

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
    const countdown = document.getElementById("countdown");

    /**
     * Launch the countdown.
     */
    conn.on("doCountdown", function(duration) {
        console.log(`Do countdown ${duration}`);

        startButton.style.display = "none";
        timer.style.display = "inline";

        let number = Math.floor(duration / 1000);
        countdown.style.visibility = "visible";
        countdown.innerHTML = number
        
        const interval = setInterval(function() {
            number--;
            countdown.innerHTML = number;

            if (number == 0) {
                countdown.style.visibility = "hidden";
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
        inGame = true;

        countdown.style.visibility = "hidden";
        let number = Math.floor(duration / 1000);
        timer.innerHTML = number;
        prompt.innerHTML = promptName;
        context.clearRect(0, 0, canvas.width, canvas.height);

        interval = setInterval(function() {
            number--;
            timer.innerHTML = number;
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
        inGame = false;

        timer.innerHTML = 0;
        clearInterval(interval);

        countdown.style.visibility = "visible";
        countdown.innerHTML = "Round<br>Over";
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
