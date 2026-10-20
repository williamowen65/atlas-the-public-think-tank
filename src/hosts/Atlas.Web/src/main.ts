import { createActionButton } from './components/ActionButton';
import './styles.css';

// A small composition proves reuse; this is not the future Atlas page design.
const root = document.querySelector<HTMLDivElement>('#app')!;
const heading = document.createElement('h1');
heading.textContent = 'Atlas component integration';
const status = document.createElement('p');
status.setAttribute('role', 'status');
status.textContent = 'No action yet';
root.append(heading, createActionButton({
  label: 'Try action',
  onActivate: () => { status.textContent = 'Action completed'; },
}), status);
