/* eslint-disable no-console */
import * as io from '@rimtrans/io';

async function genColors(): Promise<void> {
  const colorsJson: Record<string, Record<string, string>> = await io.load(
    io.join(__dirname, 'variables', 'colors.json'),
  );

  const blocks: [string, string][][] = [];

  Object.entries(colorsJson).forEach(([name, map]) => {
    const block: [string, string][] = [];
    blocks.push(block);

    Object.keys(map)
      .sort()
      .forEach(level => {
        const color = map[level];
        block.push([`--color-${name}-${level}`, color]);
      });

    Object.keys(map)
      .sort()
      .forEach(level => {
        const color = map[level];
        const hex = color.replace('#', '');
        const [r, g, b] = [[0, 2], [2, 4], [4, 6]].map(([start, end]) =>
          Number.parseInt(hex.substring(start, end), 16),
        );
        const alphaBlock: [string, string][] = [];
        blocks.push(alphaBlock);
        for (let index = 1; index <= 9; index++) {
          alphaBlock.push([
            `--color-${name}-${level}-a${index * 10}`,
            `rgba(${r}, ${g}, ${b}, ${index / 10})`,
          ]);
        }
      });
  });

  const content = blocks
    .map(block => block.map(([name, value]) => `  ${name}: ${value};`).join('\n'))
    .join('\n\n');

  const wrap = `%variables-color-palette {\n${content}\n}\n`;

  await io.save(
    io.join(
      __dirname,
      '..',
      'src',
      'renderer',
      'components',
      'base',
      'variables-color-palette.scss',
    ),
    wrap,
  );
}

export async function genVariables(): Promise<void> {
  await Promise.all([genColors()]);
}
