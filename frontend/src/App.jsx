import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import ProjectListPage from './pages/Projects/ProjectListPage'
import ProjectDetailsPage from './pages/Projects/ProjectDetailsPage'
import WorkPackagesPage from './pages/WorkPackages/WorkPackagePage'
import TimelogsPage from './pages/Timelogs/TimelogsPage'

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Navigate to="/projects" replace />} />
        <Route path="/projects" element={<ProjectListPage />} />
        <Route path="/projects/:id" element={<ProjectDetailsPage />} />
        <Route path="/projects/:projectId/work-packages" element={<WorkPackagesPage />} />
        <Route path="/projects/:projectId/timelogs" element={<TimelogsPage />} />
        <Route path="/timelogs" element={<TimelogsPage />} />
      </Routes>
    </BrowserRouter>
  )
}

export default App
