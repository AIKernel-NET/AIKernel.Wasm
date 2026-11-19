namespace AIKernel.Wasm.Tests;

using System.Collections;
using AIKernel.Wasm.Models;

public sealed class ModelCancellationTests
{
    [Theory]
    [InlineData("operation")]
    [InlineData("task")]
    [InlineData("derived")]
    [InlineData("linked")]
    public async Task ModelBoundaryPreservesCancellationBeforeGpuWork(string kind)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken); linked.Cancel();
        var token = kind == "linked" ? linked.Token : CancellationToken.None;
        OperationCanceledException expected = kind switch { "task" or "linked" => new TaskCanceledException("test", null, token), "derived" => new DerivedCancellation(token), _ => new("test", token) };
        var pending = new WebGpuResidentModelProvider().ExecuteModelAsync(Request(expected), TestContext.Current.CancellationToken).AsTask();
        var error = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
        Assert.Same(expected, error); Assert.Equal(token, error.CancellationToken); Assert.True(pending.IsCanceled);
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task NonCancellationRemainsStructuredFailure(bool timeout)
    {
        var result = await new WebGpuResidentModelProvider().ExecuteModelAsync(Request(timeout ? new TimeoutException("test") : new IOException("test")), TestContext.Current.CancellationToken);
        Assert.False(result.IsFailure); Assert.False(result.Value!.Succeeded);
        Assert.Equal("WASM_MODEL_EXECUTION_FAILED", result.Value.ErrorCode);
    }
    private static WebGpuResidentModelExecutionRequest Request(Exception error) => new()
    {
        KernelSource = "test-never-dispatched", Bindings = new FailingBindings(error)
    };
    private sealed class DerivedCancellation(CancellationToken token) : OperationCanceledException("test", token);
    // Enumerating bindings fails inside the protected execution boundary and before any GPU allocation.
    private sealed class FailingBindings(Exception error) : IReadOnlyList<WebGpuResidentModelBufferBinding>
    {
        public int Count => 1;
        public WebGpuResidentModelBufferBinding this[int index] => throw error;
        public IEnumerator<WebGpuResidentModelBufferBinding> GetEnumerator() => throw error;
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
