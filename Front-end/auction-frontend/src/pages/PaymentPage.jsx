import { PayPalScriptProvider, PayPalButtons } from '@paypal/react-paypal-js';

export default function PaymentPage({ amount, auctionId }) {
    return (
        <div className="page-container">
            <div className="page-header">
                <div className="header-content">
                    <h1 className="page-title">💳 Payment</h1>
                    <p className="page-subtitle">Complete your auction purchase securely</p>
                </div>
            </div>

            <div className="payment-container">
                <div className="payment-card card">
                    <div className="payment-header">
                        <div className="payment-icon">💰</div>
                        <h2 className="payment-title">Payment Summary</h2>
                    </div>
                    
                    <div className="payment-details">
                        <div className="detail-row">
                            <span className="detail-label">Auction ID:</span>
                            <span className="detail-value">#{auctionId}</span>
                        </div>
                        <div className="detail-row">
                            <span className="detail-label">Amount Due:</span>
                            <span className="detail-value amount">${amount}</span>
                        </div>
                    </div>

                    <div className="payment-methods">
                        <h3 className="methods-title">Choose Payment Method</h3>
                        <div className="paypal-section">
                            <div className="paypal-header">
                                <div className="paypal-logo">PayPal</div>
                                <span className="paypal-subtitle">Secure payment processing</span>
                            </div>
                            
                            <PayPalScriptProvider options={{ 
                                clientId: "AY5iZ2SbFNEMLI5R4ZWbhWce1FaMgq1AG5R2Fzlfy2S126jJPQEvgmkyZMBc9xMfzAk0evmF_fykVej_" 
                            }}>
                                <PayPalButtons
                                    createOrder={(data, actions) => {
                                        return actions.order.create({
                                            purchase_units: [{
                                                amount: {
                                                    value: amount
                                                },
                                                description: `Auction #${auctionId} Payment`
                                            }]
                                        });
                                    }}
                                    onApprove={async (data, actions) => {
                                        const details = await actions.order.capture();
                                        alert("Transaction completed by " + details.payer.name.given_name);
                                        // Call your API to mark item as paid
                                    }}
                                    style={{
                                        layout: 'horizontal',
                                        color: 'blue',
                                        shape: 'rect',
                                        label: 'pay'
                                    }}
                                />
                            </PayPalScriptProvider>
                        </div>
                    </div>

                    <div className="payment-footer">
                        <div className="security-info">
                            <span className="security-icon">🔒</span>
                            <span className="security-text">
                                Your payment information is secure and encrypted
                            </span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}