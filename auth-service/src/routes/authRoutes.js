const express = require('express');
const router = express.Router();
const authenticateToken = require('../middleware/authenticateToken');
const { register, login, logout } = require('../controllers/authController');

router.post('/register', register);
router.post('/login', login);
router.get('/profile', authenticateToken, (req, res) => {
    res.json({ message: 'Bạn đã đăng nhập!', user: req.user });
});
router.post('/logout', authenticateToken, logout);

module.exports = router;