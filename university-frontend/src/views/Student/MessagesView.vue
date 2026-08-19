<template>
  <div class="messages-container">
    <header class="page-header">
      <h1>Messages</h1>
      <button @click="goBack" class="back-btn">← Back</button>
    </header>

    <div v-if="error" class="alert error">{{ error }}</div>
    <div v-if="success" class="alert success">{{ success }}</div>

    <div class="compose-section">
      <h2>Send a Message</h2>
      <form @submit.prevent="sendMessage">
        <div class="form-group">
          <label for="instructor">Select Instructor:</label>
          <select id="instructor" v-model="selectedInstructorId" required>
            <option value="">-- Select an Instructor --</option>
            <option v-for="instructor in instructors" :key="instructor.id" :value="instructor.id">
              {{ instructor.fullName }} ({{ instructor.email }})
            </option>
          </select>
        </div>

        <div class="form-group">
          <label for="subject">Subject:</label>
          <input id="subject" v-model="subject" type="text" placeholder="Enter subject" required />
        </div>

        <div class="form-group">
          <label for="content">Message:</label>
          <textarea id="content" v-model="content" rows="5" placeholder="Write your message here..." required></textarea>
        </div>

        <button type="submit" :disabled="sending" class="send-btn">
          {{ sending ? 'Sending...' : 'Send Message' }}
        </button>
      </form>
    </div>

    <div class="sent-section">
      <h2>Your Sent Messages</h2>
      <div v-if="loading.sent" class="loading">Loading sent messages...</div>
      <div v-else-if="sentMessages.length === 0" class="empty">
        You haven't sent any messages yet.
      </div>
      <div v-else>
        <div v-for="msg in sentMessages" :key="msg.id" class="message-card">
          <div class="message-header">
            <strong>To: {{ msg.receiverName }}</strong>
            <span class="date">{{ formatDate(msg.sentDate) }}</span>
          </div>
          <div class="message-subject">{{ msg.subject }}</div>
          <div class="message-content">{{ msg.content }}</div>
          <div class="message-status">
            <span v-if="msg.isRead" class="read">✓ Read</span>
            <span v-else class="unread">● Unread</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import api from '@/Utils/api';

interface Instructor {
  id: number;
  fullName: string;
  email: string;
}

interface SentMessage {
  id: number;
  receiverName: string;
  subject: string;
  content: string;
  sentDate: string;
  isRead: boolean;
}

const router = useRouter();
const instructors = ref<Instructor[]>([]);
const selectedInstructorId = ref('');
const subject = ref('');
const content = ref('');
const sending = ref(false);
const error = ref('');
const success = ref('');
const sentMessages = ref<SentMessage[]>([]);
const loading = ref({ sent: false });

const loadInstructors = async () => {
  try {
    const res = await api.get('/users?roles=Instructor');
    instructors.value = res.data.map((u: any) => ({
      id: u.id,
      fullName: `${u.firstName} ${u.lastName}`,
      email: u.email,
    }));
  } catch (err) {
    console.error('Failed to load instructors:', err);
  }
};

const loadSentMessages = async () => {
  loading.value.sent = true;
  try {
    const res = await api.get('/students/me/messages');
    sentMessages.value = res.data;
  } catch (err) {
    console.error('Failed to load sent messages:', err);
  } finally {
    loading.value.sent = false;
  }
};

const sendMessage = async () => {
  if (!selectedInstructorId.value) {
    error.value = 'Please select an instructor.';
    return;
  }
  if (!subject.value.trim() || !content.value.trim()) {
    error.value = 'Subject and content are required.';
    return;
  }

  sending.value = true;
  error.value = '';
  success.value = '';

  try {
    await api.post('/students/me/message', {
      receiverInstructorId: parseInt(selectedInstructorId.value),
      subject: subject.value,
      content: content.value,
    });

    success.value = 'Message sent successfully!';
    subject.value = '';
    content.value = '';
    selectedInstructorId.value = '';
    await loadSentMessages();
  } catch (err: any) {
    error.value = err.response?.data?.message || 'Failed to send message.';
  } finally {
    sending.value = false;
  }
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

const goBack = () => {
  router.back();
};

onMounted(() => {
  loadInstructors();
  loadSentMessages();
});
</script>

<style scoped>
.messages-container {
  max-width: 850px;
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

.alert {
  padding: 12px 16px;
  border-radius: 6px;
  margin-bottom: 16px;
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

.compose-section {
  background: #f9fafb;
  padding: 24px;
  border-radius: 10px;
  margin-bottom: 30px;
  border-left: 5px solid #4CAF50;
}

.compose-section h2 {
  margin: 0 0 16px 0;
  color: #2c3e50;
  font-size: 1.2rem;
}

.form-group {
  margin-bottom: 14px;
}

.form-group label {
  display: block;
  font-weight: 600;
  margin-bottom: 4px;
  color: #333;
  font-size: 0.9rem;
}

.form-group select,
.form-group input,
.form-group textarea {
  width: 100%;
  padding: 10px 12px;
  border: 1px solid #ddd;
  border-radius: 6px;
  font-size: 1rem;
  box-sizing: border-box;
}

.form-group select:focus,
.form-group input:focus,
.form-group textarea:focus {
  outline: none;
  border-color: #4CAF50;
  box-shadow: 0 0 0 3px rgba(76, 175, 80, 0.1);
}

.send-btn {
  padding: 10px 24px;
  background: #4CAF50;
  color: white;
  border: none;
  border-radius: 6px;
  font-size: 1rem;
  cursor: pointer;
  transition: background 0.3s;
}

.send-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.send-btn:hover:not(:disabled) {
  background: #388E3C;
}

.sent-section h2 {
  color: #2c3e50;
  font-size: 1.2rem;
  margin-bottom: 16px;
}

.loading {
  color: #666;
  font-style: italic;
  padding: 10px 0;
}

.empty {
  color: #888;
  padding: 20px;
  background: #f9f9f9;
  border-radius: 6px;
}

.message-card {
  background: white;
  border: 1px solid #e8e8e8;
  border-radius: 8px;
  padding: 16px 20px;
  margin-bottom: 12px;
  transition: box-shadow 0.2s;
}

.message-card:hover {
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
}

.message-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 4px;
}

.message-header strong {
  color: #2c3e50;
}

.date {
  font-size: 0.8rem;
  color: #888;
}

.message-subject {
  font-weight: 600;
  font-size: 1.05rem;
  color: #1a237e;
  margin: 4px 0;
}

.message-content {
  color: #444;
  margin: 6px 0;
  white-space: pre-wrap;
  word-wrap: break-word;
}

.message-status {
  margin-top: 6px;
  font-size: 0.85rem;
}

.read {
  color: #2e7d32;
}

.unread {
  color: #f57c00;
}
</style>