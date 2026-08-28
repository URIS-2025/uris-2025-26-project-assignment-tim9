// Empty / error panel for the projects page
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
