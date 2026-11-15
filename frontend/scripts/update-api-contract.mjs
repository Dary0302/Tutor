import { readFile, writeFile } from 'node:fs/promises';
import { parse } from 'yaml';
import openapiTS, { astToString, COMMENT_HEADER } from 'openapi-typescript';
import { fetchContract, showDifference, snapshot } from './api-contract.mjs';
const { document, source } = await fetchContract();
showDifference(parse(await readFile(snapshot, 'utf8')), document);
if (!process.argv.includes('--apply')) {
  console.log(
    'Preview only. Review this diff; then explicitly run pnpm api:contract:update --apply to update the snapshot and types.',
  );
} else {
  const generated = COMMENT_HEADER + astToString(await openapiTS(document));
  await writeFile(snapshot, source);
  await writeFile(
    new URL('../src/shared/api/generated/schema.d.ts', import.meta.url),
    generated,
  );
  console.log(
    'Updated snapshot and generated types. Review the working-tree diff.',
  );
}
