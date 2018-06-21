/**
 * Init nodejs env by dotenv.
 */

import dotenv from 'dotenv';
import fs from 'fs';

export interface Config {
  dirCoreDefs: string;
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
    dirCoreDefs: local.DIR_CORE_DEFS || env.DIR_CORE_DEFS,
    dirWorkshopMods: local.DIR_WORKSHOP_MODS || env.DIR_WORKSHOP_MODS,
  };
}

// const config: Config = envInit();
// console.log(config);
