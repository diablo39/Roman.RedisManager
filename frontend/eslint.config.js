import vuetify from 'eslint-config-vuetify'

export default [
  ...vuetify(),
  {
    files: ['*.vue'],
    rules: {
      'vue/script-indent': ['error', 2, { baseIndent: 1 }],
      'indent': 'off'
    }
  }
]
