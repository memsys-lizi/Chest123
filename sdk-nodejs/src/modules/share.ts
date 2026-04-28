import { ApiModule, type AnyData } from './types.js';

export class ShareModule extends ApiModule {
  create(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/api/v1/share/create', { body: params });
  }

  list(params: AnyData): Promise<unknown> {
    return this.http.request('GET', '/api/v1/share/list', { query: params });
  }

  update(params: AnyData): Promise<unknown> {
    return this.http.request('PUT', '/api/v1/share/list/info', { body: params });
  }

  createPaid(params: AnyData): Promise<unknown> {
    return this.http.request('POST', '/api/v1/share/content-payment/create', { body: params });
  }

  listPaid(params: AnyData): Promise<unknown> {
    return this.http.request('GET', '/api/v1/share/payment/list', { query: params });
  }

  updatePaid(params: AnyData): Promise<unknown> {
    return this.http.request('PUT', '/api/v1/share/list/payment/info', { body: params });
  }
}
