const canvas = document.getElementById("canvas");

/**
 * @type {CanvasRenderingContext2D}
 */
const context = canvas.getContext("2d");
let canvasRect = canvas.getBoundingClientRect();

let minCoordX = Infinity;
let minCoordY = Infinity;
let maxCoordX = -Infinity;
let maxCoordY = -Infinity;

let isDrawing = false;
let isRunning = false;
let hasDrawn = false;
context.lineCap = "round";

/**
 * Clear the canvas.
 */
function clear() {
    context.fillStyle = "white";
    context.fillRect(0, 0, canvas.width, canvas.height);

    // Reset image bounds
    minCoordX = Infinity;
    minCoordY = Infinity;
    maxCoordX = -Infinity;
    maxCoordY = -Infinity;

    hasDrawn = false;
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
    hasDrawn = isRunning;
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

        // Update bounds of image
        maxCoordX = Math.max(maxCoordX, x);
        maxCoordY = Math.max(maxCoordY, y);
        minCoordX = Math.min(minCoordX, x);
        minCoordY = Math.min(minCoordY, y);
    }
});

/**
 * Stop drawing when the user releases the mouse anywhere.
 */
document.body.addEventListener("mouseup", function(_event) {
    isDrawing = false;
});

document.getElementById("canvas-clear-button").addEventListener("click", clear);