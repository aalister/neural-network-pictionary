(function() {
    const canvas = document.getElementById("canvas");

    /** @type {CanvasRenderingContext2D} */
    const context = canvas.getContext("2d");
    const bounds = canvas.getBoundingClientRect();

    context.clearRect(0, 0, canvas.width, canvas.height);
    context.strokeStyle = "black";
    context.lineWidth = 8;

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

    canvas.addEventListener("mouseup", function(event) {
        setMousePos(event);
        isDrawing = false;
    });

    function setMousePos(event) {
        mouseX = event.clientX - bounds.left;
        mouseY = event.clientY - bounds.top;
    }
})();
