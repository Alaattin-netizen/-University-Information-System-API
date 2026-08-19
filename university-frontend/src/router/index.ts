import { createRouter, createWebHistory } from 'vue-router';
import LoginView from '@/views/LoginView.vue';
import DashboardView from '@/views/DashboardView.vue';
import StudentMainView from '@/views/Student/StudentMainView.vue';
import CoursesView from '@/views/Student/CoursesView.vue';
import GradesView from '@/views/Student/GradesView.vue';
import MessagesView from '@/views/Student/MessagesView.vue';
import ProfileView from '@/views/ProfileView.vue';
import InstructorView from '@/views/Instructor/InstructorView.vue';
import { useAuthStore } from '@/stores/auth';

const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    routes: [
        {
            path: '/login',
            name: 'login',
            component: LoginView,
            meta: { requiresGuest: true }
        },
        {
            path: '/student',
            name: 'student',
            component: StudentMainView,
            meta: { requiresAuth: true }
        },
        {
            path: '/student/courses',
            name: 'student-courses',
            component: CoursesView,
            meta: { requiresAuth: true }
        },
        {
            path: '/student/messages',
            name: 'student-messages',
            component: MessagesView,
            meta: { requiresAuth: true }
        },
        {
            path: '/student/grades', name: 'student-grades', component: GradesView, meta: { requiresAuth: true }
        },
        { path: '/profile', name: 'profile', component: ProfileView, meta: { requiresAuth: true } },
        {
            path: '/dashboard',
            name: 'dashboard',
            component: DashboardView,
            meta: { requiresAuth: true }
        },
        {
            path: '/instructor',
            name: 'instructor',
            component: InstructorView,
            meta: { requiresAuth: true }
        },
        { path: '/', redirect: '/dashboard' },
    ],
});

router.beforeEach((to, from, next) => {
    const authStore = useAuthStore();

    if (to.meta.requiresAuth && !authStore.isAuthenticated) {
        next('/login');
    } else if (to.meta.requiresGuest && authStore.isAuthenticated) {
        next('/dashboard');
    } else {
        next();
    }
});

export default router;