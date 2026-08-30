import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useAuth } from '../../auth/useAuth';
import { getWorkPackage } from '../../api/workPackageApi';
import TaskBoard from '../../components/TaskBoard';
import TaskDetails from '../../components/TaskDetails';
import Modal from '../../components/Modal';
import './WorkPackageDetailPage.css';

export default function WorkPackageDetailPage() {
  const { projectId, workPackageId } = useParams();
  const { token } = useAuth();
  const [selectedTask, setSelectedTask] = useState(null);
  const [name, setName] = useState(`Work Package #${workPackageId}`);
  const [boardKey, setBoardKey] = useState(0);

  useEffect(() => {
    let ignore = false;
    getWorkPackage(workPackageId, token)
      .then((wp) => {
        if (!ignore && wp?.name) setName(wp.name);
      })
      .catch(() => {});
    return () => {
      ignore = true;
    };
  }, [workPackageId, token]);

  return (
    <section className="wp-detail">
      <Link to={`/projects/${projectId}/work-packages`} className="wp-detail__back">
        ← Back to Work Packages
      </Link>

      <h1>{name}</h1>

      <TaskBoard key={boardKey} workPackageId={workPackageId} onTaskClick={setSelectedTask} />

      {selectedTask && (
        <Modal title={selectedTask.title} onClose={() => setSelectedTask(null)}>
          <TaskDetails
            taskId={selectedTask.id}
            onChanged={() => setBoardKey((k) => k + 1)}
          />
        </Modal>
      )}
    </section>
  );
}
