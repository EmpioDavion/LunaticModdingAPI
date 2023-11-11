# LunaticModdingAPI
Lunatic is a modding API for the Unity game Lunacid. This is the mod template Unity project that includes the Lunatic scripts.
The mod template project has all layers, sorting layers and tags set up to match the game, and contains prefabs for quick mod asset creation.

Lunatic relies on [BepInEx](https://github.com/BepInEx/BepInEx/) for loading your script dlls at runtime.

Install it into your Lunacid game directory.


Before starting modding, you'll want to download and run [AssetRipper](https://github.com/AssetRipper/AssetRipper) on the Lunacid game directory.

This will extract out all of the game assets to a folder that the Lunatic mod template will use.

I recommend pointing AssetRipper to the LUNACID_Data folder with "File/Open Folder" then clicking "Export/Export all Files".

![AssetRipper](https://raw.githubusercontent.com/EmpioDavion/LunaticModdingAPI/main/Images/Step1.png "AssetRipper")


When you first open the mod template project, you'll have a few errors pop up. This is because Lunacid game assets are not packaged with the mod template.

Start by opening the Meta Connect tool by going to "Window/Meta Connect" in the top menu, then clicking "Run" at the bottom of the window.

This should ask you to provide paths to the exported project folder and Lunacid.exe in the game directory.

The tool will then copy over the game assets and libraries to the "Packages/com.empiodavion.lunatic@#.#.#/Lunacid/" folder.

![Window/Meta Connect](https://raw.githubusercontent.com/EmpioDavion/LunaticModdingAPI/main/Images/Step2.png "Window/Meta Connect")
![Run Meta Connect](https://raw.githubusercontent.com/EmpioDavion/LunaticModdingAPI/main/Images/Step3.png "Run Meta Connect")


This will also copy over meta files for the game scripts, matching them to Lunatic's versions of those scripts.

This will also automatically replace Lunacid game scripts on Lunacid prefabs with Lunatic's versions.

The main reason for this is that prefabs and GameObjects that contain a Component class from a dll will become missing scripts on reload.

After creating your mod scripts, you can create prefabs and assets using them with the Mod Asset Tool by going to "Window/Mod Asset Tool" in the top menu.

Clicking the tabs at the top of the Mod Asset Tool window will change between categories of assets and scripts.

Clicking on the button for your script class will instantiate a prefab or asset of that type.

![Window/Mod Asset Tool](https://raw.githubusercontent.com/EmpioDavion/LunaticModdingAPI/main/Images/Step4.png "Window/Mod Asset Tool")
![Mod Asset Tool categories](https://raw.githubusercontent.com/EmpioDavion/LunaticModdingAPI/main/Images/Step5.png "Mod Asset Tool categories")
![Script class prefab](https://raw.githubusercontent.com/EmpioDavion/LunaticModdingAPI/main/Images/Step6.png "Script class prefab")


NPCs created are based off Demi's GameObject in the HUB_01 scene (Wing's Rest).

Weapons created are based off a copy of the "Lunacid/Resources/weps/HERITAGE SWORD" prefab.

Magics created are based off a copy of the "Lunacid/Resources/magic/FLAME FLARE" prefab.

Items create are based off a copy of the "Lunacid/Resources/items/Crystal Shard" prefab.


I recommend **creating new scripts derived from Lunatic scripts**, rather than using Lunatic's Mod### scripts directly.

The reason for this is that attempting to update your mod with new content in future Lunacid updates **may cause your prefabs to break** due to different meta file GUIDs from a fresh AssetRipper export.

This may still happen due to some prefabs targeting Lunatic scripts, since I don't expect people to derive from and reassign every script.

I will have to investigate retargeting scripts in prefabs.


Once you are ready to package your mod, you can do so by going to "Assets/Build Mod". This will build your asset bundles and copy your mod dlls into LunaticModTemplate/Build folder.

Copy these files into your "Lunacid/BepInEx/plugins" folder, and then launch Lunacid normally.
