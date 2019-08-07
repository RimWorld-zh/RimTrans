import { InitService } from '../models';
import { initHandler } from './handler';

export const initServiceModMetaData: InitService = states => {
  initHandler(states);
};
