import { rollup } from 'rollup';
const NUL = String.fromCharCode(0);

async function run(label, virtId) {
  const entryCode = `import h from 'virt'; console.log(h(1));`;
  const b = await rollup({
    input: 'e',
    plugins: [{
      name: 'v',
      resolveId(id) {
        if (id === 'e') return 'e';
        if (id === 'virt') return virtId;
        return null;
      },
      load(id) {
        if (id === 'e') return entryCode;
        if (id === virtId) return 'export default x => x';
        return null;
      }
    }]
  });
  const { output } = await b.generate({ format: 'es' });
  await b.close();
  console.log(label, 'OK chunks=', output.length);
}

try {
  await run('plain-id', 'virt');
} catch (e) { console.log('plain-id ERR', e.message); }

try {
  await run('NUL-id', NUL + 'virt');
  console.log('NUL test survived');
} catch (e) { console.log('NUL-id ERR', e.message); }
