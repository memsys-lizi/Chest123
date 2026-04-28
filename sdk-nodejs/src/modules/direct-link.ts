import { retryWhileFileChecking, type FileCheckingRetryOptions } from './file-checking.js';
import { ApiModule, type AnyData } from './types.js';
import type { DirectLinkUrlData } from '../types.js';

interface DirectLinkUrlParams extends FileCheckingRetryOptions {
  fileID: number;
}

export class DirectLinkModule extends ApiModule {
  enable(params: { fileID: number }): Promise<unknown> {
    return this.http.request('POST', '/api/v1/direct-link/enable', { body: params });
  }

  disable(params: { fileID: number }): Promise<unknown> {
    return this.http.request('POST', '/api/v1/direct-link/disable', { body: params });
  }

  async url(params: DirectLinkUrlParams): Promise<DirectLinkUrlData> {
    return retryWhileFileChecking(
      () =>
        this.http.request('GET', '/api/v1/direct-link/url', {
          query: {
            fileID: params.fileID
          }
        }),
      params
    );
  }

  refreshCache(): Promise<unknown> {
    return this.http.request('POST', '/api/v1/direct-link/cache/refresh');
  }

  getTrafficLogs(params: AnyData): Promise<unknown> {
    return this.http.request('GET', '/api/v1/direct-link/log', { query: params });
  }

  getOfflineLogs(params: AnyData): Promise<unknown> {
    return this.http.request('GET', '/api/v1/direct-link/offline/logs', { query: params });
  }

  setIpBlacklistEnabled(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/api/v1/developer/config/forbide-ip/switch', { body: params });
  }

  updateIpBlacklist(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/api/v1/developer/config/forbide-ip/update', { body: params });
  }

  listIpBlacklist(params: AnyData): Promise<unknown> {
    return this.http.request('GET', '/api/v1/developer/config/forbide-ip/list', { query: params });
  }
}
