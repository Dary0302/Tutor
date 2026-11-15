import { readFile, mkdtemp, writeFile, rm } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { isDeepStrictEqual } from 'node:util';
import { pathToFileURL } from 'node:url';
import { parse, stringify } from 'yaml';
import openapiTS from 'openapi-typescript';

export const snapshot = new URL(
  '../openapi/backend.openapi.yaml',
  import.meta.url,
);
export async function fetchContract() {
  const url = process.env.OPENAPI_SCHEMA_URL;
  if (!url)
    throw new Error(
      'Set OPENAPI_SCHEMA_URL to the real backend OpenAPI URL. Backend parity has NOT been checked.',
    );
  const response = await fetch(url, {
    signal: AbortSignal.timeout(30_000),
    headers: { Accept: 'application/json, application/yaml' },
  });
  if (!response.ok)
    throw new Error(`OpenAPI download failed: HTTP ${response.status}`);
  const source = await response.text();
  const document = parse(source);
  if (
    !document ||
    typeof document !== 'object' ||
    !/^3\.(0|1)\./.test(document.openapi ?? '') ||
    !document.info?.title ||
    !document.info?.version ||
    !document.paths ||
    typeof document.paths !== 'object'
  ) {
    throw new Error(
      'Expected a valid OpenAPI 3.0/3.1 document with info and paths.',
    );
  }
  if (document['x-placeholder'])
    throw new Error('Remote contract is a placeholder.');
  const temporary = await mkdtemp(join(tmpdir(), 'kurator-contract-'));
  try {
    await writeFile(join(temporary, 'schema.yaml'), source);
    // Validate references from the downloaded local document, not arbitrary remote refs.
    await openapiTS(pathToFileURL(join(temporary, 'schema.yaml')));
  } finally {
    await rm(temporary, { recursive: true, force: true });
  }
  return { document, source };
}
export async function compareContract(document) {
  const local = parse(await readFile(snapshot, 'utf8'));
  return isDeepStrictEqual(local, document);
}
export function showDifference(local, remote) {
  console.log('--- local snapshot\n' + stringify(local));
  console.log('+++ remote candidate\n' + stringify(remote));
}
