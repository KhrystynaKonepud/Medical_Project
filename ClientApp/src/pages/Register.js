import React, { useState } from 'react';
import axios from 'axios';
import { useNavigate, Link } from 'react-router-dom';

const Register = () => {
    const [formData, setFormData] = useState({
        fullName: '',
        email: '',
        password: '',
        confirmPassword: '',
        phoneNumber: ''
    });
    const [error, setError] = useState('');
    const navigate = useNavigate();

    const handleChange = e => setFormData({ ...formData, [e.target.name]: e.target.value });

    const handleRegister = async (e) => {
        e.preventDefault();
        if (formData.password !== formData.confirmPassword) {
            setError('Паролі не співпадають.');
            return;
        }
        setError('');
        try {
            await axios.post('/api/auth/register', formData);
            // Після успішної реєстрації одразу перекидаємо в кабінет
            navigate('/patient/dashboard');
        } catch (err) {
            setError('Помилка реєстрації. Можливо, такий email вже існує або дані некоректні.');
        }
    };

    return (
        <div className="row justify-content-center">
            <div className="col-md-6 col-lg-5">
                <div className="card shadow-sm p-4 mt-5">
                    <h2 className="text-center">Реєстрація</h2>
                    <form onSubmit={handleRegister} className="mt-3">
                        <div className="mb-3">
                            <label className="form-label">Повне ім'я</label>
                            <input type="text" name="fullName" onChange={handleChange} className="form-control" required />
                        </div>
                        <div className="mb-3">
                            <label className="form-label">Email</label>
                            <input type="email" name="email" onChange={handleChange} className="form-control" required />
                        </div>
                        <div className="mb-3">
                            <label className="form-label">Телефон</label>
                            <input type="tel" name="phoneNumber" placeholder="+380XXXXXXXXX" onChange={handleChange} className="form-control" required />
                        </div>
                        <div className="mb-3">
                            <label className="form-label">Пароль</label>
                            <input type="password" name="password" onChange={handleChange} className="form-control" minLength="8" required />
                        </div>
                        <div className="mb-3">
                            <label className="form-label">Підтвердження пароля</label>
                            <input type="password" name="confirmPassword" onChange={handleChange} className="form-control" required />
                        </div>
                        {error && <div className="alert alert-danger">{error}</div>}
                        <button type="submit" className="w-100 btn btn-primary">Зареєструватися</button>
                        <p className="text-center mt-3 small">
                            Вже є акаунт? <Link to="/login">Увійти</Link>
                        </p>
                    </form>
                </div>
            </div>
        </div>
    );
};

export default Register;