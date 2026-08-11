import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import HeroView from '@/views/HeroView.vue'
import LoginView from '@/views/auth/LoginView.vue'
import RegisterView from '@/views/auth/RegisterView.vue'

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
    }
  ],
})

export default router
