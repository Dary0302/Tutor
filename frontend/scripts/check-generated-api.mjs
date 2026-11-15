import { mkdtemp, readFile, writeFile, rm } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import openapiTS, { astToString, COMMENT_HEADER } from 'openapi-typescript';

const temporary = await mkdtemp(join(tmpdir(), 'kurator-api-'));
try {
  const generated =
    COMMENT_HEADER +
    astToString(
      await openapiTS(
        new URL('../openapi/backend.openapi.yaml', import.meta.url),
      ),
    );
  const file = join(temporary, 'schema.d.ts');
  await writeFile(file, generated);
  const committed = await readFile(
    new URL('../src/shared/api/generated/schema.d.ts', import.meta.url),
    'utf8',
  );
  if (
    committed.replaceAll('\r\n', '\n') !==
    (await readFile(file, 'utf8')).replaceAll('\r\n', '\n')
  ) {
    throw new Error(
      'Generated API types are stale. Run pnpm api:generate and review the diff.',
    );
  }
  console.log('Generated API types match the local snapshot.');
} finally {
  await rm(temporary, { recursive: true, force: true });
}
