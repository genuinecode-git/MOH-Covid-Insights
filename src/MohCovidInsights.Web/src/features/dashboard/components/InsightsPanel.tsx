import { Box, Paper, Stack, Typography } from '@mui/material';
import TrendIcon from '@mui/icons-material/TrendingUpRounded';
import HospitalIcon from '@mui/icons-material/ApartmentRounded';
import GaugeIcon from '@mui/icons-material/DonutLargeRounded';
import ChartIcon from '@mui/icons-material/BarChartRounded';
import type { Insight } from '../../../types';

const TONES = {
  infections:  { bg: '#EEF2FF', fg: '#4353E8', icon: <TrendIcon fontSize="small" /> },
  hospital:    { bg: '#F5F3FF', fg: '#8B5CF6', icon: <HospitalIcon fontSize="small" /> },
  icu:         { bg: '#FFF7ED', fg: '#F59E0B', icon: <GaugeIcon fontSize="small" /> },
  utilisation: { bg: '#ECFDF5', fg: '#10B981', icon: <ChartIcon fontSize="small" /> },
} as const;

export default function InsightsPanel({ insights }: { insights: Insight[] }) {
  return (
    <Paper sx={{ p: 2.5, height: '100%' }}>
      <Typography variant="h6" mb={2}>Insights (Selected Period)</Typography>
      <Stack spacing={1.25}>
        {insights.map((ins) => {
          const tone = TONES[ins.tone];
          return (
            <Stack key={ins.id} direction="row" spacing={1.5} alignItems="center"
              sx={{ p: 1.5, border: 1, borderColor: 'divider', borderRadius: 2 }}>
              <Box sx={{
                width: 34, height: 34, borderRadius: 1.5, flexShrink: 0,
                bgcolor: tone.bg, color: tone.fg, display: 'grid', placeItems: 'center',
              }}>
                {tone.icon}
              </Box>
              <Box minWidth={0}>
                <Typography variant="body2" color="text.secondary">{ins.headline}</Typography>
                <Typography variant="body2" fontWeight={600}>{ins.detail}</Typography>
              </Box>
            </Stack>
          );
        })}
      </Stack>
    </Paper>
  );
}