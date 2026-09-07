# External themes: proposal / 外部主题提案

Status: schema v1 discovery, validation, preview metadata, switching state, and
Classic fallback are implemented on `deskbox-themes`. Generic visual rendering
of every token is the current integration step.

状态：`deskbox-themes` 已实现 schema v1 的发现、校验、预览元数据、切换状态及经典主题回退；当前正在把全部视觉参数接入通用渲染层。

## Separation

Theme data → validation and API-version checks → shared appearance parameters → application-owned rendering.

主题文件提供参数；宿主负责校验、版本兼容、控件渲染及交互。颜色、渐变、边框、圆角先行，反光层、材质、卡片变体及动效预设逐步开放。

## Folder structure

```text
User theme directory/
  alpine-mist/
    theme.json
    preview.svg
```

The host loads bundled packs from its `Themes` output folder and user packs from
`%LOCALAPPDATA%\DeskBox\Themes`. Debug data-root isolation is respected. A pack
declares `schemaVersion` and `minimumDeskBoxVersion`; incompatible packs are
reported and skipped.

宿主从程序输出目录的 `Themes` 加载内置主题，从 `%LOCALAPPDATA%\DeskBox\Themes` 加载用户主题，并遵循 Debug 数据隔离目录。主题声明 `schemaVersion` 与 `minimumDeskBoxVersion`；不兼容的主题会被报告并跳过。

Schema v1 supports identity, localized names, authorship, license, preview,
surface colors, edge/specular/shadow colors, geometry, material intent, deep
surface assignments, and bounded open/close/hover/press motion values.

## Host responsibilities

- Validate types, ranges, file sizes, paths, and API versions.
- Restrict assets to the theme directory; reject traversal and unsafe links.
- Use built-in defaults for missing fields and a usable fallback for invalid themes.
- Respect high contrast, reduced motion, and resource-saving preferences.
- Implement layered surfaces generically instead of branching on individual theme names.
- Avoid executing theme-provided DLLs, scripts, or arbitrary XAML.

## Migration before publication

The local 1.4.9 prototypes include application changes beyond styling. Compare them against their original baseline before moving theme changes onto the fork's 1.5.0 baseline. Do not overwrite current upstream application files wholesale. Startup, workspaces, and performance changes need separate review.

本地 1.4.9 原型还包含样式以外的功能修改。迁移到 1.5.0 前应按原始基线梳理差异，主题代码逐项迁移，启动、工作区及性能改动分别审查。

## Acceptance for the first installable collection

- Both themes render correctly in expanded and collapsed widgets.
- Windows 10/11 and DPI coverage is recorded, with unsupported effects documented.
- Theme switching and malformed-theme fallback are verified.
- Changing themes does not alter file ownership, placement, or workspace content.
- Each pack records a tested host version, authorship, asset licenses, and actual screenshots.
- Install instructions only appear after a compatible host is available.

## Upstream relationship

This is an independent experiment. Upstream adoption is not assumed. Keep the interface small and adaptable to the original project's future extension architecture.

