import resolve from '@rollup/plugin-node-resolve';
import commonJS from '@rollup/plugin-commonjs';
import json from '@rollup/plugin-json';
import babel from '@rollup/plugin-babel';
import typescript from '@rollup/plugin-typescript';
import pkg from './package.json';
import { optimizeLodashImports } from "@optimize-lodash/rollup-plugin";

const extensions = ['.mjs', '.js', '.ts', '.json'];

export default {
    input: pkg.source,
    external: ['nakama-runtime'],
    plugins: [
        // Allows node_modules resolution
        resolve({ extensions }),

        // Compile TypeScript
        typescript(),

        json(),

        // Resolve CommonJS modules
        commonJS({ extensions }),

        // Transpile to ES5
        babel({
            extensions,
            babelHelpers: 'bundled',
        }),

        optimizeLodashImports(),
    ],
    output: {
        file: pkg.main,
    },
};
