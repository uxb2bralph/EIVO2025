// bootstrap.ts — 取用由 vendor/bootstrap/bootstrap.bundle.min.js 掛上的全域 bootstrap 物件。
// 刻意不從 node_modules import，避免 BS5 被重複打包進 main.js。

export interface BsCollapse {
    show(): void;
    hide(): void;
    toggle(): void;
}

export interface BsTab {
    show(): void;
}

export interface BootstrapGlobal {
    Collapse: {
        new(element: Element, options?: Record<string, unknown>): BsCollapse;
        getOrCreateInstance(element: Element, options?: Record<string, unknown>): BsCollapse;
    };
    Tab: {
        new(element: Element): BsTab;
        getOrCreateInstance(element: Element): BsTab;
    };
    Modal: new (element: Element, options?: Record<string, unknown>) => { show(): void; hide(): void };
    Popover: new (element: Element, options?: Record<string, unknown>) => unknown;
    Tooltip: new (element: Element, options?: Record<string, unknown>) => unknown;
    Offcanvas: { getOrCreateInstance(element: Element): { show(): void; hide(): void } };
}

export function bs(): BootstrapGlobal {
    const g = (window as unknown as { bootstrap?: BootstrapGlobal }).bootstrap;
    if (!g) {
        throw new Error('Bootstrap 5 bundle 未載入：請確認 vendor/bootstrap/bootstrap.bundle.min.js 在 main.js 之前載入。');
    }
    return g;
}
