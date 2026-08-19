<template>
  <div class="dashboard-container">
    <header class="dashboard-header">
      <h1>Welcome, {{ user?.firstName }} {{ user?.lastName }}!</h1>
      <div class="header-actions">
        <span class="role-badge">{{ roles.join(', ') }}</span>
        <button @click="logout" class="logout-btn">Logout</button>
      </div>
    </header>

    <hr />

    <div v-if="isAdmin" class="panel admin-panel">
      <h3>Admin Panel</h3>
      <button @click="fetchAdminData" class="panel-btn">Get All Users</button>
      <pre v-if="adminData" class="data-display">{{ adminData }}</pre>
    </div>

    <div v-if="isInstructor" class="panel instructor-panel">
      <h3>Instructor Panel</h3>
      <button @click="routeInstructorPage" class="panel-btn">Instructor Page</button>
      <pre v-if="instructorData" class="data-display">{{ instructorData }}</pre>
    </div>

    <div v-if="isStudent" class="panel student-panel">
      <h3>Student Panel</h3>
      <button @click="routeStudentPage" class="panel-btn">Student Page</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth';
import api from '@/Utils/api';

const authStore = useAuthStore();
const router = useRouter();

const user = ref(authStore.user);
const roles = ref(authStore.roles);
const adminData = ref(null);
const instructorData = ref(null);
const studentData = ref(null);

const isAdmin = authStore.isAdmin;      
const isInstructor = authStore.isInstructor;
const isStudent = authStore.isStudent;

onMounted(() => {
  user.value = authStore.user;
  roles.value = authStore.roles;        
});

const logout = () => {
  authStore.logout();
  router.push('/login');
};

const fetchAdminData = async () => {
  try {
    const res = await api.get('/users');
    adminData.value = res.data;
  } catch (e) {
    console.error(e);
  }
};

const routeInstructorPage = () => {
  router.push('/instructor');
};

const routeStudentPage = () => {
  router.push('/student');
};
</script>

<style scoped>
.dashboard-container {
  max-width: 1100px;
  margin: 0 auto;
  padding: 30px 20px;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
}

.dashboard-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 15px;
}

.dashboard-header h1 {
  margin: 0;
  color: #2c3e50;
  font-size: 1.8rem;
}

.header-actions {
  display: flex;
  align-items: center;
  gap: 15px;
}

.role-badge {
  background: #e8f5e9;
  color: #2e7d32;
  padding: 6px 14px;
  border-radius: 20px;
  font-size: 0.85rem;
  font-weight: 600;
}

.logout-btn {
  padding: 8px 20px;
  background: #f44336;
  color: white;
  border: none;
  border-radius: 6px;
  font-size: 0.9rem;
  cursor: pointer;
  transition: background 0.3s;
}

.logout-btn:hover {
  background: #c62828;
}

hr {
  margin: 20px 0;
  border: none;
  border-top: 2px solid #e0e0e0;
}

.panel {
  background: #f9fafb;
  border-radius: 10px;
  padding: 24px;
  margin-bottom: 20px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
  border-left: 5px solid transparent;
}

.admin-panel {
  border-left-color: #e53935;
}

.instructor-panel {
  border-left-color: #1e88e5;
}

.student-panel {
  border-left-color: #43a047;
}

.panel h3 {
  margin: 0 0 10px 0;
  color: #2c3e50;
  font-size: 1.2rem;
}

.panel-btn {
  padding: 8px 18px;
  background: #4CAF50;
  color: white;
  border: none;
  border-radius: 5px;
  font-size: 0.9rem;
  cursor: pointer;
  transition: background 0.3s;
}

.panel-btn:hover {
  background: #388E3C;
}

.data-display {
  background: #1e1e2e;
  color: #cdd6f4;
  padding: 16px;
  border-radius: 6px;
  overflow-x: auto;
  font-size: 0.85rem;
  margin-top: 12px;
  max-height: 300px;
  overflow-y: auto;
  white-space: pre-wrap;
  word-wrap: break-word;
}

/* Mobile responsiveness */
@media (max-width: 600px) {
  .dashboard-header {
    flex-direction: column;
    align-items: flex-start;
  }
  .header-actions {
    width: 100%;
    justify-content: space-between;
  }
}
</style>