import vuetify from 'eslint-config-vuetify'

const vuetifyConfig = await vuetify()

export default [
  {
    ignores: ['node_modules/**', 'dist/**', 'build/**', 'coverage/**', '*.min.js'],
  },
  ...vuetifyConfig,
  {
    files: ['*.vue'],
    rules: {
      'vue/script-indent': ['error', 2, { baseIndent: 1 }],
      'indent': 'off',
    },
  },
]
