import FormData from 'form-data';
import type { Pan123FormFields } from '../types.js';

export function appendFormFields(form: FormData, fields: Pan123FormFields): void {
  for (const [key, rawValue] of Object.entries(fields)) {
    if (rawValue === undefined || rawValue === null) continue;
    if (typeof rawValue === 'object' && 'value' in rawValue && !Buffer.isBuffer(rawValue)) {
      form.append(key, rawValue.value, rawValue.options as FormData.AppendOptions);
      continue;
    }
    form.append(key, rawValue as string | number | boolean | Buffer | NodeJS.ReadableStream);
  }
}
