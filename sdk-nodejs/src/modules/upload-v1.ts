import { ApiModule, type AnyData } from './types.js';

export class UploadV1Module extends ApiModule {
  create(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/upload/v1/file/create', { body: params });
  }

  getUploadUrl(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/upload/v1/file/get_upload_url', { body: params });
  }

  listUploadParts(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/upload/v1/file/list_upload_parts', { body: params });
  }

  complete(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/upload/v1/file/upload_complete', { body: params });
  }

  asyncResult(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/upload/v1/file/upload_async_result', { body: params });
  }
}
