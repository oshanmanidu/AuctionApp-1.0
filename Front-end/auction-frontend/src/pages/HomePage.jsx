// pages/HomePage.jsx
import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import '../styles/HomePage.css';

export default function HomePage() {
  const navigate = useNavigate();

  // Redirect if not logged in
  useEffect(() => {
    const token = localStorage.getItem('token');
    if (!token) {
      navigate('/');
    }
  }, [navigate]);

  const handleExplore = () => {
    navigate('/auctions');
  };

  return (
    <div className="homepage">
      {/* Navbar */}
      <nav className="navbar">
        <h2>BiddingBoom</h2>
        <ul>
          <li onClick={() => navigate('/auctions')}>Auctions</li>
          <li onClick={() => navigate('/profile')}>Profile</li>
          <li onClick={() => navigate('/admin')} style={{ color: 'var(--primary)' }}>
            Admin
          </li>
        </ul>
      </nav>

      {/* Hero Carousel */}
      <section className="hero">
        <div className="carousel">
          <img
            src="https://images.unsplash.com/photo-1525609004556-c46c7d6cf023?w=2000"
            alt="Luxury Watch"
            className="carousel-image active"
          />
          <img
            src="https://images.unsplash.com/photo-1502877338535-766e1452684a?w=2000"
            alt="Classic Car"
            className="carousel-image"
          />
          <img
            src="https://images.unsplash.com/photo-1526170375885-4d8ecf77b99f?w=2000"
            alt="Artwork"
            className="carousel-image"
          />
        </div>

        <div className="hero-content">
          <h1>Win Amazing Items</h1>
          <p>Join live auctions and bid on luxury watches, cars, art, and more!</p>
          <button onClick={handleExplore} className="btn-primary">
            Explore Auctions
          </button>
        </div>
      </section>

      {/* Features Section */}
      <section className="features">
        <div className="feature-card">
          <h3>🔥 Live Bidding</h3>
          <p>Real-time updates with instant bid confirmation.</p>
        </div>
        <div className="feature-card">
          <h3>⏱️ Timed Auctions</h3>
          <p>Every auction has a countdown — no extensions!</p>
        </div>
        <div className="feature-card">
          <h3>🏆 Winner Notified</h3>
          <p>Email sent instantly when you win an auction.</p>
        </div>
      </section>

      {/* Footer */}
      <footer className="footer">
        <p>&copy; 2025 BiddingBoom. All rights reserved.</p>
        <div className="social-links">
          <a href="https://facebook.com" target="_blank" rel="noopener noreferrer">📘</a>
          <a href="https://twitter.com" target="_blank" rel="noopener noreferrer">🐦</a>
          <a href="https://instagram.com" target="_blank" rel="noopener noreferrer">📸</a>
          <a href="https://linkedin.com" target="_blank" rel="noopener noreferrer">💼</a>
        </div>
      </footer>
    </div>
  );
}
