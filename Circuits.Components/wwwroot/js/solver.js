const solverWorkers = {};

function testBroydensMethod(url) {
    const worker = new Worker(url, { type: "module" });
    worker.postMessage('start');
}

function startSolver(url, dotNetRef) {
    const worker = new Worker(url);
    solverWorkers[url] = worker;

    worker.addEventListener('message', function (e) {

        switch (e.data) {
            case 'completed': {
                if (solverWorkers[url]) {
                    worker.terminate();
                    delete solverWorkers[url];

                    // console.log('solverWorkers', solverWorkers);

                    dotNetRef.invokeMethodAsync('SolverCompletedCallback', url); // trigger as stopped
                }

                break;
            }
            default: {
                const feedbackData = e.data.reverse();

                console.log('feedbackData', feedbackData);

                dotNetRef.invokeMethodAsync('SolverUpdateCallback', {
                    url: url,
                    varInfos: feedbackData
                }); // trigger feedback

                break;
            }
        }

    }, false);
    worker.postMessage('start');
}

function stopSolver(url) {
    const worker = solverWorkers[url];

    if (worker) {
        worker.terminate();
        delete solverWorkers[url];

        // trigger as stopped
    }
}

// https://www.lakeheadu.ca/sites/default/files/uploads/77/docs/RemaniFinal.pdf
// https://math.stackexchange.com/questions/728666/calculate-jacobian-matrix-without-closed-form-or-analytical-form

// const systemFunctions = [];
// const systemVars = [];
//
// function calculateJacobian(x, dx) {
//     const jacobian = [];
//
//     for (let i = 0; i < systemFunctions.length; i++) { // iterate over rows
//         const row = [];
//
//         for (let j = 0; j < systemFunctions.length; j++) {
//             // reset variables
//             for (let v = 0; v < systemVars.length; v++) {
//                 const element = systemVars[v];
//                 element.variable.value = x[v];
//
//                 if (j === v) element.variable.value += dx;
//             }
//
//             let partialDerivative = systemFunctions[i] / dx;
//
//             row.push(partialDerivative);
//         }
//
//         jacobian.push(row);
//     }
//
//     return jacobian;
// }