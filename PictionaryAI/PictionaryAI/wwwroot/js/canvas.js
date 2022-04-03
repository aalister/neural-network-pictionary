const canvas = document.getElementById("canvas");

/**
 * @type {CanvasRenderingContext2D}
 */
const context = canvas.getContext("2d");
let canvasRect = canvas.getBoundingClientRect();
let isDrawing = false;
let isRunning = false;
context.lineCap = "round";

/**
 * Clear the canvas.
 */
function clear() {
    context.fillStyle = "white";
    context.fillRect(0, 0, canvas.width, canvas.height);
}

/**
 * Update the canvas when the window resizes.
 */
 function onResize() {
    canvasRect = canvas.getBoundingClientRect();

    canvas.width = canvasRect.width;
    canvas.height = canvasRect.height;

    context.lineWidth = Math.ceil(canvas.width / 28);
    clear();
}
new ResizeObserver(onResize).observe(document.body);
new ResizeObserver(onResize).observe(canvas);
onResize();

/**
 * Get the current mouse position on the canvas.
 * 
 * @param {MouseEvent} event 
 */
function getMousePos(event) {
    const x = event.clientX - canvasRect.left;
    const y = event.clientY - canvasRect.top;
    return { x: x, y: y };
}

/**
 * Start drawing when the user clicks.
 */
canvas.addEventListener("mousedown", function(event) {
    const { x, y } = getMousePos(event);
    isDrawing = isRunning;
    context.beginPath();
    context.moveTo(x, y);
});

/**
 * Draw a line to the new mouse position.
 */
canvas.addEventListener("mousemove", function(event) {
    const { x, y } = getMousePos(event);

    if (isDrawing && isRunning) {
        context.lineTo(x, y);
        context.stroke();
    }
});

/**
 * Stop drawing when the user releases the mouse anywhere.
 */
document.body.addEventListener("mouseup", function(_event) {
    isDrawing = false;
});