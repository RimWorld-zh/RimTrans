/**
 * Injection
 */

/**
 * The default end of line character. Use \n for LF and \r\n for CRLF.
 * 默认行尾字符。使用 \n 表示 LF，\r\n 表示 CRLF。
 */
export enum EOL {
  LF = '\n',
  CRLF = '\r\n',
}

export interface Injection {
  fieldPath: string[];
}

export interface InjectionData {
  [defType: string]: {
    [fileName: string]: Injection;
  };
}
