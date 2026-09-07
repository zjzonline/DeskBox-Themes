# Theme collection / 主题目录

The first two schema-v1 theme folders are now included. They are loaded by the
theme-enabled development branch and are ready for host integration testing.

首批两个 schema-v1 主题文件夹已经加入仓库，由主题版开发分支加载，目前进入宿主视觉联调阶段。

| Theme | Palette and surface | Status |
| --- | --- | --- |
| [Smoke Glass / 烟熏玻璃](smoke-glass) | Charcoal, cyan, violet; layered translucent edges | Schema v1 pack; integration testing |
| [Alpine Mist / 雾凇](alpine-mist) | Frost white, glacier blue; soft surface depth | Schema v1 pack; integration testing |

This directory will host reviewed community submissions. Start from an existing
pack and validate `theme.json` against [schema-v1.json](schema-v1.json). See
[CONTRIBUTING.md](../CONTRIBUTING.md) for author, license, preview, and compatibility requirements.

本目录用于收录经审核的社区投稿。主题文件仅包含 `theme.json` 与安全的预览/图片资源，不允许 DLL、脚本或任意 XAML。

For local development, built-in packs are copied to the app's `Themes` output
folder. User packs belong in `%LOCALAPPDATA%\DeskBox\Themes\<theme-id>`; Debug
builds that set `DESKBOX_DEV_DATA_ROOT` use `<development-root>\Themes` instead.

