<template>
  <div class="profile-container">
    <header class="profile-header">
      <h1>My Profile</h1>
      <button @click="goBack" class="back-btn">← Back</button>
    </header>

    <div v-if="loading" class="loading">Loading profile...</div>

    <div v-if="error" class="alert error">{{ error }}</div>
    <div v-if="success" class="alert success">{{ success }}</div>

    <div v-else class="profile-card">
      <!-- Read-only Info -->
      <div class="info-section">
        <div class="info-row">
          <span class="label">Name:</span>
          <span class="value">{{ user.firstName }} {{ user.lastName }}</span>
        </div>
        <div class="info-row">
          <span class="label">Email:</span>
          <span class="value">{{ user.email }}</span>
        </div>
        <div class="info-row">
          <span class="label">Roles:</span>
          <span class="value role-badge-list">{{ user.roles?.join(', ') || 'N/A' }}</span>
        </div>
        <div class="info-row">
          <span class="label">Department:</span>
          <span class="value">{{ user.departmentName || 'Not assigned' }}</span>
        </div>
        <div class="info-row">
          <span class="label">Advisor:</span>
          <span class="value">{{ user.advisorName || 'Not assigned' }}</span>
        </div>
      </div>

      <hr />

      <!-- Editable Fields -->
      <div class="edit-section">
        <h2>Edit Profile</h2>
        <form @submit.prevent="updateProfile">
          <div class="form-group">
            <label for="firstName">First Name</label>
            <input id="firstName" v-model="editForm.firstName" type="text" required />
          </div>
          <div class="form-group">
            <label for="lastName">Last Name</label>
            <input id="lastName" v-model="editForm.lastName" type="text" required />
          </div>

          <button type="submit" :disabled="saving" class="update-btn">
            {{ saving ? 'Saving...' : 'Update Profile' }}
          </button>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, reactive } from 'vue';
import { useAuthStore } from '@/stores/auth';
import { useRouter } from 'vue-router';
import api from '@/Utils/api';

const authStore = useAuthStore();
const router = useRouter();

// State
const user = ref<any>({});
const loading = ref(true);
const saving = ref(false);
const error = ref('');
const success = ref('');


const editForm = reactive({
  firstName: '',
  lastName: '',
});

// Load profile data
const loadProfile = async () => {
  loading.value = true;
  error.value = '';
  try {
    const res = await api.get('/users/me');
    user.value = res.data;
    // Populate edit form
    editForm.firstName = user.value.firstName || '';
    editForm.lastName = user.value.lastName || '';
  } catch (err: any) {
    error.value = err.response?.data?.message || 'Failed to load profile.';
  } finally {
    loading.value = false;
  }
};

// Update profile
const updateProfile = async () => {
  error.value = '';
  success.value = '';
  saving.value = true;

  try {
    await api.put('/users/me/profile', {
      firstName: editForm.firstName,
      lastName: editForm.lastName,
    });

    // Update local state
    user.value.firstName = editForm.firstName;
    user.value.lastName = editForm.lastName;

    // Update Pinia store (so header/dashboard shows new name)
    if (authStore.user) {
      authStore.user.firstName = editForm.firstName;
      authStore.user.lastName = editForm.lastName;
    }

    success.value = 'Profile updated successfully!';
    router.go(0); // Refresh the page to reflect changes
  } catch (err: any) {
    error.value = err.response?.data?.message || 'Failed to update profile.';
  
  } finally {
    saving.value = false;
  }
};

const goBack = () => {
  router.back();
};

// Load on mount
onMounted(() => {
  loadProfile();
});
</script>

<style scoped>
.profile-container {
  max-width: 750px;
  margin: 0 auto;
  padding: 30px 20px;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
}

.profile-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.profile-header h1 {
  margin: 0;
  color: #2c3e50;
  font-size: 1.8rem;
  font-weight: 600;
}

.back-btn {
  padding: 8px 18px;
  background: #e0e0e0;
  color: #333;
  border: none;
  border-radius: 6px;
  font-size: 0.9rem;
  cursor: pointer;
  transition: background 0.3s;
}

.back-btn:hover {
  background: #bdbdbd;
}

.loading {
  color: #666;
  font-style: italic;
  padding: 20px 0;
}

.alert {
  padding: 14px 18px;
  border-radius: 6px;
  margin: 10px 0 20px 0;
  font-size: 0.95rem;
}

.alert.error {
  background: #ffebee;
  color: #c62828;
  border: 1px solid #ef9a9a;
}

.alert.success {
  background: #e8f5e9;
  color: #2e7d32;
  border: 1px solid #a5d6a7;
}

.profile-card {
  background: #f9fafb;
  border-radius: 10px;
  padding: 28px 32px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
  border-left: 5px solid #4CAF50;
}

.info-section {
  margin-bottom: 20px;
}

.info-row {
  display: flex;
  padding: 10px 0;
  border-bottom: 1px solid #eee;
}

.info-row:last-of-type {
  border-bottom: none;
}

.info-row .label {
  font-weight: 600;
  width: 130px;
  color: #555;
  flex-shrink: 0;
}

.info-row .value {
  flex: 1;
  color: #222;
  word-break: break-word;
}

.role-badge-list {
  color: #2e7d32;
  font-weight: 500;
}

hr {
  margin: 24px 0;
  border: none;
  border-top: 2px dashed #ddd;
}

.edit-section h2 {
  color: #34495e;
  margin: 0 0 18px 0;
  font-size: 1.2rem;
}

.form-group {
  margin-bottom: 16px;
}

.form-group label {
  display: block;
  font-weight: 600;
  margin-bottom: 4px;
  color: #333;
  font-size: 0.9rem;
}

.form-group input {
  width: 100%;
  padding: 12px 14px;
  border: 1px solid #ddd;
  border-radius: 6px;
  font-size: 1rem;
  box-sizing: border-box;
  transition: border-color 0.3s;
}

.form-group input:focus {
  outline: none;
  border-color: #4CAF50;
  box-shadow: 0 0 0 3px rgba(76, 175, 80, 0.1);
}

.update-btn {
  padding: 12px 28px;
  background: #4CAF50;
  color: white;
  border: none;
  border-radius: 6px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.3s;
  width: 100%;
}

.update-btn:hover:not(:disabled) {
  background: #388E3C;
}

.update-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* Mobile responsiveness */
@media (max-width: 600px) {
  .profile-container {
    padding: 16px;
  }

  .profile-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 12px;
  }

  .profile-card {
    padding: 20px 16px;
  }

  .info-row {
    flex-direction: column;
    padding: 8px 0;
  }

  .info-row .label {
    width: 100%;
    margin-bottom: 2px;
  }

  .info-row .value {
    padding-left: 0;
  }
}
</style>