(function () {
    window.addEventListener("resize", updateContainerSize);

    let remPx = parseFloat(getComputedStyle(document.documentElement).fontSize);
    let everythingContainer = document.getElementById("everything-container");
    let mainContainer = document.getElementById("main-container");

    function updateContainerSize() {
        let windowWidth = window.innerWidth;
        let windowHeight = window.innerHeight;
        let maxWidth = windowWidth - (remPx * 50);
        let maxHeight = Math.min(windowHeight - (remPx * 10), remPx * 75);
        let output = `${Math.min(maxWidth, maxHeight)}px`;
        everythingContainer.style.width = output;
        mainContainer.style.height = output;
    }

    updateContainerSize();
})()