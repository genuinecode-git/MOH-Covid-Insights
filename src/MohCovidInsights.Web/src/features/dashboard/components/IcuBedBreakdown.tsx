import { Box, Paper, Stack, Typography } from '@mui/material';
import { Cell, Pie, PieChart, ResponsiveContainer } from 'recharts';
import { TONE_COLORS } from '../../../theme/tones';
import type { BedSegment, BedUtilisation } from '../../../types';

const getSegmentColor = (segment: BedSegment) => TONE_COLORS[segment.tone] ?? '#6B7280';

export default function IcuBedBreakdown({ data }: { data: BedUtilisation }) {
  return (
    <Paper sx={{ p: 2.5, height: '100%', display: 'flex', flexDirection: 'column' }}>
      <Typography variant="h6" mb={2}>ICU Bed Utilisation Breakdown (Latest Week)</Typography>
      <br/>
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} alignItems="center" flexGrow={1}>
        <Box sx={{ position: 'relative', width: 200, height: 200, flexShrink: 0 }}>
          <ResponsiveContainer width="100%" height="100%">
            <PieChart>
              <Pie data={data.segments} dataKey="beds" innerRadius={62} outerRadius={95}
                paddingAngle={1} startAngle={90} endAngle={-270} stroke="none">
                {data.segments.map((s) => <Cell key={s.label} fill={getSegmentColor(s)} />)}
              </Pie>
            </PieChart>
          </ResponsiveContainer>
          <Box sx={{
            position: 'absolute', inset: 0, display: 'grid',
            placeItems: 'center', pointerEvents: 'none',
          }}>
            <Box textAlign="center">
              <Typography sx={{ fontSize: '1.5rem', fontWeight: 700, lineHeight: 1.1 }}>
                {data.utilisationPct}%
              </Typography>
              <Typography variant="caption" color="text.secondary">Utilisation</Typography>
            </Box>
          </Box>
        </Box>

        <Stack spacing={2} sx={{ minWidth: 150 }}>
          {data.segments.map((s) => (
            <Stack key={s.label} direction="row" spacing={1.25}>
              <Box sx={{ width: 10, height: 10, borderRadius: '50%', bgcolor: getSegmentColor(s), mt: 0.5 }} />
              <Box>
                <Typography variant="body2" fontWeight={600}>{s.label}</Typography>
                <Typography variant="body2" color="text.secondary">
                  {s.beds} ({s.pct}%)
                </Typography>
              </Box>
            </Stack>
          ))}
          <Box>
            <Typography variant="body2" fontWeight={600}>Total Capacity</Typography>
            <Typography variant="body2" color="text.secondary">
              {data.totalCapacity.toLocaleString()} Beds
            </Typography>
          </Box>
        </Stack>
      </Stack>

      <Typography variant="caption" color="text.secondary" mt={2}>
        {data.weekLabel}
      </Typography>
    </Paper>
  );
}