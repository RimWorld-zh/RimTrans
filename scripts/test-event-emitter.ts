import { EventEmitter } from 'events';

const emitter = new EventEmitter();

emitter.on('foobar', (...args) => {
  console.log('on foobar event', ...args);
});

async function test(): Promise<void> {
  console.log('before foobar event');
  emitter.emit('foobar', 'emit foobar event');
  console.log('after foobar event');
}

test();
