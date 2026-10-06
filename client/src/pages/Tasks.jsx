import { useEffect, useState } from "react";
import api from "../services/api";

const emptyForm = {
    projectId: "",
    assignedToUserId: "",
    title: "",
    description: "",
    status: "",
    priority: "",
    dueDate: ""
};

function Tasks() {
    const [tasks, setTasks] = useState([]);
    const [projects, setProjects] = useState([]);

    const [formData, setFormData] = useState(emptyForm);

    const [editingId, setEditingId] = useState(null);
    const [showForm, setShowForm] = useState(false);

    const [search, setSearch] = useState("");
    const [statusFilter, setStatusFilter] = useState("");
    const [priorityFilter, setPriorityFilter] = useState("");

    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);

    const [error, setError] = useState("");
    const [success, setSuccess] = useState("");

    // ================================
    // LOAD PROJECTS
    // ================================

    const loadProjects = async () => {
        try {
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
        }
    };

    // ================================
    // LOAD TASKS
    // ================================

    const loadTasks = async () => {
        try {
            setLoading(true);
            setError("");

            const params = {
                Page: 1,
                PageSize: 100
            };

            if (search.trim()) {
                params.Search = search.trim();
            }

            if (statusFilter.trim()) {
                params.Status = statusFilter.trim();
            }

            if (priorityFilter.trim()) {
                params.Priority = priorityFilter.trim();
            }

            const response = await api.get("/tasks", {
                params
            });

            const data = response.data;

            const items = Array.isArray(data)
                ? data
                : data.items ?? data.data ?? [];

            setTasks(items);
        } catch (error) {
            console.error(error);

            setError(
                error.response?.data?.message ||
                "Unable to load tasks."
            );
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadProjects();
        loadTasks();
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
    // ADD TASK
    // ================================

    const openAddForm = () => {
        setEditingId(null);
        setFormData(emptyForm);

        setError("");
        setSuccess("");

        setShowForm(true);
    };

    // ================================
    // EDIT TASK
    // ================================

    const openEditForm = (task) => {
        setEditingId(task.id);

        setFormData({
            projectId: task.projectId ?? "",
            assignedToUserId:
                task.assignedToUserId ?? "",
            title: task.title ?? "",
            description: task.description ?? "",
            status: task.status ?? "",
            priority: task.priority ?? "",
            dueDate: task.dueDate
                ? task.dueDate.substring(0, 10)
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
    // CREATE / UPDATE
    // ================================

    const handleSubmit = async (e) => {
        e.preventDefault();

        if (!formData.projectId) {
            setError("Please select a project.");
            return;
        }

        if (!formData.title.trim()) {
            setError("Task title is required.");
            return;
        }

        try {
            setSaving(true);
            setError("");
            setSuccess("");

            const payload = {
                projectId: formData.projectId,
                assignedToUserId:
                    formData.assignedToUserId || null,
                title: formData.title,
                description: formData.description,
                status: formData.status,
                priority: formData.priority,
                dueDate: formData.dueDate || null
            };

            if (editingId) {
                await api.put(
                    `/tasks/${editingId}`,
                    payload
                );

                setSuccess(
                    "Task updated successfully."
                );
            } else {
                await api.post(
                    "/tasks",
                    payload
                );

                setSuccess(
                    "Task created successfully."
                );
            }

            closeForm();

            await loadTasks();

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
                    "Unable to save task."
                );
            }
        } finally {
            setSaving(false);
        }
    };

    // ================================
    // DELETE
    // ================================

    const handleDelete = async (id) => {
        const confirmed = window.confirm(
            "Are you sure you want to delete this task?"
        );

        if (!confirmed) {
            return;
        }

        try {
            setError("");
            setSuccess("");

            await api.delete(
                `/tasks/${id}`
            );

            setSuccess(
                "Task deleted successfully."
            );

            await loadTasks();

        } catch (error) {
            console.error(error);

            setError(
                error.response?.data?.message ||
                "Unable to delete task."
            );
        }
    };

    // ================================
    // SEARCH / FILTER
    // ================================

    const handleFilter = (e) => {
        e.preventDefault();

        loadTasks();
    };

    const clearFilters = () => {
        setSearch("");
        setStatusFilter("");
        setPriorityFilter("");

        setTimeout(() => {
            loadTasks();
        }, 0);
    };

    // ================================
    // PROJECT NAME
    // ================================

    const getProjectName = (projectId) => {
        const project = projects.find(
            (item) => item.id === projectId
        );

        return project
            ? project.name
            : projectId;
    };

    return (
        <div className="page">

            {/* HEADER */}

            <div className="page-header">

                <div>
                    <h1>Tasks</h1>

                    <p>
                        Manage project tasks and assignments.
                    </p>
                </div>

                <button
                    className="primary-button"
                    onClick={openAddForm}
                >
                    + Add Task
                </button>

            </div>

            {/* ERROR */}

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

            {/* FILTERS */}

            <div className="filter-card">

                <form onSubmit={handleFilter}>

                    <div className="filter-grid">

                        <div className="form-group">

                            <label htmlFor="search">
                                Search
                            </label>

                            <input
                                id="search"
                                type="text"
                                value={search}
                                onChange={(e) =>
                                    setSearch(
                                        e.target.value
                                    )
                                }
                                placeholder="Search tasks..."
                            />

                        </div>

                        <div className="form-group">

                            <label htmlFor="statusFilter">
                                Status
                            </label>

                            <input
                                id="statusFilter"
                                type="text"
                                value={statusFilter}
                                onChange={(e) =>
                                    setStatusFilter(
                                        e.target.value
                                    )
                                }
                                placeholder="Pending"
                            />

                        </div>

                        <div className="form-group">

                            <label htmlFor="priorityFilter">
                                Priority
                            </label>

                            <input
                                id="priorityFilter"
                                type="text"
                                value={priorityFilter}
                                onChange={(e) =>
                                    setPriorityFilter(
                                        e.target.value
                                    )
                                }
                                placeholder="High"
                            />

                        </div>

                    </div>

                    <div className="form-actions">

                        <button
                            type="submit"
                            className="primary-button"
                        >
                            Search
                        </button>

                        <button
                            type="button"
                            className="secondary-button"
                            onClick={clearFilters}
                        >
                            Clear
                        </button>

                    </div>

                </form>

            </div>

            {/* TASK FORM */}

            {showForm && (
                <div className="form-card">

                    <h2>
                        {editingId
                            ? "Edit Task"
                            : "Add New Task"}
                    </h2>

                    <form onSubmit={handleSubmit}>

                        <div className="form-grid">

                            {/* PROJECT */}

                            <div className="form-group full-width">

                                <label htmlFor="projectId">
                                    Project
                                </label>

                                <select
                                    id="projectId"
                                    name="projectId"
                                    value={
                                        formData.projectId
                                    }
                                    onChange={
                                        handleChange
                                    }
                                    required
                                >

                                    <option value="">
                                        Select a project
                                    </option>

                                    {projects.map(
                                        (project) => (
                                            <option
                                                key={
                                                    project.id
                                                }
                                                value={
                                                    project.id
                                                }
                                            >
                                                {
                                                    project.name
                                                }
                                            </option>
                                        )
                                    )}

                                </select>

                            </div>

                            {/* USER */}

                            <div className="form-group full-width">

                                <label htmlFor="assignedToUserId">
                                    Assigned User ID
                                </label>

                                <input
                                    id="assignedToUserId"
                                    type="text"
                                    name="assignedToUserId"
                                    value={
                                        formData.assignedToUserId
                                    }
                                    onChange={
                                        handleChange
                                    }
                                    placeholder="Optional user GUID"
                                />

                                <small>
                                    Leave empty if the task
                                    is not assigned yet.
                                </small>

                            </div>

                            {/* TITLE */}

                            <div className="form-group full-width">

                                <label htmlFor="title">
                                    Task Title
                                </label>

                                <input
                                    id="title"
                                    type="text"
                                    name="title"
                                    value={
                                        formData.title
                                    }
                                    onChange={
                                        handleChange
                                    }
                                    required
                                />

                            </div>

                            {/* STATUS */}

                            <div className="form-group">

                                <label htmlFor="taskStatus">
                                    Status
                                </label>

                                <input
                                    id="taskStatus"
                                    type="text"
                                    name="status"
                                    value={
                                        formData.status
                                    }
                                    onChange={
                                        handleChange
                                    }
                                    placeholder="Pending"
                                />

                            </div>

                            {/* PRIORITY */}

                            <div className="form-group">

                                <label htmlFor="priority">
                                    Priority
                                </label>

                                <input
                                    id="priority"
                                    type="text"
                                    name="priority"
                                    value={
                                        formData.priority
                                    }
                                    onChange={
                                        handleChange
                                    }
                                    placeholder="High"
                                />

                            </div>

                            {/* DUE DATE */}

                            <div className="form-group">

                                <label htmlFor="dueDate">
                                    Due Date
                                </label>

                                <input
                                    id="dueDate"
                                    type="date"
                                    name="dueDate"
                                    value={
                                        formData.dueDate
                                    }
                                    onChange={
                                        handleChange
                                    }
                                />

                            </div>

                            {/* DESCRIPTION */}

                            <div className="form-group full-width">

                                <label htmlFor="taskDescription">
                                    Description
                                </label>

                                <textarea
                                    id="taskDescription"
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
                                    projects.length === 0
                                }
                            >
                                {saving
                                    ? "Saving..."
                                    : editingId
                                        ? "Update Task"
                                        : "Create Task"}
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

            {/* TASK LIST */}

            {loading ? (

                <p>
                    Loading tasks...
                </p>

            ) : tasks.length === 0 ? (

                <div className="empty-state">

                    <h3>
                        No tasks found
                    </h3>

                    <p>
                        Create a task or change your filters.
                    </p>

                </div>

            ) : (

                <div className="table-container">

                    <table>

                        <thead>

                            <tr>
                                <th>Title</th>
                                <th>Project</th>
                                <th>Status</th>
                                <th>Priority</th>
                                <th>Due Date</th>
                                <th>Actions</th>
                            </tr>

                        </thead>

                        <tbody>

                            {tasks.map(
                                (task) => (

                                    <tr
                                        key={task.id}
                                    >

                                        <td>
                                            {
                                                task.title
                                            }
                                        </td>

                                        <td>
                                            {
                                                getProjectName(
                                                    task.projectId
                                                )
                                            }
                                        </td>

                                        <td>
                                            {
                                                task.status
                                            }
                                        </td>

                                        <td>
                                            {
                                                task.priority
                                            }
                                        </td>

                                        <td>
                                            {
                                                task.dueDate
                                                    ? new Date(
                                                        task.dueDate
                                                    ).toLocaleDateString()
                                                    : "No date"
                                            }
                                        </td>

                                        <td>

                                            <button
                                                className="edit-button"
                                                onClick={() =>
                                                    openEditForm(
                                                        task
                                                    )
                                                }
                                            >
                                                Edit
                                            </button>

                                            <button
                                                className="delete-button"
                                                onClick={() =>
                                                    handleDelete(
                                                        task.id
                                                    )
                                                }
                                            >
                                                Delete
                                            </button>

                                        </td>

                                    </tr>

                                )
                            )}

                        </tbody>

                    </table>

                </div>

            )}

        </div>
    );
}

export default Tasks;