import {
  Bar, CartesianGrid, ComposedChart, Line, ResponsiveContainer, Tooltip, XAxis, YAxis,
} from 'recharts';
import { ChartTooltip } from './chartConfig';
import { AXIS, GRID, fmtCompact } from './chartConfig.constants';
import { palette } from '../../../theme/theme';
import type { WeeklyPoint } from '../../../types';

export default function HealthcarePressureChart({ data }: { data: WeeklyPoint[] }) {
  return (
    <ResponsiveContainer width="100%" height="100%">
      <ComposedChart data={data} margin={{ top: 8, right: 8, left: -8, bottom: 18 }}>
        <CartesianGrid {...GRID} />
        <XAxis dataKey="epiWeek" {...AXIS} interval={5}
          label={{ value: 'Epi Week', position: 'insideBottom', offset: -12, fontSize: 11, fill: '#8A90A2' }} />
        <YAxis yAxisId="left" {...AXIS} tickFormatter={fmtCompact} />
        <YAxis yAxisId="right" orientation="right" domain={[0, 100]} {...AXIS}
          tickFormatter={(v: number) => `${v}%`} />
        <Tooltip content={<ChartTooltip suffixes={{ 'ICU Utilisation %': '%' }} />} />
        <Bar yAxisId="left" dataKey="avgHospitalisedCases" name="Avg Hospitalised Cases"
          fill={palette.hospitalisedAvg} barSize={5} radius={[2, 2, 0, 0]} />
        <Bar yAxisId="left" dataKey="avgIcuCases" name="Avg ICU Cases"
          fill={palette.icuAvg} barSize={5} radius={[2, 2, 0, 0]} />
        <Line yAxisId="right" dataKey="icuUtilisationPct" name="ICU Utilisation %"
          stroke={palette.utilisation} strokeWidth={1.6} dot={{ r: 2 }} activeDot={{ r: 4 }} />
      </ComposedChart>
    </ResponsiveContainer>
  );
}