import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../auth/useAuth';
import { registerUser } from '../../api/userApi';
import '../../shared/styles/forms.css';
import './LoginPage.css';

export default function SignUpPage() {
  const { login } = useAuth();
  const navigate = useNavigate();

  const [name, setName] = useState('');
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [contactInfo, setContactInfo] = useState('');
  const [password, setPassword] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');

  const trimmedName = name.trim();
  const trimmedUsername = username.trim();
  const trimmedEmail = email.trim();

  const nameError = trimmedName ? '' : 'Name is required.';
  const usernameError = !trimmedUsername
    ? 'Username is required.'
    : trimmedUsername.length < 3
      ? 'Username must be at least 3 characters.'
      : '';
  const emailError = !trimmedEmail
    ? 'Email is required.'
    : !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(trimmedEmail)
      ? 'Enter a valid email address.'
      : '';
  const passwordError = !password
    ? 'Password is required.'
    : password.length < 6
      ? 'Password must be at least 6 characters.'
      : '';

  const hasErrors = Boolean(nameError || usernameError || emailError || passwordError);

  const handleSubmit = async (event) => {
    event.preventDefault();
    if (hasErrors) {
      setErrorMessage('Please fix the highlighted fields.');
      return;
    }

    setSubmitting(true);
    setErrorMessage('');
    try {
      await registerUser({
        name: trimmedName,
        username: trimmedUsername,
        email: trimmedEmail,
        contactInfo: contactInfo.trim(),
        password,
      });
      // New accounts start active, so sign the person straight in.
      await login(trimmedUsername, password);
      navigate('/projects', { replace: true });
    } catch (error) {
      const httpStatus = error && error.status;
      setErrorMessage(
        httpStatus === 400
          ? error.message || 'That username or email is already taken.'
          : httpStatus === 0
            ? error.message
            : 'Something went wrong while creating your account. Please try again.'
      );
      setSubmitting(false);
    }
  };

  return (
    <section className="login-page">
      <div className="login-card">
        <h1 className="login-title">Create account</h1>
        <p className="login-subtitle">
          New accounts start as a Team Member. An admin can change your role later.
        </p>

        <form className="stacked-form" onSubmit={handleSubmit} noValidate>
          {errorMessage && (
            <p className="form-message error" role="alert">
              {errorMessage}
            </p>
          )}

          <label>
            Full name
            <input
              type="text"
              value={name}
              onChange={(event) => setName(event.target.value)}
              maxLength={200}
              autoComplete="name"
              autoFocus
              required
            />
          </label>

          <label>
            Username
            <input
              type="text"
              value={username}
              onChange={(event) => setUsername(event.target.value)}
              maxLength={50}
              autoComplete="username"
              required
            />
            {usernameError && <span className="field-hint error">{usernameError}</span>}
          </label>

          <label>
            Email
            <input
              type="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              autoComplete="email"
              required
            />
          </label>

          <label>
            Contact info (optional)
            <input
              type="text"
              value={contactInfo}
              onChange={(event) => setContactInfo(event.target.value)}
              maxLength={50}
              placeholder="Phone number"
              autoComplete="tel"
            />
          </label>

          <label>
            Password
            <input
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              autoComplete="new-password"
              required
            />
            {passwordError && <span className="field-hint error">{passwordError}</span>}
          </label>

          <button type="submit" className="primary-button" disabled={submitting}>
            {submitting ? 'Creating account…' : 'Create account'}
          </button>
        </form>

        <p className="login-switch">
          Already have an account? <Link to="/login">Sign in</Link>
        </p>
      </div>
    </section>
  );
}
