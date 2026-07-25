import {
  Box, Chip, Link, Paper, Stack, Table, TableBody, TableCell,
  TableHead, TableRow, Typography,
} from '@mui/material';
import ArrowIcon from '@mui/icons-material/ArrowForwardRounded';
import type { DatasetCoverage } from '../../../types';

export default function DataCoverageTable({ rows }: { rows: DatasetCoverage[] }) {
  return (
    <Paper sx={{ p: 2.5, height: '100%', display: 'flex', flexDirection: 'column' }}>
      <Typography variant="h6" mb={1}>Data Coverage</Typography>
      <Table size="small" sx={{ '& td, & th': { borderColor: 'divider', py: 0.9 } }}>
        <TableHead>
          <TableRow>
            <TableCell sx={{ color: 'text.secondary', fontWeight: 600 }}>Dataset</TableCell>
            <TableCell sx={{ color: 'text.secondary', fontWeight: 600 }}>Coverage</TableCell>
            <TableCell />
          </TableRow>
        </TableHead>
        <TableBody>
          {rows.map((r) => (
            <TableRow key={r.id} hover>
              <TableCell>
                <Stack direction="row" spacing={1.25} alignItems="center">
                  <Box sx={{ width: 8, height: 8, borderRadius: '50%', bgcolor: r.color, flexShrink: 0 }} />
                  <Typography variant="body2">{r.name}</Typography>
                </Stack>
              </TableCell>
              <TableCell>
                <Typography variant="body2" color="text.secondary">{r.coverage}</Typography>
              </TableCell>
              <TableCell align="right">
                <Chip label={r.volume} size="small"
                  sx={{ bgcolor: '#F3F4F8', color: 'text.secondary', height: 22, fontSize: '0.7rem' }} />
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
      <Box flexGrow={1} />
      <Link href="#" underline="hover" variant="body2"
        sx={{ mt: 1.5, display: 'inline-flex', alignItems: 'center', gap: 0.5 }}>
        View all datasets status <ArrowIcon sx={{ fontSize: 15 }} />
      </Link>
    </Paper>
  );
}