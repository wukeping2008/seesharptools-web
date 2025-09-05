<template>
  <div class="language-toggle-container">
    <!-- Design 1: Sliding Toggle -->
    <button 
      v-if="design === 'slide'"
      @click="toggleLanguage" 
      class="lang-toggle slide-design"
      :aria-label="`Switch to ${locale === 'zh-CN' ? 'English' : '中文'}`"
    >
      <span class="lang-option" :class="{ 'active': locale === 'zh-CN' }">中文</span>
      <span class="lang-option" :class="{ 'active': locale === 'en-US' }">EN</span>
      <span class="toggle-slider" :class="{ 'slide-right': locale === 'en-US' }"></span>
    </button>

    <!-- Design 2: Simple Text Toggle -->
    <button 
      v-else-if="design === 'text'"
      @click="toggleLanguage" 
      class="lang-toggle text-design"
    >
      <span class="lang-current">{{ locale === 'zh-CN' ? '中文' : 'EN' }}</span>
      <span class="lang-separator">|</span>
      <span class="lang-next">{{ locale === 'zh-CN' ? 'EN' : '中文' }}</span>
    </button>

    <!-- Design 3: Minimal Icon + Text -->
    <button 
      v-else-if="design === 'minimal'"
      @click="toggleLanguage" 
      class="lang-toggle minimal-design"
    >
      <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
        <circle cx="12" cy="12" r="10"></circle>
        <path d="M2 12h20M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z"></path>
      </svg>
      <span>{{ locale === 'zh-CN' ? '中文' : 'English' }}</span>
    </button>

    <!-- Design 4: Compact Badge -->
    <button 
      v-else
      @click="toggleLanguage" 
      class="lang-toggle badge-design"
    >
      {{ locale === 'zh-CN' ? '中' : 'A' }}
    </button>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';

interface Props {
  design?: 'slide' | 'text' | 'minimal' | 'badge'
}

const props = withDefaults(defineProps<Props>(), {
  design: 'slide'
});

const { locale } = useI18n();

const toggleLanguage = () => {
  const newLocale = locale.value === 'zh-CN' ? 'en-US' : 'zh-CN';
  locale.value = newLocale;
  localStorage.setItem('language', newLocale);
};
</script>

<style scoped>
.language-toggle-container {
  display: inline-flex;
}

.lang-toggle {
  border: none;
  cursor: pointer;
  font-weight: 600;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  justify-content: center;
}

/* Design 1: Sliding Toggle */
.slide-design {
  position: relative;
  padding: 2px;
  background: white;
  border: 2px solid var(--border-color);
  border-radius: 100px;
  min-width: 100px;
  height: 36px;
}

.slide-design:hover {
  border-color: var(--primary-color);
  box-shadow: 0 2px 8px rgba(0, 102, 204, 0.15);
}

.slide-design .lang-option {
  position: relative;
  z-index: 2;
  flex: 1;
  padding: 0 1rem;
  color: var(--light-text);
  transition: color 0.3s ease;
  font-size: 0.875rem;
}

.slide-design .lang-option.active {
  color: white;
}

.slide-design .toggle-slider {
  position: absolute;
  top: 2px;
  left: 2px;
  bottom: 2px;
  width: calc(50% - 2px);
  background: linear-gradient(135deg, var(--primary-color), #0052a3);
  border-radius: 100px;
  transition: transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  z-index: 1;
}

.slide-design .toggle-slider.slide-right {
  transform: translateX(100%);
}

/* Design 2: Simple Text Toggle */
.text-design {
  padding: 0.5rem 1rem;
  background: transparent;
  color: var(--text-color);
  font-size: 0.9rem;
  gap: 0.5rem;
}

.text-design:hover .lang-next {
  color: var(--primary-color);
  text-decoration: underline;
}

.text-design .lang-current {
  font-weight: 700;
  color: var(--primary-color);
}

.text-design .lang-separator {
  color: var(--border-color);
  font-weight: 300;
}

.text-design .lang-next {
  opacity: 0.7;
  transition: all 0.2s ease;
}

/* Design 3: Minimal Icon + Text */
.minimal-design {
  padding: 0.5rem 1rem;
  background: white;
  border: 1px solid var(--border-color);
  border-radius: 8px;
  color: var(--text-color);
  gap: 0.5rem;
  font-size: 0.9rem;
}

.minimal-design:hover {
  background: var(--surface);
  border-color: var(--primary-color);
  color: var(--primary-color);
}

.minimal-design svg {
  width: 16px;
  height: 16px;
}

/* Design 4: Compact Badge */
.badge-design {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background: var(--primary-color);
  color: white;
  font-size: 1rem;
  font-weight: 700;
  padding: 0;
}

.badge-design:hover {
  transform: rotate(180deg);
  background: linear-gradient(135deg, var(--primary-color), #0052a3);
}

/* Responsive adjustments */
@media (max-width: 768px) {
  .slide-design {
    min-width: 80px;
    height: 32px;
  }

  .slide-design .lang-option {
    font-size: 0.8rem;
    padding: 0 0.75rem;
  }
}
</style>