import React, { useState, useEffect } from "react";
import axios from "axios";
import { useNavigate, useParams } from "react-router-dom";

const EditDoctor = () => {
    const navigate = useNavigate();
    const { id } = useParams();

    const [form, setForm] = useState({
        fullName: "",
        email: "",
        specialization: "",
        experienceYears: "",
        bio: "",
    });
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(true);

    // Завантаження даних лікаря
    useEffect(() => {
        const fetchDoctor = async () => {
            try {
                const response = await axios.get(`/api/admin/doctors/${id}`);
                const doctor = response.data;

                setForm({
                    fullName: doctor.fullName || "",
                    email: doctor.email || "",
                    specialization: doctor.specialization || "",
                    experienceYears: doctor.experienceYears?.toString() || "",
                    bio: doctor.bio || "",
                });
            } catch (err) {
                console.error("Помилка при завантаженні лікаря:", err);
                setError("Не вдалося завантажити дані лікаря");
            } finally {
                setLoading(false);
            }
        };

        fetchDoctor();
    }, [id]);

    const handleChange = (e) => {
        setForm({ ...form, [e.target.name]: e.target.value });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError("");

        try {
            await axios.put(`/api/admin/doctors/${id}`, {
                FullName: form.fullName,
                Email: form.email,
                Specialization: form.specialization,
                ExperienceYears: Number(form.experienceYears),
                Bio: form.bio,
            });

            navigate("/admin/doctors");
        } catch (err) {
            console.error(err);
            if (err.response && err.response.data) {
                setError(JSON.stringify(err.response.data));
            } else {
                setError("Помилка при оновленні лікаря");
            }
        }
    };

    if (loading) {
        return <div className="container mt-4">Завантаження...</div>;
    }

    return (
        <div className="container mt-4">
            <h2>Редагувати лікаря</h2>
            {error && <div className="alert alert-danger">{error}</div>}

            <form onSubmit={handleSubmit}>
                <input
                    name="fullName"
                    placeholder="Повне ім'я"
                    value={form.fullName}
                    onChange={handleChange}
                    className="form-control mb-2"
                />
                <input
                    name="email"
                    type="email"
                    placeholder="Email"
                    value={form.email}
                    onChange={handleChange}
                    className="form-control mb-2"
                />
                <input
                    name="specialization"
                    placeholder="Спеціалізація"
                    value={form.specialization}
                    onChange={handleChange}
                    className="form-control mb-2"
                />
                <input
                    name="experienceYears"
                    type="number"
                    placeholder="Роки досвіду"
                    value={form.experienceYears}
                    onChange={handleChange}
                    className="form-control mb-2"
                />
                <textarea
                    name="bio"
                    placeholder="Біо"
                    value={form.bio}
                    onChange={handleChange}
                    className="form-control mb-2"
                />

                <button type="submit" className="btn btn-primary">
                    Зберегти зміни
                </button>
            </form>
        </div>
    );
};

export default EditDoctor;