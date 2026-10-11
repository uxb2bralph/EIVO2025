// sidebar.ts — 側邊選單的多層摺疊。
//
// 取代原本的 metisMenu（BS3 時代外掛）。選單 .cshtml（Views/SiteAction/*Menu.cshtml）
// 的結構維持不變：`li > a` 後面接一個 `ul.nav-second-level` / `ul.nav-third-level`，
// 由本模組在執行期補上 BS5 Collapse 所需的 id / data 屬性與 aria 狀態。

import { bs } from './bootstrap';

let seq = 0;

function initSubMenu(submenu: HTMLElement): void {
    const toggler = submenu.previousElementSibling;
    if (!(toggler instanceof HTMLAnchorElement)) {
        return;
    }

    if (!submenu.id) {
        submenu.id = `eivo-submenu-${++seq}`;
    }
    submenu.classList.add('collapse');

    // 目前頁面落在此子選單內時預設展開
    const expanded = submenu.querySelector('a.active') !== null;
    if (expanded) {
        submenu.classList.add('show');
    }

    toggler.setAttribute('role', 'button');
    toggler.setAttribute('aria-controls', submenu.id);
    toggler.setAttribute('aria-expanded', String(expanded));

    // 原 metisMenu 的箭頭 <span class="fa arrow"> → 統一為可旋轉的 FA 圖示
    const arrow = toggler.querySelector('span.arrow, span.fa.arrow');
    if (arrow) {
        arrow.className = 'eivo-menu-arrow fa fa-angle-right';
        arrow.setAttribute('aria-hidden', 'true');
    }

    const collapse = bs().Collapse.getOrCreateInstance(submenu, { toggle: false });

    toggler.addEventListener('click', (evt) => {
        // 這類節點只負責展開，href 恆為 "#"
        evt.preventDefault();
        collapse.toggle();
    });

    submenu.addEventListener('show.bs.collapse', (evt) => {
        if (evt.target === submenu) {
            toggler.setAttribute('aria-expanded', 'true');
        }
    });
    submenu.addEventListener('hide.bs.collapse', (evt) => {
        if (evt.target === submenu) {
            toggler.setAttribute('aria-expanded', 'false');
        }
    });
}

/** 標記目前頁面對應的選單項目 */
function markActive(root: HTMLElement): void {
    const current = window.location.pathname.replace(/\/+$/, '').toLowerCase();
    if (!current) {
        return;
    }

    root.querySelectorAll('a[href]').forEach((a) => {
        const href = a.getAttribute('href');
        if (!href || href === '#') {
            return;
        }
        try {
            const target = new URL(href, window.location.href).pathname.replace(/\/+$/, '').toLowerCase();
            if (target === current) {
                a.classList.add('active');
            }
        } catch {
            // 忽略無法解析的 href（如 javascript:）
        }
    });
}

export function initSidebar(): void {
    const menu = document.getElementById('side-menu');
    if (!menu) {
        return;
    }

    markActive(menu);
    menu.querySelectorAll<HTMLElement>('ul.nav-second-level, ul.nav-third-level').forEach(initSubMenu);
}
