# External themes: proposal / 外部主题提案

Status: design only; no external theme loader has been implemented in this repository.

状态：设计提案，本仓库尚未实现外部主题加载器。

## Separation

Theme data → validation and API-version checks → shared appearance parameters → application-owned rendering.

主题文件提供参数；宿主负责校验、版本兼容、控件渲染及交互。颜色、渐变、边框、圆角先行，反光层、材质、卡片变体及动效预设逐步开放。

## Proposed folder structure

```text
User theme directory/
  alpine-mist/
    manifest.json
    theme.json
    preview.png
    assets/
```

The directory and schema are not final. A user-data location outside the installation directory is preferred. Preserving files during updates is distinct from preserving compatibility: the host must continue supporting the declared theme API version.

目录和字段尚未定稿。拟放在安装目录之外的用户数据位置；升级保留文件不等于任意新版都兼容，仍需稳定的主题接口。

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

