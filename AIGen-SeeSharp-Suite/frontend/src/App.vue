<template>
  <div id="app">
    <nav class="nav">
      <div class="nav-container container">
        <div class="nav-brand">
          <h3 class="gradient-text" style="margin: 0;">JYTEK SeeSharp Suite</h3>
        </div>
        <div class="nav-links">
          <router-link to="/solution-generator" class="nav-link" active-class="active">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
              <polyline points="14 2 14 8 20 8"></polyline>
              <line x1="16" y1="13" x2="8" y2="13"></line>
              <line x1="16" y1="17" x2="8" y2="17"></line>
              <polyline points="10 9 9 9 8 9"></polyline>
            </svg>
            {{ t('nav.solutionGenerator') }}
          </router-link>
          <router-link to="/realtime-dashboard" class="nav-link" active-class="active">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M3 12h18m-9-9v18"></path>
              <rect x="3" y="3" width="18" height="18" rx="2" ry="2"></rect>
            </svg>
            {{ t('nav.realtimeDashboard') }}
          </router-link>
        </div>
        <div class="nav-actions">
          <LanguageToggle design="slide" />
        </div>
      </div>
    </nav>
    
    <main class="main-content">
      <router-view v-slot="{ Component }">
        <transition name="fade" mode="out-in">
          <component :is="Component" />
        </transition>
      </router-view>
    </main>
    
    <footer class="footer">
      <div class="container">
        <p>{{ t('footer.poweredBy') }}</p>
      </div>
    </footer>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import LanguageToggle from './components/LanguageToggle.vue';

const { t } = useI18n();
</script>

<style scoped>
.main-content {
  min-height: calc(100vh - 140px);
  padding: 2rem 0;
}

.footer {
  background: var(--surface);
  border-top: 1px solid var(--border-color);
  padding: 2rem 0;
  text-align: center;
  margin-top: auto;
}

.footer p {
  margin: 0;
  font-size: 0.875rem;
  color: var(--light-text);
}

.nav-brand {
  display: flex;
  align-items: center;
}

.nav-container {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.nav-links {
  display: flex;
  gap: 2rem;
  align-items: center;
  flex: 1;
  justify-content: center;
}

.nav-actions {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.nav-link {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
}

.nav-link svg {
  width: 20px;
  height: 20px;
  flex-shrink: 0;
}


.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

@media (max-width: 768px) {
  .nav-container {
    flex-wrap: wrap;
  }
  
  .nav-links {
    order: 2;
    width: 100%;
    justify-content: center;
    padding-top: 1rem;
  }
  
  .nav-actions {
    order: 1;
  }
}
</style>