const translations = {
  zh: {
    navThemes: "主题", navInstall: "安装", navCreate: "创作",
    eyebrow: "开放主题实验室 · SCHEMA V1",
    heroTitle: "让桌面拥有<br><em>自己的天气。</em>",
    heroIntro: "不改动你的文件，只改变光、材质与开合节奏。每个主题都是可阅读、可分享、可移除的数据文件夹。",
    explore: "浏览首发主题", github: "在 GitHub 查看", factThemes: "首发主题", factCode: "主题内可执行代码", factSubmit: "开放投稿方式",
    mockSearch: "搜索文件、应用与灵感…", mockFooter: "SMOKE GLASS · 烟熏玻璃", stageNote: "真实 Acrylic 取样<br>＋应用内高光与边缘深度",
    collectionTitle: "首发收藏", collectionIntro: "两个方向，两种桌面气候。主题包只描述颜色、圆角、材质和动效参数，渲染始终由 DeskBox 掌控。",
    filterAll: "全部 02", filterDark: "深色 01", filterLight: "浅色 01",
    smokeName: "烟熏玻璃", smokeAlias: "Smoke Glass", smokeDescription: "深色半透明表面，青色与紫色玻璃边缘，适合高饱和壁纸和夜间桌面。", smokeTag1: "深层 Acrylic", smokeTag2: "彩色折射边缘", smokeTag3: "380ms 柔和开启",
    mistName: "雾凇", mistAlias: "Alpine Mist", mistDescription: "瓷白卡片、冰川蓝层次与漫射冬日光，适合克制、安静的工作桌面。", mistTag1: "柔和瓷白表面", mistTag2: "冰川蓝深层卡片", mistTag3: "340ms 轻盈开启",
    viewFiles: "查看主题文件", copyPath: "复制安装路径", copied: "安装路径已复制",
    installTitle: "三步换一种<br><em>桌面空气。</em>", step1Title: "下载收藏", step1Body: "下载 deskbox-themes 分支 ZIP，解压后选中一个主题文件夹。", step2Title: "放入主题目录", step2Body: "在“设置 › 外观”点击“打开主题文件夹”，复制完整主题文件夹。", step3Title: "扫描并切换", step3Body: "点击“重新扫描主题”，从同一页的预览列表中选择并立即切换。",
    download: "下载主题收藏", submit: "提交你的主题", compatibility: "目前主题需要支持 schema v1 的 DeskBox Themes 开发分支。官方 DeskBox 尚不能直接安装这些主题。", footer: "由社区维护，与 DeskBox 上游项目保持清晰署名与边界。"
  },
  en: {
    navThemes: "Themes", navInstall: "Install", navCreate: "Create",
    eyebrow: "OPEN THEME LAB · SCHEMA V1",
    heroTitle: "Give your desktop<br><em>its own weather.</em>",
    heroIntro: "Change the light, material, and rhythm without touching your files. Every theme is a readable, shareable, removable data folder.",
    explore: "Explore the first drop", github: "View on GitHub", factThemes: "launch themes", factCode: "executable theme code", factSubmit: "open submission route",
    mockSearch: "Search files, apps, and ideas…", mockFooter: "SMOKE GLASS · THEME 01", stageNote: "Native Acrylic sampling<br>＋ app-rendered highlights and depth",
    collectionTitle: "The first collection", collectionIntro: "Two directions, two desktop climates. Packs describe color, corners, material, and motion while DeskBox always owns rendering.",
    filterAll: "All 02", filterDark: "Dark 01", filterLight: "Light 01",
    smokeName: "Smoke Glass", smokeAlias: "烟熏玻璃", smokeDescription: "Dark translucent surfaces with cyan and violet glass edges, made for vivid wallpaper and night desktops.", smokeTag1: "Deep Acrylic", smokeTag2: "Chromatic edges", smokeTag3: "380ms soft entrance",
    mistName: "Alpine Mist", mistAlias: "雾凇", mistDescription: "Porcelain cards, glacier-blue depth, and diffuse winter light for a quiet, focused workspace.", mistTag1: "Porcelain surface", mistTag2: "Glacier depth", mistTag3: "340ms light entrance",
    viewFiles: "View theme files", copyPath: "Copy install path", copied: "Install path copied",
    installTitle: "Change the air<br><em>in three steps.</em>", step1Title: "Download the collection", step1Body: "Download the deskbox-themes branch ZIP, extract it, and choose a theme folder.", step2Title: "Add the theme folder", step2Body: "In Settings › Appearance, choose Open theme folder and copy the complete folder inside.", step3Title: "Scan and switch", step3Body: "Choose Reload themes, then select the new theme from the preview list on the same page.",
    download: "Download collection", submit: "Submit your theme", compatibility: "Themes currently require the schema-v1 DeskBox Themes development branch. Official DeskBox cannot install them yet.", footer: "Community maintained with clear attribution and boundaries from the upstream DeskBox project."
  }
};

const root = document.documentElement;
const languageButton = document.querySelector(".language-switch");
const toast = document.querySelector(".toast");
let toastTimer;

function applyLanguage(language) {
  const copy = translations[language];
  root.dataset.language = language;
  root.lang = language === "zh" ? "zh-CN" : "en";
  document.querySelectorAll("[data-i18n]").forEach((element) => {
    const value = copy[element.dataset.i18n];
    if (value) element.textContent = value;
  });
  document.querySelectorAll("[data-i18n-html]").forEach((element) => {
    const value = copy[element.dataset.i18nHtml];
    if (value) element.innerHTML = value;
  });
  languageButton.innerHTML = language === "zh"
    ? '<span class="language-active">中</span><span>EN</span>'
    : '<span>中</span><span class="language-active">EN</span>';
  localStorage.setItem("deskbox-market-language", language);
}

function showToast(message) {
  clearTimeout(toastTimer);
  toast.textContent = message;
  toast.classList.add("is-visible");
  toastTimer = setTimeout(() => toast.classList.remove("is-visible"), 1800);
}

languageButton.addEventListener("click", () => {
  applyLanguage(root.dataset.language === "zh" ? "en" : "zh");
});

document.querySelectorAll(".filter").forEach((button) => {
  button.addEventListener("click", () => {
    document.querySelectorAll(".filter").forEach((item) => item.classList.remove("is-active"));
    button.classList.add("is-active");
    const filter = button.dataset.filter;
    document.querySelectorAll(".theme-card").forEach((card) => {
      card.classList.toggle("is-hidden", filter !== "all" && card.dataset.tone !== filter);
    });
  });
});

document.querySelectorAll(".copy-path").forEach((button) => {
  button.addEventListener("click", async () => {
    try {
      await navigator.clipboard.writeText(button.dataset.copy);
      showToast(translations[root.dataset.language].copied);
    } catch {
      showToast(button.dataset.copy);
    }
  });
});

const savedLanguage = localStorage.getItem("deskbox-market-language");
const preferredLanguage = savedLanguage || (navigator.language.startsWith("zh") ? "zh" : "en");
applyLanguage(preferredLanguage);
