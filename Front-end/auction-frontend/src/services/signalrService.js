import { HubConnectionBuilder } from '@microsoft/signalr';

let connection = null;

export const startConnection = (onBid, onAuction, onAuctionClosed) => {
  connection = new HubConnectionBuilder()
    .withUrl('https://localhost:7252/notificationHub', {
      accessTokenFactory: () => localStorage.getItem('token')
    })
    .withAutomaticReconnect()
    .build();

  connection.start()
    .then(() => {
      console.log('SignalR Connected');
    })
    .catch(err => {
      console.error('SignalR Connection Error: ', err);
      // Connection failed, but we still return the connection object
      // The component can handle connection state via event listeners
    });

  if (onBid) {
    connection.on('ReceiveBid', onBid);
  }

  if (onAuction) {
    connection.on('ReceiveAuction', onAuction);
  }

  if (onAuctionClosed) {
    connection.on('AuctionClosed', onAuctionClosed);
  }

  return connection;
};

export const stopConnection = () => {
  if (connection) {
    connection.stop();
  }
};