import type FormData from 'form-data';

export type HttpMethod = 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH';

export type Pan123Primitive = string | number | boolean | null | undefined;

export type Pan123Params = Record<string, unknown>;

export interface Pan123Response<T = unknown> {
  code: number;
  message: string;
  data: T;
  'x-traceID'?: string;
}

export interface Pan123ClientOptions {
  clientId?: string;
  clientSecret?: string;
  accessToken?: string;
  tokenExpiresAt?: string | Date | number;
  baseURL?: string;
  platform?: string;
  timeoutMs?: number;
}

export interface Pan123RequestOptions {
  query?: Pan123Params;
  body?: unknown;
  form?: FormData | Pan123FormFields;
  headers?: Record<string, string>;
  baseURL?: string;
  auth?: boolean;
  responseType?: 'arraybuffer' | 'blob' | 'document' | 'json' | 'stream' | 'text';
}

export type Pan123FormValue =
  | Pan123Primitive
  | Buffer
  | NodeJS.ReadableStream
  | {
      value: Buffer | NodeJS.ReadableStream | string;
      options?: FormData.AppendOptions | string;
    };

export type Pan123FormFields = Record<string, Pan123FormValue>;

export interface AccessTokenRequest {
  clientID: string;
  clientSecret: string;
}

export interface AccessTokenData {
  accessToken: string;
  expiredAt: string;
}

export interface OAuthTokenParams {
  [key: string]: unknown;
  client_id: string;
  client_secret: string;
  grant_type: 'authorization_code' | 'refresh_token';
  code?: string;
  refresh_token?: string;
  redirect_uri?: string;
}

export interface OAuthTokenData {
  token_type: string;
  access_token: string;
  refresh_token: string;
  expires_in: number;
  scope: string;
}

export interface FileListParams extends Pan123Params {
  parentFileId: number;
  limit: number;
  searchData?: string;
  searchMode?: number;
  lastFileId?: number;
}

export interface FileInfo {
  fileID?: number;
  fileId?: number;
  filename: string;
  type: number;
  size: number;
  etag?: string;
  status?: number;
  parentFileID?: number;
  parentFileId?: number;
  createAt?: string;
  updateAt?: string;
  trashed?: number;
  [key: string]: unknown;
}

export interface FileListData {
  lastFileId: number;
  fileList: FileInfo[];
}

export interface UploadCreateParams extends Pan123Params {
  parentFileID: number;
  filename: string;
  etag: string;
  size: number;
  duplicate?: number;
  containDir?: boolean;
}

export interface UploadCreateData {
  fileID?: number;
  reuse: boolean;
  preuploadID?: string;
  sliceSize?: number;
  servers?: string[];
}

export interface UploadCompleteData {
  completed: boolean;
  fileID: number;
}

export type UploadProgressStage = 'hashing' | 'single' | 'create' | 'reuse' | 'slice' | 'complete';

export interface UploadProgressEvent {
  stage: UploadProgressStage;
  loadedBytes: number;
  totalBytes: number;
  percent: number;
  sliceNo?: number;
  totalSlices?: number;
  completedSlices?: number;
  attempt?: number;
}

export interface UploadFileOptions {
  filePath: string;
  parentFileID: number;
  filename?: string;
  duplicate?: number;
  containDir?: boolean;
  /**
   * Defaults to 1GB, matching the official single-upload limit.
   * Lower values are useful for tests.
   */
  singleUploadMaxBytes?: number;
  /**
   * Maximum upload_complete polling attempts for multipart uploads.
   * Defaults to 60.
   */
  completePollingAttempts?: number;
  /**
   * Delay between upload_complete polling attempts in milliseconds.
   * Defaults to 1000.
   */
  completePollingDelayMs?: number;
  /**
   * Retries transient upload queue/rate-limit responses such as
   * "任务队列削峰中,请慢一点". Defaults to 5.
   */
  transientRetryAttempts?: number;
  /**
   * Delay between transient upload retries in milliseconds.
   * Defaults to 1000.
   */
  transientRetryDelayMs?: number;
  onProgress?: (event: UploadProgressEvent) => void | Promise<void>;
}

export interface UploadFileResult {
  fileID: number;
  completed: boolean;
  reuse?: boolean;
}

export interface DownloadInfoData {
  downloadUrl: string;
}

export interface DirectLinkUrlData {
  url: string;
}
