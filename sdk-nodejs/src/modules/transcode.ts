import { ApiModule, type AnyData } from './types.js';
import type { FileListData, FileListParams } from '../types.js';

export class TranscodeModule extends ApiModule {
  listCloudDiskVideos(params: FileListParams): Promise<FileListData> {
    return this.http.request('GET', '/api/v2/file/list', { query: params });
  }

  uploadFromCloudDisk(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/api/v1/transcode/upload/from_cloud_disk', { body: params });
  }

  listFiles(params: FileListParams): Promise<FileListData> {
    return this.http.request('GET', '/api/v2/file/list', { query: params });
  }

  folderInfo(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/api/v1/transcode/folder/info', { body: params });
  }

  videoResolutions(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/api/v1/transcode/video/resolutions', { body: params });
  }

  list(params: AnyData): Promise<unknown> {
    return this.http.request('GET', '/api/v1/video/transcode/list', { query: params });
  }

  transcode(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/api/v1/transcode/video', { body: params });
  }

  record(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/api/v1/transcode/video/record', { body: params });
  }

  result(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/api/v1/transcode/video/result', { body: params });
  }

  delete(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/api/v1/transcode/delete', { body: params });
  }

  downloadOriginal(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/api/v1/transcode/file/download', { body: params });
  }

  downloadM3u8OrTs(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/api/v1/transcode/m3u8_ts/download', { body: params });
  }

  downloadAll(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/api/v1/transcode/file/download/all', { body: params });
  }
}
