
const solverWorkers = {};

function startSolver(url) {
    const worker = new Worker(url);
    solverWorkers[url] = worker;

    worker.addEventListener('message', function(e) {
        
        switch (e.data) {
            case 'completed': {
                if (solverWorkers[url]) {
                    worker.terminate();
                    delete solverWorkers[url];
                    
                    // trigger as stopped

                    console.log('solverWorkers', solverWorkers);
                }
                
                break;
            }
            default: {
                const feedbackData = e.data;
                console.log('feedback-data', feedbackData);
                
                // trigger feedback
                
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
