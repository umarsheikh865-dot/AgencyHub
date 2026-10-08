import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import api from "../services/api";

function Dashboard() {
    const [stats, setStats] = useState({
        clients: 0,
        projects: 0,
        tasks: 0,
        completedTasks: 0,
        pendingTasks: 0,
        totalBudget: 0
    });

    const [recentProjects, setRecentProjects] = useState([]);
    const [recentTasks, setRecentTasks] = useState([]);

    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        const loadDashboard = async () => {
            try {
                setError("");

                const [
                    clientsResponse,
                    projectsResponse,
                    tasksResponse
                ] = await Promise.all([
                    api.get("/clients"),
                    api.get("/projects"),
                    api.get("/tasks", {
                        params: {
                            Page: 1,
                            PageSize: 10
                        }
                    })
                ]);

                const clientsData = clientsResponse.data;
                const projectsData = projectsResponse.data;
                const tasksData = tasksResponse.data;

                const clients = Array.isArray(clientsData)
                    ? clientsData
                    : clientsData?.items ?? [];

                const projects = Array.isArray(projectsData)
                    ? projectsData
                    : projectsData?.items ?? [];

                const tasks = Array.isArray(tasksData)
                    ? tasksData
                    : tasksData?.items ?? [];

                const completedTasks = tasks.filter(
                    task =>
                        String(task.status ?? "").toLowerCase() ===
                        "completed"
                ).length;

                const pendingTasks = tasks.filter(
                    task => {
                        const status = String(
                            task.status ?? ""
                        ).toLowerCase();

                        return (
                            status !== "completed" &&
                            status !== "cancelled"
                        );
                    }
                ).length;

                const totalBudget = projects.reduce(
                    (total, project) =>
                        total + Number(project.budget ?? 0),
                    0
                );

                setStats({
                    clients: clients.length,
                    projects: projects.length,
                    tasks: tasks.length,
                    completedTasks,
                    pendingTasks,
                    totalBudget
                });

                setRecentProjects(projects.slice(0, 5));
                setRecentTasks(tasks.slice(0, 5));
            } catch (err) {
                console.error("Dashboard error:", err);
                setError(
                    "Unable to load dashboard data. Please refresh the page."
                );
            } finally {
                setLoading(false);
            }
        };

        loadDashboard();
    }, []);

    if (loading) {
        return (
            <div className="page">
                <div className="dashboard-loading">
                    <div className="loading-spinner"></div>
                    <p>Loading your dashboard...</p>
                </div>
            </div>
        );
    }

    return (
        <div className="page dashboard-page">
            <div className="dashboard-header">
                <div>
                    <span className="dashboard-eyebrow">
                        AGENCY MANAGEMENT
                    </span>

                    <h1>Dashboard</h1>

                    <p>
                        Manage your agency, projects and team from one place.
                    </p>
                </div>

                <div className="dashboard-actions">
                    <Link
                        to="/clients"
                        className="dashboard-action-button"
                    >
                        + New Client
                    </Link>

                    <Link
                        to="/projects"
                        className="dashboard-action-button secondary"
                    >
                        + New Project
                    </Link>
                </div>
            </div>

            {error && (
                <div className="error-message">
                    {error}
                </div>
            )}

            <div className="stats-grid dashboard-stats">
                <div className="stat-card">
                    <div className="stat-card-top">
                        <span className="stat-icon">👥</span>
                        <span className="stat-label">
                            CLIENTS
                        </span>
                    </div>

                    <p className="stat-number">
                        {stats.clients}
                    </p>

                    <span className="stat-description">
                        Active agency clients
                    </span>
                </div>

                <div className="stat-card">
                    <div className="stat-card-top">
                        <span className="stat-icon">📁</span>
                        <span className="stat-label">
                            PROJECTS
                        </span>
                    </div>

                    <p className="stat-number">
                        {stats.projects}
                    </p>

                    <span className="stat-description">
                        Projects in your agency
                    </span>
                </div>

                <div className="stat-card">
                    <div className="stat-card-top">
                        <span className="stat-icon">✓</span>
                        <span className="stat-label">
                            TASKS
                        </span>
                    </div>

                    <p className="stat-number">
                        {stats.tasks}
                    </p>

                    <span className="stat-description">
                        Total project tasks
                    </span>
                </div>

                <div className="stat-card">
                    <div className="stat-card-top">
                        <span className="stat-icon">⚡</span>
                        <span className="stat-label">
                            PENDING
                        </span>
                    </div>

                    <p className="stat-number">
                        {stats.pendingTasks}
                    </p>

                    <span className="stat-description">
                        Tasks requiring attention
                    </span>
                </div>
            </div>

            <div className="dashboard-grid">
                <section className="dashboard-panel">
                    <div className="panel-header">
                        <div>
                            <h2>Recent Projects</h2>
                            <p>Your latest agency projects</p>
                        </div>

                        <Link to="/projects">
                            View all →
                        </Link>
                    </div>

                    {recentProjects.length === 0 ? (
                        <div className="empty-state">
                            <span>📁</span>
                            <p>No projects yet.</p>

                            <Link to="/projects">
                                Create your first project
                            </Link>
                        </div>
                    ) : (
                        <div className="dashboard-list">
                            {recentProjects.map((project, index) => (
                                <div
                                    className="dashboard-list-item"
                                    key={
                                        project.id ??
                                        project.projectId ??
                                        index
                                    }
                                >
                                    <div className="list-item-icon">
                                        📁
                                    </div>

                                    <div className="list-item-content">
                                        <strong>
                                            {project.name ??
                                                project.projectName ??
                                                "Unnamed Project"}
                                        </strong>

                                        <span>
                                            {project.status ??
                                                "No status"}
                                        </span>
                                    </div>

                                    <div className="list-item-value">
                                        {project.budget != null
                                            ? Number(
                                                project.budget
                                            ).toLocaleString()
                                            : "—"}
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </section>

                <section className="dashboard-panel">
                    <div className="panel-header">
                        <div>
                            <h2>Task Overview</h2>
                            <p>Current task progress</p>
                        </div>

                        <Link to="/tasks">
                            View all →
                        </Link>
                    </div>

                    <div className="task-overview">
                        <div className="task-overview-card">
                            <span className="overview-number">
                                {stats.completedTasks}
                            </span>

                            <span>
                                Completed
                            </span>
                        </div>

                        <div className="task-overview-card">
                            <span className="overview-number">
                                {stats.pendingTasks}
                            </span>

                            <span>
                                Pending
                            </span>
                        </div>
                    </div>

                    <div className="progress-section">
                        <div className="progress-header">
                            <span>Completion rate</span>

                            <strong>
                                {stats.tasks > 0
                                    ? Math.round(
                                        (stats.completedTasks /
                                            stats.tasks) *
                                        100
                                    )
                                    : 0}
                                %
                            </strong>
                        </div>

                        <div className="progress-bar">
                            <div
                                className="progress-bar-fill"
                                style={{
                                    width: `${stats.tasks > 0
                                            ? Math.min(
                                                100,
                                                Math.round(
                                                    (stats.completedTasks /
                                                        stats.tasks) *
                                                    100
                                                )
                                            )
                                            : 0
                                        }%`
                                }}
                            ></div>
                        </div>
                    </div>

                    <div className="budget-summary">
                        <span>Total project budget</span>

                        <strong>
                            {stats.totalBudget.toLocaleString()}
                        </strong>
                    </div>
                </section>
            </div>

            <section className="dashboard-panel recent-tasks-panel">
                <div className="panel-header">
                    <div>
                        <h2>Recent Tasks</h2>
                        <p>Latest tasks across your projects</p>
                    </div>

                    <Link to="/tasks">
                        Manage tasks →
                    </Link>
                </div>

                {recentTasks.length === 0 ? (
                    <div className="empty-state">
                        <span>✓</span>
                        <p>No tasks yet.</p>

                        <Link to="/tasks">
                            Create your first task
                        </Link>
                    </div>
                ) : (
                    <div className="dashboard-list">
                        {recentTasks.map((task, index) => (
                            <div
                                className="dashboard-list-item"
                                key={
                                    task.id ??
                                    task.taskId ??
                                    index
                                }
                            >
                                <div className="list-item-icon">
                                    ✓
                                </div>

                                <div className="list-item-content">
                                    <strong>
                                        {task.title ??
                                            task.taskTitle ??
                                            "Untitled Task"}
                                    </strong>

                                    <span>
                                        {task.status ??
                                            "No status"}
                                        {task.priority
                                            ? ` • ${task.priority}`
                                            : ""}
                                    </span>
                                </div>

                                <div className="list-item-value">
                                    {task.dueDate
                                        ? new Date(
                                            task.dueDate
                                        ).toLocaleDateString()
                                        : "No due date"}
                                </div>
                            </div>
                        ))}
                    </div>
                )}
            </section>
        </div>
    );
}

export default Dashboard;