import { useState } from 'react';
import type { Props } from './ChartCardTypes';
import {
  Box, IconButton, Menu, MenuItem, Paper, Stack, Tooltip, Typography,
} from '@mui/material';
import InfoIcon from '@mui/icons-material/InfoOutlined';
import MoreIcon from '@mui/icons-material/MoreVertRounded';

export default function ChartCard({ title, hint, series, height = 300, children }: Props) {
  const [anchor, setAnchor] = useState<null | HTMLElement>(null);

  return (
    <Paper sx={{ p: 2.5, display: 'flex', flexDirection: 'column', height: '100%' }}>
      <Stack direction="row" alignItems="center" spacing={0.75} mb={1.5}>
        <Typography variant="h6">{title}</Typography>
        {hint && (
          <Tooltip title={hint}>
            <InfoIcon sx={{ fontSize: 16, color: 'text.secondary' }} />
          </Tooltip>
        )}
        <Box flexGrow={1} />
        <IconButton size="small" aria-label={`${title} options`} onClick={(e) => setAnchor(e.currentTarget)}>
          <MoreIcon fontSize="small" />
        </IconButton>
        <Menu anchorEl={anchor} open={Boolean(anchor)} onClose={() => setAnchor(null)}>
          <MenuItem onClick={() => setAnchor(null)}>Download CSV</MenuItem>
          <MenuItem onClick={() => setAnchor(null)}>Copy image</MenuItem>
        </Menu>
      </Stack>

      {series && (
        <Stack direction="row" flexWrap="wrap" gap={2.5} mb={1.5}>
          {series.map((s) => (
            <Stack key={s.label} direction="row" alignItems="center" spacing={0.75}>
              <Box sx={{ width: 14, height: 3, borderRadius: 2, bgcolor: s.color }} />
              <Typography variant="caption" color="text.secondary">{s.label}</Typography>
            </Stack>
          ))}
        </Stack>
      )}

      <Box sx={{ flexGrow: 1, height }}>{children}</Box>
    </Paper>
  );
}