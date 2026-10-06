import api from "./api";

export const login = async (data) => {
    const response = await api.post("/auth/login", data);
    return response.data;
};

export const register = async (data) => {
    const response = await api.post("/auth/register", data);
    return response.data;
};

// Added alias to fix the Register.jsx import error
export const registerUser = register;

export const getProfile = async () => {
    const response = await api.get("/auth/profile");
    return response.data;
};

export const logout = async () => {
    const response = await api.post("/auth/logout");
    return response.data;
};