<template>
  <div class="student-dashboard">
    <h1>Student Dashboard</h1>

    <div class="tab">
      <div>
        <h2>Available Courses</h2>
        <div v-if="loading.available" class="loading">Loading courses...</div>
        <div v-else-if="availableCourses.length === 0" class="empty">
          No courses available for enrollment.
        </div>
        <div v-else class="course-list">
          <div v-for="course in availableCourses" :key="course.id" class="course-item">
            <div class="course-info">
              <strong>{{ course.code }}</strong> – {{ course.name }}
              <span class="credits">({{ course.credits }} ECTS)</span>
              <span class="quota">Quota: {{ course.availableSlots }} / {{ course.quota }}</span>
              <span v-if="course.hasPrerequisite" class="prereq">
                Prereq: {{ course.prerequisiteCode }}
              </span>
            </div>
            <button
              @click="enroll(course.id)"
              :disabled="enrolling === course.id"
              class="enroll-btn"
            >
              {{ enrolling === course.id ? 'Enrolling...' : 'Enroll' }}
            </button>
          </div>
        </div>
      </div>

      <div class="tab-panel">
        <h2>Current Enrollments</h2>
        <div v-if="loading.enrollments" class="loading">Loading enrollments...</div>
        <div v-else-if="myEnrollments.length === 0" class="empty">
          You are not enrolled in any courses.
        </div>
        <div v-else class="course-list">
          <div v-for="enrollment in myEnrollments" :key="enrollment.id" class="course-item">
            <div class="course-info">
              <strong>{{ enrollment.courseCode }}</strong> – {{ enrollment.courseName }}
              <span class="credits">({{ enrollment.credits }} ECTS)</span>
              <span class="grade" v-if="enrollment.letterGrade">
                Grade: {{ enrollment.letterGrade }} ({{ enrollment.gradePoint }})
              </span>
              <span v-else class="grade">Not graded</span>
            </div>
            <button
              @click="drop(enrollment.id)"
              :disabled="dropping === enrollment.id"
              class="drop-btn"
            >
              {{ dropping === enrollment.id ? 'Dropping...' : 'Drop' }}
            </button>
          </div>
        </div>
      </div>
    </div>

    <div class="schedule-panel">
      <h2>Your Weekly Schedule</h2>
      <div v-if="loading.schedule" class="loading">Loading schedule...</div>
      <div v-else-if="schedule.length === 0" class="empty">
        No courses scheduled.
      </div>
      <div v-else class="schedule-grid">
        <div class="schedule-header">
          <div>Day</div>
          <div>Course</div>
          <div>Time</div>
          <div>Room</div>
          <div>Instructor</div>
        </div>
        <div
          v-for="item in schedule"
          :key="item.courseOfferingId"
          class="schedule-row"
        >
          <div>{{ item.day }}</div>
          <div><strong>{{ item.courseCode }}</strong> – {{ item.courseName }}</div>
          <div>{{ item.startTime }} – {{ item.endTime }}</div>
          <div>{{ item.classroom }}</div>
          <div>{{ item.instructor }}</div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import api from '@/Utils/api';


const availableCourses = ref<any[]>([]);
const myEnrollments = ref<any[]>([]);
const schedule = ref<any[]>([]);

const loading = ref({
  available: false,
  enrollments: false,
  schedule: false,
});
const enrolling = ref<number | null>(null);
const dropping = ref<number | null>(null);
const error = ref<string | null>(null);

const loadData = async () => {
  await Promise.all([
    loadAvailableCourses(),
    loadEnrollments(),
    loadSchedule(),
  ]);
};

const loadAvailableCourses = async () => {
  loading.value.available = true;
  try {
    const res = await api.get('/students/me/open-courses');
    availableCourses.value = res.data;
  } catch (err) {
    console.error('Failed to load courses:', err);
  } finally {
    loading.value.available = false;
  }
};

const loadEnrollments = async () => {
  loading.value.enrollments = true;
  try {
    const res = await api.get('/students/me/enrollments');
    myEnrollments.value = res.data;
  } catch (err) {
    console.error('Failed to load enrollments:', err);
  } finally {
    loading.value.enrollments = false;
  }
};

const loadSchedule = async () => {
  loading.value.schedule = true;
  try {
    const res = await api.get('/students/me/schedule');
    schedule.value = res.data;
  } catch (err) {
    console.error('Failed to load schedule:', err);
  } finally {
    loading.value.schedule = false;
  }
};

const enroll = async (courseOfferingId: number) => {
  enrolling.value = courseOfferingId;
  error.value = null;
  try {
    await api.post('/students/me/enroll', { courseOfferingId });
    await loadData();
    alert('Successfully enrolled!');
  } catch (err: any) {
    const msg = err.response?.data?.message || 'Failed to enroll.';
    error.value = msg;
    alert(`Error: ${msg}`);
  } finally {
    enrolling.value = null;
  }
};

// Drop a course
const drop = async (enrollmentId: number) => {
  dropping.value = enrollmentId;
  error.value = null;
  try {
    await api.delete(`/students/me/enrollments/${enrollmentId}`);
    await loadData();
    alert('Successfully dropped!');
  } catch (err: any) {
    const msg = err.response?.data?.message || 'Failed to drop.';
    error.value = msg;
    alert(`Error: ${msg}`);
  } finally {
    dropping.value = null;
  }
};

// Load data when component mounts
onMounted(() => {
  loadData();
});
</script>

<style scoped>
.student-dashboard {
  max-width: 1100px;
  margin: 0 auto;
  padding: 30px 20px;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
}

.student-dashboard h1 {
  color: #2c3e50;
  font-size: 1.8rem;
  margin: 0 0 24px 0;
}

.student-dashboard h2 {
  color: #2c3e50;
  font-size: 1.2rem;
  margin: 0 0 16px 0;
  border-bottom: 2px solid #4CAF50;
  padding-bottom: 8px;
}

.loading {
  color: #666;
  font-style: italic;
  padding: 20px 0;
}

.empty {
  color: #888;
  padding: 20px;
  background: #f9f9f9;
  border-radius: 6px;
  margin-top: 10px;
}

.tab {
  display: flex;
  flex-direction: column;
  gap: 30px;
}

.course-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.course-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 14px 18px;
  border: 1px solid #e8e8e8;
  border-radius: 8px;
  background: #fafafa;
  transition: border-color 0.2s, box-shadow 0.2s;
}

.course-item:hover {
  border-color: #4CAF50;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}

.course-info {
  display: flex;
  flex-wrap: wrap;
  gap: 8px 16px;
  align-items: center;
  flex: 1;
}

.course-info strong {
  color: #2c3e50;
}

.credits {
  color: #555;
  font-size: 0.9rem;
}

.quota {
  color: #888;
  font-size: 0.85rem;
}

.prereq {
  color: #d32f2f;
  font-size: 0.85rem;
  background: #ffebee;
  padding: 2px 10px;
  border-radius: 12px;
}

.grade {
  color: #2e7d32;
  font-weight: 500;
}

.enroll-btn {
  padding: 6px 16px;
  background: #4CAF50;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  transition: background 0.3s;
  font-size: 0.9rem;
  flex-shrink: 0;
}

.enroll-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.enroll-btn:hover:not(:disabled) {
  background: #388E3C;
}

.drop-btn {
  padding: 6px 16px;
  background: #f44336;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  transition: background 0.3s;
  font-size: 0.9rem;
  flex-shrink: 0;
}

.drop-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.drop-btn:hover:not(:disabled) {
  background: #c62828;
}

.schedule-panel {
  margin-top: 40px;
  border-top: 2px solid #e0e0e0;
  padding-top: 20px;
}

.schedule-panel h2 {
  color: #2c3e50;
  border-bottom: 2px solid #4CAF50;
  padding-bottom: 8px;
  margin-bottom: 16px;
}

.schedule-grid {
  display: grid;
  grid-template-columns: 1fr 2fr 1fr 1fr 2fr;
  gap: 4px;
  background: #f9fafb;
  padding: 8px;
  border-radius: 8px;
}

.schedule-header {
  display: contents;
  font-weight: bold;
}

.schedule-header > div {
  background: #e0e0e0;
  padding: 8px 4px;
  text-align: center;
  border-radius: 4px 4px 0 0;
}

.schedule-row {
  display: contents;
}

.schedule-row > div {
  padding: 6px 4px;
  border-bottom: 1px solid #eee;
  background: white;
}

@media (max-width: 700px) {
  .course-item {
    flex-direction: column;
    align-items: stretch;
    gap: 10px;
  }

  .enroll-btn,
  .drop-btn {
    width: 100%;
    padding: 8px;
  }

  .schedule-grid {
    grid-template-columns: 0.8fr 1.8fr 0.8fr 0.8fr 1.2fr;
    font-size: 0.8rem;
  }
}
</style>