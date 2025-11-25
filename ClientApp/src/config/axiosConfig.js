// Глобальна конфігурація axios для відправки cookies з кожним запитом
import axios from 'axios';

// Дозволяємо відправку cookies (для Cookie-based authentication)
axios.defaults.withCredentials = true;

// Базовий URL (якщо потрібно)
// axios.defaults.baseURL = 'https://localhost:7263';

// Інтерцептор для обробки помилок 401 (неавторизований)
axios.interceptors.response.use(
    response => response,
    error => {
        if (error.response?.status === 401) {
            // Якщо прийшов 401, можна перенаправити на логін
            console.warn('Unauthorized request - redirecting to login');
            // window.location.href = '/login';
        }
        return Promise.reject(error);
    }
);

export default axios;
