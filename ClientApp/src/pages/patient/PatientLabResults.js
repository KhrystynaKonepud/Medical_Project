import React, { useState, useEffect } from 'react';
import axios from 'axios';

export default function PatientLabResults() {
    const [results, setResults] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    useEffect(() => {
        const fetchResults = async () => {
            try {
                const { data } = await axios.get('/api/patient-data/test-results');
                setResults(data);
            } catch (err) {
                setError("Не вдалося завантажити результати аналізів.");
            } finally {
                setLoading(false);
            }
        };
        fetchResults();
    }, []);

    if (loading) return <p>Завантаження...</p>;

    return (
        <div>
            <h1 className="display-5">Перегляд аналізів</h1>
            {error && <div className="alert alert-danger">{error}</div>}

            {results.length === 0 && !error && (
                <p>У вас поки що немає результатів аналізів.</p>
            )}

            {results.map(res => (
                <div key={res.id} className="card mb-3">
                    <div className="card-header">
                        <strong>{res.testName}</strong> - {new Date(res.dateConducted).toLocaleDateString()}
                    </div>
                    <div className="card-body">
                        <p className="card-text"><strong>Результат:</strong> {res.result}</p>
                        <p className="card-text"><strong>Нотатки:</strong> {res.notes || "Немає"}</p>
                        <footer className="blockquote-footer">Лікар: {res.doctorName}</footer>
                    </div>
                </div>
            ))}
        </div>
    );
}