import React, { useMemo, useState } from "react";
import axios from "axios";
import { Link, useNavigate } from "react-router-dom";

// Якщо не поставиш глобально в index.js:
axios.defaults.withCredentials = true;

const PHONE_RE = /^\+?380\d{9}$/;                          // +380XXXXXXXXX
const PASSWORD_RE = /^(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,16}$/; // 8–16, 1 велика, 1 цифра, 1 спец

export default function Register() {
    const navigate = useNavigate();

    const [form, setForm] = useState({
        fullName: "",
        email: "",
        phoneNumber: "",
        password: "",
        confirmPassword: ""
    });
    const [showPwd, setShowPwd] = useState(false);
    const [serverError, setServerError] = useState("");
    const [pending, setPending] = useState(false);

    const errors = useMemo(() => {
        const e = {};

        // Full name ≤ 500
        if (!form.fullName) e.fullName = "Вкажіть повне ім'я.";
        else if (form.fullName.length > 500) e.fullName = "Не більше 500 символів.";

        // Email формат + ≤ 50 (бо Username ≤ 50)
        if (!form.email) e.email = "Вкажіть email.";
        else {
            const emailValid = /\S+@\S+\.\S+/.test(form.email);
            if (!emailValid) e.email = "Невірний формат email.";
            else if (form.email.length > 50) e.email = "Email (username) має бути ≤ 50 символів.";
        }

        // Телефон UA
        if (!form.phoneNumber) e.phoneNumber = "Вкажіть номер.";
        else if (!PHONE_RE.test(form.phoneNumber)) e.phoneNumber = "Формат: +380XXXXXXXXX.";

        // Пароль 8–16, 1 велика, 1 цифра, 1 спец
        if (!form.password) e.password = "Вкажіть пароль.";
        else if (!PASSWORD_RE.test(form.password))
            e.password = "8–16 символів, мінімум 1 велика літера, 1 цифра, 1 спецсимвол.";

        // Підтвердження
        if (!form.confirmPassword) e.confirmPassword = "Підтвердіть пароль.";
        else if (form.confirmPassword !== form.password) e.confirmPassword = "Паролі не збігаються.";

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
            const payload = {
                fullName: form.fullName.trim(),
                email: form.email.trim(),
                password: form.password,
                phoneNumber: form.phoneNumber.trim(),
            };

            const { data } = await axios.post("/api/auth/register", payload, { withCredentials: true });

            // бекенд повертає role="Patient" за замовчуванням (або іншу)
            const role = data?.role ?? "Patient";
            if (role === "Admin") navigate("/admin/dashboard");
            else if (role === "Doctor") navigate("/doctor/dashboard");
            else navigate("/patient/dashboard");
        } catch (err) {
            const msg =
                err?.response?.data?.message ||
                err?.response?.data?.errors?.[0]?.description ||
                "Помилка реєстрації. Перевірте дані та спробуйте ще раз.";
            setServerError(msg);
        } finally {
            setPending(false);
        }
    };

    return (
        <div className="row justify-content-center">
            <div className="col-md-7 col-lg-5">
                <div className="card shadow-sm p-4 mt-5">
                    <h2 className="text-center">Реєстрація</h2>

                    <form className="mt-3" onSubmit={onSubmit} noValidate>
                        {/* Full name */}
                        <div className="mb-3">
                            <label className="form-label">Повне ім'я</label>
                            <input
                                type="text"
                                name="fullName"
                                value={form.fullName}
                                onChange={onChange}
                                className={`form-control ${errors.fullName ? "is-invalid" : ""}`}
                                maxLength={500}
                                required
                            />
                            {errors.fullName && <div className="invalid-feedback">{errors.fullName}</div>}
                        </div>

                        {/* Email */}
                        <div className="mb-3">
                            <label className="form-label">Email</label>
                            <input
                                type="email"
                                name="email"
                                value={form.email}
                                onChange={onChange}
                                className={`form-control ${errors.email ? "is-invalid" : ""}`}
                                maxLength={50} // Username ≤ 50
                                required
                            />
                            {errors.email && <div className="invalid-feedback">{errors.email}</div>}
                        </div>

                        {/* Phone */}
                        <div className="mb-3">
                            <label className="form-label">Телефон</label>
                            <input
                                type="tel"
                                name="phoneNumber"
                                value={form.phoneNumber}
                                onChange={onChange}
                                className={`form-control ${errors.phoneNumber ? "is-invalid" : ""}`}
                                placeholder="+380XXXXXXXXX"
                                pattern="\+?380\d{9}"
                                required
                            />
                            {errors.phoneNumber && <div className="invalid-feedback">{errors.phoneNumber}</div>}
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
                                    // HTML5 дублює наш regex; бекенд все одно перевірить
                                    pattern="(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,16}"
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
                            <small className="text-muted">8–16, мінімум 1 велика, 1 цифра, 1 спецсимвол.</small>
                        </div>

                        {/* Confirm */}
                        <div className="mb-3">
                            <label className="form-label">Підтвердження пароля</label>
                            <input
                                type={showPwd ? "text" : "password"}
                                name="confirmPassword"
                                value={form.confirmPassword}
                                onChange={onChange}
                                className={`form-control ${errors.confirmPassword ? "is-invalid" : ""}`}
                                required
                            />
                            {errors.confirmPassword && (
                                <div className="invalid-feedback">{errors.confirmPassword}</div>
                            )}
                        </div>

                        {serverError && <div className="alert alert-danger">{serverError}</div>}

                        <button type="submit" className="w-100 btn btn-primary" disabled={!isValid || pending}>
                            {pending ? "Реєстрація..." : "Зареєструватися"}
                        </button>

                        <p className="text-center mt-3 small">
                            Вже маєте акаунт? <Link to="/login">Увійти</Link>
                        </p>
                    </form>
                </div>
            </div>
        </div>
    );
}
