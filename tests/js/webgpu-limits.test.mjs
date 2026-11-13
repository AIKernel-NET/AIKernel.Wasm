import assert from "node:assert/strict";
import test from "node:test";
import { createWebGpuBrowserExecutor, createWebGpuEnvelopeBridge } from "../../src/Compute/WebGpuComputeProvider/Runtime/Browser/webgpu-envelope-bridge.js";

const ids = ["gpu.aisthesis.raw-frame", "gpu.spatial-reasoning", "gpu.hud.composite"];
const methods = ["dispatchAisthesisEnvelope", "dispatchSpatialReasoningEnvelope", "dispatchHudCompositeEnvelope"];
const limits = { maxBufferSize: 4096, maxStorageBufferBindingSize: 4096, maxUniformBufferBindingSize: 256, maxComputeWorkgroupsPerDimension: 16 };

function envelope(kind = 1, width = 8, height = 8) {
  return {
    Schema: "aikernel.gpu.dispatch", Version: "0.1.3", PassId: ids[kind - 1], PassKind: kind,
    Frame: { FrameId: "ws4", FrameIndex: 1, SampleTicks: 1 },
    Targets: [
      { Role: "raw", TargetId: "raw", Kind: 1, Backend: 2, Width: width, Height: height, ZeroCopy: true },
      { Role: "hud", TargetId: "hud", Kind: 2, Backend: 2, Width: width, Height: height, ZeroCopy: true }
    ], Buffers: [], Scalars: {}, FeatureFlags: {}, Labels: [], Tags: {},
    Metadata: {
      authoritative_ready: "false", candidate_streak: "1", diagnostic_ready: "true", diagnostic_streak: "1",
      feature_mask_storage_texture: "false", frame_index: "1", sample_ticks: "1", pass_id: ids[kind - 1],
      path_role: kind === 3 ? "hud" : "sensor", pilot_state: "browser-builtin-compute", promotion_gate: "trace-candidate",
      required_streak: "1", execution_mode: "browser-webgpu-compute",
      pass_readiness: "Passes.{Aisthesis,SpatialReasoning,HudComposite}:Aisthesis{ShaderBound=true,PipelineCached=false,BuiltInExecutor=true,InjectedExecutor=false,ReadyForBuiltIn=true}"
    }
  };
}

function harness(overrides = {}, passExecutor = null, adapterLimits = null) {
  const calls = { buffers: [], dispatches: [], shaders: 0, pipelines: 0, encoders: 0, textures: 0, submissions: 0, injected: 0 };
  const reasons = [];
  const device = {
    limits: { ...limits, ...overrides }, lost: new Promise(() => {}),
    createBuffer({ size, usage }) {
      calls.buffers.push({ size, usage });
      const data = new ArrayBuffer(Math.min(size, 4096));
      return { size, data, async mapAsync() {}, getMappedRange: (offset, length) => data.slice(offset, offset + length), unmap() {} };
    },
    createTexture() { calls.textures++; return { createView: () => ({}) }; },
    createShaderModule() { calls.shaders++; return {}; },
    createComputePipeline() { calls.pipelines++; return { getBindGroupLayout: () => ({}) }; },
    createBindGroup: descriptor => descriptor,
    createCommandEncoder() {
      calls.encoders++;
      return {
        beginComputePass: () => ({ setPipeline() {}, setBindGroup() {}, end() {}, dispatchWorkgroups: (...counts) => calls.dispatches.push(counts) }),
        copyBufferToBuffer() {}, finish: () => ({})
      };
    },
    queue: { writeBuffer() {}, submit() { calls.submissions++; } }
  };
  const executor = createWebGpuBrowserExecutor({
    device, adapter: adapterLimits ? { limits: adapterLimits } : null,
    textureRegistry: { get: () => ({ createView: () => ({}) }) },
    shaderSources: Object.fromEntries(ids.map(id => [id, "@compute @workgroup_size(1) fn main() {}"])),
    ...(passExecutor ? { passExecutors: async context => { calls.injected++; return passExecutor(context); } } : {}),
    onFallback: reason => reasons.push(reason)
  });
  return { device, executor, calls, reasons };
}

async function rejectBeforeGpu(h, request, reason, bridge = false) {
  const target = bridge ? createWebGpuEnvelopeBridge({ executor: h.executor }) : h.executor;
  await assert.rejects(target[methods[request.PassKind - 1]](request), error => error instanceof RangeError && error.message === reason);
  assert.deepEqual(h.calls, { buffers: [], dispatches: [], shaders: 0, pipelines: 0, encoders: 0, textures: 0, submissions: 0, injected: 0 });
  assert.equal(h.executor.getDiagnostics().LastFallbackReason, reason);
}

for (const kind of [1, 2, 3]) {
  const bytes = kind === 2 ? 1360 : kind === 3 ? 64 : 128;
  test(`pass ${kind} accepts exact buffer/dispatch limits`, async () => {
    const h = harness({ maxBufferSize: bytes, maxStorageBufferBindingSize: bytes, maxUniformBufferBindingSize: bytes, maxComputeWorkgroupsPerDimension: 1 });
    assert.ok(await h.executor[methods[kind - 1]](envelope(kind)));
    assert.deepEqual(h.calls.dispatches, [[1, 1, 1]]);
    assert.ok(h.calls.buffers.every(buffer => buffer.size <= bytes));
  });
  test(`pass ${kind} rejects buffer limit before any GPU work`, async () => {
    await rejectBeforeGpu(harness({ maxBufferSize: bytes - 1 }), envelope(kind), "webgpu-limit-exceeded:maxBufferSize");
  });
  const binding = kind === 3 ? "maxUniformBufferBindingSize" : "maxStorageBufferBindingSize";
  test(`pass ${kind} rejects binding limit before any GPU work`, async () => {
    await rejectBeforeGpu(harness({ [binding]: bytes - 1 }), envelope(kind), `webgpu-limit-exceeded:${binding}`);
  });
  test(`pass ${kind} requires usable limits`, async () => {
    const h = harness(); delete h.device.limits;
    await rejectBeforeGpu(h, envelope(kind), "webgpu-limit-unavailable:maxBufferSize");
  });
}

for (const kind of [1, 3]) for (const axis of [0, 1]) {
  test(`pass ${kind} rejects dispatch axis ${axis} over limit`, async () => {
    const request = envelope(kind, axis === 0 ? 129 : 8, axis === 1 ? 129 : 8);
    await rejectBeforeGpu(harness(), request, "webgpu-limit-exceeded:maxComputeWorkgroupsPerDimension");
  });
  test(`pass ${kind} accepts dispatch axis ${axis} exactly at limit`, async () => {
    const h = harness();
    await h.executor[methods[kind - 1]](envelope(kind, axis === 0 ? 128 : 8, axis === 1 ? 128 : 8));
    assert.deepEqual(h.calls.dispatches, [[axis === 0 ? 16 : 1, axis === 1 ? 16 : 1, 1]]);
  });
}

for (const value of [undefined, NaN, Infinity, 0, -1, 1.5, Number.MAX_SAFE_INTEGER + 1]) {
  test(`rejects invalid device maxBufferSize ${value}`, async () => {
    await rejectBeforeGpu(harness({ maxBufferSize: value }), envelope(), "webgpu-limit-unavailable:maxBufferSize");
  });
}

test("uses the lower adapter limit", async () => {
  await rejectBeforeGpu(harness({}, null, { ...limits, maxBufferSize: 127 }), envelope(), "webgpu-limit-exceeded:maxBufferSize");
});
test("uses the lower device limit", async () => {
  await rejectBeforeGpu(harness({ maxBufferSize: 127 }, null, limits), envelope(), "webgpu-limit-exceeded:maxBufferSize");
});
test("does not replace missing device limits with adapter limits", async () => {
  const h = harness({}, null, limits); delete h.device.limits;
  await rejectBeforeGpu(h, envelope(), "webgpu-limit-unavailable:maxBufferSize");
});
test("rejects missing adapter limits when an adapter is supplied", async () => {
  await rejectBeforeGpu(harness({}, null, {}), envelope(), "webgpu-limit-unavailable:maxBufferSize");
});

for (const value of [NaN, Infinity, -1, 1.5, Number.MAX_SAFE_INTEGER + 1]) {
  test(`rejects invalid target dimension ${value}`, async () => {
    await rejectBeforeGpu(harness(), envelope(1, value), "webgpu-invalid-size");
  });
}

for (const field of ["Values", "Count", "Stride"]) {
  test(`checks envelope buffer ${field} before injected executor`, async () => {
    const h = harness({ maxBufferSize: 4096 }, () => ({}));
    const request = envelope();
    request.Buffers = [{ Name: "oversized", Stride: 1, Count: 1, Values: [], [field]: field === "Values" ? Array(1025).fill(0) : 1025 }];
    await rejectBeforeGpu(h, request, "webgpu-limit-exceeded:maxBufferSize");
  });
}
test("rejects overflowing descriptor multiplication", async () => {
  const request = envelope(); request.Buffers = [{ Stride: Number.MAX_SAFE_INTEGER, Count: 2, Values: [] }];
  await rejectBeforeGpu(harness(), request, "webgpu-invalid-size");
});
test("bridge normalization cannot hide an infinite dimension", async () => {
  await assert.rejects(createWebGpuEnvelopeBridge({ executor: harness().executor }).dispatchAisthesisEnvelope(envelope(1, Infinity)), /webgpu-invalid-size/);
});

for (const operation of ["storage", "output", "readback", "overflow"]) {
  test(`injected ${operation} helper rejects before buffer allocation`, async () => {
    const h = harness({}, async context => {
      if (operation === "storage") context.createStorageBufferFromValues(Array(1025).fill(0));
      if (operation === "output") context.createOutputBuffer(1025);
      if (operation === "overflow") context.createOutputBuffer(Number.MAX_SAFE_INTEGER);
      if (operation === "readback") await context.readFloatBuffer({}, 1025);
      return {};
    });
    await assert.rejects(h.executor.dispatchAisthesisEnvelope(envelope()), /webgpu-(limit-exceeded:maxBufferSize|invalid-size)/);
    assert.equal(h.calls.buffers.length, 0);
    assert.equal(h.calls.encoders, 0);
    assert.equal(h.calls.submissions, 0);
  });
}

test("empty storage/output helpers retain their four-byte minimum", async () => {
  const h = harness({}, context => { context.createStorageBufferFromValues([]); context.createOutputBuffer(0); return {}; });
  await h.executor.dispatchAisthesisEnvelope(envelope());
  assert.deepEqual(h.calls.buffers.map(buffer => buffer.size), [4, 4]);
});

test("outer envelope bridge rejects over-limit work instead of reporting fallback success", async () => {
  await rejectBeforeGpu(harness({ maxBufferSize: 127 }), envelope(), "webgpu-limit-exceeded:maxBufferSize", true);
});

test("rejects oversized array-like input before reading or converting values", async () => {
  let read = false;
  const h = harness({}, context => {
    context.createStorageBufferFromValues({ length: 1025, get 0() { read = true; return 1; } });
    return {};
  });
  await assert.rejects(h.executor.dispatchAisthesisEnvelope(envelope()), /webgpu-limit-exceeded:maxBufferSize/);
  assert.equal(read, false);
  assert.equal(h.calls.buffers.length, 0);
});

test("checks dynamically acquired device limits without increasing them", async () => {
  const h = harness({ maxBufferSize: 127 });
  let requested = 0;
  const adapter = { limits, async requestDevice(descriptor) { requested++; assert.deepEqual(descriptor, {}); return h.device; } };
  const saved = Object.getOwnPropertyDescriptor(globalThis, "navigator");
  Object.defineProperty(globalThis, "navigator", { configurable: true, value: { gpu: { requestAdapter: async () => adapter } } });
  try {
    const executor = createWebGpuBrowserExecutor({ shaderSources: {}, textureRegistry: {} });
    await assert.rejects(executor.dispatchAisthesisEnvelope(envelope()), /webgpu-limit-exceeded:maxBufferSize/);
    assert.equal(requested, 1);
    assert.equal(h.calls.buffers.length, 0);
  } finally {
    if (saved) Object.defineProperty(globalThis, "navigator", saved); else delete globalThis.navigator;
  }
});
