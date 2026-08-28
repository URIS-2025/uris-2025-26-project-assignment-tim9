// Full-width panel used for the non-list states of the projects page:
// `variant="empty"` (no projects yet) and `variant="error"` (load failed).
// Pass `onRetry` to render the "Try again" button (error case).
export default function ProjectsState({ variant = 'empty', title, children, onRetry }) {
  return (
    <div
      className={`projects-state projects-state--${variant}`}
      role={variant === 'error' ? 'alert' : undefined}
    >
      <p className="projects-state__title">{title}</p>
      <p className="projects-state__text">{children}</p>
      {onRetry && (
        <button type="button" className="projects-retry" onClick={onRetry}>
          Try again
        </button>
      )}
    </div>
  );
}
