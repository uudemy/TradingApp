import type { ApiResponse } from '../types/api';

// Vite proxy kullanıyoruz, baseUrl boş
const BASE_URL = '';

export class ApiError extends Error {
  status: number;
  apiMessage: string;
  errors: string[];

  constructor(status: number, apiMessage: string, errors: string[]) {
    super(apiMessage);
    this.status = status;
    this.apiMessage = apiMessage;
    this.errors = errors;
  }
}

async function request<T>(
  path: string,
  options: RequestInit = {},
  token?: string | null
): Promise<T> {
  const headers = new Headers(options.headers || {});
  if (!headers.has('Content-Type') && options.body) {
    headers.set('Content-Type', 'application/json');
  }
  if (token) {
    headers.set('Authorization', `Bearer ${token}`);
  }

  const res = await fetch(`${BASE_URL}${path}`, { ...options, headers });

  if (res.status === 204) {
    return undefined as T;
  }

  const text = await res.text();
  let json: any = null;
  try {
    json = text ? JSON.parse(text) : null;
  } catch {
    // JSON değil (örneğin HTML hata sayfası)
    if (!res.ok) throw new ApiError(res.status, text || res.statusText, []);
    return text as unknown as T;
  }

  const api = json as ApiResponse<T> | null;

  if (!res.ok) {
    const message = api?.message || res.statusText;
    const errors = api?.errors || [];
    throw new ApiError(res.status, message, errors);
  }

  // Eğer zarf varsa data döner, yoksa json direkt döner
  if (api && typeof api === 'object' && 'success' in api && 'data' in api) {
    return api.data as T;
  }

  return json as T;
}

export const api = {
  get: <T>(path: string, token?: string | null) =>
    request<T>(path, { method: 'GET' }, token),

  post: <T>(path: string, body?: unknown, token?: string | null) =>
    request<T>(
      path,
      { method: 'POST', body: body !== undefined ? JSON.stringify(body) : undefined },
      token
    ),

  put: <T>(path: string, body?: unknown, token?: string | null) =>
    request<T>(
      path,
      { method: 'PUT', body: body !== undefined ? JSON.stringify(body) : undefined },
      token
    ),

  delete: <T>(path: string, token?: string | null) =>
    request<T>(path, { method: 'DELETE' }, token),
};