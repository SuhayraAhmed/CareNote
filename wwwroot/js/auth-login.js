// Firebase configuration
const firebaseConfig = {
    apiKey: "AIzaSyCkfMgfuNuKSHVJy9K230Fu8K0oqztjzuM",
    authDomain: "carenoteproject-af500.firebaseapp.com",
    projectId: "carenoteproject-af500",
    storageBucket: "carenoteproject-af500.firebasestorage.app",
    messagingSenderId: "586783000111",
    appId: "1:586783000111:web:2e8cdfed4203f2a5daa8da",
    measurementId: "G-PZF9ZH1MNN"
};

console.log('Firebase config loaded from JavaScript file');

document.addEventListener('DOMContentLoaded', function() {
    console.log('DOM loaded, initializing Firebase...');

    try {
        console.log('Initializing Firebase...');

        // Initialize Firebase
        if (!firebase.apps.length) {
            firebase.initializeApp(firebaseConfig);
            console.log('Firebase initialized successfully');
        } else {
            console.log(' Firebase already initialized');
        }

        // Setup view switching FIRST
        setupViewSwitching();

        // Setup Google login button
        const googleLoginBtn = document.getElementById('googleLoginBtn');
        if (googleLoginBtn) {
            googleLoginBtn.addEventListener('click', handleGoogleLogin);
            console.log(' Google login button event listener added');
        }

        // Setup email login form
        const emailLoginForm = document.getElementById('emailLoginForm');
        if (emailLoginForm) {
            emailLoginForm.addEventListener('submit', handleEmailLogin);
            console.log(' Email login form event listener added');
        }

        // Setup email register form
        const emailRegisterForm = document.getElementById('emailRegisterForm');
        if (emailRegisterForm) {
            emailRegisterForm.addEventListener('submit', handleEmailRegister);
            console.log(' Email register form event listener added');
        }

    } catch (error) {
        console.error(' Firebase initialization error:', error);
        alert('Firebase initialization failed: ' + error.message);
    }
});

// View switching setup
function setupViewSwitching() {
    const showRegisterBtn = document.getElementById('showRegisterBtn');
    const showLoginBtn = document.getElementById('showLoginBtn');

    console.log(' Setting up view switching...');
    console.log('Show Register Button:', showRegisterBtn);
    console.log('Show Login Button:', showLoginBtn);

    if (showRegisterBtn) {
        showRegisterBtn.addEventListener('click', showRegisterView);
        console.log('Show register button event listener added');
    } else {
        console.log(' Show register button not found');
    }

    if (showLoginBtn) {
        showLoginBtn.addEventListener('click', showLoginView);
        console.log('✅ Show login button event listener added');
    } else {
        console.log(' Show login button not found');
    }
}

// View switching functions
function showRegisterView() {
    console.log(' Switching to register view...');

    const loginSection = document.getElementById('loginSection');
    const registerSection = document.getElementById('registerSection');

    if (!loginSection || !registerSection) {
        console.error(' Login or Register section not found');
        return;
    }

    // Hide login section
    loginSection.style.display = 'none';

    // Show register section
    registerSection.style.display = 'block';

    console.log(' Switched to register view');
}

function showLoginView() {
    console.log(' Switching to login view...');

    const loginSection = document.getElementById('loginSection');
    const registerSection = document.getElementById('registerSection');

    if (!loginSection || !registerSection) {
        console.error(' Login or Register section not found');
        return;
    }

    // Hide register section
    registerSection.style.display = 'none';

    // Show login section
    loginSection.style.display = 'block';

    console.log(' Switched to login view');
}

// Google Login
async function handleGoogleLogin(event) {
    if (event) event.preventDefault();

    const button = document.getElementById('googleLoginBtn');
    if (!button) return;

    const originalText = button.innerHTML;
    button.disabled = true;
    button.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Signing in...';

    try {
        console.log(' Starting Google login...');

        const provider = new firebase.auth.GoogleAuthProvider();
        provider.addScope('email');
        provider.addScope('profile');

        console.log(' Opening Google sign-in popup...');
        const result = await firebase.auth().signInWithPopup(provider);

        if (!result || !result.user) {
            throw new Error('Google sign-in failed - no user returned');
        }

        // VALIDERA ATT DET ÄR @care.se email
        const userEmail = result.user.email;
        if (!userEmail.endsWith('@care.se')) {
            // Logga ut användaren om det inte är @care.se
            await firebase.auth().signOut();
            throw new Error('Only @care.se emails are allowed. Please use your healthcare organization email.');
        }

        console.log(' Google login successful:', userEmail);
        const idToken = await result.user.getIdToken();
        console.log(' Got ID token, sending to server...');

        // Send to server for verification
        const response = await fetch('/Auth/GoogleLogin', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify({
                idToken: idToken,
                userId: result.user.uid,
                email: userEmail,
                displayName: result.user.displayName
            })
        });

        const data = await response.json();
        console.log(' Server response:', data);

        if (data.success) {
            console.log(' Login successful, redirecting to Home...');
            window.location.href = '/Home';
        } else {
            throw new Error(data.error || 'Server rejected login');
        }

    } catch (error) {
        console.error(' Google login error:', error);

        let errorMessage = 'Google login failed: ';
        if (error.message.includes('@care.se')) {
            errorMessage = 'Only healthcare staff with @care.se emails are allowed. ';
            errorMessage += 'Please use your healthcare organization email.';
        } else if (error.message.includes('popup')) {
            errorMessage += 'Popup was blocked. Please allow popups for this site.';
        } else {
            errorMessage += error.message;
        }

        alert(errorMessage);
        button.disabled = false;
        button.innerHTML = originalText;
    }
}

// Email/Password Login - Använder vår EGEN auth nu
async function handleEmailLogin(event) {
    event.preventDefault();

    const button = event.target.querySelector('button[type="submit"]');
    const email = document.getElementById('loginEmail').value;
    const password = document.getElementById('loginPassword').value;

    if (!button) return;

    // VALIDERA EMAIL DOMÄN
    if (!email.endsWith('@care.se')) {
        alert('Only @care.se emails are allowed. Please use your healthcare organization email.');
        return;
    }

    const originalText = button.innerHTML;
    button.disabled = true;
    button.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Signing in...';

    try {
        console.log(' Starting email login with OUR auth...', email);

        // Använd vår EGEN server auth istället för Firebase Auth
        const response = await fetch('/Auth/EmailPasswordLogin', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify({
                email: email,
                password: password
            })
        });

        const data = await response.json();
        console.log(' Server response:', data);

        if (data.success) {
            console.log(' Login successful, redirecting to Home...');
            window.location.href = '/Home';
        } else {
            throw new Error(data.error || 'Login failed');
        }

    } catch (error) {
        console.error(' Email login error:', error);

        let errorMessage = 'Login failed: ';
        if (error.message.includes('not found')) {
            errorMessage = 'No account found with this email. Please create an account.';
        } else if (error.message.includes('Invalid password')) {
            errorMessage = 'Invalid password. Please try again.';
        } else if (error.message.includes('service unavailable')) {
            errorMessage = 'Service temporarily unavailable. Please try again.';
        } else {
            errorMessage += error.message;
        }

        alert(errorMessage);
        button.disabled = false;
        button.innerHTML = originalText;
    }
}

// Email/Password Registration - Använder vår EGEN auth nu
async function handleEmailRegister(event) {
    event.preventDefault();

    const button = event.target.querySelector('button[type="submit"]');
    const name = document.getElementById('registerName').value;
    const email = document.getElementById('registerEmail').value;
    const password = document.getElementById('registerPassword').value;

    if (!button) return;

    // VALIDERA EMAIL DOMÄN
    if (!email.endsWith('@care.se')) {
        alert('Only @care.se emails are allowed for registration. Please use your healthcare organization email.');
        return;
    }

    // VALIDERA LÖSENORD
    if (password.length < 6) {
        alert('Password must be at least 6 characters long.');
        return;
    }

    const originalText = button.innerHTML;
    button.disabled = true;
    button.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Creating account...';

    try {
        console.log(' Starting email registration with OUR auth...', email);

        // Använd vår EGEN server registration
        const response = await fetch('/Auth/EmailRegister', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify({
                email: email,
                password: password,
                displayName: name
            })
        });

        const data = await response.json();
        console.log('📨 Server response:', data);

        if (data.success) {
            console.log(' Registration successful, redirecting to Home...');
            window.location.href = '/Home';
        } else {
            throw new Error(data.error || 'Registration failed');
        }

    } catch (error) {
        console.error(' Email registration error:', error);

        let errorMessage = 'Registration failed: ';
        if (error.message.includes('already registered')) {
            errorMessage = 'This email is already registered. Please sign in instead.';
        } else if (error.message.includes('required')) {
            errorMessage = 'All fields are required.';
        } else {
            errorMessage += error.message;
        }

        alert(errorMessage);
        button.disabled = false;
        button.innerHTML = originalText;
    }
}