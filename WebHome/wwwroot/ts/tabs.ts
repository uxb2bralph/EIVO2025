// tabs.ts — master tab 列（BS5 Tab）。
//
// BS5 移除了 jQuery 外掛介面（不再有 $(el).tab('show')），因此改以 bootstrap.Tab 實例操作。
// 對外 API 由 index.ts 掛在 window.EivoShell.tabs，供 CommonScriptInclude.cshtml 的 $global 轉呼叫。

import { bs } from './bootstrap';

/** 同時開啟的 tab 上限（含不可關閉的首頁 tab） */
const MAX_TAB_COUNT = 8;
/** tab 標題顯示上限，超過以省略號收斂，完整標題放 title 屬性 */
const MAX_TITLE_LENGTH = 12;

function tabList(): HTMLElement | null {
    return document.getElementById('masterTab');
}

function tabContentHost(): HTMLElement | null {
    return document.getElementById('masterTabContent');
}

/** 轉義屬性選擇器字串內的 \ 與 "（tabId 由呼叫端傳入，不保證乾淨） */
function escapeAttrValue(value: string): string {
    return value.replace(/\\/g, '\\\\').replace(/"/g, '\\"');
}

function linkOf(tabId: string): HTMLAnchorElement | null {
    const list = tabList();
    return list ? list.querySelector<HTMLAnchorElement>(`a[href="#${escapeAttrValue(tabId)}"]`) : null;
}

/** 顯示指定 tab；找不到時回傳 false */
export function showTab(tabId: string): boolean {
    const link = linkOf(tabId);
    if (!link) {
        return false;
    }
    bs().Tab.getOrCreateInstance(link).show();
    return true;
}

/** 顯示最後一個 tab（關閉目前 tab 後的遞補對象） */
function showLastTab(): void {
    const list = tabList();
    const links = list ? list.querySelectorAll<HTMLAnchorElement>('a.nav-link') : null;
    if (links && links.length > 0) {
        bs().Tab.getOrCreateInstance(links[links.length - 1]).show();
    }
}

export function removeTab(tabId: string): void {
    const link = linkOf(tabId);
    if (link) {
        link.closest('li')?.remove();
    }
    document.getElementById(tabId)?.remove();
    showLastTab();
}

/** 超過上限時關閉最舊的可關閉 tab（首頁 tab 不在其列） */
function enforceTabLimit(): void {
    const list = tabList();
    if (!list) {
        return;
    }
    let items = Array.from(list.querySelectorAll<HTMLElement>('li'));
    while (items.length >= MAX_TAB_COUNT) {
        const oldest = items.find((li) => li.querySelector('.closeTab') !== null);
        if (!oldest) {
            return; // 全部都是不可關閉的 tab
        }
        const href = oldest.querySelector('a')?.getAttribute('href') ?? '';
        removeTab(href.replace(/^#/, ''));
        items = Array.from(list.querySelectorAll<HTMLElement>('li'));
    }
}

/**
 * 建立（或重建）一個 tab，回傳其內容容器。
 * 內容填入由呼叫端負責（既有程式多為 jQuery 物件或 HTML 字串）。
 */
export function createTab(tabId: string, tabText: string, show = true): HTMLElement | null {
    const list = tabList();
    const host = tabContentHost();
    if (!list || !host) {
        return null;
    }

    removeTab(tabId);
    enforceTabLimit();

    const li = document.createElement('li');
    li.className = 'nav-item d-flex align-items-center';
    li.setAttribute('role', 'presentation');

    const link = document.createElement('a');
    link.className = 'nav-link tab-link';
    link.href = `#${tabId}`;
    link.setAttribute('role', 'tab');
    link.setAttribute('data-bs-toggle', 'tab');
    link.setAttribute('aria-controls', tabId);
    link.title = tabText;

    const text = document.createElement('span');
    text.className = 'eivo-tab-text';
    text.textContent = tabText.length > MAX_TITLE_LENGTH
        ? `${tabText.slice(0, MAX_TITLE_LENGTH)}…`
        : tabText;
    link.appendChild(text);

    const close = document.createElement('button');
    close.type = 'button';
    close.className = 'btn-close closeTab';
    close.setAttribute('aria-label', `關閉 ${tabText}`);
    close.addEventListener('click', (evt) => {
        evt.preventDefault();
        evt.stopPropagation();
        removeTab(tabId);
    });

    li.append(link, close);
    list.appendChild(li);

    const pane = document.createElement('div');
    pane.className = 'tab-pane fade';
    pane.id = tabId;
    pane.setAttribute('role', 'tabpanel');
    host.appendChild(pane);

    if (show) {
        showTab(tabId);
    }

    return pane;
}

export const tabs = { createTab, removeTab, showTab };
