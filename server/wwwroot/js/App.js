import React from 'react';
import moment from 'moment';
import $ from 'jquery';

import injectTapEventPlugin from 'react-tap-event-plugin';
import * as Colors from 'material-ui/styles/colors';
import MuiThemeProvider from 'material-ui/styles/MuiThemeProvider';
import getMuiTheme from 'material-ui/styles/getMuiTheme';
import Divider from 'material-ui/Divider'
import Paper from 'material-ui/Paper';

import * as Constants from './Constants'
import PointHistory from './PointHistory';
import SelectTag from './SelectTag';
import PointInput from './PointInput';

injectTapEventPlugin();

class App extends React.Component {
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
        startDate: moment().subtract(30, 'days').format(Constants.dateFormat)
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
    const theme = {
      headerBackground: Colors.deepPurple900,
      headerText: Colors.fullWhite,
      topBackground: Colors.fullWhite,
      historyBackground: Colors.grey100,
      historyDateText: Colors.deepPurple900
    }
    const muiTheme = getMuiTheme({
      palette: {
        primary1Color: Colors.amber900,
        accent1Color: Colors.yellow500
      }
    });
    const columnStyle = {
      maxWidth: 800,
      margin: "auto",
      display: "flex",
      flexDirection: "column",
      flexHeight: "100%",
      flexMinHeight: "100%"
    };
    const headerStyle = {
      paddingLeft: 24,
      paddingRight: 24,
      color: theme.headerText,
      background: theme.headerBackground
    }
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
    return (
      <MuiThemeProvider muiTheme={muiTheme}>
        <Paper
          style={columnStyle}
          zDepth={5}
        >
          <div style={headerStyle}>
            <h1 className="mdc-typography--display2">
              Human Telemetry
            </h1>
          </div>
          <Divider />
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
        </Paper>
      </MuiThemeProvider>
    );
  }
}

export default App;
