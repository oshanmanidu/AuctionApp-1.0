// import axios from 'axios';
//
// const API_BASE = 'https://localhost:7148/api'; // Your ASP.NET API URL
//
// const api = axios.create({
//     baseURL: API_BASE,
// });
//
// // Add token to requests
// api.interceptors.request.use((config) => {
//     const token = localStorage.getItem('token');
//     if (token) {
//         config.headers.Authorization = `Bearer ${token}`;
//     }
//     return config;
// });
//
// export default api;


import axios from 'axios';

const api = axios.create({
    baseURL: 'https://localhost:7252/api', // Proxied to https://localhost:7148/api
    headers: {
        'Content-Type': 'application/json',
    },
});

// Add Authorization Header with JWT
api.interceptors.request.use((config) => {
    const token = localStorage.getItem('token');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

export default api;