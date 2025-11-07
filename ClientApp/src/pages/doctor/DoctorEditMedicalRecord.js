import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import axios from "axios";

const DoctorEditMedicalRecord = () => {
    const { id } = useParams();
    const navigate = useNavigate();

    const [record, setRecord] = useState({
        diagnosis: "",
        treatment: "",
        notes: "",
        appointmentId: null
    });

    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        loadRecord();
    }, []);

    const loadRecord = async () => {
        try {
            const response = await axios.get(
                `https://localhost:7263/api/doctor/medical-records/${id}`,
                { withCredentials: true }
            );

            setRecord({
                diagnosis: response.data.diagnosis,
                treatment: response.data.treatment,
                notes: response.data.notes,
                appointmentId: response.data.appointmentId
            });
        } catch (err) {
            console.error(err);
            setError("Не вдалося завантажити медичний запис.");
        } finally {
            setLoading(false);
        }
    };

    const handleSave = async () => {
        try {
            await axios.put(
                `https://localhost:7263/api/doctor/medical-records/${id}`,
                record,
                { withCredentials: true }
            );

            // ✅ Переходимо до перегляду цього ж запису
            navigate(`/doctor/dashboard/medicalrecord/${id}`);
        } catch (err) {
            console.error(err);
            alert("Помилка під час збереження!");
        }
    };

    if (loading) return <div className="container mt-4">Завантаження...</div>;
    if (error) return <div className="container mt-4 alert alert-danger">{error}</div>;

    return (
        <div className="container mt-4">
            <h2 className="mb-4">Редагування медичного запису</h2>

            <div className="card shadow-sm">
                <div className="card-body">

                    <div className="mb-3">
                        <label className="form-label fw-bold">Діагноз</label>
                        <textarea
                            className="form-control"
                            rows="3"
                            value={record.diagnosis}
                            onChange={(e) => setRecord({ ...record, diagnosis: e.target.value })}
                        ></textarea>
                    </div>

                    <div className="mb-3">
                        <label className="form-label fw-bold">Лікування</label>
                        <textarea
                            className="form-control"
                            rows="3"
                            value={record.treatment}
                            onChange={(e) => setRecord({ ...record, treatment: e.target.value })}
                        ></textarea>
                    </div>

                    <div className="mb-3">
                        <label className="form-label fw-bold">Рекомендації</label>
                        <textarea
                            className="form-control"
                            rows="3"
                            value={record.notes}
                            onChange={(e) => setRecord({ ...record, notes: e.target.value })}
                        ></textarea>
                    </div>

                    <div className="d-flex gap-2">
                        <button className="btn btn-success" onClick={handleSave}>
                            Зберегти
                        </button>

                    </div>

                </div>
            </div>
        </div>
    );
};

export default DoctorEditMedicalRecord;