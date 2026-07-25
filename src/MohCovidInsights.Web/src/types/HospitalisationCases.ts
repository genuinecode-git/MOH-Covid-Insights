import type { HospitalisationCaseRow } from './HospitalisationCaseRow';

export interface HospitalisationCases {
  from: string;
  to: string;
  clinicalStatuses: string[];
  ageGroups: string[];
  rows: HospitalisationCaseRow[];
}
