import { readFile } from 'node:fs/promises';
import { parse } from 'yaml';
import {
  fetchContract,
  compareContract,
  showDifference,
  snapshot,
} from './api-contract.mjs';
const { document } = await fetchContract();
if (!(await compareContract(document))) {
  showDifference(parse(await readFile(snapshot, 'utf8')), document);
  throw new Error(
    'Backend contract differs. Review the diff and coordinate significant changes before updating. Snapshot was not modified.',
  );
}
console.log('Backend contract matches the local snapshot.');
