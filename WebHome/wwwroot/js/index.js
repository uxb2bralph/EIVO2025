export function greet(name) {
    console.log(`Hello ${name}`);
}
// expose to global so cshtml inline script can call it
;
window.myApp = {
    greet
};
//# sourceMappingURL=index.js.map