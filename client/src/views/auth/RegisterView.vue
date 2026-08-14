<script setup>
import axios from 'axios';
import { ref } from 'vue';
import { RouterLink, useRouter } from 'vue-router';
import { register } from '@/services/auth';

const router = useRouter();
const name = ref('');
const email = ref('');
const password = ref('');
const isLoading = ref(false);

const errorMessage = ref('');

const handleRegister = async (e) => {
  e.preventDefault();
  isLoading.value = true;
  errorMessage.value = '';

  try {
    const form = {
      fullName: name.value,
      email: email.value,
      password: password.value
    };

    const res = await register(form);
    
    if(res.status == 200) {
      localStorage.setItem('token', res.data.token);
      router.push('/home');
    }
  } 
  catch (ex) {
    if(ex.response && ex.response.data && ex.response.data.errors) {
      const errors = ex.response.data.errors;
      if (Array.isArray(errors)) {
        errorMessage.value = errors.join(', ');
      } else if (typeof errors === 'object') {
        errorMessage.value = Object.values(errors).flat().join(', ');
      } else {
        errorMessage.value = errors.toString();
      }
    } else {
      errorMessage.value = "Registration failed. Please try again.";
    }
    console.error(ex);
  }
  finally {
    isLoading.value = false;
  }
};
</script>

<template>
  <div class="min-h-screen flex items-center justify-center bg-bg-gray relative overflow-hidden p-4">
    <!-- Decorative background elements -->
    <div class="absolute top-[-20%] left-[-10%] w-[50%] h-[50%] bg-secondary rounded-full blur-[150px] opacity-20 pointer-events-none"></div>
    <div class="absolute bottom-[-20%] right-[-10%] w-[50%] h-[50%] bg-primary rounded-full blur-[150px] opacity-20 pointer-events-none"></div>

    <div class="w-full max-w-md bg-white/80 backdrop-blur-2xl p-8 rounded-3xl shadow-2xl border border-white/50 z-10">
      <div class="text-center mb-8">
        <RouterLink to="/" class="inline-flex items-center justify-center w-12 h-12 bg-secondary rounded-xl text-white font-bold text-xl mb-4 shadow-lg shadow-secondary/30 transform transition-transform hover:scale-105">T</RouterLink>
        <h2 class="text-3xl font-extrabold text-text-main">Create Account</h2>
        <p class="text-text-muted mt-2">Start tracking your investments and trades today.</p>
      </div>

      <!-- Error Message Alert -->
      <div v-if="errorMessage" class="mb-6 p-4 rounded-xl bg-red-50 border border-red-100 flex items-start gap-3 transition-all">
        <svg class="w-5 h-5 text-red-500 mt-0.5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>
        <p class="text-sm font-medium text-red-700 leading-tight">{{ errorMessage }}</p>
      </div>

      <form @submit="handleRegister" class="space-y-5">
        <div>
          <label for="name" class="block text-sm font-semibold text-text-main mb-1.5">Full Name</label>
          <input 
            type="text" 
            id="name" 
            v-model="name"
            required
            class="w-full px-4 py-3 rounded-xl bg-bg-gray/50 border border-gray-200 focus:border-secondary focus:ring-2 focus:ring-secondary/20 transition-all outline-none text-text-main placeholder-text-muted"
            placeholder="John Doe"
          />
        </div>

        <div>
          <label for="email" class="block text-sm font-semibold text-text-main mb-1.5">Email Address</label>
          <input 
            type="email" 
            id="email" 
            v-model="email"
            required
            class="w-full px-4 py-3 rounded-xl bg-bg-gray/50 border border-gray-200 focus:border-secondary focus:ring-2 focus:ring-secondary/20 transition-all outline-none text-text-main placeholder-text-muted"
            placeholder="you@example.com"
          />
        </div>
        
        <div>
          <label for="password" class="block text-sm font-semibold text-text-main mb-1.5">Password</label>
          <input 
            type="password" 
            id="password" 
            v-model="password"
            required
            class="w-full px-4 py-3 rounded-xl bg-bg-gray/50 border border-gray-200 focus:border-secondary focus:ring-2 focus:ring-secondary/20 transition-all outline-none text-text-main placeholder-text-muted"
            placeholder="••••••••"
          />
          <p class="text-xs text-text-muted mt-2">Must be at least 8 characters.</p>
        </div>

        <button 
          type="submit" 
          :disabled="isLoading"
          class="w-full py-3.5 rounded-xl bg-secondary text-white font-bold text-lg shadow-lg shadow-secondary/30 hover:bg-opacity-90 hover:-translate-y-0.5 transition-all duration-300 disabled:opacity-70 disabled:hover:translate-y-0 flex items-center justify-center"
        >
          <svg v-if="isLoading" class="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
          </svg>
          {{ isLoading ? 'Creating account...' : 'Sign Up' }}
        </button>
      </form>

      <p class="mt-8 text-center text-text-muted font-medium">
        Already have an account? 
        <RouterLink to="/auth/login" class="text-secondary font-bold hover:underline transition-all">Sign in</RouterLink>
      </p>
    </div>
  </div>
</template>

<style scoped>
</style>