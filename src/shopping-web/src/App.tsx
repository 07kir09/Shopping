import React from 'react';
import { 
    CssBaseline, 
    Container, 
    AppBar, 
    Toolbar, 
    Typography,
    ThemeProvider,
    createTheme
} from '@mui/material';
import CreateOrder from './components/CreateOrder';

const theme = createTheme({
    palette: {
        primary: {
            main: '#1976d2',
        },
        secondary: {
            main: '#dc004e',
        },
    },
});

function App() {
    return (
        <ThemeProvider theme={theme}>
            <CssBaseline />
            <AppBar position="static">
                <Toolbar>
                    <Typography variant="h6">
                        Shopping Application
                    </Typography>
                </Toolbar>
            </AppBar>
            <Container>
                <CreateOrder />
            </Container>
        </ThemeProvider>
    );
}

export default App;
