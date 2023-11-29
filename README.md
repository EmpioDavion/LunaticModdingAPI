# LunaticModdingAPI

Lunatic is a modding API for the Unity game Lunacid. This is the mod template Unity project that includes the Lunatic scripts.
The mod template project has all layers, sorting layers and tags set up to match the game, and contains prefabs for quick mod asset creation.


## Unity

You'll need to download [Unity version 2020.3.4f1](https://unity.com/releases/editor/whats-new/2020.3.4#release-notes) as this is the version Lunacid was built with, and you may run into problems with other Unity versions.


## BepInEx

Lunatic relies on [BepInEx](https://github.com/BepInEx/BepInEx/) for loading your script dlls at runtime.

BepInEx is a preloader tool commonly used for modding various games that were built in Unity.

Install BepInEx into your Lunacid game directory.


## AssetRipper

Before starting modding, you'll want to download and run [AssetRipper](https://github.com/AssetRipper/AssetRipper) on the Lunacid game directory.

This will extract out all of the game assets to a folder that the Lunatic mod template will use.

I recommend pointing AssetRipper to the LUNACID_Data folder with "File/Open Folder" then clicking "Export/Export all Files".

![AssetRipper](https://raw.githubusercontent.com/EmpioDavion/LunaticModdingAPI/main/Images/Step1.png "AssetRipper")

**You will need to open the exported project in Unity after exporting to generate all the meta files that will be copied later.**


## Opening Lunatic

When you first open the mod template, Unity will ask if you want to start in safe mode. Select no/ignore.

If you are unable to open the project, start in safe mode, then exit safe mode using the button in the top right.

Script errors will not be able to be fixed in safe mode.


## Meta Connect

When you first open the mod template project, you'll have a few errors pop up. This is because Lunacid game assets are not packaged with the mod template.

Start by opening the Meta Connect tool by going to "Window/Meta Connect" in the top menu, then clicking "Run" at the bottom of the window.

This should ask you to provide paths to the exported project solution file, and Lunacid.exe in the game directory.

The tool will then copy over the game assets and libraries to the "Packages/com.empiodavion.lunatic/Lunacid/" folder.

![Window/Meta Connect](https://raw.githubusercontent.com/EmpioDavion/LunaticModdingAPI/main/Images/MetaConnect1.png "Window/Meta Connect")
![Run Meta Connect](https://raw.githubusercontent.com/EmpioDavion/LunaticModdingAPI/main/Images/MetaConnect2.png "Run Meta Connect")


This will also copy over meta files for the game scripts, matching them to Lunatic's versions of those scripts.

This will also automatically replace Lunacid game scripts on Lunacid prefabs with Lunatic's versions.

The main reason for this is that every time the Lunacid game assets are exported, they will have different meta GUIDs.

Which would cause most Lunacid assets to lose their script references.

The process will take 5 to 10 minutes. Afterwards, Unity will import the copied assets.


## Mod Asset Tool

After creating your mod scripts, you can create prefabs and assets using them with the Mod Asset Tool by going to "Window/Mod Asset Tool" in the top menu.

This saves you having to manually add menu items for all of your classes.

Clicking the tabs at the top of the Mod Asset Tool window will change between categories of assets and scripts.

Clicking on the button for your script class will instantiate a prefab or asset of that type in the current project folder.

![Window/Mod Asset Tool](https://raw.githubusercontent.com/EmpioDavion/LunaticModdingAPI/main/Images/ModAssetTool1.png "Window/Mod Asset Tool")
![Mod Asset Tool categories](https://raw.githubusercontent.com/EmpioDavion/LunaticModdingAPI/main/Images/ModAssetTool2.png "Mod Asset Tool categories")
![Script class prefab](https://raw.githubusercontent.com/EmpioDavion/LunaticModdingAPI/main/Images/ModAssetTool3.png "Script class prefab")


## Prefab Bases

NPCs created are based off the template wolf NPC.

Weapons created are based off a copy of the "Lunacid/Resources/weps/HERITAGE SWORD" prefab.

Magics created are based off a copy of the "Lunacid/Resources/magic/FLAME FLARE" prefab.

Items created are based off a copy of the "Lunacid/Resources/items/Crystal Shard" prefab.


## Scripting Notes

I recommend **creating new scripts derived from Lunatic scripts**, rather than using Lunatic's **Mod### scripts** directly.

The reason for this is that attempting to update your mod with new content in future Lunacid updates **may cause your prefabs to break** due to different meta file GUIDs from a fresh AssetRipper export.

This may still happen due to some prefabs targeting Lunatic scripts, since I don't expect people to derive from and reassign every script.

I suggest you use some sort of version control solution for your mods, in case anything does break.


## Packaging Your Mod

You can set the information for your mod by editing the Mod asset in the Assets folder.

Note that changing your mod name will break any Lunatic save data. So only change the name if you have not yet released your mod to the public.

You can edit your local Lunatic save files to match your changed mod name at "Lunacid/Lunacid_Data/SAVE_#.LUNATIC".


Do not rename the Mod.asset file itself, as Lunatic looks for the "Assets/Mod.asset" file to get your mod information.

Ensure all the assets that need to be built into your mod asset bundle are given its AssetBundle Asset Label.

You can set this at the bottom of the Inspector tab when an asset is selected.


Once you are ready to package your mod, you can do so by going to "Assets/Build Mod and Deploy".

![Build Mod and Deploy](https://raw.githubusercontent.com/EmpioDavion/LunaticModdingAPI/main/Images/BuildMod.png "Build Mod and Deploy")

This will build your asset bundles and copy your mod dll(s) into "LunaticModTemplate/Build" folder for zipping and distributing.

Your mod files will also be copied into the "Lunacid/BepInEx/plugins/{Mod.Name}" folder. So you can launch Lunacid straight after building to test your mod.

This will also copy the Lunatic dlls to the "Lunacid/BepInEx/plugins/Lunatic" folder, ensuring you're using the correct version for your mod.

Launching Lunacid normally should have BepInEx run and load your mod dll(s) and assets.


## Pitfalls

### Some of my asset data is not being deserialised from the asset bundle

Unity asset bundles are unable to deserialise classes and structs that are not packaged into the asset bundle.

If you want your classes to deserialise properly, they must derive from ScriptableObject and become an asset. The class assets also need to be labeled to build into your asset bundle.


### I'm getting null reference exceptions when deserialising my ModGame data

Chances are that your ModGame data does not exist yet for that save file and that you need to assign defaults for new saves.


### Some of my items are losing data on equip or loading a save file

Lunatic will handle loading basic data for items for you, such as any types of items collected, weapon experience, and recipes unlocked, etc.

You'll need to handle loading any custom data that is not part of Lunacid normally.

You can do this by using Lunatic.GetModData and Lunatic.SetModData as shown in the TemplateGame script.


### My weapon is throwing exceptions when it's loaded, equipped or spawned

Check to make sure that the root GameObject of the weapon prefab is set to inactive.

Lunacid runs some code on the weapon before setting it to active. It's possible your weapon is accessing something before it's set up.


## Class Information

Most classes are straightforward and you can use the Show help toggle in the inspector for each class to see what each value means.

You can also check the template class script files for ideas on how to use each class. Also check the Lunacid prefabs to see how the game's objects are built.

### ModItemPickup

An object in the scene that serves as a container for inventory objects that can be collected by the player, this can contain a weapon, material, item, magic, or gold.


### ModClass

The class/job the player can select when creating their character. Examples of Lunacid classes/jobs are Thief, Knight, Witch.


### ModGame

A top level manager class for your mod. You should keep track of any custom data based on save files in here.

The mod data you load in here is per save file. I may add functionality for global mod data at a later date.


### ModWeapon

Any weapon that can be wielded by the **player**. Does not include any weapons that enemies or NPCs use.


### ModMagic

The class for spells cast from rings, such as Flame Flare and Wind Dash.


### ModItem

A useable item that can be assigned to the player's item hotbar, such as Health Vial and Crystal Shard.


### ModMaterial

The materials that are used in the alchemy table to forge recipe items.


### ModRecipe

Is used to define a recipe that can be forged in the alchemy table.


### ModMultipleStates

Typically used by NPCs for their dialogue. Toggles objects on and off based on a state value. This is the base class for modded NPCs.

An example use case is having a bridge that becomes broken when a boss is defeated, and then later fixed again when an NPC is rescued.


### ModScene

A class that is used for scene specific functionality. Has events that fire for only the specified scene.


### ModDialog

The class that is used for handling NPC dialogue chains and activating triggers typically to open menus such as shops, or to change the NPC's current state.

Dialogue in Lunacid is a bit convoluted in how it works. Some ModDialogs have a script on their TALK_SND GameObject that is activated when the player talks to their NPC.

This script will then turn on another GameObject that will set the current state of the NPC to a new value.

The reason that this functionality isn't just on the TALK_SND GameObject is that it gets turned on and off repeatedly as the NPC talks.

Only the NPC's state is saved when saving, loading, or warping. The dialogue progress itself is reset to zero.

This is why NPCs have multiple stages or states. Each state serves as a checkpoint for new dialogue.


### LSceneObjectGroup

Contains a list of scene objects that will spawn based on conditions when the specified scene is loaded.

The intent for this class is that it allows you to set up objects for a scene in the editor, instead of having to trial and error position data.

This also lets you set conditions for objects and groups of objects spawning, such as checking save data for whether an item has been collected yet or not.

If no condition is set, it defaults to true.

The signature for an LSceneObjectGroup condition method is
bool MethodName(LSceneObjectGroup sceneObjectGroup) { }


### LSceneObject

An object in a LSceneObjectGroup list that defines an object to spawn in a scene under a certain condition.

You can place prefab objects into an existing Lunacid scene in the editor, then drag them onto the LSceneObject Game Object field to copy the transform data.

Only prefab assets may be spawned, as regular scene objects will not exist on build.

If no condition is set, it defaults to true.

The signature for an LSceneObject condition method is
bool MethodName(LSceneObject sceneObject) { }
