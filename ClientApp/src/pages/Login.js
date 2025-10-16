import React, { useState } from 'react';
import axios from 'axios';
import { useNavigate, Link } from 'react-router-dom';

const Login = () => {
    const [formData, setFormData] = useState({ email: '', password: '' });
    const [error, setError] = useState('');
    const navigate = useNavigate();

    const handleChange = e => setFormData({ ...formData, [e.target.name]: e.target.value });

    const handleLogin = async (e) => {
        e.preventDefault();
        setError('');
        try {
            const response = await axios.post('/api/auth/login', formData);
            const { role } = response.data;

            if (role === 'Admin') {
                navigate('/admin/dashboard');
            } else if (role === 'Doctor') {
                navigate('/doctor/dashboard');
            } else {
                navigate('/patient/dashboard');
            }
        } catch (err) {
            setError('Неправильний email або пароль.');
        }
    };

    return (
        <div className="row justify-content-center">
            <div className="col-md-6 col-lg-4">
                <div className="card shadow-sm p-4 mt-5">
                    <h2 className="text-center">Вхід</h2>
                    <form onSubmit={handleLogin} className="mt-3">
                        <div className="mb-3">
                            <label className="form-label">Email</label>
                            <input type="email" name="email" onChange={handleChange} className="form-control" required />
                        </div>
                        <div className="mb-3">
                            <label className="form-label">Пароль</label>
                            <input type="password" name="password" onChange={handleChange} className="form-control" required />
                        </div>
                        {error && <div className="alert alert-danger">{error}</div>}
                        <button type="submit" className="w-100 btn btn-primary">Увійти</button>
                        <p className="text-center mt-3 small">
                            Немає акаунту? <Link to="/register">Створити</Link>
                        </p>
                    </form>
                </div>
            </div>
        </div>
    );
};

export default Login;