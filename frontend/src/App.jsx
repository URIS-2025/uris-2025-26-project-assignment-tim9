import WorkPackageList from './features/workpackage/components/WorkPackageList'
import TaskBoard from './features/workpackage/components/TaskBoard'

function App() {
  return (
    <div>
      <WorkPackageList projectId="00000000-0000-0000-0000-000000000000" />
      <TaskBoard workPackageId="00000000-0000-0000-0000-000000000000" />
    </div>
  )
}

export default App
