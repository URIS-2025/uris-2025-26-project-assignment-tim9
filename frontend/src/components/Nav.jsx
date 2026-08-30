import { NavLink } from 'react-router-dom';
import { useAuth } from '../auth/useAuth';
import './Nav.css';

export default function Nav() {
  const { username, role, logout } = useAuth();

  return (
    <nav className="app-nav">
      <div className="app-nav-links">
        <NavLink to="/projects" className={({ isActive }) => (isActive ? 'app-nav-link is-active' : 'app-nav-link')}>
          Projects
        </NavLink>
        {role === 'Admin' && (
          <NavLink to="/users" className={({ isActive }) => (isActive ? 'app-nav-link is-active' : 'app-nav-link')}>
            Users
          </NavLink>
        )}
      </div>
      <div className="app-nav-account">
        <span className="app-nav-username">
          {username} <span className="app-nav-role">{role}</span>
        </span>
        <button type="button" className="app-nav-logout" onClick={logout}>
          Sign out
        </button>
      </div>
    </nav>
  );
}
