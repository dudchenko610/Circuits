function bchSelectAddOnOuterFocusOut(inputId, containerId, dotNetHelper, methodName) {

    const inputElement = document.getElementById(inputId);

    if (inputElement) {
        inputElement.addEventListener('blur', (e) => {

            const container = document.getElementById(containerId);

            if (container && !container.contains(e.relatedTarget)) {

                setTimeout(() => {
                    dotNetHelper.invokeMethodAsync(methodName);
                }, 0);
            }
        });
    }
}