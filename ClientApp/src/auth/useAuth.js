// src/auth/useAuth.js
import { useEffect, useState } from "react";
import { getUser } from "./authService";

function toArray(val) {
    if (!val) return [];
    if (Array.isArray(val)) return val;
    return [val];
}

// Підтримує різні назви клеймів: role / roles / groups
function extractRoles(profile) {
    if (!profile) return [];
    const r1 = toArray(profile.role);
    const r2 = toArray(profile.roles);
    const r3 = toArray(profile.groups);
    return Array.from(new Set([...r1, ...r2, ...r3])); // унікальні
}

export function useAuth() {
    const [user, setUser] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        getUser().then(u => {
            setUser(u);
            setLoading(false);
        });
    }, []);

    const isAuthenticated = !!user;
    const roles = extractRoles(user?.profile);

    const hasRole = (allowed) =>
        !allowed || allowed.some(r => roles.includes(r));

    return { user, isAuthenticated, roles, hasRole, loading };
}
