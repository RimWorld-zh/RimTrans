/**
 * Service main
 */

import express from 'express';

const app = express();

app.use('*', (request, response) =>
  response.send(`Hello world! Service is running in ${__dirname}`),
);

app.listen(9102, () => {
  console.info('Service is listening at localhost:9102');
});
