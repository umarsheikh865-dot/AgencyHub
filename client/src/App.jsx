import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";

// Components & Layouts
import ProtectedRoute from "./components/ProtectedRoute";
import Navbar from "./components/Navbar";
import Sidebar from "./components/Sidebar";

// Pages
import Login from "./pages/Login";
import Register from "./pages/Register";
import Dashboard from "./pages/Dashboard";
import Clients from "./pages/Clients";
import Projects from "./pages/Projects";
import Tasks from "./pages/Tasks";
import Profile from "./pages/Profile";
import AIAssistant from "./pages/AIAssistant";
import FaceVerification from "./pages/FaceVerification";

function ProtectedLayout({ children }) {
    return (
        <ProtectedRoute>
            <div className="app-layout">
                <Sidebar />

                <div className="main-area">
                    <Navbar />

                    <main>
                        {children}
                    </main>
                </div>
            </div>
        </ProtectedRoute>
    );
}

function App() {
    return (
        <BrowserRouter>
            <Routes>
                {/* Public Authentication Routes */}
                <Route path="/login" element={<Login />} />
                <Route path="/register" element={<Register />} />

                {/* Protected Dashboard & Feature Routes */}
                <Route
                    path="/dashboard"
                    element={
                        <ProtectedLayout>
                            <Dashboard />
                        </ProtectedLayout>
                    }
                />

                <Route
                    path="/clients"
                    element={
                        <ProtectedLayout>
                            <Clients />
                        </ProtectedLayout>
                    }
                />

                <Route
                    path="/projects"
                    element={
                        <ProtectedLayout>
                            <Projects />
                        </ProtectedLayout>
                    }
                />

                <Route
                    path="/tasks"
                    element={
                        <ProtectedLayout>
                            <Tasks />
                        </ProtectedLayout>
                    }
                />

                <Route
                    path="/profile"
                    element={
                        <ProtectedLayout>
                            <Profile />
                        </ProtectedLayout>
                    }
                />

                <Route
                    path="/ai-assistant"
                    element={
                        <ProtectedLayout>
                            <AIAssistant />
                        </ProtectedLayout>
                    }
                />

                <Route
                    path="/face-verification"
                    element={
                        <ProtectedLayout>
                            <FaceVerification />
                        </ProtectedLayout>
                    }
                />

                {/* Root & Catch-all Fallbacks */}
                <Route
                    path="/"
                    element={
                        <Navigate
                            to="/dashboard"
                            replace
                        />
                    }
                />

                <Route
                    path="*"
                    element={
                        <Navigate
                            to="/dashboard"
                            replace
                        />
                    }
                />
            </Routes>
        </BrowserRouter>
    );
}

export default App;