import { NavLink } from "react-router-dom";

function Sidebar() {
    return (
        <aside className="sidebar">
            <h2>AgencyHub</h2>

            <nav>
                <NavLink to="/dashboard">
                    Dashboard
                </NavLink>

                <NavLink to="/clients">
                    Clients
                </NavLink>

                <NavLink to="/projects">
                    Projects
                </NavLink>

                <NavLink to="/tasks">
                    Tasks
                </NavLink>

                <NavLink to="/profile">
                    Profile
                </NavLink>
            </nav>
        </aside>
    );
}

export default Sidebar;