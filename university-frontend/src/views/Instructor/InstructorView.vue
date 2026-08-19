<template>
  <div class="instructor-container">
    <button @click="logout" class="logout-btn">Logout</button>

    <div class="tabs">
      <button
        :class="['tab-btn', { active: activeTab === 'courses' }]"
        @click="activeTab = 'courses'"
      >
        My Courses
      </button>
      <button
        :class="['tab-btn', { active: activeTab === 'students' }]"
        @click="activeTab = 'students'"
        :disabled="!selectedCourseId"
      >
        Enrolled Students
      </button>
      <button
        :class="['tab-btn', { active: activeTab === 'announcements' }]"
        @click="activeTab = 'announcements'"
        :disabled="!selectedCourseId"
      >
        Announcements
      </button>
    </div>

    <div v-if="activeTab === 'courses'" class="tab-panel">
      <h2>Your Courses</h2>
      <div v-if="loading.courses" class="loading">Loading courses...</div>
      <div v-else-if="courses.length === 0" class="empty">
        You are not assigned to any courses.
      </div>
      <div v-else class="course-list">
        <div
          v-for="course in courses"
          :key="course.courseOfferingId"
          class="course-item"
          @click="selectCourse(course.courseOfferingId)"
          :class="{ selected: selectedCourseId === course.courseOfferingId }"
        >
          <div class="course-info">
            <strong>{{ course.courseCode }}</strong> – {{ course.courseName }}
            <span class="credits">({{ course.credits }} ECTS)</span>
            <span class="schedule">{{ course.day }} {{ course.startTime }}–{{ course.endTime }}</span>
            <span class="enrolled">Enrolled: {{ course.enrolledStudentsCount }} / {{ course.quota }}</span>
          </div>
          <button class="select-btn" @click.stop="selectCourse(course.courseOfferingId)">
            Select
          </button>
        </div>
      </div>
    </div>

    <!-- Tab Content: Enrolled Students -->
    <div v-if="activeTab === 'students'" class="tab-panel">
      <h2>Students in {{ selectedCourse?.courseCode || 'Selected Course' }}</h2>
      <div v-if="loading.students" class="loading">Loading students...</div>
      <div v-else-if="students.length === 0" class="empty">
        No students enrolled in this course.
      </div>
      <div v-else>
        <table class="student-table">
          <thead>
            <tr>
              <th>Student</th>
              <th>Email</th>
              <th>Midterm</th>
              <th>Assignment</th>
              <th>Makeup</th>
              <th>Final</th>
              <th>Total</th>
              <th>Grade</th>
              <th>Attendance</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="student in students" :key="student.studentId">
              <td>{{ student.fullName }}</td>
              <td>{{ student.email }}</td>
              <td>{{ student.midtermScore ?? '-' }}</td>
              <td>{{ student.assignmentScore ?? '-' }}</td>
              <td>{{ student.makeupScore ?? '-' }}</td> 
              <td>{{ student.finalScore ?? '-' }}</td>
              <td>{{ student.totalScore ?? '-' }}</td>
              <td :class="getGradeClass(student.letterGrade)">
                {{ student.letterGrade || '—' }}
              </td>
              <td>{{ student.attendanceCount }} / {{ student.totalClasses }}</td>
              <td>
                <button class="grade-btn" @click="openGradeModal(student)">Grade</button>
                <button class="attendance-btn" @click="openAttendanceModal(student)">Attendance</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <div v-if="activeTab === 'announcements'" class="tab-panel">
      <h2>Announcements for {{ selectedCourse?.courseCode || 'Selected Course' }}</h2>
      <div class="announcement-form">
        <h3>Create New Announcement</h3>
        <form @submit.prevent="createAnnouncement">
          <div class="form-group">
            <label>Title</label>
            <input v-model="announcementTitle" type="text" required />
          </div>
          <div class="form-group">
            <label>Content</label>
            <textarea v-model="announcementContent" rows="4" required></textarea>
          </div>
          <button type="submit" :disabled="sendingAnnouncement">
            {{ sendingAnnouncement ? 'Creating...' : 'Create Announcement' }}
          </button>
        </form>
      </div>
      <div v-if="announcements.length > 0" class="announcement-list">
        <div v-for="ann in announcements" :key="ann.id" class="announcement-item">
          <h4>{{ ann.title }}</h4>
          <p>{{ ann.content }}</p>
          <span class="date">{{ formatDate(ann.createdDate) }}</span>
        </div>
      </div>
    </div>

    <div v-if="gradeModalVisible" class="modal-overlay" @click.self="closeGradeModal">
      <div class="modal">
        <h3>Enter Grades for {{ selectedStudent?.fullName }}</h3>
        <form @submit.prevent="submitGrades">
          <div class="form-group">
            <label>Midterm</label>
            <input v-model.number="gradeForm.midtermScore" type="number" step="0.01" min="0" max="100" />
          </div>
          <div class="form-group">
            <label>Final</label>
            <input v-model.number="gradeForm.finalScore" type="number" step="0.01" min="0" max="100" />
          </div>
          <div class="form-group">
            <label>Assignment</label>
            <input v-model.number="gradeForm.assignmentScore" type="number" step="0.01" min="0" max="100" />
          </div>
          <div class="form-group">
            <label>Makeup</label>
            <input v-model.number="gradeForm.makeupScore" type="number" step="0.01" min="0" max="100" />
          </div>
          <div class="modal-actions">
            <button type="submit" :disabled="savingGrade">Save Grades</button>
            <button type="button" @click="closeGradeModal">Cancel</button>
          </div>
          <p v-if="gradeError" class="error">{{ gradeError }}</p>
        </form>
      </div>
    </div>

    <div v-if="attendanceModalVisible" class="modal-overlay" @click.self="closeAttendanceModal">
      <div class="modal">
        <h3>Enter Attendance for {{ selectedStudent?.fullName }}</h3>
        <form @submit.prevent="submitAttendance">
          <div class="form-group">
            <label>Date</label>
            <input v-model="attendanceForm.date" type="date" required />
          </div>
          <div class="form-group">
            <label>Status</label>
            <select v-model="attendanceForm.isPresent">
              <option :value="true">Present</option>
              <option :value="false">Absent</option>
            </select>
          </div>
          <div class="modal-actions">
            <button type="submit" :disabled="savingAttendance">Save Attendance</button>
            <button type="button" @click="closeAttendanceModal">Cancel</button>
          </div>
          <p v-if="attendanceError" class="error">{{ attendanceError }}</p>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth';
import api from '@/Utils/api';

const router = useRouter();
const authStore = useAuthStore();

// ========================
// State
// ========================
const user = ref(authStore.user);
const activeTab = ref<'courses' | 'students' | 'announcements'>('courses');
const selectedCourseId = ref<number | null>(null);
const selectedCourse = ref<any>(null);

const courses = ref<any[]>([]);
const students = ref<any[]>([]);
const announcements = ref<any[]>([]);

const announcementTitle = ref('');
const announcementContent = ref('');
const sendingAnnouncement = ref(false);

const loading = ref({
  courses: false,
  students: false,
});

// Grade Modal
const gradeModalVisible = ref(false);
const selectedStudent = ref<any>(null);
const gradeForm = ref({
      enrollmentId: null as number | null,
  midtermScore: null as number | null,
  finalScore: null as number | null,
  assignmentScore: null as number | null,
  makeupScore: null as number | null,
});
const savingGrade = ref(false);
const gradeError = ref('');

// Attendance Modal
const attendanceModalVisible = ref(false);
const attendanceForm = ref({
  date: new Date().toISOString().split('T')[0],
  isPresent: true,
});
const savingAttendance = ref(false);
const attendanceError = ref('');

// ========================
// Computed
// ========================
const isCourseSelected = computed(() => !!selectedCourseId.value);

// ========================
// Methods
// ========================
const getGradeClass = (letter: string | null) => {
  if (!letter) return '';
  if (letter.startsWith('A')) return 'grade-a';
  if (letter.startsWith('B')) return 'grade-b';
  if (letter.startsWith('C')) return 'grade-c';
  if (letter.startsWith('D')) return 'grade-d';
  if (letter === 'FF') return 'grade-f';
  return '';
};

const formatDate = (dateString: string) => {
  const date = new Date(dateString);
  return date.toLocaleString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
};

// ========================
// Data Fetching
// ========================
const loadCourses = async () => {
  loading.value.courses = true;
  try {
    const res = await api.get('/instructors/me/Responsible-Courses');
    courses.value = res.data;
    if (courses.value.length > 0 && !selectedCourseId.value) {
      selectCourse(courses.value[0].courseOfferingId);
    }
  } catch (err) {
    console.error('Failed to load courses:', err);
  } finally {
    loading.value.courses = false;
  }
};

const loadStudents = async () => {
  if (!selectedCourseId.value) return;
  loading.value.students = true;
  try {
    const res = await api.get(`/instructors/me/Responsible-Courses/${selectedCourseId.value}/Registered-Students`);
    students.value = res.data;
  } catch (err) {
    console.error('Failed to load students:', err);
  } finally {
    loading.value.students = false;
  }
};

const loadAnnouncements = async () => {
  if (!selectedCourseId.value) return;
  try {
    const res = await api.get('/announcements'); // Adjust if you have a filtered endpoint
    announcements.value = res.data.filter((a: any) => a.courseOfferingId === selectedCourseId.value);
  } catch (err) {
    console.error('Failed to load announcements:', err);
  }
};


const selectCourse = (courseOfferingId: number) => {
  selectedCourseId.value = courseOfferingId;
  selectedCourse.value = courses.value.find(c => c.courseOfferingId === courseOfferingId);
  activeTab.value = 'students';
  loadStudents();
  loadAnnouncements();
};

const logout = () => {
  authStore.logout();
  router.push('/login');
};

const createAnnouncement = async () => {
  if (!selectedCourseId.value) {
    alert('Please select a course first.');
    return;
  }
  sendingAnnouncement.value = true;
  try {
    await api.post('/instructors/me/Announcements', {
      courseOfferingId: selectedCourseId.value,
      title: announcementTitle.value,
      content: announcementContent.value,
    });
    announcementTitle.value = '';
    announcementContent.value = '';
    await loadAnnouncements();
    alert('Announcement created successfully!');
  } catch (err: any) {
    alert(err.response?.data?.message || 'Failed to create announcement.');
  } finally {
    sendingAnnouncement.value = false;
  }
};


const openGradeModal = (student: any) => {
  selectedStudent.value = student;
  gradeForm.value = {
     enrollmentId: student.enrollmentId,
    midtermScore: student.midtermScore ?? null,
    finalScore: student.finalScore ?? null,
    assignmentScore: student.assignmentScore ?? null,
    makeupScore: student.makeupScore ?? null,
  };
  gradeError.value = '';
  gradeModalVisible.value = true;
};

const closeGradeModal = () => {
  gradeModalVisible.value = false;
  selectedStudent.value = null;
};

const submitGrades = async () => {
  if (!selectedStudent.value) return;
   const sanitize = (val: any) => (val === '' || val === undefined ? null : val);

  const payload = {
    enrollmentId: gradeForm.value.enrollmentId,
    midtermScore: sanitize(gradeForm.value.midtermScore),
    finalScore: sanitize(gradeForm.value.finalScore),
    assignmentScore: sanitize(gradeForm.value.assignmentScore),
    makeupScore: sanitize(gradeForm.value.makeupScore),
  };
  savingGrade.value = true;
  gradeError.value = '';
  try {
  
    await api.post('/instructors/me/Enter-Grades', 
        
     payload
    );
    closeGradeModal();
    await loadStudents();
    alert('Grades saved successfully!');
  } catch (err: any) {
    gradeError.value = err.response?.data?.message || 'Failed to save grades.';
  } finally {
    savingGrade.value = false;
  }
};


const openAttendanceModal = (student: any) => {
  selectedStudent.value = student;
  attendanceForm.value = {
    date: new Date().toISOString().split('T')[0],
    isPresent: true,
  };
  attendanceError.value = '';
  attendanceModalVisible.value = true;
};

const closeAttendanceModal = () => {
  attendanceModalVisible.value = false;
  selectedStudent.value = null;
};

const submitAttendance = async () => {
  if (!selectedStudent.value || !selectedCourseId.value) return;
  savingAttendance.value = true;
  attendanceError.value = '';
  try {
    await api.post('/instructors/me/Enter-Attendance', {
      studentId: selectedStudent.value.studentId,
      courseOfferingId: selectedCourseId.value,
      date: attendanceForm.value.date,
      isPresent: attendanceForm.value.isPresent,
    });
    await loadStudents();
    closeAttendanceModal();
    alert('Attendance saved successfully!');
  } catch (err: any) {
    attendanceError.value = err.response?.data?.message || 'Failed to save attendance.';
  } finally {
    savingAttendance.value = false;
  }
};


onMounted(() => {
  loadCourses();
});
</script>

<style scoped>
.instructor-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
  font-family: sans-serif;
}

.logout-btn {
  float: right;
  padding: 8px 16px;
  background: #f44336;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

.tabs {
  display: flex;
  gap: 10px;
  margin: 20px 0;
  border-bottom: 1px solid #ccc;
}

.tab-btn {
  padding: 10px 20px;
  background: none;
  border: none;
  border-bottom: 3px solid transparent;
  cursor: pointer;
  font-weight: bold;
  font-size: 1rem;
}

.tab-btn.active {
  border-bottom-color: #4CAF50;
  color: #4CAF50;
}

.tab-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.tab-panel {
  padding: 10px 0;
  min-height: 300px;
}

.loading {
  color: #666;
  font-style: italic;
  padding: 20px;
}

.empty {
  color: #888;
  padding: 20px;
  background: #f9f9f9;
  border-radius: 4px;
}

/* Course List */
.course-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.course-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 16px;
  border: 1px solid #e0e0e0;
  border-radius: 6px;
  background: #fafafa;
  cursor: pointer;
}

.course-item:hover {
  background: #f0f7ff;
}

.course-item.selected {
  border-color: #4CAF50;
  background: #e8f5e9;
}

.course-info {
  display: flex;
  flex-wrap: wrap;
  gap: 8px 16px;
  align-items: center;
}

.credits {
  color: #555;
  font-size: 0.9rem;
}

.schedule {
  color: #888;
  font-size: 0.85rem;
}

.enrolled {
  color: #2e7d32;
  font-size: 0.85rem;
  background: #e8f5e9;
  padding: 2px 8px;
  border-radius: 12px;
}

.select-btn {
  padding: 6px 12px;
  background: #4CAF50;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

/* Student Table */
.student-table {
  width: 100%;
  border-collapse: collapse;
  margin-top: 10px;
}

.student-table th,
.student-table td {
  padding: 10px 12px;
  border: 1px solid #ddd;
  text-align: left;
}

.student-table th {
  background: #f0f0f0;
  font-weight: 600;
}

.student-table tr:nth-child(even) {
  background: #fafafa;
}

.grade-btn {
  padding: 4px 10px;
  background: #1976d2;
  color: white;
  border: none;
  border-radius: 4px;
  margin-right: 4px;
  cursor: pointer;
}

.attendance-btn {
  padding: 4px 10px;
  background: #f57c00;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

.grade-a { color: #2e7d32; font-weight: bold; }
.grade-b { color: #1976d2; font-weight: bold; }
.grade-c { color: #f57c00; font-weight: bold; }
.grade-d { color: #e65100; font-weight: bold; }
.grade-f { color: #c62828; font-weight: bold; }

/* Announcements */
.announcement-form {
  background: #f9f9f9;
  padding: 20px;
  border-radius: 8px;
  margin-bottom: 20px;
}

.announcement-form .form-group {
  margin-bottom: 12px;
}

.announcement-form label {
  display: block;
  font-weight: 600;
  margin-bottom: 4px;
}

.announcement-form input,
.announcement-form textarea {
  width: 100%;
  padding: 8px;
  border: 1px solid #ccc;
  border-radius: 4px;
  box-sizing: border-box;
}

.announcement-form button {
  padding: 8px 20px;
  background: #4CAF50;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

.announcement-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.announcement-item {
  background: white;
  border: 1px solid #e0e0e0;
  border-radius: 6px;
  padding: 16px;
}

.announcement-item h4 {
  margin: 0 0 6px 0;
  color: #1a237e;
}

.announcement-item p {
  margin: 0 0 8px 0;
  color: #444;
}

.announcement-item .date {
  font-size: 0.8rem;
  color: #888;
}

/* Modal */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0,0,0,0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.modal {
  background: white;
  padding: 30px;
  border-radius: 8px;
  max-width: 450px;
  width: 90%;
  box-shadow: 0 4px 20px rgba(0,0,0,0.2);
}

.modal h3 {
  margin-top: 0;
  color: #2c3e50;
}

.modal .form-group {
  margin-bottom: 12px;
}

.modal .form-group label {
  display: block;
  font-weight: 600;
  margin-bottom: 4px;
}

.modal .form-group input,
.modal .form-group select {
  width: 100%;
  padding: 8px;
  border: 1px solid #ccc;
  border-radius: 4px;
  box-sizing: border-box;
}

.modal-actions {
  display: flex;
  gap: 10px;
  margin-top: 15px;
}

.modal-actions button {
  padding: 8px 20px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

.modal-actions button[type="submit"] {
  background: #4CAF50;
  color: white;
}

.modal-actions button[type="button"] {
  background: #ccc;
  color: #333;
}

.modal-actions button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.error {
  color: #c62828;
  margin-top: 8px;
}
</style>