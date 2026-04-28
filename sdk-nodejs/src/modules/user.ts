import { ApiModule } from './types.js';

export class UserModule extends ApiModule {
  info(): Promise<unknown> {
    return this.http.request('GET', '/api/v1/user/info');
  }
}
