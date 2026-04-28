import { Pan123ApiError } from '../errors.js';

const FILE_CHECKING_CODE = 20103;
const DEFAULT_CHECKING_RETRY_ATTEMPTS = 60;
const DEFAULT_CHECKING_RETRY_DELAY_MS = 1000;

export interface FileCheckingRetryOptions {
  checkingRetryAttempts?: number;
  checkingRetryDelayMs?: number;
}

function delay(ms: number): Promise<void> {
  return new Promise(resolve => setTimeout(resolve, ms));
}

function isFileCheckingError(error: unknown): boolean {
  return error instanceof Pan123ApiError && error.code === FILE_CHECKING_CODE;
}

export async function retryWhileFileChecking<T>(
  task: () => Promise<T>,
  options: FileCheckingRetryOptions = {}
): Promise<T> {
  const attempts = Math.max(1, Math.floor(options.checkingRetryAttempts ?? DEFAULT_CHECKING_RETRY_ATTEMPTS));
  const delayMs = Math.max(0, Math.floor(options.checkingRetryDelayMs ?? DEFAULT_CHECKING_RETRY_DELAY_MS));

  for (let attempt = 1; attempt <= attempts; attempt++) {
    try {
      return await task();
    } catch (error) {
      if (!isFileCheckingError(error) || attempt === attempts) {
        throw error;
      }

      await delay(delayMs);
    }
  }

  throw new Pan123ApiError({
    code: FILE_CHECKING_CODE,
    message: 'File checking did not finish before retry attempts were exhausted.'
  });
}
