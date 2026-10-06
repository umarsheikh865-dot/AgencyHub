import { useEffect, useState } from "react";
import { getProfile } from "../services/authService";

function Profile() {
    const [profile, setProfile] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        const loadProfile = async () => {
            try {
                const data = await getProfile();

                setProfile(data);
            } catch (error) {
                console.error(error);
                setError("Unable to load profile.");
            } finally {
                setLoading(false);
            }
        };

        loadProfile();
    }, []);

    if (loading) {
        return <div className="page">Loading profile...</div>;
    }

    if (error) {
        return (
            <div className="page">
                <div className="error-message">
                    {error}
                </div>
            </div>
        );
    }

    return (
        <div className="page">
            <h1>Profile</h1>

            {profile && (
                <div className="profile-card">
                    <p>
                        <strong>First Name:</strong>{" "}
                        {profile.firstName}
                    </p>

                    <p>
                        <strong>Last Name:</strong>{" "}
                        {profile.lastName}
                    </p>

                    <p>
                        <strong>Email:</strong>{" "}
                        {profile.email}
                    </p>

                    <p>
                        <strong>Role:</strong>{" "}
                        {profile.role}
                    </p>

                    <p>
                        <strong>Agency:</strong>{" "}
                        {profile.tenantName}
                    </p>
                </div>
            )}
        </div>
    );
}

export default Profile;