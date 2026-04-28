import { ApiModule, type AnyData } from './types.js';

export class OfflineModule extends ApiModule {
  createDownloadTask(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/api/v1/offline/download', { body: params });
  }

  getDownloadProcess(params: AnyData): Promise<unknown> {
    return this.http.request('GET', '/api/v1/offline/download/process', { query: params });
  }
}
