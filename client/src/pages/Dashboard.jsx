import { useEffect, useState } from "react";
import api from "../services/api";

function Dashboard() {
    const [stats, setStats] = useState({
        clients: 0,
        projects: 0,
        tasks: 0
    });

    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        const loadDashboard = async () => {
            try {
                const [clientsResponse, projectsResponse, tasksResponse] =
                    await Promise.all([
                        api.get("/clients"),
                        api.get("/projects"),
                        api.get("/tasks")
                    ]);

                const clientsData = clientsResponse.data;
                const projectsData = projectsResponse.data;
                const tasksData = tasksResponse.data;

                setStats({
                    clients: Array.isArray(clientsData)
                        ? clientsData.length
                        : clientsData.items?.length ?? 0,

                    projects: Array.isArray(projectsData)
                        ? projectsData.length
                        : projectsData.items?.length ?? 0,

                    tasks: Array.isArray(tasksData)
                        ? tasksData.length
                        : tasksData.items?.length ?? 0
                });
            } catch (error) {
                console.error(error);
                setError("Unable to load dashboard data.");
            } finally {
                setLoading(false);
            }
        };

        loadDashboard();
    }, []);

    if (loading) {
        return <div className="page">Loading dashboard...</div>;
    }

    return (
        <div className="page">
            <h1>Dashboard</h1>

            {error && (
                <div className="error-message">
                    {error}
                </div>
            )}

            <div className="stats-grid">
                <div className="stat-card">
                    <h3>Clients</h3>
                    <p>{stats.clients}</p>
                </div>

                <div className="stat-card">
                    <h3>Projects</h3>
                    <p>{stats.projects}</p>
                </div>

                <div className="stat-card">
                    <h3>Tasks</h3>
                    <p>{stats.tasks}</p>
                </div>
            </div>
        </div>
    );
}

export default Dashboard;