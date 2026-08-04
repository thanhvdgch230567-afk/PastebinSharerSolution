const express = require('express');
require('dotenv').config();

const app = express();
app.use(express.json());
const authRoutes = require('./routes/authRoutes');
app.use('/api/auth', authRoutes);

app.get('/health', (req, res) => {
    res.json({ status: 'auth-service is running' });
});

const PORT = process.env.PORT || 3000;
app.listen(PORT, () => {
    console.log(`Auth service running on port ${PORT}`);
});