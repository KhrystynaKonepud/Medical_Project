import React from 'react';

export default function DoctorMessages() {
    return (
        <div>
            <h1 className="display-5 mb-4">Повідомлення</h1>
            <p className="lead">Це ваш центр обміну повідомленнями. Тут ви можете спілкуватися з пацієнтами та колегами.</p>

            <div className="alert alert-warning">
                Функціонал обміну повідомленнями знаходиться на етапі розробки.
            </div>

            {/* Тут буде інтерфейс чату або списку повідомлень */}
        </div>
    );
}