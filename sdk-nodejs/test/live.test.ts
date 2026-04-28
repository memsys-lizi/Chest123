import { describe, expect, it } from 'vitest';
import { existsSync } from 'node:fs';
import path from 'node:path';
import { createPan123Client } from '../src/index.js';

const clientId = process.env.PAN123_CLIENT_ID;
const clientSecret = process.env.PAN123_CLIENT_SECRET;
const parentFileID = Number(process.env.PAN123_PARENT_FILE_ID ?? 0);
const testFile = path.resolve(process.cwd(), '..', 'Test.txt');
const runLive = Boolean(clientId && clientSecret && existsSync(testFile));

const describeLive = runLive ? describe : describe.skip;

describeLive('123Pan live API', () => {
  it('authenticates, lists root, uploads Test.txt, and gets download info', async () => {
    const client = createPan123Client({
      clientId,
      clientSecret,
      timeoutMs: 60000
    });

    const token = await client.auth.getAccessToken();
    expect(token.accessToken).toBeTruthy();
    expect(token.expiredAt).toBeTruthy();

    const user = await client.user.info();
    expect(user).toBeTruthy();

    const list = await client.files.list({ parentFileId: 0, limit: 100 });
    expect(Array.isArray(list.fileList)).toBe(true);

    const uploaded = await client.upload.uploadFile({
      filePath: testFile,
      parentFileID,
      filename: `Test-sdk-live-${Date.now()}.txt`,
      duplicate: 1
    });
    expect(uploaded.fileID).toBeGreaterThan(0);
    expect(uploaded.completed).toBe(true);

    const detail = await client.files.detail({ fileID: uploaded.fileID });
    expect(detail).toBeTruthy();

    const download = await client.files.downloadInfo({ fileId: uploaded.fileID });
    expect(download.downloadUrl).toMatch(/^https?:\/\//);
  });
});
