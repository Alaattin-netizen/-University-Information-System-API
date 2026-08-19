<template>
  <div class="login-container">
    <div class="login-card">
      <h2>University Information System</h2>
      <form @submit.prevent="handleLogin">
        <div>
          <label>Email:</label>
          <input v-model="email" type="email" required />
        </div>
        <div>
          <label>Password:</label>
          <input v-model="password" type="password" required />
        </div>
        <button type="submit" :disabled="loading">
          {{ loading ? 'Logging in...' : 'Login' }}
        </button>
        <p v-if="error" class="error">{{ error }}</p>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth';

const email = ref('');
const password = ref('');
const loading = ref(false);
const error = ref('');
const router = useRouter();
const authStore = useAuthStore();

const handleLogin = async () => {
  loading.value = true;
  error.value = '';

  const result = await authStore.login(email.value, password.value);

  if (result.success) {
    router.push('/dashboard');
  } else {
    error.value = result.message;
  }

  loading.value = false;
};
</script>

<style scoped>
.login-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 20px;
}

.login-card {
  background: white;
  padding: 40px 36px;
  border-radius: 12px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
  max-width: 420px;
  width: 100%;
}

.login-card h2 {
  margin: 0 0 24px 0;
  text-align: center;
  color: #2c3e50;
  font-weight: 600;
  font-size: 1.8rem;
}

.login-card label {
  display: block;
  font-weight: 600;
  margin-bottom: 4px;
  color: #333;
  font-size: 0.9rem;
}

.login-card input {
  width: 100%;
  padding: 12px 14px;
  border: 1px solid #ddd;
  border-radius: 6px;
  font-size: 1rem;
  box-sizing: border-box;
  margin-bottom: 18px;
  transition: border-color 0.3s;
}

.login-card input:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.login-card button {
  width: 100%;
  padding: 12px;
  background: #4CAF50;
  color: white;
  border: none;
  border-radius: 6px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.3s;
}

.login-card button:hover:not(:disabled) {
  background: #388E3C;
}

.login-card button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.error {
  color: #c62828;
  margin-top: 12px;
  text-align: center;
  font-size: 0.9rem;
}
</style>