import { dotnet } from "./_framework/dotnet.js";

const pageSize = 256;
const fileLength = 2 * pageSize + 13;
const xor = 0x12da6204;
const name = `aikernel-weight-memory-test-${crypto.randomUUID()}`;

async function run() {
    const root = await navigator.storage.getDirectory();
    const entry = await root.getFileHandle(name, { create: true });
    let device;
    try {
        const expected = Uint8Array.from({ length: fileLength }, (_, index) => index * 17 + (index >> 8) * 29 + 3);
        const writer = await entry.createSyncAccessHandle();
        try {
            if (writer.write(expected) !== expected.length) throw new Error("Incomplete fixture write.");
            writer.flush();
        } finally {
            writer.close();
        }

        const adapter = await navigator.gpu?.requestAdapter({ powerPreference: "low-power" });
        if (!adapter || adapter.info.isFallbackAdapter) throw new Error("No physical WebGPU adapter is available.");
        device = await adapter.requestDevice();
        const storage = device.createBuffer({ size: 2 * pageSize, usage: GPUBufferUsage.STORAGE | GPUBufferUsage.COPY_DST });
        const transformed = device.createBuffer({ size: pageSize, usage: GPUBufferUsage.STORAGE | GPUBufferUsage.COPY_SRC });
        const readback = device.createBuffer({ size: pageSize, usage: GPUBufferUsage.MAP_READ | GPUBufferUsage.COPY_DST });
        const pipeline = device.createComputePipeline({
            layout: "auto",
            compute: {
                entryPoint: "main",
                module: device.createShaderModule({ code: `
                    @group(0) @binding(0) var<storage, read> page: array<u32>;
                    @group(0) @binding(1) var<storage, read_write> output: array<u32>;
                    @compute @workgroup_size(64)
                    fn main(@builtin(global_invocation_id) id: vec3<u32>) {
                        output[id.x] = page[id.x] ^ ${xor}u;
                    }
                ` }),
            },
        });
        const group = device.createBindGroup({ layout: pipeline.getBindGroupLayout(0), entries: [
            { binding: 0, resource: { buffer: storage, offset: pageSize, size: pageSize } },
            { binding: 1, resource: { buffer: transformed } },
        ] });
        const { setModuleImports, runMain, getConfig } = await dotnet.create();
        const checks = [];
        let stats;
        setModuleImports("smoke", {
            moduleUrl: () => new URL("./opfs-weight-memory.mjs", import.meta.url).href,
            fileName: () => name,
            device: () => device,
            buffer: () => storage,
            report: (backingReads, residentRamPages, nonresidentPages, peakRamPageBytes) => {
                stats = { backingReads, residentRamPages, nonresidentPages, peakRamPageBytes };
            },
            checkGpuPage: async (index) => {
                const encoder = device.createCommandEncoder();
                const pass = encoder.beginComputePass();
                pass.setPipeline(pipeline);
                pass.setBindGroup(0, group);
                pass.dispatchWorkgroups(1);
                pass.end();
                encoder.copyBufferToBuffer(transformed, 0, readback, 0, pageSize);
                device.queue.submit([encoder.finish()]);
                await readback.mapAsync(GPUMapMode.READ);
                try {
                    const result = new DataView(readback.getMappedRange());
                    const reference = new Uint8Array(pageSize);
                    reference.set(expected.subarray(index * pageSize, (index + 1) * pageSize));
                    const referenceWords = new DataView(reference.buffer);
                    for (let byte = 0; byte < pageSize; byte += 4) {
                        if (result.getUint32(byte, true) !== ((referenceWords.getUint32(byte, true) ^ xor) >>> 0)) {
                            throw new Error(`WebGPU output differs at page ${index}, byte ${byte}.`);
                        }
                    }
                    checks.push(index);
                } finally {
                    readback.unmap();
                }
            },
        });
        device.pushErrorScope("validation");
        const exitCode = await runMain(getConfig().mainAssemblyName, []);
        const validation = await device.popErrorScope();
        if (exitCode !== 0 || checks.length !== 5 || !stats || validation) {
            throw new Error(validation?.message ?? `Managed smoke test failed with exit code ${exitCode}.`);
        }
        const exclusive = await entry.createSyncAccessHandle();
        exclusive.close();
        return { passed: true, browser: navigator.userAgent, vendor: adapter.info.vendor,
            architecture: adapter.info.architecture, fallback: adapter.info.isFallbackAdapter,
            checks, fileLength, pageSize, gpuUploadBufferBytes: storage.size, ...stats };
    } finally {
        if (device) device.destroy();
        await root.removeEntry(name);
    }
}

try {
    postMessage(await run());
} catch (error) {
    postMessage({ passed: false, error: String(error), stack: error.stack });
}
