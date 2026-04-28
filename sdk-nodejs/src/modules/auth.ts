import { ApiModule } from './types.js';
import type { AccessTokenData, OAuthTokenData, OAuthTokenParams } from '../types.js';

export class AuthModule extends ApiModule {
  getAccessToken(): Promise<AccessTokenData> {
    return this.http.getAccessToken();
  }

  getOAuthToken(params: OAuthTokenParams): Promise<OAuthTokenData> {
    return this.http.getOAuthToken(params);
  }

  setAccessToken(token: string, expiresAt?: string | Date | number): void {
    this.http.setAccessToken(token, expiresAt);
  }

  ensureAccessToken(): Promise<string> {
    return this.http.ensureAccessToken();
  }
}
