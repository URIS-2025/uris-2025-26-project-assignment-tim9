import { BrowserRouter, Routes, Route } from 'react-router-dom'
import ProjectListPage from './pages/Projects/ProjectListPage'
import ProjectDetailsPage from './pages/Projects/ProjectDetailsPage'
import WorkPackagesPage from './pages/WorkPackages/WorkPackagesPage'
import WorkPackageDetailPage from './pages/WorkPackages/WorkPackageDetailPage'
import BacklogPage from './pages/WorkPackages/BacklogPage'

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/projects" element={<ProjectListPage />} />
        <Route path="/projects/:id" element={<ProjectDetailsPage />} />
        <Route path="/projects/:projectId/work-packages" element={<WorkPackagesPage />} />
        <Route path="/projects/:projectId/work-packages/:workPackageId" element={<WorkPackageDetailPage />} />
        <Route path="/projects/:projectId/backlog" element={<BacklogPage />} />
      </Routes>
    </BrowserRouter>
  )
}

export default App