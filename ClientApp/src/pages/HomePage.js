import React from 'react';
import { Link } from 'react-router-dom';

const HomePage = () => (
    <div className="text-center p-5 shadow-sm rounded bg-light" style={{ marginTop: '20vh' }}>
        <h1>Вітаємо в MedCenter!</h1>
        <p className="lead text-muted">Ваше здоров'я - наш пріоритет.</p>
        <div className="mt-4">
            <Link to="/login" className="btn btn-primary btn-lg me-2">Увійти</Link>
            <Link to="/register" className="btn btn-outline-secondary btn-lg">Зареєструватися</Link>
        </div>
    </div>
);

export default HomePage;