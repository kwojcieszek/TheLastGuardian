# The Last Guardian

*Piter: Rescue Aleksandra*

2.5D side-scrolling platformer (Unity 6000.2, URP, Input System) built from the `Piter_Rescue_Aleksandra_Unity` asset package.
Piter runs to the right through ten long stages to rescue his sister Aleksandra from the sorcerer Malachor.

## Running

Open `Assets/Scenes/MainMenu.unity` and press Play. All scenes are listed in Build Settings (menu first, then ten levels).

## Regenerating content

Prefabs, level scenes, the main menu and the UI sprite library are generated from code:

- **The Last Guardian > Build Everything** - rebuilds `Assets/Prefabs`, `Assets/Scenes`, `Assets/Resources/UiArtLibrary.asset` and Build Settings.
- **The Last Guardian > Run Smoke Test** - plays through the menu and all levels with simulated input and checks saving.
- **The Last Guardian > Delete Save Data / Unlock All Stages** - development helpers.

Batch mode:

```
Unity -batchmode -nographics -projectPath src -executeMethod PiterGame.EditorTools.GameBuildPipeline.BatchBuildEverything
Unity -batchmode -projectPath src -executeMethod PiterGame.EditorTools.QA.SmokeTestLauncher.BatchRun -smokeOutput <folder>
Unity -batchmode -nographics -projectPath src -executeMethod PiterGame.EditorTools.GameBuildPipeline.BatchBuildWindowsPlayer -buildOutput Builds/Windows/Piter.exe
```

Level design data lives in `Assets/Resources/Data/levels.json` and `dialogues.json`; per-level objectives and bosses are in
`Assets/Scripts/Editor/Layout/LevelDesignTable.cs`, character balancing in `Assets/Scripts/Editor/ActorCatalog.cs`.

## Controls

| Action | Keyboard | Gamepad | Touch |
|---|---|---|---|
| Move | A / D, arrows | Left stick, D-pad | Left / right buttons |
| Jump (double jump from stage 2) | Space, W, Up | A | Jump button |
| Attack / reflect orbs | J, K, Ctrl | X, RT | Sword button |
| Drink potion | E, Q | Y | Potion button |
| Pause | Esc, P | Start | Pause icon |

## Save system

Progress is stored as JSON in `Application.persistentDataPath`:

- `piter_save.json` - unlocked stages, best results, current checkpoint, potions. Written at every checkpoint, on stage completion and on "Save & main menu".
- `piter_settings.json` - sound volume, fullscreen, touch controls, dialogue skipping.

"Continue" resumes from the last checkpoint (or the next unfinished stage).

## Gameplay summary

- Three hearts, checkpoints heal and save, falling into a pit returns Piter to the last checkpoint.
- Stomp enemies or use the sword. Health potions heal one heart; up to three can be carried.
- Story objectives (levers, crystals, seals, eggs) and guardians (Bone Captain, Gate Guardian, Young Dragon, Malachor) unlock the exit.
- Malachor has three phases: orb volleys, summoned skeletons and fire runes. Reflect his orbs with the sword.
- Sound effects are synthesized at runtime; no audio files are required.
