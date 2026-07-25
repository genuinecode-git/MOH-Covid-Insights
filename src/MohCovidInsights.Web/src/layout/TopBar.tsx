import { Box, Chip, IconButton, Stack, Typography } from '@mui/material';
import MenuIcon from '@mui/icons-material/MenuRounded';
import CalendarIcon from '@mui/icons-material/CalendarTodayRounded';
import ChevronIcon from '@mui/icons-material/KeyboardArrowDownRounded';

export default function TopBar({ rangeLabel }: { rangeLabel: string }) {
  return (
    <Box
      component="header"
      sx={{
        position: 'sticky', top: 0, zIndex: 10,
        bgcolor: 'background.paper',
        borderBottom: 1, borderColor: 'divider',
        px: 3, py: 1.5,
      }}
    >
      <Stack direction="row" alignItems="center" spacing={2}>
        <IconButton size="small" aria-label="Toggle navigation">
          <MenuIcon />
        </IconButton>
        <Typography variant="h6" sx={{ fontSize: '1.125rem', fontWeight: 600 }}>
          Dashboard
        </Typography>

        <Box flexGrow={1} />

        <Chip
          icon={<CalendarIcon sx={{ fontSize: 15 }} />}
          deleteIcon={<ChevronIcon />}
          onDelete={() => {}}
          label={rangeLabel}
          variant="outlined"
          sx={{ borderRadius: 2, height: 36, px: 0.5 }}
        />
      </Stack>
    </Box>
  );
}