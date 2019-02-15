// tslint:disable:no-any no-unsafe-any no-console
import { file } from '@rimtrans/service';

/**
 * Test
 */
export default async function(): Promise<void> {
  const content = await file.get({
    params: {
      path:
        '/mnt/d/Games/SteamLibrary/steamapps/common/RimWorld/Mods/Core/About/About.xml',
    },
  });

  console.log(content);
}
