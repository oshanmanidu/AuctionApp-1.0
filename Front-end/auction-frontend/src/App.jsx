import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import Login from './pages/Login';
import Register from './pages/Register';
import HomePage from './pages/HomePage';
import AuctionList from './pages/AuctionList';
import AddAuction from './pages/AddAuction';
import AdminDashboard from './pages/AdminDashboard';
import ProtectedRoute from './components/ProtectedRoute';
import Navbar from './components/Navbar'; // Optional: Show navbar on most pages

export default function App() {
    return (
        <Router>
            <div style={{ minHeight: '100vh' }}>
                {/* Show Navbar on all pages except Login and Register */}
                <Routes>
                    <Route
                        path="/"
                        element={null} // No navbar on login
                    />
                    <Route
                        path="/register"
                        element={null} // No navbar on register
                    />
                    <Route
                        path="/home"
                        element={null} // No navbar on homepage (has its own navbar)
                    />
                    <Route
                        path="*"
                        element={<Navbar />} // Show navbar on all other routes
                    />
                </Routes>

                <main>
                    <Routes>
                        {/* Public Routes */}
                        <Route path="/" element={<Login />} />
                        <Route path="/register" element={<Register />} />

                        {/* Homepage Route */}
                        <Route
                            path="/home"
                            element={
                                <ProtectedRoute allowedRoles={['User', 'Admin']}>
                                    <HomePage />
                                </ProtectedRoute>
                            }
                        />

                        {/* User and Admin Routes */}
                        <Route
                            path="/auctions"
                            element={
                                <ProtectedRoute allowedRoles={['User', 'Admin']}>
                                    <AuctionList />
                                </ProtectedRoute>
                            }
                        />
                        <Route
                            path="/add-auction"
                            element={
                                <ProtectedRoute allowedRoles={['User']}>
                                    <AddAuction />
                                </ProtectedRoute>
                            }
                        />

                        {/* Admin Route */}
                        <Route
                            path="/admin"
                            element={
                                <ProtectedRoute allowedRoles={['Admin']}>
                                    <AdminDashboard />
                                </ProtectedRoute>
                            }
                        />

                        {/* Optional: Redirect any unknown route */}
                        <Route path="*" element={<Navigate to="/" />} />
                    </Routes>
                </main>
            </div>
        </Router>
    );
}