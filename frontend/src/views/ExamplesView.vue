<template>
  <div class="examples-view">
    <div class="page-header">
      <h1 class="page-title">综合示例</h1>
      <p class="page-subtitle">完整的测量系统界面示例和最佳实践</p>
    </div>

    <div class="coming-soon">
      <el-icon class="icon"><Grid /></el-icon>
      <h2>正在开发中</h2>
      <p>综合示例正在开发中，敬请期待...</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { Grid } from '@element-plus/icons-vue'
</script>

<style lang="scss" scoped>
.examples-view {
  padding: 24px;
  max-width: 1400px;
  margin: 0 auto;
}

.page-header {
  text-align: center;
  margin-bottom: 48px;
  
  .page-title {
    font-size: 36px;
    font-weight: 700;
    color: var(--text-primary);
    margin-bottom: 12px;
  }
  
  .page-subtitle {
    font-size: 18px;
    color: var(--text-secondary);
    line-height: 1.6;
  }
}

.coming-soon {
  text-align: center;
  padding: 80px 20px;
  
  .icon {
    font-size: 64px;
    color: var(--primary-color);
    margin-bottom: 24px;
  }
  
  h2 {
    font-size: 24px;
    color: var(--text-primary);
    margin-bottom: 12px;
  }
  
  p {
    font-size: 16px;
    color: var(--text-secondary);
  }
}
</style>
