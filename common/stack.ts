/**
 * Stack
 */
// tslint:disable:no-any
export default class Stack<T = any> {
  private itemsSource: T[] = [];

  private top: number = 0;

  public get items(): T[] {
    return this.itemsSource.slice(0, this.top);
  }

  public push(item: T): this {
    this.itemsSource[this.top] = item;
    this.top++;

    return this;
  }

  public pop(): this {
    this.top--;

    return this;
  }

  public get peek(): T {
    return this.itemsSource[this.top - 1];
  }

  public get length(): number {
    return this.top;
  }

  public clear(): this {
    this.top = 0;
    this.itemsSource = [];

    return this;
  }
}
