export function greet(name: string) {
    console.log(`Hello ${name}`);
}

// expose to global so cshtml inline script can call it
; (window as any).myApp = {
    greet
};