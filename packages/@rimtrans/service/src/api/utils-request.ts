import Axios, { AxiosRequestConfig } from 'axios';
import { BASE_URL_REST_API } from './model';

/**
 * Internal Axios instance for RESTful API
 */
const axios = Axios.create({
  baseURL: BASE_URL_REST_API,
});

/**
 * HTTP Request Methods
 */
export type RequestMethod =
  | 'get'
  | 'head'
  | 'post'
  | 'put'
  | 'delete'
  | 'connect'
  | 'options'
  | 'trace'
  | 'patch';

export interface RequestConfig<P, D> extends AxiosRequestConfig {
  url?: never;
  method?: never;
  params: P;
  data?: D;
}

export function requestFunction<R, P, D = never>(
  method: RequestMethod,
  url: string,
): (config: RequestConfig<P, D>) => Promise<R> {
  return async config => {
    const { data } = await axios.request<R>({
      ...config,
      url,
      method,
    });

    return data;
  };
}
