import { useAuth } from '../../auth/useAuth';

function initials(name) {
  if (!name) return '?';
  return name.slice(0, 2).toUpperCase();
}

export default function Navbar() {
  const { username, role, logout } = useAuth();

  return (
    <header className="navbar">
      <div className="navbar-user">
        <span className="navbar-avatar" aria-hidden="true">
          {initials(username)}
        </span>
        <span className="navbar-identity">
          <span className="navbar-username">{username}</span>
          <span className="navbar-role">{role}</span>
        </span>
      </div>

      <button type="button" className="secondary-button navbar-logout" onClick={logout}>
        Log out
      </button>
    </header>
  );
}
