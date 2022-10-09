var drgimg = document.createElement("img");
drgimg.src = 'data:image/gif;base64,R0lGODlhAQABAIAAAAUEBAAAACwAAAAAAQABAAACAkQBADs=';

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

Blazor.registerCustomEventType('mouseleave', {
    browserEventName: 'mouseleave',
    createEventArgs: event => {

        return {
        };
    }
});

Blazor.registerCustomEventType('extmouseout', {
    browserEventName: 'mouseout',
    createEventArgs: event => {

        const pathCoordinates = getPathCoordinates(event);

        return {
            relatedTargetIsChildOfTarget: (event.target && event.relatedTarget) ? event.target.contains(event.relatedTarget) : false,
            relatedTargetClassList: event.relatedTarget ? event.relatedTarget.classList.value : '',
            targetClassList: event.target ? event.target.classList.value : '',
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

Blazor.registerCustomEventType('extdragstart', {
    browserEventName: 'dragstart',
    createEventArgs: event => {

        //var isSafari = /constructor/i.test(window.HTMLElement) || (function (p) { return p.toString() === "[object SafariRemoteNotification]"; })(!window['safari'] || (typeof safari !== 'undefined' && safari.pushNotification));

        event.dataTransfer.effectAllowed = "copyMove";
        event.dataTransfer.setDragImage(drgimg, 0, 0);

        // setTimeout(function () {
        //     event.target.setAttribute('dragging', '');
        // }, 0);

        const pathCoordinates = getPathCoordinates(event);

        return {
            offsetX: event.offsetX,
            offsetY: event.offsetY,
            pageX: event.pageX,
            pageY: event.pageY,
            screenX: event.screenX,
            screenY: event.screenY,
            pathCoordinates: pathCoordinates
        };
    }
});

Blazor.registerCustomEventType('extdragend', {
    browserEventName: 'dragend',
    createEventArgs: event => {
        // event.target.removeAttribute('dragging');
        const pathCoordinates = getPathCoordinates(event);

        return {
            offsetX: event.offsetX,
            offsetY: event.offsetY,
            pageX: event.pageX,
            pageY: event.pageY,
            screenX: event.screenX,
            screenY: event.screenY,

            pathCoordinates: pathCoordinates
        };
    }
});

Blazor.registerCustomEventType('extdragover', {
    browserEventName: 'dragover',
    createEventArgs: event => {
        
        event.dataTransfer.dropEffect = "copy";
        const pathCoordinates = getPathCoordinates(event);

        return {
            offsetX: event.offsetX,
            offsetY: event.offsetY,
            pageX: event.pageX,
            pageY: event.pageY,
            screenX: event.screenX,
            screenY: event.screenY,

            pathCoordinates: pathCoordinates
        };
    }
});

Blazor.registerCustomEventType('extdrop', {
    browserEventName: 'drop',
    createEventArgs: event => {
        const pathCoordinates = getPathCoordinates(event);

        return {
            offsetX: event.offsetX,
            offsetY: event.offsetY,
            pageX: event.pageX,
            pageY: event.pageY,
            screenX: event.screenX,
            screenY: event.screenY,

            pathCoordinates: pathCoordinates
        };
    }
});

function subscribeOnMouseMove(contextId, dotNetHelper) {
    const contextElement = document.getElementById(contextId);
    
    contextElement.addEventListener('mousemove', (event) => {
        const pathCoordinates = getPathCoordinates(event);
        
        dotNetHelper.invokeMethodAsync('OnMouseMove', {
            offsetX: event.offsetX,
            offsetY: event.offsetY,
            pageX: event.pageX,
            pageY: event.pageY,
            screenX: event.screenX,
            screenY: event.screenY,

            pathCoordinates: pathCoordinates
        });
    });
}

Blazor.registerCustomEventType('extdragstart', {
    browserEventName: 'dragstart',
    createEventArgs: event => {

        event.dataTransfer.effectAllowed = "copyMove";

        if (!bchGhostElementAppended) {
            document.body.appendChild(bchGhostElement);
            bchGhostElementAppended = true;
        }

        var isSafari = /constructor/i.test(window.HTMLElement) || (function (p) { return p.toString() === "[object SafariRemoteNotification]"; })(!window['safari'] || (typeof safari !== 'undefined' && safari.pushNotification));

        if (!isSafari) {
            event.dataTransfer.setDragImage(bchGhostElement, 0, 0);
        } else {
            event.dataTransfer.setDragImage(drgimg, 0, 0);
        }

        setTimeout(function () {
            event.target.setAttribute('dragging', '');
        }, 0);

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

        return {
            offsetX: event.offsetX,
            offsetY: event.offsetY,
            pageX: event.pageX,
            pageY: event.pageY,
            screenX: event.screenX,
            screenY: event.screenY,

            pathCoordinates: pathCoordinates
        };
    }
});


function subscribeOnDragOver(contextId, dotNetHelper) {
    const contextElement = document.getElementById(contextId);

    contextElement.addEventListener('dragover', (event) => {
        const pathCoordinates = getPathCoordinates(event);

        dotNetHelper.invokeMethodAsync('OnDragOver', {
            offsetX: event.offsetX,
            offsetY: event.offsetY,
            pageX: event.pageX,
            pageY: event.pageY,
            screenX: event.screenX,
            screenY: event.screenY,

            pathCoordinates: pathCoordinates
        });
    });
}