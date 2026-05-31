import { pth, fse, globby } from '@rimtrans/extractor';
import {
  IPC_NAMESPACE_TRANSLATOR_PROJECT,
  EXT_NAME_TRANSLATOR_PROJECT,
} from '../../utils/constants';
import { createIpc } from '../../utils/ipc';
import { States } from '../../utils/states';
import { SlaverMain, createSlaverMain } from '../../utils/slaver';
import { initFSWatchHandler } from '../fs-watcher';
import { TranslatorProject, TranslatorProjectMetaData } from './models';

export function initProjectHandler(states: States): void {
  const { paths, ipc } = states;
  const { ipc: translatorProjectIpc, slaver } = initFSWatchHandler<
    TranslatorProject,
    TranslatorProjectMetaData
  >({
    ipcNamespace: IPC_NAMESPACE_TRANSLATOR_PROJECT,
    slaverPath: pth.join(__dirname, 'project-slaver'),
    slaverArgs: [paths.translatorProjects],
  });
}
