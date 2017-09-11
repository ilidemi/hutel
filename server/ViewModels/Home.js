import React from 'react';
import PropTypes from 'prop-types';

import AppBar from 'material-ui/AppBar';
import Divider from 'material-ui/Divider';
import IconButton from 'material-ui/IconButton';
import IconMenu from 'material-ui/IconMenu';
import MenuItem from 'material-ui/MenuItem';
import MoreVertIcon from 'material-ui/svg-icons/navigation/more-vert';
import MuiThemeProvider from 'material-ui/styles/MuiThemeProvider';

import Charts from './Charts';
import PointHistory from './PointHistory';
import SelectTag from './SelectTag';

class Home extends React.Component {
  constructor(props) {
    super(props);
  }

  onEditRawTagsClick() {
    this.props.history.push('/edit/tags');
  }

  onEditRawPointsClick() {
    this.props.history.push('/edit/points');
  }
  
  onEditRawChartsClick() {
    this.props.history.push('/edit/charts');
  }

  render() {
    return (
      <div>
        <MuiThemeProvider muiTheme={this.props.theme.headerMuiTheme}>
          <AppBar
            title="Human Telemetry"
            showMenuIconButton={false}
            iconElementRight={
              <IconMenu
                iconButtonElement={
                  <IconButton><MoreVertIcon /></IconButton>
                }
                targetOrigin={{horizontal: 'right', vertical: 'top'}}
                anchorOrigin={{horizontal: 'right', vertical: 'top'}}
              >
                <MenuItem
                  primaryText="Edit raw tags"
                  onTouchTap={this.onEditRawTagsClick.bind(this)}
                />
                <MenuItem
                  primaryText="Edit raw points"
                  onTouchTap={this.onEditRawPointsClick.bind(this)}
                />
                <MenuItem
                  primaryText="Edit raw charts"
                  onTouchTap={this.onEditRawChartsClick.bind(this)}
                />
              </IconMenu>
            }
          />
        </MuiThemeProvider>
        <SelectTag
          tags={this.props.tags}
          history={this.props.history}
          theme={this.props.theme}
        />
        <Divider />
        <Charts
          charts={this.props.charts}
          chartsPoints={this.props.chartsPoints}
        />
        <Divider />
        <PointHistory
          points={this.props.points}
          theme={this.props.theme}
        />
      </div>
    );
  }
}

Home.propTypes = {
  tags: PropTypes.array,
  points: PropTypes.array,
  charts: PropTypes.array,
  chartsPoints: PropTypes.array,
  history: PropTypes.object,
  theme: PropTypes.object
};

export default Home;
