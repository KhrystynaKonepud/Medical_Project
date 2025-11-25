// ClientApp/src/pages/Login.js
import React, { useEffect, useMemo, useState } from "react";
import axios from "axios";
import { useNavigate, Link, useLocation } from "react-router-dom";

export default function Login() {
    const navigate = useNavigate();
    const location = useLocation();

    const [form, setForm] = useState({ email: "", password: "" });
    const [showPwd, setShowPwd] = useState(false);
    const [serverError, setServerError] = useState("");
    const [pending, setPending] = useState(false);

    // Показати помилку, якщо Google повернув ?error=...
    useEffect(() => {
        const params = new URLSearchParams(location.search);
        const googleErr = params.get("error");
        if (googleErr) {
            const map = {
                "google-no-info": "Не вдалося отримати дані від Google.",
                "google-no-email": "Google не повернув email.",
                "create-failed": "Не вдалося створити користувача за Google-аккаунтом."
            };
            setServerError(map[googleErr] || "Помилка входу через Google.");
        }
    }, [location.search]);

    const errors = useMemo(() => {
        const e = {};
        if (!form.email) e.email = "Вкажіть email.";
        else if (!/\S+@\S+\.\S+/.test(form.email)) e.email = "Невірний формат email.";

        if (!form.password) e.password = "Вкажіть пароль.";
        return e;
    }, [form]);

    const isValid = useMemo(() => Object.keys(errors).length === 0, [errors]);

    const onChange = (e) => {
        const { name, value } = e.target;
        setForm((s) => ({ ...s, [name]: value }));
    };

    const onSubmit = async (e) => {
        e.preventDefault();
        setServerError("");
        if (!isValid) return;

        try {
            setPending(true);
            const payload = { email: form.email.trim(), password: form.password };
            const { data } = await axios.post("/api/auth/login", payload, { withCredentials: true });
            const role = data?.role ?? "Patient";

            if (role === "Admin") navigate("/admin/dashboard");
            else if (role === "Doctor") navigate("/doctor/dashboard");
            else navigate("/patient/dashboard");
        } catch (err) {
            const msg = err?.response?.data?.message || "Неправильний email або пароль.";
            setServerError(msg);
        } finally {
            setPending(false);
        }
    };

    const handleGoogleLogin = () => {
        // куди повертати після успіху — можеш змінити на "/"
        const returnUrl = encodeURIComponent("/patient/dashboard");
        window.location.href = `/api/auth/google/login?returnUrl=${returnUrl}`;
    };

    return (
        <div className="row justify-content-center">
            <div className="col-md-6 col-lg-4">
                <div className="card shadow-sm p-4 mt-5">
                    <h2 className="text-center">Вхід</h2>

                    <form onSubmit={onSubmit} className="mt-3" noValidate>
                        {/* Email */}
                        <div className="mb-3">
                            <label className="form-label">Email</label>
                            <input
                                type="email"
                                name="email"
                                value={form.email}
                                onChange={onChange}
                                className={`form-control ${errors.email ? "is-invalid" : ""}`}
                                required
                            />
                            {errors.email && <div className="invalid-feedback">{errors.email}</div>}
                        </div>

                        {/* Password */}
                        <div className="mb-3">
                            <label className="form-label">Пароль</label>
                            <div className="input-group">
                                <input
                                    type={showPwd ? "text" : "password"}
                                    name="password"
                                    value={form.password}
                                    onChange={onChange}
                                    className={`form-control ${errors.password ? "is-invalid" : ""}`}
                                    required
                                />
                                <button
                                    type="button"
                                    className="btn btn-outline-secondary"
                                    onClick={() => setShowPwd((s) => !s)}
                                >
                                    {showPwd ? "Сховати" : "Показати"}
                                </button>
                            </div>
                            {errors.password && <div className="invalid-feedback d-block">{errors.password}</div>}
                        </div>

                        {serverError && <div className="alert alert-danger">{serverError}</div>}

                        <button type="submit" className="w-100 btn btn-primary" disabled={!isValid || pending}>
                            {pending ? "Вхід..." : "Увійти"}
                        </button>

                        <div className="text-center text-muted my-3">або</div>

                        {/* Google Sign-In */}
                        <button
                            type="button"
                            className="w-100 btn btn-outline-danger"
                            onClick={handleGoogleLogin}
                        >
                            Увійти з Google
                        </button>

                        <p className="text-center mt-3 small">
                            Немає акаунту? <Link to="/register">Створити</Link>
                        </p>
                    </form>
                </div>
            </div>
        </div>
    );
}
