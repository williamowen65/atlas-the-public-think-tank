import $ from 'jquery';
import installSelect2 from 'select2';
import 'select2/dist/css/select2.css';
import './nodeTypeSelect.css';
import { customNodeTypeDescription } from '../api/nodePage';

type Settings = {
  label: string;
  value: string;
  focus?: boolean;
};

/** Enhance the native creation control while keeping its form value and change event. */
export function nodeTypeSelect(element: HTMLSelectElement, settings: Settings) {
  if (!$.fn.select2) installSelect2(window, $);

  const select = $(element);
  select.select2({
    width: '100%',
    dropdownCssClass: 'atlas-node-type-results',
    templateResult(option) {
      const optionElement = (option as { element?: HTMLOptionElement }).element;
      const isCustomType = optionElement?.value === 'new';
      const description = isCustomType ? customNodeTypeDescription : optionElement?.dataset.description;
      if (!description) return option.text;
      const result = $('<div>').addClass('atlas-node-type-result');
      if (isCustomType) result.addClass('atlas-custom-node-type');
      return result.append(
        $('<strong>').text(option.text),
        $('<div>').addClass('atlas-node-type-description').text(description),
      );
    },
  });

  const selection = select.next('.select2').find('.select2-selection');
  selection.attr({ 'aria-label': settings.label, 'aria-labelledby': null });
  select.on('change.atlasNodeType', (event) => {
    if (!event.originalEvent) element.dispatchEvent(new Event('change', { bubbles: true }));
  });
  select.on('select2:opening.atlasNodeType', (event) => {
    if (element.matches(':disabled')) event.preventDefault();
  });

  if (settings.focus) {
    requestAnimationFrame(() => {
      if (!element.isConnected) return;
      selection.get(0)?.focus({ preventScroll: true });
      element.closest('form')?.scrollIntoView({
        block: 'start',
        behavior: matchMedia('(prefers-reduced-motion: reduce)').matches ? 'instant' : 'smooth',
      });
    });
  }

  return {
    update(next: Settings) {
      select.val(next.value).trigger('change.select2');
    },
    destroy() {
      select.off('.atlasNodeType');
      select.select2('destroy');
    },
  };
}
