<template>
  <div class="container fade-in">
    <div class="hero">
      <h1 class="hero-title gradient-text">{{ t('hero.solutionGeneratorTitle') }}</h1>
      <p class="hero-subtitle">{{ t('hero.solutionGeneratorSubtitle') }}</p>
    </div>

    <div class="generator-card card">
      <div class="card-header">
        <h2>{{ t('generator.title') }}</h2>
        <p>{{ t('generator.description') }}</p>
      </div>

      <div class="form-container">
        <div class="form-group">
          <label for="prompt">{{ t('generator.requirements') }}</label>
          <textarea 
            id="prompt"
            v-model="prompt" 
            :placeholder="t('generator.requirementsPlaceholder')"
            :disabled="isLoading"
          ></textarea>
          <div class="char-count">{{ t('generator.charCount', { count: prompt.length }) }}</div>
        </div>

        <div class="form-row">
          <div class="form-group flex-1">
            <label for="model">{{ t('generator.aiModel') }}</label>
            <select id="model" v-model="selectedModel" :disabled="isLoading">
              <optgroup :label="t('generator.modelGroups.recommended')">
                <option value="ernie-speed-128k">{{ t('generator.models.speedRecommended') }}</option>
                <option value="ernie-4.5-turbo-128k">{{ t('generator.models.turboRecommended') }}</option>
              </optgroup>
              <optgroup :label="t('generator.modelGroups.other')">
                <option value="ernie-x1-turbo-32k">{{ t('generator.models.x1Turbo') }}</option>
                <option value="ernie-lite-8k">{{ t('generator.models.lite') }}</option>
              </optgroup>
            </select>
          </div>

          <div class="form-group">
            <label>&nbsp;</label>
            <button 
              @click="generateSolution" 
              :disabled="isLoading || !prompt.trim()" 
              class="btn-primary full-width"
            >
              <svg v-if="!isLoading" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M12 2v20M2 12h20M6.5 6.5l11 11M17.5 6.5l-11 11"></path>
              </svg>
              <span class="loading" v-if="isLoading"></span>
              {{ isLoading ? t('generator.generating') : t('generator.generateButton') }}
            </button>
          </div>
        </div>

        <div v-if="errorMessage" class="error-message fade-in">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <circle cx="12" cy="12" r="10"></circle>
            <line x1="12" y1="8" x2="12" y2="12"></line>
            <line x1="12" y1="16" x2="12.01" y2="16"></line>
          </svg>
          {{ errorMessage }}
        </div>

        <div v-if="successMessage" class="success-message fade-in">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path>
            <polyline points="22 4 12 14.01 9 11.01"></polyline>
          </svg>
          {{ successMessage }}
        </div>
      </div>
    </div>

    <div class="features">
      <h3>{{ t('features.title') }}</h3>
      <div class="features-grid">
        <div class="feature-card">
          <div class="feature-icon">
            <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="url(#gradient1)" stroke-width="2">
              <path d="M12 2L2 7v10c0 5.55 3.84 10.74 9 12 5.16-1.26 9-6.45 9-12V7l-10-5z"></path>
            </svg>
          </div>
          <h4>{{ t('features.misd.title') }}</h4>
          <p>{{ t('features.misd.description') }}</p>
        </div>
        
        <div class="feature-card">
          <div class="feature-icon">
            <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="url(#gradient2)" stroke-width="2">
              <path d="M13 2L3 14h9l-1 8 10-12h-9l1-8z"></path>
            </svg>
          </div>
          <h4>{{ t('features.jytek.title') }}</h4>
          <p>{{ t('features.jytek.description') }}</p>
        </div>
        
        <div class="feature-card">
          <div class="feature-icon">
            <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="url(#gradient3)" stroke-width="2">
              <circle cx="12" cy="12" r="10"></circle>
              <polyline points="12 6 12 12 16 14"></polyline>
            </svg>
          </div>
          <h4>{{ t('features.realtime.title') }}</h4>
          <p>{{ t('features.realtime.description') }}</p>
        </div>
        
        <div class="feature-card">
          <div class="feature-icon">
            <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="url(#gradient4)" stroke-width="2">
              <path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"></path>
              <polyline points="3.27 6.96 12 12.01 20.73 6.96"></polyline>
              <line x1="12" y1="22.08" x2="12" y2="12"></line>
            </svg>
          </div>
          <h4>{{ t('features.deploy.title') }}</h4>
          <p>{{ t('features.deploy.description') }}</p>
        </div>
      </div>
    </div>
  </div>

  <svg width="0" height="0">
    <defs>
      <linearGradient id="gradient1" x1="0%" y1="0%" x2="100%" y2="100%">
        <stop offset="0%" style="stop-color:#0066cc;stop-opacity:1" />
        <stop offset="100%" style="stop-color:#00c896;stop-opacity:1" />
      </linearGradient>
      <linearGradient id="gradient2" x1="0%" y1="0%" x2="100%" y2="100%">
        <stop offset="0%" style="stop-color:#ff6b35;stop-opacity:1" />
        <stop offset="100%" style="stop-color:#f7931e;stop-opacity:1" />
      </linearGradient>
      <linearGradient id="gradient3" x1="0%" y1="0%" x2="100%" y2="100%">
        <stop offset="0%" style="stop-color:#667eea;stop-opacity:1" />
        <stop offset="100%" style="stop-color:#764ba2;stop-opacity:1" />
      </linearGradient>
      <linearGradient id="gradient4" x1="0%" y1="0%" x2="100%" y2="100%">
        <stop offset="0%" style="stop-color:#00c896;stop-opacity:1" />
        <stop offset="100%" style="stop-color:#0066cc;stop-opacity:1" />
      </linearGradient>
    </defs>
  </svg>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const prompt = ref('');
const selectedModel = ref('ernie-speed-128k');
const isLoading = ref(false);
const errorMessage = ref('');
const successMessage = ref('');

const generateSolution = async () => {
  if (!prompt.value.trim()) {
    errorMessage.value = t('generator.errorEmpty');
    setTimeout(() => errorMessage.value = '', 3000);
    return;
  }

  isLoading.value = true;
  errorMessage.value = '';
  successMessage.value = '';

  try {
    const response = await fetch('/api/generation/generate-solution', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ 
        prompt: prompt.value, 
        model: selectedModel.value 
      }),
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(errorText || `HTTP error! status: ${response.status}`);
    }

    const blob = await response.blob();
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `GeneratedSolution_${new Date().getTime()}.zip`;
    document.body.appendChild(a);
    a.click();
    a.remove();
    window.URL.revokeObjectURL(url);
    
    successMessage.value = t('generator.successGenerated');
    setTimeout(() => successMessage.value = '', 5000);

  } catch (error) {
    console.error('Error generating solution:', error);
    errorMessage.value = error instanceof Error ? error.message : t('generator.errorGeneration');
    setTimeout(() => errorMessage.value = '', 5000);
  } finally {
    isLoading.value = false;
  }
};
</script>

<style scoped>
.generator-card {
  max-width: 800px;
  margin: 3rem auto;
  padding: 2.5rem;
}

.card-header {
  text-align: center;
  margin-bottom: 2rem;
}

.card-header h2 {
  margin-bottom: 0.5rem;
  font-size: 2rem;
}

.card-header p {
  font-size: 1.1rem;
}

.form-container {
  margin-top: 2rem;
}

.form-row {
  display: flex;
  gap: 1.5rem;
  align-items: flex-end;
}

.flex-1 {
  flex: 1;
}

.full-width {
  width: 100%;
}

textarea {
  min-height: 200px;
  font-family: 'Inter', monospace;
}

.char-count {
  text-align: right;
  font-size: 0.875rem;
  color: var(--light-text);
  margin-top: 0.25rem;
}

.error-message,
.success-message {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 1rem;
  border-radius: 8px;
  margin-top: 1.5rem;
  font-weight: 500;
}

.error-message {
  background: rgba(239, 68, 68, 0.1);
  color: #dc2626;
  border: 1px solid rgba(239, 68, 68, 0.2);
}

.success-message {
  background: rgba(34, 197, 94, 0.1);
  color: #16a34a;
  border: 1px solid rgba(34, 197, 94, 0.2);
}

.error-message svg,
.success-message svg {
  flex-shrink: 0;
}

.features {
  margin-top: 5rem;
  text-align: center;
}

.features h3 {
  font-size: 2rem;
  margin-bottom: 3rem;
}

.features-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 2rem;
  margin-top: 2rem;
}

.feature-card {
  background: white;
  padding: 2rem;
  border-radius: var(--border-radius);
  box-shadow: var(--shadow-sm);
  transition: var(--transition);
  border: 1px solid var(--border-color);
}

.feature-card:hover {
  transform: translateY(-4px);
  box-shadow: var(--shadow-md);
}

.feature-icon {
  width: 64px;
  height: 64px;
  margin: 0 auto 1.5rem;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--surface);
  border-radius: 50%;
}

.feature-card h4 {
  margin-bottom: 0.75rem;
  font-size: 1.25rem;
}

.feature-card p {
  font-size: 0.95rem;
  line-height: 1.6;
}

button svg {
  animation: rotate 2s linear infinite;
}

button:not(:disabled):hover svg {
  animation: none;
}

@keyframes rotate {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

@media (max-width: 768px) {
  .form-row {
    flex-direction: column;
  }
  
  .generator-card {
    padding: 1.5rem;
  }
  
  .features-grid {
    grid-template-columns: 1fr;
  }
}
</style>
