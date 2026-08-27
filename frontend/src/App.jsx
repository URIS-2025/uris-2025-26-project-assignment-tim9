import WorkPackageList from './features/workpackage/components/WorkPackageList'
import TaskBoard from './features/workpackage/components/TaskBoard'
import SubTaskTree from './features/workpackage/components/SubTaskTree'
import TaskDetails from './features/workpackage/components/TaskDetails'
import BacklogView from './features/workpackage/components/BacklogView'

function App() {
  return (
    <div>
      <WorkPackageList projectId="00000000-0000-0000-0000-000000000000" />
      <TaskBoard workPackageId="00000000-0000-0000-0000-000000000000" />
      <SubTaskTree taskId="00000000-0000-0000-0000-000000000000" />
      <TaskDetails taskId="00000000-0000-0000-0000-000000000000" />
      <BacklogView projectId="00000000-0000-0000-0000-000000000000" />
    </div>
  )
}

export default App
