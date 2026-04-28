import { ApiModule, type AnyData } from './types.js';

export class OssModule extends ApiModule {
  mkdir(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/upload/v1/oss/file/mkdir', { body: params });
  }

  create(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/upload/v1/oss/file/create', { body: params });
  }

  getUploadUrl(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/upload/v1/oss/file/get_upload_url', { body: params });
  }

  complete(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/upload/v1/oss/file/upload_complete', { body: params });
  }

  asyncResult(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/upload/v1/oss/file/upload_async_result', { body: params });
  }

  createCopyTask(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/api/v1/oss/source/copy', { body: params });
  }

  getCopyProcess(params: AnyData): Promise<unknown> {
    return this.http.request('GET', '/api/v1/oss/source/copy/process', { query: params });
  }

  getCopyFailList(params: AnyData): Promise<unknown> {
    return this.http.request('GET', '/api/v1/oss/source/copy/fail', { query: params });
  }

  move(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/api/v1/oss/file/move', { body: params });
  }

  delete(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/api/v1/oss/file/delete', { body: params });
  }

  detail(params: AnyData): Promise<unknown> {
    return this.http.request('GET', '/api/v1/oss/file/detail', { query: params });
  }

  list(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/api/v1/oss/file/list', { body: params });
  }

  createOfflineMigration(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/api/v1/oss/offline/download', { body: params });
  }

  getOfflineMigration(params: AnyData): Promise<unknown> {
    return this.http.request('GET', '/api/v1/oss/offline/download/process', { query: params });
  }
}
