# LunaticModdingAPI
Lunatic is a modding API for the Unity game Lunacid. This is the mod template Unity project that includes the Lunatic scripts.

Before starting modding, you'll want to download and run [AssetRipper](https://github.com/AssetRipper/AssetRipper) on the Lunacid game directory.
This will extract out all of the game assets to a folder that the Lunatic mod template will use.

![AssetRipper](https://github.com/EmpioDavion/LunaticModdingAPI/raw/master/src/common/images/Step1.png "AssetRipper")

When you first open the mod template project, you'll have a few errors pop up. This is because Lunacid game assets are not packaged with the mod template.
Start by opening the Meta Connect tool by going to "Window/Meta Connect" in the top menu, then clicking "Run" at the bottom of the window.
This should ask you to provide paths to the exported project folder and Lunacid.exe in the game directory.
The tool will then copy over the game assets and libraries to the "Packages/com.empiodavion.lunatic@#.#.#/Lunacid/" folder.

![Window/Meta Connect](https://github.com/EmpioDavion/LunaticModdingAPI/raw/master/src/common/images/Step2.png "Window/Meta Connect")
![Run Meta Connect](https://github.com/EmpioDavion/LunaticModdingAPI/raw/master/src/common/images/Step3.png "Run Meta Connect")

This will also copy over meta files for the game scripts, matching them to Lunatic's versions of those scripts.
This will also automatically replace Lunacid game scripts on Lunacid prefabs with Lunatic's versions.
The main reason for this is that prefabs and GameObjects that contain a Component class from a dll will become missing scripts on reload.

After creating your mod scripts, you can create prefabs and assets using them with the Mod Asset Tool by going to "Window/Mod Asset Tool" in the top menu.
Clicking the tabs at the top of the Mod Asset Tool window will change between categories of assets and scripts.
Clicking on the button for your script class will instantiate a prefab or asset of that type.

![Window/Mod Asset Tool](https://github.com/EmpioDavion/LunaticModdingAPI/raw/master/src/common/images/Step4.png "Window/Mod Asset Tool")
![Mod Asset Tool categories](https://github.com/EmpioDavion/LunaticModdingAPI/raw/master/src/common/images/Step5.png "Mod Asset Tool categories")
![Script class prefab](https://github.com/EmpioDavion/LunaticModdingAPI/raw/master/src/common/images/Step6.png "Script class prefab")

NPCs created are based off Demi's GameObject in the HUB_01 scene (Wing's Rest).
Weapons created are based off a copy of the "Lunacid/Resources/weps/HERITAGE SWORD" prefab.
Magics created are based off a copy of the "Lunacid/Resources/magic/FLAME FLARE" prefab.
Items create are based off a copy of the "Lunacid/Resources/items/Crystal Shard" prefab.
