import { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../../auth/useAuth';
import '../../shared/styles/forms.css';
import './LoginPage.css';

export default function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const from = location.state?.from ?? '/projects';

  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');

  const handleSubmit = async (event) => {
    event.preventDefault();
    if (!username.trim() || !password) {
      setErrorMessage('Please enter both a username and a password.');
      return;
    }

    setSubmitting(true);
    setErrorMessage('');
    try {
      await login(username.trim(), password);
      navigate(from, { replace: true });
    } catch (error) {
      const httpStatus = error && error.status;
      setErrorMessage(
        httpStatus === 401
          ? 'Incorrect username or password.'
          : httpStatus === 503
            ? 'The user service is unavailable right now. Please try again shortly.'
            : httpStatus === 0
              ? error.message
              : 'Something went wrong while signing in. Please try again.'
      );
      setSubmitting(false);
    }
  };

  return (
    <section className="login-page">
      <div className="login-card">
        <h1 className="login-title">Sign in</h1>
        <p className="login-subtitle">Use your project account to continue.</p>

        <form className="stacked-form" onSubmit={handleSubmit} noValidate>
          {errorMessage && (
            <p className="form-message error" role="alert">
              {errorMessage}
            </p>
          )}

          <label>
            Username
            <input
              type="text"
              value={username}
              onChange={(event) => setUsername(event.target.value)}
              autoComplete="username"
              autoFocus
              required
            />
          </label>

          <label>
            Password
            <input
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              autoComplete="current-password"
              required
            />
          </label>

          <button type="submit" className="primary-button" disabled={submitting}>
            {submitting ? 'Signing in…' : 'Sign in'}
          </button>
        </form>
      </div>
    </section>
  );
}
