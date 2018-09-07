/**
 * The default end of line character. Use \n for LF and \r\n for CRLF.
 * 默认行尾字符。使用 \n 表示 LF，\r\n 表示 CRLF。
 */
export enum EOL {
  LF = '\n',
  CRLF = '\r\n',
}

export interface Config {
  /**
   * The end of line: unix style or windows style.
   */
  eol: '\n' | '\r\n';
  /**
   * The indent style for xml: 2/4 spaces or a tab character.
   */
  indent: '  ' | '    ' | '\t';

  preservingRules: {
    duplicated: boolean;
    nonDefMatched: boolean;
    nonFieldMatched: boolean;
    nonSchemaMatched: boolean;
  };
}

const config: Config = {
  eol: '\r\n',
  indent: '  ',

  preservingRules: {
    duplicated: false,
    nonDefMatched: false,
    nonFieldMatched: true,
    nonSchemaMatched: false,
  },
};

export default config;
