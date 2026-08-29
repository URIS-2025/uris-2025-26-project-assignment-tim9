import { useParams } from 'react-router-dom';
import { useAuth } from '../../auth/useAuth';
import WorkPackageList from '../../components/WorkPackageList';
import TaskBoard from '../../components/TaskBoard';
import SubTaskTree from '../../components/SubTaskTree';
import TaskDetails from '../../components/TaskDetails';
import BacklogView from '../../components/BacklogView';
import './WorkPackagesPage.css';

export default function WorkPackagesPage() {
  const { projectId } = useParams();
  const { token } = useAuth();

  return (
    <section className="work-packages-page">
      <WorkPackageList projectId={projectId} token={token} />
      <TaskBoard workPackageId={projectId} />
      <SubTaskTree taskId={projectId} />
      <TaskDetails taskId={projectId} />
      <BacklogView projectId={projectId} />
    </section>
  );
}