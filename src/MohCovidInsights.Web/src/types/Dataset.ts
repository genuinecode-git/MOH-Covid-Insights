import type { DatasetMetric } from './DatasetMetric';
import type { DatasetCoverageWindow } from './DatasetCoverageWindow';

export interface Dataset {
  id: string;
  name: string;
  tone: string;
  metrics: DatasetMetric[];
  coverage: DatasetCoverageWindow;
}
