// src/auth/authService.js
import { UserManager, WebStorageStateStore } from "oidc-client-ts";

// CRA читає env змінні як process.env.REACT_APP_*
const authority = process.env.REACT_APP_OIDC_AUTHORITY; // напр. https://idp.example.com
const clientId = process.env.REACT_APP_OIDC_CLIENT_ID; // напр. spa-client
const redirectUri = `${window.location.origin}/auth/callback`;
const postLogoutRedirectUri = `${window.location.origin}/`;

const oidcSettings = {
    authority,
    client_id: clientId,
    redirect_uri: redirectUri,
    post_logout_redirect_uri: postLogoutRedirectUri,
    response_type: "code",
    scope: "openid profile email api offline_access",
    // токени краще зберігати не в localStorage → sessionStorage або in-memory
    userStore: new WebStorageStateStore({ store: window.sessionStorage }),
};

export const userManager = new UserManager(oidcSettings);

export async function login() {
    await userManager.signinRedirect();
}

export async function handleCallback() {
    const user = await userManager.signinRedirectCallback();
    return user;
}

export async function logout() {
    await userManager.signoutRedirect();
}

export async function getUser() {
    return await userManager.getUser();
}
