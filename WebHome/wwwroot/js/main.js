"use strict";
(() => {
  // wwwroot/ts/bootstrap.ts
  function bs() {
    const g = window.bootstrap;
    if (!g) {
      throw new Error("Bootstrap 5 bundle \u672A\u8F09\u5165\uFF1A\u8ACB\u78BA\u8A8D vendor/bootstrap/bootstrap.bundle.min.js \u5728 main.js \u4E4B\u524D\u8F09\u5165\u3002");
    }
    return g;
  }

  // wwwroot/ts/sidebar.ts
  var seq = 0;
  function initSubMenu(submenu) {
    const toggler = submenu.previousElementSibling;
    if (!(toggler instanceof HTMLAnchorElement)) {
      return;
    }
    if (!submenu.id) {
      submenu.id = `eivo-submenu-${++seq}`;
    }
    submenu.classList.add("collapse");
    const expanded = submenu.querySelector("a.active") !== null;
    if (expanded) {
      submenu.classList.add("show");
    }
    toggler.setAttribute("role", "button");
    toggler.setAttribute("aria-controls", submenu.id);
    toggler.setAttribute("aria-expanded", String(expanded));
    const arrow = toggler.querySelector("span.arrow, span.fa.arrow");
    if (arrow) {
      arrow.className = "eivo-menu-arrow fa fa-angle-right";
      arrow.setAttribute("aria-hidden", "true");
    }
    const collapse = bs().Collapse.getOrCreateInstance(submenu, { toggle: false });
    toggler.addEventListener("click", (evt) => {
      evt.preventDefault();
      collapse.toggle();
    });
    submenu.addEventListener("show.bs.collapse", (evt) => {
      if (evt.target === submenu) {
        toggler.setAttribute("aria-expanded", "true");
      }
    });
    submenu.addEventListener("hide.bs.collapse", (evt) => {
      if (evt.target === submenu) {
        toggler.setAttribute("aria-expanded", "false");
      }
    });
  }
  function markActive(root) {
    const current = window.location.pathname.replace(/\/+$/, "").toLowerCase();
    if (!current) {
      return;
    }
    root.querySelectorAll("a[href]").forEach((a) => {
      const href = a.getAttribute("href");
      if (!href || href === "#") {
        return;
      }
      try {
        const target = new URL(href, window.location.href).pathname.replace(/\/+$/, "").toLowerCase();
        if (target === current) {
          a.classList.add("active");
        }
      } catch {
      }
    });
  }
  function initSidebar() {
    const menu = document.getElementById("side-menu");
    if (!menu) {
      return;
    }
    markActive(menu);
    menu.querySelectorAll("ul.nav-second-level, ul.nav-third-level").forEach(initSubMenu);
  }

  // wwwroot/ts/tabs.ts
  var MAX_TAB_COUNT = 8;
  var MAX_TITLE_LENGTH = 12;
  function tabList() {
    return document.getElementById("masterTab");
  }
  function tabContentHost() {
    return document.getElementById("masterTabContent");
  }
  function escapeAttrValue(value) {
    return value.replace(/\\/g, "\\\\").replace(/"/g, '\\"');
  }
  function linkOf(tabId) {
    const list = tabList();
    return list ? list.querySelector(`a[href="#${escapeAttrValue(tabId)}"]`) : null;
  }
  function showTab(tabId) {
    const link = linkOf(tabId);
    if (!link) {
      return false;
    }
    bs().Tab.getOrCreateInstance(link).show();
    return true;
  }
  function showLastTab() {
    const list = tabList();
    const links = list ? list.querySelectorAll("a.nav-link") : null;
    if (links && links.length > 0) {
      bs().Tab.getOrCreateInstance(links[links.length - 1]).show();
    }
  }
  function removeTab(tabId) {
    const link = linkOf(tabId);
    if (link) {
      link.closest("li")?.remove();
    }
    document.getElementById(tabId)?.remove();
    showLastTab();
  }
  function enforceTabLimit() {
    const list = tabList();
    if (!list) {
      return;
    }
    let items = Array.from(list.querySelectorAll("li"));
    while (items.length >= MAX_TAB_COUNT) {
      const oldest = items.find((li) => li.querySelector(".closeTab") !== null);
      if (!oldest) {
        return;
      }
      const href = oldest.querySelector("a")?.getAttribute("href") ?? "";
      removeTab(href.replace(/^#/, ""));
      items = Array.from(list.querySelectorAll("li"));
    }
  }
  function createTab(tabId, tabText, show = true) {
    const list = tabList();
    const host = tabContentHost();
    if (!list || !host) {
      return null;
    }
    removeTab(tabId);
    enforceTabLimit();
    const li = document.createElement("li");
    li.className = "nav-item d-flex align-items-center";
    li.setAttribute("role", "presentation");
    const link = document.createElement("a");
    link.className = "nav-link tab-link";
    link.href = `#${tabId}`;
    link.setAttribute("role", "tab");
    link.setAttribute("data-bs-toggle", "tab");
    link.setAttribute("aria-controls", tabId);
    link.title = tabText;
    const text = document.createElement("span");
    text.className = "eivo-tab-text";
    text.textContent = tabText.length > MAX_TITLE_LENGTH ? `${tabText.slice(0, MAX_TITLE_LENGTH)}\u2026` : tabText;
    link.appendChild(text);
    const close = document.createElement("button");
    close.type = "button";
    close.className = "btn-close closeTab";
    close.setAttribute("aria-label", `\u95DC\u9589 ${tabText}`);
    close.addEventListener("click", (evt) => {
      evt.preventDefault();
      evt.stopPropagation();
      removeTab(tabId);
    });
    li.append(link, close);
    list.appendChild(li);
    const pane = document.createElement("div");
    pane.className = "tab-pane fade";
    pane.id = tabId;
    pane.setAttribute("role", "tabpanel");
    host.appendChild(pane);
    if (show) {
      showTab(tabId);
    }
    return pane;
  }
  var tabs = { createTab, removeTab, showTab };

  // wwwroot/ts/index.ts
  function initOverlays() {
    document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach((el) => {
      new (bs()).Tooltip(el);
    });
    document.querySelectorAll('[data-bs-toggle="popover"]').forEach((el) => {
      new (bs()).Popover(el);
    });
  }
  var LEGACY_DATA_ATTRS = ["toggle", "target", "dismiss", "placement", "content"];
  function applyLegacyDataAttrCompat(root = document) {
    for (const name of LEGACY_DATA_ATTRS) {
      root.querySelectorAll(`[data-${name}]:not([data-bs-${name}])`).forEach((el) => {
        const value = el.getAttribute(`data-${name}`);
        if (value !== null) {
          el.setAttribute(`data-bs-${name}`, value);
        }
      });
    }
  }
  function init() {
    applyLegacyDataAttrCompat();
    initSidebar();
    initOverlays();
  }
  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", init);
  } else {
    init();
  }
  var EivoShell = { tabs, initSidebar, initOverlays, applyLegacyDataAttrCompat };
  window.EivoShell = EivoShell;
})();
//# sourceMappingURL=main.js.map
