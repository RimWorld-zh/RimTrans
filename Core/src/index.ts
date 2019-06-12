import fs from 'fs';
import pth from 'path';
import globby from 'globby';

export async function getCoreFiles(): Promise<{
  defs: Record<string, string>;
  defInjected: Record<string, string>;
  keyed: Record<string, string>;
  strings: Record<string, string>;
}> {
  const cwd = pth.join(__dirname, '..');
  const [defs, defInjected, keyed, strings] = await Promise.all(
    [
      'Defs/**/*.xml',
      'Languages/English/DefInjected/**/*.xml',
      'Languages/English/Keyed/**/*.xml',
      'Languages/English/Strings/**/*.txt',
    ].map(async pattern => {
      const result: Record<string, string> = {};
      const files = await globby(pattern, { cwd });
      await Promise.all(
        files.map(async f => {
          const content = await fs.promises.readFile(pth.join(cwd, f), 'utf-8');
          result[f] = content;
        }),
      );
      return result;
    }),
  );

  return { defs, defInjected, keyed, strings };
}
