import { useEffect, useState } from "react";
import api from "../services/api";

const emptyForm = {
    clientId: "",
    name: "",
    description: "",
    status: "",
    budget: "",
    startDate: "",
    endDate: ""
};

function Projects() {
    const [projects, setProjects] = useState([]);
    const [clients, setClients] = useState([]);

    const [formData, setFormData] = useState(emptyForm);

    const [editingId, setEditingId] = useState(null);
    const [showForm, setShowForm] = useState(false);

    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);

    const [error, setError] = useState("");
    const [success, setSuccess] = useState("");

    // ================================
    // LOAD CLIENTS
    // ================================

    const loadClients = async () => {
        try {
            const response = await api.get("/clients");

            const data = response.data;

            const items = Array.isArray(data)
                ? data
                : data.items ?? data.data ?? [];

            setClients(items);
        } catch (error) {
            console.error(error);

            setError(
                error.response?.data?.message ||
                "Unable to load clients."
            );
        }
    };

    // ================================
    // LOAD PROJECTS
    // ================================

    const loadProjects = async () => {
        try {
            setLoading(true);
            setError("");

            const response = await api.get("/projects");

            const data = response.data;

            const items = Array.isArray(data)
                ? data
                : data.items ?? data.data ?? [];

            setProjects(items);
        } catch (error) {
            console.error(error);

            setError(
                error.response?.data?.message ||
                "Unable to load projects."
            );
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadClients();
        loadProjects();
    }, []);

    // ================================
    // FORM CHANGE
    // ================================

    const handleChange = (e) => {
        setFormData({
            ...formData,
            [e.target.name]: e.target.value
        });
    };

    // ================================
    // OPEN ADD
    // ================================

    const openAddForm = () => {
        setEditingId(null);

        setFormData(emptyForm);

        setError("");
        setSuccess("");

        setShowForm(true);
    };

    // ================================
    // OPEN EDIT
    // ================================

    const openEditForm = (project) => {
        setEditingId(project.id);

        setFormData({
            clientId: project.clientId ?? "",
            name: project.name ?? "",
            description: project.description ?? "",
            status: project.status ?? "",
            budget: project.budget ?? "",
            startDate: project.startDate
                ? project.startDate.substring(0, 10)
                : "",
            endDate: project.endDate
                ? project.endDate.substring(0, 10)
                : ""
        });

        setError("");
        setSuccess("");

        setShowForm(true);
    };

    // ================================
    // CLOSE FORM
    // ================================

    const closeForm = () => {
        setShowForm(false);
        setEditingId(null);
        setFormData(emptyForm);
    };

    // ================================
    // CREATE / UPDATE PROJECT
    // ================================

    const handleSubmit = async (e) => {
        e.preventDefault();

        if (!formData.clientId) {
            setError("Please select a client.");
            return;
        }

        try {
            setSaving(true);
            setError("");
            setSuccess("");

            const payload = {
                clientId: formData.clientId,
                name: formData.name,
                description: formData.description,
                status: formData.status,
                budget: formData.budget
                    ? Number(formData.budget)
                    : null,
                startDate: formData.startDate || null,
                endDate: formData.endDate || null
            };

            if (editingId) {
                await api.put(
                    `/projects/${editingId}`,
                    payload
                );

                setSuccess(
                    "Project updated successfully."
                );
            } else {
                await api.post(
                    "/projects",
                    payload
                );

                setSuccess(
                    "Project created successfully."
                );
            }

            closeForm();

            await loadProjects();

        } catch (error) {
            console.error(error);

            const validationErrors =
                error.response?.data?.errors;

            if (validationErrors) {
                setError(
                    Object.values(validationErrors)
                        .flat()
                        .join(" ")
                );
            } else {
                setError(
                    error.response?.data?.message ||
                    "Unable to save project."
                );
            }

        } finally {
            setSaving(false);
        }
    };

    // ================================
    // DELETE PROJECT
    // ================================

    const handleDelete = async (id) => {
        const confirmed = window.confirm(
            "Are you sure you want to delete this project?"
        );

        if (!confirmed) {
            return;
        }

        try {
            setError("");
            setSuccess("");

            await api.delete(
                `/projects/${id}`
            );

            setSuccess(
                "Project deleted successfully."
            );

            await loadProjects();

        } catch (error) {
            console.error(error);

            setError(
                error.response?.data?.message ||
                "Unable to delete project."
            );
        }
    };

    // ================================
    // GET CLIENT NAME
    // ================================

    const getClientName = (clientId) => {
        const client = clients.find(
            (item) => item.id === clientId
        );

        return client
            ? client.companyName
            : clientId;
    };

    return (
        <div className="page">

            {/* HEADER */}

            <div className="page-header">

                <div>
                    <h1>Projects</h1>

                    <p>
                        Manage agency projects.
                    </p>
                </div>

                <button
                    className="primary-button"
                    onClick={openAddForm}
                >
                    + Add Project
                </button>

            </div>

            {/* ERRORS */}

            {error && (
                <div className="error-message">
                    {error}
                </div>
            )}

            {/* SUCCESS */}

            {success && (
                <div className="success-message">
                    {success}
                </div>
            )}

            {/* FORM */}

            {showForm && (
                <div className="form-card">

                    <h2>
                        {editingId
                            ? "Edit Project"
                            : "Add New Project"}
                    </h2>

                    <form onSubmit={handleSubmit}>

                        <div className="form-grid">

                            {/* CLIENT */}

                            <div className="form-group full-width">

                                <label htmlFor="clientId">
                                    Client
                                </label>

                                <select
                                    id="clientId"
                                    name="clientId"
                                    value={
                                        formData.clientId
                                    }
                                    onChange={
                                        handleChange
                                    }
                                    required
                                >

                                    <option value="">
                                        Select a client
                                    </option>

                                    {clients.map(
                                        (client) => (
                                            <option
                                                key={
                                                    client.id
                                                }
                                                value={
                                                    client.id
                                                }
                                            >
                                                {
                                                    client.companyName
                                                }
                                            </option>
                                        )
                                    )}

                                </select>

                                {clients.length === 0 && (
                                    <small>
                                        No clients found.
                                        Create a client
                                        first.
                                    </small>
                                )}

                            </div>

                            {/* NAME */}

                            <div className="form-group">

                                <label htmlFor="name">
                                    Project Name
                                </label>

                                <input
                                    id="name"
                                    type="text"
                                    name="name"
                                    value={
                                        formData.name
                                    }
                                    onChange={
                                        handleChange
                                    }
                                    required
                                />

                            </div>

                            {/* STATUS */}

                            <div className="form-group">

                                <label htmlFor="status">
                                    Status
                                </label>

                                <input
                                    id="status"
                                    type="text"
                                    name="status"
                                    value={
                                        formData.status
                                    }
                                    onChange={
                                        handleChange
                                    }
                                    placeholder="InProgress"
                                />

                            </div>

                            {/* BUDGET */}

                            <div className="form-group">

                                <label htmlFor="budget">
                                    Budget
                                </label>

                                <input
                                    id="budget"
                                    type="number"
                                    name="budget"
                                    value={
                                        formData.budget
                                    }
                                    onChange={
                                        handleChange
                                    }
                                    min="0"
                                    step="0.01"
                                />

                            </div>

                            {/* START DATE */}

                            <div className="form-group">

                                <label htmlFor="startDate">
                                    Start Date
                                </label>

                                <input
                                    id="startDate"
                                    type="date"
                                    name="startDate"
                                    value={
                                        formData.startDate
                                    }
                                    onChange={
                                        handleChange
                                    }
                                />

                            </div>

                            {/* END DATE */}

                            <div className="form-group">

                                <label htmlFor="endDate">
                                    End Date
                                </label>

                                <input
                                    id="endDate"
                                    type="date"
                                    name="endDate"
                                    value={
                                        formData.endDate
                                    }
                                    onChange={
                                        handleChange
                                    }
                                />

                            </div>

                            {/* DESCRIPTION */}

                            <div className="form-group full-width">

                                <label htmlFor="description">
                                    Description
                                </label>

                                <textarea
                                    id="description"
                                    name="description"
                                    value={
                                        formData.description
                                    }
                                    onChange={
                                        handleChange
                                    }
                                    rows="4"
                                />

                            </div>

                        </div>

                        {/* BUTTONS */}

                        <div className="form-actions">

                            <button
                                type="submit"
                                className="primary-button"
                                disabled={
                                    saving ||
                                    clients.length === 0
                                }
                            >
                                {saving
                                    ? "Saving..."
                                    : editingId
                                        ? "Update Project"
                                        : "Create Project"}
                            </button>

                            <button
                                type="button"
                                className="secondary-button"
                                onClick={closeForm}
                            >
                                Cancel
                            </button>

                        </div>

                    </form>

                </div>
            )}

            {/* PROJECT LIST */}

            {loading ? (

                <p>
                    Loading projects...
                </p>

            ) : projects.length === 0 ? (

                <div className="empty-state">

                    <h3>
                        No projects yet
                    </h3>

                    <p>
                        Create your first project.
                    </p>

                </div>

            ) : (

                <div className="cards-grid">

                    {projects.map(
                        (project) => (

                            <div
                                className="project-card"
                                key={project.id}
                            >

                                <h3>
                                    {project.name}
                                </h3>

                                <p>
                                    {
                                        project.description ||
                                        "No description"
                                    }
                                </p>

                                <p>
                                    <strong>
                                        Client:
                                    </strong>{" "}
                                    {
                                        getClientName(
                                            project.clientId
                                        )
                                    }
                                </p>

                                <p>
                                    <strong>
                                        Status:
                                    </strong>{" "}
                                    {
                                        project.status
                                    }
                                </p>

                                <p>
                                    <strong>
                                        Budget:
                                    </strong>{" "}
                                    {
                                        project.budget ??
                                        "N/A"
                                    }
                                </p>

                                <div className="card-actions">

                                    <button
                                        className="edit-button"
                                        onClick={() =>
                                            openEditForm(
                                                project
                                            )
                                        }
                                    >
                                        Edit
                                    </button>

                                    <button
                                        className="delete-button"
                                        onClick={() =>
                                            handleDelete(
                                                project.id
                                            )
                                        }
                                    >
                                        Delete
                                    </button>

                                </div>

                            </div>

                        )
                    )}

                </div>

            )}

        </div>
    );
}

export default Projects;