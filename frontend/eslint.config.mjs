import path from 'node:path';
import js from '@eslint/js';
import tseslint from 'typescript-eslint';
import hooks from 'eslint-plugin-react-hooks';

const root = path.resolve('src');
const allowed = {
  shared: ['shared'],
  entities: ['shared'],
  features: ['shared', 'entities'],
  routes: ['shared', 'entities', 'features', 'routes'],
  app: ['shared', 'entities', 'features', 'routes', 'app', 'test'],
  test: ['shared', 'entities', 'features', 'routes', 'app', 'test'],
};
const architecture = {
  meta: {
    type: 'problem',
    schema: [],
    messages: {
      boundary: 'Forbidden architecture import: {{source}}',
      public: 'Import another module through its public index.ts: {{source}}',
    },
  },
  create(context) {
    const filename = path.resolve(context.filename);
    const origin = path.relative(root, filename).split(path.sep);
    return {
      'ImportDeclaration, ExportNamedDeclaration, ExportAllDeclaration, ImportExpression'(
        node,
      ) {
        const source = node.source?.value;
        if (
          typeof source !== 'string' ||
          (!source.startsWith('.') && !source.startsWith('@/'))
        )
          return;
        const target = source.startsWith('@/')
          ? path.resolve(root, source.slice(2))
          : path.resolve(path.dirname(filename), source);
        const parts = path.relative(root, target).split(path.sep);
        if (!allowed[parts[0]]) return;
        const sameModule = origin[0] === parts[0] && origin[1] === parts[1];
        const sameInfrastructure =
          ['app', 'test'].includes(origin[0]) && origin[0] === parts[0];
        const testing = filename.includes('.test.');
        const invalidMockImport =
          origin[0] === 'app' &&
          parts[0] === 'test' &&
          parts.join('/') !== 'test/mocks/browser';
        if (
          !testing &&
          !sameModule &&
          !sameInfrastructure &&
          (invalidMockImport ||
            !allowed[origin[0]]?.includes(parts[0]) ||
            (origin[0] === parts[0] &&
              ['entities', 'features'].includes(origin[0])))
        ) {
          context.report({ node, messageId: 'boundary', data: { source } });
        }
        if (
          !testing &&
          !sameModule &&
          !sameInfrastructure &&
          !source.endsWith('.css') &&
          !['app', 'test'].includes(parts[0]) &&
          parts.length > 2 &&
          (parts.length > 3 || !/^index(?:\.[cm]?[jt]sx?)?$/.test(parts[2]))
        ) {
          context.report({ node, messageId: 'public', data: { source } });
        }
      },
    };
  },
};
export default tseslint.config(
  {
    ignores: [
      'node_modules/**',
      '.pnpm-store/**',
      'dist/**',
      'coverage/**',
      '.tools/**',
      'public/mockServiceWorker.js',
      'src/shared/api/generated/**',
      'test-results/**',
      'playwright-report/**',
    ],
  },
  js.configs.recommended,
  {
    files: ['**/*.mjs'],
    languageOptions: {
      globals: {
        process: 'readonly',
        console: 'readonly',
        URL: 'readonly',
        fetch: 'readonly',
        AbortSignal: 'readonly',
      },
    },
  },
  ...tseslint.configs.recommended,
  {
    files: ['src/**/*.{ts,tsx}'],
    plugins: {
      architecture: { rules: { boundaries: architecture } },
      'react-hooks': hooks,
    },
    rules: {
      'architecture/boundaries': 'error',
      'react-hooks/rules-of-hooks': 'error',
      'react-hooks/exhaustive-deps': 'error',
    },
  },
);
