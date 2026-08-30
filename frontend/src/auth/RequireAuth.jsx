import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from './useAuth';
import Nav from '../components/Nav';

// Wrap a route element to require a logged-in user, and optionally one of a
// set of roles. Redirects to /login (preserving where the user was headed).
// Also renders the shared Nav bar, since every authenticated page needs it.
export default function RequireAuth({ children, roles }) {
  const { isAuthenticated, role } = useAuth();
  const location = useLocation();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />;
  }

  if (roles && !roles.includes(role)) {
    return <Navigate to="/projects" replace />;
  }

  return (
    <>
      <Nav />
      {children}
    </>
  );
}
