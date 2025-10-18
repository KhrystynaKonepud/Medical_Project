// src/auth/PrivateRoute.js
import { Navigate } from "react-router-dom";
import { useAuth } from "./useAuth";

export function PrivateRoute({ children, allowedRoles }) {
    const { isAuthenticated, hasRole, loading } = useAuth();

    if (loading) return <div>Loading...</div>;
    if (!isAuthenticated) return <Navigate to="/login" replace />;

    if (allowedRoles && !hasRole(allowedRoles)) {
        return <h3 style={{ textAlign: "center", marginTop: "2rem" }}>403 — Access denied</h3>;
    }
    return children;
}
