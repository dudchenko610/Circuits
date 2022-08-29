
// for Safari and Firefox
if (!("path" in MouseEvent.prototype)) {
    Object.defineProperty(MouseEvent.prototype, "path", {
        get: function () {

            var path = [];
            var currentElem = this.target;
            while (currentElem) {
                path.push(currentElem);
                currentElem = currentElem.parentElement;
            }
            if (path.indexOf(window) === -1 && path.indexOf(document) === -1)
                path.push(document);
            if (path.indexOf(window) === -1)
                path.push(window);
            return path;
        }
    });
}

function getPathCoordinates(event) {

    const pathCoordinates = event.path.map(element => {
        if (element.getBoundingClientRect) {
            var viewportOffset = element.getBoundingClientRect();

            return {
                x: event.pageX - viewportOffset.left,
                y: event.pageY - viewportOffset.top,
                scrollTop: element.scrollTop,
                classList: element.classList.value,
                id: element.id
            };
        }
    }).filter(x => x);

    return pathCoordinates;
}

Blazor.registerCustomEventType('extscroll', {
    browserEventName: 'scroll',
    createEventArgs: event => {

        return {
            clientHeight: event.target.clientHeight,
            scrollHeight: event.target.scrollHeight,
            scrollTop: event.target.scrollTop,
            scrollLeft: event.target.scrollLeft,
            clientWidth: event.target.clientWidth
        };
    }
});

Blazor.registerCustomEventType('extmousemove', {
    browserEventName: 'mousemove',
    createEventArgs: event => {

        const pathCoordinates = getPathCoordinates(event);

        return {
            offsetX: event.offsetX,
            offsetY: event.offsetY,
            pageX: event.pageX,
            pageY: event.pageY,
            clientHeight: event.target.clientHeight,
            clientWidth: event.target.clientWidth,
            pathCoordinates: pathCoordinates
        };
    }
});

Blazor.registerCustomEventType('extmousewheel', {
    browserEventName: 'mousewheel',
    createEventArgs: event => {

        const x = event.clientX - event.target.offsetLeft;
        const y = event.clientY - event.target.offsetTop;
        const pathCoordinates = getPathCoordinates(event);

        return {
            x: x,
            y: y,
            deltaX: event.deltaX,
            deltaY: event.deltaY,
            pathCoordinates: pathCoordinates
        };
    }
});