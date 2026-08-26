import { useState } from 'react';
import { useAuth } from '../auth/useAuth';
import * as authApi from '../api/authApi';
import './LoginPage.css';

// Real login/auth is another colleague's work for later. Until then, these
// quick-login buttons cover the common roles for testing - they still call
// the real AuthService (same as the form below), so every other API call
// keeps working with a real, valid token. Swap this out once real auth
// ships; nothing else in the app depends on how someone got logged in.
const DEMO_ACCOUNTS = [
  { username: 'admin', role: 'Admin' },
  { username: 'pm', role: 'ProjectManager' },
  { username: 'member', role: 'TeamMember' },
  { username: 'client', role: 'Client' },
];
const DEMO_PASSWORD = 'password123';

export default function LoginPage() {
  const { login } = useAuth();
  const [mode, setMode] = useState('login'); // 'login' | 'register'
  const [pending, setPending] = useState(false);
  const [quickLoginPending, setQuickLoginPending] = useState(null);
  const [error, setError] = useState(null);
  const [info, setInfo] = useState(null);

  const [loginForm, setLoginForm] = useState({ username: '', password: '' });
  const [registerForm, setRegisterForm] = useState({
    name: '',
    username: '',
    email: '',
    contactInfo: '',
    password: '',
  });

  async function handleLogin(e) {
    e.preventDefault();
    setError(null);
    setPending(true);
    try {
      await login(loginForm.username, loginForm.password);
    } catch (err) {
      setError(err.message || 'Login failed.');
    } finally {
      setPending(false);
    }
  }

  async function handleQuickLogin(account) {
    setError(null);
    setQuickLoginPending(account.username);
    try {
      await login(account.username, DEMO_PASSWORD);
    } catch (err) {
      setError(err.message || 'Login failed.');
    } finally {
      setQuickLoginPending(null);
    }
  }

  async function handleRegister(e) {
    e.preventDefault();
    setError(null);
    setInfo(null);
    setPending(true);
    try {
      await authApi.register(registerForm);
      setInfo('Account created. You can log in now.');
      setLoginForm({ username: registerForm.username, password: '' });
      setMode('login');
    } catch (err) {
      setError(err.message || 'Registration failed.');
    } finally {
      setPending(false);
    }
  }

  return (
    <div className="login-page">
      <div className="login-card">
        <h1>Sprints</h1>
        <p className="login-subtitle">Sign in to plan sprints and track their tasks.</p>

        <div className="quick-login">
          <p className="quick-login-label">Quick login (demo accounts)</p>
          <div className="quick-login-grid">
            {DEMO_ACCOUNTS.map((account) => (
              <button
                key={account.username}
                type="button"
                className="quick-login-button"
                disabled={quickLoginPending !== null || pending}
                onClick={() => handleQuickLogin(account)}
              >
                {quickLoginPending === account.username ? 'Signing in…' : account.role}
              </button>
            ))}
          </div>
        </div>

        <div className="login-divider">
          <span>or sign in manually</span>
        </div>

        <div className="login-tabs">
          <button
            type="button"
            className={mode === 'login' ? 'active' : ''}
            onClick={() => {
              setMode('login');
              setError(null);
            }}
          >
            Log in
          </button>
          <button
            type="button"
            className={mode === 'register' ? 'active' : ''}
            onClick={() => {
              setMode('register');
              setError(null);
            }}
          >
            Create account
          </button>
        </div>

        {error && <div className="form-message error">{error}</div>}
        {info && <div className="form-message success">{info}</div>}

        {mode === 'login' ? (
          <form onSubmit={handleLogin} className="stacked-form">
            <label>
              Username
              <input
                required
                autoFocus
                value={loginForm.username}
                onChange={(e) => setLoginForm((f) => ({ ...f, username: e.target.value }))}
              />
            </label>
            <label>
              Password
              <input
                required
                type="password"
                value={loginForm.password}
                onChange={(e) => setLoginForm((f) => ({ ...f, password: e.target.value }))}
              />
            </label>
            <button
              type="submit"
              className="primary-button"
              disabled={pending || quickLoginPending !== null}
            >
              {pending ? 'Signing in…' : 'Sign in'}
            </button>
          </form>
        ) : (
          <form onSubmit={handleRegister} className="stacked-form">
            <label>
              Full name
              <input
                required
                value={registerForm.name}
                onChange={(e) => setRegisterForm((f) => ({ ...f, name: e.target.value }))}
              />
            </label>
            <label>
              Username
              <input
                required
                minLength={3}
                value={registerForm.username}
                onChange={(e) => setRegisterForm((f) => ({ ...f, username: e.target.value }))}
              />
            </label>
            <label>
              Email
              <input
                required
                type="email"
                value={registerForm.email}
                onChange={(e) => setRegisterForm((f) => ({ ...f, email: e.target.value }))}
              />
            </label>
            <label>
              Contact info
              <input
                value={registerForm.contactInfo}
                onChange={(e) => setRegisterForm((f) => ({ ...f, contactInfo: e.target.value }))}
              />
            </label>
            <label>
              Password
              <input
                required
                minLength={6}
                type="password"
                value={registerForm.password}
                onChange={(e) => setRegisterForm((f) => ({ ...f, password: e.target.value }))}
              />
            </label>
            <button type="submit" className="primary-button" disabled={pending}>
              {pending ? 'Creating…' : 'Create account'}
            </button>
          </form>
        )}
      </div>
    </div>
  );
}
