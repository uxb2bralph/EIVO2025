/* ============================================================================
   EIVO Layout Script
   - Restructures the legacy #side-menu (Bootstrap/SB-Admin) into the Vuetify
     "vertical nav" structure used by the InsightFitPro sample.
   - Wires up: sidebar collapse (pin/unpin), nav-group accordion, mobile
     overlay, and dark/light theme toggle.
   Loaded AFTER jQuery (uses $ if present, otherwise vanilla fallback).
   ========================================================================== */
(function () {
  'use strict';

  var $ = (typeof window.jQuery !== 'undefined') ? window.jQuery : null;

  /* Map legacy Font Awesome icons -> Tabler icons (best effort). */
  var ICON_MAP = {
    'fa-hand-o-right': 'tabler-circle',
    'fa-angle-double-right': 'tabler-list',
    'fa-cog': 'tabler-settings',
    'fa-gear': 'tabler-settings',
    'fa-user': 'tabler-user',
    'fa-users': 'tabler-users',
    'fa-file': 'tabler-file',
    'fa-files-o': 'tabler-files',
    'fa-search': 'tabler-search',
    'fa-bar-chart': 'tabler-chart-bar',
    'fa-bar-chart-o': 'tabler-chart-bar',
    'fa-money': 'tabler-cash',
    'fa-dollar': 'tabler-currency-dollar',
    'fa-print': 'tabler-printer',
    'fa-download': 'tabler-download',
    'fa-upload': 'tabler-upload',
    'fa-envelope': 'tabler-mail',
    'fa-bell': 'tabler-bell',
    'fa-calendar': 'tabler-calendar',
    'fa-list': 'tabler-list',
    'fa-th': 'tabler-layout-grid',
    'fa-dashboard': 'tabler-layout-dashboard',
    'fa-home': 'tabler-home',
    'fa-shield': 'tabler-shield',
    'fa-key': 'tabler-key',
    'fa-lock': 'tabler-lock',
    'fa-unlock': 'tabler-lock-open',
    'fa-wrench': 'tabler-tool',
    'fa-cubes': 'tabler-cube',
    'fa-database': 'tabler-database',
    'fa-truck': 'tabler-truck',
    'fa-building': 'tabler-building',
    'fa-inbox': 'tabler-inbox',
    'fa-tasks': 'tabler-list-check',
    'fa-check-square': 'tabler-checkbox',
    'fa-credit-card': 'tabler-credit-card',
    'fa-receipt': 'tabler-receipt',
    'fa-qrcode': 'tabler-qrcode',
    'fa-mobile': 'tabler-device-mobile',
    'fa-desktop': 'tabler-device-desktop',
    'fa-language': 'tabler-language',
    'fa-globe': 'tabler-globe',
    'fa-flag': 'tabler-flag',
    'fa-star': 'tabler-star',
    'fa-heart': 'tabler-heart',
    'fa-bolt': 'tabler-bolt',
    'fa-fire': 'tabler-fire',
    'fa-rocket': 'tabler-rocket',
    'fa-award': 'tabler-award',
    'fa-trophy': 'tabler-trophy',
    'fa-chart-pie': 'tabler-chart-pie',
    'fa-chart-line': 'tabler-chart-line',
    'fa-line-chart': 'tabler-chart-line',
    'fa-pie-chart': 'tabler-chart-pie',
    'fa-area-chart': 'tabler-chart-area'
  };

  function iconFromLegacy(el) {
    if (!el) return 'tabler-circle';
    var fa = el.querySelector('i.fa, i[class*="fa-"]');
    if (fa) {
      var cls = (fa.getAttribute('class') || '').split(/\s+/);
      for (var i = 0; i < cls.length; i++) {
        if (ICON_MAP[cls[i]]) return ICON_MAP[cls[i]];
      }
    }
    return 'tabler-circle';
  }

  function textOf(el) {
    // Clone and strip icons so we get clean label text.
    var clone = el.cloneNode(true);
    var icons = clone.querySelectorAll('i, span.fa, .fa');
    for (var i = 0; i < icons.length; i++) icons[i].remove();
    return (clone.textContent || '').replace(/\s+/g, ' ').trim();
  }

  function currentPath() {
    return (window.location.pathname || '').toLowerCase();
  }

  function hrefMatches(href) {
    if (!href) return false;
    if (href === '#' || href === '' || href.charAt(0) === 'j') return false;
    var path = href.split('?')[0].split('#')[0].toLowerCase();
    if (!path) return false;
    // Normalise leading ../ and ./
    path = path.replace(/^\.\.\//, '').replace(/^\.\//, '');
    var cur = currentPath().replace(/^\//, '');
    return cur === path || cur.indexOf(path) !== -1 || path.indexOf(cur) !== -1;
  }

  /* Build a leaf <li class="nav-link"> from a legacy <li>. */
  function buildLeafLi(li) {
    var a = li.querySelector('a');
    if (!a) return null;
    var href = a.getAttribute('href') || '#';
    var label = textOf(a) || a.textContent.trim();
    var icon = iconFromLegacy(a);
    var active = hrefMatches(href) ? ' active' : '';

    var liEl = document.createElement('li');
    liEl.className = 'nav-link';
    liEl.innerHTML =
      '<a href="' + href + '"' + (active ? ' class="active"' : '') + '>' +
        '<i class="' + icon + ' nav-item-icon" aria-hidden="true"></i>' +
        '<span class="nav-item-title"></span>' +
      '</a>';
    liEl.querySelector('.nav-item-title').textContent = label;
    return liEl;
  }

  /* Build a group <li class="nav-group"> from a legacy <li> that has children. */
  function buildGroupLi(li) {
    var a = li.querySelector(':scope > a');
    var childUl = li.querySelector(':scope > ul');
    if (!a) return null;
    var label = textOf(a) || a.textContent.trim();
    var icon = iconFromLegacy(a);

    var group = document.createElement('li');
    group.className = 'nav-group';

    var labelEl = document.createElement('div');
    labelEl.className = 'nav-group-label';
    labelEl.innerHTML =
      '<i class="' + icon + ' nav-item-icon" aria-hidden="true"></i>' +
      '<span class="nav-item-title"></span>' +
      '<i class="tabler-chevron-right nav-group-arrow" aria-hidden="true"></i>';
    labelEl.querySelector('.nav-item-title').textContent = label;
    group.appendChild(labelEl);

    var children = document.createElement('ul');
    children.className = 'nav-group-children';
    if (childUl) {
      var childLis = childUl.querySelectorAll(':scope > li');
      for (var i = 0; i < childLis.length; i++) {
        var leaf = buildLeafLi(childLis[i]);
        if (leaf) children.appendChild(leaf);
      }
    }
    group.appendChild(children);
    return group;
  }

  /* Build a standalone top-level link (no children) as a leaf group. */
  function buildLeafGroupLi(li) {
    var a = li.querySelector(':scope > a');
    if (!a) return null;
    var href = a.getAttribute('href') || '#';
    var label = textOf(a) || a.textContent.trim();
    var icon = iconFromLegacy(a);
    var active = hrefMatches(href) ? ' active' : '';

    var group = document.createElement('li');
    group.className = 'nav-group nav-leaf';
    var labelEl = document.createElement('a');
    labelEl.className = 'nav-group-label' + (active ? ' active' : '');
    labelEl.setAttribute('href', href);
    labelEl.innerHTML =
      '<i class="' + icon + ' nav-item-icon" aria-hidden="true"></i>' +
      '<span class="nav-item-title"></span>';
    labelEl.querySelector('.nav-item-title').textContent = label;
    group.appendChild(labelEl);
    return group;
  }

  function enhanceMenu() {
    var sideMenu = document.getElementById('side-menu');
    if (!sideMenu) return;

    var navItems = document.createElement('ul');
    navItems.className = 'nav-items';

    var topLis = sideMenu.querySelectorAll(':scope > li');
    var hasActive = false;

    for (var i = 0; i < topLis.length; i++) {
      var li = topLis[i];
      var hasChildren = !!li.querySelector(':scope > ul');
      var node = hasChildren ? buildGroupLi(li) : buildLeafGroupLi(li);
      if (node) {
        navItems.appendChild(node);
        if (node.querySelector('a.active')) hasActive = true;
      }
    }

    // Replace the legacy menu with the new structure.
    sideMenu.parentNode.replaceChild(navItems, sideMenu);

    // Open the group that contains the active link (or first group by default).
    var activeGroup = navItems.querySelector('.nav-group:has(a.active)');
    if (activeGroup) {
      activeGroup.classList.add('open', 'active');
    } else if (!hasActive) {
      var first = navItems.querySelector('.nav-group:not(.nav-leaf)');
      if (first) first.classList.add('open');
    }

    // Accordion behaviour for group labels.
    navItems.addEventListener('click', function (e) {
      var label = e.target.closest('.nav-group-label');
      if (!label) return;
      var group = label.closest('.nav-group');
      if (!group) return;
      // Leaf groups navigate normally.
      if (group.classList.contains('nav-leaf')) return;
      e.preventDefault();
      group.classList.toggle('open');
    });
  }

  function setupSidebar() {
    var wrapper = document.querySelector('.layout-wrapper');
    var nav = document.querySelector('.layout-vertical-nav');
    var overlay = document.querySelector('.layout-overlay');
    if (!wrapper || !nav) return;

    // Pin / unpin (collapse) — desktop.
    var pinBtn = document.querySelector('.nav-pin');
    if (pinBtn) {
      pinBtn.addEventListener('click', function () {
        wrapper.classList.toggle('layout-vertical-nav-collapsed');
      });
    }

    // Mobile hamburger.
    var toggleBtn = document.getElementById('vertical-nav-toggle-btn');
    function closeMobile() {
      nav.classList.remove('visible');
      if (overlay) overlay.classList.remove('visible');
    }
    if (toggleBtn) {
      toggleBtn.addEventListener('click', function () {
        nav.classList.toggle('visible');
        if (overlay) overlay.classList.toggle('visible');
      });
    }
    if (overlay) {
      overlay.addEventListener('click', closeMobile);
    }
    // Close mobile drawer when a link is clicked.
    var navItems = document.querySelector('.nav-items');
    if (navItems) {
      navItems.addEventListener('click', function (e) {
        if (e.target.closest('a') && window.innerWidth < 1280) closeMobile();
      });
    }
  }

  function setupTheme() {
    var btn = document.querySelector('[data-theme-toggle]');
    if (!btn) return;
    var root = document.documentElement;
    var KEY = 'eivo-theme';

    function apply(theme) {
      if (theme === 'light') {
        root.classList.add('skin--light');
        root.classList.remove('skin--dark');
      } else {
        root.classList.add('skin--dark');
        root.classList.remove('skin--light');
      }
      try { localStorage.setItem(KEY, theme); } catch (e) { /* ignore */ }
      var icon = btn.querySelector('.v-icon, i');
      if (icon) icon.className = icon.className.replace(/tabler-(moon-stars|sun)/, theme === 'light' ? 'tabler-sun' : 'tabler-moon-stars');
    }

    var saved = 'dark';
    try { saved = localStorage.getItem(KEY) || 'dark'; } catch (e) { /* ignore */ }
    apply(saved);

    btn.addEventListener('click', function () {
      apply(root.classList.contains('skin--light') ? 'dark' : 'light');
    });
  }

  function init() {
    enhanceMenu();
    setupSidebar();
    setupTheme();
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }
})();
