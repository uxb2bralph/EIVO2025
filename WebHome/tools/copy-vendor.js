// copy-vendor.js — 將 node_modules 的前端相依複製到 wwwroot/vendor/
// 資產必須隨 repo 版控並由 dotnet publish 一併輸出，故不在執行期依賴 node_modules。

const fs = require('fs');
const path = require('path');

const root = path.resolve(__dirname, '..');
const nm = path.join(root, 'node_modules');
const vendor = path.join(root, 'wwwroot', 'vendor');

/** 要複製的項目：[來源（相對 node_modules）, 目的（相對 wwwroot/vendor）] */
const items = [
  // Bootstrap 5.3 — CSS 由 SCSS 管線編譯，此處只需 JS bundle（含 Popper）
  ['bootstrap/dist/js/bootstrap.bundle.min.js', 'bootstrap/bootstrap.bundle.min.js'],
  ['bootstrap/dist/js/bootstrap.bundle.min.js.map', 'bootstrap/bootstrap.bundle.min.js.map'],

  // Font Awesome 6 Free（自架，取代原 maxcdn FA 4.2.0）
  // v4-shims 讓既有 `fa fa-xxx` 舊名稱在 FA6 下仍可顯示（FA4→6 改名屬 P2.1）
  ['@fortawesome/fontawesome-free/css/all.min.css', 'fontawesome/css/all.min.css'],
  ['@fortawesome/fontawesome-free/css/v4-shims.min.css', 'fontawesome/css/v4-shims.min.css'],
  ['@fortawesome/fontawesome-free/webfonts', 'fontawesome/webfonts'],

  // jQuery UI 1.13 CSS — 與既有 Scripts/jquery-ui-1.13.2.js 版本對齊
  ['jquery-ui-dist/jquery-ui.min.css', 'jquery-ui/jquery-ui.min.css'],
  ['jquery-ui-dist/jquery-ui.structure.min.css', 'jquery-ui/jquery-ui.structure.min.css'],
  ['jquery-ui-dist/jquery-ui.theme.min.css', 'jquery-ui/jquery-ui.theme.min.css'],
  ['jquery-ui-dist/images', 'jquery-ui/images'],
];

let copied = 0;

for (const [from, to] of items) {
  const src = path.join(nm, from);
  const dest = path.join(vendor, to);

  if (!fs.existsSync(src)) {
    console.error(`[copy-vendor] 找不到來源，請先 npm install：${from}`);
    process.exitCode = 1;
    continue;
  }

  fs.mkdirSync(path.dirname(dest), { recursive: true });
  fs.cpSync(src, dest, { recursive: true });
  copied++;
}

console.log(`[copy-vendor] 已複製 ${copied}/${items.length} 個項目至 wwwroot/vendor/`);
