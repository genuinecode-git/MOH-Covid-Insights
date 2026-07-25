import {
  Box, Divider, List, ListItem, ListItemButton, ListItemIcon,
  ListItemText, Stack, Tooltip, Typography,
} from '@mui/material';
import DashboardIcon from '@mui/icons-material/GridViewRounded';
import PressureIcon from '@mui/icons-material/LocalHospitalOutlined';
import FavoriteIcon from '@mui/icons-material/FavoriteRounded';
import { NavLink } from 'react-router-dom';
import type { ReactNode } from 'react';

export const SIDEBAR_WIDTH = 260;

const NAV: Array<{ label: string; icon: ReactNode; to?: string }> = [
  { label: 'Dashboard', icon: <DashboardIcon fontSize="small" />, to: '/' },
  { label: 'Avg. Daily Hosp./ICU', icon: <PressureIcon fontSize="small" />, to: '/hospitalisation-cases' },
];

export default function Sidebar() {
  return (
    <Box
      component="nav"
      aria-label="Main navigation"
      sx={{
        width: SIDEBAR_WIDTH,
        flexShrink: 0,
        bgcolor: 'background.paper',
        borderRight: 1,
        borderColor: 'divider',
        display: 'flex',
        flexDirection: 'column',
        height: '100vh',
        position: 'sticky',
        top: 0,
        overflowY: 'auto',
      }}
    >
      <Stack direction="row" alignItems="center" spacing={1.5} sx={{ px: 2.5, py: 2.25 }}>
        <Box
          sx={{
            width: 36, height: 36, borderRadius: 2, bgcolor: 'primary.main',
            display: 'grid', placeItems: 'center', color: '#fff',
          }}
        >
          <FavoriteIcon fontSize="small" />
        </Box>
        <Typography variant="h6" fontWeight={700}>
          SG Health Trends
        </Typography>
      </Stack>
      <Divider />

      <List sx={{ px: 1.5, py: 1.5 }}>
        {NAV.map((item) => (
          <ListItem key={item.label} disablePadding>
            <Tooltip title={item.to ? '' : 'Not part of this assessment scope'} placement="right">
              <span style={{ width: '100%' }}>
                <ListItemButton
                  component={item.to ? NavLink : 'div'}
                  to={item.to}
                  end={item.to === '/'}
                  disabled={!item.to}
                  sx={{
                    '&.active': {
                      bgcolor: 'primary.light',
                      color: 'primary.main',
                      '& .MuiListItemIcon-root': { color: 'primary.main' },
                    },
                  }}
                >
                  <ListItemIcon>{item.icon}</ListItemIcon>
                  <ListItemText
                    primary={item.label}
                    primaryTypographyProps={{ fontSize: '0.875rem', fontWeight: item.to ? 600 : 500 }}
                  />
                </ListItemButton>
              </span>
            </Tooltip>
          </ListItem>
        ))}
      </List>
    </Box>
  );
}