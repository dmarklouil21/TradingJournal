<script setup>
import { ref } from 'vue';
import { RouterLink, useRouter } from 'vue-router';
import axios from 'axios';
import { login } from '@/services/auth';

const router = useRouter();
const email = ref('');
const password = ref('');
const isLoading = ref(false);

const errorMessage = ref('');

const handleLogin = async (e) => {
  e.preventDefault();
  isLoading.value = true;
  errorMessage.value = '';

  try {
    const credentials = {
      email: email.value,
      password: password.value
    };

    const res = await login(credentials);
    
    if(res.status == 200) {
      localStorage.setItem('token', res.data.token);
      router.push('/home');
    }
  }
  catch(ex) {
    if(ex.response && ex.response.data) {
      if(ex.response.data.error) {
        errorMessage.value = ex.response.data.error;
      } else if (ex.response.data.errors) {
        errorMessage.value = Object.values(ex.response.data.errors).flat().join(', ');
      } else {
        errorMessage.value = "Login failed. Please check your credentials.";
      }
    } else {
      errorMessage.value = "An unexpected error occurred. Please try again.";
    }
    console.error(ex);
  }
  finally{
    isLoading.value = false;
  }
};
</script>

<template>
  <div class="min-h-screen flex items-center justify-center bg-bg-gray relative overflow-hidden p-4">
    <!-- Decorative background elements -->
    <div class="absolute top-[-20%] right-[-10%] w-[50%] h-[50%] bg-primary rounded-full blur-[150px] opacity-20 pointer-events-none"></div>
    <div class="absolute bottom-[-20%] left-[-10%] w-[50%] h-[50%] bg-secondary rounded-full blur-[150px] opacity-20 pointer-events-none"></div>

    <div class="w-full max-w-md bg-white/80 backdrop-blur-2xl p-8 rounded-3xl shadow-2xl border border-white/50 z-10">
      <div class="text-center mb-8">
        <RouterLink to="/" class="inline-flex items-center justify-center w-12 h-12 bg-primary rounded-xl text-white font-bold text-xl mb-4 shadow-lg shadow-primary/30 transform transition-transform hover:scale-105">T</RouterLink>
        <h2 class="text-3xl font-extrabold text-text-main">Welcome Back</h2>
        <p class="text-text-muted mt-2">Log in to manage your dual-engine portfolio.</p>
      </div>

      <!-- Error Message Alert -->
      <div v-if="errorMessage" class="mb-6 p-4 rounded-xl bg-red-50 border border-red-100 flex items-start gap-3 transition-all">
        <svg class="w-5 h-5 text-red-500 mt-0.5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>
        <p class="text-sm font-medium text-red-700 leading-tight">{{ errorMessage }}</p>
      </div>

      <form @submit="handleLogin" class="space-y-5">
        <div>
          <label for="email" class="block text-sm font-semibold text-text-main mb-1.5">Email Address</label>
          <input 
            type="email" 
            id="email" 
            v-model="email"
            required
            class="w-full px-4 py-3 rounded-xl bg-bg-gray/50 border border-gray-200 focus:border-primary focus:ring-2 focus:ring-primary/20 transition-all outline-none text-text-main placeholder-text-muted"
            placeholder="you@example.com"
          />
        </div>
        
        <div>
          <div class="flex justify-between items-center mb-1.5">
            <label for="password" class="block text-sm font-semibold text-text-main">Password</label>
            <a href="#" class="text-sm font-medium text-primary hover:text-opacity-80 transition-colors">Forgot password?</a>
          </div>
          <input 
            type="password" 
            id="password" 
            v-model="password"
            required
            class="w-full px-4 py-3 rounded-xl bg-bg-gray/50 border border-gray-200 focus:border-primary focus:ring-2 focus:ring-primary/20 transition-all outline-none text-text-main placeholder-text-muted"
            placeholder="••••••••"
          />
        </div>

        <button 
          type="submit" 
          :disabled="isLoading"
          class="w-full py-3.5 rounded-xl bg-primary text-white font-bold text-lg shadow-lg shadow-primary/30 hover:bg-opacity-90 hover:-translate-y-0.5 transition-all duration-300 disabled:opacity-70 disabled:hover:translate-y-0 flex items-center justify-center"
        >
          <svg v-if="isLoading" class="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
          </svg>
          {{ isLoading ? 'Signing In...' : 'Sign In' }}
        </button>
      </form>

      <p class="mt-8 text-center text-text-muted font-medium">
        Don't have an account? 
        <RouterLink to="/auth/register" class="text-primary font-bold hover:underline transition-all">Sign up now</RouterLink>
      </p>
    </div>
  </div>
</template>

<style scoped>
</style>