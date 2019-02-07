import { Response } from 'express';

export type ContentType = 'application/json' | 'text/plain' | 'text/html' | 'text/xml';

/**
 * Set header ContentType to text/xml
 */
export function setContentType(response: Response, contentType: ContentType): Response {
  response.setHeader('Content-Type', contentType);

  return response;
}
