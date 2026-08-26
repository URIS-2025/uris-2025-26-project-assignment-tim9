import { AuthProvider } from './auth/AuthContext';
import { useAuth } from './auth/useAuth';
import LoginPage from './pages/LoginPage';
import SprintsPage from './pages/SprintsPage';
import './App.css';

function AppShell() {
  const { isAuthenticated } = useAuth();
  return isAuthenticated ? <SprintsPage /> : <LoginPage />;
}

function App() {
  return (
    <AuthProvider>
      <AppShell />
    </AuthProvider>
  );
}

export default App;
