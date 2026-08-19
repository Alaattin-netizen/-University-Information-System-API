<template>
  <div class="grades-container">
    <header class="page-header">
      <h1>My Grades</h1>
      <button @click="goBack" class="back-btn">← Back</button>
    </header>

    <div class="gpa-summary">
      <div class="gpa-card">
        <h3>Semester GPA</h3>
        <p class="gpa-value">{{ semesterGPA.toFixed(2) }}</p>
      </div>
      <div class="gpa-card">
        <h3>Cumulative GPA</h3>
        <p class="gpa-value">{{ cumulativeGPA.toFixed(2) }}</p>
      </div>
      <div class="gpa-card">
        <h3>Total Credits</h3>
        <p class="gpa-value">{{ totalCredits }}</p>
      </div>
    </div>

    <div class="section">
      <h2>Current Enrollments</h2>
      <div v-if="loading.grades" class="loading">Loading grades...</div>
      <div v-else-if="grades.length === 0" class="empty">
        You are not enrolled in any courses, or no grades have been entered yet.
      </div>
      <div v-else>
        <table class="grade-table">
          <thead>
            <tr>
              <th>Course Code</th>
              <th>Course Name</th>
              <th>Credits</th>
              <th>Midterm</th>
              <th>Final</th>
              <th>Total</th>
              <th>Letter Grade</th>
              <th>Grade Point</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="grade in grades" :key="grade.courseCode">
              <td>{{ grade.courseCode }}</td>
              <td>{{ grade.courseName }}</td>
              <td>{{ grade.credits }}</td>
              <td>{{ grade.midterm ?? '-' }}</td>
              <td>{{ grade.final ?? '-' }}</td>
              <td>{{ grade.totalScore ?? '-' }}</td>
              <td :class="getGradeClass(grade.letterGrade)">
                {{ grade.letterGrade || '—' }}
              </td>
              <td>{{ grade.gradePoint ?? '-' }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <div class="section">
      <h2>Transcript</h2>
      <div v-if="loading.transcript" class="loading">Loading transcript...</div>
      <div v-else-if="transcript.length === 0" class="empty">
        No completed courses yet.
      </div>
      <div v-else>
        <table class="grade-table">
          <thead>
            <tr>
              <th>Semester</th>
              <th>Course Code</th>
              <th>Course Name</th>
              <th>Credits</th>
              <th>Letter Grade</th>
              <th>Grade Point</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in transcript" :key="item.courseCode">
              <td>{{ item.semester || '—' }}</td>
              <td>{{ item.courseCode }}</td>
              <td>{{ item.courseName }}</td>
              <td>{{ item.credits }}</td>
              <td :class="getGradeClass(item.letterGrade)">
                {{ item.letterGrade || '—' }}
              </td>
              <td>{{ item.gradePoint ?? '-' }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import api from '@/Utils/api';

interface Grade {
  courseCode: string;
  courseName: string;
  credits: number;
  midterm: number | null;
  final: number | null;
  totalScore: number | null;
  letterGrade: string | null;
  gradePoint: number | null;
}

interface TranscriptItem {
  semester?: string;
  courseCode: string;
  courseName: string;
  credits: number;
  letterGrade: string | null;
  gradePoint: number | null;
}

const router = useRouter();
const grades = ref<Grade[]>([]);
const transcript = ref<TranscriptItem[]>([]);
const semesterGPA = ref(0);
const cumulativeGPA = ref(0);
const totalCredits = ref(0);

const loading = ref({
  grades: false,
  transcript: false,
});

const getGradeClass = (letter: string | null) => {
  if (!letter) return '';
  if (letter.startsWith('A')) return 'grade-a';
  if (letter.startsWith('B')) return 'grade-b';
  if (letter.startsWith('C')) return 'grade-c';
  if (letter.startsWith('D')) return 'grade-d';
  if (letter === 'FF') return 'grade-f';
  return '';
};

const loadGrades = async () => {
  loading.value.grades = true;
  try {
    const res = await api.get('/students/me/grades');
    grades.value = res.data;
  } catch (err) {
    console.error('Failed to load grades:', err);
  } finally {
    loading.value.grades = false;
  }
};

const loadGPA = async () => {
  try {
    const res = await api.get('/students/me/gpa');
    semesterGPA.value = res.data.semesterGPA || 0;
    cumulativeGPA.value = res.data.cumulativeGPA || 0;
    totalCredits.value = res.data.totalCredits || 0;
  } catch (err) {
    console.error('Failed to load GPA:', err);
  }
};

const loadTranscript = async () => {
  loading.value.transcript = true;
  try {
    const res = await api.get('/students/me/transcript');
    transcript.value = res.data;
  } catch (err) {
    console.error('Failed to load transcript:', err);
  } finally {
    loading.value.transcript = false;
  }
};

const goBack = () => {
  router.back();
};

onMounted(() => {
  loadGrades();
  loadGPA();
  loadTranscript();
});
</script>

<style scoped>
.grades-container {
  max-width: 1100px;
  margin: 0 auto;
  padding: 30px 20px;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.page-header h1 {
  margin: 0;
  color: #2c3e50;
  font-size: 1.8rem;
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

.gpa-summary {
  display: flex;
  gap: 20px;
  margin-bottom: 30px;
}

.gpa-card {
  flex: 1;
  background: #f9fafb;
  padding: 16px 24px;
  border-radius: 10px;
  text-align: center;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.06);
}

.gpa-card h3 {
  margin: 0 0 6px 0;
  color: #555;
  font-weight: normal;
  font-size: 0.85rem;
  text-transform: uppercase;
}

.gpa-value {
  font-size: 2.2rem;
  font-weight: bold;
  margin: 0;
  color: #2c3e50;
}

.section {
  margin-top: 30px;
}

.section h2 {
  color: #2c3e50;
  font-size: 1.2rem;
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

.grade-table {
  width: 100%;
  border-collapse: collapse;
  margin-top: 10px;
}

.grade-table th,
.grade-table td {
  padding: 10px 12px;
  border: 1px solid #e0e0e0;
  text-align: left;
}

.grade-table th {
  background: #f0f0f0;
  font-weight: 600;
  color: #333;
}

.grade-table tr:nth-child(even) {
  background: #fafafa;
}

.grade-table tr:hover {
  background: #f0f7ff;
}

.grade-a {
  color: #2e7d32;
  font-weight: bold;
}

.grade-b {
  color: #1976d2;
  font-weight: bold;
}

.grade-c {
  color: #f57c00;
  font-weight: bold;
}

.grade-d {
  color: #e65100;
  font-weight: bold;
}

.grade-f {
  color: #c62828;
  font-weight: bold;
}

@media (max-width: 600px) {
  .gpa-summary {
    flex-direction: column;
  }
  .grade-table {
    font-size: 0.85rem;
  }
  .grade-table th,
  .grade-table td {
    padding: 6px 8px;
  }
}
</style>