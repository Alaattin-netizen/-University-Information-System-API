<template>
  <div class="student-dashboard-container">
    <header class="dashboard-header">
      <h1>Student Dashboard</h1>
      <button @click="logout" class="logout-btn">Logout</button>
    </header>

    <div class="dashboard-grid">
      <div class="card" @click="routeCourse">
        <div class="card-icon">📚</div>
        <h3>Course Operations</h3>
        <p>Enroll in courses, view your schedule</p>
      </div>

      <div class="card" @click="routeGrades">
        <div class="card-icon">📊</div>
        <h3>Grades</h3>
        <p>View your grades and GPA</p>
      </div>

      <div class="card" @click="routeMessages">
        <div class="card-icon">💬</div>
        <h3>Messages</h3>
        <p>Send and receive messages</p>
      </div>

      <div class="card" @click="routeProfile">
        <div class="card-icon">👤</div>
        <h3>Profile</h3>
        <p>View and edit your profile</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth';

const router = useRouter();
const authStore = useAuthStore();

const routeCourse = () => {
  router.push('/student/courses');
};

const routeGrades = () => {
  router.push('/student/grades');
};

const routeMessages = () => {
  router.push('/student/messages');
};

const routeProfile = () => {
  router.push('/profile');
};

const logout = () => {
  authStore.logout();
  router.push('/login');
};
</script>

<style scoped>
.student-dashboard-container {
  max-width: 1100px;
  margin: 0 auto;
  padding: 30px 20px;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
}

.dashboard-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 30px;
}

.dashboard-header h1 {
  margin: 0;
  color: #2c3e50;
  font-size: 1.8rem;
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

.dashboard-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 24px;
}

.card {
  background: #f9fafb;
  border-radius: 10px;
  padding: 30px 20px;
  text-align: center;
  cursor: pointer;
  transition: transform 0.2s, box-shadow 0.2s;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
  border: 1px solid #e8e8e8;
}

.card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
  border-color: #4CAF50;
}

.card-icon {
  font-size: 2.8rem;
  margin-bottom: 12px;
}

.card h3 {
  margin: 0 0 6px 0;
  color: #2c3e50;
  font-size: 1.1rem;
}

.card p {
  margin: 0;
  color: #888;
  font-size: 0.9rem;
}
</style>