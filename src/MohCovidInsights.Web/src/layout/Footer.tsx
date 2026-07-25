import { Box, Link, Stack, Typography } from '@mui/material';

export default function Footer({ lastUpdated }: { lastUpdated: string }) {
  return (
    <Box component="footer" sx={{ px: 3, py: 2, borderTop: 1, borderColor: 'divider', mt: 3 }}>
      <Stack direction={{ xs: 'column', md: 'row' }} spacing={1.5}
        justifyContent="space-between" alignItems={{ md: 'center' }}>
        <Typography variant="caption" color="text.secondary">
          Source: Ministry of Health (MOH) via data.gov.sg
        </Typography>
        <Typography variant="caption" color="text.secondary">
          Last updated: {lastUpdated}
        </Typography>
        <Stack direction="row" spacing={2}>
          <Link href="#" variant="caption" underline="hover" color="text.secondary">Privacy</Link>
          <Link href="#" variant="caption" underline="hover" color="text.secondary">Terms</Link>
        </Stack>
      </Stack>
    </Box>
  );
}