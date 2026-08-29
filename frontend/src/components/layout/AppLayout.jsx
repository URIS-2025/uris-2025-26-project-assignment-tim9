import { Outlet } from 'react-router-dom';
import Sidebar from './Sidebar';
import Navbar from './Navbar';
import './layout.css';

// Okvir oko svih prijavljenih stranica: sidebar levo, navbar gore,
// a stranice se renderuju kroz Outlet u sadrzajnom delu.
export default function AppLayout() {
  return (
    <div className="app-shell">
      <Sidebar />
      <div className="app-main">
        <Navbar />
        <main className="app-content">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
