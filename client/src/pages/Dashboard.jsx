import { useState, useEffect } from "react";
import { useNavigate, Outlet, Link, useLocation } from "react-router-dom";
import api from "../services/api";

export default function Dashboard() {
    const navigate = useNavigate();
    const location = useLocation();
    const role = localStorage.getItem("role") || "User";
    const tenantId = localStorage.getItem("tenantId") || "N/A";

    // State for live counts
    const [counts, setCounts] = useState({
        clients: 0,
        projects: 0,
        tasks: 0
    });
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchDashboardData = async () => {
            try {
                // Fetch counts concurrently from your backend endpoints
                const [clientsRes, projectsRes, tasksRes] = await Promise.all([
                    api.get("/clients").catch(() => ({ data: [] })),
                    api.get("/projects").catch(() => ({ data: [] })),
                    api.get("/tasks").catch(() => ({ data: [] }))
                ]);

                setCounts({
                    clients: Array.isArray(clientsRes.data) ? clientsRes.data.length : (clientsRes.data?.items?.length || 0),
                    projects: Array.isArray(projectsRes.data) ? projectsRes.data.length : (projectsRes.data?.items?.length || 0),
                    tasks: Array.isArray(tasksRes.data) ? tasksRes.data.length : (tasksRes.data?.items?.length || 0)
                });
            } catch (err) {
                console.error("Failed to load dashboard metrics", err);
            } finally {
                setLoading(false);
            }
        };

        fetchDashboardData();
    }, []);

    const handleLogout = () => {
        localStorage.clear();
        navigate("/login");
    };

    const isHome = location.pathname === "/dashboard" || location.pathname === "/dashboard/";

    return (
        <div style={{ display: "flex", height: "100vh", fontFamily: "Segoe UI, Tahoma, Geneva, Verdana, sans-serif" }}>
            {/* Sidebar */}
            <div style={{ width: "250px", backgroundColor: "#343a40", color: "#fff", display: "flex", flexDirection: "column" }}>
                <div style={{ padding: "20px", fontSize: "20px", fontWeight: "bold", borderBottom: "1px solid #4f5962", textAlign: "center" }}>
                    AgencyHub 🚀
                </div>
                <nav style={{ flex: 1, padding: "20px" }}>
                    <ul style={{ listStyle: "none", padding: 0, margin: 0 }}>
                        <li style={{ marginBottom: "15px" }}><Link to="/dashboard" style={{ color: "#cfd8dc", textDecoration: "none" }}>📊 Dashboard</Link></li>
                        <li style={{ marginBottom: "15px" }}><Link to="/dashboard/clients" style={{ color: "#cfd8dc", textDecoration: "none" }}>👥 Clients</Link></li>
                        <li style={{ marginBottom: "15px" }}><Link to="/dashboard/projects" style={{ color: "#cfd8dc", textDecoration: "none" }}>📁 Projects</Link></li>
                        <li style={{ marginBottom: "15px" }}><Link to="/dashboard/tasks" style={{ color: "#cfd8dc", textDecoration: "none" }}>📋 Tasks</Link></li>
                        <li style={{ marginBottom: "15px" }}><Link to="/dashboard/profile" style={{ color: "#cfd8dc", textDecoration: "none" }}>👤 Profile</Link></li>
                    </ul>
                </nav>
                <div style={{ padding: "20px", borderTop: "1px solid #4f5962", fontSize: "12px", color: "#9e9e9e" }}>
                    Role: {role}
                </div>
            </div>

            {/* Main Content Area */}
            <div style={{ flex: 1, display: "flex", flexDirection: "column", backgroundColor: "#f8f9fa" }}>
                {/* Top Navbar */}
                <div style={{ height: "60px", background: "#fff", borderBottom: "1px solid #dee2e6", display: "flex", alignItems: "center", justifyContent: "space-between", padding: "0 30px" }}>
                    <span style={{ fontSize: "14px", color: "#6c757d" }}>Tenant ID: <b>{tenantId.substring(0, 8)}...</b></span>
                    <div>
                        <Link to="/dashboard/profile" style={{ marginRight: "15px", textDecoration: "none", color: "#007bff", fontWeight: "500" }}>Profile</Link>
                        <button onClick={handleLogout} style={{ padding: "6px 12px", backgroundColor: "#dc3545", color: "white", border: "none", borderRadius: "4px", cursor: "pointer" }}>
                            Logout
                        </button>
                    </div>
                </div>

                {/* Page View Body */}
                <div style={{ padding: "30px", overflowY: "auto", flex: 1 }}>
                    {isHome ? (
                        <div>
                            <h2>Welcome to AgencyHub</h2>
                            <p style={{ color: "#666", marginBottom: "30px" }}>Here is a quick overview of your multi-tenant agency performance.</p>

                            {loading ? (
                                <p>Loading metrics from backend...</p>
                            ) : (
                                <div style={{ display: "grid", gridTemplateColumns: "repeat(3, 1fr)", gap: "20px" }}>
                                    <div style={{ background: "#fff", padding: "20px", borderRadius: "8px", boxShadow: "0 2px 4px rgba(0,0,0,0.05)", borderLeft: "4px solid #007bff" }}>
                                        <h4 style={{ margin: "0 0 10px 0", color: "#666" }}>Clients</h4>
                                        <div style={{ fontSize: "28px", fontWeight: "bold", color: "#333" }}>{counts.clients}</div>
                                    </div>
                                    <div style={{ background: "#fff", padding: "20px", borderRadius: "8px", boxShadow: "0 2px 4px rgba(0,0,0,0.05)", borderLeft: "4px solid #28a745" }}>
                                        <h4 style={{ margin: "0 0 10px 0", color: "#666" }}>Projects</h4>
                                        <div style={{ fontSize: "28px", fontWeight: "bold", color: "#333" }}>{counts.projects}</div>
                                    </div>
                                    <div style={{ background: "#fff", padding: "20px", borderRadius: "8px", boxShadow: "0 2px 4px rgba(0,0,0,0.05)", borderLeft: "4px solid #ffc107" }}>
                                        <h4 style={{ margin: "0 0 10px 0", color: "#666" }}>Tasks</h4>
                                        <div style={{ fontSize: "28px", fontWeight: "bold", color: "#333" }}>{counts.tasks}</div>
                                    </div>
                                </div>
                            )}
                        </div>
                    ) : (
                        <Outlet />
                    )}
                </div>
            </div>
        </div>
    );
}