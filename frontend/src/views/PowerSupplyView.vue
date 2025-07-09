<template>
  <div class="power-supply-view">
    <div class="view-header">
      <h1>可编程直流电源</h1>
      <p>高精度可编程直流电源，支持恒压恒流模式、多重保护和序列控制</p>
    </div>
    
    <div class="power-container">
      <PowerSupply />
    </div>
  </div>
</template>

<script setup lang="ts">
import PowerSupply from '@/components/instruments/PowerSupply.vue'
</script>

<style lang="scss" scoped>
.power-supply-view {
  padding: 20px;
  min-height: 100vh;
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
  
  .view-header {
    text-align: center;
    margin-bottom: 30px;
    
    h1 {
      font-size: 2.5rem;
      font-weight: 700;
      color: #2c3e50;
      margin-bottom: 10px;
      text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.1);
    }
    
    p {
      font-size: 1.1rem;
      color: #7f8c8d;
      max-width: 600px;
      margin: 0 auto;
      line-height: 1.6;
    }
  }
  
  .power-container {
    max-width: 1400px;
    margin: 0 auto;
    background: white;
    border-radius: 16px;
    box-shadow: 0 10px 30px rgba(0, 0, 0, 0.1);
    overflow: hidden;
  }
}

@media (max-width: 768px) {
  .power-supply-view {
    padding: 10px;
    
    .view-header {
      h1 {
        font-size: 2rem;
      }
      
      p {
        font-size: 1rem;
        padding: 0 20px;
      }
    }
  }
}
</style>
