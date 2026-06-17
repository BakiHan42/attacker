# attacker

Unity game project.

## Requirements

- **Unity `6000.3.16f1`** — everyone must use this exact version. A different
  version silently re-imports/re-serializes assets and creates noisy conflicts.
- **Git LFS** — large binary assets (textures, models, audio, archives) are
  stored in LFS. You must have it installed or you'll get pointer files instead
  of real assets.

## First-time setup

Run once per machine after cloning:

```bash
# 1. Install Git LFS hooks (pulls real binaries, not pointers)
git lfs install
git lfs pull

# 2. Configure Unity SmartMerge so scene/prefab merges don't corrupt
#    (adjust the path to your Unity install)
git config merge.unityyamlmerge.name "Unity SmartMerge"
git config merge.unityyamlmerge.driver \
  "'/path/to/Editor/Data/Tools/UnityYAMLMerge' merge -p %O %B %A %A"
git config merge.unityyamlmerge.recursive binary
```

The UnityYAMLMerge tool ships with the editor, e.g. on Linux:
`<UnityInstall>/6000.3.16f1/Editor/Data/Tools/UnityYAMLMerge`.

## Project structure

```
Assets/
  _Project/        <- OUR game (scripts, scenes, prefabs, art, audio, ...)
    Art/           <- our models, materials, characters, UI art
    Scenes/        <- Subway.unity (main level), MainMenu.unity, SampleScenes/
    Scripts/       <- gameplay code
    Prefabs/  Data/  Audio/  Input/  Settings/  Animation/
  _ThirdParty/     <- imported asset-store / external packages (don't edit)
  Plugins/         <- DOTween etc.
  TextMesh Pro/    <- Unity package
```

- Put **our** work under `Assets/_Project/`.
- Put **imported/3rd-party** packs under `Assets/_ThirdParty/`.

## Working together (please read)

- **Branch off `main`**, open a **Pull Request**; don't push straight to `main`.
- **Pull/merge `main` often.** Small frequent merges beat one giant divergence.
- **Scenes & prefabs are hard to merge.** Coordinate: only one person edits a
  given scene at a time, or split big scenes into additive sub-scenes / prefabs.
  SmartMerge helps but is not magic.
- **Never commit** the original `.zip`/`.rar` download bundle once a model is
  imported — keep only the extracted assets.
- Write commit messages someone can understand later (not `idk` / `.`).
