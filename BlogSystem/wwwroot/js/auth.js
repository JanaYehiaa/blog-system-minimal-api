export function getUserInfo() {
    const token = localStorage.getItem("token");
    if (!token) return null;

    try {
        const payload = token.split('.')[1];
        const json = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
        const decoded = JSON.parse(json);

        return {
            username: decoded["username"],
            role: decoded["role"] || decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"]
        };
    } catch (e) {
        console.error("Invalid token:", e);
        return null;
    }
}


export function isLoggedIn() {
    return !!localStorage.getItem("token");
}

export function logout() {
    localStorage.removeItem("token");
    location.reload();
}
