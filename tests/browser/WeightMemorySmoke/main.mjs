const output = document.getElementById("result");
const worker = new Worker("./worker.mjs", { type: "module" });

function finish(result) {
    globalThis.weightMemorySmoke = result;
    output.textContent = JSON.stringify(result, null, 2);
    worker.terminate();
}

worker.onmessage = ({ data }) => finish(data);
worker.onerror = (event) => finish({ passed: false, error: event.message });
