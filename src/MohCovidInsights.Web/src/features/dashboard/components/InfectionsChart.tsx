import {
  CartesianGrid, Legend, Line, LineChart, ResponsiveContainer, Tooltip, XAxis, YAxis,
} from 'recharts';
import { ChartTooltip } from './chartConfig';
import { AXIS, GRID, fmtCompact } from './chartConfig.constants';
import { palette } from '../../../theme/theme';
import type { WeeklyPoint } from '../../../types';

export default function InfectionsChart({ data }: { data: WeeklyPoint[] }) {
  return (
    <ResponsiveContainer width="100%" height="100%">
      <LineChart data={data} margin={{ top: 8, right: 8, left: -8, bottom: 18 }}>
        <CartesianGrid {...GRID} />
        <XAxis dataKey="epiWeek" {...AXIS} interval={5}
          label={{ value: 'Epi Week', position: 'insideBottom', offset: -12, fontSize: 11, fill: '#8A90A2' }} />
        <YAxis yAxisId="left" {...AXIS} tickFormatter={fmtCompact} />
        <YAxis yAxisId="right" orientation="right" {...AXIS} tickFormatter={fmtCompact} />
        <Tooltip content={<ChartTooltip />} />
        <Legend content={() => null} />
        <Line yAxisId="left" dataKey="estimatedInfections" name="Estimated Infections"
          stroke={palette.infections} strokeWidth={1.6} dot={{ r: 2 }} activeDot={{ r: 4 }} />
        <Line yAxisId="right" dataKey="hospitalAdmissions" name="Hospital Admissions"
          stroke={palette.hospital} strokeWidth={1.6} dot={{ r: 2 }} activeDot={{ r: 4 }} />
        <Line yAxisId="right" dataKey="icuAdmissions" name="ICU Admissions"
          stroke={palette.icu} strokeWidth={1.6} dot={{ r: 2 }} activeDot={{ r: 4 }} />
      </LineChart>
    </ResponsiveContainer>
  );
}