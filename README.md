Build Setup Instructions:
===============
These directions are posted at: https://github.com/Dheuster/NPCCustomPortraitEnabler. Note that these directions are not for installation and usage of the mod in-game. They are for setting up the build environment to make use of the mod's source code (To further enhance it, etc..)

1. Clone NPCCustomPortraitEnabler:

```
    git clone https://github.com/Dheuster/NPCCustomPortraitEnabler.git
```

2. Make sure your WOTR is patched and up to date. I recommend uninstalling it and re-installing it to make sure you don't have any garbage from previous modding attempts. When you uninstall make sure that following directories don't exist:
   	 
   If you downloaded any Portraits or Modifications, you may wish to back them up:

   - **%LocalAppData%/../LocalLow\Owlcat Games\Pathfinder Wrath Of The Righteous\Portraits**
   - **%LocalAppData%/../LocalLow\Owlcat Games\Pathfinder Wrath Of The Righteous\Modifications"**
   - **%LocalAppData%/../LocalLow\Owlcat Games\Pathfinder Wrath Of The Righteous\Saves**

   Once anything you wish to backup has been saved off,  make sure the following directories do not exist:

   - **.../steam/steamapps/common/Pathfinder Second Adventure**
   - **%LocalAppData%/../LocalLow\Owlcat Games\Pathfinder Wrath Of The Righteous**

   If the computer won't remove the directory because it is "in use by another process", you may have to restart the computer. 
  
   Once the above directories have been removed, re-install the game from Steam or where ever you purchased it. 
  
3. Download/Install/Start [Unity 2019.4.26f.1](https://unity.com/releases/editor/whats-new/2019.4.26)
   * Open the project associated with this git clone project. 
   * If you see warning about project being made with older version of Unity (4.20), Change the version and hit continue when it presents the "Non-Matching Editor Installation" warning. 
4. Modification Tools -> Setup Project -> Point to install dir of WOTR
   - **.../steam/steamapps/common/Pathfinder Second Adventure" for Steam**
5. When Importing is done:
   - If it prompts you for "API Update", select "No Thanks"
   - File -> Exit -> Re-Open the project
6. Modificiation Tools -> Setup Render Pipeline.
   - In the "Scene" Tab, Make sure the first pull-down is set to "Render Paths"
   - _(Make sure it isn't set to any of the Shaded modes or UNITY will constantly spam the console with error messages everytime the mouse hovers over the  rendered area of the tool.)_
   - File -> Exit and Then Re-Open the project
7. Upon restart, use **Modification Tools -> Build**
   - You will be prompted for which project to build
   - Select NPCCustomPortraitEnabler. 
8. Build product can be found under the "build" directory (Where-ever you git cloned the project)

9. Start -> Windows System -> File Explorer 

10. In the Location Bar, enter: (I refer to this as the WOTR_CONFIG_DIR)

```
    %LocalAppData%/../LocalLow\Owlcat Games\Pathfinder Wrath Of The Righteous
```
     
11. If all is well, you should see folders\files that look similar to: 

```
    > Saved Games
    > Screenshots
    > Unity
    combatLog.txt
    GameLog.txt
    GameLogFull.txt
    general_settings.json
    Player.log

    (">" <- indicates folder/directory)
```
   - You might also see a "Portraits" directory if you have installed custom portraits in the past. 
   - You might also see a "Modifications" directory if you have installed mods before.
   - You might also see a "OwlcatModificationManagerSettings.json" file if you have installed mods before. 

12. _(a)_ If you **do NOT see** OwlcatModificationManagerSettings.json file, create it:

   - Right click "general_settings.json" and select "Copy"
   - Right click the background and select "Paste"
   - Rename the copied file to "OwlcatModificationManagerSettings"
   - Double click "OwlcatModificationManagerSettings" to edit
   - Within Notepad (or whatever editor you have configured)
      - Select all (CTRL+A works in most windows apps)
      - Delete (Hit the delete key)
      - Paste the following:
```json5
    {
        "EnabledModifications": ["NPCCustomPortraitEnabler"]
    }
```
   - Save the file 

12. _(b)_ If you **do see** OwlcatModificationManagerSettings, update it:

     * Double click the file "OwlcatModificationManagerSettings" to edit
     * Add "NPCCustomPortraitEnabler" to the list of enabled 
       modifications. It should look something like:
```json5
    {
        "EnabledModifications": ["<MOD1>","<MOD2>","NPCCustomPortraitEnabler"]
    }
```

13. If you do NOT see a Modifications Folder you will need to create it:
   - Right click the background and selecting "New -> Folder". 
   - Name the New Folder "Modifications"

14. If you do NOT see a Portraits Folder, you should create it:
   - Right click the background and selecting "New -> Folder". 
   - Name the New Folder "Portraits"

15. Confirm: In the Location Bar, enter:

```
    %LocalAppData%/../LocalLow\Owlcat Games\Pathfinder Wrath Of The Righteous
```

   If all is well, you should see something like this:

```
    > Modifications
    > Portraits
    > Saved Games
    > Screenshots
    > Unity
    combatLog.txt
    GameLog.txt
    GameLogFull.txt
    general_settings.json
    OwlcatModificationManagerSettings.json
    Player.log
```

16. Copy the generated build directory:

    GIT_CLONE/build/NPCCustomPortraitEnabler 
	
    -- to -- 

    WOTR_CONFIG_DIR/Modifications
	
    Under the modifications directory you should now see:
	
```
    > Modifications
        > NPCCustomPortraitEnabler
    > Portraits
    > Saved Games
    > Screenshots
    > Unity
    combatLog.txt
    GameLog.txt
    GameLogFull.txt
    general_settings.json
    OwlcatModificationManagerSettings.json
    Player.log
```
    	
Testing and Development
========

_**TODO**_

* Start Game
* Exit Game
* Edit the generated "Modificiations/NPCCustomPortraitEnabler.json"
* Set debug to true
* Set CreateDirectories to true
* Now when you run the game, logDebug and logAlways methods within the mod code will output to: WOTR_CONFIG_DIR/GameLogFull.txt
* The main tool I used to view into the game code and expose what methods/interfaces were available: https://github.com/dnSpy/dnSpy . There may be some official documentation somewhere, but I haven't found it.
  
Creating Your own Mod
========

All content of your modification must be placed in folder with **Modification** scriptable object or it's subfolders.

### Scripts

All of your scripts must be placed in assemblies (in folder with ***.asmdef** files or it's subfolders). **Never** put your scripts (except Editor scripts) in other places.

### Content

All of your content (assets, prefabs, scenes, sprites, etc) must be placed in **_your-modification-name_/Content** folder.

### Blueprints

Blueprints are JSON files which represent serialized version of static game data (classes inherited from **SimpleBlueprint**).

* Blueprints must have file extension ***.jbp** and must be situated in **_your-modification-name_/Blueprints** folder.
    * _example: Examples/Basics/Blueprints/TestBuff.jbp_

    ```json5
    // *.jbp file format
    {
        "AssetId": "unity-file-guid-from-meta", // "42ea8fe3618449a5b09561d8207c50ab" for example
        "Data": {
            "$type": "type-id, type-name", // "618a7e0d54149064ab3ffa5d9057362c, BlueprintBuff" for example
            
            // type-specific data
        }
    }
    ```

    * if you specify **AssetId** of an existing blueprint (built-in or from another modification) then the existing blueprint will be replaced

* For access to metadata of all built-in blueprints use this method
    ```C#
    // read data from <WotR-installation-path>/Bundles/cheatdata.json
    // returns object {Entries: [{Name, Guid, TypeFullName}]}
    BlueprintList Kingmaker.Cheats.Utilities.GetAllBlueprints();
    ```

* You can write patches for existing blueprints: to do so, create a ***.patch** JSON file in **_your-modification-name_/Blueprints** folder. Instead of creating a new blueprint, these files will modify existing ones by changing only fields that are specified in the patch and retaining everything else as-is.

    * _Example 1: Examples/Basics/Blueprints/ChargeAbility.patch_
    * 
    * _Example 2: Examples/Basics/Blueprints/InvisibilityBuff.patch_ 

    * Connection between the existing blueprint and the patch must be specified in **BlueprintPatches** scriptable object _(right click in folder -> Create -> Blueprints' Patches)_

        * _example: Examples/Basics/BlueprintPatches.asset_
      
    * **OLD**: Newtonsoft.Json's Populate is used for patching (_#ArrayMergeSettings and _#Entries isn't supported)
  
      * https://www.newtonsoft.com/json/help/html/PopulateObject.htm 
  
    * **NEW** (game version 1.1.1): Newtonsoft.Json's Merge is used for patching
  
      * https://www.newtonsoft.com/json/help/html/MergeJson.htm

    ```json5
    // *.patch file format: change icon in BlueprintBuff and disable first component
    {
      "_#ArrayMergeSettings": "Merge", // "Union"/"Concat"/"Replace"
      "m_Icon": {"guid": "b937cb64288636b4c8fd4ba7bea337ea", "fileid": 21300000},
      "Components": [
        {
          "m_Flags": 1
        }
      ]
    }
    ```
  _OR_

    ```json5
    {
      "_#Entries": [
        {
          "_#ArrayMergeSettings": "Merge", // "Union"/"Concat"/"Replace"
          "m_Icon": {"guid": "b937cb64288636b4c8fd4ba7bea337ea", "fileid": 21300000},
          "Components": [
            {
              "m_Flags": 1
            }
          ]
        }
      ]
    }
    ```

### Localization

You can add localized strings to the game or replace existing strings. Create **enGB|ruRU|deDE|frFR|zhCN|esES.json** file(s) in **_your-modification-name_/Localization** folder.

* _example: Examples/Basics/Localizations/enGB.json_

* You shouldn't copy enGB locale with different names if creating only enGB strings: enGB locale will be used if modification doesn't contains required locale.

* The files should be in UTF-8 format (no fancy regional encodings, please!)

```json5
// localization file fromat
{
    "strings": [
        {
            "Key": "guid", // "15edb451-dc5b-4def-807c-a451743eb3a6" for example
            "Value": "whatever-you-want"
        }
    ]
}
```

### Assembly entry point

You can mark static method with **OwlcatModificationEnterPoint** attribute and the game will invoke this method with corresponding _OwlcatModification_ parameter once on game start. Only one entry point per assembly is allowed.

* _example: Examples/Basics/Scripts/ModificationRoot.cs (ModificationRoot.Initialize method)_

```C#
[OwlcatModificationEnterPoint]
public static void EnterPoint(OwlcatModification modification)
{
    ...
}
```

### GUI

Use **OwlcatModification.OnGUI** for inserting GUI to the game. It will be accessible from modifications' window (_ctrl+M_ to open). GUI should be implemented with **IMGUI** (root layout is **vertical**).

* _example: Examples/Basics/Scripts/ModificationRoot.cs (ModificationRoot.Initialize method)_

### Harmony Patching

Harmony lib is included in the game and you can use it for patching code at runtime.

* _example: Examples/Basics/Scripts/ModificationRoot.cs (ModificationRoot.Initialize method) and Examples/Basics/Scripts/Tests/HarmonyPatch.cs_

* [Harmony Documentation](https://harmony.pardeike.net/articles/intro.html)

```C#
OwlcatModification modification = ...;
modification.OnGUI = () => GUILayout.Label("Hello world!");
```

### Storing data

* You can save/load global modification's data or settings with methods _OwlcatModification_.**LoadData** and  _OwlcatModification_.**SaveData**. Unity Serializer will be used for saving this data.

    * _Example: Examples/Basics/Scripts/ModificationRoot.cs (ModificationRoot.TestData method)_

    ```C#
    [Serialzable]
    public class ModificationData
    {
        public int IntValue;
    }
    ...
    OwlcatModification modification = ...;
    var data = modification.LoadData<ModificationData>();
    data.IntValue = 42;
    modification.SaveData(data);
    ```

* You can save/load per-save modification's data or settings by adding **EntityPartKeyValueStorage** to **Game.Instance.Player**.

    * _Example: Examples/Basics/Scripts/Tests/PerSaveDataTest.cs_

    ```C#
    var data = Game.Instance.Player.Ensure<EntityPartKeyValueStorage>().GetStorage("storage-name");
    data["IntValue"] = 42.ToString();
    ```

### EventBus

You can subscribe to game events with **EventBus.Subscribe** or raise your own event using **EventBus.RaiseEvent**.

* _Example (subscribe): Examples/Basics/Scripts/ModificationRoot.cs (ModificationRoot.Initialize method)_

* Raise your own event:

    ```C#
    interface IModificationEvent : IGlobalSubscriber
    {
        void HandleModificationEvent(int intValue);
    }
    ...
    EventBus.RaiseEvent<IModificationEvent>(h => h.HandleModificationEvent(42))
    ```

### Rulebook Events

* **IBeforeRulebookEventTriggerHandler** and **IAfterRulebookEventTriggerHandler** exists specifically for modifications. These events are raised before _OnEventAboutToTrigger_ and _OnEventDidTigger_ correspondingly.
* Use _RulebookEvent_.**SetCustomData** and _RulebookEvent_.**TryGetCustomData** to store and read your custom RulebookEvent data.

### Resources

_OwlcatModification_.**LoadResourceCallbacks** is invoked every time when a resource (asset, prefab or blueprint) is loaded.

### Game Modes and Controllers

A **Controller** is a class that implements a particular set of game mechanics. It must implementi _IController_ interface.

**Game Modes** (objects of class _GameMode_) are logical groupings of **Controllers** which all must be active at the same time. Only one **Game Mode** can be active at any moment. Each frame the game calls **Tick** method for every **Controller** in active **Game Mode**. You can add your own logic to Pathfinder's main loop or extend/replace existing logic using **OwlcatModificationGameModeHelper**.

* _Example (subscribe): Examples/Basics/Scripts/Tests/ControllersTest.cs_

### Using Pathfinder shaders

Default Unity shaders doesn't work in Pathfinder. Use shaders from **Owlcat** namespace in your materials. If you don't know what you need it's probably **Owlcat/Lit** shader.

### Scenes

You can create scenes for modifications but there is a couple limitations:

* if you want to use Owlcat's MonoBehaviours (i.e. UnitSpawner) you must inherit from it and use child class defined in your assembly

* place an object with component **OwlcatModificationMaterialsInSceneFixer** in every scene which contains Renderers

### Helpers

* Copy guid and file id as json string: _right-click-on-asset -> Modification Tools -> Copy guid and file id_

* Copy blueprint's guid: _right-click-on-blueprint -> Modification Tools -> Copy blueprint's guid_
    
* Create blueprint: _right-click-in-folder -> Modification Tools -> Create Blueprint_

* Find blueprint's type: _Modification Tools -> Blueprints' Types_

### Interactions and dependencies between modifications

Work in progress. Please note that users will be able to change order of mods in the manager. We're planning to provide the ability to specify a list of dependencies for your modification, but it will only work as a hint: the user will be responsible for arranging a correct order of mods in the end.
 
### Testing

* Command line argument **-start_from=_area-name/area-preset-name_** allows you to start game from the specified area without loading main menu.
* Cheat **reload_modifications_data** allows you to reload content, blueprints and localizations. All instantiated objects (prefab instances, for example) stays unchanged.
