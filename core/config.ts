/**
 * The default end of line character. Use \n for LF and \r\n for CRLF.
 * 默认行尾字符。使用 \n 表示 LF，\r\n 表示 CRLF。
 */
export enum EOL {
  LF = '\n',
  CRLF = '\r\n',
}

export interface Config {
  eol: '\n' | '\r\n';
  indent: '  ' | '    ' | '\t';
}

const config: Config = {
  eol: '\r\n',
  indent: '  ',
};

export default config;
