// Validate the development API guard before Vite loads it. No generated output.
import { readFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import ts from 'typescript';

const sourcePath = fileURLToPath(new URL('./localApi.ts', import.meta.url));
const source = readFileSync(sourcePath, 'utf8');
const file = ts.createSourceFile(sourcePath, source, ts.ScriptTarget.Latest, true, ts.ScriptKind.TS);
const failures = [];

for (const diagnostic of file.parseDiagnostics) {
  const { line, character } = file.getLineAndCharacterOfPosition(diagnostic.start ?? 0);
  failures.push(`localApi.ts:${line + 1}:${character + 1} ${ts.flattenDiagnosticMessageText(diagnostic.messageText, '\n')}`);
}

// A syntactically valid copy/paste of the entire guard is still a serious mistake.
// Check the expected unique declarations independently of TypeScript parsing.
for (const declaration of [
  'publicApiGuard',
  'sessionApiGuard',
  'publicApiProxy',
  'sessionApiProxy',
]) {
  const occurrences = [...source.matchAll(new RegExp(`\\b(?:const|function)\\s+${declaration}\\b`, 'g'))].length;
  if (occurrences !== 1) failures.push(`Expected exactly one declaration of ${declaration}, found ${occurrences}.`);
}
const imports = [...source.matchAll(/import type \{ Plugin, ProxyOptions \} from 'vite';/g)].length;
if (imports !== 1) failures.push(`Expected one Vite type import, found ${imports}.`);

if (failures.length) {
  console.error('Development API proxy integrity check failed:\n' + failures.join('\n'));
  process.exitCode = 1;
} else {
  console.log('Development API proxy syntax and unique declarations verified.');
}
