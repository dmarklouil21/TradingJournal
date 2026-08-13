<script setup>
import { ref } from 'vue';
import { RouterLink, useRouter } from 'vue-router';
import axios from 'axios';

const router = useRouter();
const email = ref('');
const password = ref('');
const isLoading = ref(false);

const handleLogin = async (e) => {
  e.preventDefault();
  isLoading.value = true;
  // Mock login for now
  // setTimeout(() => {
  //   isLoading.value = false;
  //   router.push('/home');
  // }, 1000);
  try {
    const credentials = {
      email: email.value,
      password: password.value
    };

    const res = await axios.post("http://localhost:5234/api/auth/login", credentials);
    if(res.data) {
      alert("Login successfull");
    }
  }
  catch(e) {
    alert("Login failed!");
    console.log("An error occured while attempting to login", e);
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