import pth from 'path';
import * as logger from './logger';

interface Env {
  readonly filename: string;
  readonly dirname: string;
  readonly cwd: string;
  readonly execPath: string;
  readonly argv0: string;
  readonly argv1: string;
  readonly mainFilename?: string;
  readonly isPkg: boolean;
  readonly entrypoint?: string;
  readonly defaultEntrypoint?: string;
}

function getEnv(): Env {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const { pkg } = process as any;

  return {
    filename: __filename,
    dirname: __dirname,
    cwd: process.cwd(),
    execPath: process.execPath,
    argv0: process.argv[0],
    argv1: process.argv[1],
    mainFilename: require.main && require.main.filename,
    isPkg: !!pkg,
    entrypoint: pkg && pkg.entrypoint,
    defaultEntrypoint: pkg && pkg.defaultEntrypoint,
  };
}

const ENV = getEnv();
const INSTALLATION_DIRECTORY = ENV.isPkg ? pth.basename(ENV.execPath) : ENV.dirname;

async function main(): Promise<void> {
  logger.info(ENV);
  logger.info(INSTALLATION_DIRECTORY);
}

main().catch(logger.error);
