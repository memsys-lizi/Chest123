import { Pan123HttpClient } from './internal/http-client.js';
import { AuthModule } from './modules/auth.js';
import { DirectLinkModule } from './modules/direct-link.js';
import { FilesModule } from './modules/files.js';
import { OfflineModule } from './modules/offline.js';
import { OssModule } from './modules/oss.js';
import { ShareModule } from './modules/share.js';
import { TranscodeModule } from './modules/transcode.js';
import { UploadModule } from './modules/upload.js';
import { UploadV1Module } from './modules/upload-v1.js';
import { UserModule } from './modules/user.js';
import type { HttpMethod, Pan123ClientOptions, Pan123RequestOptions } from './types.js';

export class Pan123Client {
  private readonly http: Pan123HttpClient;

  readonly auth: AuthModule;
  readonly files: FilesModule;
  readonly upload: UploadModule;
  readonly uploadV1: UploadV1Module;
  readonly share: ShareModule;
  readonly offline: OfflineModule;
  readonly user: UserModule;
  readonly directLink: DirectLinkModule;
  readonly oss: OssModule;
  readonly transcode: TranscodeModule;

  constructor(options: Pan123ClientOptions = {}) {
    this.http = new Pan123HttpClient(options);
    this.auth = new AuthModule(this.http);
    this.files = new FilesModule(this.http);
    this.upload = new UploadModule(this.http);
    this.uploadV1 = new UploadV1Module(this.http);
    this.share = new ShareModule(this.http);
    this.offline = new OfflineModule(this.http);
    this.user = new UserModule(this.http);
    this.directLink = new DirectLinkModule(this.http);
    this.oss = new OssModule(this.http);
    this.transcode = new TranscodeModule(this.http);
  }

  request<T = unknown>(
    method: HttpMethod,
    requestPath: string,
    options: Pan123RequestOptions = {}
  ): Promise<T> {
    return this.http.request<T>(method, requestPath, options);
  }
}

export function createPan123Client(options: Pan123ClientOptions = {}): Pan123Client {
  return new Pan123Client(options);
}
