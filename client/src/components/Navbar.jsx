import { useNavigate } from "react-router-dom";

function Navbar() {
    const navigate = useNavigate();

    const handleLogout = () => {
        localStorage.removeItem("accessToken");
        localStorage.removeItem("user");

        navigate("/login");
    };

    return (
        <header className="navbar">
            <div className="navbar-brand">
                AgencyHub
            </div>

            <button
                className="logout-button"
                onClick={handleLogout}
            >
                Logout
            </button>
        </header>
    );
}

export default Navbar;