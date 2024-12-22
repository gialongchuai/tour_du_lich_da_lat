document.addEventListener('DOMContentLoaded', function () {
    // Đăng Nhập
    const loginForm = document.getElementById('loginForm');
    loginForm.addEventListener('submit', async function (event) {
        event.preventDefault();

        const username = document.getElementById('username').value.trim();
        const password = document.getElementById('password').value.trim();

        if (!username || !password) {
            alert('Vui lòng điền đầy đủ thông tin');
            return;
        }

        const response = await fetch(loginForm.action, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ username, password }),
        });

        const result = await response.json();
        const messageDiv = document.getElementById('loginMessage');
        messageDiv.classList.remove('d-none');
        messageDiv.classList.remove('alert-success', 'alert-danger');
        messageDiv.classList.add(result.success ? 'alert-success' : 'alert-danger');
        messageDiv.innerText = result.message;

        if (result.success) {
            setTimeout(() => {
                $('#loginModal').modal('hide');
                location.reload();
            }, 2000);
        }
    });

    // Quên Mật Khẩu
    const forgotPasswordForm = document.getElementById('forgotPasswordForm');
    forgotPasswordForm.addEventListener('submit', async function (event) {
        event.preventDefault();

        const email = document.getElementById('email').value.trim();
        if (!email || !validateEmail(email)) {
            alert('Vui lòng nhập email hợp lệ');
            return;
        }

        const response = await fetch(forgotPasswordForm.action, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ email }),
        });

        const result = await response.json();
        alert(result.message);

        if (result.success) {
            setTimeout(() => {
                location.reload();
            }, 2000);
        }
    });

    // Đăng Ký
    const registerForm = document.getElementById('registerForm');
    registerForm.onsubmit = async function (event) {
        event.preventDefault();

        const formData = new FormData(this);

        const response = await fetch(this.action, {
            method: 'POST',
            body: formData
        });

        const result = await response.json();

        const messageDiv = document.getElementById('registerMessage');
        messageDiv.classList.remove('d-none');
        messageDiv.classList.remove('alert-success', 'alert-danger');
        messageDiv.classList.add(result.success ? 'alert-success' : 'alert-danger');
        messageDiv.innerText = result.message;

        if (result.success) {
            setTimeout(() => {
                $('#registerModal').modal('hide');
                location.reload();
            }, 2000);
        }
    };

    const changePasswordForm = document.getElementById('changePasswordForm');
    changePasswordForm.addEventListener('submit', async function (event) {
        event.preventDefault();

        const oldPassword = document.getElementById('oldPassword').value.trim();
        const newPassword = document.getElementById('newPassword').value.trim();
        const confirmNewPassword = document.getElementById('confirmNewPassword').value.trim();

        if (!oldPassword || !newPassword || !confirmNewPassword) {
            alert('Vui lòng điền đầy đủ thông tin');
            return;
        }

        if (newPassword !== confirmNewPassword) {
            alert('Mật khẩu mới và xác nhận mật khẩu mới không khớp');
            return;
        }

        const response = await fetch(changePasswordForm.action, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                oldPassword,
                newPassword,
                confirmNewPassword
            })
        });

        const result = await response.json();
        const messageDiv = document.getElementById('passwordChangeMessage');
        messageDiv.classList.remove('d-none');
        messageDiv.classList.remove('alert-success', 'alert-danger');
        messageDiv.classList.add(result.success ? 'alert-success' : 'alert-danger');
        messageDiv.innerText = result.message;
    });

    // Hàm kiểm tra email hợp lệ
    function validateEmail(email) {
        const re = /^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,6}$/;
        return re.test(email);
    }
});
