const readline = require('readline');
const { spawn } = require('child_process');
const path = require('path');

const rl = readline.createInterface({
  input: process.stdin,
  output: process.stdout
});

console.log('\x1b[36m%s\x1b[0m', 'Development Server Options:');
console.log('1. HTTP  (Port 3000)');
console.log('2. HTTPS (Port 3443)');

rl.question('\nEnter your choice (1 or 2): ', (answer) => {
  rl.close();
  
  const script = answer.trim() === '2' ? 'dev:https' : 'dev:http';
  const protocol = answer.trim() === '2' ? 'HTTPS' : 'HTTP';
  const port = answer.trim() === '2' ? '3443' : '3000';
  
  console.log(`\nStarting development server with ${protocol} on port ${port}...\n`);
  
  const npmCmd = process.platform === 'win32' ? 'npm.cmd' : 'npm';
  const child = spawn(npmCmd, ['run', script], { 
    stdio: 'inherit',
    shell: true,
    cwd: process.cwd()
  });
  
  child.on('error', (error) => {
    console.error(`Error: ${error.message}`);
    process.exit(1);
  });
  
  child.on('exit', (code) => {
    process.exit(code);
  });
}); 