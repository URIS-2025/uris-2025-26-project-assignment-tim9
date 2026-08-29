import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { useAuth } from '../../auth/useAuth';
import { getProjectById, deleteProject } from '../../api/projectApi';
import { resolveStatus } from '../../utils/projectStatus';
import ProjectStatusBadge from '../../components/ProjectStatusBadge';
import ProjectsState from '../../components/ProjectsState';
import MilestoneList from '../../components/MilestoneList';
import RequirementList from '../../components/RequirementList';
import ProjectMemberList from '../../components/ProjectMemberList';
import Modal from '../../components/Modal';
import CollapsibleSection from '../../components/CollapsibleSection';
import ProjectForm from '../../components/ProjectForm';
import MilestoneForm from '../../components/MilestoneForm';
import RequirementForm from '../../components/RequirementForm';
import ProjectMemberForm from '../../components/ProjectMemberForm';
import AttachmentsButton from '../../components/AttachmentsButton';
import './ProjectListPage.css';
import './ProjectDetailsPage.css';

const numberFormat = new Intl.NumberFormat();
const dateFormat = new Intl.DateTimeFormat(undefined, {
  day: 'numeric',
  month: 'short',
  year: 'numeric',
});
const dateTimeFormat = new Intl.DateTimeFormat(undefined, {
  day: 'numeric',
  month: 'short',
  year: 'numeric',
  hour: '2-digit',
  minute: '2-digit',
});

function formatDate(value, formatter) {
  if (!value) return null;
  const parsed = new Date(value);
  return Number.isNaN(parsed.getTime()) ? null : formatter.format(parsed);
}

function isDeadlineOverdue(deadline, statusKey) {
  if (!deadline || statusKey === 'Completed' || statusKey === 'Cancelled') return false;
  const time = new Date(deadline).getTime();
  return !Number.isNaN(time) && time < Date.now();
}

function DetailRow({ label, children, muted = false }) {
  return (
    <div className="pd-row">
      <dt className="pd-row__label">{label}</dt>
      <dd className={muted ? 'pd-row__value is-muted' : 'pd-row__value'}>{children}</dd>
    </div>
  );
}

function LineIcon({ children }) {
  return (
    <svg
      viewBox="0 0 20 20"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.5"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      {children}
    </svg>
  );
}

function WorkPackagesIcon() {
  return (
    <LineIcon>
      <rect x="3.5" y="3.5" width="13" height="13" rx="2" />
      <path d="M6.8 8l1.7 1.7 3.2-3.4" />
      <path d="M6.8 12.8h6.4" />
    </LineIcon>
  );
}

function SprintsIcon() {
  return (
    <LineIcon>
      <rect x="3" y="4.4" width="14" height="12.2" rx="1.8" />
      <path d="M3 8h14" />
      <path d="M7 2.8v3.2" />
      <path d="M13 2.8v3.2" />
    </LineIcon>
  );
}

function TimelogsIcon() {
  return (
    <LineIcon>
      <circle cx="10" cy="10" r="7" />
      <path d="M10 6v4.2l2.6 1.6" />
    </LineIcon>
  );
}

function RelatedCard({ to, label, icon }) {
  return (
    <Link to={to} className="pd-related__card">
      <span className="pd-related__card-icon" aria-hidden="true">
        {icon}
      </span>
      <span className="pd-related__card-label">{label}</span>
    </Link>
  );
}

function ProjectDetailsSkeleton() {
  return (
    <article className="pd-card pd-card--skeleton" aria-hidden="true">
      <dl className="pd-rows">
        {Array.from({ length: 4 }).map((_, index) => (
          <div className="pd-row" key={index}>
            <span className="pd-skeleton pd-skeleton--label" />
            <span className="pd-skeleton pd-skeleton--line" />
          </div>
        ))}
      </dl>
    </article>
  );
}

export default function ProjectDetailsPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { token, role } = useAuth();
  const canManage = role === 'Admin' || role === 'ProjectManager';
  const isAdmin = role === 'Admin';
  const [project, setProject] = useState(null);
  const [phase, setPhase] = useState('loading');
  const [errorMessage, setErrorMessage] = useState('');
  const [reloadKey, setReloadKey] = useState(0);
  const [showMilestoneForm, setShowMilestoneForm] = useState(false);
  const [milestoneReload, setMilestoneReload] = useState(0);
  const [showRequirementForm, setShowRequirementForm] = useState(false);
  const [requirementReload, setRequirementReload] = useState(0);
  const [showMemberForm, setShowMemberForm] = useState(false);
  const [memberReload, setMemberReload] = useState(0);
  const [showEditForm, setShowEditForm] = useState(false);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [deleteError, setDeleteError] = useState('');

  useEffect(() => {
    let ignore = false;

    getProjectById(id, token)
      .then((data) => {
        if (ignore) return;
        if (!data) {
          setPhase('notfound');
          return;
        }
        setProject(data);
        setErrorMessage('');
        setPhase('ready');
      })
      .catch((error) => {
        if (ignore) return;
        if (error && error.status === 404) {
          setPhase('notfound');
          return;
        }
        setErrorMessage(
          error && error.status === 401
            ? 'Your session has expired. Please sign in again to see this project.'
            : 'Something went wrong while loading the project. Check your connection and try again.'
        );
        setPhase('error');
      });

    return () => {
      ignore = true;
    };
  }, [id, token, reloadKey]);

  const reload = () => {
    setPhase('loading');
    setErrorMessage('');
    setReloadKey((key) => key + 1);
  };

  const handleDelete = async () => {
    setDeleting(true);
    setDeleteError('');
    try {
      await deleteProject(project.projectId, token);
      navigate('/projects');
    } catch (error) {
      const status = error && error.status;
      setDeleteError(
        status === 403
          ? "You don't have permission to delete this project."
          : (error && error.message) ||
              'Something went wrong while deleting the project. Please try again.'
      );
      setDeleting(false);
    }
  };

  const deadline = project && formatDate(project.deadline, dateFormat);
  const createdAt = project && formatDate(project.createdAt, dateTimeFormat);
  const deadlineOverdue =
    project && isDeadlineOverdue(project.deadline, resolveStatus(project.status).key);

  return (
    <section className="project-details-page">
      <header className="pd-header">
        <Link to="/projects" className="pd-back">
          <span aria-hidden="true">&larr;</span> Projects
        </Link>
        <div className="pd-title-row">
          <h1 className="pd-title">{phase === 'ready' ? project.name : 'Project details'}</h1>
          {phase === 'ready' && (canManage || isAdmin) && (
            <div className="pd-title-actions">
              {canManage && (
                <button
                  type="button"
                  className="pd-edit-button"
                  onClick={() => setShowEditForm(true)}
                >
                  Edit Project
                </button>
              )}
              {isAdmin && (
                <button
                  type="button"
                  className="pd-delete-button"
                  onClick={() => {
                    setDeleteError('');
                    setShowDeleteConfirm(true);
                  }}
                >
                  Delete Project
                </button>
              )}
            </div>
          )}
        </div>
      </header>

      {phase === 'loading' && (
        <>
          <p className="pd-visually-hidden" role="status">
            Loading project
          </p>
          <ProjectDetailsSkeleton />
        </>
      )}

      {phase === 'error' && (
        <ProjectsState variant="error" title={'We couldn’t load the project'} onRetry={reload}>
          {errorMessage}
        </ProjectsState>
      )}

      {phase === 'notfound' && (
        <ProjectsState variant="empty" title="Project not found">
          We couldn’t find a project with that id. It may have been removed.
        </ProjectsState>
      )}

      {phase === 'ready' && (
        <>
          <article className="pd-card">
            <dl className="pd-rows">
              <DetailRow label="Status">
                <ProjectStatusBadge status={project.status} />
              </DetailRow>
              <DetailRow label="Budget">
                <span className="pd-budget">{numberFormat.format(project.budget ?? 0)}</span>
              </DetailRow>
              <DetailRow label="Deadline" muted={!deadline}>
                <span className="pd-deadline-value">
                  <span>{deadline ?? 'Not set'}</span>
                  {deadlineOverdue && <span className="pd-overdue">Overdue</span>}
                </span>
              </DetailRow>
              <DetailRow label="Created" muted={!createdAt}>
                {createdAt ?? 'Unknown'}
              </DetailRow>
            </dl>
          </article>

          <section className="pd-related">
            <h2 className="pd-related__title">Related</h2>

            <div className="pd-related__attachments">
              <AttachmentsButton projectId={project.projectId} label="Attachments" />
            </div>

            <div className="pd-related__grid">
              <RelatedCard
                to={`/projects/${project.projectId}/work-packages`}
                label="Work Packages"
                icon={<WorkPackagesIcon />}
              />
              <RelatedCard
                to={`/projects/${project.projectId}/sprints`}
                label="Sprints"
                icon={<SprintsIcon />}
              />
              <RelatedCard
                to={`/projects/${project.projectId}/timelogs`}
                label="Timelogs"
                icon={<TimelogsIcon />}
              />
            </div>
          </section>

          <CollapsibleSection
            title="Milestones"
            action={
              canManage ? (
                <button
                  type="button"
                  className="section-add-button"
                  onClick={() => setShowMilestoneForm(true)}
                >
                  Add Milestone
                </button>
              ) : null
            }
          >
            <MilestoneList
              projectId={project.projectId}
              token={token}
              reloadSignal={milestoneReload}
              canManage={canManage}
            />
          </CollapsibleSection>

          <CollapsibleSection
            title="Requirements"
            action={
              canManage ? (
                <button
                  type="button"
                  className="section-add-button"
                  onClick={() => setShowRequirementForm(true)}
                >
                  Add Requirement
                </button>
              ) : null
            }
          >
            <RequirementList
              projectId={project.projectId}
              token={token}
              reloadSignal={requirementReload}
              canManage={canManage}
            />
          </CollapsibleSection>

          <CollapsibleSection
            title="Members"
            action={
              canManage ? (
                <button
                  type="button"
                  className="section-add-button"
                  onClick={() => setShowMemberForm(true)}
                >
                  Add Member
                </button>
              ) : null
            }
          >
            <ProjectMemberList
              projectId={project.projectId}
              token={token}
              reloadSignal={memberReload}
              canManage={canManage}
            />
          </CollapsibleSection>
        </>
      )}

      {showEditForm && project && (
        <Modal title="Edit Project" onClose={() => setShowEditForm(false)}>
          <ProjectForm
            mode="edit"
            project={project}
            token={token}
            onCancel={() => setShowEditForm(false)}
            onSaved={() => {
              setShowEditForm(false);
              reload();
            }}
          />
        </Modal>
      )}

      {showMilestoneForm && project && (
        <Modal title="Add Milestone" onClose={() => setShowMilestoneForm(false)}>
          <MilestoneForm
            projectId={project.projectId}
            token={token}
            onCancel={() => setShowMilestoneForm(false)}
            onCreated={() => {
              setShowMilestoneForm(false);
              setMilestoneReload((key) => key + 1);
            }}
          />
        </Modal>
      )}

      {showRequirementForm && project && (
        <Modal title="Add Requirement" onClose={() => setShowRequirementForm(false)}>
          <RequirementForm
            projectId={project.projectId}
            token={token}
            onCancel={() => setShowRequirementForm(false)}
            onCreated={() => {
              setShowRequirementForm(false);
              setRequirementReload((key) => key + 1);
            }}
          />
        </Modal>
      )}

      {showMemberForm && project && (
        <Modal title="Add Member" onClose={() => setShowMemberForm(false)}>
          <ProjectMemberForm
            projectId={project.projectId}
            token={token}
            onCancel={() => setShowMemberForm(false)}
            onCreated={() => {
              setShowMemberForm(false);
              setMemberReload((key) => key + 1);
            }}
          />
        </Modal>
      )}

      {showDeleteConfirm && project && (
        <Modal
          title="Delete Project"
          onClose={() => {
            if (!deleting) setShowDeleteConfirm(false);
          }}
        >
          <p className="pd-confirm__text">
            Are you sure you want to delete this project? This will also delete all its
            milestones, requirements, and members.
          </p>

          {deleteError && (
            <p className="pd-delete-error" role="alert">
              {deleteError}
            </p>
          )}

          <div className="modal-actions">
            <button
              type="button"
              className="secondary-button"
              onClick={() => setShowDeleteConfirm(false)}
              disabled={deleting}
            >
              Cancel
            </button>
            <button
              type="button"
              className="pd-delete-button"
              onClick={handleDelete}
              disabled={deleting}
            >
              {deleting ? 'Deleting…' : 'Delete Project'}
            </button>
          </div>
        </Modal>
      )}
    </section>
  );
}
