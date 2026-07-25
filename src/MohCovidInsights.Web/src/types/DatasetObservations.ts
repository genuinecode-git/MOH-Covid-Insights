import type { EpiWeekInfo } from './EpiWeekInfo';
import type { Observation } from './Observation';

export interface DatasetObservations {
  datasetId: string;
  from: string;
  to: string;
  weeks: EpiWeekInfo[];
  observations: Observation[];
}
