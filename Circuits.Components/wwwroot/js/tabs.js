function bchSetTabContentPosition(contentId, styleStr) {
    const element = document.getElementById(contentId);
    element.style = styleStr;
}

function bchSubscribeOnTabsDragOver(contextId, dotNetHelper) {
    const contextElement = document.getElementById(contextId);

    contextElement.addEventListener('dragover', (event) => {
        event.dataTransfer.dropEffect = "copy";

        if (!contextElement.classList.contains('dragging')) {
            return;
        }

        const pathCoordinates = event.path.map(element => {
            if (element.getBoundingClientRect) {
                var viewportOffset = element.getBoundingClientRect();

                return {
                    x: event.pageX - viewportOffset.left,
                    y: event.pageY - viewportOffset.top,
                    scrollTop: element.scrollTop,
                    classList: element.classList.value,
                    id: element.id
                }
            }
        }).filter(x => x);

        dotNetHelper.invokeMethodAsync('OnDragOverContainer', {
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

function bchAddTabDraggingClass(contextId) {
    const contextElement = document.getElementById(contextId);
    contextElement.classList.add('dragging');
}

function bchRemoveTabDraggingClass(contextId) {
    const contextElement = document.getElementById(contextId);
    contextElement.classList.remove('dragging');
}