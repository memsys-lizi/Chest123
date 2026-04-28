import { ApiModule, type AnyData } from './types.js';
import type { DownloadInfoData, FileListData, FileListParams } from '../types.js';

export class FilesModule extends ApiModule {
  mkdir(params: { name: string; parentID: number }): Promise<{ dirID: number }> {
    return this.http.request('POST', '/upload/v1/file/mkdir', { body: params });
  }

  rename(params: AnyData): Promise<unknown> {
    return this.http.request('PUT', '/api/v1/file/name', { body: params });
  }

  batchRename(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/api/v1/file/rename', { body: params });
  }

  trash(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/api/v1/file/trash', { body: params });
  }

  copy(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/api/v1/file/copy', { body: params });
  }

  asyncCopy(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/api/v1/file/async/copy', { body: params });
  }

  asyncCopyProcess(params: AnyData): Promise<unknown> {
    return this.http.request('GET', '/api/v1/file/async/copy/process', { query: params });
  }

  recover(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/api/v1/file/recover', { body: params });
  }

  recoverByPath(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/api/v1/file/recover/by_path', { body: params });
  }

  detail(params: { fileID: number }): Promise<unknown> {
    return this.http.request('GET', '/api/v1/file/detail', { query: params });
  }

  infos(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/api/v1/file/infos', { body: params });
  }

  list(params: FileListParams): Promise<FileListData> {
    return this.http.request('GET', '/api/v2/file/list', { query: params });
  }

  listLegacy(params: AnyData): Promise<unknown> {
    return this.http.request('GET', '/api/v1/file/list', { query: params });
  }

  move(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/api/v1/file/move', { body: params });
  }

  downloadInfo(params: { fileId: number }): Promise<DownloadInfoData> {
    return this.http.request('GET', '/api/v1/file/download_info', { query: params });
  }
}
