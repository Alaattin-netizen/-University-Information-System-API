import { defineStore } from 'pinia';
import api from '@/Utils/api';

interface User {
    firstName: string;
    lastName: string;
    id: number;
    email: string;
}

export const useAuthStore = defineStore('auth', {
    state: () => ({
    user: JSON.parse(localStorage.getItem('user') || 'null') as User | null,
        token: localStorage.getItem('token') || null,
        roles: JSON.parse(localStorage.getItem('roles') || '[]') as string[],
    }),
    getters: {
        isAuthenticated: (state) => !!state.token,
        isAdmin: (state) => state.roles.includes('Admin'),
        isInstructor: (state) => state.roles.includes('Instructor'),
        isStudent: (state) => state.roles.includes('Student'),
    },
    actions: {
        async login(email: string, password: string) {
            try {
                const response = await api.post('/auth/login', { email, password });
                const { userId, email: userEmail, roles, token, firstName, lastName } = response.data;

                this.user = { id: userId, email: userEmail, firstName, lastName };
                this.token = token;
                this.roles = roles;

                localStorage.setItem('token', token);
                localStorage.setItem('roles', JSON.stringify(roles));
                localStorage.setItem('user', JSON.stringify({ id: userId, email: userEmail }));

                return { success: true };
            } catch (error: any) {
                return {
                    success: false,
                    message: error.response?.data?.message || 'Login failed',
                };
            }
        },
        logout() {
            this.user = null;
            this.token = null;
            this.roles = [];
            localStorage.removeItem('token');
            localStorage.removeItem('roles');
            localStorage.removeItem('user');
        },
    },
});