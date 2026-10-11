// index.ts — 前端進入點，由 esbuild 打包為 wwwroot/js/main.js
//
// 對外只掛一個全域物件 EivoShell；其餘互動（$global、$inquiryAgent）仍在
// CommonScriptInclude.cshtml，待 P3.3 再逐步移入本目錄。

import { bs } from './bootstrap';
import { initSidebar } from './sidebar';
import { tabs } from './tabs';

/** 初始化宣告式元件（BS5 不再自動初始化 tooltip / popover） */
function initOverlays(): void {
    document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach((el) => {
        new (bs().Tooltip)(el);
    });
    document.querySelectorAll('[data-bs-toggle="popover"]').forEach((el) => {
        new (bs().Popover)(el);
    });
}

/**
 * BS4→BS5 相容層：把殘留的 `data-toggle` / `data-target` / `data-dismiss`
 * 補成 `data-bs-*`。
 *
 * 需要它的原因有二：
 *  1. Views/InvoiceBusiness/POSDevice/{DataItem,EditItem}.cshtml 目前受檔案 ACL 限制無法修改。
 *  2. 由後端動態產生、未經 Razor 遷移的 HTML 片段。
 *
 * 屬過渡措施：待上述來源清乾淨後即可移除（對應 spec P2 一致性清理）。
 */
const LEGACY_DATA_ATTRS = ['toggle', 'target', 'dismiss', 'placement', 'content'] as const;

function applyLegacyDataAttrCompat(root: ParentNode = document): void {
    for (const name of LEGACY_DATA_ATTRS) {
        root.querySelectorAll(`[data-${name}]:not([data-bs-${name}])`).forEach((el) => {
            const value = el.getAttribute(`data-${name}`);
            if (value !== null) {
                el.setAttribute(`data-bs-${name}`, value);
            }
        });
    }
}

function init(): void {
    applyLegacyDataAttrCompat();
    initSidebar();
    initOverlays();
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
} else {
    init();
}

const EivoShell = { tabs, initSidebar, initOverlays, applyLegacyDataAttrCompat };

(window as unknown as { EivoShell: typeof EivoShell }).EivoShell = EivoShell;

export { EivoShell };
