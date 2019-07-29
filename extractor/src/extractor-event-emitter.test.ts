import {
  sleep,
  ExtractorEventEmitter,
  ExtractorEventListener,
} from './extractor-event-emitter';

describe('extractor-event-emitter', () => {
  const emitter = new ExtractorEventEmitter();

  test('sleep', async () => {
    const ms = 200;
    const start = Date.now();
    await sleep(ms);
    const end = Date.now();
    const inaccuracy = end - start - ms;
    expect(inaccuracy >= -2 && inaccuracy <= 2).toBe(true);
  });

  test('event emitter', () => {
    let flag = false;
    const listener: ExtractorEventListener<string> = (event, data): void => {
      expect(data).toBe('foobar event emitted.');
    };

    emitter.addListener('foobar', listener);

    flag = emitter.emit('foobar', 'foobar event emitted.');
    expect(flag).toBe(true);

    emitter.removeListener('foobar', listener);
    flag = emitter.emit('foobar', 'foobar listener has been removed.');
    expect(flag).toBe(false);
  });
});
