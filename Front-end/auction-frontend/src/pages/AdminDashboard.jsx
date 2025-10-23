// src/pages/AdminDashboard.jsx
import { useEffect, useState } from 'react';
import api from '../services/api';
import { useNavigate } from 'react-router-dom';
import StartBidModal from '../components/StartBidModal';

export default function AdminDashboard() {
    const [users, setUsers] = useState([]);
    const [auctions, setAuctions] = useState([]);
    const [loading, setLoading] = useState(true);
    const [deletingUser, setDeletingUser] = useState(null);
    const [startBidModal, setStartBidModal] = useState(null);
    const [endingBid, setEndingBid] = useState(null);
    const navigate = useNavigate();

    useEffect(() => {
        const fetchData = async () => {
            try {
                const [usersRes, auctionsRes] = await Promise.all([
                    api.get('/users'),
                    api.get('/auctionitems')
                ]);
                setUsers(usersRes.data);
                setAuctions(auctionsRes.data);
                setLoading(false);
            } catch (err) {
                if (err.response?.status === 401) {
                    navigate('/'); // Redirect if unauthorized
                } else {
                    alert('Failed to load data');
                }
            }
        };
        fetchData();
    }, [navigate]);

    const handleDelete = async (id) => {
        if (!window.confirm('Are you sure you want to delete this user? This action cannot be undone.')) return;

        setDeletingUser(id);
        try {
            await api.delete(`/users/${id}`);
            setUsers(users.filter(u => u.id !== id));
        } catch (err) {
            alert('Delete failed. Please try again.');
        } finally {
            setDeletingUser(null);
        }
    };

    const handleStartBid = (itemId) => {
        setStartBidModal(itemId);
    };

    const handleEndBid = async (itemId) => {
        if (!window.confirm("Are you sure you want to end bidding for this auction? This action cannot be undone.")) return;

        setEndingBid(itemId);
        try {
            await api.put(`/auctionitems/end-bid/${itemId}`);
            alert("Bidding ended successfully!");
            // Refresh the auctions
            const res = await api.get('/auctionitems');
            setAuctions(res.data);
        } catch (err) {
            alert("Failed to end bidding: " + (err.response?.data || err.message));
        } finally {
            setEndingBid(null);
        }
    };

    const handleStartBidSuccess = async () => {
        // Refresh the auctions
        const res = await api.get('/auctionitems');
        setAuctions(res.data);
    };

    if (loading) {
        return (
            <div className="page-container">
                <div className="loading-container">
                    <div className="loading loading-lg"></div>
                    <p className="loading-text">Loading admin dashboard...</p>
                </div>
            </div>
        );
    }

    return (
        <div className="page-container">
            <div className="page-header">
                <div className="header-content">
                    <h1 className="page-title">⚙️ Admin Dashboard</h1>
                    <p className="page-subtitle">Manage platform users and system settings</p>
                </div>
                <div className="header-stats">
                    <div className="stat-item">
                        <span className="stat-number">{users.length}</span>
                        <span className="stat-label">Total Users</span>
                    </div>
                    <div className="stat-item">
                        <span className="stat-number">
                            {users.filter(u => u.role === 'Admin').length}
                        </span>
                        <span className="stat-label">Admins</span>
                    </div>
                    <div className="stat-item">
                        <span className="stat-number">
                            {users.filter(u => u.role === 'User').length}
                        </span>
                        <span className="stat-label">Regular Users</span>
                    </div>
                    <div className="stat-item">
                        <span className="stat-number">{auctions.length}</span>
                        <span className="stat-label">Total Auctions</span>
                    </div>
                    <div className="stat-item">
                        <span className="stat-number">
                            {auctions.filter(a => a.bidStartTime && new Date() >= new Date(a.bidStartTime) && new Date() <= new Date(a.bidEndTime)).length}
                        </span>
                        <span className="stat-label">Active Auctions</span>
                    </div>
                </div>
            </div>

            <div className="admin-content">
                <div className="content-card card">
                    <div className="card-header">
                        <h2 className="card-title">👥 User Management</h2>
                        <p className="card-subtitle">View and manage registered users</p>
                    </div>
                    
                    <div className="card-body">
                        <div className="table-container">
                            <table className="table">
                                <thead>
                                    <tr>
                                        <th>ID</th>
                                        <th>Email</th>
                                        <th>Role</th>
                                        <th>Status</th>
                                        <th>Actions</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {users.map(user => (
                                        <tr key={user.id} className="user-row">
                                            <td className="user-id">#{user.id}</td>
                                            <td className="user-email">
                                                <div className="user-info">
                                                    <div className="user-avatar-small">
                                                        {user.email.charAt(0).toUpperCase()}
                                                    </div>
                                                    <span>{user.email}</span>
                                                </div>
                                            </td>
                                            <td>
                                                <span className={`role-badge role-${user.role.toLowerCase()}`}>
                                                    {user.role}
                                                </span>
                                            </td>
                                            <td>
                                                <span className="status-badge status-active">
                                                    Active
                                                </span>
                                            </td>
                                            <td>
                                                <button
                                                    onClick={() => handleDelete(user.id)}
                                                    className="btn btn-danger btn-sm"
                                                    disabled={deletingUser === user.id}
                                                >
                                                    {deletingUser === user.id ? (
                                                        <>
                                                            <span className="loading"></span>
                                                            Deleting...
                                                        </>
                                                    ) : (
                                                        '🗑️ Delete'
                                                    )}
                                                </button>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>

                {/* Auction Management Section */}
                <div className="content-card card">
                    <div className="card-header">
                        <h2 className="card-title">🏷️ Auction Management</h2>
                        <p className="card-subtitle">Manage auction bidding and status</p>
                    </div>
                    
                    <div className="card-body">
                        <div className="table-container">
                            <table className="table">
                                <thead>
                                    <tr>
                                        <th>ID</th>
                                        <th>Item Name</th>
                                        <th>Starting Price</th>
                                        <th>Bidding Status</th>
                                        <th>Actions</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {auctions.map(auction => {
                                        const hasBidWindow = auction.bidStartTime && auction.bidEndTime;
                                        const now = new Date();
                                        const canBid = hasBidWindow && now >= new Date(auction.bidStartTime) && now <= new Date(auction.bidEndTime);
                                        const biddingClosed = hasBidWindow && now > new Date(auction.bidEndTime);
                                        
                                        return (
                                            <tr key={auction.id} className="auction-row">
                                                <td className="auction-id">#{auction.id}</td>
                                                <td className="auction-name">
                                                    <div className="auction-info">
                                                        <span>{auction.name}</span>
                                                    </div>
                                                </td>
                                                <td>
                                                    <span className="price-value">${auction.startingPrice}</span>
                                                </td>
                                                <td>
                                                    {!auction.bidStartTime ? (
                                                        <span className="status-badge status-pending">Not Started</span>
                                                    ) : canBid ? (
                                                        <span className="status-badge status-active">Active</span>
                                                    ) : biddingClosed ? (
                                                        <span className="status-badge status-closed">Closed</span>
                                                    ) : (
                                                        <span className="status-badge status-scheduled">Scheduled</span>
                                                    )}
                                                </td>
                                                <td>
                                                    <div className="admin-auction-actions">
                                                        {!auction.bidStartTime && (
                                                            <button
                                                                onClick={() => handleStartBid(auction.id)}
                                                                className="btn btn-success btn-sm"
                                                            >
                                                                🚀 Start
                                                            </button>
                                                        )}
                                                        {canBid && (
                                                            <button
                                                                onClick={() => handleEndBid(auction.id)}
                                                                className="btn btn-warning btn-sm"
                                                                disabled={endingBid === auction.id}
                                                            >
                                                                {endingBid === auction.id ? (
                                                                    <>
                                                                        <span className="loading"></span>
                                                                        Ending...
                                                                    </>
                                                                ) : (
                                                                    '⏹️ End'
                                                                )}
                                                            </button>
                                                        )}
                                                    </div>
                                                </td>
                                            </tr>
                                        );
                                    })}
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>

                <div className="admin-actions">
                    <button
                        onClick={() => navigate('/auctions')}
                        className="btn btn-secondary"
                    >
                        ← Back to Auctions
                    </button>
                    
                    <div className="admin-info">
                        <div className="info-item">
                            <span className="info-icon">ℹ️</span>
                            <span className="info-text">
                                Only delete users when absolutely necessary. This action cannot be undone.
                            </span>
                        </div>
                    </div>
                </div>
            </div>

            {/* Start Bidding Modal */}
            {startBidModal && (
                <StartBidModal
                    itemId={startBidModal}
                    itemName={auctions.find(auction => auction.id === startBidModal)?.name || 'Unknown Item'}
                    onClose={() => setStartBidModal(null)}
                    onSuccess={handleStartBidSuccess}
                />
            )}
        </div>
    );
}