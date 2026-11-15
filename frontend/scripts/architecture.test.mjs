import assert from 'node:assert/strict';
import { test } from 'node:test';
import { ESLint } from 'eslint';

const eslint = new ESLint();
const fixtures = [
  ['src/shared/lib/probe.ts', "import '@/features/search';", true],
  ['src/entities/user/probe.ts', "import '@/features/search';", true],
  ['src/features/search/probe.ts', "import '../profile';", true],
  ['src/features/search/probe.ts', "import('@/features/profile');", true],
  ['src/routes/home/probe.ts', "import '@/features/search/internal';", true],
  ['src/app/probe.ts', "import '@/test/mocks/server';", true],
  ['src/features/search/probe.ts', "import '@/entities/user';", false],
  ['src/features/search/probe.ts', "import '@/shared/ui';", false],
  ['src/features/search/probe.ts', "import './internal';", false],
  ['src/app/probe.ts', "import('../test/mocks/browser');", false],
  ['src/app/probe.ts', "import '@/shared/styles/global.css';", false],
  ['src/shared/lib/probe.ts', "import '@/features/search/style.css';", true],
];
for (const [filePath, code, forbidden] of fixtures) {
  test(`${forbidden ? 'rejects' : 'permits'} ${filePath}: ${code}`, async () => {
    const [result] = await eslint.lintText(code, { filePath });
    const violations = result.messages.filter(
      ({ ruleId }) => ruleId === 'architecture/boundaries',
    );
    assert.equal(
      violations.length > 0,
      forbidden,
      JSON.stringify(result.messages),
    );
  });
}
