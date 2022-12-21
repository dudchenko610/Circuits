function bchInitGraphCanvas(canvasId, size, cellSize) { // OUTDATED
    const canvas = document.getElementById(canvasId);
    const ctx = canvas.getContext("2d");

    canvas.width = size.x;
    canvas.height = size.y;

    ctx.clearRect(0, 0, size.x, size.y);
    ctx.fillStyle = "white";
    ctx.fillRect(0, 0, size.x, size.y);
    ctx.font = "12px Roboto";

    ctx.lineWidth = 1;
    ctx.strokeStyle = 'rgba(3, 2, 41, 0.1)';
    
    ctx.beginPath();
    
    for (let i = 0; i < 10; i ++) {
        ctx.moveTo(0, cellSize.y * i);
        ctx.lineTo(size.x, cellSize.y * i);
    }

    ctx.stroke();

    ctx.lineWidth = 1;
    ctx.strokeStyle = 'red';

    ctx.beginPath();
    ctx.moveTo(0, Math.sin(0) * 0.5 * size.y + 0.5 * size.y);

    let i = 0;
    let height = size.y;

    for (let x = 0; x < Math.PI * 4; x += Math.PI / 180) {
        const y = Math.sin(x) * 0.5 * size.y + 0.5 * size.y;
        
        // if (y > height) {
        //     height = y;
        //     canvas.height = height;
        // }
        
        ctx.lineTo(i, y);
        
        i++;
    }
    
    ctx.stroke();
}