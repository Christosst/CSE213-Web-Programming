const http = require('node:http');
const fs = require('node:fs/promises');
const path = require('node:path');
const root = path.resolve(__dirname, '..');
const types = { '.html': 'text/html', '.css': 'text/css', '.js': 'text/javascript', '.json': 'application/json', '.jpg': 'image/jpeg', '.svg': 'image/svg+xml', '.vtt': 'text/vtt', '.md': 'text/plain', '.txt': 'text/plain' };
const escape = value => value.replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;').replaceAll('"', '&quot;');

http.createServer(async (request, response) => {
  try {
    const url = new URL(request.url, 'http://localhost');
    if (request.method === 'POST' && url.pathname === '/form-echo') {
      let body = '';
      for await (const chunk of request) {
        body += chunk;
        if (body.length > 16384) { response.writeHead(413); response.end('Form too large'); return; }
      }
      const fields = new URLSearchParams(body);
      response.writeHead(200, { 'Content-Type': 'text/html; charset=utf-8' });
      response.end(`<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Form received</title><main><h1>Form received</h1><p>This classroom endpoint echoes data without saving it.</p><pre>${escape(JSON.stringify(Object.fromEntries(fields), null, 2))}</pre><a href="/">Back to demos</a></main></html>`);
      return;
    }
    if (!['GET', 'HEAD'].includes(request.method)) { response.writeHead(405); response.end('Method not allowed'); return; }
    const relative = decodeURIComponent(url.pathname).replace(/^\/+/, '');
    if (relative.split(/[\\/]/).some(part => part.startsWith('.') || ['node_modules', 'bin', 'obj', 'tests', 'tools', 'test-results', 'playwright-report'].includes(part)) || /\.(?:cs|csproj|db|bru|yml)$/i.test(relative)) {
      response.writeHead(403); response.end('Private project file'); return;
    }
    let file = path.resolve(root, relative || 'index.html');
    if (!file.startsWith(root + path.sep)) { response.writeHead(403); response.end('Forbidden'); return; }
    if ((await fs.stat(file)).isDirectory()) file = path.join(file, 'index.html');
    const data = await fs.readFile(file);
    response.writeHead(200, { 'Content-Type': `${types[path.extname(file)] || 'application/octet-stream'}${['.html','.css','.js','.json','.md','.txt','.vtt'].includes(path.extname(file)) ? '; charset=utf-8' : ''}`, 'Cache-Control': 'no-store' });
    response.end(request.method === 'HEAD' ? undefined : data);
  } catch (error) {
    response.writeHead(error instanceof URIError ? 400 : 404, { 'Content-Type': 'text/plain; charset=utf-8' });
    response.end(error instanceof URIError ? 'Invalid URL' : 'Not found');
  }
}).listen(3000, '127.0.0.1', () => console.log('CSE213 demos: http://localhost:3000'));
