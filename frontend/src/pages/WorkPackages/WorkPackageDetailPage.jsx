import { useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import TaskBoard from '../../components/TaskBoard';
import TaskDetails from '../../components/TaskDetails';
import Modal from '../../components/Modal';
import './WorkPackageDetailPage.css';

const MOCK_WORK_PACKAGE_NAMES = {
  1: 'Authentication and authorization',
  2: 'Task management',
  3: 'Notifications',
};

export default function WorkPackageDetailPage() {
  const { projectId, workPackageId } = useParams();
  const [selectedTask, setSelectedTask] = useState(null);

  const name = MOCK_WORK_PACKAGE_NAMES[workPackageId] ?? `Work Package #${workPackageId}`;

  return (
    <section className="wp-detail">
      <Link to={`/projects/${projectId}/work-packages`} className="wp-detail__back">
        ← Back to Work Packages
      </Link>

      <h1>{name}</h1>

      <TaskBoard workPackageId={workPackageId} onTaskClick={setSelectedTask} />

      {selectedTask && (
        <Modal title={selectedTask.title} onClose={() => setSelectedTask(null)}>
          <TaskDetails taskId={selectedTask.id} />
        </Modal>
      )}
    </section>
  );
}
