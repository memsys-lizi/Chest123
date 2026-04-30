import { describe, expect, it } from 'vitest';
import { createHash, randomBytes } from 'node:crypto';
import { createReadStream } from 'node:fs';
import { mkdtemp, rm, writeFile } from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';
import { createPan123Client, Pan123ApiError } from '../src/index.js';

interface ProbeResult {
  scenario: string;
  ok: boolean;
  data?: unknown;
  error?: {
    name: string;
    message: string;
    code?: number;
    status?: number;
    traceId?: string;
    response?: unknown;
  };
}

const runProbe =
  process.env.PAN123_UPLOAD_ERROR_PROBE === '1' &&
  Boolean(process.env.PAN123_CLIENT_ID && process.env.PAN123_CLIENT_SECRET);

const describeProbe = runProbe ? describe : describe.skip;

function md5(buffer: Buffer): string {
  return createHash('md5').update(buffer).digest('hex');
}

function uniqueName(prefix: string): string {
  return `${prefix}-${Date.now()}-${Math.random().toString(16).slice(2)}.txt`;
}

async function capture(scenario: string, task: () => Promise<unknown>): Promise<ProbeResult> {
  try {
    const data = await task();
    return { scenario, ok: true, data };
  } catch (error) {
    if (error instanceof Pan123ApiError) {
      return {
        scenario,
        ok: false,
        error: {
          name: error.name,
          message: error.message,
          code: error.code,
          status: error.status,
          traceId: error.traceId,
          response: error.response
        }
      };
    }
    const fallback = error instanceof Error ? error : new Error(String(error));
    return {
      scenario,
      ok: false,
      error: {
        name: fallback.name,
        message: fallback.message
      }
    };
  }
}

async function delay(ms: number): Promise<void> {
  await new Promise(resolve => setTimeout(resolve, ms));
}

describeProbe('123Pan upload error probe', () => {
  it(
    'records live error and incomplete upload responses',
    async () => {
      const client = createPan123Client({
        clientId: process.env.PAN123_CLIENT_ID,
        clientSecret: process.env.PAN123_CLIENT_SECRET,
        timeoutMs: 120000
      });
      const parentFileID = Number(process.env.PAN123_PARENT_FILE_ID ?? 0);
      const invalidParentFileID = 999_999_999_999_999;
      const results: ProbeResult[] = [];
      const uploadedFileIDs: number[] = [];
      const dir = await mkdtemp(path.join(os.tmpdir(), 'pan123-upload-probe-'));

      async function findFileByName(filename: string): Promise<unknown> {
        for (let attempt = 1; attempt <= 4; attempt++) {
          const list = await client.files.list({
            parentFileId: parentFileID,
            limit: 100,
            searchData: filename,
            searchMode: 1
          });
          const matched = list.fileList.find(file => file.filename === filename);
          const fileID = Number(matched?.fileID ?? matched?.fileId);
          if (Number.isFinite(fileID) && fileID > 0) {
            uploadedFileIDs.push(fileID);
            return { attempt, matched };
          }
          await delay(3000);
        }
        return null;
      }

      try {
        const smallBuffer = Buffer.from(`small-${Date.now()}-${Math.random()}`);
        const sliceBuffer = Buffer.from(`slice-${Date.now()}-${Math.random()}`);
        const forcedSliceBuffer = Buffer.from(`forced-slice-${Date.now()}-${Math.random()}`);
        const realSliceBuffer = Buffer.concat([
          Buffer.from(`real-slice-${Date.now()}-${Math.random()}`),
          randomBytes(17 * 1024 * 1024)
        ]);
        const smallPath = path.join(dir, 'small.txt');
        const slicePath = path.join(dir, 'slice.txt');
        const forcedSlicePath = path.join(dir, 'forced-slice.txt');
        const realSlicePath = path.join(dir, 'real-slice.bin');
        await writeFile(smallPath, smallBuffer);
        await writeFile(slicePath, sliceBuffer);
        await writeFile(forcedSlicePath, forcedSliceBuffer);
        await writeFile(realSlicePath, realSliceBuffer);

        const domains = await client.upload.domain();
        const uploadURL = domains[0];
        expect(uploadURL).toBeTruthy();

        results.push(
          await capture('v2_create_invalid_filename', () =>
            client.upload.create({
              parentFileID,
              filename: 'bad:name.txt',
              etag: md5(smallBuffer),
              size: smallBuffer.length,
              duplicate: 1
            })
          )
        );

        results.push(
          await capture('v2_create_invalid_parent', () =>
            client.upload.create({
              parentFileID: invalidParentFileID,
              filename: uniqueName('invalid-parent-create'),
              etag: md5(smallBuffer),
              size: smallBuffer.length,
              duplicate: 1
            })
          )
        );

        results.push(
          await capture('v2_single_invalid_parent', () =>
            client.request('POST', '/upload/v2/file/single/create', {
              baseURL: uploadURL,
              form: {
                file: {
                  value: createReadStream(smallPath),
                  options: 'small.txt'
                },
                parentFileID: invalidParentFileID,
                filename: uniqueName('invalid-parent-single'),
                etag: md5(smallBuffer),
                size: smallBuffer.length,
                duplicate: 1
              }
            })
          )
        );

        const singleBadEtagName = uniqueName('bad-etag-single');
        results.push(
          await capture('v2_single_bad_etag', () =>
            client.request('POST', '/upload/v2/file/single/create', {
              baseURL: uploadURL,
              form: {
                file: {
                  value: createReadStream(smallPath),
                  options: singleBadEtagName
                },
                parentFileID,
                filename: singleBadEtagName,
                etag: '00000000000000000000000000000000',
                size: smallBuffer.length,
                duplicate: 1
              }
            })
          )
        );

        const created = await client.upload.create({
          parentFileID,
          filename: uniqueName('complete-without-slices'),
          etag: md5(sliceBuffer),
          size: sliceBuffer.length,
          duplicate: 1
        });
        results.push({ scenario: 'v2_create_for_incomplete_upload', ok: true, data: created });

        if (created.preuploadID) {
          results.push(
            await capture('v2_complete_without_slices', () =>
              client.upload.complete({
                preuploadID: created.preuploadID!,
                checkingRetryAttempts: 3,
                checkingRetryDelayMs: 1000
              })
            )
          );

          results.push(
            await capture('v2_slice_bad_md5', () =>
              client.upload.slice({
                uploadURL: created.servers?.[0] ?? uploadURL,
                preuploadID: created.preuploadID!,
                sliceNo: 1,
                sliceMD5: '00000000000000000000000000000000',
                slice: sliceBuffer,
                filename: 'slice.txt'
              })
            )
          );
        }

        const singleSuccessName = uniqueName('success-single');
        const successful = await capture('v2_single_success_control', () =>
          client.upload.single({
            uploadURL,
            filePath: smallPath,
            parentFileID,
            filename: singleSuccessName,
            etag: md5(smallBuffer),
            size: smallBuffer.length,
            duplicate: 1
          })
        );
        results.push(successful);
        const maybeFileID = Number((successful.data as { fileID?: unknown } | undefined)?.fileID);
        if (Number.isFinite(maybeFileID) && maybeFileID > 0) uploadedFileIDs.push(maybeFileID);

        if (!maybeFileID) {
          results.push(await capture('v2_single_pending_find_by_filename', () => findFileByName(singleSuccessName)));
        }

        const highLevelName = uniqueName('upload-file-helper-single');
        const highLevelSingle = await capture('sdk_uploadFile_single_path_current_behavior', () =>
          client.upload.uploadFile({
            filePath: smallPath,
            parentFileID,
            filename: highLevelName,
            duplicate: 1
          })
        );
        results.push(highLevelSingle);
        const highLevelFileID = Number((highLevelSingle.data as { fileID?: unknown } | undefined)?.fileID);
        if (Number.isFinite(highLevelFileID) && highLevelFileID > 0) uploadedFileIDs.push(highLevelFileID);
        if (!highLevelFileID) {
          results.push(await capture('sdk_uploadFile_single_pending_find_by_filename', () => findFileByName(highLevelName)));
        }

        const forcedSliceName = uniqueName('upload-file-helper-forced-slice');
        const forcedSlice = await capture('sdk_uploadFile_forced_slice_path_current_behavior', () =>
          client.upload.uploadFile({
            filePath: forcedSlicePath,
            parentFileID,
            filename: forcedSliceName,
            duplicate: 1,
            singleUploadMaxBytes: 1
          })
        );
        results.push(forcedSlice);
        const forcedSliceFileID = Number((forcedSlice.data as { fileID?: unknown } | undefined)?.fileID);
        if (Number.isFinite(forcedSliceFileID) && forcedSliceFileID > 0) uploadedFileIDs.push(forcedSliceFileID);
        if (!forcedSliceFileID) {
          results.push(await capture('sdk_uploadFile_forced_slice_find_by_filename', () => findFileByName(forcedSliceName)));
        }

        const realSliceName = uniqueName('upload-file-helper-real-slice').replace(/\.txt$/, '.bin');
        const realSlice = await capture('sdk_uploadFile_real_slice_path_current_behavior', () =>
          client.upload.uploadFile({
            filePath: realSlicePath,
            parentFileID,
            filename: realSliceName,
            duplicate: 1,
            singleUploadMaxBytes: 1
          })
        );
        results.push(realSlice);
        const realSliceFileID = Number((realSlice.data as { fileID?: unknown } | undefined)?.fileID);
        if (Number.isFinite(realSliceFileID) && realSliceFileID > 0) uploadedFileIDs.push(realSliceFileID);
        if (!realSliceFileID) {
          results.push(await capture('sdk_uploadFile_real_slice_find_by_filename', () => findFileByName(realSliceName)));
        }

        console.log(`PAN123_UPLOAD_ERROR_PROBE_RESULTS=${JSON.stringify(results, null, 2)}`);

        expect(results.length).toBeGreaterThan(0);
      } finally {
        if (uploadedFileIDs.length > 0) {
          await capture('cleanup_uploaded_files', () => client.files.trash({ fileIDs: uploadedFileIDs }));
        }
        await rm(dir, { recursive: true, force: true });
      }
    },
    180000
  );
});
