(async function() {
    const conn = new signalR.HubConnectionBuilder().withUrl("/pictionaryHub").build();

    conn.on("playerListChange", function(players) {
        console.log(players);
    });

    await conn.start();
    const connId = conn.connection.connectionId;

    let { code } = new Proxy(new URLSearchParams(window.location.search), {
        get: (searchParams, prop) => searchParams.get(prop)
    });
    if (code) {
        joinGame(code);
    } else {
        hostGame();
    }

    document.getElementById("copy-code").addEventListener("click", function(_) {
        navigator.clipboard.writeText(code);
    });

    async function joinGame(code) {
        document.getElementById("code").innerHTML = code;
        await fetch(`/api/game/join/${code}?id=${connId}`, { method: "POST" });
    }

    async function hostGame() {
        const response = await fetch(`/api/game/host?id=${connId}`, { method: "POST" });
        code = await response.text();
        document.getElementById("code").innerHTML = code;
    }
})();

(function() {
    const canvas = document.getElementById("canvas");

    /** @type {CanvasRenderingContext2D} */
    const context = canvas.getContext("2d");
    const bounds = canvas.getBoundingClientRect();

    context.strokeStyle = "black";
    context.lineWidth = 20;
    context.fillStyle = "rgba(0, 0, 0, 0)";
    context.fillRect(0, 0, canvas.width, canvas.height);

    let mouseX = 0;
    let mouseY = 0;
    let isDrawing = false;

    canvas.addEventListener("mousedown", function(event) {
        setMousePos(event);
        isDrawing = true;

        context.beginPath();
        context.moveTo(mouseX, mouseY);
    });
    
    canvas.addEventListener("mousemove", function(event) {
        setMousePos(event);

        if (isDrawing) {
            context.lineTo(mouseX, mouseY);
            context.stroke();
        }
    });

    document.body.addEventListener("mouseup", function(event) {
        setMousePos(event);
        isDrawing = false;
    });

    function setMousePos(event) {
        mouseX = event.clientX - bounds.left;
        mouseY = event.clientY - bounds.top;
    }

    function downScaleCanvas() {
        const result = document.createElement("canvas");

        result.width = 28;
        result.height = 28;

        /** @type {CanvasRenderingContext2D} */
        const resultContext = result.getContext("2d");
        resultContext.drawImage(canvas, 0, 0, 28, 28);

        // For testing
        document.body.append(result);

        const image = resultContext.getImageData(0, 0, 28, 28).data;
        // Only return the alpha channel
        return image.filter((_, index) => (index - 3) % 4 == 0);
    }
})();
