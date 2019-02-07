/**
 * Utils for client side api requests
 */

import axios from 'axios';

/**
 * Axios instance api requests
 */
export const api = axios.create({
  baseURL: '/api',
  timeout: 10000,
});
