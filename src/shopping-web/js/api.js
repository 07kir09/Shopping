const API = {
    orders: {
        baseUrl: 'http://localhost:5001/api',
        async createOrder(orderData) {
            try {
                const response = await fetch(`${this.baseUrl}/Orders`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(orderData)
                });
                
                if (!response.ok) {
                    const errorData = await response.json();
                    throw new Error(errorData.message || 'Ошибка при создании заказа');
                }
                
                return response.json();
            } catch (error) {
                console.error('Error creating order:', error);
                throw error;
            }
        },
        async getOrders(userId) {
            try {
                const response = await fetch(`${this.baseUrl}/Orders?userId=${userId}`);
                
                if (!response.ok) {
                    const errorData = await response.json();
                    throw new Error(errorData.message || 'Ошибка при получении заказов');
                }
                
                return response.json();
            } catch (error) {
                console.error('Error getting orders:', error);
                throw error;
            }
        }
    },
    payments: {
        baseUrl: 'http://localhost:5002/api',
        async createAccount(userId) {
            try {
                const response = await fetch(`${this.baseUrl}/Payments/accounts`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({ userId })
                });
                
                if (!response.ok) {
                    const errorData = await response.json();
                    throw new Error(errorData.message || 'Ошибка при создании аккаунта');
                }
                
                return response.json();
            } catch (error) {
                console.error('Error creating account:', error);
                throw error;
            }
        },
        async getAccount(userId) {
            try {
                const response = await fetch(`${this.baseUrl}/Payments/accounts/${userId}`);
                
                if (!response.ok) {
                    const errorData = await response.json();
                    throw new Error(errorData.message || 'Ошибка при получении информации об аккаунте');
                }
                
                return response.json();
            } catch (error) {
                console.error('Error getting account:', error);
                throw error;
            }
        },
        async processPayment(paymentData) {
            try {
                const response = await fetch(`${this.baseUrl}/Payments/process`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(paymentData)
                });
                
                if (!response.ok) {
                    const errorData = await response.json();
                    throw new Error(errorData.message || 'Ошибка при обработке платежа');
                }
                
                return response.json();
            } catch (error) {
                console.error('Error processing payment:', error);
                throw error;
            }
        }
    }
}; 