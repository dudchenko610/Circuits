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

//
// function createObjectURL(value) {
//     const byteString = atob(value);
//     const ab = new ArrayBuffer(byteString.length);
//     const ia = new Uint8Array(ab);
//
//     for (let i = 0; i < byteString.length; i++) {
//         ia[i] = byteString.charCodeAt(i);
//     }
//     const blob = new Blob([ab], { type: 'image/jpeg' });
//
//     return URL.createObjectURL(blob);
// }

function createObjectURL(value) {

    const blob = new Blob(
        [value], // Blob parts.
        {
            // type: "text/plain;charset=utf-8",
            type: "application/x-javascript"
        }
    );

    return URL.createObjectURL(blob);
}

const listeners = {};

function addDocumentListener(key, eventName, dotnetReference, methodName) {
    listeners[key + eventName] = function (event) {
        let response = {};

        switch (eventName) {
            case "touchmove":
                const touches = Object.entries(event.touches).map((value, key) => {
                    const touch = value[1];

                    return {
                        clientX: touch.clientX,
                        clientY: touch.clientY,
                        pageX: touch.pageX,
                        pageY: touch.pageY
                    }
                });

                const pathCoordinates = getPathCoordinates({
                    path: [document.elementFromPoint(touches[0].pageX, touches[0].pageY)]
                });

                response = {
                    touches: touches,
                    pathCoordinates: pathCoordinates
                };
                break;
            default:

                response = {
                    offsetX: event.offsetX,
                    offsetY: event.offsetY,
                    pageX: event.pageX,
                    pageY: event.pageY,
                    screenX: event.screenX,
                    screenY: event.screenY,
                    clientX: event.clientX,
                    clientY: event.clientY,
                    pathCoordinates: getPathCoordinates(event)
                };
                break;
        }

        dotnetReference.invokeMethodAsync(methodName, response);
    };

    document.addEventListener(eventName, listeners[key + eventName]);
}

function removeDocumentListener(key, eventName) {
    if (listeners[key + eventName]) {
        document.body.removeEventListener(eventName, listeners[key + eventName]);
        delete listeners[key + eventName];
    }
}