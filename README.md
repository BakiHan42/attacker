# attacker

Unity game project.

## Requirements

- **Unity `6000.3.16f1`** — everyone must use this exact version. A different
  version silently re-imports/re-serializes assets and creates noisy conflicts.
- **GitHub Desktop** — clone and push with this; Git LFS is bundled and works
  automatically, no extra steps needed for binaries.

## First-time setup

### 1. Clone with GitHub Desktop

Open GitHub Desktop → **File → Clone repository** → paste the repo URL.
GitHub Desktop will handle Git LFS automatically and download all assets.

### 2. Configure Unity SmartMerge (one-time, requires terminal)

This makes scene/prefab merges semantic instead of corrupting them.
Open **Git Bash** (Windows) or **Terminal** (Mac) and run:

**Windows** (Git Bash):
```bash
git config --global merge.unityyamlmerge.name "Unity SmartMerge"
git config --global merge.unityyamlmerge.driver \
  "'C:/Program Files/Unity/Hub/Editor/6000.3.16f1/Editor/Data/Tools/UnityYAMLMerge.exe' merge -p %O %B %A %A"
git config --global merge.unityyamlmerge.recursive binary
```

**Mac** (Terminal):
```bash
git config --global merge.unityyamlmerge.name "Unity SmartMerge"
git config --global merge.unityyamlmerge.driver \
  "'/Applications/Unity/Hub/Editor/6000.3.16f1/Unity.app/Contents/Tools/UnityYAMLMerge' merge -p %O %B %A %A"
git config --global merge.unityyamlmerge.recursive binary
```

You only run this once. GitHub Desktop will use it automatically after that.

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

- **Create a branch** in GitHub Desktop before starting work → open a
  **Pull Request** on GitHub when done; don't push straight to `main`.
- **Fetch/pull `main` often** (GitHub Desktop: **Fetch origin** button).
  Small frequent merges beat one giant divergence.
- **Scenes & prefabs are hard to merge.** Coordinate: only one person edits a
  given scene at a time, or split big scenes into additive sub-scenes / prefabs.
  SmartMerge helps but is not magic.
- **Never commit** the original `.zip`/`.rar` download bundle once a model is
  imported — keep only the extracted assets.
- Write commit messages someone can understand later (not `idk` / `.`).
