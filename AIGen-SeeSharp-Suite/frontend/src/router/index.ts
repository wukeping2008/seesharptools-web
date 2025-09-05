import { createRouter, createWebHistory } from 'vue-router';
import SolutionGenerator from '../views/SolutionGenerator.vue';
import RealtimeDashboard from '../views/RealtimeDashboard.vue';
import RealtimeHardwareDashboard from '../views/RealtimeHardwareDashboard.vue';

const routes = [
  {
    path: '/',
    redirect: '/solution-generator'
  },
  {
    path: '/solution-generator',
    name: 'SolutionGenerator',
    component: SolutionGenerator
  },
  {
    path: '/realtime-dashboard',
    name: 'RealtimeDashboard',
    component: RealtimeHardwareDashboard  // Use hardware dashboard instead
  }
];

const router = createRouter({
  history: createWebHistory(),
  routes
});

export default router;
