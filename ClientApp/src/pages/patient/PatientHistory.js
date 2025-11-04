import React, { useState, useEffect } from 'react';
import axios from 'axios';

// Тут має бути модалка для відгуків (Пункт 6)
const LeaveReviewModal = ({ appointment, onClose, onReviewSuccess }) => {
    const [rating, setRating] = useState(5);
    const [comment, setComment] = useState('');
    const [error, setError] = useState('');

    const handleSubmit = async () => {
        try {
            const payload = {
                doctorId: appointment.doctorId, // Потрібно додати doctorId в GetMedicalHistory
                rating: Number(rating),
                comment: comment
            };
            await axios.post('/api/patient-data/leave-review', payload);
            onReviewSuccess();
            onClose();
        } catch (err) {
            setError(err.response?.data?.message || "Помилка відправки відгуку.");
        }
    };

    return (
        <div className="modal fade show d-block" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
            <div className="modal-dialog modal-dialog-centered">
                <div className="modal-content">
                    <div className="modal-header">
                        <h5 className="modal-title">Залишити відгук</h5>
                        <button type="button" className="btn-close" onClick={onClose}></button>
                    </div>
                    <div className="modal-body">
                        <p>Оцініть ваш візит до {appointment.doctorName} ({appointment.doctorSpecialization})</p>
                        <div className="mb-3">
                            <label className="form-label">Рейтинг (1-5)</label>
                            <input type="number" min="1" max="5" value={rating} onChange={e => setRating(e.target.value)} className="form-control" />
                        </div>
                        <div className="mb-3">
                            <label className="form-label">Коментар</label>
                            <textarea value={comment} onChange={e => setComment(e.target.value)} className="form-control" rows="3"></textarea>
                        </div>
                        {error && <div className="alert alert-danger">{error}</div>}
                    </div>
                    <div className="modal-footer">
                        <button type="button" className="btn btn-light" onClick={onClose}>Скасувати</button>
                        <button type="button" className="btn btn-primary" onClick={handleSubmit}>Відправити</button>
                    </div>
                </div>
            </div>
        </div>
    );
};


export default function PatientHistory() {
    const [history, setHistory] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [reviewAppointment, setReviewAppointment] = useState(null);
    const [success, setSuccess] = useState('');

    const fetchHistory = async () => {
        try {
            // Виправлення: Потрібно оновити ендпоінт в PatientDataController, щоб він повертав doctorId
            // Зараз припускаємо, що він оновлений
            const { data } = await axios.get('/api/patient-data/medical-history');
            setHistory(data);
        } catch (err) {
            setError("Не вдалося завантажити історію візитів.");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchHistory();
    }, []);

    const getStatusClass = (status) => {
        if (status === 1) return "text-success"; // Completed
        if (status === 2) return "text-danger"; // Canceled
        return "text-primary"; // Scheduled
    };

    const getStatusText = (status) => {
        if (status === 1) return "Проведено";
        if (status === 2) return "Скасовано";
        return "Заплановано";
    };


    return (
        <div>
            <h1 className="display-5">Історія записів</h1>
            {error && <div className="alert alert-danger">{error}</div>}
            {success && <div className="alert alert-success">{success}</div>}

            {loading && <p>Завантаження...</p>}

            {history.length === 0 && !loading && <p>У вас ще немає записів.</p>}

            <div className="list-group">
                {history.map(item => (
                    <div key={item.id} className="list-group-item list-group-item-action flex-column align-items-start">
                        <div className="d-flex w-100 justify-content-between">
                            <h5 className="mb-1">{item.doctorName} ({item.doctorSpecialization})</h5>
                            <small className={getStatusClass(item.status)}>{getStatusText(item.status)}</small>
                        </div>
                        <p className="mb-1"><strong>Дата:</strong> {new Date(item.date).toLocaleString()}</p>
                        <p className="mb-1"><strong>Причина:</strong> {item.reason}</p>

                        {/* 6. Можливість залишити відгук */}
                        {item.status === 1 && ( // 1 = Completed
                            <button
                                className="btn btn-sm btn-outline-info mt-2"
                                onClick={() => setReviewAppointment(item)}
                            >
                                Залишити відгук
                            </button>
                        )}
                    </div>
                ))}
            </div>

            {reviewAppointment && (
                <LeaveReviewModal
                    appointment={reviewAppointment}
                    onClose={() => setReviewAppointment(null)}
                    onReviewSuccess={() => setSuccess("Дякуємо за ваш відгук!")}
                />
            )}
        </div>
    );
}