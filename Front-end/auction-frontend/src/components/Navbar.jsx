// src/components/Navbar.jsx
import { useNavigate } from 'react-router-dom';

export default function Navbar() {
    const role = localStorage.getItem('role');
    const email = localStorage.getItem('email');
    const navigate = useNavigate();

    const handleLogout = () => {
        localStorage.removeItem('token');
        localStorage.removeItem('role');
        localStorage.removeItem('email');
        navigate('/');
    };

    return (
        <nav className="navbar">
            <div className="navbar-container">
                <div className="navbar-brand" onClick={() => navigate('/auctions')}>
                    <div className="brand-icon">🏛️</div>
                    <div className="brand-text">
                        <h1 className="brand-title">AuctionHub</h1>
                        <span className="brand-subtitle">Premium Auctions</span>
                    </div>
                </div>

                <div className="navbar-menu">
                    {email ? (
                        <div className="navbar-user">
                            <div className="user-info">
                                <div className="user-avatar">
                                    {email.charAt(0).toUpperCase()}
                                </div>
                                <div className="user-details">
                                    <span>Welcome, <strong>{email}</strong> ({role})</span>
                                </div>
                            </div>

                            <div className="navbar-actions">
                                {/* Auction List - Available for both User and Admin */}
                                <button 
                                    className="btn btn-outline btn-sm" 
                                    onClick={() => navigate('/auctions')}
                                >
                                    {role === 'Admin' ? '🏷️ Manage Auctions' : '🏷️ Auctions'}
                                </button>

                                {/* Add Auction - Only for User role */}
                                {role === 'User' && (
                                    <button 
                                        className="btn btn-primary btn-sm" 
                                        onClick={() => navigate('/add-auction')}
                                    >
                                        ➕ Add Item
                                    </button>
                                )}

                                {/* Admin Dashboard - Only for Admin role */}
                                {role === 'Admin' && (
                                    <button 
                                        className="btn btn-warning btn-sm" 
                                        onClick={() => navigate('/admin')}
                                    >
                                        ⚙️ Admin Panel
                                    </button>
                                )}

                                <button 
                                    className="btn btn-danger btn-sm" 
                                    onClick={handleLogout}
                                >
                                    🚪 Logout
                                </button>
                            </div>
                        </div>
                    ) : (
                        <button 
                            className="btn btn-primary" 
                            onClick={() => navigate('/')}
                        >
                            🔑 Sign In
                        </button>
                    )}
                </div>
            </div>
        </nav>
    );
}