import React from 'react';
import PropTypes from 'prop-types';

import AppBar from 'material-ui/AppBar'
import LinearProgress from 'material-ui/LinearProgress';
import MuiThemeProvider from 'material-ui/styles/MuiThemeProvider';

const Loading = ({theme}) => {
  const style = {
    padding: 10,
    display: "flex",
    flexWrap: "wrap",
  };
  return (
    <div>
      <MuiThemeProvider muiTheme={theme.headerMuiTheme}>
        <AppBar
          title="Human Telemetry"
          showMenuIconButton={false}
        />
      </MuiThemeProvider>
      <div style={style}>
        <LinearProgress mode="indeterminate" />
      </div>
    </div>
  );
}

Loading.propTypes = {
  theme: PropTypes.object
}

export default Loading;