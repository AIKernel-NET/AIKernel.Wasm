export async function openWeightFile(name, pageSize) {
    const root = await navigator.storage.getDirectory();
    const entry = await root.getFileHandle(name);
    const access = await entry.createSyncAccessHandle({ mode: "read-only" });
    try {
        const size = access.getSize();
        if (!Number.isSafeInteger(size) || size <= 0) {
            throw new RangeError("The OPFS weight file must have a positive, precisely addressable length.");
        }
        return { access, size, page: new Uint8Array(pageSize) };
    } catch (error) {
        access.close();
        throw error;
    }
}

export function weightFileSize(file) {
    return file.size;
}

export function readWeightPage(file, offset, destination) {
    const { access, size, page } = file;
    const length = Math.min(page.length, size - offset);
    let filled = 0;
    while (filled < length) {
        const read = access.read(page.subarray(filled, length), { at: offset + filled });
        if (read === 0) {
            throw new Error("The OPFS weight file ended during a page read.");
        }
        filled += read;
    }
    page.fill(0, length);
    destination.set(page);
}

export function writeGpuPage(file, device, buffer, offset, source) {
    const { page } = file;
    if ((buffer.usage & GPUBufferUsage.COPY_DST) === 0 || offset > buffer.size - page.length) {
        throw new RangeError("The WebGPU COPY_DST buffer cannot hold the weight page at this offset.");
    }
    source.copyTo(page);
    device.queue.writeBuffer(buffer, offset, page);
}

export function closeWeightFile(file) {
    file.access.close();
}
