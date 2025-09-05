import { createI18n } from 'vue-i18n';
import zhCN from './locales/zh-CN';
import enUS from './locales/en-US';

// Get saved language or use browser language
const savedLang = localStorage.getItem('language');
const browserLang = navigator.language.toLowerCase();
const defaultLang = savedLang || (browserLang.includes('zh') ? 'zh-CN' : 'en-US');

const i18n = createI18n({
  legacy: false,
  locale: defaultLang,
  fallbackLocale: 'en-US',
  messages: {
    'zh-CN': zhCN,
    'en-US': enUS
  }
});

export default i18n;