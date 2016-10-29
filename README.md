# RimTrans

A translation and transform tool for RimWorld Core and Mods!

### Projects

* RimTrans: The library.
* RimTransCMD: The command line tool.
* RimTransGUI: The WPF base application. [TODO]

### Download on [Release Page](https://github.com/duduluu/RimTrans/releases)

---

## RimTrans CMD

![2016-10-29](https://cloud.githubusercontent.com/assets/10762097/19828012/4f11dc40-9ded-11e6-9a8e-207e6404217f.png)

### Learn More on [Wiki Page](https://github.com/duduluu/RimTrans/wiki/RimTrans-CMD)

### Available Commands (v0.1.0)

| Command      | Purpose
|:-------------|:-------------
| Info         | List the information about the application or the mods.
| Set          | Set the global variables: directory of RimWorld or the Workshop mods, target language.
| Trans        | Transform Defs of the specific mod to DefInjected, as well as Keyed.
| Trans-Custom | Similar to trans command, to transform the mod by custom path.

* PS: all of the commands ignore case.

### Examples

```
info /all
set /game-dir:"D:\Games\Rimworld" /workshop-dir:"D:\SteamLibrary\steamapps\workshop\content\294100"
set /target-language:Evlish
trans /mod:"Expanded Prosthetics and Organ Engineering"
trans /mod:725947920 /where:workshop /output:"D:\Git\Rimsenal-zh\ChineseSimplified"
trans-custom /mod-path:"D:\Git\RWMod\RimworldAllowTool\Mods\AllowTool" /lang:ChineseTraditional
trans-custom /mod-path:"D:\Games\Rimworld\Mods\Core" /output:"D:\Git\Ludeon\RimWorld-ChineseSimplified"
```

---

## License

RimTrans is distributed under the [MIT License](https://opensource.org/licenses/MIT).

Included open-source libraries:

* [Steam Local Library](https://github.com/ObsidianMinor/Steam-Local-Library) 0.4.1: MIT License



