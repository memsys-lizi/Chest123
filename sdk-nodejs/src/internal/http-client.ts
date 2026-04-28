import axios, { AxiosError, type AxiosInstance, type AxiosRequestConfig } from 'axios';
import FormData from 'form-data';
import { Pan123ApiError } from '../errors.js';
import { appendFormFields } from './form.js';
import type {
  AccessTokenData,
  HttpMethod,
  OAuthTokenData,
  OAuthTokenParams,
  Pan123ClientOptions,
  Pan123RequestOptions,
  Pan123Response
} from '../types.js';

const DEFAULT_BASE_URL = 'https://open-api.123pan.com';
const DEFAULT_PLATFORM = 'open_platform';
const DEFAULT_TIMEOUT_MS = 30000;

function toDateMs(value?: string | Date | number): number | undefined {
  if (value === undefined || value === null) return undefined;
  if (typeof value === 'number') return value;
  const time = value instanceof Date ? value.getTime() : new Date(value).getTime();
  return Number.isFinite(time) ? time : undefined;
}

function isPan123Response(value: unknown): value is Pan123Response {
  return Boolean(value && typeof value === 'object' && 'code' in value && 'message' in value);
}

export class Pan123HttpClient {
  readonly baseURL: string;
  readonly platform: string;
  readonly timeoutMs: number;
  readonly clientId?: string;
  readonly clientSecret?: string;

  private readonly http: AxiosInstance;
  private accessToken?: string;
  private tokenExpiresAtMs?: number;

  constructor(options: Pan123ClientOptions = {}) {
    this.baseURL = options.baseURL ?? DEFAULT_BASE_URL;
    this.platform = options.platform ?? DEFAULT_PLATFORM;
    this.timeoutMs = options.timeoutMs ?? DEFAULT_TIMEOUT_MS;
    this.clientId = options.clientId;
    this.clientSecret = options.clientSecret;
    this.accessToken = options.accessToken;
    this.tokenExpiresAtMs = toDateMs(options.tokenExpiresAt);
    this.http = axios.create({
      baseURL: this.baseURL,
      timeout: this.timeoutMs,
      maxBodyLength: Infinity,
      maxContentLength: Infinity
    });
  }

  setAccessToken(token: string, expiresAt?: string | Date | number): void {
    this.accessToken = token;
    this.tokenExpiresAtMs = toDateMs(expiresAt);
  }

  async ensureAccessToken(): Promise<string> {
    const refreshAt = (this.tokenExpiresAtMs ?? 0) - 60_000;
    if (this.accessToken && (!this.tokenExpiresAtMs || Date.now() < refreshAt)) {
      return this.accessToken;
    }
    const data = await this.getAccessToken();
    return data.accessToken;
  }

  async getAccessToken(): Promise<AccessTokenData> {
    if (!this.clientId || !this.clientSecret) {
      throw new Pan123ApiError({
        message: 'clientId and clientSecret are required to fetch access_token'
      });
    }
    const data = await this.request<AccessTokenData>('POST', '/api/v1/access_token', {
      auth: false,
      body: {
        clientID: this.clientId,
        clientSecret: this.clientSecret
      }
    });
    this.accessToken = data.accessToken;
    this.tokenExpiresAtMs = toDateMs(data.expiredAt);
    return data;
  }

  async getOAuthToken(params: OAuthTokenParams): Promise<OAuthTokenData> {
    return this.request<OAuthTokenData>('POST', '/api/v1/oauth2/access_token', {
      auth: false,
      query: params
    });
  }

  async request<T = unknown>(
    method: HttpMethod,
    requestPath: string,
    options: Pan123RequestOptions = {}
  ): Promise<T> {
    const headers: Record<string, string> = {
      Platform: this.platform,
      ...options.headers
    };

    if (options.auth !== false) {
      headers.Authorization = `Bearer ${await this.ensureAccessToken()}`;
    }

    let data = options.body;
    if (options.form) {
      const form = options.form instanceof FormData ? options.form : new FormData();
      if (!(options.form instanceof FormData)) appendFormFields(form, options.form);
      data = form;
      Object.assign(headers, form.getHeaders());
    } else if (data !== undefined && !headers['Content-Type']) {
      headers['Content-Type'] = 'application/json';
    }

    const config: AxiosRequestConfig = {
      method,
      url: requestPath,
      baseURL: options.baseURL ?? this.baseURL,
      params: options.query,
      data,
      headers,
      timeout: this.timeoutMs,
      responseType: options.responseType,
      maxBodyLength: Infinity,
      maxContentLength: Infinity
    };

    try {
      const response = await this.http.request(config);
      const body = response.data;
      if (isPan123Response(body)) {
        if (body.code !== 0) {
          throw new Pan123ApiError({
            code: body.code,
            message: body.message,
            traceId: body['x-traceID'],
            status: response.status,
            response: body
          });
        }
        return body.data as T;
      }
      return body as T;
    } catch (error) {
      if (error instanceof Pan123ApiError) throw error;
      if (error instanceof AxiosError) {
        const responseBody = error.response?.data;
        const traceId =
          responseBody && typeof responseBody === 'object' && 'x-traceID' in responseBody
            ? String(responseBody['x-traceID'])
            : undefined;
        throw new Pan123ApiError({
          message: error.message,
          status: error.response?.status,
          traceId,
          response: responseBody
        });
      }
      throw error;
    }
  }
}
