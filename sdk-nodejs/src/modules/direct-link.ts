import { ApiModule, type AnyData } from './types.js';
import type { DirectLinkUrlData } from '../types.js';

export class DirectLinkModule extends ApiModule {
  enable(params: { fileID: number }): Promise<unknown> {
    return this.http.request('POST', '/api/v1/direct-link/enable', { body: params });
  }

  disable(params: { fileID: number }): Promise<unknown> {
    return this.http.request('POST', '/api/v1/direct-link/disable', { body: params });
  }

  url(params: { fileID: number }): Promise<DirectLinkUrlData> {
    return this.http.request('GET', '/api/v1/direct-link/url', { query: params });
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
