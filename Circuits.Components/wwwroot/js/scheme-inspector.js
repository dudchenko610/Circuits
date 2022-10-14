const listeners = {};

function addDocumentListener(key, eventName, dotnetReference, methodName) {
    listeners[key + eventName] = function(event) {
        dotnetReference.invokeMethodAsync(methodName, {
            offsetX: event.offsetX,
            offsetY: event.offsetY,
            pageX: event.pageX,
            pageY: event.pageY,
            screenX: event.screenX,
            screenY: event.screenY,
            clientX: event.clientX,
            clientY: event.clientY,
        });
    };

    document.body.addEventListener(eventName, listeners[key + eventName]);
}

function removeDocumentListener(key, eventName) {
    if (listeners[key + eventName]) {
        document.body.removeEventListener(eventName, listeners[key + eventName]);
        delete listeners[key + eventName];
    }
}