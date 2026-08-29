import { BrowserRouter, Routes, Route } from 'react-router-dom'
import ProjectListPage from './pages/Projects/ProjectListPage'
import ProjectDetailsPage from './pages/Projects/ProjectDetailsPage'
import WorkPackagesPage from './pages/WorkPackages/WorkPackagesPage'

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/projects" element={<ProjectListPage />} />
        <Route path="/projects/:id" element={<ProjectDetailsPage />} />
        <Route path="/projects/:projectId/work-packages" element={<WorkPackagesPage />} />
      </Routes>
    </BrowserRouter>
  )
}

export default App