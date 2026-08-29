import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import AuthProvider from './auth/AuthProvider'
import RequireAuth from './auth/RequireAuth'
import LoginPage from './pages/Auth/LoginPage'
import SignUpPage from './pages/Auth/SignUpPage'
import UsersListPage from './pages/Users/UsersListPage'
import ProjectListPage from './pages/Projects/ProjectListPage'
import ProjectDetailsPage from './pages/Projects/ProjectDetailsPage'
import WorkPackagesPage from './pages/WorkPackages/WorkPackagesPage'
import WorkPackageDetailPage from './pages/WorkPackages/WorkPackageDetailPage'
import BacklogPage from './pages/WorkPackages/BacklogPage'
import TimelogsPage from './pages/Timelogs/TimelogsPage'

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/" element={<Navigate to="/projects" replace />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/signup" element={<SignUpPage />} />
          <Route
            path="/users"
            element={
              <RequireAuth roles={['Admin']}>
                <UsersListPage />
              </RequireAuth>
            }
          />
          <Route
            path="/projects"
            element={
              <RequireAuth>
                <ProjectListPage />
              </RequireAuth>
            }
          />
          <Route
            path="/projects/:id"
            element={
              <RequireAuth>
                <ProjectDetailsPage />
              </RequireAuth>
            }
          />
          <Route
            path="/projects/:projectId/work-packages"
            element={
              <RequireAuth>
                <WorkPackagesPage />
              </RequireAuth>
            }
          />
          <Route
            path="/projects/:projectId/work-packages/:workPackageId"
            element={
              <RequireAuth>
                <WorkPackageDetailPage />
              </RequireAuth>
            }
          />
          <Route
            path="/projects/:projectId/backlog"
            element={
              <RequireAuth>
                <BacklogPage />
              </RequireAuth>
            }
          />
          <Route
            path="/projects/:projectId/timelogs"
            element={
              <RequireAuth>
                <TimelogsPage />
              </RequireAuth>
            }
          />
          <Route
            path="/timelogs"
            element={
              <RequireAuth>
                <TimelogsPage />
              </RequireAuth>
            }
          />
          <Route path="*" element={<Navigate to="/projects" replace />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  )
}

export default App