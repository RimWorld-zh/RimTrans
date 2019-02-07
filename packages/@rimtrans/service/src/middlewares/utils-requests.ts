import axios from 'axios';

const BASE_URL = '/api';

/**
 * Axios instance for api requests
 */
export const api = axios.create({
  baseURL: BASE_URL,
});

export function resolveApi(url: string): string {
  return `${BASE_URL}/${url.replace(/^\/+/, '')}`;
}
