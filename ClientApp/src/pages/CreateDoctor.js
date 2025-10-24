import React, { useState } from "react";
import axios from "axios";
import { useNavigate } from "react-router-dom";

const CreateDoctor = () => {
    const navigate = useNavigate();
    const [form, setForm] = useState({
        fullName: "",
        email: "",
        password: "",
        specialization: "",
        experienceYears: "",
        bio: "",
    });
    const [error, setError] = useState("");

    const handleChange = (e) => {
        setForm({ ...form, [e.target.name]: e.target.value });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError("");

        try {
            await axios.post("/api/admin/doctors", {
                FullName: form.fullName,
                Email: form.email,
                Password: form.password,
                Specialization: form.specialization,
                ExperienceYears: Number(form.experienceYears),
                Bio: form.bio,
            });

            navigate("/admin/doctors"); // після додавання — переходимо на список
        } catch (err) {
            console.error(err);
            if (err.response && err.response.data) {
                setError(JSON.stringify(err.response.data));
            } else {
                setError("Помилка при додаванні лікаря");
            }
        }
    };

    return (
        <div className="container mt-4">

            <h2>Додати нового лікаря</h2>
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
                    name="password"
                    type="password"
                    placeholder="Пароль"
                    value={form.password}
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

                <button type="submit" className="btn btn-success">
                    Додати лікаря
                </button>
            </form>
        </div>
    );
};

export default CreateDoctor;