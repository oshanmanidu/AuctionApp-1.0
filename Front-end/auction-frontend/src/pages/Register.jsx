// src/pages/Register.jsx
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../services/api';

export default function Register() {
    const [form, setForm] = useState({
        email: '',
        password: '',
        role: 'User'
    });
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');
    const [isLoading, setIsLoading] = useState(false);
    const navigate = useNavigate();

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        setSuccess('');
        setIsLoading(true);

        try {
            await api.post('/auth/register', form);
            setSuccess('Registration successful!');
            setTimeout(() => navigate('/'), 2000);
        } catch (err) {
            setError('Registration failed');
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="auth-container">
            <div className="auth-background">
                <div className="auth-pattern"></div>
            </div>
            
            <div className="auth-card animate-fade-in">
                <div className="auth-header">
                    <div className="auth-logo">
                        <div className="logo-icon">🏛️</div>
                        <h1 className="logo-title">AuctionHub</h1>
                    </div>
                    <p className="auth-subtitle">Join our premium auction community</p>
                </div>

                {error && (
                    <div className="alert alert-error animate-slide-in">
                        <span className="alert-icon">⚠️</span>
                        {error}
                    </div>
                )}

                {success ? (
                    <div className="alert alert-success animate-slide-in">
                        <span className="alert-icon">✅</span>
                        {success}
                    </div>
                ) : (
                    <form onSubmit={handleSubmit} className="auth-form">
                        <div className="form-group">
                            <label className="form-label" htmlFor="email">
                                Email
                            </label>
                            <div className="input-wrapper">
                                <span className="input-icon">📧</span>
                                <input
                                    id="email"
                                    type="email"
                                    className="form-input"
                                    placeholder="Enter your email"
                                    value={form.email}
                                    onChange={(e) => setForm({ ...form, email: e.target.value })}
                                    required
                                    disabled={isLoading}
                                />
                            </div>
                        </div>

                        <div className="form-group">
                            <label className="form-label" htmlFor="password">
                                Password
                            </label>
                            <div className="input-wrapper">
                                <span className="input-icon">🔒</span>
                                <input
                                    id="password"
                                    type="password"
                                    className="form-input"
                                    placeholder="Create a strong password"
                                    value={form.password}
                                    onChange={(e) => setForm({ ...form, password: e.target.value })}
                                    required
                                    disabled={isLoading}
                                />
                            </div>
                        </div>

                        <div className="form-group">
                            <label className="form-label" htmlFor="role">
                                Account Type
                            </label>
                            <div className="input-wrapper">
                                <span className="input-icon">👥</span>
                                <select
                                    id="role"
                                    className="form-input form-select"
                                    value={form.role}
                                    onChange={(e) => setForm({ ...form, role: e.target.value })}
                                    disabled={isLoading}
                                >
                                    <option value="User">User</option>
                                    <option value="Admin">Admin</option>
                                </select>
                            </div>
                        </div>

                        <button 
                            type="submit" 
                            className="btn btn-primary btn-lg auth-submit"
                            disabled={isLoading}
                        >
                            {isLoading ? (
                                <>
                                    <span className="loading"></span>
                                    Creating Account...
                                </>
                            ) : (
                                '📝 Create Account'
                            )}
                        </button>
                    </form>
                )}

                <div className="auth-footer">
                    <button 
                        className="auth-link"
                        onClick={() => navigate('/')}
                        disabled={isLoading}
                    >
                        ← Back to Sign In
                    </button>
                </div>
            </div>
        </div>
    );
}