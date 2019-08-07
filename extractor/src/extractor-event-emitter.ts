/* eslint-disable @typescript-eslint/no-explicit-any */
import { EventEmitter } from 'events';

export async function sleep(ms: number): Promise<void> {
  return new Promise<void>((resolve, reject) => {
    setTimeout(() => resolve(), ms);
  });
}

// ================================================================
// Workflow

export type WorkflowStatus = 'queue' | 'pending' | 'resolved' | 'rejected';

export interface WorkflowNode {
  status?: WorkflowStatus;
  cost?: number;
  children: Record<string, WorkflowNode>;
}

export interface WorkflowMap {
  [key: string]: WorkflowNode;
}

export type WorkflowEntry = [string, WorkflowNode];

export function createWorkflowNode(): WorkflowNode;

export function createWorkflowNode(status: WorkflowStatus): WorkflowNode;

export function createWorkflowNode(children: Record<string, WorkflowNode>): WorkflowNode;

export function createWorkflowNode(entries: WorkflowEntry[]): WorkflowNode;

export function createWorkflowNode(
  status: WorkflowStatus,
  children: Record<string, WorkflowNode>,
): WorkflowNode;

export function createWorkflowNode(
  status: WorkflowStatus,
  entries: WorkflowEntry[],
): WorkflowNode;

export function createWorkflowNode(
  arg1?: WorkflowStatus | Record<string, WorkflowNode> | WorkflowEntry[],
  arg2?: Record<string, WorkflowNode> | WorkflowEntry[],
): WorkflowNode {
  const status = (typeof arg1 === 'string' && arg1) || undefined;
  const entries =
    (Array.isArray(arg1) && arg1) || (Array.isArray(arg2) && arg2) || undefined;
  const children: WorkflowMap =
    (entries && Object.fromEntries(entries)) ||
    (typeof arg1 === 'object' && (arg1 as WorkflowMap)) ||
    (typeof arg2 === 'object' && (arg2 as WorkflowMap)) ||
    {};

  if (status) {
    return { status, cost: 0, children };
  }
  return { children };
}

function setWorkflowMapRecursively(
  map: WorkflowMap,
  paths: string[],
  status: WorkflowStatus,
  cost: number,
  strict: boolean,
): void {
  const prop = paths.shift() as string;
  let node = map[prop];
  if (!node && !strict) {
    node = { children: {} };
    map[prop] = node;
  }
  if (paths.length === 0) {
    node.status = status;
    node.cost = cost;
    return;
  }
  setWorkflowMapRecursively(node.children, paths, status, cost, strict);
}

export function setWorkflowMap(
  map: WorkflowMap,
  [status, key, cost]: [WorkflowStatus, string | string[], number],
  strict: boolean = true,
): void {
  const paths: string[] = Array.isArray(key) ? [...key] : [key];
  setWorkflowMapRecursively(map, paths, status, cost, strict);
}

export function createWorkflowMap(keyList: (string | string[])[]): WorkflowMap {
  const map: WorkflowMap = {};
  keyList.forEach(key => setWorkflowMap(map, ['queue', key, 0], false));
  return map;
}

// ================================================================
// Event Emitter

export interface ExtractorEventMap {
  /**
   * Workflow stage status: `[status, key, const]`
   * status: status for the stage, 'pending', 'resolved' or 'rejected'
   * key: the key for the stage
   * cost: cost time in millisecond
   */
  workflow: [WorkflowStatus, string | string[], number];
  workflowMap: WorkflowMap;
  info: string;
  warn: string;
  error: string;
}

export type ExtractorEvent = keyof ExtractorEventMap;

export type ExtractorEventListener<T> = (event: ExtractorEvent, data: T) => any;

export type ExtractorEventListenerMap = {
  [e in ExtractorEvent]: ExtractorEventListener<ExtractorEventMap[e]>;
};

export class ExtractorEventEmitter {
  public readonly raw = new EventEmitter();

  /**
   * Add a listener to the specified event.
   */
  public addListener<E extends ExtractorEvent>(
    event: E,
    listener: ExtractorEventListener<ExtractorEventMap[E]>,
  ): this {
    this.raw.addListener(`extractor-${event}`, listener);
    return this;
  }

  /**
   * Remove the listener in the specified event.
   */
  public removeListener<E extends ExtractorEvent>(
    event: E,
    listener: ExtractorEventListener<ExtractorEventMap[E]>,
  ): this {
    this.raw.removeListener(`extractor-${event}`, listener);
    return this;
  }

  /**
   * Emit the data to the specified event, this will call `listener(event, data)`.
   */
  public emit<E extends ExtractorEvent>(event: E, data: ExtractorEventMap[E]): boolean {
    return this.raw.emit(`extractor-${event}`, event, data);
  }

  /**
   * Create a promise based workflow for event emitter.
   * @param key the workflow stage key
   * @param work the work function to run
   */
  public async workflow<T>(key: string | string[], work: () => Promise<T>): Promise<T> {
    this.emit('workflow', ['pending', key, 0]);
    const start = Date.now();

    return new Promise<T>((resolve, reject) =>
      work()
        .then(v => {
          this.emit('workflow', ['resolved', key, Date.now() - start]);
          resolve(v);
        })
        .catch(e => {
          this.emit('workflow', ['rejected', key, Date.now() - start]);
          reject(e);
        }),
    );
  }
}
