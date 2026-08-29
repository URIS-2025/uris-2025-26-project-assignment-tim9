import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import AuthProvider from './auth/AuthProvider'
import RequireAuth from './auth/RequireAuth'
import AppLayout from './components/layout/AppLayout'
import LoginPage from './pages/Auth/LoginPage'
import SignUpPage from './pages/Auth/SignUpPage'
import UsersListPage from './pages/Users/UsersListPage'
import ProjectListPage from './pages/Projects/ProjectListPage'
import ProjectDetailsPage from './pages/Projects/ProjectDetailsPage'
import WorkPackagesPage from './pages/WorkPackages/WorkPackagesPage'
import WorkPackageDetailPage from './pages/WorkPackages/WorkPackageDetailPage'
import BacklogPage from './pages/WorkPackages/BacklogPage'
import TimelogsPage from './pages/Timelogs/TimelogsPage'
import SprintsPage from './pages/SprintsPage'
import PaymentsPage from './pages/Payments/PaymentsPage'
import InvoiceDetailsPage from './pages/Payments/InvoiceDetailsPage'
import NotificationsPage from './pages/Notifications/NotificationsPage'
import IntegrationsPage from './pages/Integrations/IntegrationsPage'

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          {/* stranice bez okvira - korisnik jos nije prijavljen */}
          <Route path="/login" element={<LoginPage />} />
          <Route path="/signup" element={<SignUpPage />} />

          {/* sve ostalo trazi prijavu i zivi unutar sidebar/navbar okvira */}
          <Route
            element={
              <RequireAuth>
                <AppLayout />
              </RequireAuth>
            }
          >
            <Route path="/" element={<Navigate to="/projects" replace />} />

            <Route path="/projects" element={<ProjectListPage />} />
            <Route path="/projects/:id" element={<ProjectDetailsPage />} />
            <Route path="/projects/:projectId/work-packages" element={<WorkPackagesPage />} />
            <Route
              path="/projects/:projectId/work-packages/:workPackageId"
              element={<WorkPackageDetailPage />}
            />
            <Route path="/projects/:projectId/backlog" element={<BacklogPage />} />
            <Route path="/projects/:projectId/timelogs" element={<TimelogsPage />} />
            <Route path="/timelogs" element={<TimelogsPage />} />

            <Route path="/projects/:projectId/sprints" element={<SprintsPage />} />
            <Route path="/sprints" element={<SprintsPage />} />

            <Route path="/payments" element={<PaymentsPage />} />
            <Route path="/payments/:invoiceId" element={<InvoiceDetailsPage />} />

            <Route path="/notifications" element={<NotificationsPage />} />

            {/* rola se proverava dodatno, samo za ove rute */}
            <Route
              path="/users"
              element={
                <RequireAuth roles={['Admin']}>
                  <UsersListPage />
                </RequireAuth>
              }
            />
            <Route
              path="/integrations"
              element={
                <RequireAuth roles={['Admin']}>
                  <IntegrationsPage />
                </RequireAuth>
              }
            />

            <Route path="*" element={<Navigate to="/projects" replace />} />
          </Route>
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  )
}

export default App
