/**
 * Simple Stack
 */
// tslint:disable-next-line:no-any
export default class Stack<T = any> {
  private items: T[] = [];

  public push(item: T): this {
    this.items.push(item);

    return this;
  }

  public pop(): this {
    this.items.pop();

    return this;
  }

  public get peek(): T {
    return this.items[this.items.length - 1];
  }

  public get isEmpty(): boolean {
    return this.items.length === 0;
  }

  public get size(): number {
    return this.items.length;
  }

  public clear(): this {
    this.items = [];

    return this;
  }
}
