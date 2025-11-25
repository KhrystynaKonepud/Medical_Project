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

        <hr className="my-4" />

        <div className="mt-4">
            <h5 className="text-muted mb-3">API Versioning Demo</h5>
            <p className="small text-muted">Демонстрація роботи з різними версіями API</p>
            <div>
                <Link to="/patients-v1" className="btn btn-info btn-sm me-2">Пацієнти (API v1)</Link>
                <Link to="/patients-v2" className="btn btn-success btn-sm">Пацієнти (API v2)</Link>
            </div>
        </div>
    </div>
);

export default HomePage;