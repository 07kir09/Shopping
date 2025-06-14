document.addEventListener('DOMContentLoaded', () => {
    // Навигация
    const ordersLink = document.getElementById('ordersLink');
    const accountLink = document.getElementById('accountLink');
    const createOrderSection = document.getElementById('createOrderSection');
    const ordersSection = document.getElementById('ordersSection');
    const accountSection = document.getElementById('accountSection');

    ordersLink.addEventListener('click', (e) => {
        e.preventDefault();
        showSection(ordersSection);
        hideSection(createOrderSection);
        hideSection(accountSection);
        updateActiveLink(ordersLink);
    });

    accountLink.addEventListener('click', (e) => {
        e.preventDefault();
        showSection(accountSection);
        hideSection(createOrderSection);
        hideSection(ordersSection);
        updateActiveLink(accountLink);
    });

    // Создание заказа
    const createOrderForm = document.getElementById('createOrderForm');
    createOrderForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        
        const orderData = {
            userId: document.getElementById('userId').value,
            amount: parseFloat(document.getElementById('amount').value),
            description: document.getElementById('description').value
        };

        try {
            // Сначала проверяем существование аккаунта
            await API.payments.getAccount(orderData.userId);
            
            // Создаем заказ
            const result = await API.orders.createOrder(orderData);
            showNotification('Заказ успешно создан!', 'success');
            createOrderForm.reset();
        } catch (error) {
            if (error.message.includes('404')) {
                // Если аккаунт не существует, создаем его
                try {
                    await API.payments.createAccount(orderData.userId);
                    // После создания аккаунта создаем заказ
                    const result = await API.orders.createOrder(orderData);
                    showNotification('Аккаунт создан и заказ успешно создан!', 'success');
                    createOrderForm.reset();
                } catch (createError) {
                    showNotification('Ошибка при создании аккаунта и заказа: ' + createError.message, 'error');
                }
            } else {
                showNotification('Ошибка при создании заказа: ' + error.message, 'error');
            }
        }
    });

    // Получение информации об аккаунте
    const getAccountBtn = document.getElementById('getAccountBtn');
    const accountDetails = document.getElementById('accountDetails');

    getAccountBtn.addEventListener('click', async () => {
        const userId = document.getElementById('accountUserId').value;
        if (!userId) {
            showNotification('Пожалуйста, введите ID пользователя', 'error');
            return;
        }

        try {
            const account = await API.payments.getAccount(userId);
            accountDetails.innerHTML = `
                <h3>Информация об аккаунте</h3>
                <p><strong>ID пользователя:</strong> ${account.userId}</p>
                <p><strong>Баланс:</strong> ${account.balance} руб.</p>
            `;
        } catch (error) {
            if (error.message.includes('404')) {
                showNotification('Аккаунт не найден. Создайте новый аккаунт через форму создания заказа.', 'warning');
            } else {
                showNotification('Ошибка при получении информации об аккаунте: ' + error.message, 'error');
            }
        }
    });

    // Вспомогательные функции
    function showSection(section) {
        section.style.display = 'block';
    }

    function hideSection(section) {
        section.style.display = 'none';
    }

    function updateActiveLink(link) {
        document.querySelectorAll('.nav-links a').forEach(a => a.classList.remove('active'));
        link.classList.add('active');
    }

    function showNotification(message, type = 'info') {
        const notification = document.createElement('div');
        notification.className = `notification ${type}`;
        notification.textContent = message;
        
        document.body.appendChild(notification);
        
        // Анимация появления
        setTimeout(() => {
            notification.style.opacity = '1';
            notification.style.transform = 'translateY(0)';
        }, 100);

        // Автоматическое скрытие через 5 секунд
        setTimeout(() => {
            notification.style.opacity = '0';
            notification.style.transform = 'translateY(-20px)';
            setTimeout(() => {
                document.body.removeChild(notification);
            }, 300);
        }, 5000);
    }
}); 