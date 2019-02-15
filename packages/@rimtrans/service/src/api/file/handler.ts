import express from 'express';
import io from '@rimtrans/io';
import { FileParams } from './model';

/**
 * Route handler for file
 */
const file = express.Router();

file.get('/', async (request, response) => {
  const { path } = request.query as FileParams;
  if (!path) {
    return response.sendStatus(400);
  }
  if (!io.exists(path)) {
    return response.sendStatus(404);
  }

  return response.sendFile(path);
});

export { file };
