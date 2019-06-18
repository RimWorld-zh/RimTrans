import Octokit from '@octokit/rest';

const owner = 'RimWorld-zh';
const repo = 'RimTrans';
const tag = 'v0.18.2.6';

async function test(): Promise<void> {
  const octokit = new Octokit();
  await octokit.repos
    .getReleaseByTag({
      owner,
      repo,
      tag,
    })
    .then(({ data: { id } }) =>
      octokit.repos.listAssetsForRelease({
        owner,
        repo,
        release_id: id,
      }),
    )
    .then(({ data: assets }) => {
      assets.forEach(a => console.log(a));
    });
}

test();
