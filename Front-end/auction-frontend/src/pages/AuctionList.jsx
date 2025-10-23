// src/pages/AuctionList.jsx
import { useState } from 'react';
import api from '../services/api';
import {  useEffect } from 'react';
import StartBidModal from '../components/StartBidModal';
import { startConnection, stopConnection } from '../services/signalrService';

function CountdownTimer({ endTime, onEnd }) {
	const [timeLeft, setTimeLeft] = useState('');

	useEffect(() => {
		const interval = setInterval(() => {
			const now = new Date().getTime();
			const end = new Date(endTime).getTime();
			const diff = end - now;

			if (diff <= 0) {
				clearInterval(interval);
				setTimeLeft("Bidding ended");
				if (onEnd) onEnd();
			} else {
				const hours = Math.floor(diff / (1000 * 60 * 60));
				const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
				const seconds = Math.floor((diff % (1000 * 60)) / 1000);
				setTimeLeft(`${hours}h ${minutes}m ${seconds}s`);
			}
		}, 1000);

		return () => clearInterval(interval);
	}, [endTime, onEnd]);

	return <span>{timeLeft}</span>;
}

export default function AuctionList() {
    const [items, setItems] = useState([]);
    const [loading, setLoading] = useState(true);
    const [bidAmounts, setBidAmounts] = useState({}); // Track bid amount per item
    const [biddingItem, setBiddingItem] = useState(null); // Track which item is being bid on
    const [deletingItem, setDeletingItem] = useState(null); // Track which item is being deleted
    const [startBidModal, setStartBidModal] = useState(null); // Track which item's start bid modal is open
    const [endingBid, setEndingBid] = useState(null); // Track which item is being ended
    const [signalRConnected, setSignalRConnected] = useState(false); // Track SignalR connection status
    const role = localStorage.getItem('role'); // Get user role for admin check

    useEffect(() => {
        const fetchItems = async () => {
            try {
                const res = await api.get('/auctionitems');
                setItems(res.data);
            } catch (err) {
                console.error(err);
            } finally {
                setLoading(false);
            }
        };

        // Fetch initial data
        fetchItems();

        // ✅ Connect to SignalR for real-time updates
        const hubConnection = startConnection(
            // Handle bid updates
            (bidData) => {
                console.log("Received bid update:", bidData);
                
                // ✅ Update specific auction item with new bid
                setItems(prev => prev.map(item => {
                    if (item.id === bidData.auctionItemId) {
                        return {
                            ...item,
                            currentHighestBid: bidData.amount,
                            bids: item.bids ? [...item.bids, { 
                                id: Date.now(), // Temporary ID for new bid
                                amount: bidData.amount,
                                userId: bidData.userId,
                                timestamp: new Date().toISOString()
                            }] : [{ 
                                id: Date.now(),
                                amount: bidData.amount,
                                userId: bidData.userId,
                                timestamp: new Date().toISOString()
                            }]
                        };
                    }
                    return item;
                }));
            },
            // Handle auction updates (status changes, etc.)
            (auctionData) => {
                console.log("Received auction update:", auctionData);
                
                // ✅ Update auction status or other properties
                setItems(prev => prev.map(item => {
                    if (item.id === auctionData.auctionItemId) {
                        return {
                            ...item,
                            ...auctionData.updates // Spread any updates from the server
                        };
                    }
                    return item;
                }));
            }
        );

        // Set connection status
        if (hubConnection) {
            // Listen for connection state changes
            hubConnection.onclose(() => {
                console.log('SignalR connection closed');
                setSignalRConnected(false);
            });
            
            hubConnection.onreconnecting(() => {
                console.log('SignalR reconnecting...');
                setSignalRConnected(false);
            });
            
            hubConnection.onreconnected(() => {
                console.log('SignalR reconnected');
                setSignalRConnected(true);
            });

            // Set initial connection status
            // The connection is started in the service, so we assume it will connect
            setSignalRConnected(true);
        }

        // ✅ Cleanup SignalR connection on component unmount
        return () => {
            if (hubConnection) {
                stopConnection();
            }
        };
    }, []);

    // PayPal Button Rendering Effect
    useEffect(() => {
        const loadPayPal = async () => {
            // Only load PayPal for closed auctions where current user is the winner
            const winnerItems = items.filter(item => 
                item.isClosed && item.winnerEmail === localStorage.getItem('email')
            );

            if (winnerItems.length === 0) return;

            const script = document.createElement('script');
            script.src = 'https://www.paypal.com/sdk/js?client-id=AUI3HOzUZNnbi79Vu9M3_PyoU6js32F-ewk2-S4VmPA8z6bJh5o-gPfhSD_NdexqgLkPPgTcXs4wfWTx&currency=USD';
            script.onload = () => {
                // Render PayPal buttons for each winner item
                winnerItems.forEach(item => {
                    const containerId = `paypal-button-container-${item.id}`;
                    const container = document.getElementById(containerId);
                    
                    if (container) {
                        // Clear existing content
                        container.innerHTML = '';
                        
                        // Render PayPal button
                        window.paypal.Buttons({
                            createOrder: (data, actions) => {
                                return actions.order.create({
                                    purchase_units: [{
                                        amount: {
                                            value: item.winningBidAmount?.toFixed(2),
                                            currency_code: 'USD'
                                        },
                                        description: `Payment for ${item.name}`
                                    }]
                                });
                            },
                            onApprove: async (data, actions) => {
                                const order = await actions.order.capture();
                                alert(`Payment successful! Order ID: ${order.id}`);

                                // Optional: Notify backend
                                try {
                                    await api.post('/payments/confirm', {
                                        orderId: order.id,
                                        auctionItemId: item.id,
                                        payerEmail: order.payer.email_address
                                    });
                                    setItems(prev => prev.map(i =>
                                        i.id === item.id ? { ...i, paid: true } : i
                                    ));
                                } catch (err) {
                                    console.error("Failed to confirm payment");
                                }
                            },
                            onError: (err) => {
                                alert("Payment failed. Please try again.");
                                console.error(err);
                            }
                        }).render(`#${containerId}`);
                    }
                });
            };
            document.body.appendChild(script);

            return () => {
                if (document.body.contains(script)) {
                    document.body.removeChild(script);
                }
            };
        };

        loadPayPal();
    }, [items]);

    // Live Auction Updates - Listen for auction closure events
    useEffect(() => {
        const connection = startConnection(
            null, // onBid - handled in main useEffect
            null, // onAuction - handled in main useEffect
            (data) => {
                // Handle auction closure
                setItems(prev => prev.map(item =>
                    item.id === data.auctionItemId
                        ? { ...item, isClosed: true, winnerEmail: data.winnerEmail, winningBidAmount: data.winningBidAmount }
                        : item
                ));
            }
        );

        return () => {
            if (connection) connection.stop();
        };
    }, []);

    const handleBidChange = (itemId, value) => {
        setBidAmounts(prev => ({
            ...prev,
            [itemId]: value
        }));
    };

    const handlePlaceBid = async (itemId) => {
        const amount = bidAmounts[itemId];
        if (!amount || amount <= 0) {
            alert("Please enter a valid bid amount");
            return;
        }

        setBiddingItem(itemId);
        try {
            await api.post('/bids', { auctionItemId: itemId, amount: parseFloat(amount) });
            alert("Bid placed successfully!");
            // Clear the bid input
            setBidAmounts(prev => ({
                ...prev,
                [itemId]: ''
            }));
            // Note: Real-time update will be handled by SignalR, no need to manually refresh
        } catch (err) {
            alert("Bid failed: " + (err.response?.data || err.message));
        } finally {
            setBiddingItem(null);
        }
    };

    const handleDelete = async (id, imageUrl) => {
        if (!window.confirm("Are you sure you want to delete this auction item? This action cannot be undone.")) return;

        setDeletingItem(id);
        try {
            await api.delete(`/auctionitems/${id}`);
            
            // Remove item from frontend state
            setItems(items.filter(i => i.id !== id));
            
            // Show success message
            alert("Auction item deleted successfully!");
        } catch (err) {
            alert("Delete failed: " + (err.response?.data || err.message));
        } finally {
            setDeletingItem(null);
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
            // Refresh the items to get updated data
            const res = await api.get('/auctionitems');
            setItems(res.data);
        } catch (err) {
            alert("Failed to end bidding: " + (err.response?.data || err.message));
        } finally {
            setEndingBid(null);
        }
    };

    const handleStartBidSuccess = async () => {
        // Refresh the items to get updated data
        const res = await api.get('/auctionitems');
        setItems(res.data);
    };

    if (loading) {
        return (
            <div className="page-container">
                <div className="loading-container">
                    <div className="loading loading-lg"></div>
                    <p className="loading-text">Loading auction items...</p>
                </div>
            </div>
        );
    }

    return (
        <div className="page-container">
            <div className="page-header">
                <div className="header-content">
                    <h1 className="page-title">🏷️ Live Auctions</h1>
                    <p className="page-subtitle">Discover unique items and place your bids</p>
                    {/* SignalR Connection Status */}
                    <div className="connection-status">
                        <span className={`status-indicator ${signalRConnected ? 'connected' : 'disconnected'}`}>
                            {signalRConnected ? '🟢 Live Updates' : '🔴 Offline'}
                        </span>
                    </div>
                </div>
                <div className="header-stats">
                    <div className="stat-item">
                        <span className="stat-number">{items.length}</span>
                        <span className="stat-label">Active Items</span>
                    </div>
                </div>
            </div>

            {items.length === 0 ? (
                <div className="empty-state">
                    <div className="empty-icon">📦</div>
                    <h3>No auctions available</h3>
                    <p>Check back later for new items or create your own auction!</p>
                </div>
            ) : (
                <div className="auction-grid">
                    {items.map(item => {
                        const hasBidWindow = item.bidStartTime && item.bidEndTime;
                        const now = new Date();
                        const canBid = hasBidWindow && now >= new Date(item.bidStartTime) && now <= new Date(item.bidEndTime);
                        const biddingClosed = hasBidWindow && now > new Date(item.bidEndTime);
                        return (
                        <div key={item.id} className="auction-card card animate-fade-in">
                            <div className="auction-image-container">
                                {item.imageUrl ? (
                                    <img
                                        src={`https://localhost:7252${item.imageUrl}`}
                                        alt={item.name}
                                        className="auction-image"
                                    />
                                ) : (
                                    <div className="auction-placeholder">
                                        <span className="placeholder-icon">🖼️</span>
                                        <span className="placeholder-text">No Image</span>
                                    </div>
                                )}
                                <div className="auction-badge">
                                    <span className="badge-text">LIVE</span>
                                </div>
                            </div>

                            <div className="auction-content">
                                <h3 className="auction-title">{item.name}</h3>
                                <p className="auction-description">{item.description}</p>
                                
                                <div className="auction-price">
                                    <span className="price-label">Starting Price:</span>
                                    <span className="price-value">${item.startingPrice}</span>
                                </div>

                                {hasBidWindow ? (
                                    <div className="auction-times">
                                        <div className="time-row"><strong>Bidding Starts:</strong>&nbsp;{new Date(item.bidStartTime).toLocaleString()}</div>
                                        <div className="time-row"><strong>Bidding Ends:</strong>&nbsp;{new Date(item.bidEndTime).toLocaleString()}</div>
                                        <div className="time-row"><strong>Time Left:</strong>&nbsp;<CountdownTimer endTime={item.bidEndTime} /></div>
                                    </div>
                                ) : (
                                    <div className="auction-times">
                                        <div className="time-row">Bidding not started</div>
                                    </div>
                                )}

                                {item.bidStartTime ? (
                                    (item.isBiddingOpen ?? canBid) ? (
                                        <p style={{ color: 'green' }}>🟢 Bidding Open!</p>
                                    ) : (now < new Date(item.bidStartTime)) ? (
                                        <p>🕒 Bidding starts at {new Date(item.bidStartTime).toLocaleString()}</p>
                                    ) : (
                                        <p style={{ color: 'red' }}>🔚 Bidding closed</p>
                                    )
                                ) : (
                                    <p>🚫 Bidding not started</p>
                                )}

                                {item.isClosed ? (
                                    <div style={{ 
                                        marginTop: '1rem', 
                                        padding: '0.75rem', 
                                        backgroundColor: '#d4edda', 
                                        color: '#155724', 
                                        borderRadius: '6px', 
                                        fontWeight: 'bold', 
                                        fontSize: '1.1em' 
                                    }}>
                                        🏆 Winner: <span style={{ color: '#004085' }}>{item.winnerEmail}</span><br />
                                        💰 Winning Bid: <span style={{ color: '#c38b00' }}>${item.winningBidAmount?.toFixed(2)}</span>
                                    </div>
                                ) : (
                                    <p>Bidding Open! Current Highest: ${item.currentHighestBid}</p>
                                )}

                                {/* Payment Section for Winners */}
                                {item.isClosed && item.winnerEmail === localStorage.getItem('email') && (
                                    <div style={{ marginTop: '1rem' }}>
                                        <h4>✅ You won this auction!</h4>
                                        <p>Please complete your payment:</p>
                                        {/* PayPal Button */}
                                        <div id={`paypal-button-container-${item.id}`}></div>
                                    </div>
                                )}

                                {item.isClosed && item.winnerEmail !== localStorage.getItem('email') && (
                                    <p>Auction ended. {item.winnerEmail} won.</p>
                                )}

                                <div className="auction-bid-section">
                                    <div className="bid-input-group">
                                        <span className="bid-currency">$</span>
                                        <input
                                            type="number"
                                            step="0.01"
                                            min={item.startingPrice}
                                            placeholder="Enter bid amount"
                                            value={bidAmounts[item.id] || ''}
                                            onChange={(e) => handleBidChange(item.id, e.target.value)}
                                            className="bid-input"
                                            disabled={biddingItem === item.id || !canBid}
                                        />
                                    </div>
                                    <button
                                        onClick={() => handlePlaceBid(item.id)}
                                        className="btn btn-primary bid-button"
                                        disabled={biddingItem === item.id || !bidAmounts[item.id] || !canBid}
                                    >
                                        {biddingItem === item.id ? (
                                            <>
                                                <span className="loading"></span>
                                                Placing Bid...
                                            </>
                                        ) : (
                                            'Place Bid'
                                        )}
                                    </button>
                                    
                                </div>

                                {/* Admin Controls */}
                                {role === 'Admin' && (
                                    <div className="admin-actions">
                                        <div className="admin-buttons">
                                            {/* Start Bidding Button - Show when bidding hasn't started */}
                                            {!item.bidStartTime && (
                                                <button
                                                    onClick={() => handleStartBid(item.id)}
                                                    className="btn btn-success btn-sm"
                                                >
                                                    🚀 Start Bidding
                                                </button>
                                            )}
                                            
                                            {/* End Bidding Button - Show when bidding is active */}
                                            {item.bidStartTime && canBid && (
                                                <button
                                                    onClick={() => handleEndBid(item.id)}
                                                    className="btn btn-warning btn-sm"
                                                    disabled={endingBid === item.id}
                                                >
                                                    {endingBid === item.id ? (
                                                        <>
                                                            <span className="loading"></span>
                                                            Ending...
                                                        </>
                                                    ) : (
                                                        '⏹️ End Bidding'
                                                    )}
                                                </button>
                                            )}
                                            
                                            {/* Delete Button */}
                                            <button
                                                onClick={() => handleDelete(item.id, item.imageUrl)}
                                                className="btn btn-danger btn-sm"
                                                disabled={deletingItem === item.id}
                                            >
                                                {deletingItem === item.id ? (
                                                    <>
                                                        <span className="loading"></span>
                                                        Deleting...
                                                    </>
                                                ) : (
                                                    '🗑️ Delete Item'
                                                )}
                                            </button>
                                        </div>
                                    </div>
                                )}
                            </div>
                        </div>
                        );
                    })}
                </div>
            )}

            {/* Start Bidding Modal */}
            {startBidModal && (
                <StartBidModal
                    itemId={startBidModal}
                    itemName={items.find(item => item.id === startBidModal)?.name || 'Unknown Item'}
                    onClose={() => setStartBidModal(null)}
                    onSuccess={handleStartBidSuccess}
                />
            )}
        </div>
    );
}