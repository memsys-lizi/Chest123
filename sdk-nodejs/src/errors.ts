export interface Pan123ApiErrorOptions {
  code?: number;
  message: string;
  traceId?: string;
  status?: number;
  response?: unknown;
}

export class Pan123ApiError extends Error {
  readonly code?: number;
  readonly traceId?: string;
  readonly status?: number;
  readonly response?: unknown;

  constructor(options: Pan123ApiErrorOptions) {
    super(options.message);
    this.name = 'Pan123ApiError';
    this.code = options.code;
    this.traceId = options.traceId;
    this.status = options.status;
    this.response = options.response;
  }
}
