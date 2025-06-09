import axios from 'axios';

const api = axios.create({
    baseURL: '/api'
});

export const ordersApi = {
    createOrder: (orderData: any) => api.post('/orders', orderData),
    getOrders: () => api.get('/orders'),
    getOrder: (id: string) => api.get(`/orders/${id}`)
};

export const paymentsApi = {
    processPayment: (paymentData: any) => api.post('/payments', paymentData),
    getPayment: (id: string) => api.get(`/payments/${id}`)
};

export default api; 