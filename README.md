# DeskBox Themes

**A desktop that feels like you.**

English | [简体中文](README.zh-CN.md)

An open theme community for [DeskBox](https://github.com/Tianyu199509/DeskBox). Explore Smoke Glass and Alpine Mist, share visual ideas, and help build a theme ecosystem for Windows desktops.

> **Early development.** The schema-v1 loader and the first two data-only theme
> folders are now on the `deskbox-themes` branch. Full widget rendering is still
> being integrated, so these packs are not a consumer release yet.

## The first collection

| Theme | Visual direction | Availability |
| --- | --- | --- |
| **Smoke Glass · 烟熏玻璃** | Dark translucent surfaces, cyan and violet edges, layered highlights, raised controls | Schema v1 pack; host integration testing |
| **Alpine Mist · 雾凇** | Pale frosted surfaces, glacier-blue cards, soft shadows, quiet depth | Schema v1 pack; host integration testing |

Browse the [theme directory](themes/README.md) for the collection and contribution requirements. Verified application screenshots will be added after migration; reference artwork is not presented as product screenshots.

## Compatibility

These themes will require a **theme-enabled version of DeskBox**. Copying theme files into an unmodified official installation does not enable this proposed theme system.

The repository was forked at the upstream 1.5.0 release commit. The local theme experiments were based on 1.4.9, so they require migration and testing before publication. Building the application currently in this repository does **not** produce those themes.

The planned theme format uses data and assets rather than executable extensions. Versioned interfaces and fallback defaults are design goals, not implemented guarantees.

## Join the community

We welcome theme concepts, original assets, documentation, and compatibility feedback. Start with the [contribution guide](CONTRIBUTING.md), then submit a pull request **to this repository**. Community theme review is separate from upstream DeskBox's contribution policy.

The initial collection will be curated through GitHub pull requests. A browsable marketplace and in-app installation are later milestones.

## Roadmap

- [x] Establish the community repository and bilingual documentation.
- [ ] Migrate and verify Smoke Glass and Alpine Mist on the current upstream baseline.
- [x] Define and implement a versioned data-only theme interface.
- [x] Add folder discovery, preview metadata, theme switching state, and graceful fallback.
- [ ] Connect every supported surface and motion token to the shared renderer.
- [ ] Publish tested theme packs with compatibility information.
- [ ] Open a browsable community catalog.

See the [theme architecture proposal](docs/themes/ARCHITECTURE.md). The development branch is `deskbox-themes`.

## Upstream and license

DeskBox was created by **Tianyu199509 (朱天雨)**. This is an independently maintained community fork, not an official DeskBox theme service. Thanks to the original author and contributors for the application on which this work builds.

The existing [GPL-3.0-only license](LICENSE) and upstream notices are retained. Theme submissions must identify the license and provenance of included assets; see [CONTRIBUTING.md](CONTRIBUTING.md).

Original application documentation: [English](README.upstream.md) · [中文](README.upstream.zh-CN.md). Those files describe upstream DeskBox, including upstream downloads, rather than a released Themes edition.

