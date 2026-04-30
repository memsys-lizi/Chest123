import { createReadStream } from 'node:fs';
import { open, stat } from 'node:fs/promises';
import path from 'node:path';
import { Pan123ApiError } from '../errors.js';
import { md5Buffer, md5File } from '../hash.js';
import { retryWhileFileChecking, type FileCheckingRetryOptions } from './file-checking.js';
import { ApiModule, type AnyData } from './types.js';
import type {
  UploadCompleteData,
  UploadCreateData,
  UploadCreateParams,
  UploadProgressEvent,
  UploadFileOptions,
  UploadFileResult
} from '../types.js';

const SINGLE_UPLOAD_MAX_BYTES = 1024 * 1024 * 1024;
const DEFAULT_COMPLETE_POLLING_ATTEMPTS = 60;
const DEFAULT_COMPLETE_POLLING_DELAY_MS = 1000;
const DEFAULT_TRANSIENT_RETRY_ATTEMPTS = 5;
const DEFAULT_TRANSIENT_RETRY_DELAY_MS = 1000;

interface UploadCompleteParams extends FileCheckingRetryOptions {
  preuploadID: string;
}

interface UploadRetryOptions {
  transientRetryAttempts?: number;
  transientRetryDelayMs?: number;
}

interface CompletePollingOptions {
  completePollingAttempts?: number;
  completePollingDelayMs?: number;
}

type ProgressCallback = UploadFileOptions['onProgress'];

async function readFileRange(filePath: string, start: number, length: number): Promise<Buffer> {
  const handle = await open(filePath, 'r');
  try {
    const buffer = Buffer.allocUnsafe(length);
    const result = await handle.read(buffer, 0, length, start);
    return result.bytesRead === length ? buffer : buffer.subarray(0, result.bytesRead);
  } finally {
    await handle.close();
  }
}

function delay(ms: number): Promise<void> {
  return new Promise(resolve => setTimeout(resolve, ms));
}

function positiveInteger(value: number | undefined, fallback: number): number {
  return Math.max(1, Math.floor(value ?? fallback));
}

function nonNegativeInteger(value: number | undefined, fallback: number): number {
  return Math.max(0, Math.floor(value ?? fallback));
}

function isCompletedUpload(data: UploadCompleteData | undefined): data is UploadCompleteData {
  return Boolean(data?.completed && data.fileID > 0);
}

function assertCompletedUpload(data: UploadCompleteData, context: string): UploadCompleteData {
  if (isCompletedUpload(data)) return data;
  throw new Pan123ApiError({
    message: `${context} did not return a completed upload with a valid fileID.`,
    response: data
  });
}

function isTransientUploadError(error: unknown): boolean {
  if (!(error instanceof Pan123ApiError)) return false;
  if (error.status === 429 || error.code === 429) return true;
  const message = error.message;
  return error.code === 1 && (message.includes('秒传队列') || message.includes('削峰') || message.includes('请慢一点'));
}

async function retryTransientUpload<T>(task: () => Promise<T>, options: UploadRetryOptions = {}): Promise<T> {
  const attempts = positiveInteger(options.transientRetryAttempts, DEFAULT_TRANSIENT_RETRY_ATTEMPTS);
  const delayMs = nonNegativeInteger(options.transientRetryDelayMs, DEFAULT_TRANSIENT_RETRY_DELAY_MS);

  for (let attempt = 1; attempt <= attempts; attempt++) {
    try {
      return await task();
    } catch (error) {
      if (!isTransientUploadError(error) || attempt === attempts) {
        throw error;
      }
      await delay(delayMs);
    }
  }

  throw new Pan123ApiError({ message: 'Transient upload retry attempts were exhausted.' });
}

async function emitProgress(callback: ProgressCallback, event: UploadProgressEvent): Promise<void> {
  await callback?.(event);
}

export class UploadModule extends ApiModule {
  create(params: UploadCreateParams): Promise<UploadCreateData> {
    return this.http.request('POST', '/upload/v2/file/create', { body: params });
  }

  slice(params: {
    uploadURL: string;
    preuploadID: string;
    sliceNo: number;
    sliceMD5: string;
    slice: Buffer | NodeJS.ReadableStream;
    filename?: string;
  }): Promise<unknown> {
    return this.http.request('POST', '/upload/v2/file/slice', {
      baseURL: params.uploadURL,
      form: {
        preuploadID: params.preuploadID,
        sliceNo: params.sliceNo,
        sliceMD5: params.sliceMD5,
        slice: {
          value: params.slice,
          options: params.filename
        }
      }
    });
  }

  complete(params: UploadCompleteParams): Promise<UploadCompleteData> {
    return retryWhileFileChecking(
      () =>
        this.http.request('POST', '/upload/v2/file/upload_complete', {
          body: {
            preuploadID: params.preuploadID
          }
        }),
      params
    );
  }

  waitComplete(params: UploadCompleteParams & CompletePollingOptions): Promise<UploadCompleteData> {
    return this.waitForUploadComplete(params.preuploadID, params, undefined, 0);
  }

  domain(): Promise<string[]> {
    return this.http.request('GET', '/upload/v2/file/domain');
  }

  single(params: UploadCreateParams & { uploadURL?: string; filePath: string }): Promise<UploadCompleteData> {
    return this.singleUpload(params).then(data => assertCompletedUpload(data, 'Single upload'));
  }

  sha1Reuse(params: AnyData): Promise<UploadCreateData> {
    return this.http.request('POST', '/upload/v2/file/sha1_reuse', { body: params });
  }

  async uploadFile(options: UploadFileOptions): Promise<UploadFileResult> {
    const fileStat = await stat(options.filePath);
    const filename = options.filename ?? path.basename(options.filePath);
    await emitProgress(options.onProgress, {
      stage: 'hashing',
      loadedBytes: 0,
      totalBytes: fileStat.size,
      percent: 0
    });
    const etag = await md5File(options.filePath);
    const size = fileStat.size;
    await emitProgress(options.onProgress, {
      stage: 'hashing',
      loadedBytes: size,
      totalBytes: size,
      percent: 100
    });
    const baseParams: UploadCreateParams = {
      parentFileID: options.parentFileID,
      filename,
      etag,
      size,
      duplicate: options.duplicate,
      containDir: options.containDir
    };
    const maxSingleBytes = options.singleUploadMaxBytes ?? SINGLE_UPLOAD_MAX_BYTES;

    if (size <= maxSingleBytes) {
      const uploadURL = (await this.domain())[0];
      if (!uploadURL) {
        throw new Pan123ApiError({ message: 'No upload domain returned by /upload/v2/file/domain' });
      }
      const data = await retryTransientUpload(
        () => this.singleUpload({ ...baseParams, uploadURL, filePath: options.filePath }),
        options
      );
      const completed = assertCompletedUpload(data, 'Single upload');
      await emitProgress(options.onProgress, {
        stage: 'single',
        loadedBytes: size,
        totalBytes: size,
        percent: 100
      });
      return {
        fileID: completed.fileID,
        completed: true
      };
    }

    await emitProgress(options.onProgress, {
      stage: 'create',
      loadedBytes: 0,
      totalBytes: size,
      percent: 0
    });
    const created = await retryTransientUpload(() => this.create(baseParams), options);
    if (created.reuse) {
      if (!created.fileID || created.fileID <= 0) {
        throw new Pan123ApiError({
          message: 'Upload create reported reuse but did not return a valid fileID.',
          response: created
        });
      }
      await emitProgress(options.onProgress, {
        stage: 'reuse',
        loadedBytes: size,
        totalBytes: size,
        percent: 100
      });
      return {
        fileID: created.fileID,
        completed: true,
        reuse: true
      };
    }
    if (!created.preuploadID || !created.sliceSize || !created.servers?.length) {
      throw new Pan123ApiError({
        message: 'Upload create did not return preuploadID, sliceSize, or servers'
      });
    }

    const uploadURL = created.servers[0];
    const sliceSize = created.sliceSize;
    const totalSlices = Math.ceil(size / sliceSize);
    for (let index = 0; index < totalSlices; index++) {
      const start = index * sliceSize;
      const length = Math.min(sliceSize, size - start);
      const buffer = await readFileRange(options.filePath, start, length);
      const sliceNo = index + 1;
      await retryTransientUpload(
        () =>
          this.slice({
            uploadURL,
            preuploadID: created.preuploadID!,
            sliceNo,
            sliceMD5: md5Buffer(buffer),
            slice: buffer,
            filename: `${filename}.part${sliceNo}`
          }),
        options
      );
      const loadedBytes = Math.min(size, start + length);
      await emitProgress(options.onProgress, {
        stage: 'slice',
        loadedBytes,
        totalBytes: size,
        percent: size === 0 ? 100 : (loadedBytes / size) * 100,
        sliceNo,
        totalSlices,
        completedSlices: sliceNo
      });
    }

    const completed = await this.waitForUploadComplete(created.preuploadID, options, options.onProgress, size);
    return {
      fileID: completed.fileID,
      completed: true
    };
  }

  private async singleUpload(params: UploadCreateParams & { uploadURL?: string; filePath: string }): Promise<UploadCompleteData> {
    const uploadURL = params.uploadURL ?? (await this.domain())[0];
    if (!uploadURL) {
      throw new Pan123ApiError({ message: 'No upload domain returned by /upload/v2/file/domain' });
    }
    return this.http.request('POST', '/upload/v2/file/single/create', {
      baseURL: uploadURL,
      form: {
        file: {
          value: createReadStream(params.filePath),
          options: params.filename
        },
        parentFileID: params.parentFileID,
        filename: params.filename,
        etag: params.etag,
        size: params.size,
        duplicate: params.duplicate,
        containDir: params.containDir
      }
    });
  }

  private completeOnce(preuploadID: string): Promise<UploadCompleteData> {
    return this.http.request('POST', '/upload/v2/file/upload_complete', {
      body: {
        preuploadID
      }
    });
  }

  private async waitForUploadComplete(
    preuploadID: string,
    options: CompletePollingOptions & UploadRetryOptions,
    onProgress: ProgressCallback,
    totalBytes: number
  ): Promise<UploadCompleteData> {
    const attempts = positiveInteger(options.completePollingAttempts, DEFAULT_COMPLETE_POLLING_ATTEMPTS);
    const delayMs = nonNegativeInteger(options.completePollingDelayMs, DEFAULT_COMPLETE_POLLING_DELAY_MS);
    let lastResponse: UploadCompleteData | undefined;

    for (let attempt = 1; attempt <= attempts; attempt++) {
      try {
        const completed = await retryTransientUpload(() => this.completeOnce(preuploadID), options);
        lastResponse = completed;
        if (isCompletedUpload(completed)) {
          await emitProgress(onProgress, {
            stage: 'complete',
            loadedBytes: totalBytes,
            totalBytes,
            percent: 100,
            attempt
          });
          return completed;
        }
      } catch (error) {
        if (!(error instanceof Pan123ApiError && error.code === 20103) || attempt === attempts) {
          throw error;
        }
      }

      await emitProgress(onProgress, {
        stage: 'complete',
        loadedBytes: totalBytes,
        totalBytes,
        percent: 100,
        attempt
      });
      if (attempt < attempts) {
        await delay(delayMs);
      }
    }

    throw new Pan123ApiError({
      message: `Upload completion did not return completed=true with a valid fileID after ${attempts} polling attempts.`,
      response: lastResponse
    });
  }
}
