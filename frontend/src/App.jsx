import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { AuthProvider } from './auth/AuthContext'
import { useAuth } from './auth/useAuth'
import LoginPage from './pages/LoginPage'
import SprintsPage from './pages/SprintsPage'
import ProjectListPage from './pages/Projects/ProjectListPage'
import ProjectDetailsPage from './pages/Projects/ProjectDetailsPage'
import WorkPackagesPage from './pages/WorkPackages/WorkPackagePage'
import TimelogsPage from './pages/Timelogs/TimelogsPage'

// LoginPage itself has no navigate() call - it was built for the original
// AppShell, which just swapped LoginPage out for SprintsPage as soon as
// isAuthenticated flipped, no routing involved. Now that /login is a real
// route, it needs an explicit way off itself once signed in (and, the
// other way around, straight past it if you're already signed in).
function LoginRoute() {
  const { isAuthenticated } = useAuth()
  return isAuthenticated ? <Navigate to="/projects" replace /> : <LoginPage />
}

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/login" element={<LoginRoute />} />
          <Route path="/" element={<Navigate to="/projects" replace />} />
          <Route path="/projects" element={<ProjectListPage />} />
          <Route path="/projects/:id" element={<ProjectDetailsPage />} />
          <Route path="/projects/:projectId/work-packages" element={<WorkPackagesPage />} />
          <Route path="/projects/:projectId/timelogs" element={<TimelogsPage />} />
          <Route path="/timelogs" element={<TimelogsPage />} />
          <Route path="/sprints" element={<SprintsPage />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  )
}

export default App
