// components/StartBidModal.jsx
import { useState } from 'react';
import api from '../services/api';

export default function StartBidModal({ itemId, itemName, onClose, onSuccess }) {
    const [startTime, setStartTime] = useState('');
    const [endTime, setEndTime] = useState('');

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!startTime || !endTime) {
            alert("Please set both start and end time");
            return;
        }

        try {
            await api.put(`/auctionitems/start-bid/${itemId}`, {
                startTime,
                endTime
            });
            alert("Bidding started!");
            if (onSuccess) onSuccess();
            onClose();
        } catch (err) {
            alert("Failed to start bidding: " + (err.response?.data || err.message));
        }
    };

    return (
        <div style={styles.modal}>
            <div style={styles.content}>
                <h3>Start Bidding: {itemName}</h3>
                <form onSubmit={handleSubmit}>
                    <div style={styles.inputGroup}>
                        <label>Start Time</label>
                        <input
                            type="datetime-local"
                            value={startTime}
                            onChange={(e) => setStartTime(e.target.value)}
                            required
                        />
                    </div>
                    <div style={styles.inputGroup}>
                        <label>End Time</label>
                        <input
                            type="datetime-local"
                            value={endTime}
                            onChange={(e) => setEndTime(e.target.value)}
                            required
                        />
                    </div>
                    <div style={styles.buttons}>
                        <button type="submit">Start Bidding</button>
                        <button type="button" onClick={onClose} style={styles.cancel}>
                            Cancel
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}

const styles = {
    modal: {
        position: 'fixed',
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        backgroundColor: 'rgba(0,0,0,0.5)',
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        zIndex: 1000
    },
    content: {
        background: 'white',
        padding: '2rem',
        borderRadius: '8px',
        width: '100%',
        maxWidth: '500px'
    },
    inputGroup: {
        marginBottom: '1rem'
    },
    buttons: {
        display: 'flex',
        gap: '1rem',
        marginTop: '1rem'
    },
    cancel: {
        background: '#95a5a6',
        color: 'white',
        border: 'none',
        padding: '0.5rem 1rem',
        borderRadius: '4px',
        cursor: 'pointer'
    }
};



