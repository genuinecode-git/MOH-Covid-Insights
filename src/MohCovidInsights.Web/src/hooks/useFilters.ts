import { useCallback, useMemo, useState } from 'react';
import type { DashboardFilters } from '../types';

export const DEFAULT_FILTERS: DashboardFilters = {
  fromYear: 2023,
  fromWeek: 1,
  toYear: 2024,
  toWeek: 4,
  ageGroup: 'All',
  clinicalStatus: 'All',
  showPercentChange: false,
};

function readFromUrl(): DashboardFilters {
  const p = new URLSearchParams(window.location.search);
  const num = (k: string, d: number) => Number(p.get(k) ?? d) || d;
  return {
    fromYear: num('fromYear', DEFAULT_FILTERS.fromYear),
    fromWeek: num('fromWeek', DEFAULT_FILTERS.fromWeek),
    toYear: num('toYear', DEFAULT_FILTERS.toYear),
    toWeek: num('toWeek', DEFAULT_FILTERS.toWeek),
    ageGroup: p.get('ageGroup') ?? DEFAULT_FILTERS.ageGroup,
    clinicalStatus: p.get('clinicalStatus') ?? DEFAULT_FILTERS.clinicalStatus,
    showPercentChange: p.get('pct') === '1',
  };
}

function writeToUrl(f: DashboardFilters) {
  const p = new URLSearchParams({
    fromYear: String(f.fromYear),
    fromWeek: String(f.fromWeek),
    toYear: String(f.toYear),
    toWeek: String(f.toWeek),
    ageGroup: f.ageGroup,
    clinicalStatus: f.clinicalStatus,
    pct: f.showPercentChange ? '1' : '0',
  });
  window.history.replaceState(null, '', `${window.location.pathname}?${p}`);
}

export function useFilters() {
  const initial = useMemo(() => readFromUrl(), []);
  const [applied, setApplied] = useState<DashboardFilters>(initial);
  const [draft, setDraft] = useState<DashboardFilters>(initial);

  const setField = useCallback(
    <K extends keyof DashboardFilters>(key: K, value: DashboardFilters[K]) =>
      setDraft((prev) => ({ ...prev, [key]: value })),
    [],
  );

  const apply = useCallback(() => {
    setApplied(draft);
    writeToUrl(draft);
  }, [draft]);

  const clear = useCallback(() => {
    setDraft(DEFAULT_FILTERS);
    setApplied(DEFAULT_FILTERS);
    writeToUrl(DEFAULT_FILTERS);
  }, []);

  const isDirty = useMemo(
    () => JSON.stringify(draft) !== JSON.stringify(applied),
    [draft, applied],
  );

  return { draft, applied, setField, apply, clear, isDirty };
}