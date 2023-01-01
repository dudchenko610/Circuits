
const solverWorkers = {};

function startSolver(url, dotNetRef) {
    const worker = new Worker(url);
    solverWorkers[url] = worker;

    worker.addEventListener('message', function(e) {
        
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
                
                // console.log('feedbackData', feedbackData);
                
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
