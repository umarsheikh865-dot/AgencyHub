import { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { register } from "../services/authService";

export default function Register() {
    const navigate = useNavigate();
    const [formData, setFormData] = useState({
        agencyName: "",
        agencySlug: "",
        fullName: "",
        email: "",
        password: ""
    });
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(false);

    const handleChange = (e) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError("");
        setLoading(true);

        try {
            await register(formData);
            alert("Registration successful! Please log in.");
            navigate("/login");
        } catch (err) {
            setError(err.response?.data?.message || "Registration failed. Please check your inputs.");
        } finally {
            setLoading(false);
        }
    };

    return (
        <div style={{ maxWidth: "450px", margin: "40px auto", padding: "30px", background: "#fff", boxShadow: "0 4px 12px rgba(0,0,0,0.1)", borderRadius: "8px" }}>
            <h2 style={{ textAlign: "center", color: "#333" }}>AgencyHub</h2>
            <h3 style={{ textAlign: "center", color: "#666", marginBottom: "20px" }}>Create New Agency Account</h3>

            {error && <div style={{ color: "#d9534f", background: "#fdf7f7", padding: "10px", borderRadius: "4px", marginBottom: "15px", border: "1px solid #f5c6cb" }}>{error}</div>}

            <form onSubmit={handleSubmit}>
                <div style={{ marginBottom: "12px" }}>
                    <label style={{ fontWeight: "500" }}>Agency Name</label><br />
                    <input type="text" name="agencyName" value={formData.agencyName} onChange={handleChange} required style={{ width: "100%", padding: "8px", boxSizing: "border-box", borderRadius: "4px", border: "1px solid #ccc" }} />
                </div>

                <div style={{ marginBottom: "12px" }}>
                    <label style={{ fontWeight: "500" }}>Agency Slug (e.g., my-agency)</label><br />
                    <input type="text" name="agencySlug" value={formData.agencySlug} onChange={handleChange} required style={{ width: "100%", padding: "8px", boxSizing: "border-box", borderRadius: "4px", border: "1px solid #ccc" }} />
                </div>

                <div style={{ marginBottom: "12px" }}>
                    <label style={{ fontWeight: "500" }}>Full Name</label><br />
                    <input type="text" name="fullName" value={formData.fullName} onChange={handleChange} required style={{ width: "100%", padding: "8px", boxSizing: "border-box", borderRadius: "4px", border: "1px solid #ccc" }} />
                </div>

                <div style={{ marginBottom: "12px" }}>
                    <label style={{ fontWeight: "500" }}>Email Address</label><br />
                    <input type="email" name="email" value={formData.email} onChange={handleChange} required style={{ width: "100%", padding: "8px", boxSizing: "border-box", borderRadius: "4px", border: "1px solid #ccc" }} />
                </div>

                <div style={{ marginBottom: "20px" }}>
                    <label style={{ fontWeight: "500" }}>Password</label><br />
                    <input type="password" name="password" value={formData.password} onChange={handleChange} required style={{ width: "100%", padding: "8px", boxSizing: "border-box", borderRadius: "4px", border: "1px solid #ccc" }} />
                </div>

                <button type="submit" disabled={loading} style={{ width: "100%", padding: "10px", backgroundColor: "#28a745", color: "white", border: "none", borderRadius: "4px", cursor: "pointer", fontWeight: "bold" }}>
                    {loading ? "Registering..." : "Register Agency"}
                </button>
            </form>

            <p style={{ marginTop: "15px", textAlign: "center" }}>
                Already have an account? <Link to="/login" style={{ color: "#007bff" }}>Sign In</Link>
            </p>
        </div>
    );
}