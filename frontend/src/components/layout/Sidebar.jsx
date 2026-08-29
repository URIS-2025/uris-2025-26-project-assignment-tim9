import { NavLink } from 'react-router-dom';
import { useAuth } from '../../auth/useAuth';

// Ikone su ubacene direktno jer projekat nema biblioteku ikona,
// a dodavanje zavisnosti zbog cetiri simbola se ne isplati.
const icons = {
  projects: (
    <path d="M3 5.5A1.5 1.5 0 0 1 4.5 4h3.2a1.5 1.5 0 0 1 1.2.6l.9 1.2h6.7A1.5 1.5 0 0 1 18 7.3v7.2A1.5 1.5 0 0 1 16.5 16h-12A1.5 1.5 0 0 1 3 14.5v-9Z" />
  ),
  timelogs: (
    <>
      <circle cx="10" cy="10" r="7" />
      <path d="M10 6v4.2l2.6 1.6" />
    </>
  ),
  payments: (
    <>
      <rect x="2.8" y="5" width="14.4" height="10" rx="1.6" />
      <path d="M2.8 8.4h14.4" />
    </>
  ),
  users: (
    <>
      <circle cx="8" cy="7.6" r="2.8" />
      <path d="M3.4 16c0-2.5 2.1-4.2 4.6-4.2s4.6 1.7 4.6 4.2" />
      <path d="M13.6 5.2a2.6 2.6 0 0 1 0 5" />
      <path d="M14.6 11.9c1.4.5 2.4 1.7 2.4 3.4" />
    </>
  ),
};

function Icon({ name }) {
  return (
    <svg
      className="sidebar-icon"
      viewBox="0 0 20 20"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.5"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      {icons[name]}
    </svg>
  );
}

export default function Sidebar() {
  const { role } = useAuth();

  //Users panel je samo za administratora, isto pravilo kao na ruti
  const links = [
    { to: '/projects', label: 'Projects', icon: 'projects' },
    { to: '/timelogs', label: 'My timelogs', icon: 'timelogs' },
    { to: '/payments', label: 'Billing', icon: 'payments' },
    ...(role === 'Admin' ? [{ to: '/users', label: 'Users', icon: 'users' }] : []),
  ];

  return (
    <aside className="sidebar">
      <div className="sidebar-brand">
        <span className="sidebar-brand-mark">URIS</span>
        <span className="sidebar-brand-name">Project Management</span>
      </div>

      <nav className="sidebar-nav" aria-label="Main navigation">
        {links.map((link) => (
          <NavLink
            key={link.to}
            to={link.to}
            className={({ isActive }) =>
              isActive ? 'sidebar-link sidebar-link-active' : 'sidebar-link'
            }
          >
            <Icon name={link.icon} />
            <span>{link.label}</span>
          </NavLink>
        ))}
      </nav>
    </aside>
  );
}
