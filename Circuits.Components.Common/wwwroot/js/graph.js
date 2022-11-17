function bchInitGraphCanvas(canvasId, size) {
    const canvas = document.getElementById(canvasId);
    const ctx = canvas.getContext("2d");

    canvas.width = size.x;
    canvas.height = size.y;


    const GRAPH_TOP = 25;
    const GRAPH_BOTTOM = 375;
    const GRAPH_LEFT = 25;
    const GRAPH_RIGHT = 475;

    ctx.clearRect(0, 0, size.x, size.y);
    ctx.fillStyle = "white";
    ctx.fillRect(0, 0, size.x, size.y);
    ctx.font = "16px Arial";

    // draw X and Y axis  
    ctx.beginPath();
    ctx.moveTo(GRAPH_LEFT, GRAPH_BOTTOM);
    ctx.lineTo(GRAPH_RIGHT, GRAPH_BOTTOM);
    ctx.lineTo(GRAPH_RIGHT, GRAPH_TOP);
    ctx.stroke();
}