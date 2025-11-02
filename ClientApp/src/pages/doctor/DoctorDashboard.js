import React from 'react';
import { Link } from 'react-router-dom';
// Примітка: useNavigate та axios більше не потрібні тут,
// оскільки логіка виходу перенесена в DoctorHeader.js

const DoctorDashboard = () => {
    // Вхідна точка, яка буде відображатися в <Outlet /> всередині DoctorLayout
    return (
        // DoctorLayout забезпечує загальну структуру, тут лише контент головної сторінки
        <div>
            <h1 className="display-5 mb-4">Головна панель лікаря</h1>

            <p className="lead mb-4">Ласкаво просимо до вашого кабінету, лікарю! Тут ви можете швидко перейти до основних розділів.</p>

            {/* Меню посилань (як швидкі кнопки) */}
            <div className="list-group list-group-horizontal-md mb-4">
                <Link to="/doctor/patients" className="list-group-item list-group-item-action text-center py-3">
                    <i className="bi bi-people-fill me-2"></i> Пацієнти
                </Link>
                <Link to="/doctor/availability" className="list-group-item list-group-item-action text-center py-3">
                    <i className="bi bi-calendar-check me-2"></i> Графік прийомів
                </Link>
                <Link to="/doctor/messages" className="list-group-item list-group-item-action text-center py-3">
                    <i className="bi bi-chat-dots me-2"></i> Повідомлення
                </Link>
            </div>

            <div className="row">
                <div className="col-md-6">
                    <div className="card shadow-sm">
                        <div className="card-body">
                            <h5 className="card-title">Останні записи</h5>
                            <p className="card-text">Перегляньте найближчі заплановані прийоми.</p>
                            <Link to="/doctor/appointments" className="btn btn-primary">
                                Перейти до записів
                            </Link>
                        </div>
                    </div>
                </div>
                <div className="col-md-6">
                    <div className="card shadow-sm">
                        <div className="card-body">
                            <h5 className="card-title">Мій Профіль</h5>
                            <p className="card-text">Перегляньте та оновіть інформацію про себе.</p>
                            <Link to="/doctor/profile" className="btn btn-outline-primary">
                                Редагувати профіль
                            </Link>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default DoctorDashboard;