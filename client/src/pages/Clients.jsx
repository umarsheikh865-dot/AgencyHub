import { useEffect, useState } from "react";
import api from "../services/api";

const emptyForm = {
    companyName: "",
    contactPerson: "",
    email: "",
    phone: "",
    address: "",
    industry: "",
    status: "",
    notes: ""
};

function Clients() {
    const [clients, setClients] = useState([]);
    const [formData, setFormData] = useState(emptyForm);

    const [editingId, setEditingId] = useState(null);
    const [showForm, setShowForm] = useState(false);

    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);

    const [error, setError] = useState("");
    const [success, setSuccess] = useState("");

    const loadClients = async () => {
        try {
            setLoading(true);
            setError("");

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
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadClients();
    }, []);

    const handleChange = (e) => {
        setFormData({
            ...formData,
            [e.target.name]: e.target.value
        });
    };

    const openAddForm = () => {
        setEditingId(null);
        setFormData(emptyForm);
        setError("");
        setSuccess("");
        setShowForm(true);
    };

    const openEditForm = (client) => {
        setEditingId(client.id);

        setFormData({
            companyName: client.companyName ?? "",
            contactPerson: client.contactPerson ?? "",
            email: client.email ?? "",
            phone: client.phone ?? "",
            address: client.address ?? "",
            industry: client.industry ?? "",
            status: client.status ?? "",
            notes: client.notes ?? ""
        });

        setError("");
        setSuccess("");
        setShowForm(true);
    };

    const closeForm = () => {
        setShowForm(false);
        setEditingId(null);
        setFormData(emptyForm);
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        try {
            setSaving(true);
            setError("");
            setSuccess("");

            if (editingId) {
                await api.put(`/clients/${editingId}`, formData);
            } else {
                await api.post("/clients", formData);
            }

            closeForm();
            await loadClients();

            setSuccess(
                editingId
                    ? "Client updated successfully."
                    : "Client created successfully."
            );
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
                    "Unable to save client."
                );
            }
        } finally {
            setSaving(false);
        }
    };

    const handleDelete = async (id) => {
        const confirmed = window.confirm(
            "Are you sure you want to delete this client?"
        );

        if (!confirmed) {
            return;
        }

        try {
            setError("");
            setSuccess("");

            await api.delete(`/clients/${id}`);

            setSuccess("Client deleted successfully.");

            await loadClients();
        } catch (error) {
            console.error(error);

            setError(
                error.response?.data?.message ||
                "Unable to delete client."
            );
        }
    };

    return (
        <div className="page">

            <div className="page-header">
                <div>
                    <h1>Clients</h1>
                    <p>Manage your agency clients.</p>
                </div>

                <button
                    className="primary-button"
                    onClick={openAddForm}
                >
                    + Add Client
                </button>
            </div>

            {error && (
                <div className="error-message">
                    {error}
                </div>
            )}

            {success && (
                <div className="success-message">
                    {success}
                </div>
            )}

            {showForm && (
                <div className="form-card">

                    <h2>
                        {editingId
                            ? "Edit Client"
                            : "Add New Client"}
                    </h2>

                    <form onSubmit={handleSubmit}>

                        <div className="form-grid">

                            <div className="form-group">
                                <label>Company Name</label>
                                <input
                                    type="text"
                                    name="companyName"
                                    value={formData.companyName}
                                    onChange={handleChange}
                                    required
                                />
                            </div>

                            <div className="form-group">
                                <label>Contact Person</label>
                                <input
                                    type="text"
                                    name="contactPerson"
                                    value={formData.contactPerson}
                                    onChange={handleChange}
                                    required
                                />
                            </div>

                            <div className="form-group">
                                <label>Email</label>
                                <input
                                    type="email"
                                    name="email"
                                    value={formData.email}
                                    onChange={handleChange}
                                />
                            </div>

                            <div className="form-group">
                                <label>Phone</label>
                                <input
                                    type="text"
                                    name="phone"
                                    value={formData.phone}
                                    onChange={handleChange}
                                />
                            </div>

                            <div className="form-group">
                                <label>Industry</label>
                                <input
                                    type="text"
                                    name="industry"
                                    value={formData.industry}
                                    onChange={handleChange}
                                />
                            </div>

                            <div className="form-group">
                                <label>Status</label>
                                <input
                                    type="text"
                                    name="status"
                                    value={formData.status}
                                    onChange={handleChange}
                                    placeholder="Active"
                                />
                            </div>

                            <div className="form-group full-width">
                                <label>Address</label>
                                <input
                                    type="text"
                                    name="address"
                                    value={formData.address}
                                    onChange={handleChange}
                                />
                            </div>

                            <div className="form-group full-width">
                                <label>Notes</label>
                                <textarea
                                    name="notes"
                                    value={formData.notes}
                                    onChange={handleChange}
                                    rows="4"
                                />
                            </div>

                        </div>

                        <div className="form-actions">

                            <button
                                type="submit"
                                className="primary-button"
                                disabled={saving}
                            >
                                {saving
                                    ? "Saving..."
                                    : editingId
                                        ? "Update Client"
                                        : "Create Client"}
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

            {loading ? (
                <p>Loading clients...</p>
            ) : clients.length === 0 ? (
                <div className="empty-state">
                    <h3>No clients yet</h3>
                    <p>Add your first client to get started.</p>
                </div>
            ) : (
                <div className="table-container">

                    <table>

                        <thead>
                            <tr>
                                <th>Company</th>
                                <th>Contact</th>
                                <th>Email</th>
                                <th>Phone</th>
                                <th>Industry</th>
                                <th>Status</th>
                                <th>Actions</th>
                            </tr>
                        </thead>

                        <tbody>

                            {clients.map((client) => (
                                <tr key={client.id}>

                                    <td>
                                        {client.companyName}
                                    </td>

                                    <td>
                                        {client.contactPerson}
                                    </td>

                                    <td>
                                        {client.email}
                                    </td>

                                    <td>
                                        {client.phone}
                                    </td>

                                    <td>
                                        {client.industry}
                                    </td>

                                    <td>
                                        {client.status}
                                    </td>

                                    <td>

                                        <button
                                            className="edit-button"
                                            onClick={() =>
                                                openEditForm(client)
                                            }
                                        >
                                            Edit
                                        </button>

                                        <button
                                            className="delete-button"
                                            onClick={() =>
                                                handleDelete(client.id)
                                            }
                                        >
                                            Delete
                                        </button>

                                    </td>

                                </tr>
                            ))}

                        </tbody>

                    </table>

                </div>
            )}

        </div>
    );
}

export default Clients;