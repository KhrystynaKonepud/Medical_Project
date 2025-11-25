import React from 'react';
import ReactDOM from 'react-dom/client';
import './config/axiosConfig'; // Глобальна конфігурація axios (withCredentials)
import App from './App';
import 'bootstrap/dist/css/bootstrap.min.css';

const root = ReactDOM.createRoot(document.getElementById('root'));
root.render(
    <React.StrictMode>
        <App />
    </React.StrictMode>
);