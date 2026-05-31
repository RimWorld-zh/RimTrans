import Readline from 'readline';

export interface Printer {
  /**
   * Write message to console.
   */
  writeLine(message: string): this;

  /**
   * Clear all contents and write message to console.
   */
  overWrite(message: string): this;

  reset(): this;
}

export function createPrinter(): Printer {
  const regexEOL = /\n|\r|\r\n/;
  const stream = process.stderr;
  /**
   * Console rows written before.
   */
  let rows = 0;

  return {
    writeLine(message) {
      rows = message.split(regexEOL).length;
      stream.write(message);
      stream.write('\n');
      return this;
    },

    overWrite(message) {
      if (rows) {
        Readline.moveCursor(stream, 0, -rows);
        Readline.cursorTo(stream, 0);
        Readline.clearScreenDown(stream);
      }
      this.writeLine(message);
      return this;
    },

    reset() {
      rows = 0;
      return this;
    },
  };
}
