import { createReadStream } from 'node:fs';
import { open, stat } from 'node:fs/promises';
import path from 'node:path';
import { Pan123ApiError } from '../errors.js';
import { md5Buffer, md5File } from '../hash.js';
import { ApiModule, type AnyData } from './types.js';
import type {
  UploadCompleteData,
  UploadCreateData,
  UploadCreateParams,
  UploadFileOptions,
  UploadFileResult
} from '../types.js';

const SINGLE_UPLOAD_MAX_BYTES = 1024 * 1024 * 1024;

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

  complete(params: { preuploadID: string }): Promise<UploadCompleteData> {
    return this.http.request('POST', '/upload/v2/file/upload_complete', { body: params });
  }

  domain(): Promise<string[]> {
    return this.http.request('GET', '/upload/v2/file/domain');
  }

  single(params: UploadCreateParams & { uploadURL?: string; filePath: string }): Promise<UploadCompleteData> {
    return this.singleUpload(params);
  }

  sha1Reuse(params: AnyData): Promise<UploadCreateData> {
    return this.http.request('POST', '/upload/v2/file/sha1_reuse', { body: params });
  }

  async uploadFile(options: UploadFileOptions): Promise<UploadFileResult> {
    const fileStat = await stat(options.filePath);
    const filename = options.filename ?? path.basename(options.filePath);
    const etag = await md5File(options.filePath);
    const size = fileStat.size;
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
      const data = await this.singleUpload({ ...baseParams, filePath: options.filePath });
      return {
        fileID: data.fileID,
        completed: data.completed
      };
    }

    const created = await this.create(baseParams);
    if (created.reuse) {
      return {
        fileID: Number(created.fileID),
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
      await this.slice({
        uploadURL,
        preuploadID: created.preuploadID,
        sliceNo: index + 1,
        sliceMD5: md5Buffer(buffer),
        slice: buffer,
        filename: `${filename}.part${index + 1}`
      });
    }

    for (let attempt = 0; attempt < 60; attempt++) {
      const completed = await this.complete({ preuploadID: created.preuploadID });
      if (completed.completed && completed.fileID) {
        return {
          fileID: completed.fileID,
          completed: true
        };
      }
      await new Promise(resolve => setTimeout(resolve, 1000));
    }

    throw new Pan123ApiError({
      message: 'Upload completion did not finish after 60 polling attempts'
    });
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
}
