import React from 'react';
import PropTypes from 'prop-types';

import AppBar from 'material-ui/AppBar';
import Divider from 'material-ui/Divider';
import FontIcon from 'material-ui/FontIcon';
import IconButton from 'material-ui/IconButton';
import IconMenu from 'material-ui/IconMenu';
import MenuItem from 'material-ui/MenuItem';
import MoreVertIcon from 'material-ui/svg-icons/navigation/more-vert';
import MuiThemeProvider from 'material-ui/styles/MuiThemeProvider';

import Charts from './Charts';
import PointsHistory from './PointsHistory';
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
                targetOrigin={{ horizontal: 'right', vertical: 'top' }}
                anchorOrigin={{ horizontal: 'right', vertical: 'top' }}
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
                <MenuItem
                  primaryText="Reload from storage"
                  onTouchTap={this.props.reloadFromStorageCallback}
                />
                <MenuItem
                  primaryText="Hide sensitive"
                  leftIcon={
                    <FontIcon
                      className="material-icons"
                      style={{ fontSize: 20 }}
                    >
                      {this.props.sensitiveHidden ? "check_box" : "check_box_outline_blank"}
                    </FontIcon>
                  }
                  onTouchTap={this.props.toggleSensitiveHiddenCallback}
                />
              </IconMenu>
            }
          />
        </MuiThemeProvider>
        <SelectTag
          tags={this.props.tags}
          sensitiveHidden={this.props.sensitiveHidden}
          history={this.props.history}
          theme={this.props.theme}
        />
        <Divider />
        <Charts
          charts={this.props.charts}
          chartsPoints={this.props.chartsPoints}
        />
        <Divider />
        <PointsHistory
          points={this.props.points}
          tagsById={this.props.tagsById}
          sensitiveHidden={this.props.sensitiveHidden}
          theme={this.props.theme}
          notifyPointsChanged={this.props.notifyPointsChanged}
        />
      </div >
    );
  }
}

Home.propTypes = {
  tags: PropTypes.array,
  tagsById: PropTypes.object,
  points: PropTypes.array,
  charts: PropTypes.array,
  chartsPoints: PropTypes.array,
  sensitiveHidden: PropTypes.bool,
  reloadFromStorageCallback: PropTypes.func,
  toggleSensitiveHiddenCallback: PropTypes.func,
  notifyPointsChanged: PropTypes.func,
  history: PropTypes.object,
  theme: PropTypes.object
};

export default Home;
