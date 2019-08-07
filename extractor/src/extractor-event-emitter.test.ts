import {
  sleep,
  WorkflowStatus,
  WorkflowMap,
  createWorkflowNode,
  setWorkflowMap,
  ExtractorEventEmitter,
  ExtractorEventListener,
} from './extractor-event-emitter';

describe('extractor-event-emitter', () => {
  const map = {
    foo: createWorkflowNode('queue', { bar: createWorkflowNode() }),
    bar: createWorkflowNode('queue', { foo: createWorkflowNode() }),
  };
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
    const listener: ExtractorEventListener<string> = (e, msg): void => {
      expect(msg).toBe('foobar event emitted.');
    };

    emitter.addListener('info', listener);

    flag = emitter.emit('info', 'foobar event emitted.');
    expect(flag).toBe(true);

    emitter.removeListener('info', listener);
    flag = emitter.emit('info', 'foobar listener has been removed.');
    expect(flag).toBe(false);
  });

  test('createWorkflowNode', () => {
    expect(createWorkflowNode()).toEqual({ children: {} });
    expect(createWorkflowNode('queue')).toEqual({
      status: 'queue',
      cost: 0,
      children: {},
    });
    expect(createWorkflowNode({ foobar: createWorkflowNode() })).toEqual({
      children: {
        foobar: { children: {} },
      },
    });
    expect(createWorkflowNode([['foobar', createWorkflowNode()]])).toEqual({
      children: {
        foobar: { children: {} },
      },
    });
    expect(createWorkflowNode('queue', { foobar: createWorkflowNode() })).toEqual({
      status: 'queue',
      cost: 0,
      children: {
        foobar: { children: {} },
      },
    });
    expect(createWorkflowNode('queue', [['foobar', createWorkflowNode()]])).toEqual({
      status: 'queue',
      cost: 0,
      children: {
        foobar: { children: {} },
      },
    });
  });

  test('setWorkflowMap', () => {
    const wf: WorkflowMap = {};

    expect(() => setWorkflowMap(wf, ['pending', ['bar', 'foo'], 0])).toThrowError(
      "Cannot read property 'children' of undefined",
    );
    expect(wf).toEqual({});

    setWorkflowMap(wf, ['pending', ['foo', 'bar'], 0], false);
    expect(wf).toEqual({
      foo: {
        children: {
          bar: {
            status: 'pending',
            cost: 0,
            children: {},
          },
        },
      },
    });
  });

  test('workflow', async () => {
    emitter.addListener('workflow', (e, payload) => setWorkflowMap(map, payload));

    expect(map).toEqual({
      foo: {
        status: 'queue',
        cost: 0,
        children: {
          bar: {
            children: {},
          },
        },
      },
      bar: {
        status: 'queue',
        cost: 0,
        children: {
          foo: {
            children: {},
          },
        },
      },
    });

    await emitter.workflow('foo', async () => {});
    await emitter.workflow(['foo', 'bar'], async () => {});
    await emitter.workflow(['bar', 'foo'], async () => {});
    await expect(
      emitter.workflow('bar', async () => {
        throw new Error('bar error');
      }),
    ).rejects.toThrowError(/bar error/);

    expect(map).toEqual({
      foo: {
        status: 'resolved',
        cost: map.foo.cost,
        children: {
          bar: {
            status: 'resolved',
            cost: map.foo.children.bar.cost,
            children: {},
          },
        },
      },
      bar: {
        status: 'rejected',
        cost: map.bar.cost,
        children: {
          foo: {
            status: 'resolved',
            cost: map.bar.children.foo.cost,
            children: {},
          },
        },
      },
    });
  });
});
