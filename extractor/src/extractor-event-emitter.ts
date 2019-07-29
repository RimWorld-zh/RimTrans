/* eslint-disable @typescript-eslint/no-explicit-any */
import { EventEmitter } from 'events';

export async function sleep(ms: number): Promise<void> {
  return new Promise<void>((resolve, reject) => {
    setTimeout(() => resolve(), ms);
  });
}

export type ProgressStatus = 'pending' | 'succeed' | 'failed';

export interface Progress {
  /**
   * The key for current progress.
   */
  action: string;
  /**
   * The key for child progress of current progress.
   */
  key: string | number;
  /**
   * The status for current progress or child progress.
   */
  status: ProgressStatus;
  /**
   * Additional message.
   */
  info?: string;
}

export interface ExtractorEventMap {
  foobar: string;
  info: string;
  warn: string;
  error: string;
  progress: Progress;
  done: string;
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
}
