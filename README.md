![RimTrans](https://user-images.githubusercontent.com/10762097/61840069-93ea3900-aec2-11e9-9e27-61cd0f4bb996.png)

[rimtrans-version]: https://img.shields.io/github/tag/RimWorld-zh/RimTrans.svg?label=version&style=flat-square&logo=github
[rimtrans-downloads]: https://img.shields.io/github/downloads/RimWorld-zh/RimTrans/total.svg?style=flat-square&logo=data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABIAAAASCAQAAAD8x0bcAAAAo0lEQVR4AcXKpUEEUAAA0I+TGABnChK7XME2wC1R8EYmMgJOxd0l41qQh57fVXj5hX+jQINpl24ta5YT0imxKNGsgvQ0IdV4iJOn0w7gzJw5Z+BdVbTkmxXXEz7pAUyq+Ul9ZE0wKD/YFLVnTuQ7RczZE9UfPIs6VxZ+KXMu6iRYJrmlFO6CVpJbSmE2yLcm0ZlTyWpDCIqNOfQq3ZNFdeGvfQBdvDOX57HMqQAAAABJRU5ErkJggg==
[rimworld-version]: https://img.shields.io/badge/RimWorld-v1.0.2282-%23f7941e.svg?style=flat-square&logo=data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABIAAAASCAQAAAD8x0bcAAAA10lEQVR4AZXSgUYDQBjA8dsTbIEaNLQoYOwFBhJEZj3BgBjY3mGoeogVLaCep6HaBgMowNqvOB9ngv2AO3/nc3dpZ/Z0DFzrqP0fHJgqPdnfTo4sZGtr2cJxmdQthRevwqd6Ch4RzpwjPERStRHeVFR8CBvVHHWxMdbXdZL+ONXVN7bBVY6GYKaRChpmYJiXI9lKKxItK9kob/SEm4jcCr3twS9S0tZOyWUxeGYiq7v3Y+3OoWyyfZlf5sLcN5bKp9EsgvCumbKg7llpWp5SUNMxMIivspNfip5GeevTmlUAAAAASUVORK5CYII=
[license-mit]: https://img.shields.io/github/license/RimWorld-zh/RimTrans.svg?style=flat-square&logo=data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABIAAAASCAQAAAD8x0bcAAAAsUlEQVR4AaXOgQbCUBSH8ZNKgWB7gMCMSCABEBGg0BsEwID0GmkIKBECWICAQA9QAioIKBM0GV+Xi1D3in78gY9z5C/4BPj2pMMT1Lq2KEKLzEmbC9rE9Msa2HAgYUFJ3sjRoIxLSMqRnnyiwgl4EHNnQEG+YYm2xxXFHu1wRDOeuxKr9cmYsjw1sjiMSdlSFTt8VqSMCLkxo2gOm5zRhmLGFG1ui+okoNYSGzwCPPndC3RGn6EAs8fYAAAAAElFTkSuQmCC

[![RimTrans-version]](https://github.com/RimWorld-zh/RimTrans/releases)
[![RimTrans-downloads]](https://github.com/RimWorld-zh/RimTrans/releases)
[![RimWorld-version]](https://RimWorldgame.com/)
[![license-mit]](https://github.com/RimWorld-zh/RimTrans/blob/master/LICENSE)

## Features

- `Translator`: parse files of Mods, extract language files, edit and publish. WIP
- `Modder`: A tool to create and edit Defs xml files for modding. TODO
- `Translation Workshop`: Players subscribe translation for mods from cloud. TODO

## Development

### Structure

- `app`: The desktop GUI app, based on Vue.js and Electron.js
- `Core`: The RimWorld Core files, includes Defs and English Language
- `extractor`: The core low-level library
- `i18n`: The i18n data for RimTrans
- `io`: The file operating library.
- `Reflection`: The dotnet core project, for get type info from assemblies (.dll files)
- `resources`: Design assets

### Environment

- dotnet core sdk >=2.2.0
- node.js >=12.0.0
- yarn >=1.17.0
- lerna >=3.15.0

### Projects development

```bash
# Install all dependencies for all projects
lerna bootstrap

# Clean all build output directories
lerna run clean

# Build all projects
lerna run build

# Run all tests
lerna run test
```

### App development

```bash
cd app

# Compiles and hot-reloads
# The app will run in development mode and restart when you edit source files
yarn serve:renderer
yarn serve:electron
```
