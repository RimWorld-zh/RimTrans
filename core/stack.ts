/**
 * Stack
 */
// tslint:disable:no-any
export default class Stack<T = any> {
  private items: T[] = [];

  private top: number = 0;

  public get data(): T[] {
    return this.items.slice(0, this.top);
  }

  public push(item: T): this {
    this.items[this.top] = item;
    this.top++;

    return this;
  }

  public pop(): this {
    this.top--;

    return this;
  }

  public get peek(): T {
    return this.items[this.top - 1];
  }

  public get length(): number {
    return this.top;
  }

  public clear(): this {
    this.top = 0;
    this.items = [];

    return this;
  }
}
