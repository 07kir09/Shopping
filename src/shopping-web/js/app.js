document.addEventListener('DOMContentLoaded', () => {
    console.log('Shopping App loaded successfully!');
    const ordersLink = document.getElementById('ordersLink');
    const accountLink = document.getElementById('accountLink');
    const createAccountLink = document.getElementById('createAccountLink');
    const createOrderSection = document.getElementById('createOrderSection');
    const ordersSection = document.getElementById('ordersSection');
    const accountSection = document.getElementById('accountSection');
    const createAccountSection = document.getElementById('createAccountSection');

    console.log('Elements found:', {
        ordersLink: !!ordersLink,
        accountLink: !!accountLink,
        createAccountLink: !!createAccountLink,
        createOrderSection: !!createOrderSection,
        ordersSection: !!ordersSection,
        accountSection: !!accountSection,
        createAccountSection: !!createAccountSection
    });

    ordersLink.addEventListener('click', (e) => {
        e.preventDefault();
        console.log('Orders link clicked');
        showSection(ordersSection);
        hideSection(createOrderSection);
        hideSection(accountSection);
        updateActiveLink(ordersLink);
        loadUserOrders();
    });

    accountLink.addEventListener('click', (e) => {
        e.preventDefault();
        showSection(accountSection);
        hideSection(createOrderSection);
        hideSection(ordersSection);
        updateActiveLink(accountLink);
    });

    createAccountLink.addEventListener('click', (e) => {
        e.preventDefault();
        showSection(createAccountSection);
        hideSection(createOrderSection);
        hideSection(ordersSection);
        hideSection(accountSection);
        updateActiveLink(createAccountLink);
        // Автоматически генерируем новый GUID при открытии секции
        document.getElementById('newAccountUserId').value = generateGuid();
    });

    // Кнопка для генерации нового GUID
    const generateGuidBtn = document.getElementById('generateGuidBtn');
    if (generateGuidBtn) {
        generateGuidBtn.addEventListener('click', () => {
            document.getElementById('newAccountUserId').value = generateGuid();
        });
    }

    // Функция генерации GUID
    function generateGuid() {
        // https://stackoverflow.com/a/2117523/65387
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
            var r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }

    const createOrderForm = document.getElementById('createOrderForm');
    createOrderForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        
        const orderData = {
            userId: document.getElementById('userId').value,
            amount: parseFloat(document.getElementById('amount').value),
            description: document.getElementById('description').value
        };
        
        if (!isValidGuid(orderData.userId)) {
            showNotification('Введите корректный GUID формат: 00000000-0000-0000-0000-000000000001', 'error');
            return;
        }

        try {
            await API.payments.getAccount(orderData.userId);
            const result = await API.orders.createOrder(orderData);
            showNotification('Заказ успешно создан!', 'success');
            createOrderForm.reset();
        } catch (error) {
            if (error.message.includes('404')) {
                try {
                    await API.payments.createAccount(orderData.userId);
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

    const createAccountForm = document.getElementById('createAccountForm');
    createAccountForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        
        const userId = document.getElementById('newAccountUserId').value;
        const initialBalance = parseFloat(document.getElementById('initialBalance').value) || 0;
        
        if (!isValidGuid(userId)) {
            showNotification('Введите корректный GUID формат: 00000000-0000-0000-0000-000000000001', 'error');
            return;
        }

        try {
            // Создаем аккаунт
            const account = await API.payments.createAccount(userId);
            
            // Если указан начальный баланс, пополняем его
            if (initialBalance > 0) {
                await API.payments.topUpBalance(userId, initialBalance);
                showNotification(`Аккаунт создан с начальным балансом ${initialBalance} руб!`, 'success');
            } else {
                showNotification('Аккаунт успешно создан!', 'success');
            }
            
            // Показываем результат
            const resultDiv = document.getElementById('accountCreationResult');
            resultDiv.innerHTML = `
                <div class="success-message">
                    <h3>✅ Аккаунт создан успешно!</h3>
                    <p><strong>ID пользователя:</strong> ${account.userId}</p>
                    <p><strong>Баланс:</strong> ${account.balance + initialBalance} руб.</p>
                    <p><strong>Дата создания:</strong> ${new Date(account.createdAt).toLocaleString()}</p>
                </div>
            `;
            
            createAccountForm.reset();
        } catch (error) {
            showNotification('Ошибка при создании аккаунта: ' + error.message, 'error');
            const resultDiv = document.getElementById('accountCreationResult');
            resultDiv.innerHTML = `
                <div class="error-message">
                    <h3>❌ Ошибка создания аккаунта</h3>
                    <p>${error.message}</p>
                </div>
            `;
        }
    });

    const getAccountBtn = document.getElementById('getAccountBtn');
    const accountDetails = document.getElementById('accountDetails');

    getAccountBtn.addEventListener('click', async () => {
        const userId = document.getElementById('accountUserId').value;
        if (!userId) {
            showNotification('Пожалуйста, введите ID пользователя', 'error');
            return;
        }
        
        if (!isValidGuid(userId)) {
            showNotification('Введите корректный GUID формат: 00000000-0000-0000-0000-000000000001', 'error');
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

    const topUpBtn = document.getElementById('topUpBtn');
    console.log('TopUp button found:', !!topUpBtn);
    if (topUpBtn) {
        topUpBtn.addEventListener('click', async () => {
            console.log('TopUp button clicked');
            const userId = document.getElementById('accountUserId').value;
            const amount = prompt('Введите сумму для пополнения:');
            
            console.log('TopUp data:', { userId, amount });
            
            if (!userId || !amount) {
                showNotification('Введите ID пользователя и сумму', 'error');
                return;
            }
            
            if (!isValidGuid(userId)) {
                showNotification('Введите корректный GUID формат', 'error');
                return;
            }

            try {
                console.log('Calling API.payments.topUpBalance...');
                await API.payments.topUpBalance(userId, parseFloat(amount));
                showNotification(`Баланс пополнен на ${amount} руб!`, 'success');
                getAccountBtn.click();
            } catch (error) {
                console.error('TopUp error:', error);
                showNotification('Ошибка пополнения: ' + error.message, 'error');
            }
        });
    }

    async function loadUserOrders() {
        console.log('Loading user orders...');
        const userId = prompt('Введите ID пользователя для просмотра заказов:');
        if (!userId) {
            console.log('No userId provided');
            return;
        }
        
        console.log('UserId provided:', userId);
        
        if (!isValidGuid(userId)) {
            console.log('Invalid GUID format');
            showNotification('Введите корректный GUID формат: 00000000-0000-0000-0000-000000000001', 'error');
            return;
        }

        try {
            console.log('Calling API.orders.getUserOrders...');
            const orders = await API.orders.getUserOrders(userId);
            console.log('Orders received:', orders);
            
            const ordersList = document.getElementById('ordersList');
            
            if (orders.length === 0) {
                console.log('No orders found');
                ordersList.innerHTML = '<p>У вас пока нет заказов</p>';
            } else {
                console.log('Displaying', orders.length, 'orders');
                ordersList.innerHTML = orders.map(order => `
                    <div class="order-card">
                        <h3>Заказ #${order.id.substring(0, 8)}</h3>
                        <p><strong>Описание:</strong> ${order.description}</p>
                        <p><strong>Сумма:</strong> ${order.amount} руб.</p>
                        <p><strong>Статус:</strong> ${order.status}</p>
                        <p><strong>Создан:</strong> ${new Date(order.createdAt).toLocaleString()}</p>
                    </div>
                `).join('');
            }
        } catch (error) {
            console.error('Orders loading error:', error);
            showNotification('Ошибка загрузки заказов: ' + error.message, 'error');
        }
    }


    function isValidGuid(guid) {
        const guidRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
        return guidRegex.test(guid);
    }

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
        
        setTimeout(() => {
            notification.style.opacity = '1';
            notification.style.transform = 'translateY(0)';
        }, 100);
        setTimeout(() => {
            notification.style.opacity = '0';
            notification.style.transform = 'translateY(-20px)';
            setTimeout(() => {
                document.body.removeChild(notification);
            }, 300);
        }, 5000);
    }
}); 