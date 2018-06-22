/**
 * Init nodejs env by dotenv.
 */

import dotenv from 'dotenv';
import fs from 'fs';

export interface Config {
  dirCore: string;
  dirWorkshopMods: string;
}

interface Env {
  [key: string]: string;
}

export default function envInit(): Config {
  const env: Env = dotenv.parse(fs.readFileSync('.env'));
  const local: Env = dotenv.parse(
    fs.existsSync('.env.local') ? fs.readFileSync('.env.local') : '',
  );

  return {
    dirCore: local.DIR_CORE || env.DIR_CORE,
    dirWorkshopMods: local.DIR_WORKSHOP_MODS || env.DIR_WORKSHOP_MODS,
  };
}

// const config: Config = envInit();
// console.log(config);
