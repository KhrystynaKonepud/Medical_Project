import React from 'react';

export default function DoctorPatients() {
    return (
        <div>
            <h1 className="display-5 mb-4">Пацієнти</h1>
            <p className="lead">Тут буде відображено список усіх ваших закріплених пацієнтів, історія їхніх прийомів та аналізів.</p>

            <div className="alert alert-info">
                Список пацієнтів незабаром буде завантажено...
            </div>

            {/* Тут буде логіка завантаження пацієнтів з API */}
        </div>
    );
}