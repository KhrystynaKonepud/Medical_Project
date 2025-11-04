import React, { useState, useEffect } from 'react';
import axios from 'axios';

export default function PatientPrescriptions() {
    const [prescriptions, setPrescriptions] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    useEffect(() => {
        const fetchPrescriptions = async () => {
            try {
                const { data } = await axios.get('/api/patient-data/prescriptions');
                setPrescriptions(data);
            } catch (err) {
                setError("Не вдалося завантажити рецепти.");
            } finally {
                setLoading(false);
            }
        };
        fetchPrescriptions();
    }, []);

    if (loading) return <p>Завантаження...</p>;

    return (
        <div>
            <h1 className="display-5">Мої рецепти</h1>
            {error && <div className="alert alert-danger">{error}</div>}

            {prescriptions.length === 0 && !error && (
                <p>У вас немає активних рецептів.</p>
            )}

            {prescriptions.map(p => (
                <div key={p.id} className="card mb-3">
                    <div className="card-header">
                        <strong>{p.medication}</strong> - (виписано: {new Date(p.dateIssued).toLocaleDateString()})
                    </div>
                    <div className="card-body">
                        <p className="card-text"><strong>Дозування:</strong> {p.dosage}</p>
                        <p className="card-text"><strong>Нотатки:</strong> {p.notes || "Немає"}</p>
                        <footer className="blockquote-footer">Лікар: {p.doctorName}</footer>
                    </div>
                </div>
            ))}
        </div>
    );
}