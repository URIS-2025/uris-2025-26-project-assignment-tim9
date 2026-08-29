import { useState } from 'react';
import './CollapsibleSection.css';

export default function CollapsibleSection({ title, action, defaultOpen = false, children }) {
  const [open, setOpen] = useState(defaultOpen);

  return (
    <section className={open ? 'collapsible is-open' : 'collapsible'}>
      <div className="collapsible__bar">
        <button
          type="button"
          className="collapsible__toggle"
          aria-expanded={open}
          onClick={() => setOpen((value) => !value)}
        >
          <span className="collapsible__chevron" aria-hidden="true" />
          <span className="collapsible__title">{title}</span>
        </button>
        {action && <div className="collapsible__action">{action}</div>}
      </div>

      {open && <div className="collapsible__body">{children}</div>}
    </section>
  );
}
