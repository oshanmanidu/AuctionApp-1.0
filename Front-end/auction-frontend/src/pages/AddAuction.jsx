import { useState } from 'react';
import api from '../services/api';
import { useNavigate } from 'react-router-dom';

export default function AddAuction() {
    const [form, setForm] = useState({ name: '', description: '', startingPrice: '' });
    const [image, setImage] = useState(null);
    const [isLoading, setIsLoading] = useState(false);
    const [imagePreview, setImagePreview] = useState(null);
    const navigate = useNavigate();

    const handleImageChange = (e) => {
        const file = e.target.files[0];
        if (file) {
            setImage(file);
            const reader = new FileReader();
            reader.onloadend = () => {
                setImagePreview(reader.result);
            };
            reader.readAsDataURL(file);
        }
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setIsLoading(true);
        
        const formData = new FormData();
        formData.append('Name', form.name);
        formData.append('Description', form.description);
        formData.append('StartingPrice', form.startingPrice);
        if (image) formData.append('ImageFile', image);

        try {
            await api.post('/auctionitems', formData, {
                headers: { 'Content-Type': 'multipart/form-data' }
            });
            alert('Auction item added successfully!');
            navigate('/auctions');
        } catch (err) {
            console.error(err);
            alert('Failed to add auction item. Please try again.');
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="page-container">
            <div className="page-header">
                <div className="header-content">
                    <h1 className="page-title">➕ Add New Auction</h1>
                    <p className="page-subtitle">Create a new auction item for bidding</p>
                </div>
            </div>

            <div className="form-container">
                <div className="form-card card">
                    <form onSubmit={handleSubmit} className="auction-form">
                        <div className="form-group">
                            <label className="form-label" htmlFor="name">
                                Item Name *
                            </label>
                            <input
                                id="name"
                                type="text"
                                className="form-input"
                                placeholder="Enter the item name"
                                value={form.name}
                                onChange={(e) => setForm({ ...form, name: e.target.value })}
                                required
                                disabled={isLoading}
                            />
                        </div>

                        <div className="form-group">
                            <label className="form-label" htmlFor="description">
                                Description
                            </label>
                            <textarea
                                id="description"
                                className="form-input form-textarea"
                                placeholder="Describe your item in detail..."
                                value={form.description}
                                onChange={(e) => setForm({ ...form, description: e.target.value })}
                                rows="4"
                                disabled={isLoading}
                            />
                        </div>

                        <div className="form-group">
                            <label className="form-label" htmlFor="startingPrice">
                                Starting Price *
                            </label>
                            <div className="price-input-wrapper">
                                <span className="price-currency">$</span>
                                <input
                                    id="startingPrice"
                                    type="number"
                                    step="0.01"
                                    min="0"
                                    className="form-input"
                                    placeholder="0.00"
                                    value={form.startingPrice}
                                    onChange={(e) => setForm({ ...form, startingPrice: e.target.value })}
                                    required
                                    disabled={isLoading}
                                />
                            </div>
                        </div>

                        <div className="form-group">
                            <label className="form-label" htmlFor="image">
                                Item Image
                            </label>
                            <div className="image-upload-container">
                                <input
                                    id="image"
                                    type="file"
                                    accept="image/*"
                                    onChange={handleImageChange}
                                    className="image-input"
                                    disabled={isLoading}
                                />
                                <label htmlFor="image" className="image-upload-label">
                                    <div className="upload-icon">📷</div>
                                    <span className="upload-text">
                                        {image ? 'Change Image' : 'Choose an image'}
                                    </span>
                                </label>
                            </div>
                            
                            {imagePreview && (
                                <div className="image-preview">
                                    <img 
                                        src={imagePreview} 
                                        alt="Preview" 
                                        className="preview-image" 
                                    />
                                    <button
                                        type="button"
                                        className="remove-image-btn"
                                        onClick={() => {
                                            setImage(null);
                                            setImagePreview(null);
                                        }}
                                        disabled={isLoading}
                                    >
                                        ✕ Remove
                                    </button>
                                </div>
                            )}
                        </div>

                        <div className="form-actions">
                            <button
                                type="button"
                                className="btn btn-secondary"
                                onClick={() => navigate('/auctions')}
                                disabled={isLoading}
                            >
                                ← Cancel
                            </button>
                            <button 
                                type="submit" 
                                className="btn btn-primary"
                                disabled={isLoading || !form.name || !form.startingPrice}
                            >
                                {isLoading ? (
                                    <>
                                        <span className="loading"></span>
                                        Creating Auction...
                                    </>
                                ) : (
                                    '🚀 Create Auction'
                                )}
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    );
}