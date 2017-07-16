import React from 'react';
import PropTypes from 'prop-types';

import $ from 'jquery';
import moment from 'moment';

import AppBar from 'material-ui/AppBar'
import Divider from 'material-ui/Divider';
import IconButton from 'material-ui/IconButton';
import IconMenu from 'material-ui/IconMenu';
import MenuItem from 'material-ui/MenuItem';
import MoreVertIcon from 'material-ui/svg-icons/navigation/more-vert';
import MuiThemeProvider from 'material-ui/styles/MuiThemeProvider';

import * as Constants from './Constants'
import PointHistory from './PointHistory';
import SelectTag from './SelectTag';
import PointInput from './PointInput';

class Home extends React.Component {
  constructor(){
    super();
    this.state = {
      points: [],
      tags: [],
      selectedTagId: null,
      selectTagLoading: true,
      pointHistoryLoading: true,
      pointInputLoading: false
    }
  }

  componentDidMount() {
    this.updateHistory();
    this.updateTags();
  }

  updateHistory() {
    this.setState({pointHistoryLoading: true}, function(){
      console.log(this.state);
    });
    $.ajax({
      url: "/api/points",
      data: {
        startDate: moment().subtract(7, 'days').format(Constants.dateFormat)
      },
      dataType:'json',
      cache: false,
      success: function(data){
        this.setState({points: data, pointHistoryLoading: false}, function(){
          console.log(this.state);
        });
      }.bind(this),
      error: function(xhr, status, err){
        console.log(err);
        this.setState({pointHistoryLoading: false}, function(){
          console.log(this.state);
        });
      }.bind(this)
    });
  }

  updateTags() {
    this.setState({selectTagLoading: true}, function() {
      console.log(this.state);
    });
    $.ajax({
      url: "/api/tags",
      dataType: "json",
      cache: false,
      success: function(data) {
        this.setState({tags: data, selectTagLoading: false}, function() {
          console.log(this.state);
        });
      }.bind(this),
      error: function(xhr, status, err) {
        console.log(err);
        this.setState({selectTagLoading: false}, function() {
          console.log(this.state);
        });
      }.bind(this)
    });
  }

  selectTag(tagId) {
    this.setState({selectedTagId: tagId}, function() {
      console.log(this.state);
    });
  }

  resetTag() {
    this.setState({selectedTagId: null}, function() {
      console.log(this.state);
    });
  }

  submitPoint(point) {
    console.log("Submitting point", point);
    this.setState({pointInputLoading: true}, function() {
      console.log(this.state);
    });
    $.ajax({
      url: "/api/points",
      dataType: "json",
      contentType:"application/json; charset=utf-8",
      method: "POST",
      data: JSON.stringify(point),
      success: function(data) {
        console.log(data);
        this.resetTag();
        this.setState({pointInputLoading: false}, function() {
          console.log(this.state);
        });
        this.updateHistory();
      }.bind(this),
      error: function(xhr, status, err) {
        console.log(err);
        this.resetTag();
        this.setState({pointInputLoading: false}, function() {
          console.log(this.state);
        });
      }.bind(this)
    });
  }

  render() {
    const theme = this.props.theme;
    var selectTagStyle = {
      background: theme.topBackground
    }
    var pointHistoryStyle = {
      background: theme.historyBackground
    }
    var selectTag = <SelectTag
      loading={this.state.selectTagLoading}
      tags={this.state.tags}
      selectTag={this.selectTag.bind(this)}
      theme={theme}
    />;
    var pointInput = <PointInput
      loading={this.state.pointInputLoading}
      tag={this.state.tags.find(tag => tag.id === this.state.selectedTagId)}
      resetTag={this.resetTag.bind(this)}
      submitPoint={this.submitPoint.bind(this)}
      theme={theme}
    />;
    var props = this.props;
    function navigateHome() {
      props.history.push('/');
    }
    return (
      <div>
        <MuiThemeProvider muiTheme={theme.headerMuiTheme}>
          <AppBar
            title="Human Telemetry"
            onLeftIconButtonTouchTap={navigateHome} // TODO: doesn't work as intended
            iconElementRight={
              <IconMenu
                iconButtonElement={
                  <IconButton><MoreVertIcon /></IconButton>
                }
                targetOrigin={{horizontal: 'right', vertical: 'top'}}
                anchorOrigin={{horizontal: 'right', vertical: 'top'}}
              >
                <MenuItem primaryText="Edit raw tags" />
                <MenuItem primaryText="Edit raw points" />
              </IconMenu>
            }
          />
        </MuiThemeProvider>
        <div style={selectTagStyle}>
          {this.state.selectedTagId === null ? selectTag : pointInput}
        </div>
        <Divider />
        <div style={pointHistoryStyle}>
          <PointHistory
            loading={this.state.pointHistoryLoading}
            points={this.state.points}
            theme={theme}
          />
        </div>
      </div>
    );
  }
}

Home.propTypes = {
  history: PropTypes.object,
  theme: PropTypes.object
}

export default Home;
