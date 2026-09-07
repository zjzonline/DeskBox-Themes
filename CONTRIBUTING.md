# Contributing to DeskBox Themes / 参与主题社区

This guide applies to **zjzonline/DeskBox-Themes**. Upstream DeskBox has its own contribution policy, preserved in [CONTRIBUTING.upstream.md](CONTRIBUTING.upstream.md).

本指南适用于本主题社区。原作者的贡献规则保存在 [CONTRIBUTING.upstream.md](CONTRIBUTING.upstream.md)，我们不会代表原作者承诺合并。

## Submit a theme concept / 提交主题创意

1. Fork this repository and create a branch from `deskbox-themes`.
2. Add a folder under `themes/<your-theme-id>/` with a README describing the theme, author, status, and asset licenses.
3. Include original mockups or sanitized application screenshots. Clearly label mockups; remove personal filenames, paths, and account details from screenshots.
4. Open a pull request targeting this repository's `deskbox-themes` branch.

主题加载格式尚未稳定，当前接受创意与文档投稿。请在 `themes/<主题标识>/` 提供主题说明、作者、开发状态、素材来源和许可，再向本仓库的 `deskbox-themes` 分支提交 PR。概念图须注明，实机截图请去除个人文件名、路径和账户信息。

## Required information / 必填信息

- Theme name and unique folder ID / 名称与唯一目录标识
- Author and source links / 作者和来源
- Concept, prototype, or tested pack status / 创意、原型或已测试主题包
- Exact host version or commit tested, if applicable / 如已测试，列出宿主版本或提交
- Windows version, display scaling, and light/dark coverage / 系统、缩放及明暗模式
- Source and license for every included third-party asset / 第三方素材来源及许可
- Known limitations / 已知限制

## Review / 审核

Maintainers review submissions before inclusion. Submission does not mean immediate publication. Future installable packs must use the supported data schema, without DLLs, scripts, arbitrary XAML, or remote code. Never submit local application data, credentials, build outputs, or installers as theme assets.

维护者审核后收录。后续可安装主题须遵循宿主支持的数据规范，不包含 DLL、脚本、任意 XAML 或远程代码。不要提交个人应用数据、凭据、构建输出或安装包。

## Licensing / 许可

Retain upstream notices. Contributions to application code follow the existing repository license. Identify asset licenses explicitly and only submit assets you have permission to redistribute. A submission does not transfer authorship or erase third-party license terms.

保留上游署名；应用代码贡献遵循仓库现有许可。素材应明确标注许可，仅提交有权再分发的内容，投稿不会转移作者身份或取消第三方许可条件。

