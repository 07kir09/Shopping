import React, { useState } from 'react';
import { 
    Box, 
    Button, 
    TextField, 
    Typography, 
    Paper,
    Snackbar,
    Alert
} from '@mui/material';
import { ordersApi } from '../services/api';

const CreateOrder: React.FC = () => {
    const [orderData, setOrderData] = useState({
        customerName: '',
        items: '',
        totalAmount: ''
    });
    const [notification, setNotification] = useState({
        open: false,
        message: '',
        severity: 'success' as 'success' | 'error'
    });

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            const items = orderData.items.split(',').map(item => ({
                name: item.trim(),
                quantity: 1
            }));
            
            const response = await ordersApi.createOrder({
                customerName: orderData.customerName,
                items,
                totalAmount: parseFloat(orderData.totalAmount)
            });

            setNotification({
                open: true,
                message: 'Order created successfully!',
                severity: 'success'
            });

            setOrderData({
                customerName: '',
                items: '',
                totalAmount: ''
            });
        } catch (error) {
            setNotification({
                open: true,
                message: 'Error creating order',
                severity: 'error'
            });
        }
    };

    return (
        <Box sx={{ maxWidth: 600, mx: 'auto', mt: 4 }}>
            <Paper sx={{ p: 3 }}>
                <Typography variant="h5" gutterBottom>
                    Create New Order
                </Typography>
                <form onSubmit={handleSubmit}>
                    <TextField
                        fullWidth
                        label="Customer Name"
                        value={orderData.customerName}
                        onChange={(e) => setOrderData({ ...orderData, customerName: e.target.value })}
                        margin="normal"
                        required
                    />
                    <TextField
                        fullWidth
                        label="Items (comma-separated)"
                        value={orderData.items}
                        onChange={(e) => setOrderData({ ...orderData, items: e.target.value })}
                        margin="normal"
                        required
                        helperText="Enter items separated by commas"
                    />
                    <TextField
                        fullWidth
                        label="Total Amount"
                        type="number"
                        value={orderData.totalAmount}
                        onChange={(e) => setOrderData({ ...orderData, totalAmount: e.target.value })}
                        margin="normal"
                        required
                    />
                    <Button 
                        type="submit" 
                        variant="contained" 
                        color="primary"
                        sx={{ mt: 2 }}
                    >
                        Create Order
                    </Button>
                </form>
            </Paper>
            <Snackbar 
                open={notification.open} 
                autoHideDuration={6000} 
                onClose={() => setNotification({ ...notification, open: false })}
            >
                <Alert 
                    onClose={() => setNotification({ ...notification, open: false })} 
                    severity={notification.severity}
                >
                    {notification.message}
                </Alert>
            </Snackbar>
        </Box>
    );
};

export default CreateOrder; 