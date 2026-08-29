import { BrowserRouter, Routes, Route } from 'react-router-dom'
import ProjectListPage from './pages/Projects/ProjectListPage'
import ProjectDetailsPage from './pages/Projects/ProjectDetailsPage'

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/projects" element={<ProjectListPage />} />
        <Route path="/projects/:id" element={<ProjectDetailsPage />} />
      </Routes>
    </BrowserRouter>
  )
}

export default App