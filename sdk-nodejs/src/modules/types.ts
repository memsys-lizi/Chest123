import type { Pan123HttpClient } from '../internal/http-client.js';

export type AnyData = Record<string, unknown>;
export type ApiMethod<TParams = AnyData, TResult = unknown> = (params: TParams) => Promise<TResult>;

export abstract class ApiModule {
  protected readonly http: Pan123HttpClient;

  constructor(http: Pan123HttpClient) {
    this.http = http;
  }
}
