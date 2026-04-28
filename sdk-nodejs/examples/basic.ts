import { createPan123Client } from '../src/index.js';

const client = createPan123Client({
  clientId: process.env.PAN123_CLIENT_ID,
  clientSecret: process.env.PAN123_CLIENT_SECRET
});

const user = await client.user.info();
console.log('user:', user);

const root = await client.files.list({ parentFileId: 0, limit: 100 });
console.log('root files:', root.fileList.length);

