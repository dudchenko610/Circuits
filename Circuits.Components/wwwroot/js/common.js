window.addEventListener("resize", () => {
    DotNet.invokeMethodAsync("Circuits.Services", 'OnBrowserResizeAsync').then(data => data);
});

function bchGetBoundingClientRectById(id, param) {

    const element = document.getElementById(id);

    if (!element) {
        return null;
    }

    const rect = element.getBoundingClientRect();

    return {
        width: rect.width,
        height: rect.height,
        bottom: rect.bottom,
        left: rect.left,
        right: rect.right,
        top: rect.top,
        x: rect.x,
        y: rect.y,
        offsetTop: element.offsetTop,
        offsetLeft: element.offsetLeft,
        offsetWidth: element.offsetWidth,
        offsetHeight: element.offsetHeight
    };
}

function bchScrollElementTo(id, x, y, behavior) {
    const element = document.getElementById(id);

    if (!element) {
        return;
    }

    element.scrollTo({
        left: x,
        top: y,
        behavior: behavior // only 'auto' or 'smooth'
    });
}

function getPixelRatio() {
    return window.devicePixelRatio;
}


