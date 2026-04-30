import { afterEach, describe, expect, it } from 'vitest';
import nock from 'nock';
import { mkdtemp, rm, writeFile } from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';
import FormData from 'form-data';
import { createPan123Client, Pan123ApiError } from '../src/index.js';
import { appendFormFields } from '../src/internal/form.js';

const baseURL = 'https://open-api.123pan.com';
const uploadURL = 'https://upload.example.com';

function mockToken(expiredAt = '2099-01-01T00:00:00+08:00') {
  return nock(baseURL)
    .post('/api/v1/access_token', {
      clientID: 'client',
      clientSecret: 'secret'
    })
    .matchHeader('Platform', 'open_platform')
    .reply(200, {
      code: 0,
      message: 'ok',
      data: {
        accessToken: 'token-123',
        expiredAt
      },
      'x-traceID': 'trace-token'
    });
}

describe('Pan123Client', () => {
  afterEach(() => {
    nock.cleanAll();
  });

  it('gets and caches access tokens while adding common headers', async () => {
    const tokenScope = mockToken();
    const userScope = nock(baseURL)
      .get('/api/v1/user/info')
      .twice()
      .matchHeader('Platform', 'open_platform')
      .matchHeader('Authorization', 'Bearer token-123')
      .reply(200, {
        code: 0,
        message: 'ok',
        data: { uid: 1 }
      });

    const client = createPan123Client({ clientId: 'client', clientSecret: 'secret' });
    await expect(client.user.info()).resolves.toEqual({ uid: 1 });
    await expect(client.user.info()).resolves.toEqual({ uid: 1 });

    expect(tokenScope.isDone()).toBe(true);
    expect(userScope.isDone()).toBe(true);
  });

  it('refreshes expired supplied tokens', async () => {
    const tokenScope = mockToken();
    const listScope = nock(baseURL)
      .get('/api/v2/file/list')
      .query({ parentFileId: 0, limit: 100 })
      .matchHeader('Authorization', 'Bearer token-123')
      .reply(200, {
        code: 0,
        message: 'ok',
        data: { lastFileId: -1, fileList: [] }
      });

    const client = createPan123Client({
      clientId: 'client',
      clientSecret: 'secret',
      accessToken: 'old-token',
      tokenExpiresAt: Date.now() - 1000
    });

    await expect(client.files.list({ parentFileId: 0, limit: 100 })).resolves.toEqual({
      lastFileId: -1,
      fileList: []
    });
    expect(tokenScope.isDone()).toBe(true);
    expect(listScope.isDone()).toBe(true);
  });

  it('throws Pan123ApiError when API code is non-zero', async () => {
    mockToken();
    nock(baseURL)
      .get('/api/v1/user/info')
      .reply(200, {
        code: 401,
        message: 'access_token无效',
        data: null,
        'x-traceID': 'trace-error'
      });

    const client = createPan123Client({ clientId: 'client', clientSecret: 'secret' });
    await expect(client.user.info()).rejects.toMatchObject({
      name: 'Pan123ApiError',
      code: 401,
      traceId: 'trace-error'
    } satisfies Partial<Pan123ApiError>);
  });

  it('supports the low-level request escape hatch', async () => {
    mockToken();
    const scope = nock(baseURL)
      .get('/api/v1/file/detail')
      .query({ fileID: 42 })
      .reply(200, {
        code: 0,
        message: 'ok',
        data: { fileID: 42 }
      });

    const client = createPan123Client({ clientId: 'client', clientSecret: 'secret' });
    await expect(client.request('GET', '/api/v1/file/detail', { query: { fileID: 42 } })).resolves.toEqual({
      fileID: 42
    });
    expect(scope.isDone()).toBe(true);
  });

  it('serializes primitive multipart fields as strings', () => {
    const form = new FormData();
    appendFormFields(form, {
      parentFileID: 0,
      size: 5,
      duplicate: 1,
      containDir: false,
      filename: 'small.txt',
      file: Buffer.from('hello')
    });

    const body = form.getBuffer().toString('utf8');
    expect(body).toContain('\r\n0\r\n');
    expect(body).toContain('\r\n5\r\n');
    expect(body).toContain('\r\n1\r\n');
    expect(body).toContain('\r\nfalse\r\n');
  });

  it('retries direct-link URL lookup while the file is still checking', async () => {
    mockToken();
    const checkingResponses = nock(baseURL)
      .get('/api/v1/direct-link/url')
      .query({ fileID: 1001 })
      .twice()
      .reply(200, {
        code: 20103,
        message: '文件正在校验中,请间隔1秒后再试',
        data: null,
        'x-traceID': 'trace-checking'
      });
    const urlResponse = nock(baseURL)
      .get('/api/v1/direct-link/url')
      .query({ fileID: 1001 })
      .reply(200, {
        code: 0,
        message: 'ok',
        data: { url: 'https://example.com/file.txt' }
      });

    const client = createPan123Client({ clientId: 'client', clientSecret: 'secret' });
    await expect(
      client.directLink.url({
        fileID: 1001,
        checkingRetryAttempts: 3,
        checkingRetryDelayMs: 0
      })
    ).resolves.toEqual({ url: 'https://example.com/file.txt' });
    expect(checkingResponses.isDone()).toBe(true);
    expect(urlResponse.isDone()).toBe(true);
  });

  it('retries download info lookup while the file is still checking', async () => {
    mockToken();
    const checkingResponse = nock(baseURL)
      .get('/api/v1/file/download_info')
      .query({ fileId: 1002 })
      .reply(200, {
        code: 20103,
        message: '文件正在校验中,请间隔1秒后再试',
        data: null,
        'x-traceID': 'trace-checking'
      });
    const downloadResponse = nock(baseURL)
      .get('/api/v1/file/download_info')
      .query({ fileId: 1002 })
      .reply(200, {
        code: 0,
        message: 'ok',
        data: { downloadUrl: 'https://example.com/download/file.txt' }
      });

    const client = createPan123Client({ clientId: 'client', clientSecret: 'secret' });
    await expect(
      client.files.downloadInfo({
        fileId: 1002,
        checkingRetryAttempts: 2,
        checkingRetryDelayMs: 0
      })
    ).resolves.toEqual({ downloadUrl: 'https://example.com/download/file.txt' });
    expect(checkingResponse.isDone()).toBe(true);
    expect(downloadResponse.isDone()).toBe(true);
  });

  it('retries upload completion while the file is still checking', async () => {
    mockToken();
    const checkingResponse = nock(baseURL)
      .post('/upload/v2/file/upload_complete', { preuploadID: 'pre-checking' })
      .reply(200, {
        code: 20103,
        message: '文件正在校验中,请间隔1秒后再试',
        data: null,
        'x-traceID': 'trace-checking'
      });
    const completeResponse = nock(baseURL)
      .post('/upload/v2/file/upload_complete', { preuploadID: 'pre-checking' })
      .reply(200, {
        code: 0,
        message: 'ok',
        data: { completed: true, fileID: 1003 }
      });

    const client = createPan123Client({ clientId: 'client', clientSecret: 'secret' });
    await expect(
      client.upload.complete({
        preuploadID: 'pre-checking',
        checkingRetryAttempts: 2,
        checkingRetryDelayMs: 0
      })
    ).resolves.toEqual({ completed: true, fileID: 1003 });
    expect(checkingResponse.isDone()).toBe(true);
    expect(completeResponse.isDone()).toBe(true);
  });

  it('uploads small files with the V2 single-upload helper', async () => {
    const dir = await mkdtemp(path.join(os.tmpdir(), 'pan123-small-'));
    const filePath = path.join(dir, 'small.txt');
    await writeFile(filePath, 'hello');

    try {
      mockToken();
      nock(baseURL)
        .get('/upload/v2/file/domain')
        .reply(200, {
          code: 0,
          message: 'ok',
          data: [uploadURL]
        });
      const singleScope = nock(uploadURL)
        .post('/upload/v2/file/single/create')
        .matchHeader('Platform', 'open_platform')
        .matchHeader('Authorization', 'Bearer token-123')
        .reply(200, {
          code: 0,
          message: 'ok',
          data: { fileID: 1001, completed: true }
        });

      const client = createPan123Client({ clientId: 'client', clientSecret: 'secret' });
      await expect(
        client.upload.uploadFile({
          filePath,
          parentFileID: 0,
          duplicate: 1
        })
      ).resolves.toEqual({ fileID: 1001, completed: true });
      expect(singleScope.isDone()).toBe(true);
    } finally {
      await rm(dir, { recursive: true, force: true });
    }
  });

  it('rejects single uploads that do not return completed=true and a valid fileID', async () => {
    const dir = await mkdtemp(path.join(os.tmpdir(), 'pan123-incomplete-single-'));
    const filePath = path.join(dir, 'small.txt');
    await writeFile(filePath, 'hello');

    try {
      mockToken();
      nock(baseURL)
        .get('/upload/v2/file/domain')
        .reply(200, {
          code: 0,
          message: 'ok',
          data: [uploadURL]
        });
      nock(uploadURL)
        .post('/upload/v2/file/single/create')
        .reply(200, {
          code: 0,
          message: 'ok',
          data: { fileID: 0, completed: false }
        });

      const client = createPan123Client({ clientId: 'client', clientSecret: 'secret' });
      await expect(
        client.upload.uploadFile({
          filePath,
          parentFileID: 0,
          duplicate: 1
        })
      ).rejects.toMatchObject({
        name: 'Pan123ApiError',
        message: 'Single upload did not return a completed upload with a valid fileID.'
      });
    } finally {
      await rm(dir, { recursive: true, force: true });
    }
  });

  it('retries transient upload queue responses before failing the high-level upload', async () => {
    const dir = await mkdtemp(path.join(os.tmpdir(), 'pan123-queue-retry-'));
    const filePath = path.join(dir, 'small.txt');
    await writeFile(filePath, 'hello');

    try {
      mockToken();
      nock(baseURL)
        .get('/upload/v2/file/domain')
        .reply(200, {
          code: 0,
          message: 'ok',
          data: [uploadURL]
        });
      const queued = nock(uploadURL)
        .post('/upload/v2/file/single/create')
        .reply(200, {
          code: 1,
          message: '该任务已成功进入秒传队列,任务队列削峰中,未直接获取到文件ID,请慢一点',
          data: null,
          'x-traceID': 'trace-queue'
        });
      const completed = nock(uploadURL)
        .post('/upload/v2/file/single/create')
        .reply(200, {
          code: 0,
          message: 'ok',
          data: { fileID: 4004, completed: true }
        });

      const client = createPan123Client({ clientId: 'client', clientSecret: 'secret' });
      await expect(
        client.upload.uploadFile({
          filePath,
          parentFileID: 0,
          duplicate: 1,
          transientRetryAttempts: 2,
          transientRetryDelayMs: 0
        })
      ).resolves.toEqual({ fileID: 4004, completed: true });
      expect(queued.isDone()).toBe(true);
      expect(completed.isDone()).toBe(true);
    } finally {
      await rm(dir, { recursive: true, force: true });
    }
  });

  it('uploads large files with the V2 multipart helper', async () => {
    const dir = await mkdtemp(path.join(os.tmpdir(), 'pan123-large-'));
    const filePath = path.join(dir, 'large.txt');
    await writeFile(filePath, 'abcdef');

    try {
      mockToken();
      nock(baseURL)
        .post('/upload/v2/file/create')
        .reply(200, {
          code: 0,
          message: 'ok',
          data: {
            fileID: 0,
            reuse: false,
            preuploadID: 'pre-1',
            sliceSize: 2,
            servers: [uploadURL]
          }
        });
      const slices = nock(uploadURL)
        .post('/upload/v2/file/slice')
        .times(3)
        .reply(200, {
          code: 0,
          message: 'ok',
          data: null
        });
      nock(baseURL)
        .post('/upload/v2/file/upload_complete', { preuploadID: 'pre-1' })
        .reply(200, {
          code: 0,
          message: 'ok',
          data: { fileID: 2002, completed: true }
        });

      const client = createPan123Client({ clientId: 'client', clientSecret: 'secret' });
      await expect(
        client.upload.uploadFile({
          filePath,
          parentFileID: 0,
          singleUploadMaxBytes: 1
        })
      ).resolves.toEqual({ fileID: 2002, completed: true });
      expect(slices.isDone()).toBe(true);
    } finally {
      await rm(dir, { recursive: true, force: true });
    }
  });

  it('polls multipart completion until completed=true with a valid fileID', async () => {
    const dir = await mkdtemp(path.join(os.tmpdir(), 'pan123-poll-complete-'));
    const filePath = path.join(dir, 'large.txt');
    await writeFile(filePath, 'abcdef');

    try {
      mockToken();
      nock(baseURL)
        .post('/upload/v2/file/create')
        .reply(200, {
          code: 0,
          message: 'ok',
          data: {
            fileID: 0,
            reuse: false,
            preuploadID: 'pre-poll',
            sliceSize: 2,
            servers: [uploadURL]
          }
        });
      nock(uploadURL)
        .post('/upload/v2/file/slice')
        .times(3)
        .reply(200, {
          code: 0,
          message: 'ok',
          data: null
        });
      const incomplete = nock(baseURL)
        .post('/upload/v2/file/upload_complete', { preuploadID: 'pre-poll' })
        .twice()
        .reply(200, {
          code: 0,
          message: 'ok',
          data: { fileID: 0, completed: false }
        });
      const complete = nock(baseURL)
        .post('/upload/v2/file/upload_complete', { preuploadID: 'pre-poll' })
        .reply(200, {
          code: 0,
          message: 'ok',
          data: { fileID: 3003, completed: true }
        });

      const client = createPan123Client({ clientId: 'client', clientSecret: 'secret' });
      await expect(
        client.upload.uploadFile({
          filePath,
          parentFileID: 0,
          singleUploadMaxBytes: 1,
          completePollingAttempts: 3,
          completePollingDelayMs: 0
        })
      ).resolves.toEqual({ fileID: 3003, completed: true });
      expect(incomplete.isDone()).toBe(true);
      expect(complete.isDone()).toBe(true);
    } finally {
      await rm(dir, { recursive: true, force: true });
    }
  });

  it('retries multipart completion while the API reports file checking', async () => {
    const dir = await mkdtemp(path.join(os.tmpdir(), 'pan123-check-complete-'));
    const filePath = path.join(dir, 'large.txt');
    await writeFile(filePath, 'abcdef');

    try {
      mockToken();
      nock(baseURL)
        .post('/upload/v2/file/create')
        .reply(200, {
          code: 0,
          message: 'ok',
          data: {
            fileID: 0,
            reuse: false,
            preuploadID: 'pre-check',
            sliceSize: 2,
            servers: [uploadURL]
          }
        });
      nock(uploadURL)
        .post('/upload/v2/file/slice')
        .times(3)
        .reply(200, {
          code: 0,
          message: 'ok',
          data: null
        });
      const checking = nock(baseURL)
        .post('/upload/v2/file/upload_complete', { preuploadID: 'pre-check' })
        .reply(200, {
          code: 20103,
          message: '文件正在校验中,请间隔1秒后再试',
          data: null,
          'x-traceID': 'trace-checking'
        });
      const complete = nock(baseURL)
        .post('/upload/v2/file/upload_complete', { preuploadID: 'pre-check' })
        .reply(200, {
          code: 0,
          message: 'ok',
          data: { fileID: 3004, completed: true }
        });

      const client = createPan123Client({ clientId: 'client', clientSecret: 'secret' });
      await expect(
        client.upload.uploadFile({
          filePath,
          parentFileID: 0,
          singleUploadMaxBytes: 1,
          completePollingAttempts: 2,
          completePollingDelayMs: 0
        })
      ).resolves.toEqual({ fileID: 3004, completed: true });
      expect(checking.isDone()).toBe(true);
      expect(complete.isDone()).toBe(true);
    } finally {
      await rm(dir, { recursive: true, force: true });
    }
  });

  it('rejects multipart uploads when completion never yields a valid fileID', async () => {
    const dir = await mkdtemp(path.join(os.tmpdir(), 'pan123-never-complete-'));
    const filePath = path.join(dir, 'large.txt');
    await writeFile(filePath, 'abcdef');

    try {
      mockToken();
      nock(baseURL)
        .post('/upload/v2/file/create')
        .reply(200, {
          code: 0,
          message: 'ok',
          data: {
            fileID: 0,
            reuse: false,
            preuploadID: 'pre-never',
            sliceSize: 2,
            servers: [uploadURL]
          }
        });
      nock(uploadURL)
        .post('/upload/v2/file/slice')
        .times(3)
        .reply(200, {
          code: 0,
          message: 'ok',
          data: null
        });
      nock(baseURL)
        .post('/upload/v2/file/upload_complete', { preuploadID: 'pre-never' })
        .twice()
        .reply(200, {
          code: 0,
          message: 'ok',
          data: { fileID: 0, completed: false }
        });

      const client = createPan123Client({ clientId: 'client', clientSecret: 'secret' });
      await expect(
        client.upload.uploadFile({
          filePath,
          parentFileID: 0,
          singleUploadMaxBytes: 1,
          completePollingAttempts: 2,
          completePollingDelayMs: 0
        })
      ).rejects.toMatchObject({
        name: 'Pan123ApiError',
        message: 'Upload completion did not return completed=true with a valid fileID after 2 polling attempts.'
      });
    } finally {
      await rm(dir, { recursive: true, force: true });
    }
  });
});
