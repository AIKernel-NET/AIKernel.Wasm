import assert from "node:assert/strict";

import {
  createWebGpuRev3BrowserExecutor,
  createWebGpuRev3EnvelopeBridge
} from "../../src/Compute/WebGpuComputeProvider/Runtime/Browser/webgpu-rev3-envelope-bridge.js";

const PASS_AISTHESIS = 1;
const PASS_SPATIAL_REASONING = 2;
const PASS_HUD_COMPOSITE = 3;
const PASS_ID_AISTHESIS = "gpu.aisthesis.raw-frame";
const PASS_ID_SPATIAL_REASONING = "gpu.spatial-reasoning";
const PASS_ID_HUD_COMPOSITE = "gpu.hud.composite";
const TARGET_RAW_FRAMEBUFFER = 1;
const TARGET_HUD_COMPOSITE = 2;
const BACKEND_WEBGPU = 2;

const envelope = {
  Schema: "aikernel.gpu.rev3.dispatch",
  Version: "0.1.3",
  PassId: PASS_ID_AISTHESIS,
  PassKind: PASS_AISTHESIS,
  Frame: {
    FrameId: "frame-vm",
    FrameIndex: 42,
    SampleTicks: 1001
  },
  Targets: [
    {
      Role: "raw",
      TargetId: "raw-vm",
      Kind: TARGET_RAW_FRAMEBUFFER,
      Backend: BACKEND_WEBGPU,
      Width: 8,
      Height: 8,
      PixelFormat: 1,
      ZeroCopy: true
    }
  ],
  Buffers: [],
  FeatureFlags: {
    redPanel: true,
    enemyDirection: true
  },
  Scalars: {},
  Labels: [],
  Metadata: metadataFor(PASS_ID_AISTHESIS, "sensor"),
  Tags: {}
};

async function run() {
  const builtInExecutor = createWebGpuRev3BrowserExecutor({
    device: createFakeWebGpuDevice(),
    textureRegistry: {
      get(id) {
        return id === "raw-vm" || id === "hud-vm"
          ? { createView: () => ({ label: "raw-vm-view" }) }
          : null;
      }
    },
    shaderSources: {
      [PASS_ID_AISTHESIS]: "@compute @workgroup_size(1) fn main() {}",
      [PASS_ID_SPATIAL_REASONING]: "@compute @workgroup_size(1) fn main() {}",
      [PASS_ID_HUD_COMPOSITE]: "@compute @workgroup_size(1) fn main() {}"
    }
  });

  const builtIn = await builtInExecutor.dispatchAisthesisEnvelope(envelope);
  assert.equal(builtIn.Diagnostics.Metadata.rev3_pass_id, PASS_ID_AISTHESIS);
  assert.equal(builtIn.Diagnostics.Metadata.rev3_frame_index, "42");
  assert.equal(builtIn.Diagnostics.Metadata.rev3_sample_ticks, "1001");
  assert.equal(builtIn.Diagnostics.Metadata.rev3_path_role, "sensor");
  assert.equal(builtIn.Diagnostics.Metadata.rev3_pilot_state, "browser-builtin-compute");
  assert.equal(builtIn.Diagnostics.Metadata.rev3_execution_mode, "browser-webgpu-compute");
  assert.equal(builtIn.Diagnostics.Metadata.rev3_diagnostic_ready, "true");
  assert.equal(builtIn.Diagnostics.Metadata.rev3_promotion_gate, "trace-candidate");
  assert.equal(builtIn.Diagnostics.Metadata.provider_trace, "from-csharp-dto");
  assert.match(builtIn.Diagnostics.Metadata.rev3_pass_readiness, /Passes\.\{Aisthesis,SpatialReasoning,HudComposite\}/);
  assert.match(builtIn.Diagnostics.Metadata.rev3_pass_readiness, /ReadyForBuiltIn=true/);
  assert.equal(builtIn.Diagnostics.Metadata.rev3_feature_mask_storage_texture, "true");
  assert.equal(builtIn.FeatureVector[0], 7);
  assert.equal(builtIn.FeatureVector.length, 32);
  assert.equal(builtIn.MaskTexture.TargetId, "raw-vm:feature-mask");
  assert.equal(builtIn.MaskTexture.Kind, 4);

  const spatial = await builtInExecutor.dispatchSpatialReasoningEnvelope(spatialEnvelope());
  assert.equal(spatial.Diagnostics.Metadata.rev3_pass_id, PASS_ID_SPATIAL_REASONING);
  assert.equal(spatial.Diagnostics.Metadata.rev3_frame_index, "42");
  assert.equal(spatial.Diagnostics.Metadata.rev3_sample_ticks, "1001");
  assert.equal(spatial.Diagnostics.Metadata.rev3_path_role, "sensor");
  assert.equal(spatial.Diagnostics.Metadata.rev3_pilot_state, "browser-builtin-compute");
  assert.equal(spatial.Diagnostics.Metadata.rev3_execution_mode, "browser-webgpu-compute");
  assert.match(spatial.Diagnostics.Metadata.rev3_pass_readiness, /SpatialReasoning\{ShaderBound=true/);
  assert.equal(spatial.SpatialVector[0], 17);
  assert.equal(spatial.SpatialVector[4], 0.5);
  assert.equal(spatial.SpatialVector.length, 32);

  const hud = await builtInExecutor.dispatchHudCompositeEnvelope(hudEnvelope());
  assert.equal(hud.TargetId, "hud-vm");
  assert.equal(hud.Kind, TARGET_HUD_COMPOSITE);
  assert.equal(hud.Backend, BACKEND_WEBGPU);
  assert.equal(hud.ZeroCopy, true);
  const builtInDiagnostics = builtInExecutor.getDiagnostics();
  assert.equal(builtInDiagnostics.FallbackCount, 0);
  assert.equal(builtInDiagnostics.Passes.Aisthesis.ShaderBound, true);
  assert.equal(builtInDiagnostics.Passes.Aisthesis.BuiltInExecutor, true);
  assert.equal(builtInDiagnostics.Passes.Aisthesis.PipelineCached, true);
  assert.equal(builtInDiagnostics.Passes.Aisthesis.ReadyForBuiltIn, true);
  assert.equal(builtInDiagnostics.Passes.SpatialReasoning.ShaderBound, true);
  assert.equal(builtInDiagnostics.Passes.SpatialReasoning.PipelineCached, true);
  assert.equal(builtInDiagnostics.Passes.HudComposite.ShaderBound, true);
  assert.equal(builtInDiagnostics.Passes.HudComposite.PipelineCached, true);
  assert.match(builtInDiagnostics.PassReadiness, /Aisthesis\{ShaderBound=true/);
  assert.match(builtInDiagnostics.PassReadiness, /HudComposite\{ShaderBound=true/);

  const fallbackBridge = createWebGpuRev3EnvelopeBridge();
  const fallback = await fallbackBridge.dispatchAisthesisEnvelope(envelope);
  assert.equal(fallback.Diagnostics.Metadata.rev3_pass_id, PASS_ID_AISTHESIS);
  assert.equal(fallback.Diagnostics.Metadata.rev3_frame_index, "42");
  assert.equal(fallback.Diagnostics.Metadata.rev3_sample_ticks, "1001");
  assert.equal(fallback.Diagnostics.Metadata.rev3_path_role, "sensor");
  assert.equal(fallback.Diagnostics.Metadata.rev3_pilot_state, "browser-deterministic-fallback");
  assert.equal(fallback.Diagnostics.Metadata.rev3_execution_mode, "deterministic-fallback");
  assert.equal(fallback.Diagnostics.Metadata.rev3_promotion_gate, "fallback");
  assert.match(fallback.Diagnostics.Metadata.rev3_pass_readiness, /ReadyForBuiltIn=false/);
  assert.equal(fallback.Diagnostics.Metadata.rev3_feature_mask_storage_texture, "false");

  const invalidMetadataBridge = createWebGpuRev3EnvelopeBridge({ executor: builtInExecutor });
  const invalidMetadataFallback = await invalidMetadataBridge.dispatchAisthesisEnvelope({
    ...envelope,
    Metadata: {
      ...metadataFor(PASS_ID_AISTHESIS, "sensor"),
      rev3_promotion_gate: "trace-candidte"
    }
  });
  assert.equal(invalidMetadataFallback.Diagnostics.FallbackReason, "metadata-invalid:rev3_promotion_gate");
  assert.equal(invalidMetadataFallback.Diagnostics.Metadata.rev3_metadata_validation_error, "rev3_promotion_gate");
  assert.equal(invalidMetadataFallback.Diagnostics.Metadata.rev3_promotion_gate, "fallback");
  assert.equal(invalidMetadataFallback.Diagnostics.Metadata.rev3_execution_mode, "deterministic-fallback");

  const executorFallbacks = [];
  const missingShaderExecutor = createWebGpuRev3BrowserExecutor({
    device: createFakeWebGpuDevice(),
    textureRegistry: {
      get(id) {
        return id === "raw-vm"
          ? { createView: () => ({ label: "raw-vm-view" }) }
          : null;
      }
    },
    onFallback(reason) {
      executorFallbacks.push(reason);
    }
  });
  assert.equal(await missingShaderExecutor.dispatchAisthesisEnvelope(envelope), null);
  const missingShaderDiagnostics = missingShaderExecutor.getDiagnostics();
  assert.equal(missingShaderDiagnostics.FallbackCount, 1);
  assert.equal(missingShaderDiagnostics.LastFallbackReason, `shader-not-bound:${PASS_ID_AISTHESIS}`);
  assert.equal(missingShaderDiagnostics.Passes.Aisthesis.ShaderBound, false);
  assert.equal(missingShaderDiagnostics.Passes.Aisthesis.ReadyForBuiltIn, false);
  assert.deepEqual(executorFallbacks, [`shader-not-bound:${PASS_ID_AISTHESIS}`]);
}

function spatialEnvelope() {
  return {
    ...envelope,
    PassId: PASS_ID_SPATIAL_REASONING,
    PassKind: PASS_SPATIAL_REASONING,
    Metadata: metadataFor(PASS_ID_SPATIAL_REASONING, "sensor"),
    Buffers: [
      matrixBuffer("ais-matrix:topos", 0.1),
      matrixBuffer("ais-matrix:route", 0.2),
      matrixBuffer("ais-matrix:threat", 0.3),
      matrixBuffer("ais-matrix:zoe", 0.4),
      {
        Name: "state-vector",
        LayoutName: "StateVector",
        Stride: 16,
        Count: 1,
        Values: Array.from({ length: 16 }, (_, index) => index / 16)
      }
    ]
  };
}

function hudEnvelope() {
  return {
    ...envelope,
    PassId: PASS_ID_HUD_COMPOSITE,
    PassKind: PASS_HUD_COMPOSITE,
    Metadata: metadataFor(PASS_ID_HUD_COMPOSITE, "hud"),
    Targets: [
      ...envelope.Targets,
      {
        Role: "hud",
        TargetId: "hud-vm",
        Kind: TARGET_HUD_COMPOSITE,
        Backend: BACKEND_WEBGPU,
        Width: 8,
        Height: 8,
        PixelFormat: 4,
        ZeroCopy: true
      }
    ],
    Buffers: [
      {
        Name: "hud-panel-rects",
        LayoutName: "HudPanelRect",
        Stride: 12,
        Count: 1,
        Values: Array.from({ length: 12 }, (_, index) => index)
      },
      {
        Name: "hud-panel-state-vectors",
        LayoutName: "HudPanelStateVector",
        Stride: 16,
        Count: 1,
        Values: Array.from({ length: 16 }, (_, index) => index / 10)
      }
    ],
    Scalars: {
      compassHeadingDegrees: 90,
      compassConfidence: 0.75,
      compassUsable: 1,
      radarMode: 0,
      kinesisX: 0.25,
      kinesisY: 0.5
    }
  };
}

function metadataFor(passId, pathRole) {
  return {
    rev3_authoritative_ready: "false",
    rev3_candidate_streak: "1",
    rev3_diagnostic_ready: "true",
    rev3_diagnostic_streak: "1",
    rev3_execution_mode: "browser-webgpu-compute",
    rev3_feature_mask_storage_texture: "false",
    rev3_frame_index: "42",
    rev3_pass_id: passId,
    rev3_pass_readiness: "Passes.{Aisthesis,SpatialReasoning,HudComposite}:Aisthesis{ShaderBound=false,PipelineCached=false,BuiltInExecutor=false,InjectedExecutor=false,ReadyForBuiltIn=false};SpatialReasoning{ShaderBound=false,PipelineCached=false,BuiltInExecutor=false,InjectedExecutor=false,ReadyForBuiltIn=false};HudComposite{ShaderBound=false,PipelineCached=false,BuiltInExecutor=false,InjectedExecutor=false,ReadyForBuiltIn=false}",
    rev3_path_role: pathRole,
    rev3_pilot_state: "browser-builtin-compute",
    rev3_promotion_gate: "trace-candidate",
    rev3_required_streak: "1",
    rev3_sample_ticks: "1001",
    provider_trace: "from-csharp-dto"
  };
}

function matrixBuffer(name, value) {
  return {
    Name: name,
    LayoutName: "AisMatrix",
    Stride: 81,
    Count: 1,
    Values: Array.from({ length: 81 }, () => value)
  };
}

function createFakeWebGpuDevice() {
  const device = {
    queue: {
      writeBuffer(buffer, offset, sourceBuffer, sourceOffset = 0, byteLength = sourceBuffer.byteLength) {
        const source = new Uint8Array(sourceBuffer, sourceOffset, byteLength);
        new Uint8Array(buffer.data).set(source, offset);
      },
      submit(commands) {
        for (const command of commands) {
          for (const operation of command) {
            if (operation.type === "compute") {
              writeComputeOutput(operation.bindGroup);
            }

            if (operation.type === "copy") {
              const source = new Uint8Array(operation.source.data, operation.sourceOffset, operation.size);
              new Uint8Array(operation.destination.data).set(source, operation.destinationOffset);
            }
          }
        }
      }
    },
    lost: new Promise(() => {}),
    createBuffer({ size }) {
      return new FakeBuffer(size);
    },
    createTexture({ label, size, format, usage }) {
      return {
        label,
        size,
        format,
        usage,
        createView() {
          return { label: `${label || "texture"}-view` };
        }
      };
    },
    createShaderModule(descriptor) {
      return { descriptor };
    },
    createComputePipeline(descriptor) {
      return {
        descriptor,
        getBindGroupLayout() {
          return {};
        }
      };
    },
    createBindGroup(descriptor) {
      return descriptor;
    },
    createCommandEncoder() {
      return new FakeCommandEncoder();
    }
  };
  return device;
}

function writeComputeOutput(bindGroup) {
  const output = bindGroup.entries.find(entry => entry.binding === 1)?.resource?.buffer;
  if (!output) {
    return;
  }

  const hasStorageInput = !!bindGroup.entries.find(entry => entry.binding === 0)?.resource?.buffer;
  const values = new Float32Array(output.data);
  values.fill(0);
  if (hasStorageInput) {
    values[0] = 17;
    values[4] = 0.5;
    return;
  }

  values[0] = 7;
  values[1] = 8;
  values[2] = 1;
  values[3] = 2;
}

class FakeBuffer {
  constructor(size) {
    this.data = new ArrayBuffer(size);
  }

  async mapAsync() {
  }

  getMappedRange(offset = 0, length = this.data.byteLength - offset) {
    return this.data.slice(offset, offset + length);
  }

  unmap() {
  }
}

class FakeCommandEncoder {
  constructor() {
    this.operations = [];
  }

  beginComputePass() {
    return new FakeComputePass(this.operations);
  }

  copyBufferToBuffer(source, sourceOffset, destination, destinationOffset, size) {
    this.operations.push({
      type: "copy",
      source,
      sourceOffset,
      destination,
      destinationOffset,
      size
    });
  }

  finish() {
    return this.operations;
  }
}

class FakeComputePass {
  constructor(operations) {
    this.operations = operations;
    this.bindGroup = null;
  }

  setPipeline() {
  }

  setBindGroup(_index, bindGroup) {
    this.bindGroup = bindGroup;
  }

  dispatchWorkgroups() {
  }

  end() {
    this.operations.push({
      type: "compute",
      bindGroup: this.bindGroup
    });
  }
}

await run();
