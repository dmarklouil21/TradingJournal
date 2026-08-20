import { createRouter, createWebHistory } from 'vue-router'
import HeroView from '@/views/HeroView.vue'
import LoginView from '@/views/auth/LoginView.vue'
import RegisterView from '@/views/auth/RegisterView.vue'
import HomeView from '@/views/main/HomeView.vue'
import InvestingTrackerView from '@/views/main/InvestingTrackerView.vue'
import ActiveTradingView from '@/views/main/ActiveTradingView.vue'
import SettingsView from '@/views/main/SettingsView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'hero',
      component: HeroView,
      meta: { hideNav: true }
    },
    {
      path: '/auth/login',
      name: 'login',
      component: LoginView,
      meta: { hideNav: true }
    },
    {
      path: '/auth/register',
      name: 'register',
      component: RegisterView,
      meta: { hideNav: true }
    },
    {
      path: '/home',
      name: 'home',
      component: HomeView
    },
    {
      path: '/investing',
      name: 'investing',
      component: InvestingTrackerView
    },
    {
      path: '/trading',
      name: 'trading',
      component: ActiveTradingView
    },
    {
      path: '/settings',
      name: 'settings',
      component: SettingsView
    }
  ],
})

router.beforeEach((to, from, next) => {
  const isAuthenticated = !!localStorage.getItem('token');
  
  if (isAuthenticated && (to.name === 'login' || to.name === 'register' || to.name === 'hero')) {
    // Redirect authenticated users away from public pages
    next({ name: 'home' });
  } else if (!isAuthenticated && !to.meta.hideNav) {
    // Redirect unauthenticated users away from protected pages
    next({ name: 'login' });
  } else {
    // Proceed normally
    next();
  }
});

export default router
